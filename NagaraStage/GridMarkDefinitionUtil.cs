using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    /// <summary>
    /// Grid markを定義するために必要なメソッド群を提供するクラスです．
    /// <para>
    /// エマルションの座標系は，エマルション上のグリッドマークの座標を元に構築されて
    /// います．使用中のエマルションの座標系はCoordManagerクラスが管理しています．
    /// このクラスはユーザーがグリッドマークを定義するのに必要な演算を行うメソッドを
    /// 提供しています．
    /// </para>
    /// <para>
    /// このクラスは定義に必要な値を算出するのみであり，座標系の管理，実際の定義処理
    /// などを行う機能は持っていません(持たせてはいけません)．
    /// </para>
    /// </summary>
    /// <author>Hirokazu Yokoyama</author>
    public class GridMarkDefinitionUtil {
        /// <summary>
        /// アプリケーション全体で使用されている座標系
        /// </summary>
        CoordManager coordManager;

        /// <summary>
        /// オフセットなどを算出するための基準となる座標系．このクラス内部でしか用いない．
        /// </summary>
        CoordManager innerCoord;

        /// <summary>
        /// 定義されたグリッドマークから，設置されているエマルションのずれの角度(degree)を取得します．
        /// <para>
        /// 顕微鏡のステージにエマルションは，垂直水平に設置されることが理想的ですが
        /// 人為的に設置されるものであるため，ズレが発生してしまうことは避けられませ
        /// ん．このプロパティは定義されたグリッドマークからエマルションのずれの角度
        /// を取得することができます．
        /// </para>
        /// <para>
        /// このプロパティを参照するには，少なくとも中央のグリッドマークともう１カ所
        /// のグリッドマークが定義されている必要があります．
        /// </para>
        /// <para>
        /// このプロパティはずれの角度を360度数で返します．
        /// </para>
        /// </summary>        
        public double OffsetAngleDeg {
            get { return Math.Atan2(OffsetX, OffsetY); }
        }

        /// <summary>
        /// 座標系のズレのX軸の補正値を定義済みのグリッドマークの座標から取得します．
        /// <para>各グリッドマークのずれの平均値を返します．</para>
        /// <para>このプロパティから値を得るには２つ以上グリッドマークが定義されている必要があります.
        /// 定義されていない場合,0が返ります.
        /// </para>
        /// </summary>
        public double OffsetX {
            get {
                if (!IsMinimumDefined) {
                    //throw new GridMarkFewException();
                    return 0;
                }

                double sumOfdisplacement = 0;
                for (int i = 0; i < CoordManager.AllGridMarksNum; ++i) {
                    if (coordManager.GetGridMark((GridMarkPoint)i).Existed) {
                        sumOfdisplacement +=
                            coordManager.GetGridMark((GridMarkPoint)i).x - innerCoord.GetGridMark((GridMarkPoint)i).x;
                    }
                }
                return sumOfdisplacement / coordManager.DefinedGridMarkNum;
            }
        }

        /// <summary>
        /// 座標系のズレのY軸の補正値を定義済みのグリッドマークの座標から取得します．
        /// <para>各グリッドマークのずれの平均値を返します．</para>
        /// <para>このプロパティから値を得るには２つ以上グリッドマークが定義されている必要があります.
        /// 定義されていない場合,0が返ります.
        /// </para>
        /// </summary>
        public double OffsetY {
            get {
                if (!IsMinimumDefined) { 
                    //throw new GridMarkFewException();
                    return 0;
                }
                
                double sumOfdisplacement = 0;
                for (int i = 0; i < CoordManager.AllGridMarksNum; ++i) {
                    if (coordManager.GetGridMark((GridMarkPoint)i).Existed) {
                        sumOfdisplacement += 
                            coordManager.GetGridMark((GridMarkPoint)i).y - innerCoord.GetGridMark((GridMarkPoint)i).y;
                    }                    
                }
                return sumOfdisplacement / coordManager.DefinedGridMarkNum;
            }
        }

        /// <summary>
        /// 順番的に次に定義すべきグリッドマークを取得します．
        /// </summary>
        public GridMarkPoint NextPoint {
            get {
                int nextPoint = 0;
                // 列挙体で定義された値順に定義済みかどうかを調べていき，
                // 最初に見つかった定義済みでなかったグリッドマークを次のグリッドマークとする．
                for (int i = (int)GridMarkPoint.CenterMiddle; i <= (int)GridMarkPoint.CenterTop; ++i) {
                    if (!coordManager.GetGridMark((GridMarkPoint)i).Existed) {
                        nextPoint = i;
                        break; // exit for loop
                    }
                }
                return (GridMarkPoint)nextPoint;
            }
        }

        /// <summary>
        /// 座標系の生成に必要な最小限のグリッドマークが定義されているかを取得します．
        /// <para>座標系の生成には少なくとも２つのグリッドマークが定義されている必要があります．</para>
        /// <para>true: 定義されている</para>
        /// </summary>
        public bool IsMinimumDefined {
            get {
                return (coordManager.DefinedGridMarkNum >= 2);
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_coordManager">coordManagerのインスタンス</param>
        public GridMarkDefinitionUtil(CoordManager _coordManager) {
            this.coordManager = _coordManager;
            this.innerCoord = _coordManager.Clone();
            innerCoord.SetGridMark(0, 0, GridMarkPoint.CenterMiddle);
            innerCoord.SetGridMark(-100, 100, GridMarkPoint.LeftTop);
            innerCoord.SetGridMark(-100, 0, GridMarkPoint.LeftMiddle);
            innerCoord.SetGridMark(-100, -100, GridMarkPoint.LeftBottom);
            innerCoord.SetGridMark(0, -100, GridMarkPoint.CenterBottom);
            innerCoord.SetGridMark(100, -100, GridMarkPoint.RightBottom);
            innerCoord.SetGridMark(100, 0, GridMarkPoint.RightMiddle);
            innerCoord.SetGridMark(100, 100, GridMarkPoint.RightTop);
            innerCoord.SetGridMark(0, 100, GridMarkPoint.CenterTop);
        }

        /// <summary>
        /// グリッドマークの名称を取得します．
        /// </summary>
        /// <param name="mark">取得するグリッドマーク</param>
        /// <returns>グリッドマークの名称</returns>
        public static string ToString(GridMarkPoint mark) {
            string name = "";
            switch (mark) { 
                case GridMarkPoint.CenterMiddle:
                    name = Properties.Strings.GridMarkCenterMiddle;
                    break;
                case GridMarkPoint.LeftTop:
                    name = Properties.Strings.GridMarkLeftTop;
                    break;
                case GridMarkPoint.LeftMiddle:
                    name = Properties.Strings.GridMarkLeftMiddle;
                    break;
                case GridMarkPoint.LeftBottom:
                    name = Properties.Strings.GridMarkLeftBottom;
                    break;
                case GridMarkPoint.CenterTop:
                    name = Properties.Strings.GridMakrCenterTop;
                    break;
                case GridMarkPoint.CenterBottom:
                    name = Properties.Strings.GridMarkCenterBottom;
                    break;
                case GridMarkPoint.RightBottom:
                    name = Properties.Strings.GridMarkRightBottom;
                    break;
                case GridMarkPoint.RightMiddle:
                    name = Properties.Strings.GridMarkRightMiddle;
                    break;
                case GridMarkPoint.RightTop:
                    name = Properties.Strings.GridMarkRightTop;
                    break;
            }
            return name;
        }

        /// <summary>
        /// 引数で与えられたグリッドマークの座標を取得します．
        /// <para>未定義のグリッドマークは予測値を取得します．</para>
        /// </summary>
        /// <param name="gridMark">座標を取得したいグリッドマーク</param>
        /// <returns>グリッドマークの座標</returns>
        public Vector2 GetGridMarkCoord(GridMarkPoint gridMark) {
            Vector2 coord = new Vector2();

            if (coordManager.GetGridMark(gridMark).Existed) {
                coord = new Vector2(
                    coordManager.GetGridMark(gridMark).x,
                    coordManager.GetGridMark(gridMark).y);
            } else {
                GridMark central = coordManager.GetGridMark(GridMarkPoint.CenterMiddle);                
                coord = new Vector2(
                    innerCoord.GetGridMark(gridMark).x + OffsetX + central.x,
                    innerCoord.GetGridMark(gridMark).y + OffsetY + central.y 
                    );
            }

            return coord;
        }
    }
}