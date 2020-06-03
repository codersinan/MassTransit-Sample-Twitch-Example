using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using NUnit.Framework;
using Sample.Components.StateMachines;
using Sample.Contracts.Customer;
using Sample.Contracts.Order;

namespace Sample.Components.Tests
{
    [TestFixture]
    public class SubmittingAnOrder
    {
        private InMemoryTestHarness _harness;
        private StateMachineSagaTestHarness<OrderState, OrderStateMachine> _saga;
        private OrderStateMachine _orderStateMachine;

        [SetUp]
        public async Task SetUp()
        {
            _harness = new InMemoryTestHarness {TestTimeout = TimeSpan.FromSeconds(5)};
            _orderStateMachine = new OrderStateMachine();
            _saga = _harness.StateMachineSaga<OrderState, OrderStateMachine>(_orderStateMachine);
            await _harness.Start();
        }

        [Test]
        public async Task Should_Create_A_State_Instance()
        {
            //arrange
            var orderId = NewId.NextGuid();
            var customerNumber = "12345";
            //act
            await _harness.Bus.Publish<OrderSubmitted>(new OrderSubmitted
            {
                OrderId = orderId,
                Timestamp = InVar.Timestamp,
                CustomerNumber = customerNumber
            });
            //assert
            _saga.Created.Select(x => x.CorrelationId == orderId).Any().Should().BeTrue();

            var instanceId = await _saga.Exists(orderId, x => x.Submitted);
            instanceId.Should().NotBeNull();

            var instance = _saga.Sagas.Contains(instanceId.Value);
            instance.CustomerNumber.Should().Be(customerNumber);
        }

        [Test]
        public async Task Should_Respond_To_Status_Checks()
        {
            //arrange
            var orderId = NewId.NextGuid();
            var customerNumber = "12345";
            //act
            await _harness.Bus.Publish<OrderSubmitted>(new OrderSubmitted
            {
                OrderId = orderId,
                Timestamp = InVar.Timestamp,
                CustomerNumber = customerNumber
            });
            //assert
            _saga.Created.Select(x => x.CorrelationId == orderId).Any().Should().BeTrue();

            var instanceId = await _saga.Exists(orderId, x => x.Submitted);
            instanceId.Should().NotBeNull();

            var requestClient = await _harness.ConnectRequestClient<CheckOrder>();
            var response = await requestClient.GetResponse<OrderStatus>(new OrderStatus
            {
                OrderId = orderId
            });

            response.Message.State.Should().Be(_orderStateMachine.Submitted.Name);
        }

        [Test]
        public async Task Should_Respond_To_Status_Checks_When_NotFound()
        {
            //arrange
            var orderId = NewId.NextGuid();
            //act
            var requestClient = await _harness.ConnectRequestClient<CheckOrder>();
            var response = await requestClient.GetResponse<OrderNotFound>(new OrderNotFound()
            {
                OrderId = orderId
            });
            //assert
            response.Message.OrderId.Should().Be(orderId);
        }

        [Test]
        public async Task Should_Cancel_When_Customer_Account_Closed()
        {
            //arrange
            var orderId = NewId.NextGuid();
            var customerNumber = "12345";
            //act
            await _harness.Bus.Publish<OrderSubmitted>(new OrderSubmitted
            {
                OrderId = orderId,
                Timestamp = InVar.Timestamp,
                CustomerNumber = customerNumber
            });
            //assert
            _saga.Created.Select(x => x.CorrelationId == orderId).Any().Should().BeTrue();

            var instanceId = await _saga.Exists(orderId, x => x.Submitted);
            instanceId.Should().NotBeNull();

            await _harness.Bus.Publish<CustomerAccountClosed>(new CustomerAccountClosed
            {
                CustomerId = InVar.Id,
                CustomerNumber = customerNumber,
            });

            instanceId = await _saga.Exists(orderId, x => x.Cancelled);
            instanceId.Should().NotBeNull();
        }

        [Test]
        public async Task Should_Accept_When_Order_Is_Accepted()
        {
            //arrange
            var orderId = NewId.NextGuid();
            var customerNumber = "12345";
            //act
            await _harness.Bus.Publish<OrderSubmitted>(new OrderSubmitted
            {
                OrderId = orderId,
                Timestamp = InVar.Timestamp,
                CustomerNumber = customerNumber
            });
            //assert
            _saga.Created.Select(x => x.CorrelationId == orderId).Any().Should().BeTrue();

            var instanceId = await _saga.Exists(orderId, x => x.Submitted);
            instanceId.Should().NotBeNull();

            await _harness.Bus.Publish<OrderAccepted>(new OrderAccepted
            {
                OrderId = orderId,
                Timestamp = InVar.Timestamp
            });

            instanceId = await _saga.Exists(orderId, x => x.Accepted);
            instanceId.Should().NotBeNull();
        }
        
        [TearDown]
        public async Task TearDown()
        {
            await _harness.Stop();
        }
    }
}