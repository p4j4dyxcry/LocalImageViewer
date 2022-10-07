using System.IO;
using YamlDotNet.Serialization;
namespace LocalImageViewer.Foundation
{
    /// <summary>
    /// yml ファイルの簡易拡張クラス
    /// </summary>
    public static class YamlSerializeHelper
    {
        public static T LoadFromFile<T>(string absolutePath)
        {
            var deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();

            var yaml = File.ReadAllText(absolutePath);
            
            return deserializer.Deserialize<T>(yaml);            
        }
        
        public static void SaveToFile<T>(string absolutePath , T value)
        {
            var serializer = new SerializerBuilder()
                .Build();

            var str = serializer.Serialize(value);

            try
            {
                File.WriteAllText(absolutePath, str);
            }
            catch
            {
                // ignore.
            }
        }
    }
}
