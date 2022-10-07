using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using LocalImageViewer.DataModel;
using Reactive.Bindings;
using YiSA.Foundation.Common.Extensions;
using YiSA.Foundation.Logging;
using YiSA.WPF.Common;
namespace LocalImageViewer.Service
{
    public class TagData : Bindable
    {
        public TagData(string tag,bool isEnabled = false)
        {
            Tag = tag;
            IsEnabled = isEnabled;
        }
        public string Tag { get; }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => TrySetProperty(ref _isEnabled,value);
        }
    }
    
    public class ConfigService : DisposableHolder
    {
        private readonly Config _config;
        private readonly ILogger _logger;
        private readonly ObservableCollection<Guid> _recent;
        private readonly ObservableCollection<TagData> _tags;

        public Config Config => _config;
        public ReadOnlyReactiveCollection<Guid> Recent { get; }
        public ReadOnlyReactiveCollection<TagData> Tags { get; }

        public ConfigService(Config config, ILogger logger)
        {
            _config = config;
            _logger = logger;
            _tags = new ObservableCollection<TagData>(config.GetTagDataList());
            
            _recent = new ObservableCollection<Guid>(config.Recent);

            Recent = _recent.ToReadOnlyReactiveCollection(x=>x).AddTo(Disposables);
            Tags = _tags.ToReadOnlyReactiveCollection().AddTo(Disposables);
            
            // クラス破棄時に編集データを設定データに反映させる
            Disposables.Add(Disposable.Create(ApplyToConfig));
        }

        public void AddTag(string tag )
        {
            _logger.WriteLine($"add tag {tag}");
            if (_tags.Select(x=>x.Tag).Contains(tag) is false)
            {
                _tags.Add(new TagData(tag));
            }
        }
        
        public void AddRecent(Guid id )
        {
            _logger.WriteLine($"add recent {id}");
            if (_recent.Contains(id))
            {
                _recent.Remove(id);
            }
            _recent.Insert(0,id);

            const int  max = 9;
            if(_recent.Count > max)
            {
                _recent.RemoveAt(max);
            }
        }

        public void ReloadRecent()
        {
            var prev = _recent.ToArray();
            _recent.Clear();

            foreach (var p in prev)
                _recent.Add(p);
        }
        
        public void RemoveTag(string tag )
        {
            var data = _tags.FirstOrDefault(x => x.Tag == tag);
            if (data is not null)
            {
                _tags.Remove(data);
            }
        }

        private void ApplyToConfig()
        {
            Config.Recent = _recent.ToArray();
            Config.Tags = _tags.Select(x=>x.Tag).ToArray();
            Config.EnabledTags = _tags.Where(x => x.IsEnabled).Select(x => x.Tag).ToArray();
        }
    }
}