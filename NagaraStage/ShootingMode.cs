using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    /// <summary>
    /// エマルションの撮影モードを定義した列挙対です．
    /// </summary>
    /// <author>Hirokazu Yokoyama</author>
    public enum ShootingMode {
        /// <summary>
        /// 1枚だけ撮影する
        /// </summary>
        Single = 0,

        /// <summary>
        /// 複数枚ばらばらに撮影する
        /// </summary>
        Plurality = 1,

        /// <summary>
        /// 複数枚を重ねて1枚にして撮影する
        /// </summary>
        Accumlative = 2
    }
}
