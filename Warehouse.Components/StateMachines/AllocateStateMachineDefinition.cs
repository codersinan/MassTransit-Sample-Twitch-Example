using GreenPipes;
using MassTransit;
using MassTransit.Definition;

namespace Warehouse.Components.StateMachines
{
    public class AllocateStateMachineDefinition :
        SagaDefinition<AllocationState>
    {
        public AllocateStateMachineDefinition()
        {
            ConcurrentMessageLimit = 4;
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator,
            ISagaConfigurator<AllocationState> sagaConfigurator)
        {
            endpointConfigurator.UseMessageRetry(r => r.Interval(3, 500));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}