﻿/**
* 外部参照ライブラリを呼び出すためのヘッダファイルです．
* このプロジェクトではVisual StudioのProjectの設定で参照ファイルを読み込んではいけません．
* Project設定をわからない人が混乱しないためです．
* @author Hirokazu Yokoyama <o1007410@edu.gifu-u.ac.jp>
* @date 2012-09-10
*/

#ifndef __LIB_PRAGMA_H__
#define __LIB_PRAGMA_H__


#if _Debug
#pragma comment(lib, "opencv_core246d.lib")
#pragma comment(lib, "opencv_highgui246d.lib")
#pragma comment(lib, "opencv_imgproc246d.lib")
#else 
#pragma comment(lib, "opencv_core246.lib")
#pragma comment(lib, "opencv_highgui246.lib")
#pragma comment(lib, "opencv_imgproc246.lib")
#endif

#endif /* __LIB_PRAGMA_H__ */