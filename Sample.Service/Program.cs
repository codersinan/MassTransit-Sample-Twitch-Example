using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Courier.Contracts;
using MassTransit.Definition;
using MassTransit.MongoDbIntegration;
using MassTransit.MongoDbIntegration.MessageData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.Components.BatchConsumers;
using Sample.Components.Consumers;
using Sample.Components.CourierActivities;
using Sample.Components.OrderStateMachineActivities;
using Sample.Components.StateMachines;
using Serilog;
using Serilog.Events;
using Warehouse.Contracts;

namespace Sample.Service
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
            
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", true);
                    config.AddEnvironmentVariables();

                    if (args != null)
                        config.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddScoped<AcceptOrderActivity>();

                    services.AddScoped<RoutingSlipBatchEventConsumer>();

                    services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
                    services.AddMassTransit(cfg =>
                    {
                        cfg.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
                        // cfg.AddConsumer<SubmitOrderConsumer>(typeof(SubmitOrderConsumerDefinition));
                        cfg.AddActivitiesFromNamespaceContaining<AllocateInventoryActivity>();

                        cfg.AddSagaStateMachine<OrderStateMachine, OrderState>(typeof(OrderStateMachineDefinition))
                            .MongoDbRepository(r =>
                            {
                                r.Connection = "mongodb://127.0.0.1";
                                r.DatabaseName = "orders";
                            });

                        cfg.AddBus(ConfigureBus);

                        cfg.AddRequestClient<AllocateInventory>();
                    });

                    services.AddHostedService<MassTransitConsoleHostedService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddSerilog(dispose: true);
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    // logging.AddConsole();
                });


            if (isService)
                await builder.UseWindowsService().Build().RunAsync();
            else
                await builder.RunConsoleAsync();
            
            Log.CloseAndFlush();
        }

        static IBusControl ConfigureBus(IRegistrationContext<IServiceProvider> context)
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.UseMessageData(new MongoDbMessageDataRepository("mongodb://127.0.0.1","attachments"));
                
                cfg.UseMessageScheduler(new Uri("queue://quartz"));
                cfg.ReceiveEndpoint(KebabCaseEndpointNameFormatter.Instance.Consumer<RoutingSlipBatchEventConsumer>(),
                    e =>
                    {
                        e.PrefetchCount = 10;
                        e.Batch<RoutingSlipCompleted>(b =>
                        {
                            b.MessageLimit = 10;
                            b.TimeLimit = TimeSpan.FromSeconds(5);

                            b.Consumer<RoutingSlipBatchEventConsumer, RoutingSlipCompleted>(context.Container);
                        });
                    });
                cfg.ConfigureEndpoints(context);
            });
        }
    }
}