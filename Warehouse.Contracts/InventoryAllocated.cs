using System;

namespace Warehouse.Contracts
{
    public class InventoryAllocated
    {
        public Guid AllocationId { get; set; }
        public string ItemNumber { get; set; }
        public decimal Quantity { get; set; }
    }
}