/**
* @brief 入力された画像がエマルションのゲル内かそれ以外かを判定する関数，クラスを定義した
* ヘッダファイルです．
*
* @detauls
* <h1>アルゴリズム</h1>
* <p>入力画像から何行か(10行程度が望ましい)を抽出する．抽出した部分の各ピクセルについて，
* その周囲の数ピクセル(デフォルトでは9ピクセル)と比較する．具体的には次の式を用いる．</p>
* <p>
* abs (中央のピクセル値 - 周囲のピクセルの平均)
* </p>
* <p>この値を抽出した部分の各ピクセルについて計算し，その合計値がしきい値(11000)であ
* ればゲル内であると判定する．</p>
* <h1>留意点</h1>
* <p>このモジュールが持つ関数は入力画像がゲル内かそうでないかを判定するのみであり，
* カメラがいまどこにいるのか，境界面であるのかといったことはあつかはないため，別途
* 実装する必要がある．</p>
*
* @author Hirokazu Yokoyama <o1007410@edu.gifu-u.ac.jp>
* @date 2012-09-11
*/

#define CvDebug 1

#include "define.h"
#include <iostream>
#if _LibTest
#include <opencv2/opencv.hpp>
#endif

#ifndef __SURFACEREC_H__
#define __SURFACEREC_H__

namespace NagaraStage {

/**
* 入力画像を保持するクラスです．
*/
class Image {
public:
    /**
    * コンストラクタ
    * @param image
    * @param width
    * @param height;
    */
    Image(unsigned char *_image, int _width, int _height);
    ~Image();

    /** 抽出する画像の辺のピクセル数 */
    static const int LengthOfSizeToExtracted = 9;

    /**
    * 指定した行，列のピクセルの値を取得します．
    * @param col 取得する行
    * @param row 取得する列
    * @return ピクセルの値
    */
    unsigned char getPixcelValue(int col, int row);

    /**
    * 入力画像から一部分を抽出し，その抽出した画像の各ピクセルの合計値を取得します．
    * @param centerCol 抽出する箇所の中心行(0から数えて)
    * @param centerRow 抽出する箇所の中心列(0から数えて)
    * @param length 抽出する画像の辺の長さ(奇数にすること), 初期値はLenghtOfSizeToExtractedを参照
    * @return 抽出した画像の各ピクセルの合計値
    * @exception lengthの値及びcenterCol,centerRowの値が元画像からはみ出す値を参照した場合
    * @exception lengthの値が偶数の場合
    */
    int sumExtractedImage(int centerCol, int centerRow, int length = LengthOfSizeToExtracted);

    /**
    * 入力画像の表面判定を行うための輝度値を算出します．
    * @param startRow 判定に用いる範囲の開始行(0行目からカウント)
    * @param endRow 判定に用いる範囲の終了行(0行目からカウント)
    * @param sideLength 判定に用いるために抽出する画像の辺の長さ(pixcel)
    */
    int sumBrightness(int startRow, int endRow, int sideLength);

    /**
    * subBrightnessを行った結果を取得します．
    * @return 輝度値の合計
    */
    int getBrightness();

    /**
    * 周囲の画素との微分の累乗値を設定します．
    * @param power 累乗値
    */
    void setPowerOfDifference(double power);

    /**
    * 輝度値の合計に対する二値化のしきい値を設定します．
    * 0以下の値を指定した場合，二値化処理が行われなくなります．．
    */
    void setThreshold(int threshold);

private:
    unsigned char* image;
    int width;
    int height;
    int sumOfLeftSide;
    int presentRow;
    int presentCol;
    int presentSum;
    int lengthOfSides;
    int brightness;
    int threshold;
    double powerOfDifference;

    /**
    * 要求された行，列，および長さが抽出可能な範囲であるかどうかを取得します．
    * @return true: 可能である，false: 不可能である
    */
    bool isInRange(int centerCol, int centerRow, int length);

    /**
    * 2個目以降の引数がすべて正の値であるかどうかを取得します．
    * @param num 2個目以降の引数の数
    * @param ... 調べる値
    */
    bool isPositiveNumbers(int num, ...);
};

/**
* 指定されたエマルション画像がゲルの内部であるのかを判定します．
* ただし，入力画像は1pixcelあたり1byteのモノクロ画像です．
* @details
* 引数に与えられた画像がゲルの内部であればtrueを返します．
* それ以外(ベース含む)であればfalseを返します．
* @param imageData エマルション画像配列のポインタ
* @param width 画像の横幅(pixcel)
* @param height 画像の縦幅(pixcel)
* @param startRow 抽出する範囲の開始行
* @param endRow 抽出する範囲の終了行
* @param lengthOfSide 抽出画像の辺の長さ(default値あり)
* @param threshold0 ゲルの内部であるかどうか判定するしきい値
* @param threshold1 輝度値の合計に対して二値化するかどうかのしきい値，0以下の場合は二値化を行わない
* @return ゲル内であればtrue, そうでなければfalse
*/
DLLEXPORT bool __stdcall isInGel(
    unsigned char *imageData, 
    int width, int height, 
    int startRow, int endRow, 
    int lengthOfSide,
    int threshold0, int threshold1,
    double powerOfDifference);

/**
* @overload
* @param imageData エマルション画像配列のポインタ
* @param width 画像の横幅(pixcel)
* @param height 画像の縦幅(pixcel)
* @param startRow 抽出する範囲の開始行
* @param endRow 抽出する範囲の終了行
* @param lengthOfSide 抽出画像の辺の長さ
* @param threshold0 ゲルの内部であるかどうか判定するしきい値
* @param threshold1 輝度値の合計に対して二値化するかどうかのしきい値，0以下の場合は二値化を行わない
* @param brightness0 [in] 指標となる輝度値を格納するポインタ
* @return ゲル内であればtrue, そうでなければfalse
*/
DLLEXPORT bool __stdcall isInGelBrightness(
    unsigned char *imageData, 
    int width, int height, 
    int startRow, int endRow, 
    int* birghtness0,
    int lengthOfSide,
    int threshold0, int threshold1,
    double powerOfDifference);
}
#endif /* __SURFACEREC_H__ */