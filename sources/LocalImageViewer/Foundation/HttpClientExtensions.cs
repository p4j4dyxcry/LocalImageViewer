using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
namespace LocalImageViewer.Foundation
{
    public static class HttpClientExtensions
    {
        public static async Task DownloadToFile(this HttpClient httpClient,string uri,string filePath)
        {
            HttpResponseMessage res = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

            await using var fileStream = File.Open(filePath,FileMode.OpenOrCreate);
            await using var httpStream = await res.Content.ReadAsStreamAsync();

            await httpStream.CopyToAsync(fileStream);
            fileStream.Flush();
        }
    }
}