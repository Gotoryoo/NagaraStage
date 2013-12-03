// ImageEnhancement.h
// C++/CLI

/**
* @author Hirokazu Yokoyama <o1007410@edu.gifu-u.ac.jp>
*/

#pragma once
#include "Stdafx.h"
#include "ImageCoonverter.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Windows::Media::Imaging;

namespace NagaraStage{
    namespace ImageEnhancement {
        public ref class LibCv {
        public:
            /// <summary>
            /// 引数で与えられたリストの画像全てを足し合わせた画像を生成します．
            /// <para>リストに含まれる画像は，サイズやパレット，フォーマットが同じである必要があります．</para>
            /// </summary>
            /// <param name="sources">画像配列</param>
            /// <return>足しあわされた画像</return>
            static BitmapSource^ Add(List<BitmapImage^>^ sources);

            /// <param name="sources">足しあわす画像へのURI</param>
            static BitmapSource^ Add(List<Uri^>^ sources);
        };
    }
}
