using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using NUnit.Framework;
using Sample.Components.Consumers;
using Sample.Contracts.Order;

namespace Sample.Components.Tests
{
    [TestFixture]
    public class WhenAnOrderRequestIsConsumed
    {
        private InMemoryTestHarness _harness;
        private ConsumerTestHarness<SubmitOrderConsumer> _consumer;

        [SetUp]
        public async Task SetUp()
        {
            _harness = new InMemoryTestHarness {TestTimeout = TimeSpan.FromSeconds(5)};
            _consumer = _harness.Consumer<SubmitOrderConsumer>();
            await _harness.Start();
        }

        [Test]
        public async Task Should_Respond_With_Rejected_If_Test()
        {
            //assert
            var orderId = NewId.NextGuid();
            var customerNumber = "TEST123";
            var requestClient = await _harness.ConnectRequestClient<SubmitOrder>();
            //act
            var response = await requestClient.GetResponse<OrderSubmissionRejected>(new OrderSubmissionRejected()
            {
                OrderId = orderId,
                Timestamp = InVar.Timestamp,
                CustomerNumber = customerNumber
            });
            await _harness.InputQueueSendEndpoint.Send<SubmitOrder>(new SubmitOrder
            {
                OrderId = orderId,
                Timestamp = InVar.Timestamp,
                CustomerNumber = customerNumber
            });
            //assert
            response.Message.OrderId.Should().Be(orderId);
            
            _consumer.Consumed.Select<SubmitOrder>().Any().Should().BeTrue();
            _harness.Sent.Select<OrderSubmissionRejected>().Any().Should().BeTrue();
        }

        [Test]
        public async Task Should_Respond_With_Acceptance_If_Ok()
        {
            //assert
            var orderId = NewId.NextGuid();
            var customerNumber = "12345";
            var requestClient = await _harness.ConnectRequestClient<SubmitOrder>();
            //act
            var response = await requestClient.GetResponse<OrderSubmissionAccepted>(new OrderSubmissionAccepted
            {
                OrderId = orderId,
                Timestamp = InVar.Timestamp,
                CustomerNumber = customerNumber
            });
            await _harness.InputQueueSendEndpoint.Send<SubmitOrder>(new SubmitOrder
            {
                OrderId = orderId,
                Timestamp = InVar.Timestamp,
                CustomerNumber = customerNumber
            });
            //assert
            response.Message.OrderId.Should().Be(orderId);
            
            _consumer.Consumed.Select<SubmitOrder>().Any().Should().BeTrue();
            _harness.Sent.Select<OrderSubmissionAccepted>().Any().Should().BeTrue();
        }

        [Test]
        public async Task Should_Consume_Submit_Order_Commands()
        {
            //assert
            var orderId = NewId.NextGuid();
            //act
            await _harness.InputQueueSendEndpoint.Send<SubmitOrder>(new SubmitOrder
            {
                OrderId = orderId,
                Timestamp = InVar.Timestamp,
                CustomerNumber = "12345"
            });
            //assert
            _consumer.Consumed.Select<SubmitOrder>().Any().Should().BeTrue();
            
            _harness.Sent.Select<OrderSubmissionAccepted>().Any().Should().BeFalse();
            _harness.Sent.Select<OrderSubmissionRejected>().Any().Should().BeFalse();
        }

        [Test]
        public async Task Should_Publish_Order_Submitted_Event()
        {
            //assert
            var orderId = NewId.NextGuid();
            //act
            await _harness.InputQueueSendEndpoint.Send<SubmitOrder>(new SubmitOrder
            {
                OrderId = orderId,
                Timestamp = InVar.Timestamp,
                CustomerNumber = "12345"
            });
            //assert
            _harness.Published.Select<OrderSubmitted>().Any().Should().BeTrue();
        }
        
        [Test]
        public async Task Should_Not_Publish_Order_Submitted_Event_When_Rejected()
        {
            //assert
            var orderId = NewId.NextGuid();
            var customerNumber = "TEST123";
            //act
            await _harness.InputQueueSendEndpoint.Send<SubmitOrder>(new SubmitOrder
            {
                OrderId = orderId,
                Timestamp = InVar.Timestamp,
                CustomerNumber = customerNumber
            });
            //assert
            _consumer.Consumed.Select<SubmitOrder>().Any().Should().BeTrue();
            _harness.Published.Select<OrderSubmitted>().Any().Should().BeFalse();
        }
        
        [TearDown]
        public async Task TearDown()
        {
            await _harness.Stop();
        }
    }
}