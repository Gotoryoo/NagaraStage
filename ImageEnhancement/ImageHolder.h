#pragma once
#include "Stdafx.h"
#include "ImageCoonverter.h"
#include "ImreadMode.h"

using namespace System;
using namespace System::Windows::Media;
using namespace System::Windows::Media::Imaging;

namespace NagaraStage{
    namespace ImageEnhancement {
        /// <summary>
        /// 画像データを保持・管理するクラスです．
        /// </summary>
        public ref class ImageHolder {
        public:
            ImageHolder();

            /// <param name="_image">画像データの配列</param>
            /// <param name="_width">画像の横幅(pixcel)</param>
            /// <param name="_height">画像の縦幅(pixcel)</param>
            /// <param name="_stride">画像のストライド</param>
            ImageHolder(array<uchar>^ _image, int _width, int _height, int _stride);

            /// <param name="_image">ソース画像/param>
            ImageHolder(BitmapImage^ _image);

            /// <param name="_image">画像データ</param>
            /// <param name="_pixcelLength">1ピクセルあたりのバイト数</param>
            ImageHolder(System::Drawing::Bitmap^ _image, int _pixcelLength);

            /// <summary>
            /// インスタンスに画像を設定します．
            /// </summary>
            /// <param name="url">画像ファイルへのパス</param>
            /// <param name="mode">画像ファイルを読み込むときのモード</param>
            void SetImage(String^ url, ImreadMode mode);

            /// <param name="_width">画像の横幅(pixcel)</param>
            /// <param name="_height">画像の縦幅(pixcel)</param>
            /// <param name="_stride">画像のストライド</param>
            void SetImage(array<uchar>^ _image, int _width, int _height, int _stride);

            /// <param name="_image">ソース画像/param>
            void SetImage(BitmapImage^ _image);

            /// <param name="_image">画像データ</param>
            /// <param name="_pixcelLength">1ピクセルあたりのバイト数</param>        
            void SetImage(System::Drawing::Bitmap^ _image, int _pixcelLength);    

            /// <param name="_image">画像データ</param>        
            void SetImage(cv::Mat* _image);

            /// <summary>
            /// 設定している画像をファイルに保存します．
            /// </summary>        
            /// <param name="faileName">保存先</param>
            void Save(String^ fileName);
            cv::Mat* GetCvMatImage();
            //#ifdef _DEBUG
            void ShowWindow();
            //#endif
        protected:
            cv::Mat* image;
        };
    }
}
