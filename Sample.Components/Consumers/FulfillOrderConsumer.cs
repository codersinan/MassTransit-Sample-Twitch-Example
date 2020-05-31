using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Courier;
using MassTransit.Courier.Contracts;
using Sample.Contracts.Order;
using Warehouse.Contracts;

namespace Sample.Components.Consumers
{
    public class FulfillOrderConsumer :
        IConsumer<FulfilOrder>
    {
        public async Task Consume(ConsumeContext<FulfilOrder> context)
        {
            var builder = new RoutingSlipBuilder(NewId.NextGuid());

            builder.AddActivity("AllocateInventory", new Uri("queue:allocate-inventory_execute"), new AllocateInventory
            {
                ItemNumber = "Item123",
                Quantity = 10.0m
            });

            builder.AddActivity("PaymentActivity", new Uri("queue:payment_execute"), new Payment
            {
                CardNumber = "5999-1234-5678-9812",
                Amount = 99.95m
            });

            builder.AddVariable("OrderId", context.Message.OrderId);

            await builder.AddSubscription(context.SourceAddress, RoutingSlipEvents.Faulted,
                RoutingSlipEventContents.None,
                x => x.Send<OrderFulfillmentFaulted>(new OrderFulfillmentFaulted
                {
                    OrderId = context.Message.OrderId,
                    Timestamp = InVar.Timestamp
                }));

            var routingSlip = builder.Build();

            await context.Execute(routingSlip);
        }
    }
}