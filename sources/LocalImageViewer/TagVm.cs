using YiSA.WPF.Common;

namespace LocalImageViewer
{
    public class TagVm : DisposableBindable
    {
        public string DisplayName { get; }
        
        public TagVm(string tag)
        {
            DisplayName = tag;
        }
    }
}