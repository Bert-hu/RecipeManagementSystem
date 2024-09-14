using Rms.Models.DataBase.Rms;
using Rms.Services.Core.RabbitMq;
using Rms.Services.Core.Utils;
using SqlSugar;
using System.IO.Compression;
using System.Net;

namespace Rms.Services.Core.Rms.RecipeTypeFunction
{
    internal class GeneralDirectory : IRecipeType
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Logger");
        private ISqlSugarClient db;
        private RabbitMqService rabbitMq;

        public GeneralDirectory(ISqlSugarClient _db, RabbitMqService _rabbitMq)
        {
            db = _db;
            rabbitMq = _rabbitMq;
        }
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
                    DecompressDirectory(data.CONTENT, recipeFullPath);
                    return (true, "");
                }
                else
                {
                    using (new NetworkConnection(recipepath, networkCredential))
                    {
                        DecompressDirectory(data.CONTENT, recipeFullPath);
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
                    var dirs = directoryInfo.GetDirectories().Select(it => it.Name).ToList();
                    return (true, "", dirs);
                }
                else
                {
                    using (var conn = new NetworkConnection(recipepath, networkCredential))
                    {
                        var dirs = directoryInfo.GetDirectories().Select(it => it.Name).ToList();
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
                    var body = CompressDirectory(recipeFullPath);

                    return (true, "", body);
                }
                else
                {
                    using (new NetworkConnection(recipepath, networkCredential))
                    {
                        var body = CompressDirectory(recipeFullPath);

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

        public (bool result, string message) CompareRecipe(string EquipmentId, string RecipeVersionId)
        {
            throw new NotImplementedException();
        }

        byte[] CompressDirectory(string directoryPath)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (string file in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
                    {
                        string entryName = file.Substring(directoryPath.Length + 1);
                        ZipArchiveEntry entry = archive.CreateEntry(entryName);
                        entry.LastWriteTime = File.GetLastWriteTime(file);
                        using (Stream entryStream = entry.Open())
                        using (FileStream fileStream = File.OpenRead(file))
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                }
                return memoryStream.ToArray();
            }
        }
        void DecompressDirectory(byte[] data, string outputDirectory)
        {

            using (MemoryStream memoryStream = new MemoryStream(data))
            using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string entryPath = Path.Combine(outputDirectory, entry.FullName);
                    string entryDirectory = Path.GetDirectoryName(entryPath);

                    if (!Directory.Exists(entryDirectory))
                        Directory.CreateDirectory(entryDirectory);

                    using (Stream entryStream = entry.Open())
                    using (FileStream fileStream = File.Create(entryPath))
                    {
                        entryStream.CopyTo(fileStream);
                    }
                    File.SetLastWriteTime(entryPath, entry.LastWriteTime.DateTime);
                }
            }
        }
    }
}
