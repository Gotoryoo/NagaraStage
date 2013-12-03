using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    /// <summary>
    /// グリッドマークが見つからない時に発生する例外クラスです．
    /// </summary>
    /// <author>Hirokazu Yokoyama</author>
    public class GridMarkNotFoundException : Exception {

        public override string Message {
            get {
                return base.Message + Properties.Strings.GridMarkNotFoundException01;
            }
        }

        public GridMarkNotFoundException() 
            : base() {}

        public GridMarkNotFoundException(string message)
            : base(message) { }

        public GridMarkNotFoundException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}
