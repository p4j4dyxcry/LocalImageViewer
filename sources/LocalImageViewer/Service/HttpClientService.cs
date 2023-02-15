using System.Net.Http;
using YiSA.WPF.Common;
namespace LocalImageViewer.Service
{
    public class HttpClientService : DisposableHolder
    {
        private HttpClient _client = null;
        public HttpClient Client => _client ??= CreateClientCore();

        private HttpClient CreateClientCore()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add( "User-Agent", "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko");
            client.DefaultRequestHeaders.Add("Accept-Language", "ja-JP");
            Disposables.Add(client);
            return client;
        }
    }
}
