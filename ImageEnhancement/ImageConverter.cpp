#include "ImageCoonverter.h"

using namespace System::Runtime::InteropServices;

namespace NagaraStage{
    namespace ImageEnhancement {

        cv::Mat* ImageConverter::ToCvMat(array<uchar>^ image, int width, int height, int stride) {
            cv::Mat* mat = new cv::Mat();
            mat -> cols = width;
            mat -> rows = height;
            mat -> step = stride;

            pin_ptr<uchar> ptr = &image[0];
            mat -> data = ptr;
            ptr = nullptr;
            return mat;
        }

        cv::Mat* ImageConverter::ToCvMat(BitmapImage^ image) {
            int depth = getDepth(image);
            int width = image -> PixelWidth;
            int height = image -> PixelHeight;
            cv::Mat* mat = new cv::Mat(height, width, GetChannelType(image));

            int length = image -> PixelWidth * image -> PixelHeight * depth;
            array<uchar>^ buf = gcnew array<uchar>(length);
            image -> CopyPixels(buf, image -> PixelWidth * depth, 0);

            pin_ptr<uchar> ptr = &buf[0];        
            mat -> data = ptr;
            ptr = nullptr;

            return mat;
        }

        cv::Mat* ImageConverter::ToCvMat(Bitmap^ image, int pixcelLength) {
            int width = image -> Width;
            int height = image -> Height;
            cv::Mat* mat = new cv::Mat(height, width, GetChannelType(image));
            BitmapData^ bmpdata = image -> LockBits(
                System::Drawing::Rectangle(Point::Empty, image -> Size),
                ImageLockMode::ReadOnly,
                image -> PixelFormat
                );        

            int length = image -> Width * image -> Height * pixcelLength;
            array<uchar>^ buf = gcnew array<uchar>(length);
            Marshal::Copy(bmpdata -> Scan0, buf, 0, buf -> Length);

            pin_ptr<uchar> ptr = &buf[0];
            mat -> data = ptr;
            ptr = nullptr;

            return mat;
        }

        cv::Mat* ImageConverter::ToCvMat(IplImage* image) {
            cv::Mat* mat = new cv::Mat();
            mat -> cols = image -> width;
            mat -> rows = image -> height;
            mat -> step = image -> widthStep;
            mat -> data = (uchar*)image -> imageData;
            return mat;
        }

        BitmapSource^ ImageConverter::ToBitmapSource(cv::Mat* image,
                                                BitmapPalette^ pallete, 
                                                System::Windows::Media::PixelFormat format,
                                                double dpiX, double dpiY) { 
            array<uchar>^ buf = gcnew array<uchar>(image->step * image->rows);
            for(int i = 0; i < buf -> Length; ++i) {
                buf[i] = image -> data[i];
            }
            return BitmapSource::Create( image -> cols, image -> rows,
                                         dpiX, dpiY, format, pallete, buf, image->step);
        }

        BitmapSource^ ImageConverter::ToBitmapSource(cv::Mat* image,
                                                BitmapPalette^ pallete, 
                                                System::Windows::Media::PixelFormat format) {           
            return ToBitmapSource(image, pallete, format, 96, 96);
        }

        int ImageConverter::GetChannelType(BitmapImage^ source) {
            int retVal = 0;
            System::Windows::Media::PixelFormat format = source -> Format;
            if(format == System::Windows::Media::PixelFormats::Bgr32 
                || format == System::Windows::Media::PixelFormats::Bgra32) {
                    retVal = CV_8UC4;
            } else if(format == System::Windows::Media::PixelFormats::Bgr24) {
                retVal = CV_8UC3;
            } else if(format == System::Windows::Media::PixelFormats::Gray8 
                || format == System::Windows::Media::PixelFormats::Indexed8) {
                    retVal = CV_8UC1;
            } else {
                throw gcnew ArgumentException("The Source is not correspondent.");
            }

            return retVal;
        }

        int ImageConverter::GetChannelType(Bitmap^ source) {
            int retVal = 0;
            System::Drawing::Imaging::PixelFormat format = source -> PixelFormat;
            switch (format)  {        
            case System::Drawing::Imaging::PixelFormat::Format32bppArgb:
                retVal = CV_8UC4;
                break;
            case System::Drawing::Imaging::PixelFormat::Format24bppRgb:
                retVal = CV_8UC3;
                break;
            case System::Drawing::Imaging::PixelFormat::Format8bppIndexed:
                retVal = CV_8UC1;        
                break;
            default:
                throw gcnew ArgumentException("The Source is not correspondent.");
            }
            return retVal;
        }    

        int ImageConverter::getDepth(BitmapImage^ image) {
            int depth = 0;
            System::Windows::Media::PixelFormat format = image -> Format;
            if (format == System::Windows::Media::PixelFormats::Pbgra32
                || format == System::Windows::Media::PixelFormats::Bgr32) {
                    depth = 4;
            } else if(format == System::Windows::Media::PixelFormats::Rgb24) {    
                depth = 3;
            } else if(format ==System::Windows::Media::PixelFormats::Gray8
                || format == System::Windows::Media::PixelFormats::Indexed8 ) {
                    depth = 1;
            } else {
                throw gcnew ArgumentException();
            }
            return depth;
        }
    }
}