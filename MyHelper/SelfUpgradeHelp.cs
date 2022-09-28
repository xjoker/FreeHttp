using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace FreeHttp.MyHelper
{
    public class SelfUpgradeHelp
    {
        private static readonly HttpClient httpClient = new HttpClient();

        private static string GetFreeHttpDllPath()
        {
            string path = null;
            try
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                path = Uri.UnescapeDataString(uri.Path);
                //Path.GetDirectoryName(path);
            }
            catch
            {
                path = null;
            }
            finally
            {
                if (string.IsNullOrEmpty(path)) path = Directory.GetCurrentDirectory() + "\\Scripts\\FreeHttp.dll";
            }

            return path;
        }

        private static async Task<bool> DownloadUpgradeFileAsync(string uri, string path)
        {
            if (File.Exists(path))
                try
                {
                    File.Delete(path);
                }
                catch
                {
                    return false;
                }

            var response = await httpClient.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var fileStrem = await response.Content.ReadAsStreamAsync();
                using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await fileStrem.CopyToAsync(fs);
                    fs.Close();
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public static async Task<string> UpdateDllAsync(string sourceFileUrl)
        {
            var path = GetFreeHttpDllPath();
            var oldFilePath = path + ".oldversion";
            var upgradeFile = Path.GetDirectoryName(path) + "/FreeHttpUpgradeFile";
            try
            {
                if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
                //https://lulianqi.com/file/FreeHttpUpgradeFile
                await DownloadUpgradeFileAsync(sourceFileUrl, upgradeFile);
                File.Move(path, oldFilePath);
                File.Move(upgradeFile, path);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return null;
        }
    }
}