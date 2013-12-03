using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Ui {
        /// <summary>
        /// MainWindowのStageFieldが変更されたときのイベントのEventArgsクラス．
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public class StageFieldEventArgs : EventArgs {
            /// <summary>
            /// ステージフィールドのコンテント
            /// </summary>
            public object StageFieldContent;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="stageFieldContent">ステージフィールドのコンテント</param>
            public StageFieldEventArgs(object stageFieldContent = null) {
                this.StageFieldContent = stageFieldContent;
            }
        }
    }
}