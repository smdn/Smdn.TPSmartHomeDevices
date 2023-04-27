// SPDX-FileCopyrightText: 2022 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace System.Threading.Tasks;

internal static class ValueTaskShim {
#if !SYSTEM_THREADING_TASKS_VALUETASK_FROMCANCELED
  public static ValueTask FromCanceled(CancellationToken cancellationToken)
    => new(Task.FromCanceled(cancellationToken));

  public static ValueTask<TResult> FromCanceled<TResult>(CancellationToken cancellationToken)
    => new(Task.FromCanceled<TResult>(cancellationToken));
#endif

#if !SYSTEM_THREADING_TASKS_VALUETASK_FROMRESULT
  public static ValueTask<TResult> FromResult<TResult>(TResult result) => new(result);
#endif
}
