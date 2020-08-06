using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Definition;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Warehouse.Components.Consumers;
using Warehouse.Components.StateMachines;

namespace Warehouse.Service
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
                    services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
                    services.AddMassTransit(cfg =>
                    {
                        cfg.AddConsumersFromNamespaceContaining<AllocateInventoryConsumer>();
                        cfg.AddSagaStateMachine<AllocationStateMachine, AllocationState>(
                                typeof(AllocateStateMachineDefinition))
                            .MongoDbRepository(r =>
                            {
                                r.Connection = "mongodb://127.0.0.1";
                                r.DatabaseName = "allocations";
                            });

                        cfg.UsingRabbitMq(ConfigureBus);
                    });

                    services.AddHostedService<MassTransitConsoleHostedService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddSerilog(dispose: true);
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                });


            if (isService)
                await builder.UseWindowsService().Build().RunAsync();
            else
                await builder.RunConsoleAsync();

            Log.CloseAndFlush();
        }

        static void ConfigureBus(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator configurator)
        {
            configurator.UseMessageScheduler(new Uri("rabbitmq://localhost/quartz"));
            configurator.ConfigureEndpoints(context);
        }
    }
}