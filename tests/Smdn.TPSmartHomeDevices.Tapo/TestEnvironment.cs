// SPDX-FileCopyrightText: 2023 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Runtime.InteropServices;

internal static class TestEnvironment {
  public static bool IsRunningOnGitHubActions => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"));
  public static bool IsRunningOnGitHubActionsMacOSRunner => IsRunningOnGitHubActions && RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
}
