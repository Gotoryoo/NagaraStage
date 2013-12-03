using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    /// <summary>
    /// エマルションを構成している各ゲルの名前を定義している列挙体です．
    /// </summary>
    public enum GelLayer {
        /// <summary>
        /// 不明
        /// </summary>
        Unknown = 0,
        
        /// <summary>
        /// 下ゲル
        /// </summary>
        Under = 1,

        /// <summary>
        /// ベース
        /// </summary>
        Base = 2,

        /// <summary>
        /// 上ゲル
        /// </summary>
        Over = 3,

        /// <summary>
        /// 下ゲルより下部でゲル外
        /// </summary>
        UnderOut = 4,

        /// <summary>
        /// 上ゲルより丈夫でゲル外
        /// </summary>
        OverOut= 5
    }
}
