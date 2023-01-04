using System;
using System.Collections.Generic;
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
        private readonly SlimLocker _documentLocker = new();

        public bool IsLoading => _tokenSource is not null;

        public DataSource<ImageDocument> DocumentSource { get; } = new();
        public IReadOnlyCollection<ImageDocument> Documents => DocumentSource.SnapshotItems;

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

            // 破棄時にメタファイルを保存する
            Disposables.Add(Disposable.Create(SaveDocuments));
        }

        private void SaveDocuments()
        {
            foreach (var doc in Documents)
            {
                if (doc.IsEditedMetaData())
                {
                    var metaData = doc.MetaData;
                    YamlSerializeHelper.SaveToFile(metaData.LatestSavedAbsolutePath,metaData);
                    _logger.WriteLine($"saved meta data {metaData.LatestSavedAbsolutePath}");
                }
            }

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
            var title = FileSystemEnumerator.EnumerateFiles(absolutePath, true)
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
                LatestSavedAbsolutePath = metaDataFilePath,
                ProjectAbsolutePath = _config.Project,
                DirectoryAbsolutePath = absolutePath,
            };
            _logger.WriteLine($"created meta data {absolutePath}");
            return true;
        }

        private object _cancelTokenLock = new();
        private CancellationTokenSource _tokenSource = null;
        public async Task LoadDocumentAsync(int initial)
        {
            try
            {
                _tokenSource?.Cancel();
                _tokenSource = null;
                if (DocumentSource.SnapshotItems.Any())
                {
                    SaveDocuments();
                    DocumentSource.Clear();
                }

            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            using (_tokenSource = new CancellationTokenSource())
            {
                using var _ = _documentLocker.WriteLock();
                Directory.CreateDirectory(_config.Project);

                await DocumentSource.ReadAsync(EnumerateImageDocuments(),initial);
                DocumentLoaded?.Invoke(this,EventArgs.Empty);
            }

            _tokenSource = null;
        }

        private IEnumerable<ImageDocument> EnumerateImageDocuments()
        {
            foreach (var directory in Directory
                .EnumerateDirectories(_config.Project,"*",SearchOption.AllDirectories)
                .OrderByDescending(x=>new FileInfo(x).LastWriteTimeUtc))
            {
                if (TryGetDocumentMetaData(directory, out var data))
                {
                    yield return new ImageDocument(data, _config);
                }
            }
        }
    }
}
