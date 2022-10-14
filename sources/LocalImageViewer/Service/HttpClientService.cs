using System.Net.Http;
using YiSA.WPF.Common;
namespace LocalImageViewer.Service
{
    public class HttpClientService : DisposableHolder
    {
        public HttpClient Client{ get; } = new HttpClient();

        public HttpClientService()
        {
            Disposables.Add(Client);
        }
    }
}
