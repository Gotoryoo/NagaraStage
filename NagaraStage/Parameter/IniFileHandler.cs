using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace NagaraStage {
    namespace Parameter {
        /// <summary>
        /// INIファイルを読み書きを行うために必要なWin32APIをインポートしラッピングを行うクラスです．
        /// </summary>
        /// <author email="hirokazu.online@gmail.com">Hirokazu Yokoyama</author>
        /// <date>2012-06-29</date>
        public class IniFileHandler {
            [DllImport("kernel32.dll")]
            public static extern uint GetPrivateProfileString(string lpAppName,
                string lpKeyName, string lpDefault,
                StringBuilder lpReturnedString, uint nSize,
                string lpFileName);

            /// <see>http://msdn.microsoft.com/ja-jp/library/cc429779.aspx</see>
            [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileStringA")]
            public static extern uint GetPrivateProfileStringByByteArray(
                string lpAppName, string lpKeyName, string lpDefault,
                byte[] lpReturnedString, uint nSize, string lpFileName);

            [DllImport("kernel32.dll")]
            public static extern uint GetPrivateProfileInt(string lpAppName,
              string lpKeyName, int nDefault, string lpFileName);

            [DllImport("kernel32.dll")]
            public static extern uint WritePrivateProfileString(string lpAppName,
                string lpKeyName, string lpString, string lpFileName);
        }
    }
}