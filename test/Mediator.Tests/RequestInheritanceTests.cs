using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mediator.Tests
{
    public sealed class RequestInheritanceTests
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

        public class CreateResponseWithN : CreateResponse
        {
            public CreateResponseWithN(Guid id, int N) : base(id)
            {
                this.N = N;
            }

            public int N { get; }
        }

        public abstract class BaseRequest<T> : IRequest<T>
        {
            public Guid Id { get; }

            public BaseRequest(Guid id) => Id = id;
        }

        public class CreateRequest : BaseRequest<CreateResponse>
        {
            public CreateRequest(Guid id) : base(id) { }
        }

        public class CreateRequestWithN : BaseRequest<CreateResponseWithN>
        {
            public CreateRequestWithN(Guid id, int N) : base(id)
            {
                this.N = N;
            }

            public int N { get; }
        }

        public sealed class CreateHandler :
            IRequestHandler<CreateRequest, CreateResponse>,
            IRequestHandler<CreateRequestWithN, CreateResponseWithN>
        {
            internal Guid Id;
            internal Guid IdForN;

            public ValueTask<CreateResponse> Handle(CreateRequest request, CancellationToken cancellationToken)
            {
                Id = request.Id;
                return new ValueTask<CreateResponse>(new CreateResponse(request.Id));
            }

            public ValueTask<CreateResponseWithN> Handle(CreateRequestWithN request, CancellationToken cancellationToken)
            {
                IdForN = request.Id;
                return new ValueTask<CreateResponseWithN>(new CreateResponseWithN(request.Id, request.N));
            }
        }

        [Fact]
        public async Task Test_Create()
        {
            var (sp, mediator) = Fixture.GetMediator();

            var id = Guid.NewGuid();
            var idForN = Guid.NewGuid();

            var response = await mediator.Send(new CreateRequest(id));
            Assert.Equal(id, response.Id);

            var responseWithN = await mediator.Send(new CreateRequestWithN(idForN, 3));
            Assert.Equal(idForN, responseWithN.Id);
            Assert.Equal(3, responseWithN.N);

            var handler = sp.GetRequiredService<CreateHandler>();
            Assert.Equal(id, handler.Id);
            Assert.Equal(idForN, handler.IdForN);
        }
    }
}
