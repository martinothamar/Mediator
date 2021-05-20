using System;
using System.Threading.Tasks;

namespace Mediator
{
    public interface IBaseResult { }

    public interface IResult<T> : IBaseResult
    {
    }

    public readonly struct Result<T> : IResult<T, Exception>
    {
        private readonly T? _result;
        private readonly Exception? _exception;

        public Result(T result)
        {
            _result = result;
            _exception = null;
        }
        public Result(Exception exception)
        {
            _result = default;
            _exception = exception;
        }

        public TResult Match<TResult>(Func<T, TResult> handleValue, Func<Exception, TResult> handleError1)
        {
            if (_result is not null)
                return handleValue(_result);
            else
                return handleError1(_exception!);
        }
    }

    public interface IResult<T, TError> : IResult<T>
    {
        /// <summary>
        /// Handle the contents of the Result, map it into a TResult value.
        /// </summary>
        /// <typeparam name="TResult">Return value type</typeparam>
        /// <param name="handleValue">Function for handling the T value</param>
        /// <param name="handleError1">Function for handling the error</param>
        /// <returns>The TResult value</returns>
        TResult Match<TResult>(Func<T, TResult> handleValue, Func<TError, TResult> handleError1);
    }

    public interface IResultMiddleware<TMessage, TResponse> : IPipelineBehavior<TMessage, Result<TResponse>>
        where TMessage : IMessage
    {
    }

    public interface IResult<T, TError1, TError2> : IBaseResult
    {
        public TResult Match<TResult>(
            Func<T, TResult> handleValue,
            Func<TError1, TResult> handleError1,
            Func<TError2, TResult> handleError2
        );
    }
}
