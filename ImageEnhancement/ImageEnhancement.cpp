
#include "stdafx.h"

#include "ImageEnhancement.h"

namespace NagaraStage {
    namespace ImageEnhancement {
        BitmapSource^ LibCv::Add(List<BitmapImage^>^ sources) {
            if(sources[0] == nullptr) {
                throw gcnew NullReferenceException();
            }
            cv::Mat* dst = NagaraStage::ImageEnhancement::ImageConverter::ToCvMat(sources[0]);
            for(int i = 1; i < sources -> Count; ++i) {
                cv::Mat* src = NagaraStage::ImageEnhancement::ImageConverter::ToCvMat(sources[i]);
                cv::add(*dst, *src, *dst);
            }
            return NagaraStage::ImageEnhancement::ImageConverter::ToBitmapSource(dst, 
                sources[0]->Palette, 
                sources[0]->Format);
        }
        
        BitmapSource^ LibCv::Add(List<Uri^>^ sources) {
            if(sources[0] == nullptr) {
                throw gcnew NullReferenceException();
            }
            
            BitmapImage^ bi = gcnew BitmapImage(sources[0]);
            cv::Mat* dst = NagaraStage::ImageEnhancement::ImageConverter::ToCvMat(bi);
            for(int i = 1; i < sources -> Count; ++i) {
                bi = gcnew BitmapImage(sources[i]);
                cv::Mat* src = NagaraStage::ImageEnhancement::ImageConverter::ToCvMat(bi);
                cv::add(*dst, *src, *dst);
            }
            return NagaraStage::ImageEnhancement::ImageConverter::ToBitmapSource(dst,
                bi -> Palette,
                bi -> Format);
        }
    }
}