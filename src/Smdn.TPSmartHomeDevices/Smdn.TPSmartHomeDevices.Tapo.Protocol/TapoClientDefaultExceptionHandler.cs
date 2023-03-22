using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

internal class TapoClientDefaultExceptionHandler : TapoClientExceptionHandler {
  public override TapoClientExceptionHandling DetermineHandling(Exception exception, int attempt, ILogger? logger)
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
              logger?.LogInformation($"Endpoint may have changed ({nameof(SocketError)}: {(int)socketErrorCode} {socketErrorCode})");

              return TapoClientExceptionHandling.RetryAfterResolveEndPoint;
            }
            else {
              logger?.LogError($"Endpoint unreachable ({nameof(SocketError)}: {(int)socketErrorCode} {socketErrorCode})");

              return TapoClientExceptionHandling.ThrowAndInvalidateEndPoint;
            }
          }

          // The HTTP client may have been invalid due to an exception at the transport layer.
          logger?.LogError(innerSocketException, $"Unexpected socket exception ({nameof(SocketError)}: {(int)socketErrorCode} {socketErrorCode})");

          return TapoClientExceptionHandling.Throw;
        }

        return TapoClientExceptionHandling.Throw;

      case SecurePassThroughInvalidPaddingException securePassThroughInvalidPaddingException:
        // The session might have been in invalid state(?)
        if (attempt == 0 /* retry just once */) {
          logger?.LogWarning(securePassThroughInvalidPaddingException, "Invalid padding in secure pass through");
          return TapoClientExceptionHandling.RetryAfterReestablishSession;
        }

        return TapoClientExceptionHandling.ThrowWrapTapoProtocolException;

      case TapoErrorResponseException errorResponseException:
        if (attempt == 0 /* retry just once */) {
          switch (errorResponseException.ErrorCode) {
            // The session might have been in invalid state(?)
            case TapoErrorCodes.DeviceBusy:
              logger?.LogWarning(errorResponseException, "Error code -1301");
              return TapoClientExceptionHandling.RetryAfterReconnect;

            case TapoErrorCodes.RequestParameterError:
              logger?.LogWarning(errorResponseException, "Error code -1008");
              return TapoClientExceptionHandling.Throw;

            default:
              logger?.LogWarning(errorResponseException, $"Unexpected error ({nameof(errorResponseException.ErrorCode)}: {(int)errorResponseException.ErrorCode})");
              return TapoClientExceptionHandling.RetryAfterReestablishSession;
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

          return TapoClientExceptionHandling.ThrowWrapTapoProtocolException;
        }

        return TapoClientExceptionHandling.Throw;

      default:
        logger?.LogError(exception, $"Unhandled exception ({exception.GetType().FullName})");
        return TapoClientExceptionHandling.Throw;
    }
  }
}
