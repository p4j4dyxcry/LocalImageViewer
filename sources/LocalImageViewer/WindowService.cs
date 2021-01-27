using System.Threading.Tasks;
using System.Windows;

namespace LocalImageViewer
{
    /// <summary>
    /// Windowを開く処理を抽象化させるinterface
    /// </summary>
    public interface IWindowService
    {
        public void Show<T, TViewModel>(TViewModel dataContext,WindowOpenOption option)
            where T : Window, new();
    }
    
    /// <summary>
    /// ウィンドウ、ダイアログを開く クラス
    /// </summary>
    public class WindowService : IWindowService
    {
        public async Task ShowAsync<T,TViewModel>(TViewModel dataContext ,WindowOpenOption option)
            where T : Window , new ()
        {
            var window = new T()
            {
                DataContext = dataContext,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };

            if (option.Maximize)
                window.WindowState = WindowState.Maximized;

            if (option.Width > 0)
                window.Width = option.Width;
            if (option.Height > 0)
                window.Height = option.Height;
                    
            // wait & focus
            await Task.Delay(1);
            if (option.IsModal)
            {
                window.ShowDialog();
            }
            else
            {
                window.Show();
                await Task.Delay(1);
                window.Focus();                
            }
        }
        
        public async void Show<T, TViewModel>(TViewModel dataContext,WindowOpenOption option)
            where T : Window , new ()
        {
            await ShowAsync<T, TViewModel>(dataContext ,option);
        }
    }
    
    public class WindowOpenOption
    {
        public static WindowOpenOption Default { get; } = new WindowOpenOption();
        
        public bool Maximize { get; set; } = false;

        public bool AutoFocus { get; set; } = true;

        public bool IsModal { get; set; } = false;

        public double Width { get; set; } = -1;
        public double Height { get; set; } = -1;
    }
}