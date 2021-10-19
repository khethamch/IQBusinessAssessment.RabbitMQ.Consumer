using IQBusinessAssessment.RabbitMQ.Consumer.Factory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;

namespace IQBusinessAssessment.RabbitMQ.Consumer
{
    static class Program
    {
        private static IServiceProvider _serviceProvider;

        static void Main(string[] args)
        {
            RegisterServices();
            IServiceScope scope = _serviceProvider.CreateScope();
            var factory = scope.ServiceProvider.GetService<IRabbitMQConnectionFactory>();
            ConnectionFactory connectionFactory = factory.GetConnectionFactory();
            ReceiveMessage(connectionFactory);
            DisposeServices();
        }

        private static void ReceiveMessage(ConnectionFactory connectionFactory)
        {
            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare("RabbitMQ-queue",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, e) =>
                {
                    var body = e.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var jsonObject = JsonConvert.DeserializeObject<dynamic>(message);

                    var responseMessage = $"Hello {jsonObject["Name"]}, I am your father!";
                    Console.WriteLine(jsonObject["Message"]);
                    Console.WriteLine(responseMessage);
                };

                channel.BasicConsume("RabbitMQ-queue", true, consumer);
                Console.ReadLine();
            }
        }

        private static void RegisterServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IRabbitMQConnectionFactory, RabbitMQConnectionFactory>();
            _serviceProvider = services.BuildServiceProvider(true);
        }

        private static void DisposeServices()
        {
            if (_serviceProvider == null)
            {
                return;
            }
            if (_serviceProvider is IDisposable)
            {
                ((IDisposable)_serviceProvider).Dispose();
            }
        }
    }
}
