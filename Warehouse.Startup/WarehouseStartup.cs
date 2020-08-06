using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.Platform.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Components.Consumers;
using Warehouse.Components.StateMachines;

namespace Warehouse.Startup
{
    public class WarehouseStartup :
        IPlatformStartup
    {
        public void ConfigureMassTransit(IServiceCollectionBusConfigurator configurator, IServiceCollection services)
        {
            configurator.AddConsumersFromNamespaceContaining<AllocateInventoryConsumer>();
            configurator
                .AddSagaStateMachine<AllocationStateMachine, AllocationState>(typeof(AllocateStateMachineDefinition))
                .MongoDbRepository(r =>
                {
                    r.Connection = "mongodb://172.20.0.25:27017";//mongodb://mongo doesnt work. Changed to docker machine ip address
                    r.DatabaseName = "allocations";
                });
        }

        public void ConfigureBus<TEndpointConfigurator>(IBusFactoryConfigurator<TEndpointConfigurator> configurator,
            IBusRegistrationContext context)
            where TEndpointConfigurator : IReceiveEndpointConfigurator
        {
        }
    }
}