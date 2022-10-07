using LocalImageViewer.DataModel;
using YiSA.WPF.Common;
namespace LocalImageViewer.ViewModel
{
    /// <summary>
    /// 最近使ったファイルのViewModel
    /// </summary>
    public class RecentVm : DisposableBindable
    {
        public string DisplayName { get; }
        
        public string Thumbnail { get; }
        
        public ImageDocument Document { get; }
        
        public RecentVm(ImageDocument value)
        {
            Document = value;
            
            if (value is null)
            {
                // データが物理的に存在していない場合
                DisplayName = "不明";
                Thumbnail = "Resources/loading.png";
            }
            else
            {
                DisplayName = value.DisplayName;
                Thumbnail = value.SmallThumbnailAbsolutePath;                
            }
        }
    }
}