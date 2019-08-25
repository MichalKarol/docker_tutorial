using System;
using System.Json;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using producer.Services;

namespace producer
{
    class Producer {
        private static readonly HttpClient client = new HttpClient();
        private static readonly IQueueService queue = new RabbitMQQueueService("queue");
        private const int S_TO_MS = 1000;
        private const int MINUTE_TO_MS = 60 * S_TO_MS;
        private const string QUEUE_NAME = "main";
        private const string API_URL = "http://backend/api";
        private static int sleepInterval = 1000;

        
        static void Main(string[] args)
        {
            _ = SettingsUpdater();

            while(true) {
                int sleepTime = Interlocked.CompareExchange(ref sleepInterval, 0, -1);
                Thread.Sleep(sleepTime);
                queue.Push(QUEUE_NAME, "Example message");
            }
        }

        static async Task SettingsUpdater() {
            while(true) {
                try {
                    string response = await client.GetStringAsync($"{API_URL}/settings");
                    JsonValue parsedResponse = JsonValue.Parse(response);
                    int messagesPerMinute = parsedResponse["generated"];
                    int msSleepPerCycle = MINUTE_TO_MS / messagesPerMinute;
                    Interlocked.Exchange(ref sleepInterval, msSleepPerCycle);
                } catch (Exception e) {
                    // Ignore communication issues  
                    System.Console.WriteLine(e);
                }
                Thread.Sleep(S_TO_MS);
            }
        }
    }
}
