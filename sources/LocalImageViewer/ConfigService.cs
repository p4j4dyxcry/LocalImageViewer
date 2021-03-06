using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Input;
using Reactive.Bindings;
using YiSA.Foundation.Common.Extensions;
using YiSA.WPF.Common;

namespace LocalImageViewer
{
    public class ConfigService : DisposableHolder
    {
        private readonly Config _config;
        private readonly ObservableCollection<Guid> _recent;
        private readonly ObservableCollection<string> _tags;

        public Config Config => _config;
        public ReadOnlyReactiveCollection<Guid> Recent { get; }
        public ReadOnlyReactiveCollection<string> Tags { get; }

        public ConfigService(Config config)
        {
            _config = config;
            _tags = new ObservableCollection<string>(config.Tags);
            _recent = new ObservableCollection<Guid>(config.Recent);

            Recent = _recent.ToReadOnlyReactiveCollection(x=>x).AddTo(Disposables);
            Tags = _tags.ToReadOnlyReactiveCollection().AddTo(Disposables);
            
            // クラス破棄時に編集データを設定データに反映させる
            Disposables.Add(Disposable.Create(ApplyToConfig));
        }

        public void AddTag(string tag )
        {
            if (_tags.Contains(tag) is false)
                _tags.Add(tag);
        }
        
        public void AddRecent(Guid id )
        {
            if (_recent.Contains(id))
                _recent.Remove(id);
            _recent.Insert(0,id);

            const int  max = 9;
            if(_recent.Count > max)
                _recent.RemoveAt(max);
        }
        
        public void RemoveTag(string tag )
        {
            _tags.Remove(tag);
        }

        private void ApplyToConfig()
        {
            Config.Recent = _recent.ToArray();
            Config.Tags = _tags.ToArray();
        }
    }
}