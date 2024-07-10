using System;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace webapi.Kafka
{
    public class KafkaClient
    {
        private string my_bootstrap_servers; // = "localhost:9092";
        private ClientConfig my_client_config;
        private KafkaClientType my_kafka_client_type;

        public KafkaClient(string bootstrap_servers, KafkaClientType kafka_client_type)
        {
            my_bootstrap_servers = bootstrap_servers;
            my_kafka_client_type = kafka_client_type;

            if (kafka_client_type == KafkaClientType.Consumer)
            {
                my_client_config = new ConsumerConfig
                {
                    GroupId = "test-consumer-group",
                    BootstrapServers = my_bootstrap_servers,
                };
            }
            else if (kafka_client_type == KafkaClientType.Producer)
            {
                my_client_config = new ProducerConfig
                {
                    BootstrapServers = my_bootstrap_servers,
                };
            }
            else
            {
                throw new Exception("Kafka client: unexpected kafka client type");
            }
        }

        public enum  KafkaClientType
        {
            Producer,
            Consumer
        }

        public string GetMesssage(string topic = "microservice-actions")
        {
            if (!(my_client_config is ConsumerConfig))
            {
                throw new Exception("Kafka client: client_config is not ConsumerConfig");
            }

            string got_value = "";

            using (var c = new ConsumerBuilder<Ignore, string>(my_client_config).Build())
            {
                c.Subscribe(topic);

                CancellationTokenSource cts = new CancellationTokenSource();
                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true; // prevent the process from terminating.
                    cts.Cancel();
                };
                try
                {
                    while (true)
                    {
                        try
                        {
                            var cr = c.Consume(cts.Token);

                            got_value = cr.Value;
                        }
                        catch (ConsumeException e)
                        {
                            Console.WriteLine($"Error occurred: {e.Error.Reason}");

                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    c.Close();
                }
            }

            return got_value;
        }

        public async void SendMesssage(string topic = "microservice-actions", string message = "")
        {
            if (!(my_client_config is ProducerConfig))
            {
                throw new Exception("Kafka client: client_config is not ProducerConfig");
            }

            using (var p = new ProducerBuilder<Null, string>(my_client_config).Build())
            {
                try
                {
                    var dr = await p.ProduceAsync(topic, new Message<Null, string> { Value = message });
                }
                catch (ProduceException<Null, string> e)
                {
                    Console.WriteLine($"Delivery failed: {e.Error.Reason}");
                }
            }
        }
    }
}
