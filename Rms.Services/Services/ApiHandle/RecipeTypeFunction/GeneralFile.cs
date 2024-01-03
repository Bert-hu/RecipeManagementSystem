using Rms.Models.DataBase.Rms;
using Rms.Utils;
using RMS.Domain.Rms;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Rms.Services.Services.ApiHandle.RecipeTypeFunction
{
    internal class GeneralFile : IRecipeType
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");
        SqlSugarClient db = DbFactory.GetSqlSugarClient();
        public (bool result, string message) DeleteAllMachineRecipes(string EquipmentId)
        {
            throw new NotImplementedException();
        }

        public (bool result, string message) DownloadRecipeToMachine(string EquipmentId, string RecipeVersionId)
        {
            try
            {
                var eqp = db.Queryable<RMS_EQUIPMENT>().InSingle(EquipmentId);
                var username = eqp.USERNAME;
                var password = eqp.PASSWORD;
                var recipepath = eqp.RECIPE_PATH;
                var version = db.Queryable<RMS_RECIPE_VERSION>().InSingle(RecipeVersionId);
                var recipe = db.Queryable<RMS_RECIPE>().InSingle(version.RECIPE_ID);
                var data = db.Queryable<RMS_RECIPE_DATA>().InSingle(version.RECIPE_DATA_ID);
                NetworkCredential networkCredential = new NetworkCredential(username, password);
                DirectoryInfo directoryInfo = new DirectoryInfo(recipepath);
                string recipeFullPath = Path.Combine(recipepath, recipe.NAME);
                if (directoryInfo.Exists)
                {
                    DecompressFile(data.CONTENT, recipeFullPath);
                    return (true, "");
                }
                else
                {
                    using (new NetworkConnection(recipepath, networkCredential))
                    {
                        DecompressFile(data.CONTENT, recipeFullPath);
                        return (true, "");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return (false, $"Error: {ex.Message}");
            }
        }

        public (bool result, string message, List<string> eppd) GetEppd(string EquipmentId)
        {
            try
            {
                var eqp = db.Queryable<RMS_EQUIPMENT>().InSingle(EquipmentId);
                var username = eqp.USERNAME;
                var password = eqp.PASSWORD;
                var recipepath = eqp.RECIPE_PATH;
                NetworkCredential networkCredential = new NetworkCredential(username, password);
                DirectoryInfo directoryInfo = new DirectoryInfo(recipepath);

                if (directoryInfo.Exists)
                {
                    var dirs = directoryInfo.GetFiles().Select(it => it.Name).ToList();
                    return (true, "", dirs);
                }
                else
                {
                    using (var conn = new NetworkConnection(recipepath, networkCredential))
                    {
                        var dirs = directoryInfo.GetFiles().Select(it => it.Name).ToList();
                        conn.Dispose();
                        return (true, "", dirs);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return (false, $"Error: {ex.Message}", null);
            }
        }

        public (bool result, string message, byte[] body) UploadRecipeToServer(string EquipmentId, string RecipeName)
        {

            try
            {
                var eqp = db.Queryable<RMS_EQUIPMENT>().InSingle(EquipmentId);
                var username = eqp.USERNAME;
                var password = eqp.PASSWORD;
                var recipepath = eqp.RECIPE_PATH;
                NetworkCredential networkCredential = new NetworkCredential(username, password);
                DirectoryInfo directoryInfo = new DirectoryInfo(recipepath);
                string recipeFullPath = Path.Combine(recipepath, RecipeName);
                if (directoryInfo.Exists)
                {
                    var body = CompressFile(recipeFullPath);

                    return (true, "", body);
                }
                else
                {
                    using (new NetworkConnection(recipepath, networkCredential))
                    {
                        var body = CompressFile(recipeFullPath);

                        return (true, "", body);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return (false, $"Error: {ex.Message}", null);
            }
        }

        byte[] CompressFile(string filePath)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    string entryName = Path.GetFileName(filePath);
                    ZipArchiveEntry entry = archive.CreateEntry(entryName);
                    entry.LastWriteTime = File.GetLastWriteTime(filePath);
                    using (Stream entryStream = entry.Open())
                    using (FileStream fileStream = File.OpenRead(filePath))
                    {
                        fileStream.CopyTo(entryStream);
                    }
                }
                return memoryStream.ToArray();
            }
        }
        void DecompressFile(byte[] data, string outputFilePath)
        {
            using (MemoryStream memoryStream = new MemoryStream(data))
            using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
            {
                if (archive.Entries.Count != 1)
                {
                    // Handle error: The archive should contain exactly one entry for a single file
                    throw new InvalidOperationException("Invalid archive format for a single file decompression.");
                }                
                ZipArchiveEntry entry = archive.Entries[0];
                using (Stream entryStream = entry.Open())
                using (FileStream fileStream = File.Create(outputFilePath))
                {
                    entryStream.CopyTo(fileStream);
                }
                File.SetLastWriteTime(outputFilePath, entry.LastWriteTime.DateTime);
            }
        }


    }
}
