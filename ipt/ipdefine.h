/**
* このプロジェクトで用いる定数をプリプロセッサで定義と必要なライブラリの取得を行なっています. 
* @author Tomonaga Wada, Hirokazu Yokoyama
* @date 2012-07-11
*/

#ifndef __IPDEFINE_H__
#define __IPDEFINE_H__

#ifdef _WIN32
# define DLLEXPORT __declspec(dllexport)
# define DLLIMPORT __declspec(dllimport)
#else
# define DLLEXPORT
# define DLLIMPORT
# define __stdcall
#endif


#pragma comment(lib, "ws2_32.lib")
#pragma comment(lib, "VP900CMD.lib")
//#pragma comment(lib, "VP900STD.lib")

#define IP_NORMAL     0    /* Normal Return */
#define IP_ERROR      -1   /* Error Return */
#define IP_PARA_ERR   100  /* Parameter Error */
#define IP_ALLOC_ERR  100  /* Allocate Error */

#define IP_INPUT_WIDTH  512
#define IP_INPUT_HEIGHT 480
#define IP_DRAW_WIDTH   512
#define IP_DRAW_HEIGHT  440
#define IP_INPUT_SIZE   (IP_INPUT_WIDTH * IP_INPUT_HEIGHT)
#define IP_DRAW_SIZE    (IP_DRAW_WIDTH * IP_DRAW_HEIGHT)
#define IP_DRAW_WIDTH2  (IP_DRAW_WIDTH * 2)
#define IP_HALF_WIDTH   (IP_DRAW_WIDTH / 2)
#define IP_HALF_HEIGHT  (IP_DRAW_HEIGHT / 2)
#define IP_HALF_SIZE    (IP_HALF_WIDTH * IP_HALF_HEIGHT)
#define IP_QUAT_WIDTH   (IP_DRAW_WIDTH / 4)
#define IP_QUAT_HEIGHT  (IP_DRAW_HEIGHT / 4)
#define IP_QUAT_SIZE    (IP_QUAT_WIDTH * IP_QUAT_HEIGHT)
#define IP_6_WIDTH      ((IP_DRAW_WIDTH / 6) +1)
#define IP_6_HEIGHT     ((IP_DRAW_HEIGHT / 6) +1)

#define XYSEGPIX		6
#define XYSEGPIX2		12
#define XYSEGPIX3		18
#define XSEGPIX			6
#define YSEGPIX			6
#define SEGPIX_HALF		3
#define SEGROW			86
#define SEGCOLLUM		74
#define SEGSIZE			(SEGROW*SEGCOLLUM)
#define SEGINTSIZE		(SEGSIZE*sizeof(int))

#define EMThinType		1
#define EMThickType		2

#define STRLEN			144

#define NumOfStep 8

#ifdef LINUX
typedef unsigned long LONG;
typedef int HWND;
typedef int HDC;
typedef int RGBQUAD;
typedef struct {
  RGBQUAD *bmiColors;
} *LPBITMAPINFO;
#endif

typedef struct {
  double x;
  double y;
} Vector2;
typedef Vector2 point;

typedef struct {
  int x, y;
  int br;
} HIT;

typedef struct {
  double xmin, xmax;
  double ymin, ymax;
} lect;

typedef struct{
  HIT *hit;
} OnePlaneData;
typedef struct{
  double  x [NumOfStep];
  double  y [NumOfStep];
  double  z [NumOfStep];
  int     br[NumOfStep];
  int nPixel[NumOfStep];
  OnePlaneData data[NumOfStep];
  int brSum;
  int nPlane;
  int flag;
  double dxPixelSearch, dyPixelSearch;
} PlaneData;

typedef struct{
  double upperGel;
  double base;
  double lowerGel;
} Thickness;
typedef struct{
  Vector2 originalPosition;
  Vector2 correctedPosition;
  Vector2 offset;
  Thickness thickness;
} GridData;

#endif  /* __IPDEFINE_H_ */
