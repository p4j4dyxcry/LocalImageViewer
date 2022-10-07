using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
namespace LocalImageViewer.Foundation
{
    public class VirtualCollectionSource<T,U> : IVirtualCollectionProvider , IDisposable
    {
        private readonly DataSource<T> _dataSource;
        private readonly List<T> _proxy;
        private readonly int _initialSize;
        private readonly Subject<CollectionChanged<T>> _collectionChangedTrigger;
        private Func<T, bool> _filter;
        private CancellationTokenSource _prevCancelToken;
        private IList<IDisposable> Disposables { get; }= new List<IDisposable>();
        public ReadOnlyReactiveCollection<U> Items { get; }
        public int ProxySize => _proxy.Count;
        public int SourceSize => _dataSource.SafeList.Count;
        public event EventHandler CollectionReset;

        public VirtualCollectionSource(DataSource<T> dataSource, Func<T,U> converter ,int initialSize , IScheduler scheduler)
        {
            _dataSource = dataSource;
            _dataSource.OnManualAdded += async (s, e) => await ResetCollectionAsync();
            _proxy = new List<T>(dataSource.SafeList);
            _initialSize = initialSize;

            var source = _proxy.Take(initialSize).ToArray();
            _collectionChangedTrigger = new Subject<CollectionChanged<T>>().AddTo(Disposables);
            Items = source.ToReadOnlyReactiveCollection(_collectionChangedTrigger,converter,scheduler, false ).AddTo(Disposables);
        }

        /// <summary>
        /// 仮想テーブルに対するフィルター関数を登録します。
        /// </summary>
        /// <param name="filter"></param>
        public void SetFilter(Func<T, bool> filter)
        {
            _filter = filter;
        }

        /// <summary>
        /// データソース→プロキシーへデータを同期させます。
        /// データソースの列挙とフィルターは別スレッドで行われます。
        /// </summary>
        /// <returns></returns>
        private async Task UpdateProxyAsync(CancellationToken cancellationToken)
        {
            var array = Array.Empty<T>();
            await Task.Run(() =>
            {
                array = _dataSource.SafeList.ToArray();
                array = array.Where(x =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return _filter?.Invoke(x) ?? true;
                }).ToArray();
            },cancellationToken);
            
            _proxy.Clear();
            _proxy.AddRange(array);
        }

        private void UpdateProxy()
        {
            var array = _dataSource.SafeList.ToArray();
            array = array.Where(x => _filter?.Invoke(x) ?? true).ToArray();

            _proxy.Clear();
            _proxy.AddRange(array);
        }
        
        /// <summary>
        /// ステージされたデータをすべて破棄し、Itemsを再構築します。
        /// 初期要素数はinitialSizeになります。
        /// </summary>
        /// <returns></returns>
        public async Task ResetCollectionAsync()
        {
            if (_prevCancelToken != null)
            {
                _prevCancelToken.Cancel();
                _prevCancelToken.Dispose();
            }
            try
            {
                _prevCancelToken = new CancellationTokenSource();
                await UpdateProxyAsync(_prevCancelToken.Token);
            }
            catch(TaskCanceledException)
            {
                return;
            }

            _collectionChangedTrigger.OnNext(CollectionChanged<T>.Reset);
            int i = 0;
            foreach (var item in _proxy.Take(_initialSize).ToArray())
            {
                _collectionChangedTrigger.OnNext(CollectionChanged<T>.Add(i++,item));
            }

            CollectionReset?.Invoke(this,EventArgs.Empty);
            _prevCancelToken.Dispose();
            _prevCancelToken = null;
        }


        /// <summary>
        /// Proxyからn分のデータをItemsにステージします。
        /// </summary>
        /// <param name="n"></param>
        /// <returns>追加があればtrue</returns>
        public bool Stage(int n)
        {
            var currentIndex = Items.Count;
            var fixedProxy = _proxy.ToArray();
            if (fixedProxy.Length <= currentIndex + n)
            {
                UpdateProxy();
            }
            var result = false;
            for (int i = 0; i < n; ++i)
            {
                var index = currentIndex + i;
                if(fixedProxy.Length <= index )
                {
                    break;
                }
                result = true;
                _collectionChangedTrigger.OnNext(
                    CollectionChanged<T>.Add(index,fixedProxy[index]));
            }

            return result;
        }
        public void Dispose()
        {
            foreach (var disposable in Disposables)
            {
                disposable.Dispose();
            }
        }
    }
    public class VirtualCollectionSource<T> : VirtualCollectionSource<T,T>
    {
        public VirtualCollectionSource(DataSource<T> dataSource, int initialSize, IScheduler scheduler) : base(dataSource, x=>x, initialSize, scheduler)
        {
        }
    }


    public interface IVirtualCollectionProvider
    {
        bool Stage(int step);

        event EventHandler CollectionReset;
    }
}
