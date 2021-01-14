using System.IO;
using System.Linq;
using YiSA.WPF.Common;

namespace LocalImageViewer
{
    /// <summary>
    /// ドキュメントデータの状態を扱うクラス
    /// 現在開いているページなどを管理する
    /// </summary>
    public class ImageDocument : DisposableBindable
    {
        public DocumentMetaData MetaData { get; }

        public string DirectoryPath => MetaData.DirectoryAbsolutePath;
        
        public string DisplayName { get; }
        
        public string LargeThumbnailAbsolutePath { get; }
        
        public string SmallThumbnailAbsolutePath { get; }
        
        public string[] Pages { get; private set; }
        
        public LoadStatus LoadStatus { get; private set; }

        private int _currentIndex = 0;

        public ImageDocument(DocumentMetaData metaData , Config config)
        {
            MetaData = metaData;
            DisplayName = MetaData.DisplayName;
            
            var extensions = MetaData.Type.GetFileExtensions();
            Pages = Directory.EnumerateFiles(MetaData.DirectoryAbsolutePath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(x => extensions.Contains(Path.GetExtension(x).ToLower()))
                .OrderBy(x => x, LogicalStringComparer.Instance)
                .ToArray();
            var thumbnailDirectory = Path.Combine(config.ThumbnailDirectory, MetaData.Id.ToString());

            LargeThumbnailAbsolutePath = Path.Combine(thumbnailDirectory, "large.png");
            SmallThumbnailAbsolutePath = Path.Combine(thumbnailDirectory, "small.png");
            metaData.PageSize = Pages.Length;
        }

        /// <summary>
        /// 次のページへ進める
        /// </summary>
        /// <returns></returns>
        public string[] SeekNextPage()
        {
            if (_currentIndex + 2 < Pages.Length)
            {
                return new[]
                {
                    Pages[++_currentIndex],
                    Pages[++_currentIndex]
                };
            }
            else
            {
                _currentIndex = Pages.Length - 1;
                return new[]
                {
                    string.Empty,
                    Pages.Last(),
                };
            }
        }

        /// <summary>
        /// 前のページへ戻す
        /// </summary>
        /// <returns></returns>
        public string[] SeekPrevPage()
        {
            if (_currentIndex - 3 > 0)
            {
                _currentIndex = _currentIndex - 2;
                return new[]
                {
                    Pages[_currentIndex - 1 ],
                    Pages[_currentIndex     ]
                };
            }
            else
            {
                _currentIndex = 0;
                return new[]
                {
                    string.Empty,
                    Pages.First(),
                };
            }
        }

        public string[] Tags()
        {
            return MetaData.Tags;
        }

        public void SetTags(params string[] tags)
        {
            MetaData.IsEdited = true;
            MetaData.Tags = tags;
        }
    }
    
    public enum LoadStatus
    {
        None,
        Loading,
        Loaded,
    }
}