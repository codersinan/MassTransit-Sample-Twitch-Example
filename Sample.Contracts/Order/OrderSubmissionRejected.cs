using System;

namespace Sample.Contracts.Order
{
    public class OrderSubmissionRejected
    {
        public Guid OrderId { get; set; }
        public DateTime Timestamp { get; set; }
        
        public string CustomerNumber { get; set; }

        public string Reason { get; set; }
    }
}