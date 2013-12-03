#pragma once
#include "Direction.h"

using namespace System;

namespace NagaraStage{
    namespace ImageEnhancement {
        /// <summary>
        /// 検出・認識ができなかった例外を示すクラスです．
        /// </summary>
        public ref class RecognizeFailedException : Exception {
        public:
            RecognizeFailedException() : Exception(){
            }
            RecognizeFailedException(String^ message) : Exception(message){
            }
            RecognizeFailedException(String^ message, Exception^ innerException)
                : Exception(message, innerException){
            }
        };
    }
}