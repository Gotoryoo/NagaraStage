#pragma once
#include "Stdafx.h"

namespace NagaraStage{
    namespace ImageEnhancement {
        /// <summary>
        /// 二値化処理のモードを定義する列挙体です．
        /// </summary>
        public enum class ThresholdMode {
            /// <summary>
            /// しきい値以下の値は0に，それ以外は指定した値(Max)になります．
            /// </summary>
            Binary = cv::THRESH_BINARY,

            /// <summary>
            /// しきい値より大きいの値は0に，それ以外は指定した値(Max)になります．
            /// </summary>
            BinaryInv = cv::THRESH_BINARY_INV,

            /// <summary>
            /// しきい値より大きい値はしきい値まで切り詰められ，それ以外はそのまま残ります．
            /// </summary>
            Trunc = cv::THRESH_TRUNC,

            /// <summary>
            /// しきい値より大きい値はそのまま残り，それ以外は0になります．
            /// </summary>
            ToZero = cv::THRESH_TOZERO,

            /// <summary>
            /// しきい値以下の値はそのまま残り，それ以外は0になります．
            /// </summary>
            ToZeroInv = cv::THRESH_TOZERO_INV
        };
    }
}

