using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Domain;
using RPC.Interface;

namespace RPC
{
    public class RPCMusicToAddSender : IRabbitMqServiceSender, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _replyQueueName;
        private readonly EventingBasicConsumer _consumer;
        private TaskCompletionSource<string> _tcs;
        private readonly object _lock = new object();

        public RPCMusicToAddSender()
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Объявляем очереди
            _channel.QueueDeclare(queue: "musicToAdd",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.QueueDeclare(queue: "musicResults",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Устанавливаем консьюмера для очереди "musicResults"
            _replyQueueName = "musicResults";
            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += OnResultReceived;

            _channel.BasicConsume(queue: _replyQueueName,
                autoAck: true,
                consumer: _consumer);
        }

        public async Task<string> SendMessage(object obj)
        {
            var message = JsonSerializer.Serialize(obj);
            var body = Encoding.UTF8.GetBytes(message);

            // Создаем уникальный идентификатор запроса
            var correlationId = Guid.NewGuid().ToString();
            
            // Настраиваем свойства сообщения
            var properties = _channel.CreateBasicProperties();
            properties.CorrelationId = correlationId;
            properties.ReplyTo = _replyQueueName;

            // Сбросим TaskCompletionSource при отправке нового сообщения
            lock (_lock)
            {
                _tcs = new TaskCompletionSource<string>();
            }

            // Отправка сообщения в очередь "musicToAdd"
            _channel.BasicPublish(exchange: "",
                routingKey: "musicToAdd",
                basicProperties: properties,
                body: body);

            // Устанавливаем таймер ожидания ответа
            return await Task.WhenAny(_tcs.Task, Task.Delay(5000)) == _tcs.Task
                ? await _tcs.Task
                : throw new TimeoutException("The operation has timed out.");
        }

        private void OnResultReceived(object sender, BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            var result = Encoding.UTF8.GetString(body);
            var props = ea.BasicProperties;

            lock (_lock)
            {
                // Проверяем, соответствует ли CorrelationId
                if (props.CorrelationId == _tcs?.Task?.ToString())
                {
                    _tcs?.TrySetResult(result);
                }
            }
        }

        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}
