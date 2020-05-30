using System;

namespace Sample.Contracts.Order
{
    public class OrderStatus
    {
        public Guid OrderId { get; set; }
        public string State { get; set; }
        public string CustomerNumber { get; set; }
    }
}