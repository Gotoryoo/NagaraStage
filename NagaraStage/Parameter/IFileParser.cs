using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage.Parameter {
    /// <summary>
    /// 外部ファイルから設定などを読み込む機能を持つクラスのためのインターフェイスです．
    /// </summary>
    public interface IFileParser {
        /// <summary>
        /// ファイルから値を入力します．
        /// </summary>
        void Load(string path);

        /// <summary>
        /// ファイルに値を保存します．
        /// </summary>
        /// <param name="path">保存するファイル</param>
        void Save(string path);
    }
}
