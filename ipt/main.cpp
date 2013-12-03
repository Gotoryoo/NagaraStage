

#ifdef _WIN32
# include <windows.h>
# include <dos.h>
# include <conio.h>
# include <malloc.h>
# include <memory.h>
#endif

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>

#include "ipdefine.h"
#include "ipproto.h"

#include "ipdefine.h"
#include "ipproto.h"
//#include "imaging.h"

//	VP-910SDK関係の定義ファイル
#include "vpxdef.h"
#include "vpxsys.h"
#include "vpxfnc.h"

#ifndef _WIN32
/* to avoid compile error */
typedef char BYTE;
typedef int HWND;
typedef int HDC;
typedef struct {
  unsigned char rgbRed, rgbGreen, rgbBlue, rgbReserved;
} RGBQUAD;
typedef struct {
  RGBQUAD *bmiColors;
} *LPBITMAPINFO;
typedef struct {
  double left, right, top, bottom;
} RECT;
#endif

static void InitView();
static long IP_ImgGet( IMGID, unsigned char * );
static void IP_ImgPrint(HWND, HDC, unsigned char *);
#ifdef DEBUG
static void DummyDataSet(unsigned char *ImgTbl);
#endif

LONG  DeviceID = 0;
int CameraType = 0;


DLLEXPORT int __stdcall IP_Init(LONG BoardNumber,long *RetCode) {

  IPErrorTbl ErrorTbl;

  if (RetCode == NULL) return IP_PARA_ERR;
  *RetCode = IP_NORMAL;

  //	画像処理ボードのオープン
  DeviceID = OpenIPDevExt(BoardNumber,NULL);
  if (DeviceID != ISPX_NULL) {
    // 画像処理コマンドを初期化する
    if (InitIP(DeviceID) != IP_NORMAL) {
      ReadIPErrorTable(DeviceID,&ErrorTbl);
      *RetCode = ErrorTbl.ErrorCode;
      return IP_ERROR;
    }
  }

  if (InitIPExt(DeviceID) != IP_NORMAL) {
    ReadIPErrorTable(DeviceID,&ErrorTbl);
    *RetCode = ErrorTbl.ErrorCode;
    return IP_ERROR;
  }

  if (AllocateImages() == IP_ERROR) {
      return IP_ERROR;
  }
  InitView();

  // 外部モニター出力の設定を行う（５１２×４４０に変更）
  FreeDispImg(DeviceID);
  AllocDispImg(DeviceID,IMG_FS_512H_512V,BMP_DISPLAY);
  SetDispFrame(DeviceID,INTERLACE,DISP_FS_512H_440V);

  return IP_NORMAL;
}

#if 0
int __stdcall IP_InitIP(long *RetCode)
{
  IPErrorTbl ErrorTbl;

  if (RetCode == NULL) return IP_PARA_ERR;
  *RetCode = IP_NORMAL;

  if (InitIP() != IP_NORMAL) {
    ReadIPErrorTable(&ErrorTbl);
    *RetCode = ErrorTbl.ErrorCode;
    return IP_ERROR;
  }

  return IP_NORMAL;
}

int __stdcall IP_InitIPExt(long *RetCode)
{
  IPErrorTbl ErrorTbl;

  if (RetCode == NULL) return IP_PARA_ERR;

  *RetCode = IP_NORMAL;
  if (InitIPExt() != IP_NORMAL) {
    ReadIPErrorTable(&ErrorTbl);
    *RetCode = ErrorTbl.ErrorCode;
    return IP_ERROR;
  }

  return IP_NORMAL;
}
#endif

/*============================================================================*/
/*   Copyright(C)  Hitachi Engineering Co.,Ltd.                               */
/*                 1998. All rights Reserved.                                 */
/*                       岐阜大学装置                                         */
/*============================================================================*/

// ＢＭＰ構造体
typedef struct _IP_BMPIMAGE {
  BITMAPFILEHEADER fhead;
  BITMAPINFOHEADER ihead;
  RGBQUAD *rgb;
  unsigned char *ptr;
} IP_BMPIMAGE;

// 内部ＷＯＲＫ
static IP_BMPIMAGE IP_BMPImg;
static unsigned char *ImgTbl;

HWND hWnd;
HDC hDC;


DLLEXPORT int __stdcall IP_CapMain( HWND hWnd0, HDC hDC0, unsigned char *Buff, int flagDrawOnPC )
{

  long Ret;
  IMGID imageID = 0;

  hWnd = hWnd0;
  hDC = hDC0;

  ImgTbl = (unsigned char *) malloc((IP_INPUT_WIDTH * IP_INPUT_HEIGHT));
  imageID = AllocImg(DeviceID,IMG_FS_512H_512V);
  if( imageID == -1 ){
    imageID = 0;
    Ret = -10;
    goto error;
  }
  if( (Ret = IP_GetCamera(DeviceID, imageID)) != 0 ){
      goto error;
  }

  if( (Ret = IP_ImgGet(imageID, ImgTbl)) != 0 ){
    goto error;
  }

#if 0
  // Window Handle, hDCを用いた描画を行わないため、IP_ImgPrintを行わない(by Hirokazu Yokoyama)
  if( flagDrawOnPC == 1 ){
    IP_ImgPrint( hWnd, hDC, ImgTbl ); // Draw camera image on PC display
  }
#endif

  if( (Ret = DispImg(DeviceID, imageID)) != 0 ){
    goto error;
  }

  memcpy( Buff, (char *) ImgTbl, (size_t)(IP_DRAW_WIDTH * IP_DRAW_HEIGHT) );

 error:
  free( ImgTbl );
  if( imageID ) Ret = FreeImg( DeviceID, imageID );

  return (int) Ret;
}


static long IP_ImgGet( IMGID imageID, unsigned char *ImgTbl )
{
  long Ret;

  Ret = SetAllWindow(DeviceID,0, 0, IP_DRAW_WIDTH - 1, IP_DRAW_HEIGHT - 1);
  if( Ret != 0 ){
    return -2;
  }

  Ret = ReadImg( DeviceID, imageID, (char *)ImgTbl, IP_DRAW_SIZE );
  if( Ret != IP_DRAW_SIZE ){
    return -1;
  }

  return 0;
}


LPBITMAPINFO SetBMHeader()
{
  LPBITMAPINFO bml;

  bml = (LPBITMAPINFO)malloc(sizeof(BITMAPINFOHEADER) + sizeof(RGBQUAD) * 256);
  bml->bmiHeader.biSize          = sizeof(BITMAPINFOHEADER);	//BITMAPINFOHEADERのサイズ
  bml->bmiHeader.biWidth         = IP_INPUT_WIDTH;		//ビットマップの幅
  bml->bmiHeader.biHeight        = IP_INPUT_HEIGHT;		//ビットマップの高さ
  bml->bmiHeader.biPlanes        = 1;				//１固定
  bml->bmiHeader.biBitCount      = 8;				//８ビット（２５６階調）
  bml->bmiHeader.biCompression   = 0;				//ＲＬＥ圧縮なし
  bml->bmiHeader.biSizeImage     = 0;				//
  bml->bmiHeader.biXPelsPerMeter = 9447;			//
  bml->bmiHeader.biYPelsPerMeter = 9447;			//
  bml->bmiHeader.biClrUsed       = 0;				//
  bml->bmiHeader.biClrImportant  = 0;				//

  return bml;
}

LPBITMAPINFO bmMono;
LPBITMAPINFO bmRB;
LPBITMAPINFO bmgRB;
static void InitView()
{
  int i;

  bmMono = SetBMHeader();
  for (i = 0; i < 256; i++) {
    bmMono->bmiColors[i].rgbRed      = (BYTE) i;
    bmMono->bmiColors[i].rgbGreen    = (BYTE) i;
    bmMono->bmiColors[i].rgbBlue     = (BYTE) i;
    bmMono->bmiColors[i].rgbReserved = 0;
  }

  bmRB = SetBMHeader();
  for (i = 0; i < 64; i++) {
    bmRB->bmiColors[i].rgbRed  = 255;
    bmRB->bmiColors[i].rgbBlue = bmRB->bmiColors[i].rgbGreen = 255 - i * 4;

    bmRB->bmiColors[i + 64].rgbRed    = 255;
    bmRB->bmiColors[i + 64].rgbBlue   = 255 - i * 2;
    bmRB->bmiColors[i + 64].rgbGreen  = 255 - i * 4;
    bmRB->bmiColors[i + 128].rgbRed   = 255 - i * 2;
    bmRB->bmiColors[i + 128].rgbBlue  = 255;
    bmRB->bmiColors[i + 128].rgbGreen = 255 - i * 4;
    bmRB->bmiColors[i + 192].rgbRed   = 255 - i * 4;
    bmRB->bmiColors[i + 192].rgbBlue  = 255;
    bmRB->bmiColors[i + 192].rgbGreen = 255 - i * 4;
  }

  bmRB->bmiColors[0].rgbRed = bmRB->bmiColors[0].rgbBlue = bmRB->bmiColors[0].rgbGreen = 255;
  bmRB->bmiColors[1].rgbRed = bmRB->bmiColors[1].rgbBlue = bmRB->bmiColors[1].rgbGreen = 0;
  bmgRB = SetBMHeader();
  for (i = 0; i < 128; i++) {
    bmgRB->bmiColors[i].rgbRed         = i * 2;
    bmgRB->bmiColors[i].rgbBlue        = 0;
    bmgRB->bmiColors[i].rgbGreen       = 0;
    bmgRB->bmiColors[i + 128].rgbRed   = i * 2;
    bmgRB->bmiColors[i + 128].rgbBlue  = i * 2;
    bmgRB->bmiColors[i + 128].rgbGreen = i * 2;
  }
}


// 取込み時のデータ表示
static void IP_ImgPrint(HWND hWnd, HDC hDC, unsigned char *ImgTbl)
{
  HBITMAP		hBmp;
  RECT			rect;
  int			i, j;
  unsigned char *ImgTblw;

  ImgTblw = (unsigned char *) malloc(IP_INPUT_WIDTH * IP_INPUT_HEIGHT);
  j = IP_INPUT_HEIGHT - 1;
  for (i = 0; i < IP_INPUT_HEIGHT; i ++,j --) {
    memcpy(&ImgTblw[j * IP_INPUT_WIDTH], &ImgTbl[i * IP_INPUT_WIDTH],IP_INPUT_WIDTH);
  }

  hBmp = CreateCompatibleBitmap(hDC, IP_DRAW_WIDTH, IP_DRAW_HEIGHT);
  int a = SetDIBits(hDC, hBmp, 0, IP_INPUT_HEIGHT, ImgTblw, (LPBITMAPINFO) bmMono, DIB_RGB_COLORS);
  SelectObject(hDC, hBmp);
  DeleteObject(hBmp);
  rect.left   = 0;
  rect.top    = 0;
  rect.right  = IP_DRAW_WIDTH;
  rect.bottom = IP_DRAW_HEIGHT;
  InvalidateRect(hWnd, &rect, TRUE);

  free(ImgTblw);
}



// 拡大時のデータ表示
void __stdcall IP_DataPrint(HWND hWnd, HDC hDC, short Hi, unsigned char *Buff)
{
  HBITMAP hBmp;
  RECT rect;
  int i, j;
  unsigned char *ImgTblw;

  ImgTblw = (unsigned char *) malloc(IP_INPUT_WIDTH * IP_INPUT_HEIGHT);
  j = IP_INPUT_HEIGHT - 1;
  for (i = 0; i < IP_DRAW_HEIGHT; i++,j--) {
    memcpy(&ImgTblw[j * IP_DRAW_WIDTH], &Buff[i * IP_DRAW_WIDTH],IP_DRAW_WIDTH);
  }
  hBmp = CreateCompatibleBitmap(hDC, IP_DRAW_WIDTH * Hi, IP_DRAW_HEIGHT * Hi);
  SelectObject(hDC, hBmp);
  DeleteObject(hBmp);
  StretchDIBits(hDC, 0, 0, IP_INPUT_WIDTH * Hi, IP_INPUT_HEIGHT * Hi,
		0, 0, IP_INPUT_WIDTH, IP_INPUT_HEIGHT,
		ImgTblw, (LPBITMAPINFO) bmMono, DIB_RGB_COLORS, SRCCOPY);

  rect.left = 0;
  rect.top = 0;
  rect.right = IP_DRAW_WIDTH * Hi;
  rect.bottom = IP_DRAW_HEIGHT * Hi;
  InvalidateRect(hWnd, &rect, TRUE);
  free(ImgTblw);
}

// ２値化時のデータ表示
DLLEXPORT void __stdcall IP_NichiPrint(HWND hWnd, HDC hDC, int Hi,int Level, unsigned char *Src,unsigned char *Bak, int emType )
/* DLLEXPORT void __stdcall IP_NichiPrint(HWND hWnd, HDC hDC, int Hi,int Level, unsigned char *Src,unsigned char *Bak) */
{
  HBITMAP	hBmp;
  RECT		rect;
  int		i, j;
  unsigned char *ImgTblw;

  for (i = 0; i < IP_DRAW_HEIGHT; i++) {
    memcpy(&Bak[i * IP_DRAW_WIDTH], &Src[i * IP_DRAW_WIDTH],IP_DRAW_WIDTH);
  }
#if 0 // 一時コメント by Yokoyama
  ImageAutoRange( Src, Bak, emType );
#endif

  for (i = 0; i < IP_DRAW_HEIGHT * IP_DRAW_WIDTH; i++) {
    if (Src[i] < Level) {
      Src[i] = 0;
    } else {
      Src[i] = 0xFF;
    }
  }
  ImgTblw = (unsigned char *) malloc(IP_INPUT_WIDTH * IP_INPUT_HEIGHT);
  j = IP_INPUT_HEIGHT - 1;
  for (i = 0; i < IP_DRAW_HEIGHT; i++,j--) {
    memcpy(&ImgTblw[j * IP_DRAW_WIDTH], &Src[i * IP_DRAW_WIDTH], IP_DRAW_WIDTH);
  }
  hBmp = CreateCompatibleBitmap(hDC, IP_DRAW_WIDTH * Hi, IP_DRAW_HEIGHT * Hi);
  SelectObject(hDC, hBmp);
  DeleteObject(hBmp);
  StretchDIBits(hDC, 0, 0, IP_INPUT_WIDTH * Hi, IP_INPUT_HEIGHT * Hi,
		0, 0, IP_INPUT_WIDTH, IP_INPUT_HEIGHT,
		ImgTblw, (LPBITMAPINFO) bmMono, DIB_RGB_COLORS, SRCCOPY);
  rect.left = 0;
  rect.top = 0;
  rect.right = IP_DRAW_WIDTH * Hi;
  rect.bottom = IP_DRAW_HEIGHT * Hi;
  InvalidateRect(hWnd, &rect, TRUE);
  free(ImgTblw);
}


// データファイル保存
DLLEXPORT int __stdcall IP_BmpSave(char *FName, unsigned char *Buff)
{
  return BmpSave(FName, NULL, IP_DRAW_WIDTH, IP_DRAW_HEIGHT, Buff);
}


int BmpSave(char *FileName, RGBQUAD * rgb, int Width, int Height, unsigned char *Buff)
{
  FILE *fp;
  int i, j, stat;
  int br;

  stat = IP_ERROR;

  if ((fp = fopen(FileName, "wb")) == (FILE *) NULL) return stat;

  IP_BMPImg.ihead.biSize = 40;
  IP_BMPImg.ihead.biWidth = Width;
  IP_BMPImg.ihead.biHeight = Height;
  IP_BMPImg.ihead.biPlanes = 1;
  IP_BMPImg.ihead.biBitCount = 8;
  IP_BMPImg.ihead.biCompression = 0;
  IP_BMPImg.ihead.biSizeImage = 0;
  IP_BMPImg.ihead.biXPelsPerMeter = 9447;
  IP_BMPImg.ihead.biYPelsPerMeter = 9447;
  IP_BMPImg.ihead.biClrUsed = 0;
  IP_BMPImg.ihead.biClrImportant = 0;
  IP_BMPImg.fhead.bfType = 0x4D42;
  IP_BMPImg.fhead.bfSize = Width * Height;
  IP_BMPImg.fhead.bfReserved1 = 0;
  IP_BMPImg.fhead.bfReserved2 = 0;
  IP_BMPImg.fhead.bfOffBits = 1078;

  if (rgb == NULL) {
    if ((IP_BMPImg.rgb = (RGBQUAD *) malloc(sizeof(RGBQUAD) * 256)) == NULL) {
      fclose(fp);
      return stat;
    }
    for (i = 0; i < 256; i++) {
      IP_BMPImg.rgb[i].rgbRed = i;
      IP_BMPImg.rgb[i].rgbGreen = i;
      IP_BMPImg.rgb[i].rgbBlue = i;
      IP_BMPImg.rgb[i].rgbReserved = 0x00;
    }
  } else {
    IP_BMPImg.rgb = rgb;
  }

  if ((IP_BMPImg.ptr = (unsigned char *) malloc(IP_BMPImg.fhead.bfSize)) == NULL) {
    fclose(fp);
    if (rgb == NULL) {
      free(IP_BMPImg.rgb);
    }
    return stat;
  }
  for (i = 0; i < IP_HALF_HEIGHT; i++) {
    for (j = 0; j < IP_HALF_WIDTH; j++) {
      br = Buff[i + j * IP_HALF_WIDTH];
    }
  }
  j = Height - 1;
  for (i = 0; i < Height; i++,j--) {
    memcpy(&IP_BMPImg.ptr[j * Width], &Buff[i * Width], Width);
  }
  if ((fwrite(&IP_BMPImg.fhead, sizeof(BYTE), sizeof(BITMAPFILEHEADER), fp)) != sizeof(BITMAPFILEHEADER))
    goto funcend;
  if ((fwrite(&IP_BMPImg.ihead, sizeof(BYTE), sizeof(BITMAPINFOHEADER), fp)) != sizeof(BITMAPINFOHEADER))
    goto funcend;
  if ((fwrite(IP_BMPImg.rgb, sizeof(RGBQUAD), 256, fp)) != 256) goto funcend;
  if ((fwrite(IP_BMPImg.ptr, sizeof(unsigned char), IP_BMPImg.fhead.bfSize, fp)) != IP_BMPImg.fhead.bfSize)
    goto funcend;
  stat = IP_NORMAL;

 funcend:
  fclose(fp);
  if (rgb == NULL) free(IP_BMPImg.rgb);
  free(IP_BMPImg.ptr);

  return stat;
}


// データファイルリード
DLLEXPORT int __stdcall IP_BmpLoad(char *FName, char *X, char *Y, char *Z, char *Com, unsigned char *Buff)
{
  char *FileName;
  char wk[200];
  FILE *fp;
  int i, j, k;

  // .Dat保存
  FileName = (char *) malloc(strlen(FName) + 1);
  if (FileName == NULL) return 1;
  memcpy(FileName, FName, strlen(FName));
  memcpy(&FileName[strlen(FName) - 3], "Dat", 3);
  FileName[strlen(FName)] = 0x00;
  if ((fp = fopen(FileName, "rt")) == (FILE *) NULL) {
    free(FileName);
    return 2;
  }
  memset(wk, 0x00, sizeof(wk));
  if ((fread(wk, sizeof(BYTE), 150, fp)) == 0) {
    free(FileName);
    fclose(fp);
    return 3;
  }
  fclose(fp);
  i = j = 0;
  for (k = 0; k < 150; k++) {
    if (wk[k] == 0x0a) {
      wk[k] = 0x00;
      j = k + 1;
      break;
    }
  }
  strcpy(X, &wk[i]);
  for (k = j; k < 150; k++) {
    if (wk[k] == 0x0a) {
      wk[k] = 0x00;
      i = j;
      j = k + 1;
      break;
    }
  }
  strcpy(Y, &wk[i]);
  for (k = j; k < 150; k++) {
    if (wk[k] == 0x0a) {
      wk[k] = 0x00;
      i = j;
      j = k + 1;
      break;
    }
  }
  strcpy(Z, &wk[i]);
  strcpy(Com, &wk[j]);

  // .Bmp保存
  IP_BMPImg.rgb = (RGBQUAD *) malloc(sizeof(RGBQUAD) * 256);
  if (IP_BMPImg.rgb == NULL) {
    free(FileName);
    return 4;
  }
  IP_BMPImg.ptr = (unsigned char *) malloc(IP_DRAW_WIDTH * IP_DRAW_HEIGHT);
  if (IP_BMPImg.ptr == NULL) {
    free(FileName);
    free(IP_BMPImg.rgb);
    return 5;
  }
  memcpy(&FileName[strlen(FName) - 3], "Bmp", 3);
  if ((fp = fopen(FileName, "rb")) == (FILE *) NULL) {
    free(FileName);
    free(IP_BMPImg.rgb);
    free(IP_BMPImg.ptr);
    return 6;
  }
  if ((fread(&IP_BMPImg.fhead, sizeof(BYTE), sizeof(BITMAPFILEHEADER), fp)) != sizeof(BITMAPFILEHEADER)) {
    free(FileName);
    free(IP_BMPImg.rgb);
    free(IP_BMPImg.ptr);
    fclose(fp);
    return 7;
  }
  if ((fread(&IP_BMPImg.ihead, sizeof(BYTE), sizeof(BITMAPINFOHEADER), fp)) != sizeof(BITMAPINFOHEADER)) {
    free(FileName);
    free(IP_BMPImg.rgb);
    free(IP_BMPImg.ptr);
    fclose(fp);
    return 8;
  }
  if ((fread(IP_BMPImg.rgb, sizeof(RGBQUAD), 256, fp)) != 256) {
    free(FileName);
    free(IP_BMPImg.rgb);
    free(IP_BMPImg.ptr);
    fclose(fp);
    return 9;
  }
  if ((fread(IP_BMPImg.ptr, sizeof(unsigned char), IP_BMPImg.fhead.bfSize,fp)) != IP_BMPImg.fhead.bfSize) {
    free(FileName);
    free(IP_BMPImg.rgb);
    free(IP_BMPImg.ptr);
    fclose(fp);
    return 9;
  }
  fclose(fp);
  j = IP_DRAW_HEIGHT - 1;
  for (i = 0; i < IP_DRAW_HEIGHT; i++) {
    memcpy(&Buff[i * IP_DRAW_WIDTH], &IP_BMPImg.ptr[j * IP_DRAW_WIDTH],IP_DRAW_WIDTH);
    j--;
  }
  free(FileName);
  free(IP_BMPImg.rgb);
  free(IP_BMPImg.ptr);

  return 0;
}


void NewImage(HWND hWnd, HDC hDC, int Hi, LPBITMAPINFO bmi, unsigned char *Src)
{
  int i, j;
  unsigned char *ImgTblw;
  HBITMAP hBmp;
  RECT rect;

  if (bmi == NULL) bmi = bmMono;
  ImgTblw = (unsigned char *) malloc(IP_INPUT_SIZE);
  j = IP_INPUT_HEIGHT - 1;

  for (i = 0; i < IP_DRAW_HEIGHT; i++) {
    memcpy(&ImgTblw[j * IP_DRAW_WIDTH], &Src[i * IP_DRAW_WIDTH],IP_DRAW_WIDTH);
    j--;
  }

  hBmp = CreateCompatibleBitmap(hDC, IP_DRAW_WIDTH * Hi, IP_DRAW_HEIGHT * Hi);
  SelectObject(hDC, hBmp);
  DeleteObject(hBmp);
  StretchDIBits(hDC, 0, 0, IP_INPUT_WIDTH * Hi, IP_INPUT_HEIGHT * Hi,
		0, 0, IP_INPUT_WIDTH, IP_INPUT_HEIGHT,
		ImgTblw, (LPBITMAPINFO) bmi, DIB_RGB_COLORS, SRCCOPY);
  rect.left = 0;
  rect.top = 0;
  rect.right = IP_DRAW_WIDTH * Hi;
  rect.bottom = IP_DRAW_HEIGHT * Hi;
  InvalidateRect(hWnd, &rect, TRUE);
  free(ImgTblw);
}


/******************************* 後から追加した関数 2004.03.15 ********************************/

DLLEXPORT int __stdcall IP_GetDeviceID()
{
  return DeviceID;
}

void UserMessage(char *Msg)
{
  FILE	*fp;

  fp = fopen("C:\\UserMessage.txt","a+");
  fputs(Msg,fp);
  fclose(fp);
}

DLLEXPORT int IP_GetCamera(int DeviceID,int ImageID)
{
    return GetCamera( DeviceID, ImageID );    
}

DLLEXPORT void __stdcall IP_SetCameraType(int Type)
{
  CameraType = Type;
}
