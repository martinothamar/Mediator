using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Sources;

namespace Mediator;

public class ValueTaskWrapper<T> : IValueTaskSource<T>
{
    private readonly ValueTask _original;
    private readonly T _result;
    private readonly ManualResetValueTaskSourceCore<T> _core;

    public ValueTaskWrapper(ValueTask original, T result)
    {
        _original = original;
        _result = result;
        _core = new ManualResetValueTaskSourceCore<T>();
        _core.RunContinuationsAsynchronously = true;

        MonitorOriginal();
    }

    private async void MonitorOriginal()
    {
        try
        {
            await _original;
            _core.SetResult(_result);
        }
        catch (Exception ex)
        {
            _core.SetException(ex);
        }
    }

    public ValueTask<T> AsValueTask()
    {
        return new ValueTask<T>(this, _core.Version);
    }

    public T GetResult(short token) => _core.GetResult(token);

    public ValueTaskSourceStatus GetStatus(short token) => _core.GetStatus(token);

    public void OnCompleted(
        Action<object?> continuation,
        object? state,
        short token,
        ValueTaskSourceOnCompletedFlags flags
    ) => _core.OnCompleted(continuation, state, token, flags);
}
