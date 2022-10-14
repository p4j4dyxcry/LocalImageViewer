using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
namespace LocalImageViewer.Foundation
{
    public static class ObservableCollectionExtensions
    {
        public static void AddRange<T>(this ObservableCollection<T> source, IEnumerable<T> collection)
        {
            var enumerable = collection as T[] ?? collection.ToArray();

            int reflectionThreshold = 25;

            if (enumerable.Length < reflectionThreshold)
            {
                foreach (var item in enumerable)
                {
                    source.Add(item);
                }
                return;
            }
            List<T> items = source.InternalItems();
            items.AddRange(enumerable);
            source.InternalOnCountPropertyChanged();
            source.InternalOnIndexerPropertyChanged();
            source.InternalOnCollectionReset();
        }

        public static void AddRangeHead<T>(this ObservableCollection<T> source, IEnumerable<T> collection)
        {
            var enumerable = collection as T[] ?? collection.ToArray();

            int reflectionThreshold = 25;

            if (enumerable.Length < reflectionThreshold)
            {
                foreach (var item in enumerable.Reverse())
                {
                    source.Insert(0,item);
                }
                return;
            }
            List<T> items = source.InternalItems();
            items.InsertRange(0,enumerable);
            source.InternalOnCountPropertyChanged();
            source.InternalOnIndexerPropertyChanged();
            source.InternalOnCollectionReset();
        }

        public static void Reset<T>(this ObservableCollection<T> source, IEnumerable<T> collection)
        {
            List<T> items = source.InternalItems();
            items.Clear();
            items.AddRange(collection);
            source.InternalOnCountPropertyChanged();
            source.InternalOnIndexerPropertyChanged();
            source.InternalOnCollectionReset();
        }

        private static void InternalOnCollectionReset<T>(this ObservableCollection<T> collection)
        {
            var onCollectionResetMethod = typeof(ObservableCollection<T>).GetMethod("OnCollectionReset", BindingFlags.NonPublic | BindingFlags.Instance);
            onCollectionResetMethod?.Invoke(collection, null);
        }

        private static void InternalOnCountPropertyChanged<T>(this ObservableCollection<T> collection)
        {
            var onCountPropertyChanged = typeof(ObservableCollection<T>).GetMethod("OnCountPropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            onCountPropertyChanged?.Invoke(collection, null);
        }

        private static void InternalOnIndexerPropertyChanged<T>(this ObservableCollection<T> collection)
        {
            var onIndexerPropertyChanged = typeof(ObservableCollection<T>).GetMethod("OnIndexerPropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            onIndexerPropertyChanged?.Invoke(collection, null);
        }

        private static List<T> InternalItems<T>(this ObservableCollection<T> collection)
        {
            var itemsProperty = typeof(ObservableCollection<T>).GetProperty("Items", BindingFlags.NonPublic | BindingFlags.Instance);
            return itemsProperty?.GetValue(collection) as List<T>;
        }
    }
}
