#include "ImageHolder.h"
#include "ImageCoonverter.h"

using namespace System::Runtime::InteropServices;

namespace NagaraStage{ 
    namespace ImageEnhancement {
        ImageHolder::ImageHolder() {
        }

        ImageHolder::ImageHolder(array<uchar>^ _image, int _width, int _height, int _stride) {
            SetImage(_image, _width, _height, _stride);        
        }

        ImageHolder::ImageHolder(BitmapImage^ _image) {
            SetImage(_image);
        }

        ImageHolder::ImageHolder(System::Drawing::Bitmap^ _image, int _pixcelLength) {
            SetImage(_image, _pixcelLength);
        }

        void ImageHolder::SetImage(String^ url, ImreadMode mode) {
            const char* cstr = (const char*) (Marshal::StringToHGlobalAnsi(url)).ToPointer();
            std::string stdUrl = cstr;
            Marshal::FreeHGlobal(System::IntPtr((void*)cstr));    
            image = new cv::Mat();
            *image = cv::imread(stdUrl, (int)mode);        
        }

        void ImageHolder::SetImage(array<uchar>^ _image, int _width, int _height, int _stride) {
            image = ImageConverter::ToCvMat(_image, _width, _height, _stride);
        }

        void ImageHolder::SetImage(BitmapImage^ _image) {
            image = ImageConverter::ToCvMat(_image);
        }

        void ImageHolder::SetImage(System::Drawing::Bitmap^ _image, int _pixcelLength) {        
            image = ImageConverter::ToCvMat(_image, _pixcelLength);
        }

        void ImageHolder::SetImage(cv::Mat* _image) {
            image = new cv::Mat();
            *image = _image -> clone();
        }

        void ImageHolder::Save(String^ fileName) {
            const char* cstr = (const char*) (Marshal::StringToHGlobalAnsi(fileName)).ToPointer();
            std::string stdFileName = cstr;
            cv::imwrite(stdFileName, *image);
        }

        cv::Mat* ImageHolder::GetCvMatImage() {
            return image;
        }

        //#ifdef _DEBUG
        void ImageHolder::ShowWindow() {
            cv::namedWindow("test", CV_WINDOW_AUTOSIZE | CV_WINDOW_FREERATIO);
            cv::imshow("test", *image);
            cv::waitKey();
        }
        //#endif
    }
}