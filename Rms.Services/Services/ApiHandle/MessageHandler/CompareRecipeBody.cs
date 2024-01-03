using Newtonsoft.Json;
using RMS.Domain.Rms;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rms.Services.Services.ApiHandle.MessageHandler;
using System.Collections;
using System.IO;
using System.Security.Cryptography;

namespace Rms.Services.Services.ApiHandle.MessageHandler
{
    public partial class CompareRecipeBody : CommonHandler
    {
        public override ResponseMessage Handle(string jsoncontent)
        {
            var db = DbFactory.GetSqlSugarClient();
            var res = new CompareRecipeBodyResponse();
            var req = JsonConvert.DeserializeObject<CompareRecipeBodyRequest>(jsoncontent);

            var recipe = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == req.EquipmentId && it.NAME == req.RecipeName).First();
            if (recipe == null)
            {
                res.Result = false;
                res.Message = "Recipe does not exist in RMS";
                return res;
            }
            var recipe_version = db.Queryable<RMS_RECIPE_VERSION>().In(recipe.VERSION_EFFECTIVE_ID).First();
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(recipe.EQUIPMENT_ID).First();

            //TODO GET BODY: FILE, DIRECTORY, SECS FORMATTED BODY, SECS UNFORMATTED BODY

            //TODO COMPARE BODY
            return res;
        }

        //用于比较文件
        bool CompareZipFiles(string machinefile, FileStream serverstream)
        {
            using (FileStream machinestream = new FileStream(machinefile, FileMode.Open, FileAccess.Read))
            {
                byte[] hash1 = ComputeHash(machinestream);
                byte[] hash2 = ComputeHash(serverstream);

                return StructuralComparisons.StructuralEqualityComparer.Equals(hash1, hash2);
            }
        }

        byte[] ComputeHash(Stream stream)
        {
            using (var hashAlgorithm = SHA256.Create())
            using (var bufferedStream = new BufferedStream(stream, 1024 * 1024))
            {
                return hashAlgorithm.ComputeHash(bufferedStream);
            }
        }


    }
}
