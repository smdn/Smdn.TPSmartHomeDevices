// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.TPSmartHomeDevices.Tapo.Protocol;

/// <summary>
/// Maintains authenticated Tapo session information, including access token and session ID.
/// </summary>
public abstract class TapoSession : IDisposable {
  private bool disposed;

  public string? SessionId { get; }
  public DateTime ExpiresOn { get; }
  public bool HasExpired => ExpiresOn <= DateTime.Now;
  public abstract string? Token { get; }

  private protected TapoSession(
    string? sessionId,
    DateTime expiresOn
  )
  {
    SessionId = sessionId;
    ExpiresOn = expiresOn;
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!disposing)
      return;

    disposed = true;
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  private protected void ThrowIfDisposed()
  {
    if (disposed)
      throw new ObjectDisposedException(GetType().FullName);
  }
}
