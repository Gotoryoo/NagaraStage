using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    /// <summary>
    /// 定義されているグリッドマークが少なすぎる場合に投げられる例外を示すクラスです．
    /// </summary>
    /// <author>Hirokazu Yokoyama</author>
    public class GridMarkFewException : Exception {
        public GridMarkFewException() : base() { }
        public GridMarkFewException(string message) 
            : base(message) { }
        public GridMarkFewException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
