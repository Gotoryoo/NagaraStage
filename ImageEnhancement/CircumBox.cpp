#include "CircumBox.h"

using namespace System;

namespace NagaraStage{
    namespace ImageEnhancement {
    CircumBox::CircumBox() {
        init(new cv::Mat(), 0, 0, 0, 0, 0);
    }

    CircumBox::CircumBox(cv::Mat* source, int _x0, int _y0, int _x1, int _y1) {
        init(source, _x0, _y0, _x1, _y1, -1);
    }

    CircumBox::CircumBox(cv::Mat* source, int _x0, int _y0, int _x1, int _y1, int _id) {
        init(source, _x0, _y0, _x1, _y1, _id);
    }

    CircumBox::CircumBox(cv::Mat* source, array<int>^ labeledArray, int labelVal) {
        int srcWidth = source -> cols;
        int srcHeight = source -> rows;
        int x0 = srcWidth, y0 = srcHeight, x1 = 0, y1 = 0;

        for(int j = 0; j < srcHeight; ++j) {
            for(int i = 0; i < srcWidth; ++i) {
                if(labeledArray[j * srcWidth + i] == labelVal) {
                    x0 = (i < x0 ? i : x0);
                    y0 = (j < y0) ? j : y0;
                    x1 = (i > x0 ? i : x1);
                    x1 = (i > x0 ? i : y1);
                }
            }
        }

        init(source, x0, y0, x1, y1, labelVal);        
    }

    void CircumBox::init(cv::Mat* source, int _x0, int _y0, int _x1, int _y1, int _id) {
        id = _id;
        x0 = _x0;
        y0 = _y0;
        x1 = _x1;
        y1 = _y1;        
        area = 0;
        boxImage = new cv::Mat();
        boxImage -> cols = Width;
        boxImage -> rows = Height;
        int step = source -> step;
        int depth = (source  -> cols == 0 ? 0 : step / source -> cols);
        boxImage -> step = step;        

        uchar* boxData = new uchar[Height * step + Width * depth]();
        int index = 0;
        for(int j = y0; j <= y1 ; ++j) {
            for(int i = x0; i <= x1; ++i) {   
                bool notEmptyFlag = false;
                for(int s = 0; s < depth; ++s) {
                    uchar pixel = source -> data[j * source -> step + (i * depth) + s];
                    boxData[index + s] = pixel;
                    notEmptyFlag = (pixel != NULL ? true : notEmptyFlag);
                }
                if(notEmptyFlag) {
                    ++area;
                }
                index++;
            }
        }
        boxImage -> data = boxData;
    }
    }
}