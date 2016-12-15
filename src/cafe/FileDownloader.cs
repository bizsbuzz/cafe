using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace cafe
{
    public class FileDownloader : IFileDownloader
    {
        private static ILogger Logger { get; } =
            ApplicationLogging.CreateLogger<FileDownloader>();

        public bool Download(Uri downloadLink, string file)
        {
            return DownloadAsync(downloadLink, file).GetAwaiter().GetResult();
        }

        public async Task<bool> DownloadAsync(Uri downloadLink, string file)
        {
            using (var httpClient = new HttpClient())
            {
                using (
                    var request = new HttpRequestMessage(HttpMethod.Get, downloadLink)
                )
                {
                    const int bufferSize = 4096;
                    var response = (await httpClient.SendAsync(request));
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        Logger.LogInformation($"File at {downloadLink} doesn not exist");
                        return false;
                    }
                    using (
                        Stream contentStream = await response.Content.ReadAsStreamAsync(),
                            stream = new FileStream(file, FileMode.Create, FileAccess.Write,
                                FileShare.None, bufferSize, true))
                    {
                        Logger.LogDebug("Downloading file");
                        await contentStream.CopyToAsync(stream);
                        Logger.LogDebug("Finished downloading file");
                    }
                }
            }
            return true;
        }
    }
}