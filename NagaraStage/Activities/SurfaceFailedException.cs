using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Activities {
        /// <summary>
        /// 表面認識処理が失敗したことを示す例外クラスです．
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public class SurfaceFailedException : Exception {
            public SurfaceFailedException() : base() { }

            public SurfaceFailedException(string message)
                : base(message) { }

            public SurfaceFailedException(string message, Exception innerException)
                : base(message, innerException) { }
        }
    }
}