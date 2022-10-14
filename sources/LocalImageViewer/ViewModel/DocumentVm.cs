using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using LocalImageViewer.DataModel;
using LocalImageViewer.Service;
using Reactive.Bindings.Extensions;
using YiSA.WPF.Command;
using YiSA.WPF.Common;
namespace LocalImageViewer.ViewModel
{
    public class DocumentVm : DisposableBindable
    {
        public ImageDocument Document { get; }
        private readonly DocumentOperator _documentOperator;
        private readonly ThumbnailService _thumbnailService;

        private string _largeThumbnail;
        public string LargeThumbnailPath
        {
            get => GetThumbnailPath();
            private set => TrySetProperty(ref _largeThumbnail, value);
        }

        private string _page1;
        public string Page1
        {
            get => _page1 ??= Document.GetCurrentPage().page1;
            private set => TrySetProperty(ref _page1, value);
        }

        private string _page2;
        public string Page2
        {
            get => _page2 ??= Document.GetCurrentPage().page2;
            private set => TrySetProperty(ref _page2, value);
        }

        public TagEditorVm TagEditorVm { get; }
        
        public ICommand ToNextCommand { get; }
        public ICommand ToPrevCommand { get; }
        public ICommand ShowWithExplorerCommand { get; }

        public string DisplayName { get; }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get => _isVisible;
            set => TrySetProperty(ref _isVisible, value);
        }
        public DocumentVm(ImageDocument document,DocumentOperator documentOperator,ThumbnailService thumbnailService,bool syncTag)
        {
            Document = document;
            _documentOperator = documentOperator;
            _thumbnailService = thumbnailService;
            DisplayName = document.DisplayName;

            ToNextCommand = new DelegateCommand(() =>
            {
                var (p1,p2) = document.SeekNextPage();
                Page1 = p1;
                Page2 = p2;
            });
            
            ToPrevCommand = new DelegateCommand(() =>
            {
                var (p1,p2) = document.SeekPrevPage();
                Page1 = p1;
                Page2 = p2;
            });

            ShowWithExplorerCommand = new DelegateCommand(() =>
            {
                _documentOperator.OpenWithExplorer(document);
            });

            TagEditorVm = _documentOperator.BuildTagVm(document,syncTag)
                .AddTo(Disposables);
        }

        private string GetThumbnailPath()
        {
            if (_largeThumbnail is null)
            {
                if (File.Exists(Document.LargeThumbnailAbsolutePath))
                {
                    _largeThumbnail = Document.LargeThumbnailAbsolutePath;
                }
                else
                {
                    _largeThumbnail = "Resources/loading.png";
                    _ = GetTitleAsync();                        
                }
            }

            return _largeThumbnail;
        }
        
        private async Task GetTitleAsync()
        {
            await _thumbnailService.CreateThumbnail(Document);
            LargeThumbnailPath = Document.LargeThumbnailAbsolutePath;
        }

        public DocumentVm Clone()
        {
            return new DocumentVm(Document, _documentOperator, _thumbnailService,false);
        }
    }
}
