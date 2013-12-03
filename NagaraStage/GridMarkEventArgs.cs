using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    /// <summary>
    /// グリッドマークに関するイベントハンドラ
    /// </summary>
    /// <author>Hirokazu Yokoyama</author>
    public class GridMarkEventArgs : EventArgs {
        /// <summary>
        /// 該当するグリッドマーク
        /// </summary>
        public GridMarkPoint GridMarkPoint;

        /// <summary>
        /// グリッドマークの位置(ビュワー座標系)
        /// </summary>
        public Vector2 ViewerPoint;

        /// <summary>
        /// グリッドマークの位置(モータエンコーダ座標系)
        /// </summary>
        public Vector2 EncoderPoint;
    }
}
