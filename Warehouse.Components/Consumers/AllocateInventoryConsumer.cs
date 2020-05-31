using System;
using System.Threading.Tasks;
using MassTransit;
using Warehouse.Contracts;

namespace Warehouse.Components.Consumers
{
    public class AllocateInventoryConsumer :
        IConsumer<AllocateInventory>
    {
        public async Task Consume(ConsumeContext<AllocateInventory> context)
        {
            await context.Publish<AllocationCreated>(new AllocationCreated
            {
                AllocationId = context.Message.AllocationId,
                HoldDuration = new TimeSpan(15000)
            });
            await context.RespondAsync<InventoryAllocated>(new InventoryAllocated
            {
                AllocationId = context.Message.AllocationId,
                ItemNumber = context.Message.ItemNumber,
                Quantity = context.Message.Quantity
            });
        }
    }
}