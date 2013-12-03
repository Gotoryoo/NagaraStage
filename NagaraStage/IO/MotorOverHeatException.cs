using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace IO {
        /// <summary>
        /// モータがオーバーヒートを検出した時の例外クラスです．
        /// </summary>
        /// <author email="o1007410@edu.gifu-u.ac.jp">Hirokazu Yokoyama</author>
        public class MotorOverHeatException : Exception {

            public MotorOverHeatException() {
            }

            public MotorOverHeatException(string message)
                : base(message) {
            }

            public MotorOverHeatException(string message, Exception innerException)
                : base(message, innerException) {
            }
        }
    }
}