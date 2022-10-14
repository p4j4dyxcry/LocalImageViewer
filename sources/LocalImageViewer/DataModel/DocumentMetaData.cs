using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
namespace LocalImageViewer.DataModel
{
    public class DocumentMetaData
    {
        /// <summary>
        /// バージョン
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// ユニークID
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// 表示名
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// タグ
        /// </summary>
        public string[] Tags { get; set; } = Array.Empty<string>();
        
        /// <summary>
        /// ドキュメントタイプ
        /// </summary>
        public DocumentType Type { get; set; }
        
        /// <summary>
        /// ページサイズ
        /// </summary>
        public int PageSize { get; set; }

        public int LatestPage { get; set; }
        
        [YamlIgnore] public string ProjectAbsolutePath { get; set; }
        [YamlIgnore] public string DirectoryAbsolutePath { get; set; }
        [YamlIgnore] public string LatestSavedAbsolutePath { get; set; }
        [YamlIgnore] public bool IsManualEdited { get; set; }

        public bool IsEdited(DocumentMetaData origin)
        {
            if (origin is null)
            {
                return true;
            }
            if (DisplayName != origin.DisplayName)
            {
                return true;
            }
            if (Type != origin.Type)
            {
                return true;
            }
            if (PageSize != origin.PageSize)
            {
                return true;
            }
            if (LatestPage != origin.LatestPage)
            {
                return true;
            }
            if (Tags?.Length != origin.Tags?.Length)
            {
                return true;
            }
            for (int i = 0; i < (Tags?.Length ?? 0); ++i)
            {
                if (Tags![i] != origin.Tags![i])
                {
                    return false;
                }
            }
            if (File.Exists(LatestSavedAbsolutePath) is false)
            {
                return true;
            }

            return false;
        }

        public DocumentMetaData ShallowCopy()
        {
            return (DocumentMetaData)MemberwiseClone();
        }

    }


    /// <summary>
    /// サポート予定のドキュメントの種類
    /// </summary>
    public enum DocumentType
    {
        Unknown,
        Img,     // 連番画像
        Pdf,     // pdf
        Archive, // Zip , Rar ...etc
    }

    public static class DocumentTypeHelper
    {
        private static readonly IReadOnlyDictionary<DocumentType, string[]> Map = new Dictionary<DocumentType, string[]>()
        {
            {DocumentType.Img,    new []{".jpg",".jpeg",".png"}},
            {DocumentType.Pdf,    new []{".pdf"}},        // 未対応
            {DocumentType.Archive,new []{".zip",".rar"}}, // 未対応
        };
        
        /// <summary>
        /// 列挙隊に紐づく拡張子一覧を取得する
        /// </summary>
        /// <param name="documentType"></param>
        /// <returns></returns>
        public static string[] GetFileExtensions(this DocumentType documentType)
        {
            return Map[documentType];
        }

        public static DocumentType FileExtensionToType(string extension)
        {
            foreach (var keyValue in Map)
            {
                if (keyValue.Value.Contains(extension))
                {
                    return keyValue.Key;
                }
            }

            return DocumentType.Unknown;
        }
    }
}
