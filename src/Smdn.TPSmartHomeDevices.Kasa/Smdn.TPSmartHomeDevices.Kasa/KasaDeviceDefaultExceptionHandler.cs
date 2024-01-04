// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;

namespace Smdn.TPSmartHomeDevices.Kasa;

internal sealed class KasaDeviceDefaultExceptionHandler : KasaDeviceExceptionHandler {
  public override KasaDeviceExceptionHandling DetermineHandling(
    KasaDevice device,
    Exception exception,
    int attempt,
    ILogger? logger
  )
  {
    switch (exception) {
      case SocketException socketException: {
        var socketErrorCode = socketException.SocketErrorCode;

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

            return KasaDeviceExceptionHandling.InvalidateEndPointAndRetry;
          }
          else {
            logger?.LogError(
              "Endpoint unreachable (SocketError: {SocketErrorCodeNumeric} {SocketErrorCode})",
              (int)socketErrorCode,
              socketErrorCode
            );

            return KasaDeviceExceptionHandling.InvalidateEndPointAndThrow;
          }
        }

        // The client may have been invalid due to an exception at the transport layer.
        logger?.LogError(
          socketException,
          "Unexpected socket exception (SocketError: {SocketErrorCodeNumeric} {SocketErrorCode})",
          (int)socketErrorCode,
          socketErrorCode
        );

        return KasaDeviceExceptionHandling.Throw;
      }

      case KasaDisconnectedException disconnectedException:
        if (attempt == 0) { // retry just once
          // The peer device disconnected the connection, or may have dropped the connection.
          if (disconnectedException.InnerException is SocketException innerSocketException) {
            logger?.LogDebug(
              "Disconnected (SocketError: {SocketErrorCodeNumeric} {SocketErrorCode})",
              (int)innerSocketException.SocketErrorCode,
              innerSocketException.SocketErrorCode
            );
          }
          else {
            logger?.LogDebug(
              "Disconnected ({Message})",
              disconnectedException.Message
            );
          }

          return KasaDeviceExceptionHandling.RetryAfterReconnect;
        }

        return KasaDeviceExceptionHandling.Throw;

      case KasaIncompleteResponseException ex:
        // The peer has been in invalid state(?) and returnd incomplete response.
        const int MaxRetryIncompleteResponse = 3;
        var nextAttempt = attempt + 1;

        if (nextAttempt < MaxRetryIncompleteResponse) { // retry up to max attempts
          logger?.LogWarning("{Message}", ex.Message);
          return KasaDeviceExceptionHandling.CreateRetry(
            retryAfter: TimeSpan.FromSeconds(2.0),
            shouldReconnect: true
          );
        }

        return KasaDeviceExceptionHandling.Throw;

      default:
        logger?.LogError(
          exception,
          "Unhandled exception ({ExceptionTypeFullName})",
          exception.GetType().FullName
        );

        return KasaDeviceExceptionHandling.Throw;
    }
  }
}
