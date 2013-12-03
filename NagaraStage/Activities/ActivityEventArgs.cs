using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Activities {
        /// <summary>
        /// モータ移動をしながら他の処理を行う際などのEventArgsクラスです．
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public class ActivityEventArgs : EventArgs {
            /// <summary>
            /// イベント発生時のX軸方向の座標値
            /// </summary>
            public double XValue = 0;

            /// <summary>
            /// イベント発生時のY軸方向の座標値
            /// </summary>
            public double YValue = 0;

            /// <summary>
            /// イベント発生時のZ軸方向の座標値
            /// </summary>
            public double ZValue = 0;

#if false
            /// <summary>
            /// イベント発生時の座標
            /// </summary>
            public Vector3 Point {
                get {
                    return new Vector3(XValue, YValue, ZValue);
                }
            }

#endif
            /// <summary>
            /// 処理が正常に完了したかどうか
            /// </summary>
            public bool IsCompleted = false;

            /// <summary>
            /// 処理が中止されたかどうか
            /// </summary>
            public bool IsAborted = false;

            /// <summary>
            /// 発生した例外
            /// </summary>
            public Exception Exception = null;
        }
    }
}