using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

// 定义一个流式查询
public sealed record StreamQuery(string Query) : IStreamQuery<StreamResult>;

public sealed record StreamResult(int Index, string Data);

// 流式查询处理器
public sealed class StreamQueryHandler : IStreamQueryHandler<StreamQuery, StreamResult>
{
    public async IAsyncEnumerable<StreamResult> Handle(
        StreamQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        for (int i = 0; i < 5; i++)
        {
            await Task.Delay(100, cancellationToken);
            yield return new StreamResult(i, $"处理 '{query.Query}' - 结果 {i}");
        }
    }
}

// 流消息预处理器 - 在处理器执行前运行
public sealed class StreamPreProcessor<TMessage, TResponse> : StreamMessagePreProcessor<TMessage, TResponse>
    where TMessage : notnull, IStreamMessage
{
    protected override ValueTask Handle(TMessage message, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[预处理] 开始处理流消息: {typeof(TMessage).Name}");
        return default;
    }
}

// 流消息后处理器 - 在每个流项产生后运行
public sealed class StreamPostProcessor<TMessage, TResponse> : StreamMessagePostProcessor<TMessage, TResponse>
    where TMessage : notnull, IStreamMessage
{
    protected override ValueTask Handle(TMessage message, TResponse response, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[后处理] 收到流项: {response}");
        return default;
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        // 配置依赖注入
        var services = new ServiceCollection();

        // 添加 Mediator
        services.AddMediator();

        // 注册流消息处理器
        services.AddSingleton(typeof(IStreamPipelineBehavior<,>), typeof(StreamPreProcessor<,>));
        services.AddSingleton(typeof(IStreamPipelineBehavior<,>), typeof(StreamPostProcessor<,>));

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        Console.WriteLine("=== 流消息处理示例 ===\n");

        // 发送流式查询
        var query = new StreamQuery("测试查询");

        int count = 0;
        await foreach (var result in mediator.CreateStream(query))
        {
            count++;
            Console.WriteLine($"[主程序] 接收到结果: {result.Data}");
        }

        Console.WriteLine($"\n总共接收到 {count} 个结果");
        Console.WriteLine("\n注意:");
        Console.WriteLine("- 预处理器在流开始前执行一次");
        Console.WriteLine("- 后处理器在每个流项产生后执行");
    }
}
