using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests;

public sealed class ResponseInheritanceTests
{
    public abstract class BaseResponse
    {
        public Guid Id { get; }

        public BaseResponse(Guid id) => Id = id;
    }

    public class CreateResponse : BaseResponse
    {
        public CreateResponse(Guid id)
            : base(id) { }
    }

    public class CreateCommandRequest : ICommand<CreateResponse>
    {
        public Guid Id { get; }

        public CreateCommandRequest(Guid id) => Id = id;
    }

    public class CreateRequestRequest : IRequest<CreateResponse>
    {
        public Guid Id { get; }

        public CreateRequestRequest(Guid id) => Id = id;
    }

    public class CreateQueryRequest : IQuery<CreateResponse>
    {
        public Guid Id { get; }

        public CreateQueryRequest(Guid id) => Id = id;
    }

    public class CreateStreamRequest : IStreamRequest<CreateResponse>
    {
        public Guid Id { get; }

        public CreateStreamRequest(Guid id) => Id = id;
    }

    public class CreateStreamQuery : IStreamQuery<CreateResponse>
    {
        public Guid Id { get; }

        public CreateStreamQuery(Guid id) => Id = id;
    }

    public class CreateStreamCommand : IStreamCommand<CreateResponse>
    {
        public Guid Id { get; }

        public CreateStreamCommand(Guid id) => Id = id;
    }

    public sealed class CreateHandler
        : ICommandHandler<CreateCommandRequest, CreateResponse>,
            IRequestHandler<CreateRequestRequest, CreateResponse>,
            IQueryHandler<CreateQueryRequest, CreateResponse>,
            IStreamRequestHandler<CreateStreamRequest, CreateResponse>,
            IStreamQueryHandler<CreateStreamQuery, CreateResponse>,
            IStreamCommandHandler<CreateStreamCommand, CreateResponse>
    {
        internal static readonly ConcurrentBag<Guid> Ids = new();

        public async ValueTask<CreateResponse> Handle(CreateCommandRequest request, CancellationToken cancellationToken)
        {
            await Task.Delay(10);
            Ids.Add(request.Id);
            return new CreateResponse(request.Id);
        }

        public async ValueTask<CreateResponse> Handle(CreateRequestRequest request, CancellationToken cancellationToken)
        {
            await Task.Delay(10);
            Ids.Add(request.Id);
            return new CreateResponse(request.Id);
        }

        public async ValueTask<CreateResponse> Handle(CreateQueryRequest request, CancellationToken cancellationToken)
        {
            await Task.Delay(10);
            Ids.Add(request.Id);
            return new CreateResponse(request.Id);
        }

        public async IAsyncEnumerable<CreateResponse> Handle(
            CreateStreamRequest query,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await Task.Delay(100, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    yield break;
                }

                yield return new CreateResponse(query.Id);
            }
        }

        public async IAsyncEnumerable<CreateResponse> Handle(
            CreateStreamQuery query,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await Task.Delay(100, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    yield break;
                }

                yield return new CreateResponse(query.Id);
            }
        }

        public async IAsyncEnumerable<CreateResponse> Handle(
            CreateStreamCommand query,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await Task.Delay(100, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    yield break;
                }

                yield return new CreateResponse(query.Id);
            }
        }
    }

    [Fact]
    public async Task Test_Requests()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var handler = sp.GetRequiredService<ICommandHandler<CreateCommandRequest, CreateResponse>>();
        Assert.NotNull(handler);

        var ids = new List<Guid>();
        var responses = new List<BaseResponse>();

        const int TOTAL_COUNT = 3;
        for (int i = 0; i < TOTAL_COUNT; i++)
        {
            ids.Add(Guid.NewGuid());
        }

        responses.Add(await RunCommand(mediator, new CreateCommandRequest(ids[0])));
        responses.Add(await RunRequest(mediator, new CreateRequestRequest(ids[1])));
        responses.Add(await RunQuery(mediator, new CreateQueryRequest(ids[2])));

        for (int i = 0; i < TOTAL_COUNT; i++)
        {
            Assert.Equal(ids[i], responses[i].Id);

            Assert.Contains(ids[i], CreateHandler.Ids);
        }
    }

    [Fact]
    public async Task Test_Streaming_Requests()
    {
        var (_, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        int counter = 0;
        await foreach (var response in RunRequestStream(mediator, new CreateStreamRequest(id)))
        {
            Assert.Equal(id, response.Id);
            counter++;
        }

        Assert.Equal(3, counter);
    }

    [Fact]
    public async Task Test_Streaming_Queries()
    {
        var (_, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        int counter = 0;
        await foreach (var response in RunQueryStream(mediator, new CreateStreamQuery(id)))
        {
            Assert.Equal(id, response.Id);
            counter++;
        }

        Assert.Equal(3, counter);
    }

    [Fact]
    public async Task Test_Streaming_Commands()
    {
        var (_, mediator) = Fixture.GetMediator();

        var id = Guid.NewGuid();

        int counter = 0;
        await foreach (var response in RunCommandStream(mediator, new CreateStreamCommand(id)))
        {
            Assert.Equal(id, response.Id);
            counter++;
        }

        Assert.Equal(3, counter);
    }

    private async Task<BaseResponse> RunCommand<TCommand>(IMediator mediator, TCommand command)
        where TCommand : class, ICommand<BaseResponse>
    {
        return await mediator.Send<BaseResponse>(command);
    }

    private async Task<BaseResponse> RunRequest<TRequest>(IMediator mediator, TRequest request)
        where TRequest : class, IRequest<BaseResponse>
    {
        return await mediator.Send<BaseResponse>(request);
    }

    private async Task<BaseResponse> RunQuery<TQuery>(IMediator mediator, TQuery query)
        where TQuery : class, IQuery<BaseResponse>
    {
        return await mediator.Send<BaseResponse>(query);
    }

    private async IAsyncEnumerable<BaseResponse> RunRequestStream<TRequest>(IMediator mediator, TRequest request)
        where TRequest : class, IStreamRequest<BaseResponse>
    {
        await foreach (var response in mediator.CreateStream<BaseResponse>(request))
        {
            yield return response;
        }
    }

    private async IAsyncEnumerable<BaseResponse> RunQueryStream<TQuery>(IMediator mediator, TQuery query)
        where TQuery : class, IStreamQuery<BaseResponse>
    {
        await foreach (var response in mediator.CreateStream<BaseResponse>(query))
        {
            yield return response;
        }
    }

    private async IAsyncEnumerable<BaseResponse> RunCommandStream<TCommand>(IMediator mediator, TCommand command)
        where TCommand : class, IStreamCommand<BaseResponse>
    {
        await foreach (var response in mediator.CreateStream<BaseResponse>(command))
        {
            yield return response;
        }
    }
}
