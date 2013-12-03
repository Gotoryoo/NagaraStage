using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    /// <summary>
    /// らせん移動のイベントです．
    /// </summary>
    /// <author>Hirokazu Yokoyama</author>
    public class SpiralEventArgs : EventArgs {
        public int X;
        public int Y;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="x">らせんのX位置</param>
        /// <param name="y">らせんのY位置</param>
        public SpiralEventArgs(int x, int y) {
            this.X = x;
            this.Y = y;
        }
    }
}
