using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.MongoDbIntegration.MessageData;
using MassTransit.Platform.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Sample.Components.BatchConsumers;
using Sample.Components.Consumers;
using Sample.Components.CourierActivities;
using Sample.Components.OrderStateMachineActivities;
using Sample.Components.StateMachines;
using Warehouse.Contracts;

namespace Sample.Startup
{
    public class SampleStartup :
        IPlatformStartup
    {
        public void ConfigureMassTransit(IServiceCollectionBusConfigurator configurator, IServiceCollection services)
        {
            services.AddScoped<AcceptOrderActivity>();

            configurator.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
            configurator.AddActivitiesFromNamespaceContaining<AllocateInventoryActivity>();
            configurator.AddConsumersFromNamespaceContaining<RoutingSlipBatchEventConsumer>();

            configurator.AddSagaStateMachine<OrderStateMachine, OrderState>(typeof(OrderStateMachineDefinition))
                .MongoDbRepository(r =>
                {
                    r.Connection = "mongodb://172.20.0.25:27017";//mongodb://mongo doesnt work. Changed to docker machine ip address
                    r.DatabaseName = "orders";
                });

            configurator.AddRequestClient<AllocateInventory>();
        }

        public void ConfigureBus<TEndpointConfigurator>(IBusFactoryConfigurator<TEndpointConfigurator> configurator,
            IBusRegistrationContext context)
            where TEndpointConfigurator : IReceiveEndpointConfigurator
        {
            configurator.UseMessageData(new MongoDbMessageDataRepository("mongodb://172.20.0.25:27017", "attachments"));//mongodb://mongo doesnt work. Changed to docker machine ip address
        }
    }
}