using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Sources;

namespace Mediator;

public class ValueTaskWrapper<T> : IValueTaskSource
{
    private readonly ValueTask<T> _inner;
    private Action<object>? _continuation;
    private object? _state;

    public ValueTaskWrapper(ValueTask<T> inner)
    {
        _inner = inner;
    }

    public ValueTask AsValueTask()
    {
        return new ValueTask(this, 0);
    }

    public void OnCompleted(
        Action<object>? continuation,
        object? state,
        short token,
        ValueTaskSourceOnCompletedFlags flags
    )
    {
        _continuation = continuation;
        _state = state;

        _ = CompleteAsync();
    }

    private async Task CompleteAsync()
    {
        await _inner;

        if (_state != null)
        {
            _continuation?.Invoke(_state);
        }
    }

    public void GetResult(short token) { }

    public ValueTaskSourceStatus GetStatus(short token)
    {
        return _inner.IsCompletedSuccessfully ? ValueTaskSourceStatus.Succeeded
            : _inner.IsFaulted ? ValueTaskSourceStatus.Faulted
            : _inner.IsCanceled ? ValueTaskSourceStatus.Canceled
            : ValueTaskSourceStatus.Pending;
    }
}
