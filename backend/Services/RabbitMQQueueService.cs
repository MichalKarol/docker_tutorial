using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Collections.Generic;

namespace backend.Services {
    public class RabbitMQQueueService : IQueueService, IDisposable
    {
        private IConnection _connection;
        private IModel _channel;

        private ISet<string> queueNameSet = new HashSet<string>();
        
        public RabbitMQQueueService(string hostname, uint prefetchSize = 0 , ushort prefetchCount = 1) {
            var factory = new ConnectionFactory() { HostName = hostname };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.BasicQos(prefetchSize, prefetchCount, global: false);
        }

        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }

        public void Push(string queueName, string message)
        {
            bool isAlreadyDeclared = queueNameSet.Contains(queueName);
            if (!isAlreadyDeclared) {
                _channel.QueueDeclare(queue: queueName,
                                      durable: false,
                                      exclusive: false,
                                      autoDelete: false,
                                      arguments: null);
            }
            
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "",
                                  routingKey: queueName,
                                  basicProperties: null,
                                  body: body);
        }

        public void RegisterConsumer(string queueName, Action<string> consumer)
        {
            var eventConsumer = new EventingBasicConsumer(_channel);
            eventConsumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                consumer(message);
                _channel.BasicAck(ea.DeliveryTag, false);
            };
            _channel.BasicConsume(queue: queueName,
                                  autoAck: false,
                                  consumer: eventConsumer);
        }

        public uint Size(string queueName)
        {
            QueueDeclareOk queue = _channel.QueueDeclare(queue: queueName,
                                                         durable: false,
                                                         exclusive: false,
                                                         autoDelete: false,
                                                         arguments: null);
            return queue.MessageCount;
        }
    }
}