using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using LocalImageViewer.DataModel;
using LocalImageViewer.Foundation;
using LocalImageViewer.Service;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using YiSA.WPF.Common;
namespace LocalImageViewer.ViewModel
{
    public class TagEditWindowVm : DisposableBindable
    {
        public VirtualCollectionSource<ImageDocument,DocumentVm> DocumentSource { get; }
        public ObservableCollection<DocumentVm> FilteredDocuments => DocumentSource.Items;
        public ReactivePropertySlim<bool> IsNoTagOnly { get; }

        public TagEditWindowVm(DataSource<ImageDocument> dataSource,DocumentOperator documentOperator,ThumbnailService thumbnailService)
        {
            DocumentSource = new(dataSource, x => new DocumentVm(x, documentOperator, thumbnailService, true),20);
            IsNoTagOnly = new ReactivePropertySlim<bool>(true).AddTo(Disposables);
            DocumentSource.SetFilter(x=>!IsNoTagOnly.Value || !x.MetaData.Tags.Any());

            IsNoTagOnly
                .Subscribe(x=> _ = DocumentSource.ResetCollectionAsync())
                .AddTo(Disposables);
        }
    }
}
