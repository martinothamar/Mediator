using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

var services = new ServiceCollection();

services.AddMediator(
    options =>
    {
        options.Namespace = null;
        options.ServiceLifetime = ServiceLifetime.Transient;
    }
);

var serviceProvider = services.BuildServiceProvider();

var mediator = serviceProvider.GetRequiredService<IMediator>();

var request = new ExportRequest();

var response = await mediator.Send(request);
Debug.Assert(4 == response.Length);

return 0;

//
// Here are the types used
//

public class ExportRequest : IRequest<byte[]> { }
