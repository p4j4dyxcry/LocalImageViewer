using System.Collections.Generic;
using System.IO;
namespace LocalImageViewer.Foundation
{
    /// <summary>
    /// ディレクトリを列挙するUtilityです。
    /// アクセス権が無いファイルやディレクトリはスキップします。
    /// </summary>
    public class FileSystemEnumerator
    {
        public static IEnumerable<string> EnumerateFiles(string directory,bool includeSubDirectory)
        {
            foreach (string file in Directory.EnumerateFiles(directory).CatchIgnored())
            {
                yield return file;
            }
            if (includeSubDirectory)
            {
                foreach (string subDirectory in Directory.EnumerateDirectories(directory).CatchIgnored())
                {
                    foreach (var subfile in EnumerateFiles(subDirectory,true).CatchIgnored())
                    {
                        yield return subfile;
                    }
                }
            }
        }
    }

    internal static class EnumeratorExtension
    {
        /// <summary>
        /// 列挙中に発生した例外を無視します。
        /// </summary>
        public static IEnumerable<T> CatchIgnored<T>(this IEnumerable<T> enumerable)
        {
            IEnumerator<T> enumerator;
            try
            {
                enumerator = enumerable.GetEnumerator();
            }
            catch
            {
                yield break;
            }
            while (true)
            {
                T ret;
                try
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    ret = enumerator.Current;
                }
                catch
                {
                    break;
                }
                yield return ret;
            }
            enumerator.Dispose();
        }
    }
}
