using System;

namespace Sample.Contracts.Order
{
    public class OrderAccepted
    {
        public Guid OrderId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}