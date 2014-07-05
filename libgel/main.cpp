#include <iostream>
#include "define.h"
#include "surface_rec.h"
#include <math.h>
#include <stdarg.h>
#include "opencv2\opencv.hpp"
#if _LibTest
#include "lib_pragma.h"
#endif

#if _LibTest
/**
* 動作確認用
*/
int main(int argc, char* args[]) {
	args[1] = "I:\\picture\\56-3-2\\56-3-2-1.bmp";
	std::cout << "hello @" << args[1] << std::endl;
	int brightness = 0;
	IplImage* iplImage = cvLoadImage(args[1], CV_LOAD_IMAGE_GRAYSCALE);
	NagaraStage::Image* image = new NagaraStage::Image((unsigned char*)iplImage->imageData, iplImage->width, iplImage->height);
	for (int startRow = 0; startRow < 600; ++startRow) {
		for(int endRow = 0; endRow < 600; ++endRow) {
			try{
				std::cout<<"row, col = " << startRow << ", " << endRow << " ";
				int length = 9;
				brightness = image -> sumBrightness(startRow, endRow, length);
				std::cout << "ok" << std::endl;
			} catch(char * str) {
				std::cout << str<< std::endl;
			}
		}
	}

	std::cout<< args[1] <<": brightness value is " << brightness << std::endl;
	return brightness;
}
#endif

DLLEXPORT bool __stdcall NagaraStage::isInGel(
	unsigned char *imageData,
	int width, int height,
	int startRow, int endRow,
	int lengthOfSide, 
	int threshold0, int threshold1,
	double powerOfDifference){
		//cv::Mat mat = cv::Mat(height,width,CV_8U,imageData);
		//cv::Mat gau = mat.clone();
		//cv::GaussianBlur(gau, gau, cv::Size(31,31), 0);
		//cv::subtract(gau, mat, mat);
		//cv::threshold(mat, mat, threshold1, 1, cv::THRESH_BINARY);
		
		cv::Mat mat = cv::Mat::ones(height,width,CV_8U);
		int brightness = cv::countNonZero(mat);

		//NagaraStage::Image *image = new Image(imageData, width, height);
		//image->setPowerOfDifference(powerOfDifference);
		//image->setThreshold(threshold1);
		//int brightness = image->sumBrightness(startRow, endRow, lengthOfSide);
		//delete image;
		return (brightness > threshold0);
		return false;
}

DLLEXPORT bool __stdcall NagaraStage::isInGelBrightness(
	unsigned char *imageData,
	int width, int height,
	int startRow, int endRow,
	int *brightness0,
	int lengthOfSide,
	int threshold0, int threshold1,
	double powerOfDifference){        
		//NagaraStage::Image *image = new Image(imageData, width, height);        
		//image->setPowerOfDifference(powerOfDifference);
		//image->setThreshold(threshold1);
		//*brightness0 = image->sumBrightness(startRow, endRow, lengthOfSide);

		//cv::Mat mat = cv::Mat(height,width,CV_8U,imageData);
		//cv::Mat gau = mat.clone();
		//cv::GaussianBlur(gau, gau, cv::Size(31,31), 0);
		//cv::subtract(gau, mat, mat);
		//cv::threshold(mat, mat, threshold1, 1, cv::THRESH_BINARY);
		
		cv::Mat mat = cv::Mat::ones(height,width,CV_8U);
		*brightness0 = cv::countNonZero(mat);
		//delete image;
		return false;
		//return (*brightness0 > threshold0);        
}



NagaraStage::Image::Image(unsigned char *_image, int _width, int _height) {
	this->image = _image;
	this->width = _width;
	this->height = _height;
}

NagaraStage::Image::~Image() {
	//delete this->image;
}

int NagaraStage::Image::sumExtractedImage(int centerCol, int centerRow, int length) {
	if(length % 2 == 0) {
		throw "Exception: value oflength must be odd number.";
	}
	if(!isInRange(centerCol, centerRow, length)) {
		throw "Given parameters are out of range.";
	}

	lengthOfSides = length;
	presentCol = centerCol;
	presentRow = centerRow;
	int halfLength = length / 2;
	int sum = 0;
	unsigned char *window = new unsigned char[length * length];
	for(int x = centerCol - halfLength; x <= centerCol + halfLength; ++x) {
		for(int y = centerRow - halfLength; y <= centerRow + halfLength; ++y) {            
			sum += getPixcelValue(x, y);
		}
	}
	delete window; 
	presentSum = sum;
	return sum;
}



int NagaraStage::Image::sumBrightness(int startRow, int endRow, int sideLength) {
	if(!isPositiveNumbers(3, startRow, endRow, sideLength)) {
		throw "parameter value must be positive numbers.";
	}

	int sideMargin = sideLength / 2;
	int size = sideLength * sideLength;
	brightness = 0;
	for(int row = startRow; row < endRow; ++row) {
		for(int col = sideMargin; col < width - sideMargin; ++col) {
			int windowSum = sumExtractedImage(col, row);
			double diff = abs((double)getPixcelValue(col, row) - (windowSum / size));
			if(threshold > 0) {
				diff = (diff > threshold ? 1 : 0);
			}
			brightness += (int)pow(diff, powerOfDifference);
		}
	}
	return brightness;
}

unsigned char NagaraStage::Image::getPixcelValue(int col, int row) {
	return image[row * width + col];
}

int NagaraStage::Image::getBrightness() {
	return brightness;
}

bool NagaraStage::Image::isInRange(int centerCol, int centerRow, int length) {
	int halfLength = length / 2;
	return (centerCol - halfLength >= 0
		&& centerRow - halfLength >= 0
		&& centerCol + halfLength < width
		&& centerRow + halfLength < height
		&& length > 0);
}

bool NagaraStage::Image::isPositiveNumbers(int num, ...) {
	bool retval = true;
	va_list ap;
	va_start(ap, num);
	for(int i = 0; i < num; ++i) {
		retval &= (va_arg(ap, int) >=0);        
	}
	va_end(ap);
	return retval;
}

void NagaraStage::Image::setPowerOfDifference(double power) {
	powerOfDifference = power;
}

void NagaraStage::Image::setThreshold(int threshold) {
	this -> threshold = threshold;
}