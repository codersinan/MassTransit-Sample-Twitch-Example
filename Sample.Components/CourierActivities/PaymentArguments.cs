using System;

namespace Sample.Components.CourierActivities
{
    public class PaymentArguments
    {
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string CardNumber { get; set; }
    }
}