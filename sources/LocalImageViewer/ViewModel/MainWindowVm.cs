using System.Linq;
using System.Windows.Input;
using LocalImageViewer.DataModel;
using LocalImageViewer.Service;
using LocalImageViewer.WPF;
using Reactive.Bindings;
using YiSA.Foundation.Common.Extensions;
using YiSA.Foundation.Logging;
using YiSA.WPF.Command;
using YiSA.WPF.Common;
namespace LocalImageViewer.ViewModel
{
    public class MainWindowVm : DisposableBindable
    {
        public ReadOnlyReactiveCollection<TagItemVm> Tags { get; }
        public ReadOnlyReactiveCollection<RecentVm> Recent { get; }
        public ReadOnlyReactiveCollection<DocumentVm> Documents { get; }
        public ICommand<string> AddTagCommand { get; }
        public ICommand ShowRenbanEditorCommand { get; }
        public ICommand ShowDocumentCommand { get; }
        
        public ICommand ReloadCommand { get; }
        
        public MainWindowVm(ConfigService configService , Project project ,ThumbnailService thumbnailService , IWindowService windowService , DocumentOperator documentOperator , ILogger logger)
        {
            Tags = configService.Tags.ToReadOnlyReactiveCollection(x => new TagItemVm(x,false)).AddTo(Disposables);
            Recent = configService.Recent.ToReadOnlyReactiveCollection(x => new RecentVm(project.Documents.FirstOrDefault(doc=>doc.MetaData.Id == x))).AddTo(Disposables);
            Documents = project.Documents.ToReadOnlyReactiveCollection(x => new DocumentVm(x,documentOperator,thumbnailService)).AddTo(Disposables);

            // ドキュメントの非同期読み込み
            _ = project.LoadDocumentAsync();
            project.DocumentLoaded += (s, e) =>
            {
                configService.ReloadRecent();
            };
            
            AddTagCommand = new DelegateCommand<string>(configService.AddTag);
            ReloadCommand = new DelegateCommand(()=>_ =project.LoadDocumentAsync());
            
            ShowDocumentCommand = new DelegateCommand<object>( args =>
            {
                var option = new WindowOpenOption()
                {
                    Maximize = true,
                };
                
                if (args is DocumentVm documentVm)
                {
                    windowService.Show<DocumentWindow,DocumentVm>(documentVm,option);
                    configService.AddRecent(documentVm.Document.MetaData.Id);
                }

                if (args is RecentVm recentVm)
                {
                    var context = Documents.FirstOrDefault(x => x.Document.MetaData.Id == recentVm.Document?.MetaData.Id);
                    if(context != null)
                    {
                        windowService.Show<DocumentWindow,DocumentVm>(context,option);
                    }
                }
            });
            
            ShowRenbanEditorCommand = new DelegateCommand( () =>
            {
                var editor = new RenbanDownLoader(project, configService.Config,logger);
                var dataContext = new RenbanVm(editor);
                windowService.Show<RenbanDownloadWindow,RenbanVm>(dataContext,WindowOpenOption.Default);
            });
        }

        private bool TagFilter(ImageDocument document)
        {
            if (Tags.All(x => x.IsEnable.Value is false))
            {
                return true;
            }
            return Tags.Where(x => x.IsEnable.Value)
                       .Any(x => 
                           document.GetTags()
                          .Contains(x.Name));
        }
        
    }
}