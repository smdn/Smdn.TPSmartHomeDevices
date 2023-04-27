// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Smdn.TPSmartHomeDevices.Tapo.Protocol;

namespace Smdn.TPSmartHomeDevices.Tapo;

internal sealed class TapoDeviceDefaultExceptionHandler : TapoDeviceExceptionHandler {
  public override TapoDeviceExceptionHandling DetermineHandling(
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

              return TapoDeviceExceptionHandling.InvalidateEndPointAndRetry;
            }
            else {
              logger?.LogError(
                "Endpoint unreachable (SocketError: {SocketErrorCodeNumeric} {SocketErrorCode})",
                (int)socketErrorCode,
                socketErrorCode
              );

              return TapoDeviceExceptionHandling.InvalidateEndPointAndThrow;
            }
          }

          // The HTTP client may have been invalid due to an exception at the transport layer.
          logger?.LogError(
            innerSocketException,
            "Unexpected socket exception (SocketError: {SocketErrorCodeNumeric} {SocketErrorCode})",
            (int)socketErrorCode,
            socketErrorCode
          );

          return TapoDeviceExceptionHandling.Throw;
        }

        return TapoDeviceExceptionHandling.Throw;

      case SecurePassThroughInvalidPaddingException securePassThroughInvalidPaddingException:
        if (attempt == 0 /* retry just once */) {
          logger?.LogWarning("{Message}", securePassThroughInvalidPaddingException.Message);

          // The session might have been in invalid state(?)
          return TapoDeviceExceptionHandling.RetryAfterReconnect;
        }

        return TapoDeviceExceptionHandling.ThrowAsTapoProtocolException;

      case TapoErrorResponseException errorResponseException:
        if (attempt == 0 /* retry just once */) {
          switch (errorResponseException.RawErrorCode) {
            case TapoErrorCodes.DeviceBusy:
              logger?.LogWarning("{Message}", errorResponseException.Message);

              // The session might have been in invalid state(?)
              return TapoDeviceExceptionHandling.CreateRetry(
                retryAfter: TimeSpan.FromSeconds(2.0),
                shouldReconnect: true
              );

            case TapoErrorCodes.RequestParameterError:
              logger?.LogWarning("{Message}", errorResponseException.Message);
              return TapoDeviceExceptionHandling.Throw;

            default:
              logger?.LogWarning(
                "Unexpected error (RawErrorCode: {RawErrorCode})",
                errorResponseException.RawErrorCode
              );

              // The session might have been in invalid state(?)
              return TapoDeviceExceptionHandling.RetryAfterReconnect;
          }
        }

        return TapoDeviceExceptionHandling.Throw;

      case TaskCanceledException taskCanceledException:
        if (taskCanceledException.InnerException is TimeoutException) {
          if (attempt < 2 /* retry up to 3 times */) {
            logger?.LogWarning("Request timed out; {ExceptionMessage}", taskCanceledException.Message);
            return TapoDeviceExceptionHandling.Retry;
          }

          logger?.LogError(taskCanceledException, "Request timed out");

          return TapoDeviceExceptionHandling.ThrowAsTapoProtocolException;
        }

        return TapoDeviceExceptionHandling.Throw;

      default:
        logger?.LogError(
          exception,
          "Unhandled exception ({ExceptionTypeFullName})",
          exception.GetType().FullName
        );

        return TapoDeviceExceptionHandling.Throw;
    }
  }
}
