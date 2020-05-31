using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Courier;
using Warehouse.Contracts;

namespace Sample.Components.CourierActivities
{
    public class AllocateInventoryActivity :
        IActivity<AllocateInventoryArguments, AllocateInventoryLogs>
    {
        private IRequestClient<AllocateInventory> _requestClient;

        public AllocateInventoryActivity(IRequestClient<AllocateInventory> requestClient)
        {
            _requestClient = requestClient;
        }

        public async Task<ExecutionResult> Execute(ExecuteContext<AllocateInventoryArguments> context)
        {
            var orderId = context.Arguments.OrderId;
            var itemNumber = context.Arguments.ItemNumber;
            if (string.IsNullOrEmpty(itemNumber))
                throw new ArgumentNullException(nameof(itemNumber));
            var quantity = context.Arguments.Quantity;
            if (quantity <= 0)
                throw new ArgumentNullException(nameof(quantity));

            var allocationId = NewId.NextGuid();
            var response = await _requestClient.GetResponse<InventoryAllocated>(new InventoryAllocated
            {
                AllocationId = allocationId,
                ItemNumber = itemNumber,
                Quantity = quantity
            });

            return context.Completed(new AllocateInventoryLogs
            {
                AllocationId = allocationId
            });
        }

        public async Task<CompensationResult> Compensate(CompensateContext<AllocateInventoryLogs> context)
        {
            await context.Publish<AllocationReleaseRequested>(new AllocationReleaseRequested
            {
                AllocationId = context.Log.AllocationId,
                Reason = $"Order Failed"
            });
            return context.Compensated();
        }
    }
}