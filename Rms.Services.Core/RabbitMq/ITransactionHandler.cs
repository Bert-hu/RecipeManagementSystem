
using Rms.Models.RabbitMq;
using SqlSugar;

namespace Rms.Services.Core.RabbitMq
{
    internal interface ITransactionHandler
    {
        void HandleTransaction(RabbitMqTransaction trans, RabbitMqService rabbitMq, ISqlSugarClient sqlSugarClient);
    }
}
