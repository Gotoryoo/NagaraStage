using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace IO {
        /// <summary>
        /// モータの速度を定義する列挙体です．
        /// <para>語尾の値が大きくなるに従って高速になります.</para>
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public enum MotorSpeed {
            Other = 0x0,
            Speed1 = 0x1,
            Speed2 = 0x2,
            Speed3 = 0x4,
            Speed4 = 0x8
        }
    }
}