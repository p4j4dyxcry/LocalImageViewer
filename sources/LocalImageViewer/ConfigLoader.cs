using System.IO;
using System.Reactive.Disposables;
using YiSA.WPF.Common;

namespace LocalImageViewer
{
    /// <summary>
    /// 設定ファイルの読み込みを扱うクラス
    /// </summary>
    public class ConfigLoader : DisposableHolder
    {
        private Config _latestConfig;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ConfigLoader()
        {
            // Loader破棄時に最後に読み込まれたConfigを自動的に保存
            Disposables.Add(Disposable.Create(() =>
            {
                if (_latestConfig != null)
                    SaveConfigAsync(_latestConfig);
            }));
        }
        
        /// <summary>
        /// Configファイルを読み込みます。
        /// 存在しない場合は新規作成します。
        /// </summary>
        /// <returns></returns>
        public Config LoadOrCreateConfig()
        {
            var applicationDirectory = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(applicationDirectory, "config.yml");

            if (File.Exists(configPath))
            {
                _latestConfig = LoadConfig(configPath);                
            }
            else
            {
                _latestConfig = new Config()
                {
                    Project = Path.Combine(applicationDirectory,"Project"),
                };                
            }

            _latestConfig.LatestSaveFilePath = configPath;
            if (string.IsNullOrEmpty(_latestConfig.ThumbnailDirectory))
                _latestConfig.ThumbnailDirectory = Path.Combine(applicationDirectory, "Thumbs");
            return _latestConfig;
        }
        
        /// <summary>
        /// ファイルパスを指定してConfigファイルを読み込みます。
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <returns></returns>
        public Config LoadConfig(string absolutePath)
        {
            return YamlSerializeHelper.LoadFromFile<Config>(absolutePath);
        }

        /// <summary>
        /// Configファイルを指定したファイルパスに保存します。
        /// ファイルパスを指定しない場合最後に保存された場所に保存されます。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="absolutePath"></param>
        /// <returns></returns>
        public void SaveConfigAsync(Config config , string absolutePath = "")
        {
            if (string.IsNullOrEmpty(absolutePath))
                absolutePath = config.LatestSaveFilePath;
            
            YamlSerializeHelper.SaveToFile(absolutePath,config);
        }
    }
}