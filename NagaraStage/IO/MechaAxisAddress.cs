using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace IO {
        /// <summary>
        /// モータコントロールボードの軸アドレスを示す列挙体です．
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public enum MechaAxisAddress {
            /// <summary>Ｘ軸トップアドレス</summary>
            XAddress = 0x0,

            /// <summary>Y軸トップアドレス</summary>
            YAddress = 0x8,

            /// <summary>Z軸トップアドレス</summary>
            ZAddress = 0x10,
#if false
            /// <summary>α軸トップアドレス</summary>
            //AAddress = 0x18
#endif
        }
    }
}