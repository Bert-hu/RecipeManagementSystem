using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace Rms.Utils
{
    public class RabbitMqHelper
    {
        ConnectionFactory factory;
        IConnection connection;
        IModel channel;

        public delegate void MessageReceivedHandler(object model, BasicDeliverEventArgs ea);
        public delegate void StrMessageReceivedHandler(object model, string message);



        public RabbitMqHelper(string _host,string _user,string _password,int _port = 5672)
        {
            factory = new ConnectionFactory();
            factory.HostName = _host;
            factory.Port = _port;
            factory.UserName = _user;
            factory.Password = _password;
            factory.AutomaticRecoveryEnabled = true;//20221010:  避免MESSAGE:ALREADY CLOSED: THE AMQP OPERATION WAS INTERRUPTED 报错
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
       }


        public void Produce(string routingKey,string message,int ExpireSecond = 60)
        {
            Dictionary<string,object> arguments = new Dictionary<string, object>() { { "x-message-ttl", 300000 } };
            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = 2;
            properties.Expiration = (ExpireSecond* 1000).ToString();
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish("", routingKey, properties, body);
        }

        public void BeginConsume(string routingKey, MessageReceivedHandler handler)
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>() { { "x-message-ttl", 300000 } };
            channel.QueueDeclare(routingKey, false, false, true, arguments);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += new EventHandler<BasicDeliverEventArgs>(handler) ;
            channel.BasicConsume(routingKey, true, consumer);

          //while (true)
          //{
          //    var aa = channel.BasicConsume(routingKey, true, consumer);
          //
          //}
        }

        public void BeginConsume(string routingKey, StrMessageReceivedHandler handler)
        {
            BeginConsume(routingKey, (model, ea) =>
            {
                handler(model, Encoding.UTF8.GetString(ea.Body.ToArray()));
            });
        }

        public void StopConsume(string routingKey)
        {
            
        }

        public void CloseSession()
        {
            connection?.Close();
        }

    }
}
