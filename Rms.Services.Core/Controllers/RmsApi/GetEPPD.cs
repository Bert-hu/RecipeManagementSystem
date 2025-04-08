
using Rms.Services.Core.Extension;
using Rms.Services.Core.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Services.Core.Rms;

namespace Rms.Services.Core.Controllers
{
    public partial class ApiController : Controller
    {
        /// <summary>
        /// 获取设备当前EPPD
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetEPPD(GetEppdRequest req)
        {
            var res = new GetEppdResponse();
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(req.EquipmentId)?.First();
            if (eqp == null)
            {
                res.Message = "EQID not exists!";
                return Json(res);
            }
            (res.Result, res.Message, res.EPPD) = rmsTransactionService.GetEppd(eqp);
            return Json(res);
        }
    }
}
