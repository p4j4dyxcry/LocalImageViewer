using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace LocalImageViewer.Foundation
{
    public class VirtualCollectionSource<T,U> : IVirtualCollectionProvider , IDisposable
    {
        private readonly DataSource<T> _dataSource;
        private readonly Func<T, U> _converter;
        private readonly List<T> _proxy;
        private readonly int _initialSize;
        private Func<T, bool> _filter = _=>true;
        private CancellationTokenSource _prevCancelToken;
        private IList<IDisposable> Disposables { get; }= new List<IDisposable>();
        public ObservableCollection<U> Items { get; }
        public int ProxySize => _proxy.Count;
        public int SourceSize => _dataSource.SafeList.Count;
        public event EventHandler CollectionReset;

        public VirtualCollectionSource(DataSource<T> dataSource, Func<T,U> converter ,int initialSize)
        {
            _dataSource = dataSource;
            _converter = converter;
            _proxy = new List<T>(dataSource.SafeList);
            _initialSize = initialSize;
            Items = new ObservableCollection<U>(_proxy.Take(initialSize).Select(converter));

            _dataSource.OnDataSourceCleared += (s, e) =>
            {
                UpdateProxy();
            };

            _dataSource.OnDataSourceManualAdded += (s, e) =>
            {
                if (e.Index is 0)
                {
                    #if false // UX敵にfilterするのが良いかは悩みどころ。
                    T[] filtered = e.Data.Where(_filter).ToArray();]
                    #endif
                    T[] filtered = e.Data.ToArray();
                    if (filtered.Length is not 0)
                    {
                        _proxy.InsertRange(0,filtered);
                        Items.AddRangeHead(filtered.Select(_converter));
                    }
                }
                else
                {
                    UpdateProxy();
                }
            };
        }

        /// <summary>
        /// 仮想テーブルに対するフィルター関数を登録します。
        /// </summary>
        /// <param name="filter"></param>
        public void SetFilter(Func<T, bool> filter)
        {
            if (filter is null)
            {
                _filter = _ => true;
            }
            else
            {
                _filter = filter;
            }
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
                    return _filter(x);
                }).ToArray();
            },cancellationToken);
            
            _proxy.Clear();
            _proxy.AddRange(array);
        }

        private void UpdateProxy()
        {
            var array = _dataSource.SafeList.ToArray();
            array = array.Where(_filter).ToArray();

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

            foreach (var disposable in Items.OfType<IDisposable>())
            {
                disposable.Dispose();
            }

            Items.Reset(_proxy.Take(_initialSize).Select(_converter));

            CollectionReset?.Invoke(this,EventArgs.Empty);
            _prevCancelToken.Dispose();
            _prevCancelToken = null;
        }

        public void ResetCollection()
        {
            UpdateProxy();
            Items.Reset(_proxy.Take(_initialSize).Select(_converter));
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

            Items.AddRange(fixedProxy.Skip(currentIndex).Take(n).Select(_converter));
            return true;
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
        public VirtualCollectionSource(DataSource<T> dataSource, int initialSize) : base(dataSource, x=>x, initialSize)
        {
        }
    }


    public interface IVirtualCollectionProvider
    {
        bool Stage(int step);

        event EventHandler CollectionReset;
    }
}
