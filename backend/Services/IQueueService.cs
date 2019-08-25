using System;

namespace backend.Services
{
    public interface IQueueService
    {
        void Push(string queueName, string message);
        void RegisterConsumer(string queueName, Action<string> consumer);
        uint Size(string queueName);
    } 
}