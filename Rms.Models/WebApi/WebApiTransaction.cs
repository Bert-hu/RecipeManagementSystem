using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Models.WebApi
{
    public class ResponseMessage
    {
        public bool Result { get; set; } = false;
        public string Message { get; set; }
        //public Dictionary<string,object> Parameters { get; set; }
    }

    public class GetEppdRequest
    {
        public string EquipmentId { get; set; }
    }
    public class GetEppdResponse : ResponseMessage
    {
        public List<string> EPPD { get; set; }
    }

    public class AddNewRecipeRequest
    {
        public string TrueName { get; set; } = "Default";
        public string EquipmentId { get; set; }
        public string RecipeName { get; set; }
    }
    public class AddNewRecipeResponse : ResponseMessage
    {
        public string RECIPE_ID { get; set; }
        public string VERSION_LATEST_ID { get; set; }
    }

    public class AddNewRecipeVersionRequest
    {
        public string TrueName { get; set; } = "Default";
        public string EquipmentId { get; set; }
        public string RecipeId { get; set; }
        public string RecipeName { get; set; }
    }
    public class AddNewRecipeVersionResponse : ResponseMessage
    {
        public string VERSION_LATEST_ID { get; set; }
    }
    public class ReloadRecipeBodyRequest
    {
        public string TrueName { get; set; } //上传人
        public string RecipeName { get; set; } //用于检查设备回复的Name是否一致
        public string VersionId { get; set; }
    }

    public class ReloadRecipeBodyResponse : ResponseMessage
    {
        public string RECIPE_DATA_ID { get; set; }
    }

    public class CompareRecipeBodyRequest
    {
        public string EquipmentId { get; set; }
        public string RecipeName { get; set; }
    }

    public class CompareRecipeBodyResponse : ResponseMessage
    {
        public string RecipeName { get; set; }
    }

    public class DownloadEffectiveRecipeToMachineRequest
    {
        public string TrueName { get; set; }
        public string EquipmentId { get; set; }
        public string RecipeName { get; set; }
    }

    public class DownloadEffectiveRecipeToMachineResponse : ResponseMessage
    {
        public string RecipeName { get; set; }
    }


    public class DownloadEffectiveRecipeByRecipeGroupRequest
    {
        public string TrueName { get; set; }
        public string EquipmentId { get; set; }
        public string RecipeGroupName { get; set; }
    }

    public class DownloadEffectiveRecipeByRecipeGroupResponse : ResponseMessage
    {
        public string RecipeName { get; set; }
    }


    public class GetSvidValueRequest
    {
        public string EquipmentId { get; set; }
        public int[] VidList { get; set; }
    }


    public class GetSvidValueResponse : ResponseMessage
    {

    }
    public class GetEquipmentStatusRequest
    {
        public string EquipmentId { get; set; }
    }

    public class GetEquipmentStatusResponse : ResponseMessage
    {
        public string Status { get; set; }
    }

    public class GetMarkingTextsRequest
    {
        public string TrueName { get; set; }
        public string EquipmentId { get; set; }
        public string RecipeName { get; set; }
        public Dictionary<string, string> SfisParameter { get; set; }
    }
    public class GetMarkingTextsResponse : ResponseMessage
    {
        public string[] MarkingTexts { get; set; }
    }

    public class CheckRecipeGroupRequest
    {
        public string EquipmentId { get; set; }
        public string RecipeGroupName { get; set; }
        public bool CheckLastRecipe { get; set; } = true;
    }

    public class CheckRecipeGroupResponse : ResponseMessage
    {
        public string RecipeName { get; set; }
    }

    public class DeleteAllRecipesRequest
    {
        public string EquipmentId { get; set; }
    }
    public class DeleteAllRecipesResponse : ResponseMessage
    {
    }


    public class AddNewRecipeVersionWithBodyRequest
    {
        public string TrueName { get; set; } = "Default";
        public string RecipeVersionId { get; set; }
        public string RecipeBody { get; set; }
    }

    public class AddNewRecipeVersionWithBodyResponse : ResponseMessage
    {
    }

    public class DownloadMapByLotRequest
    {
        public string EquipmentId { get; set; }
        public string LotId { get; set; }
    }

    public class DownloadMapByLotResponse : ResponseMessage
    {

    }

    public class UploadMapByLotRequest
    {
        public string EquipmentId { get; set; }
        public string LotId { get; set; }
        //public string UploadPath { get; set; }
    }

    public class UploadMapByLotResponse : ResponseMessage
    {

    }
    public class PpSelectRequest
    {
        public string TrueName { get; set; }
        public string EquipmentId { get; set; }
        public string RecipeName { get; set; }
    }

    public class PpSelectResponse : ResponseMessage
    {
        public string RecipeName { get; set; }
    }

    public class PpSelectByRecipeGroupRequest
    {
        public string TrueName { get; set; }
        public string EquipmentId { get; set; }
        public string RecipeGroupName { get; set; }
    }

    public class PpSelectByRecipeGroupResponse : ResponseMessage
    {
        public string RecipeName { get; set; }
    }

    public class GetRecipeNameAliasRequest
    {
        public string EquipmentTypeId { get; set; }
        public string RecipeName { get; set; }
    }

    public class GetRecipeNameAliasResponse : ResponseMessage
    {
        public string Id { get; set; }
        public string EquipmentTypeId { get; set; }
        public string RecipeName { get; set; }
        public List<string> RecipeAlias { get; set; }
    }

    public class SetRecipeNameAliasRequest
    {
        public string EquipmentTypeId { get; set; }
        public string RecipeName { get; set; }
        public List<string> RecipeAlias { get; set; }
    }

    public class SetRecipeNameAliasResponse : ResponseMessage
    { }

    public class GetRecipeNameRequest
    {
        public string EquipmentTypeId { get; set; }
        public string RecipeNameAlias { get; set; }
    }

    public class GetRecipeNameResponse : ResponseMessage
    {
        public string Id { get; set; }
        public string EquipmentTypeId { get; set; }
        public string RecipeName { get; set; }
    }
}
