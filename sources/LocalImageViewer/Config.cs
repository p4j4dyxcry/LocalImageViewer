using System;
using YamlDotNet.Serialization;

namespace LocalImageViewer
{
    public class Config
    {
        /// <summary>
        /// バージョン
        /// </summary>
        public Version Version { get; set; } = new Version(1,0,1);

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

        /// <summary>
        /// 設定ファイルの保存パス
        /// </summary>
        [YamlIgnore] public string LatestSaveFilePath { get; set; }
    }
}