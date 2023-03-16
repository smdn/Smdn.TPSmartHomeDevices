using System;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Smdn.TPSmartHomeDevices.Kasa.Protocol;

internal class KasaClientDefaultExceptionHandler : KasaClientExceptionHandler {
  public override KasaClientExceptionHandling DetermineHandling(Exception exception, int attempt, ILogger? logger)
  {
    switch (exception) {
      case SocketException socketException:
        if (
          attempt == 0 && // retry just once
          socketException.SocketErrorCode is
            SocketError.ConnectionRefused or // ECONNREFUSED
            SocketError.HostUnreachable or // EHOSTUNREACH
            SocketError.NetworkUnreachable // ENETUNREACH
        ) {
          // The end point may have changed.
          logger?.LogInformation($"Endpoint may have changed ({nameof(socketException.SocketErrorCode)}: {(int)socketException.SocketErrorCode})");

          return KasaClientExceptionHandling.RetryAfterResolveEndPoint;
        }

        // The client may have been invalid due to an exception at the transport layer.
        logger?.LogError(socketException, $"Unexpected socket exception ({nameof(socketException.SocketErrorCode)}: {(int)socketException.SocketErrorCode})");

        return KasaClientExceptionHandling.Throw;

      case KasaDisconnectedException disconnectedException:
        if (attempt == 0) { // retry just once
          // The peer device disconnected the connection, or may have dropped the connection.
          if (disconnectedException.InnerException is SocketException innerSocketException)
            logger?.LogDebug($"Disconnected ({nameof(innerSocketException.SocketErrorCode)}: {(int)innerSocketException.SocketErrorCode})");
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
          return KasaClientExceptionHandling.RetryAfterReconnect;
        }

        return KasaClientExceptionHandling.Throw;

      default:
        logger?.LogError(exception, $"Unhandled exception ({exception.GetType().FullName})");
        return KasaClientExceptionHandling.Throw;
    }
  }
}
