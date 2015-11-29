using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace IO {
        /// <summary>
        /// モータの異常状態を定義します．
        /// </summary>
        /// <author email="o1007410@edu.gifu-u.ac.jp">Hirokazu Yokoyama</author>
        public enum MotorState {

            /// <summary>
            /// 問題なし
            /// </summary>
            NoProblem = 0x0,

            /// <summary>
            /// データの設定値が不正です．
            /// </summary>
            ConfiguredValueNotCorrect = 0x80,

            /// <summary>
            /// オーバーヒートを検出しました．
            /// </summary>
            OverHeat = 0x10,

            /// <summary>
            /// マイナス方向に軸の移動が限界です．
            /// </summary>
            AxisLimitMinus = 0x2,

            /// <summary>
            /// プラス方向に軸の移動が限界です．
            /// </summary>
            AxisLimitPlus = 0x1
        }
    }
}