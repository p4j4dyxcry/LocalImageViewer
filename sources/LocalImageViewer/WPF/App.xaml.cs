using System.Windows;
using LocalImageViewer.DataModel;
using LocalImageViewer.Service;
using LocalImageViewer.ViewModel;
using SimpleInjector;
using YiSA.Foundation.Logging;
namespace LocalImageViewer.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App 
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            LaunchMainWindow();
        }

        private void LaunchMainWindow()
        {
            var logger = new ApplicationLogger("YlImgV");
            logger.WriteLine("initialize");
            var container = new Container();
            container.RegisterSingleton<ILogger>(()=> logger);
            container.RegisterSingleton<ConfigLoader>();
            container.RegisterSingleton<Config>(()=>container.GetInstance<ConfigLoader>().LoadOrCreateConfig());
            container.RegisterSingleton<ConfigService>();
            container.RegisterSingleton<Project>();
            container.RegisterSingleton<RenbanDownLoader>();
            container.RegisterSingleton<ThumbnailService>();
            container.RegisterSingleton<IWindowService,WindowService>();
            container.RegisterSingleton<DocumentOperator>();
            container.RegisterSingleton<MainWindowVm>(); 
            
        #if DEBUG
            container.Verify();
        #endif
            
            var window = new MainWindow
            {
                DataContext = container.GetInstance<MainWindowVm>()
            };
            window.Show();
            window.Closed += (_, __) =>
            {
                container.Dispose();
            };
        }
    }
}