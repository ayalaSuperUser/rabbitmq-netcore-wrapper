using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitTester.Domain.Bus;
using RabbitTester.Domain.Channel;
using RabbitTester.Domain.Common;
using RabbitTester.Domain.DTO;
using RabbitTester.Domain.Events;
using System.Text;

namespace RabbitTester.Bus
{
    public class RabbitMQBusManager : IEventBus
    {
        protected IChannel _channel;
        protected IServiceProvider _serviceProvider;
        protected Dictionary<string, List<Type>> _handlers;
        protected Dictionary<string, Type> _eventTypes;
        protected IHttpContextAccessor _httpContextAccessor;

        public RabbitMQBusManager(IServiceProvider serviceProvider,
          IChannel channel, IHttpContextAccessor httpContextAccessor)
        {
            _channel = channel;
            _serviceProvider = serviceProvider;
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new Dictionary<string, Type>();
            _httpContextAccessor = httpContextAccessor;
        }

        public RabbitMQBusManager(string serviceKey, IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
           : this(serviceProvider, serviceProvider.GetRequiredKeyedService<IChannel>(serviceKey), httpContextAccessor)
        {
        }

        public Task Publish<T>(T @event, string queueName, QueueType queueType = QueueType.Quorum, bool durable = true, IBasicProperties basicProperties = null, Dictionary<string, object> queueArgs = null) where T : Event
        {
            var eventName = queueName;

            _channel.Model.QueueDeclare(
                eventName, durable, exclusive: false, autoDelete: false, AddQueueArguments(queueType, queueArgs));

            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.Model.BasicPublish("", eventName, basicProperties, body);

            return Task.CompletedTask;
        }

        public Task Publish<T>(PublishExchangeRequest<T> publishExchangeRequest) where T : Event
        {
            _channel.Model.ExchangeDeclare(publishExchangeRequest.ExchangeName, publishExchangeRequest.ExchangeType.ToString().ToLower(),
                durable: publishExchangeRequest.Durable);

            var message = JsonConvert.SerializeObject(publishExchangeRequest.@event);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.Model.BasicPublish(publishExchangeRequest.ExchangeName, publishExchangeRequest.ExchangeRoutingKey, false, publishExchangeRequest.BasicProperties, body);

            return Task.CompletedTask;
        }

        public ConsumerResponse Subscribe<T, TH>(string queueName, QueueType queueType = RabbitMQConstants.QUEUE_TYPE, bool autoAck = false, bool durable = true, bool requeue = false, Dictionary<string, object> queueArgs = null, ushort prefertchCount = RabbitMQConstants.PREFETCH_COUNT)
            where T : Event
            where TH : IEventHandler<T>
        {
            queueArgs = AddQueueArguments(queueType, queueArgs);
            _channel.Model.QueueDeclare(queueName, durable, false, false, queueArgs);

            var consumerTag = AddConsumer<T, TH>(queueName, autoAck, requeue, prefertchCount);

            return new ConsumerResponse() { ConsumerTag = consumerTag };
        }

        public ConsumerResponse Subscribe<T, TH>(ConsumeExchangeRequest<T> consumeExchangeRequest)
          where T : Event
          where TH : IEventHandler<T>
        {
            _channel.Model.ExchangeDeclare(consumeExchangeRequest.ExchangeName, consumeExchangeRequest.ExchangeType.ToString().ToLower(),
                durable: consumeExchangeRequest.Durable);

            AddQueue(consumeExchangeRequest);

            var consumerTag = AddConsumer<T, TH>(consumeExchangeRequest.QueueName, consumeExchangeRequest.AutoAck, consumeExchangeRequest.PrefetchCount);

            return new ConsumerResponse() { ConsumerTag = consumerTag };
        }

        public void Unsubscribe<TH>(string consumerTag)
        {
            _channel.Model.BasicCancel(consumerTag);

            RemoveEventHandler<TH>(consumerTag);
        }

        private string AddConsumer<T, TH>(string queueName, bool autoAck, ushort prefetchCount)
       where T : Event where TH : IEventHandler<T>
        {
            var consumer = new AsyncEventingBasicConsumer(_channel.Model);

            consumer.Received += async (ch, ea) =>
            {
                try
                {
                    await Consumer_Received(ch, ea);
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());

                    if (!autoAck)
                    {
                        MessageAck(ea);
                    }
                }
                catch (Exception exception)
                {
                    if (!autoAck)
                    {
                        MessageNack(ea);
                    }
                }
            };

            return BasicConsume<T, TH>(queueName, autoAck, consumer, prefetchCount);
        }

        private string AddConsumer<T, TH>(string queueName, bool autoAck, bool requeue, ushort prefetchCount)
        where T : Event where TH : IEventHandler<T>
        {
            var consumer = new AsyncEventingBasicConsumer(_channel.Model);

            consumer.Received += async (ch, ea) =>
            {
                try
                {
                    await Consumer_Received(ch, ea);
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());

                    if (!autoAck)
                    {
                        MessageAck(ea);
                    }
                }
                catch (Exception exception)
                {
                    if (!autoAck)
                    {
                        MessageNack(ea);
                    }
                }
            };

            return BasicConsume<T, TH>(queueName, autoAck, consumer, prefetchCount);
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var eventName = e.ConsumerTag;

            var message = Encoding.UTF8.GetString(e.Body.ToArray());

            try
            {
                var basicProperties = e.BasicProperties;
                await ProcessEvent(eventName, message, basicProperties).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task ProcessEvent(string eventName, string message, IBasicProperties basicProperties)
        {
            if (_handlers.ContainsKey(eventName))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var subscriptions = _handlers[eventName];
                    foreach (var subscription in subscriptions)
                    {
                        var handler = scope.ServiceProvider.GetService(subscription);
                        if (handler == null)
                        {
                            continue;
                        }

                        var eventType = _eventTypes.SingleOrDefault(t => t.Key == eventName);
                        var @event = JsonConvert.DeserializeObject(message, eventType.Value);

                        var conreteType = typeof(IEventHandler<>).MakeGenericType(eventType.Value);
                        await (Task)conreteType.GetMethod("Handle").Invoke(handler, new object[] { @event });
                    }
                }
            }
        }

        private void MessageAck(BasicDeliverEventArgs ea)
        {
            _channel.Model.BasicAck(ea.DeliveryTag, false);
        }

        private void MessageNack(BasicDeliverEventArgs ea, bool requeue = false)
        {
            _channel.Model.BasicNack(ea.DeliveryTag, false, requeue);
        }

        private string BasicConsume<T, TH>(string queueName, bool autoAck, AsyncEventingBasicConsumer consumer, ushort prefetchCount)
        where T : Event
        where TH : IEventHandler<T>
        {
            _channel.Model.BasicQos(0, prefetchCount, false);
            var consumerTag = _channel.Model.BasicConsume(queueName, autoAck, consumer);
            AddEventHandler<T, TH>(consumerTag);
            return consumerTag;
        }

        private void AddEventHandler<T, TH>(string eventName)
        {
            var handlerType = typeof(TH);
            if (!_eventTypes.ContainsKey(eventName))
            {
                _eventTypes.Add(eventName, typeof(T));
            }

            if (!_handlers.ContainsKey(eventName))
            {
                _handlers.Add(eventName, new List<Type>());
            }

            if (_handlers[eventName].Any(s => s.GetType() == handlerType))
            {
                throw new ArgumentException(
                    $"Handler Type {handlerType.Name} already is registered for '{eventName}'", nameof(handlerType));
            }

            _handlers[eventName].Add(handlerType);
        }

        private void AddQueue<T>(ConsumeExchangeRequest<T> consumeExchangeRequest) where T : Event
        {
            _channel.Model.QueueDeclare(consumeExchangeRequest.QueueName, consumeExchangeRequest.DurableQueue, false, false, AddQueueType(consumeExchangeRequest.QueueType, consumeExchangeRequest.QueueArgs));

            foreach (var routingKey in consumeExchangeRequest.ExchangeRoutingKeys)
            {
                _channel.Model.QueueBind(consumeExchangeRequest.QueueName, consumeExchangeRequest.ExchangeName, routingKey, null);
            }
        }

        private Dictionary<string, object> AddQueueArguments(QueueType queueType, Dictionary<string, object> queueArgs)
        {
            queueArgs = AddQueueType(queueType, queueArgs) as Dictionary<string, object>;
            queueArgs = AddDeliveryLimit(queueType, queueArgs) as Dictionary<string, object>;
            return queueArgs;
        }

        private IDictionary<string, object> AddQueueType(QueueType queueType, Dictionary<string, object> queueArgs = null)
        {
            if (queueArgs == null)
            {
                queueArgs = new Dictionary<string, object>();
            }
            if (!queueArgs.ContainsKey("x-queue-type"))
            {
                queueArgs.Add("x-queue-type", queueType.ToString().ToLower());
            }

            return queueArgs;
        }

        private IDictionary<string, object> AddDeliveryLimit(QueueType queueType, IDictionary<string, object> queueArgs = null)
        {
            if (queueType == QueueType.Classic)
            {
                return queueArgs;
            }

            if (queueArgs == null)
            {
                queueArgs = new Dictionary<string, object>();
            }
            if (!queueArgs.ContainsKey("x-delivery-limit"))
            {
                queueArgs.Add("x-delivery-limit", RabbitMQConstants.QUEUE_DELIVERY_COUNT);
            }

            return queueArgs;
        }

        private void RemoveEventHandler<TH>(string eventName)
        {
            if (_eventTypes.ContainsKey(eventName))
            {
                _eventTypes.Remove(eventName);
            }

            if (_handlers.ContainsKey(eventName))
            {
                _handlers.Remove(eventName);
            }
        }

        public void Dispose()
        {
            _channel.Model.Dispose();
            _channel.Model.Dispose();
        }
    }
}
