/**
 * @author Hirokazu Yokoyama <o1007410@edu.gifu-u.ac.jp>
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    /// <summary>
    /// グリッドマークを画像から認識・検出するためのメソッドをもつインターフェイスです．
    /// </summary>
    public interface IGridMarkRecognizer {
        /// <summary>
        /// グリッドマークを検出し，その座標を返します．
        /// 実装はいまのところCoordManager.csにあります。
        /// </summary>
        /// <returns>グリッドマークの座標</returns>
        Vector2 SearchGridMark();

        /// <summary>
        /// この関数を実行すると，カメラから画像を取得し，グリッドマークを検出しその座標を返します．
        /// 実行時のレンズはx50対物であることを前提とします.
        /// 実装はいまのところCoordManager.csにあります。
        /// </summary>
        /// <returns>グリッドマークを検出したピクセル座標。検出できなかった時は(-1,-1)が返される</returns>
        Vector2 SearchGridMarkx50();
    }
}
