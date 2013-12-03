using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Parameter {
        /// <summary>
        /// ふさわしくないレンズタイプが設定されていた時に発生する例外です．
        /// </summary>
        /// <author email="o1007410@edu.gifu-u.ac.jp">Hirokazu Yokoyama</author>
        public class LensTypeException : Exception {

            private double correctMagnification;

            /// <summary>
            /// 本来設定されているべきレンズタイプを取得します．
            /// </summary>
            public double CorrectLensType {
                get { return correctMagnification; }
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="_correctLensType">本来設定されているべきレンズタイプ</param>
            public LensTypeException(double _correctLensType)
                : base() {
                this.correctMagnification = _correctLensType;
            }

            /// <summary>コンストラクタ</summary>
            /// <param name="_correctLensType">本来設定されているべきレンズタイプ</param>
            /// <param name="message">The message. <see cref="System.Exception" /></param>
            public LensTypeException(double _correctLensType, string message)
                : base(message) {
                this.correctMagnification = _correctLensType;
            }

            /// <summary>コンストラクタ</summary>
            /// <param name="_correctLensType">本来設定されているべきレンズタイプ</param>
            /// <param name="message">The message.</param>
            /// <param name="innerException">The inner exception.</param>
            public LensTypeException(double _correctLensType, string message, Exception innerException) :
                base(message, innerException) {
                this.correctMagnification = _correctLensType;
            }
        }
    }
}