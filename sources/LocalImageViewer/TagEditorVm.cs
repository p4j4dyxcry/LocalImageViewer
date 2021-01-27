using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Input;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using YiSA.WPF.Command;
using YiSA.WPF.Common;

namespace LocalImageViewer
{
    public class TagEditorVm : DisposableBindable
    {
        public IEnumerable<TagItemVm> Tags { get; }
        
        public ICommand ApplyCommand { get; }
        
        public TagEditorVm(ImageDocument imageDocument , ConfigService configService)
        {
            var documentTags = imageDocument.GetTags();

            Tags = configService.Tags.ToReadOnlyReactiveCollection(x => new TagItemVm(x, documentTags.Contains(x)))
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

        public TagItemVm(string tag , bool enabled)
        {
            Name = tag;
            IsEnable = new ReactiveProperty<bool>(enabled).AddTo(Disposables);
        }
    }
}