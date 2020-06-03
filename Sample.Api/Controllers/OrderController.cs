using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.MessageData.Values;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sample.Api.Models;
using Sample.Contracts.Order;

namespace Sample.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IRequestClient<SubmitOrder> _submitOrderRequestClient;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly IRequestClient<CheckOrder> _checkOrderRequestClient;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderController(ILogger<OrderController> logger,
            IRequestClient<SubmitOrder> submitOrderRequestClient,
            ISendEndpointProvider sendEndpointProvider,
            IRequestClient<CheckOrder> checkOrderRequestClient,
            IPublishEndpoint publishEndpoint
        )
        {
            _logger = logger;
            _submitOrderRequestClient = submitOrderRequestClient;
            _sendEndpointProvider = sendEndpointProvider;
            _checkOrderRequestClient = checkOrderRequestClient;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<IActionResult> Get(Guid id)
        {
            var (status, notFound) = await _checkOrderRequestClient.GetResponse<OrderStatus, OrderNotFound>(
                new CheckOrder
                {
                    OrderId = id
                });

            if (status.IsCompletedSuccessfully)
            {
                var response = await status;
                return Ok(response.Message);
            }
            else
            {
                var response = await notFound;
                return NotFound(response.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OrderViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // IMessageDataRepository repository;

            var (accepted, rejected) =
                await _submitOrderRequestClient.GetResponse<OrderSubmissionAccepted, OrderSubmissionRejected>(
                    new SubmitOrder
                    {
                        OrderId = viewModel.Id,
                        Timestamp = InVar.Timestamp,
                        CustomerNumber = viewModel.CustomerNumber,
                        PaymentCardNumber = viewModel.PaymentCardNumber,
                        Notes = new StringInlineMessageData(viewModel.Notes)
                    });
            if (accepted.IsCompletedSuccessfully)
            {
                var response = await accepted;
                return Accepted(response.Message);
            }
            else
            {
                var response = await rejected;
                return BadRequest(response.Message);
            }
        }

        [HttpPatch]
        public async Task<IActionResult> Patch(Guid id)
        {
            await _publishEndpoint.Publish<OrderAccepted>(new OrderAccepted
            {
                OrderId = id,
                Timestamp = InVar.Timestamp
            });

            return Accepted();
        }

        [HttpPut]
        public async Task<IActionResult> Put(Guid id, string customerNumber)
        {
            var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:submit-order"));
            await endpoint.Send<SubmitOrder>(new SubmitOrder
            {
                OrderId = id,
                Timestamp = InVar.Timestamp,
                CustomerNumber = customerNumber
            });

            return Accepted();
        }
    }
}