using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Parameter {
        /// <summary>
        /// レンズタイプが変更されたときのEventArgsです．
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public class LensEventArgs : EventArgs {
            /// <summary>
            /// 現在設定されているレンズの倍率です．
            /// </summary>
            public double Magnification;

            public LensEventArgs(double magnification) {
                this.Magnification = magnification;
            }
        }
    }
}