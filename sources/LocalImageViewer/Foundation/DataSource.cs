using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YiSA.Foundation.Internal;
namespace LocalImageViewer.Foundation
{
    public class DataSourceUpdatedEventArgs<T> : EventArgs
    {
        public DataSourceUpdatedEventArgs(int index, T[] data)
        {
            Index = index;
            Data = data;
        }
        public int Index { get; }
        public T[] Data { get; }
    }

    public class DataSource<T> : IDisposable
    {
        public IReadOnlyList<T> Items => _list;
        public IReadOnlyList<T> SafeList
        {
            get
            {
                using var _ = _locker.ReadLock();
                return _list.ToArray();
            }
        }

        private readonly List<T> _list = new();
        private readonly ConcurrentQueue<IEnumerable<T>> _queue = new();
        private CancellationTokenSource _cancellationTokenSource = new();
        private readonly SlimLocker _locker = new();
        public bool IsLoading { get; private set; }

        public event EventHandler<DataSourceUpdatedEventArgs<T>> OnDataSourceManualAdded;
        public event EventHandler OnDataSourceCleared;
        /// <summary>
        /// データの非同期読み取りを開始します。
        /// enumeratorはワーカースレッドから列挙されます。
        /// </summary>
        /// <returns></returns>
        public async Task ReadAsync(IEnumerable<T> enumerator, int initial)
        {
            _queue.Enqueue(enumerator);
            if (IsLoading)
            {
                return;
            }

            using var loading = StartLoading();

            _queue.TryDequeue(out var currentEnumerator);
            if (initial > 0)
            {
                using var _ = _locker.WriteLock();
                int take = 0;
                var enumerable = currentEnumerator as T[] ?? currentEnumerator?.ToArray() ?? ArraySegment<T>.Empty;
                foreach (var data in enumerable)
                {
                    if (_list.Count < initial)
                    {
                        ++take;
                        _list.Add(data);
                    }
                }
                currentEnumerator = enumerable.Skip(take);
            }

            var cancelToken = _cancellationTokenSource.Token;
            await Task.Run(() =>
            {
                do
                {
                    if (currentEnumerator is null)
                    {
                        return;
                    }

                    foreach (var data in currentEnumerator)
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            break;
                        }
                        using var _ = _locker.WriteLock();
                        _list.Add(data);
                    }
                    if (cancelToken.IsCancellationRequested)
                    {
                        _queue.Clear();
                        break;
                    }
                }
                while (_queue.TryDequeue(out currentEnumerator));
            }, cancelToken);
        }

        public void Clear()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            using var _ = _locker.WriteLock();
            _list.Clear();
        }

        public void AddRange(params T[] items)
        {
            using var _ = _locker.WriteLock();
            int index = _list.Count - 1;
            _list.AddRange(items);
            OnDataSourceManualAdded?.Invoke(this, new DataSourceUpdatedEventArgs<T>(index, items));
        }

        public void InsertRangeHead(params T[] items)
        {
            using var _ = _locker.WriteLock();
            _list.InsertRange(0, items);
            OnDataSourceManualAdded?.Invoke(this, new DataSourceUpdatedEventArgs<T>(0, items));
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }

        private IDisposable StartLoading()
        {
            IsLoading = true;
            return new DelegateDisposable(() => IsLoading = false);
        }
    }
}
