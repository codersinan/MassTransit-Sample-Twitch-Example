using System;

namespace Sample.Contracts.Order
{
    public class OrderFulfillmentFaulted
    {
        public Guid OrderId { get; set; }
        
        public DateTime Timestamp { get; set; }
    }
    
    public class OrderFulfillmentCompleted
    {
        public Guid OrderId { get; set; }
        
        public DateTime Timestamp { get; set; }
    }
}