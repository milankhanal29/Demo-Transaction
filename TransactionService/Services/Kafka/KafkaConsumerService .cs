using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TransactionService.Models;
using TransactionService.Repository;

namespace TransactionService.Services.Kafka
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly KafkaSettings _settings;
        private readonly IServiceScopeFactory _scopeFactory;
        private IConsumer<string, string>? _consumer;

        public KafkaConsumerService(IOptions<KafkaSettings> options, IServiceScopeFactory scopeFactory)
        {
            _settings = options.Value;
            _scopeFactory = scopeFactory;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var config = new ConsumerConfig
                {
                    BootstrapServers = _settings.BootstrapServers,
                    GroupId = _settings.ConsumerGroup,
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = true
                };

                _consumer = new ConsumerBuilder<string, string>(config).Build();
                _consumer.Subscribe(_settings.TransactionTopic);

                Console.WriteLine($"Kafka consumer subscribed to topic: {_settings.TransactionTopic}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kafka consumer failed to start: {ex}");
            }

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                if (_consumer == null)
                {
                    Console.WriteLine("Kafka consumer not initialized. Exiting background task.");
                    return;
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var cr = _consumer.Consume(stoppingToken);
                        var message = JsonSerializer.Deserialize<TransactionMessageDto>(cr.Message.Value)!;

                        using var scope = _scopeFactory.CreateScope();
                        var repo = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
                        var httpFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
                        var client = httpFactory.CreateClient("UserService");

                        // Parallel HTTP call
                        await Parallel.ForEachAsync(new[] { message }, new ParallelOptions
                        {
                            MaxDegreeOfParallelism = 20,
                            CancellationToken = stoppingToken
                        },
                        async (msg, token) =>
                        {
                            var payload = new
                            {
                                MerchantAccountNumber = msg.MerchantAccountNumber,
                                Transfers = new[] { msg }
                            };
                            await client.PostAsJsonAsync("/api/users/transfer", payload, token);
                        });

                        // DB insert
                        var tx = new Transaction
                        {
                            FromUserId = message.MerchantId,
                            ToUserId = message.RecipientId,
                            Amount = message.Amount,
                            Timestamp = message.Timestamp,
                            Status = "Success",
                            Remark = "Processed via Kafka"
                        };

                        await repo.AddAsync(tx);
                        await repo.SaveChangesAsync();

                        Console.WriteLine($"Processed transaction for Merchant: {message.MerchantAccountNumber}");
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Kafka consumer stopping...");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Kafka processing error: {ex}");
                        await Task.Delay(1000, stoppingToken);
                    }
                }
            }, stoppingToken);
        }

        public override void Dispose()
        {
            _consumer?.Close();
            _consumer?.Dispose();
            base.Dispose();
        }
    }
}
