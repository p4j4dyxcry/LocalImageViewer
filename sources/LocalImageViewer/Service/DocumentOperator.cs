using System.Diagnostics;
using LocalImageViewer.DataModel;
using LocalImageViewer.ViewModel;
namespace LocalImageViewer.Service
{
    public class DocumentOperator
    {
        private readonly ConfigService _configService;

        public DocumentOperator(ConfigService configService)
        {
            _configService = configService;
        }

        public TagEditorVm BuildTagVm(ImageDocument document,bool syncTag)
        {
            return new TagEditorVm(document,_configService,syncTag);
        }

        public void OpenWithExplorer(ImageDocument document)
        {
            Process.Start("explorer", document.DirectoryPath);
        }
    }
}
