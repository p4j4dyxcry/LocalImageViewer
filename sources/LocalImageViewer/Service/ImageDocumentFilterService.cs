using System.Linq;
using LocalImageViewer.DataModel;
using YiSA.WPF.Common;
namespace LocalImageViewer.Service
{
    public class ImageDocumentFilterService : DisposableHolder
    {
        private readonly Project _project;
        private readonly ConfigService _configService;

        public ImageDocumentFilterService(Project project,ConfigService configService)
        {
            _project = project;
            _configService = configService;
        }

        public bool Filter(ImageDocument imageDocument)
        {
            return TagFilter(imageDocument);
        }

        public bool TagFilter(ImageDocument imageDocument)
        {
            var tags = _configService.Tags;
            if (_configService.Tags.All(x => x.IsEnabled is false))
            {
                return true;
            }
            return tags.Where(x => x.IsEnabled)
                .Any(x => 
                    imageDocument.GetTags()
                        .Contains(x.Tag));
        }
    }
}
