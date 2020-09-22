using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Topology;
using Rabbit.Details.CommandHandlers;
using Rabbit.Details.Commands;
using RabbitMQ.Client;

namespace Rabbit.Details
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host("localhost", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                //
                //cfg.MessageTopology.SetEntityNameFormatter(new MyFormatter());
                //OR
                //cfg.Message<UpdateAccountCommand>(m =>
                //{
                //    m.SetEntityName("update-account");
                //});
                //

                //cfg.ReceiveEndpoint("account-service", e =>
                //{
                //    // means that this endpoint does not receive any message while publishing
                //    //e.ConfigureConsumeTopology = false;

                //    // avoid overloading memory
                //    e.Lazy = true;

                //    e.PrefetchCount = 20;

                //    // to explicitly bind a command
                //    //e.Bind<IUpdateAccountCommand>();

                //    // bind to exchange
                //    //e.Bind("account");

                //    e.Consumer<UpdateAccountCommandHandler>();
                //});

                //cfg.ReceiveEndpoint("another-account-service", e =>
                //{
                //    e.PrefetchCount = 10;

                //    e.Consumer<AnotherUpdateAccountCommandHandler>();
                //});

                cfg.Publish<UpdateAccountCommand>(p =>
                {
                    p.ExchangeType = ExchangeType.Direct;
                    p.BindAlternateExchangeQueue("unmatched");
                });

                cfg.ReceiveEndpoint("account-service-a", e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Lazy = true;
                    e.PrefetchCount = 20;

                    e.Bind<UpdateAccountCommand>(b =>
                    {
                        b.ExchangeType = ExchangeType.Direct;
                        b.RoutingKey = "A";
                    });

                    e.Consumer<UpdateAccountCommandHandler>();
                });

                cfg.ReceiveEndpoint("account-service-b", e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Lazy = true;
                    e.PrefetchCount = 20;

                    e.Bind<UpdateAccountCommand>(b =>
                    {
                        b.ExchangeType = ExchangeType.Direct;
                        b.RoutingKey = "B";
                    });

                    e.Consumer<UpdateAccountCommandHandler>();
                });
            });

            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            await busControl.StartAsync(cancellation.Token);

            try
            {
                Console.WriteLine("Bus was started");

                // Send
                //var endpoint = await busControl.GetSendEndpoint(new Uri("exchange:account-service"));

                //await endpoint.Send<IUpdateAccountCommand>(new
                //{
                //    AccountNumber = "123456"
                //});

                // Send
                //var endpoint = await busControl.GetSendEndpoint(new Uri("exchange:account"));

                //await endpoint.Send<UpdateAccountCommand>(new
                //{
                //    AccountNumber = "123456"
                //});

                //await endpoint.Send<DeleteAccountCommand>(new
                //{
                //    AccountNumber = "789456"
                //});

                // Publish
                //await busControl.Publish<UpdateAccountCommand>(new
                //{
                //    AccountNumber = "123456"
                //});

                await busControl.Publish<UpdateAccountCommand>(new
                {
                    AccountNumber = "12345-A"
                }, x =>
                {
                    x.SetRoutingKey("A");
                });

                await busControl.Publish<UpdateAccountCommand>(new
                {
                    AccountNumber = "67890-B"
                }, x =>
                {
                    x.SetRoutingKey("B");
                });

                await busControl.Publish<UpdateAccountCommand>(new
                {
                    AccountNumber = "67890-C"
                }, x =>
                {
                    x.SetRoutingKey("C");
                });

                //because Console.ReadLine() blocks the primary thread so:
                await Task.Run(Console.ReadLine);
            }
            finally
            {
                await busControl.StopAsync();
            }
        }
    }

    public class MyFormatter : IEntityNameFormatter
    {
        public string FormatEntityName<T>()
        {
            return typeof(T).Name;
        }
    }
}
