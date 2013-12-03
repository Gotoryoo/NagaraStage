#pragma once
#include "Stdafx.h"

using namespace System;
using namespace System::Drawing;
using namespace System::Drawing::Imaging;
using namespace System::Windows::Media;
using namespace System::Windows::Media::Imaging;

namespace NagaraStage {
    namespace ImageEnhancement{
        /// <summary>
        /// 画像の形式を変換するメソッドを持つクラスです．
        /// <para>
        /// .Net FrameworkのBitmapDataやBitmaImageとOpenCVのIplImageやMat及びbyte配列などを変換します．
        /// </para>
        /// </summary>
        public ref class ImageConverter{
        public:
            /// <summary>
            /// OpenCVのcv::Matクラスに変換したインスタンスを返します．
            /// </summary>
            /// <param name="image">画像データ</param>
            /// <param name="width">横ピクセル数</param>
            /// <param name="height">縦ピクセル数</param>
            /// <param name="stride">画像のストライド</param>
            /// <return>cv::Matインスタンス</return>
            static cv::Mat* ToCvMat(array<uchar>^ image, int width, int height, int stride);

            /// <param name="image">画像データ</param>
            /// <return>cv::Matインスタンス</return>
            /// <exception cref="System::ArgumentException">対応していない形式の場合</exception>
            static cv::Mat* ToCvMat(BitmapImage^ image);

            /// <param name="image">画像データ</param>
            /// <param name="pixcelLength">1ピクセルあたりのバイト数</param>
            /// <return>cv::Matインスタンス</return>
            /// <exception cref="System::ArgumentException">対応していない形式の場合</exception>
            static cv::Mat* ToCvMat(Bitmap^ image, int pixcelLength);

            /// <param name="image">画像データ</param>        
            /// <return>cv::Matインスタンス</return>
            static cv::Mat* ToCvMat(IplImage* iamge);

            /// <summary>
            /// OpenCVのcv::MatクラスをSystem::Windows::Media::Imaging::BitmapSourceに変換します．
            /// </summary>
            static BitmapSource^ ToBitmapSource(cv::Mat* image,
                                                BitmapPalette^ pallete, 
                                                System::Windows::Media::PixelFormat format,                                                
                                                double dpiX, double dpiY);

            static BitmapSource^ ToBitmapSource(cv::Mat* image,
                                                BitmapPalette^ pallete, 
                                                System::Windows::Media::PixelFormat format);

            /// <summary>
            /// 与えられたBitmapImageに対応したcv::Matに設定する
            /// チャンネルのコードを取得します．
            /// </summary>
            /// <param name="image">画像</param>
            /// <return>OpenCVのチャンネルコード</return>
            /// <exception cref="System::ArgumentException">対応していない形式の場合</exception>
            static int GetChannelType(BitmapImage^ source);

            /// <param name="image">画像</param>
            /// <return>OpenCVのチャンネルコード</return>
            static int GetChannelType(Bitmap^ source);       
        private:
            /// <summary>
            /// 1ピクセルあたりの長さを返します．
            /// <para>但し，引数インスタンスのFormatプロパティの値がGid,IndexedなどはArgumentExceptionになります．</para>
            /// </summary>
            /// <param name="image">画像データ</param>
            /// <return>1ピクセルあたりの長さ</return>
            /// <exception cref="System::ArgumentException"></exception>
            static int getDepth(BitmapImage^ image);
        };
    }
}
