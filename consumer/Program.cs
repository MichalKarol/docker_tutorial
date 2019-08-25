using System;
using System.Collections.Generic;
using System.Json;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using producer.Services;

namespace consumer
{
    class Program {
        static readonly HttpClient client = new HttpClient();
        private static readonly IQueueService queue = new RabbitMQQueueService("queue");
        private const int S_TO_MS = 1000;
        private const int MINUTE_TO_S = 60;
        private const int MINUTE_TO_MS = MINUTE_TO_S * S_TO_MS;
        private const int LOAD_SENSING_INTERVAL_IN_S = 10;
        private const string QUEUE_NAME = "main";
        private const string API_URL = "http://backend/api";
        private static string hostname = System.Environment.MachineName;

        private static int sleepInterval = 1000;
        private static int load = 0;
        
        static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += async (s, e) => 
            {
                await DeleteWorker();
            };

            await RegisterWorker();
            _ = BackroundWorker();


            queue.RegisterConsumer(QUEUE_NAME, (string body) => {
                Interlocked.Exchange(ref load, 1);
                int sleepTime = Interlocked.CompareExchange(ref sleepInterval, 0, -1);
                Thread.Sleep(sleepTime);
                Interlocked.Exchange(ref load, 0);
            });
        }

        static async Task RegisterWorker() {
            var payload = new JsonObject(new KeyValuePair<string, JsonValue>[] {
                new KeyValuePair<string, JsonValue>("name", new JsonPrimitive(hostname)),
            });

            // Making sure that worker will be registered as soon as communication will be ok
            while(true) {
                try {
                    await client.PostAsync($"{API_URL}/workers", new StringContent(
                        payload.ToString(), Encoding.UTF8, "application/json"
                    ));
                    break;
                } catch (Exception e) {
                    // Ignore communication issues  
                    System.Console.WriteLine(e);
                    Thread.Sleep(S_TO_MS);
                }
            }
        }

        static async Task DeleteWorker() {
            // Making sure that worker will be removed as soon as communication will be ok
            while(true) {
                try {
                    await client.DeleteAsync($"{API_URL}/workers/{hostname}");
                    break;
                } catch (Exception e) {
                    // Ignore communication issues  
                    System.Console.WriteLine(e);
                    Thread.Sleep(S_TO_MS);
                }
            }
        }

        static async Task BackroundWorker() {
            while(true) {
                try {
                    string response = await client.GetStringAsync($"{API_URL}/settings");
                    JsonValue parsedResponse = JsonValue.Parse(response);
                    int messagesPerMinute = parsedResponse["consumed"];
                    int msSleepPerCycle = MINUTE_TO_MS / messagesPerMinute;
                    Interlocked.Exchange(ref sleepInterval, msSleepPerCycle);
                } catch (Exception e) {
                    // Ignore communication issues  
                    System.Console.WriteLine(e);
                }
                

                int accumulatedLoad = LoadCounting();
                int loadAsPercent = accumulatedLoad * 100 / (MINUTE_TO_S / LOAD_SENSING_INTERVAL_IN_S);
                var loadPayload = new JsonObject(new KeyValuePair<string, JsonValue>[] {
                    new KeyValuePair<string, JsonValue>("load", new JsonPrimitive(loadAsPercent)),
                });

                try {
                    await client.PutAsync($"{API_URL}/workers/{hostname}", new StringContent(
                        loadPayload.ToString(), Encoding.UTF8, "application/json"
                    ));
                } catch (Exception e) {
                    // Ignore communication issues  
                    System.Console.WriteLine(e);
                }
            }
        }
        static int LoadCounting() {
            int accumulator = 0 ;
            for(int i = 0; i < (MINUTE_TO_S / LOAD_SENSING_INTERVAL_IN_S); i++) {
                if (Interlocked.CompareExchange(ref load, 0, -1) == 1) {
                    accumulator++;
                }
                Thread.Sleep(S_TO_MS);
            }
            return accumulator;
        }
    }
}
