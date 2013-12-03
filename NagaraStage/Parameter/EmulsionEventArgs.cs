using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Parameter {
        /// <summary>
        /// エマルションに関連したEventArgs
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public class EmulsionEventArgs : EventArgs {
            public EmulsionType EmulsionType;
            public EmulsionEventArgs(EmulsionType emulsionType) {
                this.EmulsionType = emulsionType;
            }
        }
    }
}