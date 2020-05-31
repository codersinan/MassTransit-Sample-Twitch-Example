using System;

namespace Warehouse.Contracts
{
    public class AllocateInventory
    {
        public Guid AllocationId { get; set; }
        public string ItemNumber { get; set; }
        public decimal Quantity { get; set; }
    }
}