using System;
using System.Threading.Tasks;
using Automatonymous;
using GreenPipes;
using MassTransit;
using Sample.Components.StateMachines;
using Sample.Contracts.Order;

namespace Sample.Components.OrderStateMachineActivities
{
    public class AcceptOrderActivity : Activity<OrderState, OrderAccepted>
    {
        public void Probe(ProbeContext context)
        {
            context.CreateScope("accept-order");
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<OrderState, OrderAccepted> context,
            Behavior<OrderState, OrderAccepted> next)
        {
            Console.WriteLine($"Hello,{context.Data.OrderId}");

            var consumeContext = context.GetPayload<ConsumeContext>();

            var sendEndpoint = await context.GetSendEndpoint(new Uri("exchange:fulfill-order"));
            await sendEndpoint.Send<FulfilOrder>(new FulfilOrder
            {
                OrderId = context.Data.OrderId,
                CustomerNumber = context.Instance.CustomerNumber,
                PaymentCardNumber = context.Instance.PaymentCardNumber
            });

            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<OrderState, OrderAccepted, TException> context,
            Behavior<OrderState, OrderAccepted> next) where TException : Exception
        {
            return next.Faulted(context);
        }
    }
}