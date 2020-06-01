using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Courier.Contracts;
using Microsoft.Extensions.Logging;

namespace Sample.Components.Consumers
{
    public class RoutingSlipEventConsumer :
        // IConsumer<RoutingSlipCompleted>,
        IConsumer<RoutingSlipFaulted>,
        IConsumer<RoutingSlipActivityCompleted>
    {
        private readonly ILogger<RoutingSlipCompleted> _logger;

        public RoutingSlipEventConsumer(ILogger<RoutingSlipCompleted> logger)
        {
            _logger = logger;
        }

        // public Task Consume(ConsumeContext<RoutingSlipCompleted> context)
        // {
        //     if (_logger.IsEnabled(LogLevel.Information))
        //         _logger.Log(LogLevel.Information, $"Routing Slip Completed: {context.Message.TrackingNumber}");
        //
        //     return Task.CompletedTask;
        // }

        public Task Consume(ConsumeContext<RoutingSlipActivityCompleted> context)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.Log(LogLevel.Information,
                    $"Routing Slip Activity Completed: {context.Message.TrackingNumber} {context.Message.ActivityName}");

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<RoutingSlipFaulted> context)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.Log(LogLevel.Information,
                    $"Routing Slip Faulted: {context.Message.TrackingNumber} {context.Message.ActivityExceptions.FirstOrDefault()}");

            return Task.CompletedTask;
        }
    }
}