using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Courier;
using Sample.Contracts.Order;
using Warehouse.Contracts;

namespace Sample.Components.Consumers
{
    public class FulfillOrderConsumer:
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

            builder.AddVariable("orderId", context.Message.OrderId);

            var routingSlip = builder.Build();
            
            await context.Execute(routingSlip);
        }
    }
}