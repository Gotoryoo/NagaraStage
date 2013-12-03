using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace IO {
        /// <summary>
        /// モータの軸が限界値に達した時の例外クラスです．
        /// </summary>
        /// <author email="o1007410@edu.gifu-u.ac.jp">Hirokazu Yokoyama</author>
        public class MotorAxisException : Exception {
            public MotorAxisException() {

            }

            public MotorAxisException(string message)
                : base(message) {

            }

            public MotorAxisException(string message, Exception innerException)
                : base(message, innerException) {

            }
        }
    }
}