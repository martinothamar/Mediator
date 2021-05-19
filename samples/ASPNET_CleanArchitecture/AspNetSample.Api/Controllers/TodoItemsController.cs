using AspNetSample.Application;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetSample.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiControllerBase : ControllerBase
    {
        private IMediator? _mediator;

        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();
    }

    public sealed class TodoItemsController : ApiControllerBase
    {
        [HttpGet]
        public async ValueTask<ActionResult<IEnumerable<TodoItemDto>>> GetItems(CancellationToken cancellationToken)
        {
            var items = await Mediator.Send(new GetTodoItems(), cancellationToken);

            if (items is null || !items.Any())
                return NoContent();
            else
                return Ok(items);
        }

        [HttpPost]
        public async ValueTask<ActionResult> AddItem([FromBody] AddTodoItem item, CancellationToken cancellationToken)
        {
            try
            {
                var response = await Mediator.Send(item, cancellationToken);
                return Ok(response);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.ValidationError);
            }
        }
    }
}
