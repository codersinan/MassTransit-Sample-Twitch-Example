using System;

namespace Sample.Contracts.Order
{
    public class FulfilOrder
    {
        public Guid OrderId { get; set; }
        public string CustomerNumber { get; set; }
        public string PaymentCardNumber { get; set; }
    }
}