namespace TransactionService.Services.Kafka
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; } = string.Empty;
        public string TransactionTopic { get; set; } = string.Empty;
        public string ConsumerGroup { get; set; } = string.Empty;
    }

}
