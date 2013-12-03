using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace NagaraStage {
    namespace Ui {
        /// <summary>
        /// MainWindowのワークスペースとして設置されるためのコントロールで，
        /// ダイアログボックスの役割を果たすインターフェイスです．
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public interface IDialogWorkspace {
            /// <summary>
            /// ダイアログボックスの結果を取得します．．
            /// </summary>
            MessageBoxResult Result {
                get;
            }
        }
    }
}