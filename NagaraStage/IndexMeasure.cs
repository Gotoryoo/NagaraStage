using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NagaraStage.Parameter;

namespace NagaraStage {

    /// <summary>
    /// IndexMeasurementを算出するためのクラスです．
    /// </summary>
    /// <author email="o1007410@edu.gifu-u.ac.jp">Hirokazu Yokoyama</author>
    public class IndexMeasure {

        private enum IndexInputed { 
            UpTop = 1,
            UpBottom = 2,
            LowTop = 4,
            LowBottom = 8
        }

        /// <summary>
        /// 標準的なゲルの厚さ(um)
        /// </summary>
        public const int DefaultGelThick = 500;

        private Vector3 upTop = new Vector3();
        private Vector3 upBottom = new Vector3();
        private Vector3 lowTop = new Vector3();
        private Vector3 lowBottom = new Vector3();
        private int input = 0;

        /// <summary>
        /// 上ゲル上面の境界面の座標を取得，または設定します．
        /// </summary>
        public Vector3 UpTopValue {
            get { return upTop; }
            set { 
                upTop = value;
                if ((input & (int)IndexInputed.UpTop) == 0) {
                    input += (int)IndexInputed.UpTop;
                }
            }
        }

        /// <summary>
        /// 上ゲル下面の境界面の座標を取得，または設定します．
        /// </summary>
        public Vector3 UpBottomValue {
            get { return upBottom; }
            set { 
                upBottom = value;
                if ((input & (int)IndexInputed.UpBottom) == 0) {
                    input += (int)IndexInputed.UpBottom;
                }
            }
        }

        /// <summary>
        /// 下ゲル下面の境界面の座標を取得，または設定します．
        /// </summary>
        public Vector3 LowTopValue {
            get { return lowTop; }
            set { 
                lowTop = value;
                if ((input & (int)IndexInputed.LowTop) == 0) {
                    input += (int)IndexInputed.LowTop;
                }
            }
        }

        /// <summary>
        /// 下ゲル下面の境界面の座標を取得，または設定します．
        /// </summary>
        public Vector3 LowBottomValue {
            get { return lowBottom; }
            set { 
                lowBottom = value;
                if ((input & (int)IndexInputed.LowBottom) == 0) {
                    input += (int)IndexInputed.LowBottom;
                }
            }
        }                              

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public IndexMeasure() {
        }

        /// <summary>
        /// 薄型エマルションにおけるインデックス値を計測します．
        /// </summary>
        /// <returns>インデックス値</returns>
        /// <exception cref="Exception">全ての境界面が入力されていない場合</exception>
        public EmulsionIndex MeasureIndexThinType() {
            if (!isAllValueInputed()) {
                throw new Exception("All index value is not defined");
            }

            EmulsionIndex index;
            double tan1 = ParameterManager.DistPP(upTop.X - upBottom.X, (upTop.Y-upBottom.Y) / (upTop.Z - upBottom.Z));
            double tan0 = ParameterManager.DistPP(upBottom.X - lowTop.X, (upBottom.Y-lowTop.Y) / (upBottom.Z - lowTop.Z));
            double tan2 = ParameterManager.DistPP(lowTop.X - lowBottom.X, (lowTop.Y-lowBottom.Y) / (lowTop.Z - lowBottom.Z));
            index.Up = tan1 / tan0;
            index.Down = tan2 / tan0;
            return index;
        }
        
        /// <summary>
        /// 厚型エマルションにおけるインデックス値を計測します．
        /// </summary>
        /// <param name="gelThickness">ゲルの厚さ(デフォルト値500um)</param>
        /// <returns>インデックス値</returns>
        /// <exception cref="Exception">全ての境界面が入力されていない場合</exception>
        public EmulsionIndex MeasureIndexThickType(double gelThickness = DefaultGelThick) {
            if (!isAllValueInputed()) {
                throw new Exception("All index value is not defined");
            }

            EmulsionIndex index;
            index.Up = gelThickness / (upTop.Z - upBottom.Z);
            index.Down = gelThickness/ (lowTop.Z - lowBottom.Z);
            return index;
        }

        /// <summary>
        /// 全ての境界面の値が入力されているかどうかを返します．
        /// </summary>
        /// <returns>入力されていればtrue, そうでなければfalse</returns>
        private Boolean isAllValueInputed() {
            // 全ての境界面の値が入力されていた場合，
            // フィールド変数inputは15になっているはずである．
            return (input >= 15 ? true : false);
        }

        
    }
}
