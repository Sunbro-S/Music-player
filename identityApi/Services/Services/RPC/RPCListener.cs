using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Services.Services.Interfaces;

namespace RPC;

public class RPCListener : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    public RPCListener(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        var factory = new ConnectionFactory { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

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

    }
    

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Создаем консьюмера для обработки сообщений из "musicToAdd"
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += Consume;
        // Запускаем консьюмера
        _channel.BasicConsume(queue: "musicToAdd", autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }
    
    public bool SendMessage(object obj)
    {
        var message = JsonSerializer.Serialize(obj);
        var body = Encoding.UTF8.GetBytes(message);

        // Отправка сообщения в очередь "musicToAdd"
        _channel.BasicPublish(exchange: "",
            routingKey: "musicResults",
            basicProperties: null,
            body: body);

        return true; // Предположим, что отправка прошла успешно
    }

    private void Consume(object sender, BasicDeliverEventArgs ea)
    {
        
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        var music = JsonSerializer.Deserialize<Music>(message);
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            try
            {
                var result = authService.AddFavorite(music);
                if(result.Result)
                    SendMessage(true);
                else
                    SendMessage(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            
        }
        _channel.BasicAck(ea.DeliveryTag,false);
    }
    // private ResultMessage ProcessMusic(Music music)
    // {
    //     // Ваш код обработки данных
    //     // Возвращаем результат
    //     return new ResultMessage
    //     {
    //         MessageId = "Processed music: " + music.Title,
    //         Result = true
    //     };
    // }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}
