using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using LocalImageViewer.Foundation;
using YiSA.Foundation.Logging;
using YiSA.WPF.Common;
namespace LocalImageViewer.DataModel
{
    /// <summary>
    /// プロジェクトデータを管理するクラス
    /// </summary>
    public class Project : DisposableBindable
    {
        private readonly Config _config;
        private readonly ILogger _logger;

        public ObservableCollection<ImageDocument> Documents { get; }

        public event EventHandler DocumentLoaded;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public Project(Config config , ILogger logger)
        {
            _config = config;
            _logger = logger;

            Documents = new ObservableCollection<ImageDocument>();
            
            // 破棄時にメタファイルを保存する
            Disposables.Add(Disposable.Create(() =>
            {
                foreach (var metaData in Documents.Select(x=>x.MetaData))
                {
                    if (metaData.IsEdited)
                    {
                        YamlSerializeHelper.SaveToFile(metaData.LatestSavedAbsolutePath,metaData);
                        _logger.WriteLine($"saved meta data {metaData.LatestSavedAbsolutePath}");
                    }
                }
            }));
        }
        /// <summary>
        /// ファイルからメタデータを読み込む
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <param name="documentMetaData"></param>
        /// <returns></returns>
        private bool TryGetDocumentMetaData(string absolutePath , out DocumentMetaData documentMetaData)
        {
            _logger.WriteLine($"load document meta data file {absolutePath}");
            var metaDataFilePath = Path.Combine(absolutePath, "meta.yml");
            if (File.Exists(metaDataFilePath))
            {
                documentMetaData = YamlSerializeHelper.LoadFromFile<DocumentMetaData>(metaDataFilePath);
                documentMetaData.LatestSavedAbsolutePath = metaDataFilePath;
                documentMetaData.ProjectAbsolutePath = _config.Project;
                documentMetaData.DirectoryAbsolutePath = absolutePath;

                return true;
            }

            _logger.WriteLine($"not found meta data file {absolutePath}");
            var title = Directory.EnumerateFiles(absolutePath)
                .OrderBy(x => x, LogicalStringComparer.Instance)
                .FirstOrDefault(x =>
                {
                    var ex = Path.GetExtension(x).ToLower();
                    var type = DocumentTypeHelper.FileExtensionToType(ex);
                    return type == DocumentType.Img;
                });

            if (string.IsNullOrEmpty(title))
            {
                documentMetaData = default;
                return false;                
            }

            documentMetaData = new DocumentMetaData()
            {
                DisplayName = Path.GetFileNameWithoutExtension(absolutePath),
                Type = DocumentTypeHelper.FileExtensionToType(Path.GetExtension(title).ToLower()),
                IsEdited = true,
                LatestSavedAbsolutePath = metaDataFilePath,
                ProjectAbsolutePath = _config.Project,
                DirectoryAbsolutePath = absolutePath,
            };
            _logger.WriteLine($"created meta data {absolutePath}");
            return true;
        }

        private object _cancelTokenLock = new();
        private CancellationTokenSource _tokenSource = null;
        public async Task LoadDocumentAsync()
        {
            try
            {
                _tokenSource?.Cancel();
                _tokenSource = null;
                Documents.Clear();
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            using (_tokenSource = new CancellationTokenSource())
            {
                var token = _tokenSource.Token;
                // ドキュメントデータ一覧をファイルから取得
                Directory.CreateDirectory(_config.Project);
                int index = 0;
                foreach (var directory in Directory.EnumerateDirectories(_config.Project,"*",SearchOption.AllDirectories)
                    .OrderByDescending(x=>new FileInfo(x).LastWriteTimeUtc))
                {
                    if (TryGetDocumentMetaData(directory, out var data))
                    {
                        if((index++%3) is 0)
                        {
                            await Task.Delay(1);
                        }

                        if (token.IsCancellationRequested || _tokenSource is null)
                        {
                            return;
                        }

                        Documents.Add(new ImageDocument(data,_config));
                    }
                }
                DocumentLoaded?.Invoke(this,EventArgs.Empty);
            }

            _tokenSource = null;
        }
    }
}