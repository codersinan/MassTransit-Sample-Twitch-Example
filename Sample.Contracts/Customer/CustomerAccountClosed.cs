using System;

namespace Sample.Contracts.Customer
{
    public class CustomerAccountClosed
    {
        public Guid CustomerId { get; set; }
        public string CustomerNumber { get; set; }
    }
}