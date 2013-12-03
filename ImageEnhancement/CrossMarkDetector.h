// ** C++/CLI **
/**
* @file CrossMarkDetector.h
* @author Hirokazu Yokoyama <o1007410@edu.gifu-u.ac.jp>
*/

#pragma once
#include "Stdafx.h"
#include "ImageHolder.h"
#include "ThresholdMode.h"
#include "CircumBox.h"
#include "Labeling.h"

using namespace System;
using namespace System::Collections;
using namespace System::Collections::Generic;

namespace NagaraStage{
    namespace ImageEnhancement {
        /// <summary>
        /// バツ印を検出するためのメソッドを持つクラスです．
        /// </summary>    
        public ref class CrossMarkDetector : ImageHolder {
        public:
            CrossMarkDetector();

            /// <summary>
            /// 検出するための画像の二値化処理を行います．
            /// </summary>
            /// <param name="threshold">二値化のしきい値</param>
            /// <param name="mode">二値化のモード</param>
            void Binarize(int threshold, ThresholdMode mode);

            /// <summary>
            /// 入力画像からバツ印を検出し，その中心座標を取得します．
            /// <para>
            /// このメソッドを実行する前にBinarize()メソッドで二値化処理を行うことをおすすめします．
            /// </para>
            /// <para>
            /// このメソッドは黒色バツ印の周辺に存在すると予測される白色三角形の個数と位
            /// 置からバツ印の重心を算出します．バツ印が画面中央に投影されているという理
            /// 想的な状況では白色三角形は4つあるはずと言えます．4つの白色部分が見つけら
            /// れなかった場合,例外(CrossMarkNotFoundException)が投げられます．
            /// </para>
            /// <para>
            /// 白色部分の個数を数えますが，入力画像によっては白色のノイズが混ざり個数を
            /// 多くが添え手しまうことがあります．そこで，このメソッドでは各白色部分の面
            /// 積(ピクセル数)によってフィルタリングをしています．"ある一定数"より大きい面
            /// 積でなければ白色部分の個数としてカウントしません．"ある一定数"はAreaThreshold
            /// プロパティより設定，または取得することができます．
            /// </para>
            /// </summary>
            /// <return>バツ印の中心座標</return>
            /// <exception cref="CrossMarkNotFoundException">バツ印を検出できなかった場合</exception>
            Windows::Point GetCenter();

            /// <summary>
            /// 面積のしきい値を取得，または設定します．
            /// ここで設定されている値を上回ったもののみが採用されます．
            /// </summary>
            property int AreaThreshold {
                int get() { return areaThreshod; }
                void set(int value) { areaThreshod = value; }
            }

            void Test();
        private:
            Labeling^ labeling;
            int areaThreshod;
            int getNumOfWhiteObjects(List<CircumBox^>^ boxes);
        };
    }
}