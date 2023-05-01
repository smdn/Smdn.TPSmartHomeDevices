// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Smdn.TPSmartHomeDevices.Kasa;

var services = new ServiceCollection();

// Adds CustomKasaExceptionHandler as a class for customized exception handling.
// (By default, the built-in exception handler class is used.)
services.AddKasaDeviceExceptionHandler(new CustomKasaExceptionHandler());

using var plug = new HS105(IPAddress.Parse("192.0.2.255"), services.BuildServiceProvider());

await plug.TurnOnAsync();

// This class defines the behavior when an exception occurs while operating Kasa device.
// A class for customizing exception handling must be derived from KasaDeviceExceptionHandler.
class CustomKasaExceptionHandler : KasaDeviceExceptionHandler {
  // This method is called when an exception occurs in KasaDevice and its derived classes.
  public override KasaDeviceExceptionHandling DetermineHandling(
    KasaDevice device, Exception exception, int attempt, ILogger? logger
  )
  {
    logger?.LogError(exception, "Custom exception handling");

    switch (exception) {
      case KasaErrorResponseException errorResponseException:
        // If the type of occurred exception is KasaErrorResponseException,
        // determines what to do by the response error code.
        if (errorResponseException.RawErrorCode == 9999) {
          if (attempt == 0 || attempt == 1)
            // If the response error code is 9999 and it is the first attempt (attempt=0)
            // or the first retry (attempt=1), reconnect after 1 second (shouldReconnect=true) and retry.
            return KasaDeviceExceptionHandling.CreateRetry(TimeSpan.FromSeconds(1), shouldReconnect: true);
          else
            // In the case of the second retry (attempt>=2), throw the exception as it is.
            return KasaDeviceExceptionHandling.Throw;
        }
        else {
          // In the case of an exception due to any other error code, throw it as it is.
          return KasaDeviceExceptionHandling.Throw;
        }

      default:
        // For other exceptions, let the default behavior.
        return Default.DetermineHandling(device, exception, attempt, logger);
    }
  }
}
