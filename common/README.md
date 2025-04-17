## Generated Messages.cs

```csharp
// Can be run in LINQPad
void Main()
{
	string[] messageTypes = ["Request", "Query", "Command", "Notification"];
	const int n = 100;

	var output = new StringBuilder();
	output.AppendLine("#if Mediator_Large_Project");
	output.AppendLine();
	output.AppendLine("using Mediator;");
	output.AppendLine("using System.Collections.Generic;");
	output.AppendLine("using System.Threading;");
	output.AppendLine("using System.Threading.Tasks;");
	output.AppendLine("using System.Runtime.CompilerServices;");
	output.AppendLine();
	output.AppendLine("// csharpier-ignore-start");
	foreach (var messageType in messageTypes)
	{
		for (int i = 0; i < n; i++)
		{
			if (messageType == "Notification")
			{
				output.AppendLine(
					$"public sealed record Test{messageType}{i}() : I{messageType}, MediatR.I{messageType}; " +
					$"public sealed record Test{messageType}{i}Handler : I{messageType}Handler<Test{messageType}{i}>, MediatR.I{messageType}Handler<Test{messageType}{i}> {{ " +
					$"public ValueTask Handle(Test{messageType}{i} {messageType.ToLowerInvariant()}, CancellationToken cancellationToken) => default; " +
					$"Task MediatR.I{messageType}Handler<Test{messageType}{i}>.Handle(Test{messageType}{i} {messageType.ToLowerInvariant()}, CancellationToken cancellationToken) => Task.CompletedTask; " +
					$"}}"
				);
			}
			else
			{
				output.AppendLine(
					$"public sealed record Test{messageType}{i}() : I{messageType}<int>{(messageType == "Request" ? ", MediatR.IRequest<int>" : "")}; " +
					$"public sealed record Test{messageType}{i}Handler : I{messageType}Handler<Test{messageType}{i}, int>{(messageType == "Request" ? $", MediatR.IRequestHandler<Test{messageType}{i}, int>" : "")} {{ " +
					$"public ValueTask<int> Handle(Test{messageType}{i} {messageType.ToLowerInvariant()}, CancellationToken cancellationToken) => new ValueTask<int>({i}); " +
					(messageType == "Request" ? $"Task<int> MediatR.IRequestHandler<Test{messageType}{i}, int>.Handle(Test{messageType}{i} {messageType.ToLowerInvariant()}, CancellationToken cancellationToken) => Task.FromResult({i}); " : "") +
					$"}}"
				);
			}
		}
		output.AppendLine();
	}

	foreach (var messageType in messageTypes)
	{
		if (messageType == "Notification")
			continue;

		output.AppendLine();
		for (int i = 0; i < n; i++)
		{
			output.AppendLine(
				$"public sealed record TestStream{messageType}{i}() : IStream{messageType}<int>{(messageType == "Request" ? ", MediatR.IStreamRequest<int>" : "")}; " +
				$"public sealed record TestStream{messageType}{i}Handler : IStream{messageType}Handler<TestStream{messageType}{i}, int>{(messageType == "Request" ? $", MediatR.IStreamRequestHandler<TestStream{messageType}{i}, int>" : "")} {{ " +
				$"public async IAsyncEnumerable<int> Handle(TestStream{messageType}{i} {messageType.ToLowerInvariant()}, [EnumeratorCancellation] CancellationToken cancellationToken) {{ for (int i = 0; i < 3; i++) {{ await Task.Yield(); yield return {i}; }} }} " +
				(messageType == "Request" ? $"async IAsyncEnumerable<int> MediatR.IStreamRequestHandler<TestStream{messageType}{i}, int>.Handle(TestStream{messageType}{i} {messageType.ToLowerInvariant()}, [EnumeratorCancellation] CancellationToken cancellationToken) {{ for (int i = 0; i < 3; i++) {{ await Task.Yield(); yield return {i}; }} }} " : "") +
				$"}}"
			);
		}
	}

	output.AppendLine("// csharpier-ignore-end");
	output.AppendLine();
	output.AppendLine("#endif");
	output.ToString().Dump();
}
```
