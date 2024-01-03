using Rms.Models.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Services.Services.ApiHandle
{
    public interface IMessageHandler
    {
        ResponseMessage Handle(string jsoncontent);
    }
}
