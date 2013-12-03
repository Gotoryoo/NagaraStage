using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Parameter {
        /// <summary>
        /// エマルションタイプが不正だった場合の例外です．
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public class EmulsionTypeException : Exception {

            /// <summary>
            /// 正しいエマルションタイプ
            /// </summary>
            private EmulsionType collectType;

            /// <summary>
            /// 本来使用されるべきエマルションタイプを取得します．
            /// </summary>
            public EmulsionType CollectType {
                get { return collectType; }
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="_collectType">正しいエマルションタイプ</param>
            public EmulsionTypeException(EmulsionType _collectType)
                : base() {
                this.collectType = _collectType;
            }

            public EmulsionTypeException(EmulsionType _collectType, string message)
                : base(message) {
                this.collectType = _collectType;
            }

            public EmulsionTypeException(EmulsionType _collectType, string message, Exception innerException)
                : base(message, innerException) {
                this.collectType = _collectType;
            }
        }
    }
}