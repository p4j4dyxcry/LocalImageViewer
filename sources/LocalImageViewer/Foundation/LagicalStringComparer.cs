using System.Collections.Generic;
namespace LocalImageViewer.Foundation
{
    /// <summary>
    /// 大文字小文字を区別せずに、
    /// 文字列内に含まれている数字を考慮して文字列を比較します。
    /// </summary>
    public class LogicalStringComparer :
        System.Collections.IComparer,
        IComparer<string>
    {
        public static LogicalStringComparer Instance { get; } = new LogicalStringComparer();
        
        [System.Runtime.InteropServices.DllImport("shlwapi.dll",
            CharSet = System.Runtime.InteropServices.CharSet.Unicode,
            ExactSpelling = true)]
        private static extern int StrCmpLogicalW(string x, string y);
 
        public int Compare(string x, string y)
        {
            return StrCmpLogicalW(x, y);
        }
 
        public int Compare(object x, object y)
        {
            return Compare(x.ToString(), y.ToString());
        }

        private LogicalStringComparer()
        {
            
        }
    }
}