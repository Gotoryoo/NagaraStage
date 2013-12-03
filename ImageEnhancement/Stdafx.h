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
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_calib3d246d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_contrib246d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_core246d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_features2d246d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_flann246d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_gpu246d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_haartraining_engined.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_highgui246d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_imgproc246d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_legacy246d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_ml246d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_nonfree246d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_objdetect246d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_photo246d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_stitching246d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_ts246d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_video246d.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_videostab246d.lib")
#else
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_calib3d246.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_contrib246.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_core246.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_features2d246.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_flann246.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_gpu246.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_haartraining_engine.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_highgui246.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_imgproc246.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_legacy246.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_ml246.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_nonfree246.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_objdetect246.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_photo246.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_stitching246.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_ts246.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_video246.lib")
#pragma comment(lib, "C:\\OpenCV\\opencv246\\build\\x86\\vc10\\lib\\opencv_videostab246.lib")
#endif


#include "opencv2\\opencv.hpp"


