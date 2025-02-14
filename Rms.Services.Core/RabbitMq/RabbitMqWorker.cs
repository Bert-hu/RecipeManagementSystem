using log4net;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Rms.Models.RabbitMq;
using SqlSugar;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace Rms.Services.Core.RabbitMq
{
    internal class RabbitMqWorker : BackgroundService
    {

        //private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");
        private static log4net.ILog RabbitMqLog = log4net.LogManager.GetLogger("RabbitMQ");

        private readonly IConfiguration _configuration;
        private readonly RabbitMqService _rabbitMqService;
        private readonly ISqlSugarClient _sqlSugarClient;

        public RabbitMqWorker(IConfiguration configuration, RabbitMqService rabbitMqService, ISqlSugarClient sqlSugarClient)
        {
            _configuration = configuration;
            _rabbitMqService = rabbitMqService;
            _sqlSugarClient = sqlSugarClient;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_rabbitMqService.channel);
            consumer.Received += (sender, args) =>
            {
                var message = Encoding.UTF8.GetString(args.Body.ToArray());
                var logmsg = "RabbitMq Received message: " + message;
                //如果长度超过5000，只显示前5000
                if (logmsg.Length > 5000)
                {
                    logmsg = logmsg.Substring(0, 5000);
                }
                RabbitMqLog.Info(logmsg);
                Task.Run(async () => await HandleRecivedTrans(message));
            };
            Dictionary<string, object> arguments = new Dictionary<string, object>() { { "x-message-ttl", 300000 } };
            if (!string.IsNullOrEmpty(_rabbitMqService.consumeQueue))
            {
                _rabbitMqService.channel.QueueDeclare(_rabbitMqService.consumeQueue, false, false, true, arguments);
                _rabbitMqService.channel.BasicConsume(queue: _rabbitMqService.consumeQueue, autoAck: true, consumer: consumer);

                _rabbitMqService.channel.QueueDeclare(_rabbitMqService.consumeSubQueue, false, false, true, arguments);
                _rabbitMqService.channel.BasicConsume(queue: _rabbitMqService.consumeSubQueue, autoAck: true, consumer: consumer);
            }


            return Task.CompletedTask;
        }

        //ConcurrentDictionary<string, TaskCompletionSource<RabbitMqTransaction>> dictionary = new ConcurrentDictionary<string, TaskCompletionSource<RabbitMqTransaction>>();
        private async Task HandleRecivedTrans(string message)
        {
            try
            {
                var trans = JsonConvert.DeserializeObject<RabbitMqTransaction>(message);
                if (trans.IsReply)
                {
                    var tid = trans.TransactionID;
                    if (_rabbitMqService.waitTransactions.TryGetValue(tid, out TaskCompletionSource<RabbitMqTransaction> tcs))
                    {
                        tcs.SetResult(trans);
                    }
                }
                else
                {
                    var interfaceType = typeof(ITransactionHandler);
                    var type = Assembly.GetExecutingAssembly().GetTypes().Where(t => interfaceType.IsAssignableFrom(t) && t.Name == trans.TransactionName).FirstOrDefault();

                    if (type != null && typeof(ITransactionHandler).IsAssignableFrom(type))
                    {
                        ITransactionHandler obj = (ITransactionHandler)Activator.CreateInstance(type);
                        obj.HandleTransaction(trans, _rabbitMqService, _sqlSugarClient);
                    }
                    else
                    {
                        RabbitMqLog.Error($"Transaction '{trans.TransactionName}' does not implement ITransactionHandler.");
                    }
                }
            }
            catch (Exception ex)
            {
                RabbitMqLog.Error(ex.ToString());
            }
        }
    }
}
