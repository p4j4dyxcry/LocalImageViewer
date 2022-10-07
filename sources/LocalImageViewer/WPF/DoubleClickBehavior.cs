using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
namespace LocalImageViewer.WPF
{
    public class DoubleClickBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof(ICommand), typeof(DoubleClickBehavior), new PropertyMetadata(default(ICommand)));

        public ICommand Command
        {
            get => (ICommand) GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseDown += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount is 2)
                {
                    Command?.Execute(AssociatedObject.DataContext);
                }
            };
        }
    }
}