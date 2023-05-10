using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
        public CreateResponse(Guid id) : base(id) { }
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

    public sealed class CreateHandler
        : ICommandHandler<CreateCommandRequest, CreateResponse>,
          IRequestHandler<CreateRequestRequest, CreateResponse>,
          IQueryHandler<CreateQueryRequest, CreateResponse>
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
    }

    [Fact]
    public async Task Test_Creat()
    {
        var (sp, mediator) = Fixture.GetMediator();

        var handler = sp.GetRequiredService<CreateHandler>();
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

    private async Task<BaseResponse> RunCommand<TCommand>(IMediator mediator, TCommand command)
        where TCommand : class, ICommand<BaseResponse>
    {
        return await mediator.Send(command);
    }

    private async Task<BaseResponse> RunRequest<TRequest>(IMediator mediator, TRequest request)
        where TRequest : class, IRequest<BaseResponse>
    {
        return await mediator.Send(request);
    }

    private async Task<BaseResponse> RunQuery<TQuery>(IMediator mediator, TQuery query)
        where TQuery : class, IQuery<BaseResponse>
    {
        return await mediator.Send(query);
    }
}
