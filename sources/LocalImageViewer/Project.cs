using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using YiSA.WPF.Common;

namespace LocalImageViewer
{
    /// <summary>
    /// プロジェクトデータを管理するクラス
    /// </summary>
    public class Project : DisposableBindable
    {
        private readonly Config _config;
    
        public ObservableCollection<ImageDocument> Documents { get; }
        
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="config"></param>
        public Project(Config config)
        {
            _config = config;

            var list = new List<DocumentMetaData>();

            // ドキュメントデータ一覧をファイルから取得
            Directory.CreateDirectory(_config.Project);
            foreach (var directory in Directory.EnumerateDirectories(_config.Project))
            {
                if(TryGetDocumentMetaData(directory,out var data))
                    list.Add(data);
            }

            Documents = new ObservableCollection<ImageDocument>(list.Select(x=>new ImageDocument(x,config)));
            
            // 破棄時にメタファイルを保存する
            Disposables.Add(Disposable.Create(() =>
            {
                foreach (var metaData in Documents.Select(x=>x.MetaData))
                {
                    if(metaData.IsEdited)
                        YamlSerializeHelper.SaveToFile(metaData.LatestSavedAbsolutePath,metaData);
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
            var metaDataFilePath = Path.Combine(absolutePath, "meta.yml");
            if (File.Exists(metaDataFilePath))
            {
                documentMetaData = YamlSerializeHelper.LoadFromFile<DocumentMetaData>(metaDataFilePath);
                documentMetaData.LatestSavedAbsolutePath = metaDataFilePath;
                documentMetaData.ProjectAbsolutePath = _config.Project;
                documentMetaData.DirectoryAbsolutePath = absolutePath;

                return true;
            }
            
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
            return true;
        }
    }
}