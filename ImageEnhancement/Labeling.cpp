#include "Labeling.h"
#include "Sort.h"


namespace NagaraStage {
    namespace ImageEnhancement {

        int Labeling::Label(int threshold, ThresholdMode threshMode) {
            cv::threshold(*image, *image, threshold, 255, (int)threshMode);
            return Label();
        }

        int Labeling::Label() {
            int width = image -> cols;
            int height = image -> rows; 
            int step = image -> step;
            int depth = step / width;
            int labelValue = 0;
            labeledArray = gcnew array<int>(width * height);
            circumBoxList = gcnew List<CircumBox^>;
            uchar* data = image -> data;

            /* 左上から右下にすべてのピクセルを走査していく．ピクセルが0(黒)以外の場合にラベリングを行う．*/
            // 1. ピクセルが0以外で，ラベリングされていなければラベル値を更新し新しくラベリングを行う．
            // 2. ラベリングを行った周囲のピクセルを調べ，周囲のピクセルも0以外であれば同じラベル値を割り当てる．
            // 3. 2.の処理を周囲の0でないピクセルすべてが同一のラベル値を割り当てていく．
            // 4. 1.に戻る．
            for(int y = 0; y < height; ++y) {
                for(int x = 0; x < width; ++x) {
                    // ピクセルが0(黒)の場合，次のピクセルへ
                    if(data[(y * step) + (x * depth)] == 0) {
                        continue;
                    }
                    // ラベルが割り当て済みであれば次のピクセルへ
                    if(labeledArray[y * width + x] != 0) {
                        continue;
                    }

                    // ピクセルが0以外(白)の場合，ラベルを割り当てる
                    ++labelValue;                
                    setLabels(x, y, labelValue);
                }
            }        

            return labelValue;
        }

        void Labeling::setLabels(int x, int y, int val) {
            int width = image -> cols;
            int height = image -> rows;
            int step = image -> step;
            int depth = step / width;
            uchar* data = image -> data;
            int count = 1;
            int x0 = width, y0 = height, x1 = 0, y1 = 0;

            labeledArray[y * width + x] = val;
            while(count > 0) {
                count = 0;
                for(int j = 0; j < height; ++j) {
                    for(int i = 0; i < width; ++i) {
                        if(labeledArray[j * width + i] == val) {
                            // 外接四角形を定義するためにラベリングしている物体の左上座標と
                            // 右下座標を算出する
                            x0 = (i < x0 ? i : x0);
                            y0 = (j < y0 ? j : y0);
                            x1 = (i > x1 ? i : x1);
                            y1 = (j > y1 ? j : y1);

                            // ラベリング処理
                            int startX = (i <= 0 ? 0 : i - 1);
                            int startY = (j <= 0 ? 0 : j - 1);
                            int endX = (i >= width - 1 ? width - 1 : i + 1);
                            int endY = (j >= height -1 ? height - 1 : j + 1);
                            for(int m = startY; m <= endY; ++m) {
                                for(int n = startX; n <= endX; ++n) {
                                    if(m == j && n == i) {
                                        continue;
                                    }
                                    if(data[m * step + n * depth] != 0 
                                        && labeledArray[m * width + n] == 0) {
                                            labeledArray[m * width + n] = val;
                                            count++;
                                    }
                                } // for n
                            } // for m
                        } // if
                    } // for i
                } // for j           
            }

            circumBoxList -> Add(gcnew CircumBox(image, x0, y0, x1, y1, val));

        }

        CircumBox^ Labeling::GetCircumBox(int labelVal) {
            if(!circumBoxList) {
                throw gcnew NullReferenceException(
                    "CircumBoxList is null reference. Execute Label() method before this method.");
            }

            --labelVal;        
            if(labelVal >= circumBoxList -> Count) {
                throw gcnew ArgumentOutOfRangeException("Specified Label value is out of range.");
            }
            return circumBoxList[labelVal];
        }

        List<CircumBox^>^ Labeling::GetObjectListSortedByArea(int order, int to) {
            to = (to > ObjectList -> Count ? ObjectList -> Count : to);

            array<int>^ areaList = gcnew array<int>(ObjectList -> Count);
            for(int i = 0; i < areaList -> Length; ++i) {
                areaList[i] = ObjectList[i] -> Area;
            }

            List<CircumBox^>^ sortedBoxesList = gcnew List<CircumBox^>(to);
            for(int i = 0; i < to; ++i) {
                sortedBoxesList -> Add(gcnew CircumBox());
                sortedBoxesList[i] = ObjectList[i];
            }
            switch (order) {
            case 0: // 昇順にソート
                Sort::SortCircumBoxByArea(sortedBoxesList, 0, to);
                break;
            case 1: // 降順にソート
                Sort::SortCircumBoxByArea(sortedBoxesList, 0, to);
                break;
            default:
                throw gcnew ArgumentException("'order' must be 0 or 1.");
            }

            return sortedBoxesList;
        }

        List<CircumBox^>^ Labeling::GetObjectListSortedByArea(int order) {
            return GetObjectListSortedByArea(order, ObjectList -> Count);
        }

        array<int>^ Labeling::GetLabeledTable() {
            if(!labeledArray) {
                throw gcnew NullReferenceException(
                    "The table is null reference. Execute Label() method before this method.");
            }
            return (array<int>^)labeledArray -> Clone();
        }
    }
}




