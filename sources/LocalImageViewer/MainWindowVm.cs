using System.Linq;
using System.Windows.Input;
using Reactive.Bindings;
using YiSA.Foundation.Common.Extensions;
using YiSA.WPF.Command;
using YiSA.WPF.Common;

namespace LocalImageViewer
{
    public class MainWindowVm : DisposableBindable
    {
        public ReadOnlyReactiveCollection<TagVm> Tags { get; }
        public ReadOnlyReactiveCollection<RecentVm> Recent { get; }
        public ReadOnlyReactiveCollection<DocumentVm> Documents { get; }
        public ICommand<string> AddTagCommand { get; }
        public ICommand ShowRenbanEditorCommand { get; }
        
        public ICommand ShowDocumentCommand { get; }
        
        public MainWindowVm(ConfigService configService , Project project ,ThumbnailService thumbnailService , IWindowService windowService)
        {
            Tags = configService.Tags.ToReadOnlyReactiveCollection(x => new TagVm(x)).AddTo(Disposables);
            Documents = project.Documents.ToReadOnlyReactiveCollection(x => new DocumentVm(x,thumbnailService)).AddTo(Disposables);
            Recent = configService.Recent.ToReadOnlyReactiveCollection(x => new RecentVm(project.Documents.FirstOrDefault(doc=>doc.MetaData.Id == x))).AddTo(Disposables);

            AddTagCommand = new DelegateCommand<string>(x => configService.AddTag(x));

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
                        windowService.Show<DocumentWindow,DocumentVm>(context,option);                   
                }
            });
            
            ShowRenbanEditorCommand = new DelegateCommand( () =>
            {
                var editor = new RenbanDownLoader(project, configService.Config);
                var dataContext = new RenbanVm(editor);
                windowService.Show<RenbanDownloadWindow,RenbanVm>(dataContext,WindowOpenOption.Default);
            });
        }
    }
}