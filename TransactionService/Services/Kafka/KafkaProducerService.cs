using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;
namespace TransactionService.Services.Kafka
{
    public interface IKafkaProducerService
    {
        Task PublishTransactionAsync(object transaction);
    }

    public class KafkaProducerService : IKafkaProducerService
    {
        private readonly KafkaSettings _settings;
        private readonly IProducer<string, string> _producer;

        public KafkaProducerService(IOptions<KafkaSettings> options)
        {
            _settings = options.Value;

            var config = new ProducerConfig
            {
                BootstrapServers = _settings.BootstrapServers,
                Acks = Acks.All
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task PublishTransactionAsync(object transaction)
        {
            var json = JsonSerializer.Serialize(transaction);

            var msg = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = json
            };

            Console.WriteLine($"Producing to Kafka: {json}");

            try
            {
                var deliveryResult = await _producer.ProduceAsync(_settings.TransactionTopic, msg);
                Console.WriteLine($"Message delivered to partition {deliveryResult.Partition}, offset {deliveryResult.Offset}");
            }
            catch (ProduceException<string, string> ex)
            {
                Console.WriteLine($"Kafka produce error: {ex.Error.Reason}");
                throw;
            }
        }
    }

}
