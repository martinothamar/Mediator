using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WPFApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ConcurrentQueue<(Ping Ping, Pong Pong)> _items;

        public MainWindow()
        {
            InitializeComponent();

            _items = new ConcurrentQueue<(Ping Ping, Pong Pong)>();

            lvDataBinding.ItemsSource = _items;

            _ = Task.Run(
                async () =>
                {
                    var sp = App.ServiceProvider;

                    var mediator = sp.GetRequiredService<IMediator>();

                    for (int i = 0; i < 100; i++)
                    {
                        var ping = new Ping();
                        var pong = await mediator.Send(ping);

                        lvDataBinding.Dispatcher.Invoke(
                            () =>
                            {
                                _items.Enqueue((ping, pong));
                            },
                            DispatcherPriority.Render
                        );

                        await Task.Delay(2000);
                    }
                }
            );
        }
    }

    public sealed class Ping : IRequest<Pong>
    {
        public readonly long Timestamp = Stopwatch.GetTimestamp();
    }

    public sealed class Pong
    {
        public readonly long Timestamp = Stopwatch.GetTimestamp();

        public readonly long Latency;

        public Pong(Ping ping) => Latency = Timestamp - ping.Timestamp;
    }

    public sealed class PingHandler : IRequestHandler<Ping, Pong>
    {
        public ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken)
        {
            return new ValueTask<Pong>(new Pong(request));
        }
    }
}
