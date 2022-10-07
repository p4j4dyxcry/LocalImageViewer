using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Windows.Input;
using LocalImageViewer.DataModel;
using LocalImageViewer.Foundation;
using LocalImageViewer.Service;
using LocalImageViewer.WPF;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
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
        public VirtualCollectionSource<ImageDocument,DocumentVm> DocumentSource { get; }

        public ReadOnlyReactiveCollection<DocumentVm> Documents => DocumentSource.Items;
        public ICommand<string> AddTagCommand { get; }
        public ICommand ShowRenbanEditorCommand { get; }
        public ICommand ShowDocumentCommand { get; }
        public ICommand ShowTagEditorCommand { get; }
        public ICommand ReloadCommand { get; }
        
        public MainWindowVm(ConfigService configService , Project project ,ThumbnailService thumbnailService , IWindowService windowService , DocumentOperator documentOperator,ImageDocumentFilterService imageDocumentFilterService , ILogger logger)
        {
            // ドキュメントの非同期読み込み
            _ = project.LoadDocumentAsync(30);

            project.DocumentLoaded += (s, e) =>
            {
                configService.ReloadRecent();
            };

            DocumentSource = new VirtualCollectionSource<ImageDocument, DocumentVm>(project.DocumentSource,x => new DocumentVm(x, documentOperator, thumbnailService,false),30,DispatcherScheduler.Current);
            DocumentSource.SetFilter(imageDocumentFilterService.Filter);

            Tags = configService.Tags.ToReadOnlyReactiveCollection(x =>
            {
                var result = new TagItemVm(x);

                result.IsEnable.PropertyChangedAsObservable()
                    .Subscribe( x =>
                    {
                        _ = DocumentSource.ResetCollectionAsync();
                    }).AddTo(Disposables);
                return result;
            }).AddTo(Disposables);
            
            Recent = configService.Recent.ToReadOnlyReactiveCollection(x => new RecentVm(project.Documents.FirstOrDefault(doc=>doc.MetaData.Id == x))).AddTo(Disposables);

            AddTagCommand = new DelegateCommand<string>(configService.AddTag);
            ReloadCommand = new DelegateCommand(()=>_ =project.LoadDocumentAsync(20));
            
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

            ShowTagEditorCommand = new DelegateCommand(() =>
            {
                TagEditWindowVm vm = new TagEditWindowVm(project.DocumentSource,documentOperator,thumbnailService);
                windowService.Show<TagEditWindow,TagEditWindowVm>(vm,WindowOpenOption.Default);
            });
            
            ShowRenbanEditorCommand = new DelegateCommand( () =>
            {
                var editor = new RenbanDownLoader(project, configService.Config,logger);
                var dataContext = new RenbanVm(editor);
                windowService.Show<RenbanDownloadWindow,RenbanVm>(dataContext,WindowOpenOption.Default);
            });
        }
    }
}
