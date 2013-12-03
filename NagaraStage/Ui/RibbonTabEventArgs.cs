using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Ui {
        /// <summary>
        /// リボンタブが選択されたときのEventArgs
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public class RibbonTabEventArgs : EventArgs {
            /// <summary>
            /// 選択されたリボンタブのName
            /// </summary>
            public string SelectedTabName;

            public RibbonTabEventArgs() { }
            public RibbonTabEventArgs(string selectedTabName) {
                this.SelectedTabName = selectedTabName;
            }
        }
    }
}