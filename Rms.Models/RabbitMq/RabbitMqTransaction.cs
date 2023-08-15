using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.RabbitMq
{
    public class RabbitMqTransaction
    {
        public string TransactionID { get; set; } = Guid.NewGuid().ToString();
        public string TransactionName { get; set; }
        public string EquipmentID { get; set; }
        public bool IsReply { get; set; } = false;
        public bool NeedReply { get; set; } = false;
        public string ReplyChannel { get; set; } = string.Empty;
        // public string Body { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public int ExpireSecond { get; set; } = 120;
        public Dictionary<string, object> Parameters { get; set; }
    }

    public class CacheRabbitMqTransaction : RabbitMqTransaction
    {
        public string DestinationChannel { get; set; }
    }
}
