#include "Sort.h"

namespace NagaraStage {
    namespace ImageEnhancement {

        void Sort::SortNomal(array<int>^ source, int start, int end) {
            int index = start;
            int topValIndex = start;
            for(int i = 0; i < end; ++i) {
                for(int n = index; n < source -> Length; ++n) {
                    topValIndex = (source[topValIndex] < source[n] ? n : topValIndex);
                }

                int a = source[index];                
                source[index] = source[topValIndex];
                source[topValIndex] = a;
                index++;
                topValIndex = index;
            }
        }

        void Sort::SortDescread(array<int>^ source, int start, int end) {
            int index = start;
            int topValIndex = start;
            for(int i = 0; i < end; ++i) {
                for(int n = index; n < source -> Length; ++n) {
                    topValIndex = (source[topValIndex] > source[n] ? n : topValIndex);
                }

                int a = source[index];                
                source[index] = source[topValIndex];
                source[topValIndex] = a;
                index++;
                topValIndex = index;
            }
        }

        void Sort::SortCircumBoxByArea(List<CircumBox^>^ boxes, int start, int end) {
            int index = start;
            int topValIndex = start;
            for(int i = 0; i < end; ++i) {
                for(int n = index; n < boxes -> Count; ++n) {
                    topValIndex = (boxes[topValIndex] -> Area < boxes[n] -> Area ? n : topValIndex);
                }

                CircumBox^ a = boxes[index];                
                boxes[index] = boxes[topValIndex];
                boxes[topValIndex] = a;
                index++;
                topValIndex = index;
            }
        }

        void Sort::SortDesvreadCircumBoxByArea(List<CircumBox^>^ boxes, int start, int end) {
            int index = start;
            int topValIndex = start;
            for(int i = 0; i < end; ++i) {
                for(int n = index; n < boxes -> Count; ++n) {
                    topValIndex = (boxes[topValIndex] -> Area > boxes[n] -> Area ? n : topValIndex);
                }

                CircumBox^ a = boxes[index];                
                boxes[index] = boxes[topValIndex];
                boxes[topValIndex] = a;
                index++;
                topValIndex = index;
            }
        }
    }
}