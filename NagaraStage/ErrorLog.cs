using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NagaraStage {

    /// <summary>
    /// エラーログへの出力を行うクラスです．
    /// </summary>
    public class ErrorLog {


        /// <summary>
        /// エラーをログファイルへ出力します．
        /// </summary>
        /// <param name="errorMessage">出力するエラーメッセージ</param>
        public static void OutputError(string errorMessage) {
            StreamWriter sw = File.AppendText(Profile.ErrorLog);
            sw.Write("\r\nErrorLog Entry:");
            sw.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
            sw.WriteLine(" :");
            sw.WriteLine(" :{0}", errorMessage);
            sw.WriteLine("-----------------------------------------------");
            sw.Flush();
            sw.Close();
        }
    }
}
