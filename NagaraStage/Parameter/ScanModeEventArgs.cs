using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Parameter {
        /// <summary>
        /// スキャンモードのイベント
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public class ScanModeEventArgs : EventArgs {
            public ScanMode ScanMode;

            public ScanModeEventArgs(ScanMode scanMode) {
                this.ScanMode = scanMode;
            }
        }
    }
}