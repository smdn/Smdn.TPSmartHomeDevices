// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Smdn.TPSmartHomeDevices.Tapo;

var services = new ServiceCollection();

services.AddTapoCredential(
  email: "user@mail.test",
  password: "password"
);

// Adds CustomTapoExceptionHandler as a class for customized exception handling.
// (By default, the built-in exception handler class is used.)
services.AddTapoDeviceExceptionHandler(new CustomTapoExceptionHandler());

using var plug = new P105(IPAddress.Parse("192.0.2.1"), services.BuildServiceProvider());

await plug.TurnOnAsync();

// This class defines the behavior when an exception occurs while operating Tapo device.
// A class for customizing exception handling must be derived from TapoDeviceExceptionHandler.
class CustomTapoExceptionHandler : TapoDeviceExceptionHandler {
  // This method is called when an exception occurs in TapoDevice and its derived classes.
  public override TapoDeviceExceptionHandling DetermineHandling(
    TapoDevice device, Exception exception, int attempt, ILogger? logger
  )
  {
    logger?.LogError(exception, "Custom exception handling");

    switch (exception) {
      case HttpRequestException:
        // If the type of occurred exception is HttpRequestException, throw it as it is.
        return TapoDeviceExceptionHandling.Throw;

      case TapoErrorResponseException errorResponseException:
        // If the type of occurred exception is TapoErrorResponseException,
        // determines what to do by the response error code.
        if (errorResponseException.RawErrorCode == 9999) {
          if (attempt == 0 || attempt == 1)
            // If the response error code is 9999 and it is the first attempt (attempt=0)
            // or the first retry (attempt=1), re-establish session after 1 second (shouldReestablishSession=true) and retry.
            return TapoDeviceExceptionHandling.CreateRetry(TimeSpan.FromSeconds(1), shouldReestablishSession: true);
          else
            // In the case of the second retry (attempt>=2), throw the exception as it is.
            return TapoDeviceExceptionHandling.Throw;
        }
        else {
          // In the case of an exception due to any other error code, throw it as it is.
          return TapoDeviceExceptionHandling.Throw;
        }

      default:
        // For other exceptions, let the default behavior.
        return Default.DetermineHandling(device, exception, attempt, logger);
    }
  }
}
