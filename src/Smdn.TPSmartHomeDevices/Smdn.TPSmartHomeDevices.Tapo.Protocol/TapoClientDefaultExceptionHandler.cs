using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

internal sealed class TapoClientDefaultExceptionHandler : TapoClientExceptionHandler {
  public override TapoClientExceptionHandling DetermineHandling(
    TapoDevice device,
    Exception exception,
    int attempt,
    ILogger? logger
  )
  {
    switch (exception) {
      case HttpRequestException httpRequestException:
        if (httpRequestException.InnerException is SocketException innerSocketException) {
          var socketErrorCode = innerSocketException.SocketErrorCode;

          if (
            socketErrorCode is
              SocketError.ConnectionRefused or // ECONNREFUSED
              SocketError.HostUnreachable or // EHOSTUNREACH
              SocketError.NetworkUnreachable // ENETUNREACH
          ) {
            if (attempt == 0 /* retry just once */) {
              // The end point may have changed.
              logger?.LogInformation(
                "Endpoint may have changed (SocketError: {SocketErrorCodeNumeric} {SocketErrorCode})",
                (int)socketErrorCode,
                socketErrorCode
              );

              return TapoClientExceptionHandling.InvalidateEndPointAndRetry;
            }
            else {
              logger?.LogError(
                "Endpoint unreachable (SocketError: {SocketErrorCodeNumeric} {SocketErrorCode})",
                (int)socketErrorCode,
                socketErrorCode
              );

              return TapoClientExceptionHandling.InvalidateEndPointAndThrow;
            }
          }

          // The HTTP client may have been invalid due to an exception at the transport layer.
          logger?.LogError(
            innerSocketException,
            "Unexpected socket exception (SocketError: {SocketErrorCodeNumeric} {SocketErrorCode})",
            (int)socketErrorCode,
            socketErrorCode
          );

          return TapoClientExceptionHandling.Throw;
        }

        return TapoClientExceptionHandling.Throw;

      case SecurePassThroughInvalidPaddingException securePassThroughInvalidPaddingException:
        if (attempt == 0 /* retry just once */) {
          logger?.LogWarning("{Message}", securePassThroughInvalidPaddingException.Message);

          // The session might have been in invalid state(?)
          return TapoClientExceptionHandling.RetryAfterReconnect;
        }

        return TapoClientExceptionHandling.ThrowAsTapoProtocolException;

      case TapoErrorResponseException errorResponseException:
        if (attempt == 0 /* retry just once */) {
          switch (errorResponseException.RawErrorCode) {
            case TapoErrorCodes.DeviceBusy:
              logger?.LogWarning("{Message}", errorResponseException.Message);

              // The session might have been in invalid state(?)
              return TapoClientExceptionHandling.CreateRetry(
                retryAfter: TimeSpan.FromSeconds(2.0),
                shouldReconnect: true
              );

            case TapoErrorCodes.RequestParameterError:
              logger?.LogWarning("{Message}", errorResponseException.Message);
              return TapoClientExceptionHandling.Throw;

            default:
              logger?.LogWarning(
                "Unexpected error (RawErrorCode: {RawErrorCode})",
                errorResponseException.RawErrorCode
              );

              // The session might have been in invalid state(?)
              return TapoClientExceptionHandling.RetryAfterReconnect;
          }
        }

        return TapoClientExceptionHandling.Throw;

      case TaskCanceledException taskCanceledException:
        if (taskCanceledException.InnerException is TimeoutException) {
          if (attempt < 2 /* retry up to 3 times */) {
            logger?.LogWarning("Request timed out; {ExceptionMessage}", taskCanceledException.Message);
            return TapoClientExceptionHandling.Retry;
          }

          logger?.LogError(taskCanceledException, "Request timed out");

          return TapoClientExceptionHandling.ThrowAsTapoProtocolException;
        }

        return TapoClientExceptionHandling.Throw;

      default:
        logger?.LogError(
          exception,
          "Unhandled exception ({ExceptionTypeFullName})",
          exception.GetType().FullName
        );

        return TapoClientExceptionHandling.Throw;
    }
  }
}
