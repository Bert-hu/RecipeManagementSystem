
using Newtonsoft.Json;
using Rms.Models.RabbitMq;
using Rms.Utils;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Threading.Tasks;


namespace Rms.Web
{
    public class RabbitMqService
    {
        //private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");

        static RabbitMqHelper rabbitMq;

        static RabbitMqService()
        {
        }
        public static void Initiate(string _host, string _user, string _password, int _port = 5672)
        {
            rabbitMq?.CloseSession();
            rabbitMq = new RabbitMqHelper(_host, _user, _password, _port);

        }

        public static void BeginConsume(string routingKey)
        {
            rabbitMq?.BeginConsume(routingKey, RabbitMqReceive);
        }

        public static void Produce(string routingKey, RabbitMqTransaction trans)
        {
            try
            {
                var ListenChannel = ConfigurationManager.AppSettings["ListenChannel"];

                if (trans.NeedReply == true && string.IsNullOrEmpty(trans.ReplyChannel)) trans.ReplyChannel = ListenChannel;
                var message = JsonConvert.SerializeObject(trans);

                rabbitMq?.Produce(routingKey, message, trans.ExpireSecond);

            }
            catch (Exception ex)
            {
                //Log.Error(ex);
            }

        }

        static ConcurrentDictionary<string, TaskCompletionSource<RabbitMqTransaction>> dictionary = new ConcurrentDictionary<string, TaskCompletionSource<RabbitMqTransaction>>();
        public static RabbitMqTransaction ProduceWaitReply(string routingKey, RabbitMqTransaction trans, int timeoutInSeconds)
        {
            try
            {

                var tcs = new TaskCompletionSource<RabbitMqTransaction>();
                var message = JsonConvert.SerializeObject(trans);
                rabbitMq?.Produce(routingKey, message, trans.ExpireSecond);
                var tid = trans.TransactionID;
                dictionary.TryAdd(tid, tcs);
                var completedTaskIndex = Task.WaitAny(new Task[] { tcs.Task }, timeoutInSeconds * 1000);
                if (completedTaskIndex == 0)
                {
                    dictionary.TryRemove(tid, out _);
                    return tcs.Task.Result;
                }
                else
                {
                    dictionary.TryRemove(tid, out _);
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static void RabbitMqReceive(object model, string message)
        {
            try
            {

                var trans = JsonConvert.DeserializeObject<RabbitMqTransaction>(message);
                HandleTrans(trans);
            }
            catch
            {
                //Log.Warn($"Can not parse message: {message}");
            }
        }

        private static void HandleTrans(RabbitMqTransaction trans)
        {
            try
            {

                //RabbitMqTransaction result = null;

                if (trans.IsReply)
                {
              

                    var tid = trans.TransactionID;
                    if (dictionary.TryGetValue(tid, out TaskCompletionSource<RabbitMqTransaction> tcs))
                    {
                        tcs.SetResult(trans);
                       // dictionary.TryRemove(tid, out _);
                    }
                }
                else
                {
                    //TODO handle primary in trans
                    switch (trans.TransactionName)
                    {


                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                //Log.Error(ex.Message);
            }
        }



        private static T ConvertObjectToType<T>(object obj)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));
        }


    }
}
