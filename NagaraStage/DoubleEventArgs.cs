using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    /// <summary>
    /// double型の値を持つEventArgs
    /// </summary>
    /// <author>Hirokazu Yokoyama</author>
    public class DoubleEventArgs : EventArgs {
        public double Value;

        public DoubleEventArgs(double value) {
            this.Value = value;
        }
    }
}
