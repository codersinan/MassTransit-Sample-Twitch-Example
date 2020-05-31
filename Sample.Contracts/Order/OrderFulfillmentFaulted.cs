using System;

namespace Sample.Contracts.Order
{
    public class OrderFulfillmentFaulted
    {
        public Guid OrderId { get; set; }
        
        public DateTime Timestamp { get; set; }
    }
}