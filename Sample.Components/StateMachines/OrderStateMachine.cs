using System;
using System.Linq;
using Automatonymous;
using MassTransit;
using Sample.Components.OrderStateMachineActivities;
using Sample.Contracts.Customer;
using Sample.Contracts.Order;

namespace Sample.Components.StateMachines
{
    public class OrderStateMachine :
        MassTransitStateMachine<OrderState>
    {
        #region Events

        public Event<OrderSubmitted> OrderSubmitted { get; private set; }
        public Event<CheckOrder> OrderStateRequested { get; private set; }

        public Event<CustomerAccountClosed> AccountClosed { get; private set; }

        public Event<OrderAccepted> OrderAccepted { get; set; }
        public Event<OrderFulfillmentFaulted> FulfillmentFaulted { get; set; }
        public Event<OrderFulfillmentCompleted> FulfillmentCompleted { get; set; }

        public Event<Fault<FulfilOrder>> FulfillOrderFaulted { get; set; }

        #endregion

        #region States

        public State Submitted { get; private set; }
        public State Accepted { get; private set; }
        public State Cancelled { get; private set; }
        public State Faulted { get; private set; }
        public State Completed { get; private set; }

        #endregion

        public OrderStateMachine()
        {
            Event(() => OrderSubmitted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => OrderStateRequested, x =>
            {
                x.CorrelateById(m => m.Message.OrderId);
                x.OnMissingInstance(m => m.ExecuteAsync(async context =>
                {
                    if (context.RequestId.HasValue)
                    {
                        await context.RespondAsync<OrderNotFound>(new OrderNotFound
                        {
                            OrderId = context.Message.OrderId
                        });
                    }
                }));
            });
            Event(() => AccountClosed,
                x => x.CorrelateBy((saga, context) => saga.CustomerNumber == context.Message.CustomerNumber));
            Event(() => OrderAccepted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => FulfillmentFaulted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => FulfillmentCompleted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => FulfillOrderFaulted, x => x.CorrelateById(m => m.Message.Message.OrderId));

            InstanceState(x => x.CurrentState);

            Initially(
                When(OrderSubmitted)
                    .Then(context =>
                    {
                        context.Instance.SubmitDate = context.Data.Timestamp;
                        context.Instance.CustomerNumber = context.Data.CustomerNumber;
                        context.Instance.PaymentCardNumber = context.Data.PaymentCardNumber;

                        context.Instance.Updated = DateTime.UtcNow;
                    })
                    .TransitionTo(Submitted)
            );

            During(Submitted,
                Ignore(OrderSubmitted),
                When(AccountClosed)
                    .TransitionTo(Cancelled),
                When(OrderAccepted)
                    .Activity(x => x.OfType<AcceptOrderActivity>()
                        .TransitionTo(Accepted))
            );

            During(Accepted,
                When(FulfillOrderFaulted)
                    .Then(context =>
                        Console.WriteLine(
                            $"Fulfill Order Faulted: {context.Data.Exceptions.FirstOrDefault()?.Message}"))
                    .TransitionTo(Faulted),
                When(FulfillmentFaulted)
                    .TransitionTo(Faulted),
                When(FulfillmentCompleted)
                    .TransitionTo(Completed)
            );

            DuringAny(
                When(OrderStateRequested)
                    .RespondAsync(x => x.Init<OrderStatus>(new OrderStatus
                    {
                        OrderId = x.Instance.CorrelationId,
                        State = x.Instance.CurrentState,
                        CustomerNumber = x.Instance.CustomerNumber
                    }))
            );

            DuringAny(
                When(OrderSubmitted)
                    .Then(context =>
                    {
                        context.Instance.SubmitDate ??= context.Data.Timestamp;
                        context.Instance.CustomerNumber ??= context.Data.CustomerNumber;
                    })
            );
        }
    }
}