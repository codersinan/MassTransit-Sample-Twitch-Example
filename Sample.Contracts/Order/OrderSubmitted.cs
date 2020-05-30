using System;

namespace Sample.Contracts.Order
{
    public class OrderSubmitted
    {
        public Guid OrderId { get; set; }
        public DateTime Timestamp { get; set; }
        public string CustomerNumber { get; set; }
    }
}