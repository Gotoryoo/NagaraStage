#include "CrossMarkDetector.h"
#include "Direction.h"
#include "RecognizeFailedException.h"

namespace NagaraStage{
    namespace ImageEnhancement {
        CrossMarkDetector::CrossMarkDetector() {
            areaThreshod = 400;
            labeling = gcnew Labeling();
        }

        void CrossMarkDetector::Binarize(int threshold, ThresholdMode mode) {
            if(!image) {
                throw gcnew NullReferenceException();
            }

            cv::threshold(*image, *image, threshold, 255, (int)mode);
        }

        Windows::Point CrossMarkDetector::GetCenter() {
            if(!image) {
                throw gcnew NullReferenceException(
                    "Source image is null reference. Execute SetImage() method before this method.");
            }

            array<CircumBox^>^ hide = {gcnew CircumBox(), gcnew CircumBox()};
            array<CircumBox^>^ side = gcnew array<CircumBox^>(2){gcnew CircumBox(), gcnew CircumBox()};        
            labeling = gcnew Labeling();
            labeling -> SetImage(image);
            labeling -> Label();
            List<CircumBox^>^ whiteObjects = labeling -> GetObjectListSortedByArea(0, 4);
            int numOfWhiteObjects = getNumOfWhiteObjects(whiteObjects);
            if(numOfWhiteObjects != 4) {
                throw gcnew RecognizeFailedException();
            }

            int m = 0, n = 0;
            for(int i = 0; i < whiteObjects -> Count; ++i) {
                if(whiteObjects[i] -> Width < whiteObjects[i] -> Height) {
                    side[m++] = whiteObjects[i];
                } else {
                    hide[n++] = whiteObjects[i];
                }
            }

            int averageX = 0, averageY = 0;
            if((side[0] -> X0) < (side[1] -> X0)) {            
                averageX = ((side[0] -> X1) + (side[1] -> X0)) / 2;
            } else {
                averageX = ((side[1] -> X1) + (side[0] -> X0)) / 2;
            }
            if(hide[0] -> Y0 < hide[1] -> Y0) {
                averageY = ((side[0] -> Y1) + (side[1] -> Y0)) / 2;
            } else {
                averageY = ((side[1] -> Y1) + (side[0] -> Y0)) / 2;
            }

            Windows::Point^ center = gcnew Windows::Point(averageX, averageY);
            return *center;
        }

        int CrossMarkDetector::getNumOfWhiteObjects(List<CircumBox^>^ boxes) {
            int retVal = 0;
            System::Console::WriteLine(boxes -> Count);
            for(int i = 0; i < boxes -> Count; ++i) {
                System::Console::Write(boxes[i] -> Area + ":" + boxes[i] -> Id + ", ");            
                if(boxes[i] -> Area >= areaThreshod) {
                    ++retVal;
                }
            }
            return retVal;
        }

        void CrossMarkDetector::Test() {
            Labeling^ labeling = gcnew Labeling();
            labeling -> SetImage(image);
            int numOfLabel = labeling -> Label();
            List<CircumBox^>^ whiteObjects = labeling -> GetObjectListSortedByArea(0);
            int numOfWhiteObjects = getNumOfWhiteObjects(whiteObjects);
            System::Console::WriteLine("Num of objects: " + numOfLabel);
            System::Console::WriteLine(numOfWhiteObjects);
            array<int>^ table = labeling -> GetLabeledTable();
            array<uchar>^ t2 = gcnew array<uchar>(512 * 440);
#include "ImageCoonverter.h"
            for(int i = 0; i < table -> Length; ++i) {
                t2[i] = (uchar)table[i];
            }
            cv::Mat* t = ImageConverter::ToCvMat(t2, 512, 440, 512);
            cv::namedWindow("test", CV_WINDOW_AUTOSIZE | CV_WINDOW_FREERATIO);
            cv::imshow("test", *t);
            cv::waitKey();
        }
    }
}