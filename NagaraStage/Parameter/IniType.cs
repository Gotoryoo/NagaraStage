using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Parameter {
        /// <summary>
        /// 設定ファイルの値を格納するための型です．
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public struct IniType {
            /// <summary>
            /// エンコーダ分解能
            /// </summary>
            public Vector3 EncoderResolution;

            /// <summary>
            /// モータ分解能
            /// </summary>
            public Vector3 MotorResolution;

            /// <summary>
            /// 初速度
            /// </summary>
            public Vector3 MotorInitialVelocity;

            /// <summary>
            /// 加減速時間
            /// </summary>
            public Vector3 MotorAccelTime;

            public Vector3 MotorSpeed1;
            public Vector3 MotorSpeed2;
            public Vector3 MotorSpeed3;
            public Vector3 MotorSpeed4;

            /// <summary>
            /// リミットスイッチの極性
            /// </summary>
            public long LimitPol;
        }
    }
}