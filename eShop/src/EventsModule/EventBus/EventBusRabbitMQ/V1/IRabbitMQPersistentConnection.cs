namespace eShop.EventsModule.EventBusRabbitMQ.V1
{
    using RabbitMQ.Client;
    using System;

    public interface IRabbitMQPersistentConnection
        : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();
    }
}