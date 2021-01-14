using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Reactive.Bindings;
using YiSA.WPF.Command;
using YiSA.WPF.Common;

namespace LocalImageViewer
{
    public class DocumentVm : DisposableBindable
    {
        public ImageDocument Document { get; }
        private readonly ThumbnailService _thumbnailService;

        private string _largeThumbnail;
        public string LargeThumbnailPath
        {
            get
            {
                if (_largeThumbnail is null)
                {
                    if (File.Exists(Document.LargeThumbnailAbsolutePath))
                        _largeThumbnail = Document.LargeThumbnailAbsolutePath;
                    else
                    {
                        _largeThumbnail = "Resources/loading.png";
                        _ = GetTitleAsync();                        
                    }
                }

                return _largeThumbnail;
            }
            private set => TrySetProperty(ref _largeThumbnail, value);
        }
        
        public IReactiveProperty<string> Page1 { get; } = new ReactiveProperty<string>();
        public IReactiveProperty<string> Page2 { get; } = new ReactiveProperty<string>();
        
        public ICommand ToNextCommand { get; }
        public ICommand ToPrevCommand { get; }
        
        public ICommand ShowWithExplorerCommand { get; }

        public string DisplayName { get; }

        public DocumentVm(ImageDocument document,ThumbnailService thumbnailService)
        {
            Document = document;
            _thumbnailService = thumbnailService;
            DisplayName = document.DisplayName;
            Page1.Value = document.Pages[0];

            ToNextCommand = new DelegateCommand(() =>
            {
                var tuple = document.SeekNextPage();
                Page1.Value = tuple[0];
                Page2.Value = tuple[1];
            });
            
            ToPrevCommand = new DelegateCommand(() =>
            {
                var tuple = document.SeekPrevPage();
                Page1.Value = tuple[0];
                Page2.Value = tuple[1];
            });

            ShowWithExplorerCommand = new DelegateCommand(() =>
            {
                Process.Start("explorer", document.DirectoryPath);
            });
        }
        private async Task GetTitleAsync()
        {
            await _thumbnailService.CreateThumbnail(Document);
            LargeThumbnailPath = Document.LargeThumbnailAbsolutePath;
        }
    }
}