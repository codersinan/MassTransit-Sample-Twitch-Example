using System;

namespace Warehouse.Contracts
{
    public class AllocationReleaseRequested
    {
        public Guid AllocationId { get; set; }
        public string Reason { get; set; }
    }
}