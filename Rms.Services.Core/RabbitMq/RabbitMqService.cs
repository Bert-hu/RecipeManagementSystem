using log4net.Util;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Rms.Models.RabbitMq;
using System.Collections.Concurrent;
using System.Text;


namespace Rms.Services.Core.RabbitMq
{
    public class RabbitMqService
    {
        private readonly IConfiguration _configuration;

        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");


        ConnectionFactory factory;
        public IConnection connection;
        public IModel channel;

        public readonly string consumeQueue;
        public readonly string consumeSubQueue;
        public RabbitMqService(IConfiguration configuration)
        {
            _configuration = configuration;

            factory = new ConnectionFactory();
            factory.HostName = configuration.GetSection("RabbitMQ")["HostName"];
            factory.Port = int.Parse(configuration.GetSection("RabbitMQ")["Port"] ?? "5672");
            factory.UserName = configuration.GetSection("RabbitMQ")["UserName"];
            factory.Password = configuration.GetSection("RabbitMQ")["Password"];
            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            consumeQueue = _configuration.GetSection("RabbitMQ")["QueueName"];
            consumeSubQueue = consumeQueue + "." + Guid.NewGuid().ToString("N");
        }
        public void Produce(string routingKey, RabbitMqTransaction trans)
        {
            string repchannel = consumeSubQueue;
            if (trans.NeedReply == true && string.IsNullOrEmpty(trans.ReplyChannel)) trans.ReplyChannel = repchannel;
            var message = JsonConvert.SerializeObject(trans);
            Produce(routingKey, message);
        }
        public void Produce(string routingKey, string message, int ExpireSecond = 60)
        {
            var logmsg = $"RabbitMq Send message to {routingKey}: " + message;
            //如果长度超过5000，只显示前5000
            if (logmsg.Length > 5000)
            {
                logmsg = logmsg.Substring(0, 5000);
            }
            Log.Info(logmsg);
            Dictionary<string, object> arguments = new Dictionary<string, object>() { { "x-message-ttl", 300000 } };
            //channel.QueueDeclare(routingKey, false, false, true, arguments);
            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = 2;
            properties.Expiration = (ExpireSecond * 1000).ToString();
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish("", routingKey, properties, body);
        }

        public ConcurrentDictionary<string, TaskCompletionSource<RabbitMqTransaction>> waitTransactions { get; set; } = new ConcurrentDictionary<string, TaskCompletionSource<RabbitMqTransaction>>();
        public RabbitMqTransaction? ProduceWaitReply(string routingKey, RabbitMqTransaction trans)
        {
            try
            {
                trans.ReplyChannel = consumeSubQueue;
                var tcs = new TaskCompletionSource<RabbitMqTransaction>();
                var message = JsonConvert.SerializeObject(trans);
                var tid = trans.TransactionID;
                waitTransactions.TryAdd(tid, tcs);
                Produce(routingKey, message, trans.ExpireSecond);
                var completedTaskIndex = Task.WaitAny(new Task[] { tcs.Task }, trans.ExpireSecond * 1000);
                if (completedTaskIndex == 0)
                {
                    waitTransactions.TryRemove(tid, out _);
                    return tcs.Task.Result;
                }
                else
                {
                    waitTransactions.TryRemove(tid, out _);
                    Log.Warn($"Tansaction {trans.TransactionName} timeout, {trans.ExpireSecond} s");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Warn(ex);
                return null;
            }
        }

    }
}
