using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreIndirect.BaseClasses;

[ApiController]
public abstract class ApplicationController : ControllerBase
{
    /// <summary>
    /// Send request through Mediator and convert result to IActionResult.
    /// </summary>
    /// <param name="request">request to process.</param>
    /// <typeparam name="TRequest">type of the request.</typeparam>
    /// <typeparam name="TResponse">type of the response.</typeparam>
    /// <returns>Action result.</returns>
    protected async Task<IActionResult> ProcessAsync<TRequest, TResponse>(TRequest request)
        where TRequest : IRequest<TResponse>
    {
        try
        {
            var mediator = HttpContext.RequestServices.GetRequiredService<IMediator>();

            var result = await mediator.Send(request, HttpContext.RequestAborted);

            if (typeof(TResponse) == typeof(Unit))
            {
                return NoContent();
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return new ObjectResult(new { ex.Message }) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }
}
