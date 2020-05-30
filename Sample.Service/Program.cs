using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Definition;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Sample.Components.Consumers;
using Sample.Components.StateMachines;

namespace Sample.Service
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));

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
                        cfg.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
                        // cfg.AddConsumer<SubmitOrderConsumer>(typeof(SubmitOrderConsumerDefinition));
                        cfg.AddSagaStateMachine<OrderStateMachine, OrderState>()
                            .RedisRepository();

                        cfg.AddBus(ConfigureBus);
                    });

                    services.AddHostedService<MassTransitConsoleHostedService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                });


            if (isService)
                await builder.UseWindowsService().Build().RunAsync();
            else
                await builder.RunConsoleAsync();
        }

        static IBusControl ConfigureBus(IRegistrationContext<IServiceProvider> context)
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg => { cfg.ConfigureEndpoints(context); });
        }
    }
}