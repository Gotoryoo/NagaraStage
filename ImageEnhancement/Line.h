#pragma once

#include "Stdafx.h"

namespace NagaraStage{
    namespace ImageEnhancement {
        /// <summary>
        /// 直線の情報を保持，取得するクラスです．
        /// 直線の開始点，終了点から傾きや切片を算出したり，またはその逆を行います．
        // <para>このクラスでは座標系の原点を画像の左上と定義します．
        /// 原点から右方向を横軸の正，左方向を横軸の負とします．また，原点から下方向を縦軸の正，上方向を負とします．</para>
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public ref class Line{
        public:
            /// <param name="_rho">原点から直線開始地点までの距離, 正の値である必要があります．</param>
            /// <param name="_angle">直線の角度をラジアンで指定します．(0から垂直線，Pi/2から水平線)</param>
            /// <exception cref="ArgumentException">_rhoが負の値であった場合</exception>
            Line(double _rho, double _angle);

            /// <param name="_startX">直線の開始座標(X)</param>
            /// <param name="_startY">直線の開始座標(Y)</param>
            /// <param name="_endX">直線の終了座標(X)</param>
            /// <param name="_endY">直線の終了座標(Y)</param>
            /// <exception cref="ArgumentException">引数のいずれかが負の値であった場合，もしくは線の長さが０の場合</exception>
            Line(int _startX, int _startY, int _endX, int _endY);

            /// <param name="_startX">直線の開始座標(X)</param>
            /// <param name="_startY">直線の開始座標(Y)</param>
            /// <param name="_angle">直線の角度をラジアンで指定します．(0から垂直線，Pi/2から水平線)</param>
            /// <exception cref="ArgumentException">_startX,もしくは_startYが負の値であった場合</exception>
            Line(int _startX, int _startY, double _angle);

            /// <summary>
            /// 線の開始座標(X)を取得します．
            /// </summary>
            int GetStartX();

            /// <summary>
            /// 線の開始座標(Y)を取得します．
            /// </summary>
            int GetStartY();

            /// <summary>
            /// 線の終了座標(X)を取得します．
            /// <para>終了座標が未定義の場合は-1を返します．</para>
            /// </summary>
            int GetEndX();

            /// <summary>
            /// 線の終了座標(Y)を取得します．
            /// <para>終了座標が未定義の場合は-1を返します．</para>
            /// </summary>
            int GetEndY();

            /// <summary>
            /// 線の角度をラジアンで取得します．
            /// </summary>
            double GetAngle();

            /// <summary>
            /// 線の切片を取得します．
            /// </summary>
            double GetIntercept();

            /// <summary>
            /// 引数のX値におけるY座標の値を取得します．
            /// </summary>
            /// <param name="x">横軸の値</param>
            /// <return>縦軸の値</return>
            double GetY(double x);

            /// <summary>
            /// 引数のY値におけるX座標の値を取得します．
            /// </summary>
            /// <param name="y">縦軸の値</param>
            /// <return>横軸の値</return>
            double GetX(double y);

            cv::Point GetStartPoint();
            cv::Point GetEndPoint();
        private:
            int startX;
            int startY;
            int endX;
            int endY;
            double angle;
            double intercept;
        };
    }
}