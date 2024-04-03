using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;

namespace Mediator.Tests;

public class NotificationHandlersCollectionTests
{
    static IEnumerable<INotificationHandler<FakeNotification>> GetHandlers(int count, bool isArray)
    {
        var handlers = new List<INotificationHandler<FakeNotification>>(count);
        for (var i = 0; i < count; i++)
            handlers.Add(new FakeNotificationHandler());

        return isArray ? handlers.ToArray() : handlers;
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Test_IsSingleHandler_ReturnsTrue_WhenSingleHandler_And_Array(bool isArray)
    {
        var input = GetHandlers(1, isArray);
        var handlers = new NotificationHandlers<FakeNotification>(input, isArray);

        var isSingle = handlers.IsSingleHandler(out var singleHandler);

        if (isArray)
        {
            Assert.True(isSingle);
            input.Single().Should().BeSameAs(singleHandler);
        }
        else
        {
            Assert.False(isSingle);
            Assert.Null(singleHandler);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Test_IsSingleHandler_ReturnsFalse_WhenMultipleHandlers(bool isArray)
    {
        var input = GetHandlers(2, isArray);
        var handlers = new NotificationHandlers<FakeNotification>(input, isArray);

        var isSingle = handlers.IsSingleHandler(out var singleHandler);

        Assert.False(isSingle);
        Assert.Null(singleHandler);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Test_IsArray(bool isArray)
    {
        var input = GetHandlers(2, isArray);
        var handlers = new NotificationHandlers<FakeNotification>(input, isArray);

        var result = handlers.IsArray(out var handlersArray);

        if (isArray)
        {
            Assert.True(result);
            Assert.True(handlersArray is INotificationHandler<FakeNotification>[]);
            Assert.NotNull(handlersArray);
            handlersArray.Length.Should().Be(2);
            handlersArray.Should().BeSameAs(input);
        }
        else
        {
            Assert.False(result);
            Assert.Null(handlersArray);
        }
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(2, false)]
    [InlineData(3, true)]
    [InlineData(3, false)]
    public void Test_Enumerator(int count, bool isArray)
    {
        var input = GetHandlers(count, isArray);
        var handlers = new NotificationHandlers<FakeNotification>(input, isArray);

        using var enumerator = handlers.GetEnumerator();

        for (int j = 0; j < 3; j++)
        {
            for (var i = 0; i < count; i++)
            {
                enumerator.MoveNext().Should().BeTrue();
                enumerator.Current.Should().BeSameAs(input.ElementAt(i));
            }
            enumerator.MoveNext().Should().BeFalse();
            enumerator.Reset();
        }
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(1, false)]
    [InlineData(3, true)]
    [InlineData(3, false)]
    public void Test_Enumerator_Throws_WhenEnumerationHasNotStarted(int count, bool isArray)
    {
        var input = GetHandlers(count, isArray);
        var handlers = new NotificationHandlers<FakeNotification>(input, isArray);

        using var enumerator = handlers.GetEnumerator();
        Action action = () => _ = ((IEnumerator)enumerator).Current;
        action.Should().Throw<InvalidOperationException>();
        action = () => _ = enumerator.Current;
        action.Should().NotThrow();

        var list = new List<int>([1, 2, 3]);
        using var listEnumerator = list.GetEnumerator();
        action = () => _ = ((IEnumerator)listEnumerator).Current;
        action.Should().Throw<InvalidOperationException>();
        action = () => _ = listEnumerator.Current;
        action.Should().NotThrow();

        // ^ don't know why, just keeping consistent behavior...
    }

    [Theory]
    [InlineData(3, true)]
    [InlineData(3, false)]
    public void Test_Linq_ToArray(int count, bool isArray)
    {
        var input = GetHandlers(count, isArray);
        var handlers = new NotificationHandlers<FakeNotification>(input, isArray);

        var result = handlers.ToArray();

        result.Should().BeEquivalentTo(input);
    }

    public class FakeNotification : INotification { }

    public class FakeNotificationHandler : INotificationHandler<FakeNotification>
    {
        public ValueTask Handle(FakeNotification notification, CancellationToken cancellationToken)
        {
            return default;
        }
    }
}
