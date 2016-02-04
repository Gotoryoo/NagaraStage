using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace IO {
        /// <summary>
        /// モータの異常時の例外クラスです．
        /// </summary>
        /// <author email="o1007410@edu.gifu-u.ac.jp">Hirokazu Yokoyama</author>
        public class MotorException : Exception {
            public MotorException() {

            }

            public MotorException(string message)
                : base(message) {

            }

            public MotorException(string message, Exception innerException)
                : base(message, innerException) {

            }
        }
    }
}