using System;

namespace Sample.Api.Models
{
    public class OrderViewModel
    {
        public Guid Id { get; set; }
        public string CustomerNumber { get; set; }
        public string PaymentCardNumber { get; set; }
    }
}