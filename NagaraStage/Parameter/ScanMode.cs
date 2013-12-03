using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Parameter {
        /// <summary>
        /// スキャンモードを示す列挙体です．
        /// </summary>
        /// <author email="o1007410@edu.gifu-u.ac.jp">Hirokazu Yokoyama</author>
        public enum ScanMode {
            /// <summary>
            /// 全面自動スキャン
            /// </summary>
            Automatic = 0,

            /// <summary>
            /// 一部自動スキャン
            /// </summary>
            SemiAuto = 1,

            TrackDefine = 2,
            ConnectSearch = 3,
            IndexMeasurement = 4,
            Vertex = 5,
            TrakFromEnd = 6,
            AreaCalcurate = 7,

            /// <summary>
            /// 画像データの分析(入力画像はファイルからのモードです．)
            /// </summary>
            ImageFileAnalysis = 8,
            Decay = 101,
            Search2ndTrack = 102
        }
    }
}