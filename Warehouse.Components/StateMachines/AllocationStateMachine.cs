using System;
using Automatonymous;
using MassTransit;
using Warehouse.Contracts;

namespace Warehouse.Components.StateMachines
{
    public class AllocationStateMachine :
        MassTransitStateMachine<AllocationState>
    {
        #region States

        public State Allocated { get; set; }
        public State Released { get; set; }

        #endregion

        #region Events

        public Event<AllocationCreated> AllocationCreated { get; set; }

        #endregion

        #region Schedules

        public Schedule<AllocationState, AllocationHoldDurationExpired> HoldExpiration { get; set; }

        #endregion

        public AllocationStateMachine()
        {
            Event(() => AllocationCreated, x => x.CorrelateById(m => m.Message.AllocationId));

            Schedule(() => HoldExpiration, x => x.HoldDurationToken, s =>
            {
                s.Delay = TimeSpan.FromHours(1);
                s.Received = m => m.CorrelateById(p => p.Message.AllocationId);
            });

            InstanceState(x => x.CurrentState);

            Initially(
                When(AllocationCreated)
                    .Schedule(HoldExpiration,
                        context => context.Init<AllocationHoldDurationExpired>(new {context.Data.AllocationId}),
                        context => context.Data.HoldDuration
                    )
                    .TransitionTo(Allocated)
            );

            During(Allocated,
                When(HoldExpiration.Received)
                    .ThenAsync(context =>
                        Console.Out.WriteLineAsync($"Allocation was released: {context.Instance.CorrelationId}"))
                    .Finalize()
            );
            SetCompletedWhenFinalized();
        }
    }
}