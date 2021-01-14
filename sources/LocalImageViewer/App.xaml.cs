using System.Windows;
using SimpleInjector;

namespace LocalImageViewer
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
            var container = new Container();
            container.RegisterSingleton<ConfigLoader>();
            container.RegisterSingleton<Config>(()=>container.GetInstance<ConfigLoader>().LoadOrCreateConfig());
            container.RegisterSingleton<ConfigService>();
            container.RegisterSingleton<Project>();
            container.RegisterSingleton<RenbanDownLoader>();
            container.RegisterSingleton<ThumbnailService>();
            container.RegisterSingleton<IWindowService,WindowService>();
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