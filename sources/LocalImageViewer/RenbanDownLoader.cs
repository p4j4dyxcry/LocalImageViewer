using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Reactive.Bindings;
using YiSA.Foundation.Common;

namespace LocalImageViewer
{
    /// <summary>
    /// 連番画像ファイルをダウンロードするクラス
    /// </summary>
    public class RenbanDownLoader
    {
        private readonly Project _project;
        private readonly Config _config;
        public IReactiveProperty<string> Address { get; } = new ReactiveProperty<string>(string.Empty);
        public IReactiveProperty<int> Start { get; } = new ReactiveProperty<int>(0);
        public IReactiveProperty<int> End { get; } = new ReactiveProperty<int>();
        public IReactiveProperty<string> DownloadLogInfo { get; } = new ReactiveProperty<string>();
        public IReactiveProperty<string> DownloadDirectoryName { get; } = new ReactiveProperty<string>(string.Empty);

        public IReactiveProperty<int> FillZeroCount { get; } = new ReactivePropertySlim<int>(0);
        
        public RenbanDownLoader(Project project , Config config)
        {
            _project = project;
            _config = config;
        }

        public async Task DownLoad(params string[] uris)
        {
            if(uris.Length is 0)
                return;
            
            // ディクトリの指定、からの場合はランダムなディレクトリを指定する
            var dir = DownloadDirectoryName.Value;
            if (string.IsNullOrEmpty(dir))
                dir = Guid.NewGuid().ToString();
            var absoluteDir = Path.Combine(_config.Project, dir);
            Directory.CreateDirectory(absoluteDir);

            int pageId = 0;
            foreach (var uri in uris)
            {
                using var client = new System.Net.WebClient();

                var pageUri = uri;
                
                // 出力先パス
                var filePath = Path.Combine(absoluteDir, $"{pageId++}{Path.GetExtension(pageUri)}");
                    
                int count = 2;
                while (count-- > 0)
                {
                    try
                    {
                        await client.DownloadFileTaskAsync(new Uri(pageUri),filePath);
                        DownloadLogInfo.Value += $"OK {pageUri}\n";
                        await Task.Delay(50);
                        break;
                    }
                    catch(Exception )
                    {
                        FileSystemUtility.TryFileDelete(filePath);
                            
                        DownloadLogInfo.Value += $"Error {pageUri}\n";
                            
                        // 拡張子を変えて試す。
                        ChangeUriExtension(pageUri);
                    }                        
                }
            }
            
            // ダウンロード完了、ドキュメントをメイン画面に追加する
            _project.Documents.Add(new ImageDocument(new DocumentMetaData()
            {
                Type = DocumentTypeHelper.FileExtensionToType(Path.GetExtension(uris[0])),
                DisplayName = string.Empty,
                LatestSavedAbsolutePath = Path.Combine(absoluteDir,"meta.yml"),
                DirectoryAbsolutePath = absoluteDir,
                ProjectAbsolutePath = _config.Project,
            },_config));
        }

        private string ChangeUriExtension(string value)
        {
            if (value.EndsWith("jpg"))
                value = Path.ChangeExtension(value, "png");
            else if (value.EndsWith("png"))
                value = Path.ChangeExtension(value, "jpg");
            else
                value = Path.ChangeExtension(value, "png");
            return value;
        }

        /// <summary>
        /// ダウンロードプレビュー用
        /// ImageMagicを使ってサムネイルサイズに加工してから取得する
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<ImageSource> DownLoadPreview(string uri)
        {
            try
            {
                using var client = new System.Net.WebClient();
                var bytes = await client.DownloadDataTaskAsync(new Uri(uri));
                var image = await ImageSourceHelper.LoadThumbnailFromByteAsync(bytes, 128, 128);
                return image;
            }
            catch
            {
                return ThumbnailService.NoneImageSource;
            }
        }

        /// <summary>
        /// アドレスからプロパティを自動設定する
        /// 1. アドレスの末部に数字を含む文字列が存在する場合 ワイルドーカードに置き換える
        /// 2. また、ページの最終番号を取得した数字に置き換える
        /// </summary>
        /// <param name="value"></param>
        public void SetAddressWithAutoUpdateProperties(string value)
        {
            var index = value.LastIndexOf('/');

            var element1 = string.Empty;
            var element2 = string.Empty;
            if (index > 0)
            {
                element1 = value.Substring(0, index);
                element2 = value.Substring(index);
            }
            else
            {
                element1 = string.Empty;
                element2 = value;
            }

            if (element2.Contains('*'))
            {
                Address.Value = value;
                return;
            }
         
            var convertedElement = RenbanHelper.ReplaceLastNumber(element2,"*");

            if (convertedElement.Contains("*"))
            {
                var last = RenbanHelper.FindLastNumber(element2);
                if (last != -1)
                {
                    End.Value = last;
                    Address.Value = element1 + convertedElement;
                }
            }
            else
            {
                Address.Value = value;
            }
        }

        public IEnumerable<string> EnumerateDownloadItems(string value)
        {
            bool fillZero = FillZeroCount.Value > 0;
            
            for (int i = Start.Value; i < End.Value + 1; i++)
            {
                if(fillZero is false)
                    yield return value.Replace("*", i.ToString());
                else
                {
                    var formatBuilder = new StringBuilder();

                    for (int j = 0; j < FillZeroCount.Value; ++j)
                        formatBuilder.Append('0');

                    yield return value.Replace("*", i.ToString(formatBuilder.ToString()));                    
                }
            }
        }
    }
}