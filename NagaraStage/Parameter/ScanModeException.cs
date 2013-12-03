using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Parameter {
        /// <summary>
        /// スキャンモードに不正があった時の例外クラスです．
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public class ScanModeException : Exception {

            private ScanMode correctMode;

            public ScanMode CorrectMode {
                get { return correctMode; }
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="_correctMode">正しいスキャンモード</param>
            public ScanModeException(ScanMode _correctMode)
                : base(createMessage(_correctMode)) {
                this.correctMode = _correctMode;
            }

            public ScanModeException(ScanMode _correctMode, string message)
                : base(createMessage(_correctMode) + message) {
                this.correctMode = _correctMode;
            }

            public ScanModeException(ScanMode _correctMode, string message, Exception innerException)
                : base(createMessage(_correctMode) + message, innerException) {
                this.correctMode = _correctMode;
            }

            private static string createMessage(ScanMode _correctMode) {
                return "Scan mode need to be " + _correctMode.ToString() + ". ";
            }
        }
    }
}