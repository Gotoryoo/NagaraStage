/**
* @file Stdafx.h
* @brief 必要なライブラリ及びヘッダファイルの読み込み，シンボルの定義などを行います．
* @author Hirokazu Yokoyama <hirokazu.online@gmail.com>
*/

// stdafx.h : 標準のシステム インクルード ファイルのインクルード ファイル、または
// 参照回数が多く、かつあまり変更されない、プロジェクト専用のインクルード ファイル
// を記述します。

#pragma once

#ifdef _DEBUG
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_calib3d248d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_contrib248d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_core248d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_features2d248d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_flann248d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_gpu248d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_haartraining_engined.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_highgui248d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_imgproc248d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_legacy248d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_ml248d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_nonfree248d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_objdetect248d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_photo248d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_stitching248d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_ts248d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_video248d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_videostab248d.lib")
#else
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_calib3d248.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_contrib248.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_core248.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_features2d248.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_flann248.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_gpu248.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_haartraining_engine.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_highgui248.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_imgproc248.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_legacy248.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_ml248.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_nonfree248.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_objdetect248.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_photo248.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_stitching248.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_ts248.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_video248.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv248\\buildxp\\lib\\opencv_videostab248.lib")
#endif


#include "opencv2\\opencv.hpp"


