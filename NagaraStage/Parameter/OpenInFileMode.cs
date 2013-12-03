using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Parameter {
        /// <summary>
        /// 各トラックデータなどを記録したファイルを開く時のモードを定義した列挙体です．
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public enum OpenInFileMode {
            None = -1,

            /// <summary>
            /// 拡張子.pred のPredictionファイルのモードです．
            /// </summary>
            Prediction = 0,

            /// <summary>
            /// 拡張子.scan0, .scan1 のScanDataファイルのモードです．
            /// </summary>
            ScanData = 1,

            /// <summary>
            /// 拡張子.vtx のVertexファイルのモードです．
            /// </summary>
            VertexData = 2,

            /// <summary>
            /// 不明
            /// </summary>
            IOmodeVinVout = 3
        }
    }
}