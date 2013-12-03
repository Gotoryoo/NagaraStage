using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Activities {
        /// <summary>
        /// 一定の処理などを実行中であることが原因で発生する例外を示すクラスです．
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public class InActionException : Exception {
            public InActionException() : base() { }

            public InActionException(string message)
                : base(message) { }

            public InActionException(string message, Exception innerException)
                : base(message, innerException) { }
        }
    }
}
