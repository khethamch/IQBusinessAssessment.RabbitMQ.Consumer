using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Fakes;
using System.Linq;
using System.Text;

namespace IQBusinessAssessment.RabbitMQ.Consumer.Test
{
    [TestFixture]
    public class ReceiveMessageTest
    {
        [Test]
        public void ReceiveMessagesOnQueue()
        {
            var rabbitServer = new RabbitServer();

            ConfigureQueueBinding(rabbitServer, "RabbitMQ-exchange", "RabbitMQ-queue");
            SendMessage(rabbitServer, "RabbitMQ-exchange", "I love IQ Business");

            var connectionFactory = new FakeConnectionFactory(rabbitServer);
            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var message = channel.BasicGet("RabbitMQ-queue", autoAck: false);

                Assert.That(message, Is.Not.Null);
                var messageBody = Encoding.ASCII.GetString(message.Body.ToArray());

                Assert.That(messageBody, Is.EqualTo("I love IQ Business"));

                channel.BasicAck(message.DeliveryTag, multiple: false);
            }

        }

        private static void SendMessage(RabbitServer rabbitServer, string exchange, string message, IBasicProperties basicProperties = null)
        {
            var connectionFactory = new FakeConnectionFactory(rabbitServer);

            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var messageBody = Encoding.ASCII.GetBytes(message);
                channel.BasicPublish(exchange: exchange, routingKey: null, mandatory: false, basicProperties: basicProperties, body: messageBody);
            }
        }

        private void ConfigureQueueBinding(RabbitServer rabbitServer, string exchangeName, string queueName)
        {
            var connectionFactory = new FakeConnectionFactory(rabbitServer);
            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);

                channel.QueueBind(queueName, exchangeName, null);
            }
        }
    }
}
