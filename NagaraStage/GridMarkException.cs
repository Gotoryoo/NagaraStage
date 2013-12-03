using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    /// <summary>
    /// グリッドマークの探索，その他グリッドマークのエラーがあった場合の例外です．
    /// </summary>
    /// <author>Hirokazu Yokoyama</author>
    public class GridMarkException : Exception {

        public GridMarkException() : base() { 
        }

        public GridMarkException(string message) : base(message) { 
        }

        public GridMarkException(string message, Exception innerException) :
            base(message, innerException) { 
        }

    }
}
