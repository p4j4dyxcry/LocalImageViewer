using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Xaml.Behaviors;

namespace LocalImageViewer
{
    /// <summary>
    /// ボタンクリックでポップアップさせるビヘイビア
    /// </summary>
    public class ButtonClickPopupBehavior : Behavior<Button>
    {
        /// <summary>
        /// ポップアップ対象
        /// </summary>
        public static readonly DependencyProperty PopupProperty = DependencyProperty.Register(
            "Popup", typeof(Popup), typeof(ButtonClickPopupBehavior), new PropertyMetadata(default(Popup)));

        public Popup Popup
        {
            get => (Popup) GetValue(PopupProperty);
            set => SetValue(PopupProperty, value);
        }

        /// <summary>
        /// 配置位置
        /// </summary>
        public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
            "Placement", typeof(PlacementMode), typeof(ButtonClickPopupBehavior), new PropertyMetadata(PlacementMode.Bottom));

        public PlacementMode Placement
        {
            get => (PlacementMode) GetValue(PlacementProperty);
            set => SetValue(PlacementProperty, value);
        }
        
        protected override void OnAttached()
        {
            base.OnAttached();

            if (Popup?.Child is IPopupBinder binder)
            {
                binder.Bind(Popup);
            }
            
            // クリックでポップアップさせる
            AssociatedObject.Click += async (s, e) =>
            {
                if (Popup is {})
                {
                    Popup.PlacementTarget = AssociatedObject;
                    Popup.Placement = Placement;
                    
                    //TODO: 必要があれば依存プロパティに逃がす
                    Popup.StaysOpen = false;

                    // 即時呼び出しだと反映されないので意図的に遅延させる
                    await Task.Delay(1);
                    Popup.IsOpen = true;
                }
            };
        }
    }
    
    /// <summary>
    /// ポップアップ時に関連付けを行うインタフェース
    /// </summary>
    public interface IPopupBinder
    {
        void Bind(Popup popup);
    }
}