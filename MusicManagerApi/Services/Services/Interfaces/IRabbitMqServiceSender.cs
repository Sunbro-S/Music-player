namespace RPC.Interface;

public interface IRabbitMqServiceSender
{
    Task<string> SendMessage(object obj);
}