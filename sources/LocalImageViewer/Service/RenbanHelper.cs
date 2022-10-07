using System.Linq;
namespace LocalImageViewer.Service
{
    public static class RenbanHelper
    {
        /// <summary>
        /// 文字列の最も後ろ側にある数字を置き換える
        /// </summary>
        /// <param name="data"></param>
        /// <param name="replaceValue"></param>
        /// <returns></returns>
        public static string ReplaceLastNumber(string data , string replaceValue)
        {
            int startIndex = -1;
            int endIndex = -1;
            for (int i = 0; i < data.Length; ++i)
            {
                var index = data.Length - (i + 1);
                var c = data[index];
                if (c >= '0' && c <= '9')
                {
                    if (endIndex is -1)
                    {
                        endIndex = index;
                    }
                    startIndex = index;
                }
                else if (endIndex != -1)
                {
                    break;
                }
            }

            if (endIndex != -1)
            {
                data = data.Remove(startIndex, endIndex - startIndex + 1);
                data = data.Insert(startIndex, replaceValue);
            }

            return data;
        }
        
        /// <summary>
        /// 文字列の最も後ろ側にある数字を取得する。
        /// 負の値、小数は正しく取得できない。
        /// 失敗した場合は-1が返却される
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int FindLastNumber(string data)
        {
            var str = string.Empty;
            foreach (var c in data.Reverse())
            {
                if (c >= '0' && c <= '9')
                {
                    str =$"{c}{str}";
                }
                else if (str.Any())
                {
                    break;                    
                }
            }

            if (int.TryParse(str, out var result))
            {
                return result;
            }
            return -1;
        }
    }
}