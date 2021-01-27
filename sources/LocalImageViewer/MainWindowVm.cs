using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Timers;
using System.Windows.Data;
using System.Windows.Input;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using YamlDotNet.Core.Tokens;
using YiSA.Foundation.Common.Extensions;
using YiSA.WPF.Command;
using YiSA.WPF.Common;

namespace LocalImageViewer
{
    public class MainWindowVm : DisposableBindable
    {
        public ReadOnlyReactiveCollection<TagItemVm> Tags { get; }
        public ReadOnlyReactiveCollection<RecentVm> Recent { get; }
        public ReadOnlyReactiveCollection<DocumentVm> Documents { get; }
        public ICommand<string> AddTagCommand { get; }
        public ICommand ShowRenbanEditorCommand { get; }
        
        public ICommand ShowDocumentCommand { get; }
        
        public MainWindowVm(ConfigService configService , Project project ,ThumbnailService thumbnailService , IWindowService windowService , DocumentOperator documentOperator)
        {
            Tags = configService.Tags.ToReadOnlyReactiveCollection(x => new TagItemVm(x,false)).AddTo(Disposables);
            Recent = configService.Recent.ToReadOnlyReactiveCollection(x => new RecentVm(project.Documents.FirstOrDefault(doc=>doc.MetaData.Id == x))).AddTo(Disposables);
            Documents = project.Documents.ToReadOnlyReactiveCollection(x => new DocumentVm(x,documentOperator,thumbnailService)).AddTo(Disposables);
            
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

        private bool TagFilter(ImageDocument document)
        {
            if (Tags.All(x => x.IsEnable.Value is false))
                return true;
            return Tags.Where(x => x.IsEnable.Value)
                       .Any(x => 
                           document.GetTags()
                          .Contains(x.Name));
        }
        
    }
}