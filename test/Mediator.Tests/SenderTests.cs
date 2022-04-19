using Mediator.Tests.TestTypes;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests;

internal struct FastLazyValue<T>
    where T : struct
{
    private const long UNINIT = 0;
    private const long INITING = 1;
    private const long INITD = 2;

    private Func<T> _generator;
    private long _state;
    private T _value;

    unsafe public ref readonly T Value
    {
        get
        {
            if (_state == INITD)
                return ref Unsafe.AsRef<T>(Unsafe.AsPointer(ref _value));

            var prevState = Interlocked.CompareExchange(ref _state, INITING, UNINIT);
            switch (prevState)
            {
                case INITD:
                    // Someone has already completed init
                    return ref Unsafe.AsRef<T>(Unsafe.AsPointer(ref _value));
                case INITING:
                    // Wait for someone else to complete
                    var spinWait = default(SpinWait);
                    while (Interlocked.Read(ref _state) < INITD)
                        spinWait.SpinOnce();
                    return ref Unsafe.AsRef<T>(Unsafe.AsPointer(ref _value));
                case UNINIT:
                    _value = _generator();
                    Interlocked.Exchange(ref _state, INITD);
                    return ref Unsafe.AsRef<T>(Unsafe.AsPointer(ref _value));
            }

            return ref Unsafe.AsRef<T>(Unsafe.AsPointer(ref _value));
        }
    }

    public FastLazyValue(Func<T> generator)
    {
        _generator = generator;
        _state = UNINIT;
        _value = default;
    }
}

internal sealed class Holder
{
    private readonly FastLazyValue<DICache> _cache;

    public Holder(IServiceProvider sp)
    {
        _cache = new FastLazyValue<DICache>(() => new DICache(sp));
    }

    public IMediator GetMediator()
    {
        ref readonly var cache = ref _cache.Value;
        return cache.Mediator;
    }

    private readonly struct DICache
    {
        public readonly IMediator Mediator;

        public DICache(IServiceProvider sp)
        {
            Mediator = sp.GetRequiredService<IMediator>();
        }
    } 
}

public sealed class SenderTests
{
    [Fact]
    public void Test_Lazy()
    {
        var services = new ServiceCollection();

        services.AddMediator();

        var sp = services.BuildServiceProvider(validateScopes: true);

        var holder = new Holder(sp);
        var mediator = holder.GetMediator();
        Assert.NotNull(mediator);
    }

    [Fact]
    public async Task Test_Request_Handler()
    {
        var (_, mediator) = Fixture.GetMediator();
        ISender sender = mediator;

        var id = Guid.NewGuid();

        var response = await sender.Send(new SomeRequest(id));
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
    }

    [Fact]
    public async Task Test_RequestWithoutResponse_Handler()
    {
        var (sp, mediator) = Fixture.GetMediator();
        ISender sender = mediator;

        var id = Guid.NewGuid();

        var handler = sp.GetRequiredService<SomeRequestWithoutResponseHandler>();
        Assert.NotNull(handler);
        await sender.Send(new SomeRequestWithoutResponse(id));
        Assert.Contains(id, SomeRequestWithoutResponseHandler.Ids);
    }

    [Fact]
    public async Task Test_Command_Handler()
    {
        var (_, mediator) = Fixture.GetMediator();
        ISender sender = mediator;

        var id = Guid.NewGuid();

        var response = await sender.Send(new SomeCommand(id));
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
    }

    [Fact]
    public async Task Test_CommandWithoutResponse_Handler()
    {
        var (sp, mediator) = Fixture.GetMediator();
        ISender sender = mediator;

        var id = Guid.NewGuid();

        var handler = sp.GetRequiredService<SomeCommandWithoutResponseHandler>();
        Assert.NotNull(handler);
        await sender.Send(new SomeCommandWithoutResponse(id));
        Assert.Contains(id, SomeCommandWithoutResponseHandler.Ids);
    }

    [Fact]
    public async Task Test_Query_Handler()
    {
        var (_, mediator) = Fixture.GetMediator();
        ISender sender = mediator;

        var id = Guid.NewGuid();

        var response = await sender.Send(new SomeQuery(id));
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
    }

    [Fact]
    public async Task Test_Fails_On_Dynamic_Type()
    {
        var (_, mediator) = Fixture.GetMediator();
        ISender sender = mediator;

        var id = Guid.NewGuid();
        object obj = new { Id = id };

        var request = Unsafe.As<object, IRequest>(ref obj);
        await Assert.ThrowsAsync<MissingMessageHandlerException>(async () => await sender.Send(request));
    }
}
