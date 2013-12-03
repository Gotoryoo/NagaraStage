using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    /// <summary>
    /// int型の値を持つEventArgs
    /// </summary>
    /// <author>Hirokazu Yokoyama</author>
    public class IntEventArgs : EventArgs {
        public int Value;
        public IntEventArgs(int value) {
            this.Value = value;
        }
    }
}
