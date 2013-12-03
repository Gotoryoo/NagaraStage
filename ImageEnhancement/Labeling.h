/* C++/CLI */

#pragma once
#include "Stdafx.h"
#include "ImageHolder.h"
#include "ThresholdMode.h"
#include "CircumBox.h"

using namespace System::Collections::Generic;

namespace NagaraStage {
    namespace ImageEnhancement {

        /// <summary>
        /// ラベリングを行うクラスです．ラベリングしたデータの保持・管理も行います．
        /// </summary>
        /// <author mail="o1007410@edu.gifu-u.ac.jp">Hirokazu Yokoyama</author>
        public ref class Labeling : public ImageHolder {
        public:
            /// <summary>
            /// 二値化処理を行ってからラベリングを行います．
            /// <para>このインスタンスが持つ画像データは二値化されます．</para>
            /// </summary>
            /// <param name="threshold">二値化しきい値</param>
            /// <param name="threshMode">二値化のモード</param>
            /// <return>ラベリングした数</return>
            int Label(int threshold, ThresholdMode threshMode);

            /// <summary>
            /// ラベリングを行います．
            /// <para>画像データのMax値(255)のピクセルでラベリングを行います．</para>
            /// </summary>
            /// <return>ラベリングした数</return>
            int Label();

            /// <summary>
            /// ラベル値の物体を含む外接四角形を取得します．
            /// </summary>
            /// <param name="labelVal">取得したい物体のラベル値</param>
            /// <return>物体の画像データを含む外接四角形</return>
            /// <exception cref="System::NullReferenceException">
            /// Label()メソッドが実行される前で，必要なインスタンスがNullの場合
            /// </exception
            CircumBox^ GetCircumBox(int labelVal);

            /// <summary>
            /// 面積でソートされたラベル値毎に分類した外接四角形のリストを取得します．
            /// </summary>
            /// <param name="order">ソートする順番を指定，0: 昇順, 1: 降順</param>
            /// <param name="to">0番目から数えて何番目までソートを行うかを指定</param>
            /// <exception cref="System::NullReferenceException">
            /// Label()メソッドが実行される前で，必要なインスタンスがNullの場合
            /// </exception>
            List<CircumBox^>^ GetObjectListSortedByArea(int order, int to);

            /// <param name="order">ソートする順番を指定，0: 昇順, 1: 降順</param>
            /// <exception cref="System::NullReferenceException">
            /// Label()メソッドが実行される前で，必要なインスタンスがNullの場合
            /// </exception>
            List<CircumBox^>^ GetObjectListSortedByArea(int order);

            /// <summary>
            /// ラベリングされたテーブルを取得します．
            /// </summary>
            /// <return>ラベリングされたテーブル</return>
            /// <exception cref="System::NullReferenceException">
            /// Label()メソッドが実行される前で，必要なインスタンスがNullの場合
            /// </exception>
            array<int>^ GetLabeledTable();

            /// <summary>
            /// ラベル値毎に分類した外接四角形のリストを取得します．
            /// </summary>
            /// <exception cref="System::NullReferenceException">
            /// Label()メソッドが実行される前で，必要なインスタンスがNullの場合
            /// </exception>
            property List<CircumBox^>^ ObjectList {
                List<CircumBox^>^ get() {
                    if(!circumBoxList) {
                        throw gcnew System::NullReferenceException(
                            "Execute Label() method before access this property.");
                    }
                    return circumBoxList;
                }
            }

        private:
            array<int>^ labeledArray;
            List<CircumBox^>^ circumBoxList;
            void setLabels(int x, int y, int val);
        };
    }
}

