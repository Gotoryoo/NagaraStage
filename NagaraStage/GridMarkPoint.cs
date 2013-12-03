using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    /// <summary>
    /// グリッドマークの位置を示す値を定義した列挙体です
    /// </summary>
    /// <author email="o1007410@edu.gifu-u.ac.jp">Hirokazu Yokoyama</author>
    public enum GridMarkPoint {

        /// <summary>
        /// 真ん中のグリッドマーク
        /// </summary>
        CenterMiddle = 0,

        /// <summary>
        /// 左上のグリッドマーク
        /// </summary>
        LeftTop = 1,

        /// <summary>
        /// 左側真ん中のグリッドマーク
        /// </summary>
        LeftMiddle = 2,

        /// <summary>
        /// 左下のグリッドマーク
        /// </summary>
        LeftBottom = 3,

        /// <summary>
        /// 中央下部のグリッドマーク
        /// </summary>
        CenterBottom = 4,

        /// <summary>
        /// 右下のグリッドマーク
        /// </summary>
        RightBottom  = 5,

        /// <summary>
        /// 右側中央のグリッドマーク
        /// </summary>
        RightMiddle = 6,

        /// <summary>
        /// 右上のグリッドマーク
        /// </summary>
        RightTop = 7,

        /// <summary>
        /// 中央上部のグリッドマーク
        /// </summary>
        CenterTop = 8
    }
}
