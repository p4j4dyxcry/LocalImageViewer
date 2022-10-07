using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
namespace LocalImageViewer.WPF
{
    /// <summary>
    /// TextBoxと追加ボタンの組み合わせ
    /// エンターキーで確定できる
    /// </summary>
    public partial class TextBoxAddButtonControl : IPopupBinder
    {
        public static readonly DependencyProperty ButtonNameProperty = DependencyProperty.Register(
            "ButtonName", typeof(string), typeof(TextBoxAddButtonControl), new PropertyMetadata("追加"));

        public string ButtonName
        {
            get => (string) GetValue(ButtonNameProperty);
            set => SetValue(ButtonNameProperty, value);
        }
        
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof(ICommand), typeof(TextBoxAddButtonControl), new PropertyMetadata(default(ICommand)));

        public ICommand Command
        {
            get => (ICommand) GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        
        public TextBoxAddButtonControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ポップアップに追加された場合の機能連携
        /// </summary>
        /// <param name="popup"></param>
        public void Bind(Popup popup)
        {
            // ポップアップ時にオートフォーカスする
            popup.Opened += async (s, e) =>
            {
                AddTagPopupTextBox.Text = string.Empty;
                
                // 即時呼び出しだと反映されないことがあるので意図的に遅延させる
                await Task.Delay(1);
                AddTagPopupTextBox.Focus();
            };
            
            AddButton.Click += (s, e) =>
            {
                // ボタンをクリックした段階でポップアップを閉じる
                popup.IsOpen = false;
            };
        }
    }
}