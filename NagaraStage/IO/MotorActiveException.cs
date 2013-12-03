using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace IO {
        /// <summary>
        /// モータが移動中であるときに発生する例外クラスです．
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public class MotorActiveException : Exception {
            public MotorActiveException()
                : base() { }

            public MotorActiveException(string message)
                : base(message) { }

            public MotorActiveException(string message, Exception innerException)
                : base(message, innerException) { }
        }
    }
}