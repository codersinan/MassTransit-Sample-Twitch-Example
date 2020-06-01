using System.Threading.Tasks;
using MassTransit;
using Sample.Contracts.Order;

namespace Sample.Components.Consumers
{
    public class FaultConsumer:
        IConsumer<Fault<FulfilOrder>>
    {
        public async Task Consume(ConsumeContext<Fault<FulfilOrder>> context)
        {
            
        }
    }
}