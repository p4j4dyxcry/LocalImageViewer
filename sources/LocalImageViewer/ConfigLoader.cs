using System;
using System.IO;
using System.Reactive.Disposables;
using YiSA.Foundation.Logging;
using YiSA.WPF.Common;

namespace LocalImageViewer
{
    /// <summary>
    /// 設定ファイルの読み込みを扱うクラス
    /// </summary>
    public class ConfigLoader : DisposableHolder
    {
        private readonly ILogger _logger;
        private Config _latestConfig;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ConfigLoader(ILogger logger)
        {
            _logger = logger;
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
            _logger.WriteLine("load config");
            var applicationDirectory = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(applicationDirectory, "config.yml");

            if (File.Exists(configPath))
            {
                try
                {
                    _latestConfig = LoadConfig(configPath);
                    _logger.WriteLine($"config loaded {configPath}");
                }
                catch (Exception e)
                {
                    _logger.Error(e,$"config loaded failed {configPath}");
                    _latestConfig = new Config()
                    {
                        Project = Path.Combine(applicationDirectory,"Project"),
                    };        
                }
            }
            else
            {
                _logger.WriteLine($"not found config path = {configPath}");
                _latestConfig = new Config()
                {
                    Project = Path.Combine(applicationDirectory,"Project"),
                };                
            }

            _latestConfig.LatestSaveFilePath = configPath;
            if (string.IsNullOrEmpty(_latestConfig.ThumbnailDirectory))
                _latestConfig.ThumbnailDirectory = Path.Combine(applicationDirectory, "Thumbs");
            
            _logger.WriteLine($"LatestSaveFilePath {configPath}");
            _logger.WriteLine($"ThumbnailDirectory {_latestConfig.ThumbnailDirectory}");
            _logger.WriteLine($"ProjectDirectory {_latestConfig.Project}");
            
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

            _logger.WriteLine($"saved config {absolutePath}");
            YamlSerializeHelper.SaveToFile(absolutePath,config);
        }
    }
}