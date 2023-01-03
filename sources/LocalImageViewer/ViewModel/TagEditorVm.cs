using System;
using System.Linq;
using System.Windows.Input;
using LocalImageViewer.DataModel;
using LocalImageViewer.Service;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using YiSA.WPF.Command;
using YiSA.WPF.Common;
namespace LocalImageViewer.ViewModel
{
    public class TagEditorVm : DisposableBindable
    {
        public ReadOnlyReactiveCollection<TagItemVm> Tags { get; }
        
        public ICommand ApplyCommand { get; }
        
        public TagEditorVm(ImageDocument imageDocument , ConfigService configService,bool autoTagsSync)
        {
            var documentTags = imageDocument.GetTags();

            Tags = configService.Tags.ToReadOnlyReactiveCollection(x =>
                {
                    var tag = new TagData(x.Tag, documentTags.Contains(x.Tag));
                    var result = new TagItemVm(tag);

                    if (autoTagsSync)
                    {
                        tag.PropertyChangedAsObservable()
                            .Subscribe(x=>ApplyCommand?.Execute(null)).AddTo(Disposables);
                    }

                    return result;
                })
                .AddTo(Disposables);

            ApplyCommand = new DelegateCommand(() =>
            {
                imageDocument.SetTags(
                    Tags.Where(x=>x.IsEnable.Value)
                        .Select(x=>x.Name)
                        .ToArray());
            });
        }
    }

    public class TagItemVm : DisposableBindable
    {
        public string Name { get; }
        
        public IReactiveProperty<bool> IsEnable { get; }

        public TagItemVm(TagData tagData)
        {
            Name = tagData.Tag;
            IsEnable = tagData.ToReactivePropertyAsSynchronized(x=>x.IsEnabled).AddTo(Disposables);
        }
    }
}
