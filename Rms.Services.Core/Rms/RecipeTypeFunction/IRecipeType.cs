using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Services.Core.Rms.RecipeTypeFunction
{
    internal interface IRecipeType
    {
        (bool result, string message, List<string> eppd) GetEppd(string EquipmentId);
        (bool result, string message) DeleteAllMachineRecipes(string EquipmentId);
        (bool result, string message) DownloadRecipeToMachine(string EquipmentId, string RecipeVersionId);
        (bool result, string message, byte[] body) UploadRecipeToServer(string EquipmentId, string RecipeName);
        (bool result, string message) CompareRecipe(string EquipmentId, string RecipeVersionId);
    }
}
