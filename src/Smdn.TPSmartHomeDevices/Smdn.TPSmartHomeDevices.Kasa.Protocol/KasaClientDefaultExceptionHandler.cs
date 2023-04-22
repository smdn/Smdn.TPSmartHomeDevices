using System;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Smdn.TPSmartHomeDevices.Kasa.Protocol;

internal sealed class KasaClientDefaultExceptionHandler : KasaClientExceptionHandler {
  public override KasaClientExceptionHandling DetermineHandling(
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
            logger?.LogInformation($"Endpoint may have changed ({nameof(SocketError)}: {(int)socketErrorCode} {socketErrorCode})");

            return KasaClientExceptionHandling.InvalidateEndPointAndRetry;
          }
          else {
            logger?.LogError($"Endpoint unreachable ({nameof(SocketError)}: {(int)socketErrorCode} {socketErrorCode})");

            return KasaClientExceptionHandling.InvalidateEndPointAndThrow;
          }
        }

        // The client may have been invalid due to an exception at the transport layer.
        logger?.LogError(socketException, $"Unexpected socket exception ({nameof(SocketError)}: {(int)socketErrorCode} {socketErrorCode})");

        return KasaClientExceptionHandling.Throw;
      }

      case KasaDisconnectedException disconnectedException:
        if (attempt == 0) { // retry just once
          // The peer device disconnected the connection, or may have dropped the connection.
          if (disconnectedException.InnerException is SocketException innerSocketException)
            logger?.LogDebug($"Disconnected ({nameof(SocketError)}: {(int)innerSocketException.SocketErrorCode} {innerSocketException.SocketErrorCode})");
          else
            logger?.LogDebug($"Disconnected ({disconnectedException.Message})");

          return KasaClientExceptionHandling.RetryAfterReconnect;
        }

        return KasaClientExceptionHandling.Throw;

      case KasaIncompleteResponseException ex:
        // The peer has been in invalid state(?) and returnd incomplete response.
        const int maxRetryIncompleteResponse = 3;
        var nextAttempt = attempt + 1;

        if (nextAttempt < maxRetryIncompleteResponse) { // retry up to max attempts
          logger?.LogWarning(ex.Message);
          return KasaClientExceptionHandling.CreateRetry(
            retryAfter: TimeSpan.FromSeconds(2.0),
            shouldReconnect: true
          );
        }

        return KasaClientExceptionHandling.Throw;

      default:
        logger?.LogError(exception, $"Unhandled exception ({exception.GetType().FullName})");
        return KasaClientExceptionHandling.Throw;
    }
  }
}
