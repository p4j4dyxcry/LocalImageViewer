using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
namespace LocalImageViewer.Foundation
{
    public sealed class TaskCompletionNotifier<TResult> : INotifyPropertyChanged
    {
        public static TaskCompletionNotifier<TResult> Default { get; }= new (default);

        private TResult _result;
        public TaskCompletionNotifier(Task<TResult> task, TResult @default = default)
        {
            _result = @default;
            if (!task.IsCompleted)
            {
                var scheduler = SynchronizationContext.Current is null ? TaskScheduler.Current : TaskScheduler.FromCurrentSynchronizationContext();
                task.ContinueWith(t =>
                    {
                        var propertyChanged = PropertyChanged;
                        if (propertyChanged is not null)
                        {
                            _result = t.Result;
                            propertyChanged(this, new PropertyChangedEventArgs(nameof(Value)));
                        }
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    scheduler);
            }
            else
            {
                _result = task.Result;
            }
        }

        public TaskCompletionNotifier(TResult sync)
        {
            _result = sync;
        }
        public TResult Value => _result;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
