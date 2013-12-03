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
    public interface IGridMarkRecogner {
        /// <summary>
        /// グリッドマークを検出し，その座標を返します．
        /// </summary>
        /// <returns>グリッドマークの座標</returns>
        Vector2 SearchGridMark();
    }
}
