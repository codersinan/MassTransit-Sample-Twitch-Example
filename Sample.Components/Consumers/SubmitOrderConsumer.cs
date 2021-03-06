using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Sample.Contracts.Order;

namespace Sample.Components.Consumers
{
    public class SubmitOrderConsumer :
        IConsumer<SubmitOrder>
    {
        private readonly ILogger<SubmitOrderConsumer> _logger;

        public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger)
        {
            _logger = logger;
        }

        public SubmitOrderConsumer()
        {
        }

        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            _logger?.Log(LogLevel.Debug, $"SubmitOrderConsumer:{context.Message.CustomerNumber}");

            if (context.Message.CustomerNumber.Contains("TEST"))
            {
                if (context.RequestId != null)
                {
                    await context.RespondAsync<OrderSubmissionRejected>(new OrderSubmissionRejected
                    {
                        Timestamp = InVar.Timestamp,
                        OrderId = context.Message.OrderId,
                        CustomerNumber = context.Message.CustomerNumber,
                        Reason = $"Test Customer cannot submit orders: {context.Message.CustomerNumber}"
                    });
                }

                return;
            }

            var notes = context.Message.Notes;
            if (notes.HasValue)
            {
                string notesValue = await notes.Value;
                Console.WriteLine($"Notes: {notesValue}");
            }

            await context.Publish<OrderSubmitted>(new OrderSubmitted
            {
                OrderId = context.Message.OrderId,
                Timestamp = context.Message.Timestamp,
                CustomerNumber = context.Message.CustomerNumber,
                PaymentCardNumber = context.Message.PaymentCardNumber,
                Notes=context.Message.Notes
            });

            if (context.RequestId != null)
            {
                await context.RespondAsync<OrderSubmissionAccepted>(new OrderSubmissionAccepted
                {
                    Timestamp = InVar.Timestamp,
                    OrderId = context.Message.OrderId,
                    CustomerNumber = context.Message.CustomerNumber
                });
            }
        }
    }
}