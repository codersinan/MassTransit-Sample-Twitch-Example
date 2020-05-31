using System;

namespace Sample.Components.CourierActivities
{
    public class AllocateInventoryArguments
    {
        public Guid OrderId { get; set; }
        public string ItemNumber { get; set; }
        public decimal Quantity { get; set; }
    }
}