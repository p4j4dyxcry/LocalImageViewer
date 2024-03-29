using System;
using System.Collections.Generic;
using System.Linq;
using LocalImageViewer.Service;
using YamlDotNet.Serialization;
namespace LocalImageViewer.DataModel
{
    public class Config
    {
        /// <summary>
        /// バージョン
        /// </summary>
        public Version Version { get; set; } = new(1,0,2);

        /// <summary>
        /// プロジェクトディレクトリ(絶対パス)
        /// </summary>
        public string Project { get; set; } = string.Empty;
        
        /// <summary>
        /// サムネイルディレクトリ(絶対パス)
        /// </summary>
        public string ThumbnailDirectory { get; set; } = string.Empty;

        /// <summary>
        /// 最近使ったファイル(プロジェクトからの相対パス)
        /// </summary>
        public Guid[] Recent { get; set; } = Array.Empty<Guid>();
        
        /// <summary>
        /// お気に入り
        /// </summary>
        public Guid[] Fav { get; set; }= Array.Empty<Guid>();

        /// <summary>
        /// タグ
        /// </summary>
        public string[] Tags { get; set; } = Array.Empty<string>();

        public string[] EnabledTags { get; set; } = Array.Empty<string>();

        /// <summary>
        /// タグマップ
        /// </summary>
        public Dictionary<Guid, string[]> TagMap { get; set; } = new();

        public int LastDownloadStartIndex { get; set; } = 0;

        /// <summary>
        /// 設定ファイルの保存パス
        /// </summary>
        [YamlIgnore] public string LatestSaveFilePath { get; set; }
    }
    
    public static class ConfigExtensions
    {
        public static HashSet<TagData> GetTagDataList(this Config config)
        {
            HashSet<string> enabledTags = config.EnabledTags.ToHashSet();
            return config.Tags.Select(x => new TagData(x)
            {
                IsEnabled = enabledTags.Contains(x)
            }).ToHashSet();
        }
    }
}
