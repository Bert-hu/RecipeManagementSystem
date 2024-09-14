using Microsoft.AspNetCore.Mvc;
using Rms.Models.DataBase.Rms;
using Rms.Models.WebApi;
using Rms.Services.Core.Rms;
using SqlSugar;
using System.Collections;
using System.Security.Cryptography;

namespace Rms.Services.Core.Controllers
{
    public partial class ApiController : Controller
    {
        [HttpPost]
        public JsonResult CompareRecipeBody(CompareRecipeBodyRequest req)
        {
            var res = new CompareRecipeBodyResponse();

            var recipe = db.Queryable<RMS_RECIPE>().Where(it => it.EQUIPMENT_ID == req.EquipmentId && it.NAME == req.RecipeName).First();
            if (recipe == null)
            {
                res.Result = false;
                res.Message = "Recipe does not exist in RMS";
                return Json(res);
            }
            var recipe_version = db.Queryable<RMS_RECIPE_VERSION>().In(recipe.VERSION_EFFECTIVE_ID).First();
            var eqp = db.Queryable<RMS_EQUIPMENT>().In(recipe.EQUIPMENT_ID).First();

            (res.Result, res.Message) = rmsTransactionService.CompareRecipe(eqp, recipe_version.ID);
            return Json(res);
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
