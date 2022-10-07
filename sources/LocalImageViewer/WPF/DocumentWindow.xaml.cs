using System.Windows.Input;
using LocalImageViewer.ViewModel;
namespace LocalImageViewer.WPF
{
    public partial class DocumentWindow
    {
        public DocumentWindow()
        {
            InitializeComponent();

            this.PreviewKeyDown += (s, e) =>
            {
                ToPage(e.Key);
            };

            this.PreviewMouseWheel += (s, e) =>
            {
                ToPage(e.Delta > 0 ? Key.Left : Key.Right);
            };
            
            this.PreviewMouseDown += (s, e) =>
            {
                if(e.LeftButton == MouseButtonState.Pressed)
                {
                    ToPage(Key.Right);
                }
            };
        }

        private void ToPage(Key key)
        {
            if (DataContext is DocumentVm documentVm)
            {
                if(key == Key.Left)
                {
                    documentVm.ToPrevCommand?.Execute(this);
                }
                else if(key == Key.Right)
                {
                    documentVm.ToNextCommand?.Execute(this);
                }
            }
        }
    }
}