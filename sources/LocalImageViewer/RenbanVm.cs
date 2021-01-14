using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using YiSA.WPF.Common;

namespace LocalImageViewer
{
    /// <summary>
    /// 連番ダウンロード画面のViewModel
    /// </summary>
    public class RenbanVm : DisposableBindable
    {
        public IReactiveProperty<string> Address { get; }
        public IReactiveProperty<int> Start => _renbanDownLoader.Start;
        public IReactiveProperty<int> End => _renbanDownLoader.End;
        public IReactiveProperty<string> SaveDirectoryName => _renbanDownLoader.DownloadDirectoryName;
        
        public IReactiveProperty<string> TextInput { get; }
        public IReactiveProperty<string> UrlsPreview { get; }
        public IReactiveProperty<string> DownloadLogInfo => _renbanDownLoader.DownloadLogInfo;
        public AsyncReactiveCommand DownLoadCommand { get; }

        public ImageSource Preview1 { get; private set;} = ThumbnailService.NoneImageSource;
        public ImageSource Preview2 { get; private set;} = ThumbnailService.NoneImageSource;
        public ImageSource Preview3 { get; private set;} = ThumbnailService.NoneImageSource;
        
        
        private readonly RenbanDownLoader _renbanDownLoader;
        public RenbanVm(RenbanDownLoader renbanDownLoader)
        {
            _renbanDownLoader = renbanDownLoader;

            Address = new ReactiveProperty<string>(string.Empty);
            UrlsPreview = new ReactivePropertySlim<string>(string.Empty).AddTo(Disposables);
            TextInput = new ReactiveProperty<string>(string.Empty).AddTo(Disposables);

            // ダウンロードコマンド
            {
                DownLoadCommand = UrlsPreview
                    .Select(x=>ToUrls())
                    .Select(urls => urls.Any() && urls.All(url => Uri.TryCreate(url, UriKind.Absolute, out _)))
                    .ToAsyncReactiveCommand();

                DownLoadCommand.Subscribe(async () =>
                    {
                        DownloadLogInfo.Value = string.Empty;
                        await _renbanDownLoader.DownLoad(ToUrls());
                    })
                    .AddTo(Disposables);                
            }
            
            // Vmの入力を RenbanDownloaderで処理し、データを加工する
            Address.Throttle(TimeSpan.FromMilliseconds(100), UIDispatcherScheduler.Default)
                .Subscribe(x =>
                {
                    _renbanDownLoader.SetAddressWithAutoUpdateProperties(x);
                    Address.Value = renbanDownLoader.Address.Value;
                })
                .AddTo(Disposables);

            // 各パラメータの更新に併せてダウンロード候補一覧表示を更新する
            Address.ToUnit()
                .Merge(TextInput.ToUnit())
                .Merge(Start.ToUnit())
                .Merge(End.ToUnit())
                .Throttle(TimeSpan.FromMilliseconds(250), UIDispatcherScheduler.Default)
                .Select(_ =>CreateDownloadCandidate())
                .Subscribe(x => UrlsPreview.Value = x).AddTo(Disposables);

            UrlsPreview.Throttle(TimeSpan.FromMilliseconds(500), UIDispatcherScheduler.Default)
                .Subscribe(async _=> await CreatePreviewThumbnailsAsync()).AddTo(Disposables);
        }

        /// <summary>
        /// プレビューサムネイル一覧を非同期で生成する
        /// </summary>
        /// <returns></returns>
        private async Task CreatePreviewThumbnailsAsync()
        {
            var uris = ToUrls()
                .Where(x => Uri.TryCreate(x, UriKind.Absolute, out _))
                .Take(3)
                .ToArray();

            await Task.WhenAll(uris.Select(SetPreviewImageAsync));
        }

        /// <summary>
        /// 個別のプレビューイメージを非同期で設定する
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task SetPreviewImageAsync(string uri, int id)
        {
            if (id is 0)
            {
                Preview1 = await _renbanDownLoader.DownLoadPreview(uri);
                OnPropertyChanged(nameof(Preview1));
            }

            if (id is 1)
            {
                Preview2 = await _renbanDownLoader.DownLoadPreview(uri);
                OnPropertyChanged(nameof(Preview2));
            }

            if (id is 2)
            {
                Preview3 = await _renbanDownLoader.DownLoadPreview(uri);
                OnPropertyChanged(nameof(Preview3));
            }
        }

        /// <summary>
        /// 処理するUrl一覧を取得する
        /// </summary>
        /// <returns></returns>
        private string[] ToUrls()
        {
            return UrlsPreview.Value.Split('\r', '\n').Where(x => string.IsNullOrWhiteSpace(x) is false).ToArray();
        }

        /// <summary>
        /// ダウンロード候補一覧のプレビューを文字列を生成する
        /// </summary>
        /// <returns></returns>
        private string CreateDownloadCandidate()
        {
            if (string.IsNullOrEmpty(TextInput.Value) is false)
            {
                return TextInput.Value;
            }
                    
            try
            {
                var items = _renbanDownLoader.EnumerateDownloadItems(_renbanDownLoader.Address.Value).ToArray();
                if (items.Length is 0)
                    return string.Empty;
                if (items.Length is 1)
                    return items.First();
                else
                    return items.Aggregate((x, y) => $"{x}\n{y}");
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}