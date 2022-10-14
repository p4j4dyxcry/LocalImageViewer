using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LocalImageViewer.Foundation;
using Microsoft.Xaml.Behaviors;
using Reactive.Bindings;
namespace LocalImageViewer.WPF
{
    /// <summary>
    /// マウスホイールイベントの紐づけて仮想コンテナをステージさせるビヘイビアです。
    /// </summary>
    public class LazyLoadBehavior : Behavior<ListBox>
    {
        public static readonly DependencyProperty ProviderProperty = DependencyProperty.Register(
            nameof(Provider), typeof(IVirtualCollectionProvider), typeof(LazyLoadBehavior), new PropertyMetadata(default(IVirtualCollectionProvider)));

        public IVirtualCollectionProvider Provider
        {
            get => (IVirtualCollectionProvider) GetValue(ProviderProperty);
            set => SetValue(ProviderProperty, value);
        }

        private IDisposable _disposable;
        
        protected override void OnAttached()
        {
            base.OnAttached();
            bool initialized = false;
            
            AssociatedObject.Loaded += (_,_1)=>
            {
                if (initialized)
                    return;
                initialized = true;
                // 初期化後,List
                var scrollViewer = AssociatedObject.FindChild<ScrollViewer>();
                
                if (Provider != null)
                    Provider.CollectionReset += (_2, _3) =>
                    {
                        scrollViewer.ScrollToTop();
                    };
                
                _disposable = Observable
                    .FromEventPattern<ScrollChangedEventHandler, ScrollChangedEventArgs>(
                        x => OnScrollViewerOnScrollChanged,
                        x => scrollViewer.ScrollChanged += x,
                        x => scrollViewer.ScrollChanged -= x)
                    .Throttle(TimeSpan.FromMilliseconds(10), UIDispatcherScheduler.Default) // 適度に間引く
                    .Subscribe();

                void OnScrollViewerOnScrollChanged(object s, ScrollChangedEventArgs e)
                {
                    // スクロール割合を計算
                    var scrollRatio = (scrollViewer.VerticalOffset + scrollViewer.ViewportHeight) / scrollViewer.ExtentHeight;

                    if (scrollRatio >= 0.9) // 計算誤差がでることがあるので調整
                    {
                        // 末端あたりまでスクロールした段階で仮想テーブルにデータをロードする
                        Provider?.Stage(6);
                    }
                }
            };
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            _disposable.Dispose();
        }
    }
    
    internal static class Extensions
    {
        public static T FindChild<T>(this FrameworkElement root, Func<FrameworkElement, bool> compare = null) 
            where T : FrameworkElement
        {
            compare ??= (x => true);
            
            var children = Enumerable.Range(0, VisualTreeHelper.GetChildrenCount(root)).Select(x => VisualTreeHelper.GetChild(root, x)).OfType<FrameworkElement>().ToArray();

            foreach (var child in children)
            {
                if (child is T t && compare(child))
                    return t;

                if (child.FindChild<T>(compare) is T success)
                    return success;
            }
            return null;
        }    
    }

}
