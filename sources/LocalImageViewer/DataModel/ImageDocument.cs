using System.Collections.Generic;
using System.IO;
using System.Linq;
using LocalImageViewer.Foundation;
using YiSA.WPF.Common;
namespace LocalImageViewer.DataModel
{
    /// <summary>
    /// ドキュメントデータの状態を扱うクラス
    /// 現在開いているページなどを管理する
    /// </summary>
    public class ImageDocument : DisposableBindable
    {
        public DocumentMetaData MetaData { get; }
        private DocumentMetaData InitialMetaData { get; }
        public bool IsEditedMetaData() => MetaData.IsEdited(InitialMetaData);

        public string DirectoryPath => MetaData.DirectoryAbsolutePath;
        public string DisplayName { get; }
        public string LargeThumbnailAbsolutePath { get; }
        public string SmallThumbnailAbsolutePath { get; }

        private string[] _pages;
        public string[] Pages
        {
            get
            {
                if (_pages is null)
                {
                    LoadPages();
                }
                return _pages;
            }
        }

        public LoadStatus LoadStatus { get; private set; }

        private int _currentIndex = 0;

        public ImageDocument(DocumentMetaData metaData , Config config)
        {
            MetaData = metaData;
            InitialMetaData = metaData.ShallowCopy();
            DisplayName = MetaData.DisplayName;
            
            var thumbnailDirectory = Path.Combine(config.ThumbnailDirectory, MetaData.Id.ToString());

            LargeThumbnailAbsolutePath = Path.Combine(thumbnailDirectory, "large.png");
            SmallThumbnailAbsolutePath = Path.Combine(thumbnailDirectory, "small.png");
        }

        private void LoadPages()
        {
            var extensions = MetaData.Type.GetFileExtensions();

            _pages = Directory.EnumerateFiles(MetaData.DirectoryAbsolutePath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(x => extensions.Contains(Path.GetExtension(x).ToLower()))
                .OrderBy(x => x, LogicalStringComparer.Instance)
                .ToArray();

            MetaData.PageSize = _pages.Length;

            _currentIndex = MetaData.LatestPage;
        }

        /// <summary>
        /// 次のページへ進める
        /// </summary>
        /// <returns></returns>
        public (string page1, string page2) SeekNextPage()
        {
            UpdateAndKeepInRangeCurrentIndex(+2);

            return (GetPageSafe(_currentIndex),GetPageSafe(_currentIndex+1));

        }

        /// <summary>
        /// 前のページへ戻す
        /// </summary>
        /// <returns></returns>
        public (string page1,string page2) SeekPrevPage()
        {
            UpdateAndKeepInRangeCurrentIndex(-2);

            return (GetPageSafe(_currentIndex),GetPageSafe(_currentIndex+1));
        }

        public (string page1, string page2) GetCurrentPage()
        {
            UpdateAndKeepInRangeCurrentIndex(0);

            return (GetPageSafe(_currentIndex),GetPageSafe(_currentIndex+1));

        }

        private string GetPageSafe(int index)
        {
            if (index < 0)
            {
                return string.Empty;
            }
            if (index < Pages.Length)
            {
                return Pages[index];
            }
            return string.Empty;
        }

        private void UpdateAndKeepInRangeCurrentIndex(int increase)
        {
            _currentIndex += increase;

            if (Pages.Length < 2)
            {
                _currentIndex = 0;
            }
            if (_currentIndex < 0)
            {
                _currentIndex = 0;
            }
            if (_currentIndex >= Pages.Length)
            {
                _currentIndex = Pages.Length - 1;

                if (Pages.Length % 2 == 0)
                {
                    _currentIndex -= 1;
                }

            }
            MetaData.LatestPage = _currentIndex;
        }

        private (bool _2orloss, string p1, string p2) ValidPages()
        {
            return Pages.Length switch
            {
                0 => (true, string.Empty, string.Empty),
                1 => (true, Pages[0], string.Empty),
                2 => (true, Pages[0], Pages[1]),
                _ => (false, string.Empty, string.Empty)
            };
        }


        public HashSet<string> GetTags()
        {
            return MetaData.Tags.ToHashSet();
        }

        public void SetTags(params string[] tags)
        {
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
