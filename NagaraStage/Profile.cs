using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NagaraStage.IO;

namespace NagaraStage {
    /// <summary>
    /// ソフトウェアの細かい設定などを定義するクラスです．
    /// </summary>
    /// <author>Hirokazu Yokoyama</author>
    public class Profile {

        /// <summary>
        /// ソフトウェア名
        /// </summary>
        public const string Name = "AutoStage";

        /// <summary>
        /// 使用しているカメラタイプ
        /// </summary>
        public static CameraType CameraType {
            get { return CameraType.SONY_XC_HR3000_2I_59MHz; }
        }

        /// <summary>
        /// エラーログファイル
        /// </summary>
        public const string ErrorLog = "errorAutoStage.log";
    }
}
