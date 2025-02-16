using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RabbitTester.Domain.Bus;
using RabbitTester.Domain.DTO;
using RabbitTester.Tester;

namespace RabbitTester.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class RabbitMqTesterController(IEventBus bus) : ControllerBase
    {
        [HttpGet("publish")]
        public async Task<ActionResult> Publish()
        {
            var data = new Data() { Id = 2 };
            var rabbitPublishExchangeRequest = new PublishExchangeRequest<Data>()
            {
                ExchangeName = TesterConstants.ExchangeName,
                ExchangeType = TesterConstants.Exchange_Type,
                ExchangeRoutingKey = $"{TesterConstants.ExchangeName}.{data.Id}",
                @event = data
            };

            await bus.Publish(rabbitPublishExchangeRequest);
            return Ok();
        }

        [HttpGet("subscribe")]
        public ActionResult<string> Subscribe()
        {
            var rabbitConsumeExchangeRequest = new ConsumeExchangeRequest<Data>()
            {
                ExchangeName = TesterConstants.ExchangeName,
                ExchangeType = TesterConstants.Exchange_Type,
                QueueName = TesterConstants.QueueName1,
                ExchangeRoutingKeys = Enumerable.Range(1, 10).Select(n => $"{TesterConstants.ExchangeName}.{n}").ToList()
            };

            var response = bus.Subscribe<Data, DataHandler>(rabbitConsumeExchangeRequest);
            return Ok(response.ConsumerTag);
        }
    }
}
