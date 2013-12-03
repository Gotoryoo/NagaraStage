#pragma once
#include "Stdafx.h"

namespace NagaraStage {
    namespace ImageEnhancement {

        /// <summary>
        /// 画像内の特定の物体を囲う外接四角形を定義しているクラスです．
        /// </summary>
        public ref class CircumBox {
        public:
            CircumBox();

            /// <summary>
            /// コンストラクタ
            /// <para>
            /// ソース画像の中に四角形を定義します．
            /// このクラスのメソッドを用いることで四角形内の画像データを取得すると言ったことが可能です．
            /// </para>
            /// <para>ソース画像に変更が加えられることはありません．</para>
            /// </summary>
            /// <param name="source">ソース画像へのポインタ</param>
            /// <param name="_x0">左上の座標値(X)</param>
            /// <param name="_y0">左上の座標値(Y)</param>
            /// <param name="_x1">右下の座標値(X)</param>
            /// <param name="_y1">右下の座標値(Y)</param>
            CircumBox(cv::Mat* source, int _x0, int _y0, int _x1, int _y1);

            /// <param name="source">ソース画像へのポインタ</param>
            /// <param name="_x0">左上の座標値(X)</param>
            /// <param name="_y0">左上の座標値(Y)</param>
            /// <param name="_x1">右下の座標値(X)</param>
            /// <param name="_y1">右下の座標値(Y)</param>
            /// <param name="id">識別番号</param>
            CircumBox(cv::Mat* source, int _x0, int _y0, int _x1, int _y1, int _id);

            /// <summary>
            /// コンストラクタ
            /// <para>
            /// ラベリングしたテーブルでlabelVal値の部分を囲む外接四角形を定義します．
            /// </para>
            /// <para>ソース画像に変更が加えられることはありません．</para>
            /// </summary>
            /// <param name="">ソース画像</param>
            /// <param name="">ラベリングのテーブル</param>
            /// <param name="">抽出するラベル値</param>
            CircumBox(cv::Mat* source, array<int>^ labeledArray, int labelVal);

            /// <summary>
            /// 識別番号を取得，または設定します．デフォルトは-1です．
            /// </summary>
            property int Id {
                int get() {return id;}
                void set(int value) {id = value;}
            }
            /// <summary>
            /// 外接四角形の左上座標(X)を取得します．
            /// </summary>
            property int X0 {
                int get() {return x0;}
            }

            /// <summary>
            /// 外接四角形の左上座標(Y)を取得します．
            /// </summary>
            property int Y0 {
                int get() {return y0;}
            }

            /// <summary>
            /// 外接四角形の右下座標(X)を取得します．
            /// </summary>
            property int X1 {
                int get() {return x1;}
            }

            /// <summary>
            /// 外接四角形の右下座標(Y)を取得します．
            /// </summary>
            property int Y1 {
                int get() {return y1;}
            }

            /// <summary>
            /// 外接四角形の横幅を取得します．
            /// </summary>
            property int Width {
                int get() { return (x1 - x0) + 1; }
            }

            /// <summary>
            /// 外接四角形の縦幅を取得します．
            /// </summary>
            property int Height {
                int get() {return (y1 - y0) + 1;}
            }

            /// <summary>
            /// 外接四角形に含まれる物体の面積を取得します．
            /// <para>外接四角形の面積を取得するわけではありません．</para>
            /// </summary>
            property int Area {
                int get() {return area;}
            }

        private:        
            int id;
            int x0;
            int y0;
            int x1;
            int y1;
            int area;
            void init(cv::Mat* source, int _x0, int _y0, int _x1, int _y1, int _id);
            cv::Mat* boxImage;
        };
    }
}

