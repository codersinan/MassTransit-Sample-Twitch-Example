using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Courier.Contracts;
using Microsoft.Extensions.Logging;

namespace Sample.Components.BatchConsumers
{
    public class RoutingSlipBatchEventConsumer :
        IConsumer<Batch<RoutingSlipCompleted>>
    {
        private readonly ILogger<RoutingSlipCompleted> _logger;

        public RoutingSlipBatchEventConsumer(ILogger<RoutingSlipCompleted> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<Batch<RoutingSlipCompleted>> context)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.Log(LogLevel.Information,$"Routing Slip Completed:{string.Join(", ", context.Message.Select(x=>x.Message.TrackingNumber))}");

            return Task.CompletedTask;
        }
    }
}