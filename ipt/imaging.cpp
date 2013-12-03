/*
 *  $Id: imaging.c,v 1.9 2002/07/04 08:40:57 thitoshi Exp $
 */

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

//********ヘッダファイルの追加sawa**************
#include <iostream>
#include <fstream>
#include <stdio.h>
#include <string.h>
#include <cstring>
//#include <opencv/cv.h>
//#include <opencv/highgui.h>
#include <math.h>
//#include <opencv/cxcore.h>
#include <time.h>
//#pragma comment(lib,"cv.lib")
//#pragma comment(lib,"cxcore.lib")
//#pragma comment(lib,"highgui.lib")
#define I(IMG,X,Y)((uchar*)((IMG)->imageData + (IMG)->widthStep*(Y)))[(X)]
//IplImage *img_in;
//********ヘッダファイルの追加sawa**************

//	VP-910SDK関係の定義ファイル
#include "vpxdef.h"
#include "vpxsys.h"
#include "vpxfnc.h"

#include "ipdefine.h"
#include "dst4.h"
#include "gausDist.h"

#include "ipproto.h"

#include "imaging.h"
#include "Parameter.h"

extern "C"{
extern FILE *emio_log;
}
extern "C"{
extern TrackSearchParameter gTSP;
extern HWND hWnd;
extern HDC hDC;
extern int Hi;
extern LPBITMAPINFO bmRB;
}

static unsigned char TmpDraw[IP_DRAW_HEIGHT][IP_DRAW_WIDTH];
static unsigned char TmpHalf1[IP_HALF_HEIGHT][IP_HALF_WIDTH];
static unsigned char TmpHalf2[IP_HALF_HEIGHT][IP_HALF_WIDTH];

static int image_thresh_br_hit_number =    45;
static int image_thresh_br_dust       =   100;
static int image_thresh_dust_thin     =  6000;
static int image_thresh_dust_thick    = 10000;

static int    bokashi_size = 10;
static double bokashi_sigma = 5.0;

static IMGID image_imgid, image_imgid_inv;
static IMGID image_imgid_laplacian, image_imgid_tmp;
static IMGID *RowImageIDs;


extern LONG DeviceID;


static void ImageAutoRangeThin ( unsigned char *dest, unsigned char *src );
static void ImageAutoRangeThick( unsigned char *dest, unsigned char *src );
int PreImageProcessing( int emType );
static void PreImageProcessingThick( void );

#define NumOfImageDataColl 10
static ImageDataColl imageDataColl[NumOfImageDataColl];

//using namespace std;

void DumpCameraImage( char *filename, IMGID *imgID, int n );

#if 0
//*******************関数の宣言sawa******************************************************
  void myThinningInit(CvMat** kpw, CvMat** kpb)
  {
    //cvFilter2D用のカーネル
    //アルゴリズムでは白、黒のマッチングとなっているのをkpwカーネルと二値画像、
    //kpbカーネルと反転した二値画像の2組に分けて畳み込み、その後でANDをとる
    for (int i=0; i<8; i++){
      *(kpw+i) = cvCreateMat(3, 3, CV_8UC1);
      *(kpb+i) = cvCreateMat(3, 3, CV_8UC1);
      cvSet(*(kpw+i), cvRealScalar(0), NULL);
      cvSet(*(kpb+i), cvRealScalar(0), NULL);
    }
    //cvSet2Dはy,x(row,column)の順となっている点に注意
    //kernel1
    cvSet2D(*(kpb+0), 0, 0, cvRealScalar(1));
    cvSet2D(*(kpb+0), 0, 1, cvRealScalar(1));
    cvSet2D(*(kpb+0), 1, 0, cvRealScalar(1));
    cvSet2D(*(kpw+0), 1, 1, cvRealScalar(1));
    cvSet2D(*(kpw+0), 1, 2, cvRealScalar(1));
    cvSet2D(*(kpw+0), 2, 1, cvRealScalar(1));
    //kernel2
    cvSet2D(*(kpb+1), 0, 0, cvRealScalar(1));
    cvSet2D(*(kpb+1), 0, 1, cvRealScalar(1));
    cvSet2D(*(kpb+1), 0, 2, cvRealScalar(1));
    cvSet2D(*(kpw+1), 1, 1, cvRealScalar(1));
    cvSet2D(*(kpw+1), 2, 0, cvRealScalar(1));
    cvSet2D(*(kpw+1), 2, 1, cvRealScalar(1));
    //kernel3
    cvSet2D(*(kpb+2), 0, 1, cvRealScalar(1));
    cvSet2D(*(kpb+2), 0, 2, cvRealScalar(1));
    cvSet2D(*(kpb+2), 1, 2, cvRealScalar(1));
    cvSet2D(*(kpw+2), 1, 0, cvRealScalar(1));
    cvSet2D(*(kpw+2), 1, 1, cvRealScalar(1));
    cvSet2D(*(kpw+2), 2, 1, cvRealScalar(1));
    //kernel4
    cvSet2D(*(kpb+3), 0, 2, cvRealScalar(1));
    cvSet2D(*(kpb+3), 1, 2, cvRealScalar(1));
    cvSet2D(*(kpb+3), 2, 2, cvRealScalar(1));
    cvSet2D(*(kpw+3), 0, 0, cvRealScalar(1));
    cvSet2D(*(kpw+3), 1, 0, cvRealScalar(1));
    cvSet2D(*(kpw+3), 1, 1, cvRealScalar(1));
    //kernel5
    cvSet2D(*(kpb+4), 1, 2, cvRealScalar(1));
    cvSet2D(*(kpb+4), 2, 2, cvRealScalar(1));
    cvSet2D(*(kpb+4), 2, 1, cvRealScalar(1));
    cvSet2D(*(kpw+4), 0, 1, cvRealScalar(1));
    cvSet2D(*(kpw+4), 1, 1, cvRealScalar(1));
    cvSet2D(*(kpw+4), 1, 0, cvRealScalar(1));
    //kernel6
    cvSet2D(*(kpb+5), 2, 0, cvRealScalar(1));
    cvSet2D(*(kpb+5), 2, 1, cvRealScalar(1));
    cvSet2D(*(kpb+5), 2, 2, cvRealScalar(1));
    cvSet2D(*(kpw+5), 0, 2, cvRealScalar(1));
    cvSet2D(*(kpw+5), 0, 1, cvRealScalar(1));
    cvSet2D(*(kpw+5), 1, 1, cvRealScalar(1));
    //kernel7
    cvSet2D(*(kpb+6), 1, 0, cvRealScalar(1));
    cvSet2D(*(kpb+6), 2, 0, cvRealScalar(1));
    cvSet2D(*(kpb+6), 2, 1, cvRealScalar(1));
    cvSet2D(*(kpw+6), 0, 1, cvRealScalar(1));
    cvSet2D(*(kpw+6), 1, 1, cvRealScalar(1));
    cvSet2D(*(kpw+6), 1, 2, cvRealScalar(1));
    //kernel8
    cvSet2D(*(kpb+7), 0, 0, cvRealScalar(1));
    cvSet2D(*(kpb+7), 1, 0, cvRealScalar(1));
    cvSet2D(*(kpb+7), 2, 0, cvRealScalar(1));
    cvSet2D(*(kpw+7), 1, 1, cvRealScalar(1));
    cvSet2D(*(kpw+7), 1, 2, cvRealScalar(1));
    cvSet2D(*(kpw+7), 2, 2, cvRealScalar(1));
  }




  //***********************
  //画像をずらすプログラム
  //***********************
  void displace(IplImage *Input, int beginend){
      int x,y;
	  int xstep=0.02;
      int d=3; //3pixelずらす
      if (xstep < 0){
	    for (y=0; y<440; y++){
		    for (x=0; x<512; x++){
			    if (d*beginend+x>512){
				    I(Input,x,y)=255;
			    }
			    else{
				    I(Input,x,y)=I(Input,d*beginend+x,y);
		    	}
		  	}
	  	}
	  }

      if (xstep < 0){
	    for (y=440-1; y>=0; y--){
		    for (x=512-1; x>=0; x--){
			    if (-d*beginend+x<0){
				    I(Input,x,y)=255;
		    	}
			    else{
				    I(Input,x,y)=I(Input,-d*beginend+x,y);
			    }
		    }
    	}
	  }
  }





  //***********************
  //画像を重ねるプログラム
  //***********************
  void superimpose(IplImage *img_in, IplImage *Input){
	  for (int y=0; y<440; y++){
   		for (int x=0; x<512; x++){
		 		if (I(img_in,x,y)>I(Input,x,y)){
					I(img_in,x,y)=I(Input,x,y);
				}
			}
	  }

//	  cvReleaseImage(&Input);
  }



  //*********************
  //画像処理のプログラム
  //*********************
  void gazou_process(IplImage *img_in, IplImage *img_out, char fName[100]){
//	  cvSaveImage(fName,img_in);

	  int w, h,iroR,iroG,iroB;
	  int rinkaku,p1,p2,p3,p4,p5,hozon,thre,kaizoudo,saisenka;
//    double time1[5],time2[5],time3[5],time4[5],time5[5],time6[5],time7[5];
//    clock_t t1,t2,t3,t4,t5,t6,t7;
	

	  //*********input********************
	  hozon=0;   //hozon=1の時、画像を保存する
	  kaizoudo=1;    //解像度を変える時は1
	  thre=0;
	  saisenka=1;    //細線化処理をする時は1
	  rinkaku=35;    //輪郭の長さ
	  p1=1;       //pixel
	  p2=1;       //radian
	  p3=20;      //数
	  p4=10;      //線分の最小値
	  p5=5;       //1本にする線の間隔
      iroR=255;
	  iroG=0;
	  iroB=0;
	//**********************************

 //   t1=clock();
	  if (kaizoudo==1){
        //***********************
	    //2x2pixelを1pixelにする
	    //***********************
	    img_out = cvCreateImage(cvSize(img_in->width/2,img_in->height/2),IPL_DEPTH_8U,1);//処理画像

	    int T1,T2,T3,T4,T;
	    w=img_in->width, h=img_in->height;

	    for (int y=0; y<h/2; y++){
		  for (int x=0; x<w/2; x++){
			  T1=I(img_in,2*x,2*y);
			  T2=I(img_in,2*x,2*y+1);
			  T3=I(img_in,2*x+1,2*y);
			  T4=I(img_in,2*x+1,2*y+1);
			  T=(T1+T2+T3+T4)/4;
			  I(img_out,x,y)=T;
		  }
		}

//	if (hozon==1){
//		sprintf(fName,"%s_2x2.bmp",Name);
//	    cvSaveImage(fName,img_out);
//	}


	  }

	  if (kaizoudo!=1){
		img_out = cvCreateImage(cvSize(img_in->width,img_in->height),IPL_DEPTH_8U,1);//処理画像
		cvCopy(img_in,img_out);
	  }

//    t2=clock();
      //***************
	  //filterをかける
	  //***************
	  CvMat kernel;

	  float data[36]={
		1, 1,   1,   1, 1, 1,
		1, 2,   3,   3, 2, 1,
		1, 3, -13, -13, 3, 1,
        1, 3, -13, -13, 3, 1,
		1, 2,   3,   3, 2, 1,
		1, 1,   1,   1, 1, 1,
	  };
	  cvInitMatHeader(&kernel, 6, 6, CV_32FC1,data);
	
	  w=img_out->width, h=img_out->height;

      //画像領域の確保
	  cvFilter2D(img_out,img_out,&kernel);

	  int x,y;
	  for (y=0; y<h; y++){
		for (x=0; x<w; x++){
			I(img_out,x,y)=255-I(img_out,x,y);
		}
	  }


//	if (hozon==1){
//		sprintf(fName,"%s_filter.bmp",Name);
//	    cvSaveImage(fName,img_out);
//	}


//   t3=clock();
	 //*******************
	 //thresholdをかける
	 //*******************
	 cvThreshold(img_out, img_out, thre, 255, CV_THRESH_BINARY);


//	if (hozon==1){
//		sprintf(fName,"%s_threshold.bmp",Name);
//	    cvSaveImage(fName,img_out);
//	}


//   t4=clock();
	 //****************
	 //輪郭追跡を行う
	 //****************
	 int i;//変数
	 int value=0;
	 IplImage *Proc;//処理画像

     //輪郭情報
	 CvMemStorage* Storage;//メモリストレージ
	 CvSeq* Contours=0;//輪郭データ
	 int Count=0;//輪郭の数
	 double Length=0;//輪郭線の長さ
	 double Area=0;//面積
	 CvRect rect;//矩形

	 //白黒反転させる、また外2pixel分をけす

	 w=img_out->width, h=img_out->height;

	 for (y=0; y<h; y++){
		for (int x=0; x<w; x++){
			I(img_out,x,y)=255-I(img_out,x,y);
		}
	 }
	 for (y=0; y<h; y++){
		I(img_out,0,y)=0;
		I(img_out,w-1,y)=0;
		I(img_out,1,y)=0;
		I(img_out,w-2,y)=0;
	 }
	 for (x=0; x<w; x++){
		I(img_out,x,0)=0;
		I(img_out,x,h-1)=0;
		I(img_out,x,1)=0;
		I(img_out,x,h-2)=0;
	 }

	 Proc = cvCreateImage(cvSize(img_out->width,img_out->height),IPL_DEPTH_8U,1);//処理画像
	
	 for (int j=1; j<=4; j++){
		cvCopy(img_out,Proc,NULL);
		Contours=0;//輪郭データ
		Count=0;//輪郭の数
		Length=0;//輪郭線の長さ
		Area=0;//面積
		Storage=cvCreateMemStorage(0);//メモリストレージを確保
		//輪郭を取得
    	Count = cvFindContours(Proc,Storage,&Contours,sizeof(CvContour),CV_RETR_EXTERNAL,CV_CHAIN_APPROX_SIMPLE,cvPoint(0,0));
	
		//輪郭情報の取得	
		for(i=1;i<=Count;i++){
			Length = cvArcLength(Contours, CV_WHOLE_SEQ, 1);//輪郭長
			if (Length<=rinkaku){         //パラメータ
				cvDrawContours(img_out,Contours,CV_RGB(0,0,0),CV_RGB(0,0,0),0,1,CV_AA,cvPoint(0,0));
			}

			Contours=Contours->h_next;//次の輪郭へ
		}
     cvReleaseMemStorage(&Storage);
	 }

	 for (y=0; y<h; y++){
		for (int x=0; x<w; x++){
			I(img_out,x,y)=255-I(img_out,x,y);
		}
	 }

//	if (hozon==1){
//		sprintf(fName,"%s_rinkaku.bmp",Name);
//	    cvSaveImage(fName,img_out);
//	}

	 cvReleaseImage(&Proc);

 //  t5=clock();
	 IplImage *dst;
	 if (saisenka==1){
	   //***************
	   //細線化を行う
	   //***************
       //白黒それぞれ8個のカーネルの入れ物
       CvMat** kpb = new CvMat *[8];
       CvMat** kpw = new CvMat *[8];
       myThinningInit(kpw, kpb);
//     IplImage* src = cvLoadImage("D:\\Full_Auto\\sample\\Mikage×50\\Mikage50-24\\Mikage_rinkaku_test2.bmp",0);

  	   w=img_out->width, h=img_out->height;

	   for (int y=0; y<h; y++){
		  for (int x=0; x<w; x++){
		  	 I(img_out,x,y)=255-I(img_out,x,y);
		  }
	   }

       dst= cvCloneImage(img_out);
       //32Fの方が都合が良い
       IplImage* src_w = cvCreateImage(cvGetSize(img_out), IPL_DEPTH_32F, 1);
       IplImage* src_b = cvCreateImage(cvGetSize(img_out), IPL_DEPTH_32F, 1);
       IplImage* src_f = cvCreateImage(cvGetSize(img_out), IPL_DEPTH_32F, 1);
       cvScale(img_out, src_f, 1/255.0, 0);
       //原画像を2値化(しきい値は用途に合わせて考える)
       //src_f:2値化した画像(32F)
       //src_w:作業バッファ
       //src_b:作業バッファ(反転)
       cvThreshold(src_f,src_f,0.5,1.0,CV_THRESH_BINARY);
       cvThreshold(src_f,src_w,0.5,1.0,CV_THRESH_BINARY);
       cvThreshold(src_f,src_b,0.5,1.0,CV_THRESH_BINARY_INV);
       //デバッグ用
       //cvNamedWindow("src",1);
       //cvShowImage("src",src);
       //1ターンでマッチしてなければ終了
       double sum=1;
       while(sum>0){
         sum=0;
         for (int i=0; i<8; i++){
           cvFilter2D(src_w, src_w, *(kpw+i));
           cvFilter2D(src_b, src_b, *(kpb+i));
           //各カーネルで注目するのは3画素ずつなので、マッチした注目画素の濃度は3となる
           //カーネルの値を1/9にしておけば、しきい値は0.99で良い
           cvThreshold(src_w,src_w,2.99,1,CV_THRESH_BINARY); //2.5->2.99に修正
           cvThreshold(src_b,src_b,2.99,1,CV_THRESH_BINARY); //2.5->2.99
           cvAnd(src_w, src_b, src_w);
           //この時点でのsrc_wが消去候補点となり、全カーネルで候補点が0となった時に処理が終わる
           sum += cvSum(src_w).val[0];
           //原画像から候補点を消去(二値画像なのでXor)
           cvXor(src_f, src_w, src_f);
           //作業バッファを更新
           cvCopyImage(src_f, src_w);
           cvThreshold(src_f,src_b,0.5,1,CV_THRESH_BINARY_INV);
		 }
	   }

	   w=dst->width, h=dst->height;


       //8Uの画像に戻して表示
       cvConvertScaleAbs(src_f, dst, 255, 0);

	   for (y=0; y<h; y++){
		 for (int x=0; x<w; x++){
			I(dst,x,y)=255-I(dst,x,y);
		 }
	   }

//	if (hozon==1){
//		sprintf(fName,"%s_saisen.bmp",Name);
//	    cvSaveImage(fName,dst);
//	}

	   cvReleaseImage(&src_w);
	   cvReleaseImage(&src_b);
	   cvReleaseImage(&src_f);
	   cvReleaseImage(&dst);
	}


	if (saisenka!=1){
		dst = cvCreateImage(cvSize(img_out->width,img_out->height),IPL_DEPTH_8U,1);//処理画像
		cvCopy(img_out, dst);
	}


 // t6=clock();
	//***************
	//Hough変換する
	//***************

	CvMemStorage *storage;
	CvSeq *lines=0;
	CvPoint *point;  //, pt1, pt2;

	IplImage *src_img_std, *img2;
	
	src_img_std = cvCreateImage(cvSize(dst->width,dst->height),IPL_DEPTH_8U,3);
	cvCvtColor(dst, src_img_std, CV_GRAY2BGR);




	//ハフ変換のための前処理
	//白黒反転
//	src_img_gray=cvCreateImage(cvSize(dst->width,dst->height),IPL_DEPTH_8U,1);
	img2 = cvCreateImage(cvSize(dst->width,dst->height),IPL_DEPTH_8U,1);

	w=dst->width, h=dst->height;

	for (y=0; y<h; y++){
		for (int x=0; x<w; x++){
			I(img2,x,y)=255;
			I(dst,x,y)=255-I(dst,x,y);
		}
	}

    storage=cvCreateMemStorage(0);
	//確率的ハフ変換
	lines=0;
	lines=cvHoughLines2(
		dst,
		storage,
		CV_HOUGH_PROBABILISTIC,
		p1,    //線を表すのに必要な分解能(pixel)
		CV_PI/180*p2,    //線を表すのに必要な分解能(radian)
		p3,  //「pixel」x「radian」のセルに入る数
		p4,     //線分の最小値
		p5      //2本の線を1本の線とするための線分間の距離
		);

//	FILE *fp;
//	if (hozon==1){
//		sprintf(fName,"%s_Hough.txt",Name);
//	    fp=fopen(fName,"w");
//		fprintf(fp,"%3s%5s%5s%5s\n\n","x1","y1","x2","y2");
//		fprintf(fp,"%20s\n","--------------------");
//	}

	for (i=0; i<lines->total; i++){
		point=(CvPoint*)cvGetSeqElem (lines,i);
		cvLine(img2,point[0],point[1],CV_RGB(255,0,0),1,8,0);
		cvLine(src_img_std,point[0],point[1],CV_RGB(iroR,iroG,iroB),1,8,0);

		cvCircle(
			img2,
			point[0],
			1,
			CV_RGB(255,0,0),
			2,
			8
			);


		cvCircle(
			img2,
			point[1],
			1,
			CV_RGB(255,0,0),
			2,
			8
			);

//	    if (hozon==1){
//			FILE *fp;
//			fprintf(fp,"%3d%5d%5d%5d\n",point[0].x, point[0].y, point[1].x, point[1].y);
//		}

	}

	cvReleaseImage(&src_img_std);
	cvReleaseImage(&img2);
//	cvReleaseImage(&img_in);
	cvReleaseImage(&img_out);
 }

//*******************関数の宣言sawa******************************************************
#endif


int AllocateImages()
{
  if( (image_imgid =           AllocImg(DeviceID,IMG_FS_512H_512V)) == -1 )
    return IP_ERROR;
  if( (image_imgid_inv =       AllocImg(DeviceID,IMG_FS_512H_512V)) == -1 )
    return IP_ERROR;
  if( (image_imgid_laplacian = AllocImg(DeviceID,IMG_FS_512H_512V)) == -1 )
    return IP_ERROR;
  if( (image_imgid_tmp =       AllocImg(DeviceID,IMG_FS_512H_512V)) == -1 )
    return IP_ERROR;

  return IP_NORMAL;
}

int AllocateImages4Track( int step )
{
  int i;

  if( (RowImageIDs = (IMGID *)malloc(sizeof(IMGID) * step)) == NULL )
	return IP_ERROR;

  for( i = 0; i < step; i++ )
    if( (RowImageIDs[i] = AllocImg(DeviceID,IMG_FS_512H_512V)) == -1 )
      return IP_ERROR;

  return IP_NORMAL;
}

DLLEXPORT void __stdcall IP_SetThreshold( int thr_hit_num, int thr_br_dust,
					  int thr_dust1, int thr_dust2 )
{
  image_thresh_br_hit_number = thr_hit_num;
  image_thresh_br_dust       = thr_br_dust;
  image_thresh_dust_thin     = thr_dust1;
  image_thresh_dust_thick    = thr_dust2;
}
#if 0
static void Distinctive( int throffset, unsigned char *Half,
			 unsigned char *Image, unsigned char *Base )
{
  int thr;
  int i, ix, iy;
  int iIm1, iIm2;
  int val;

  for( i=0; i<IP_HALF_SIZE; i++, Base++, Half++ ){
    ix = i%IP_HALF_WIDTH;
    iy = (i/IP_HALF_WIDTH);
    iIm1 = ix*2 + iy*2*IP_DRAW_WIDTH;
    iIm2 = iIm1 + 1;

    thr = *Base + throffset;

    val = Image[iIm1] - thr;
    if( val < 0 ) Image[iIm1] = 0;
    else          Image[iIm1] = val;

    val = Image[iIm2] - thr;
    if( val < 0 ) Image[iIm2] = 0;
    else          Image[iIm2] = val;

    val = *Half - thr;
    if( val<0 ) *Half = 0;
    else        *Half = val;
  }
}

static void LineBrSum( unsigned short *lsum, unsigned char *lnum,
		       unsigned char *src )
{
  // src: input
  // lsum[i]: output, sum of src[i]...src[i+j]
  // lnum[i]: output, number of summed elements ( equal to j )
  int iyp;
  unsigned char *p;
  int M22, M13;

  M22 = IP_HALF_WIDTH - 22;
  M13 = IP_HALF_WIDTH - 13;
  for( iyp = 0; iyp < IP_HALF_HEIGHT; iyp++ ){
    *lsum = 0;
    for( p = src; p <= src + 10; p++ ){
      *lsum += *p;
    }
    *lnum = 11;
    lsum++;
    lnum++;
    for( p = src + 11; p <= src + 20; p++ ){
      *lsum = *(lsum - 1) + *p;
      *lnum = *(lnum - 1) + 1;
      lsum ++;
      lnum ++;
    }
    for( p = src + 21; p < src + IP_HALF_WIDTH; p++ ){
      *lsum = *(lsum - 1) + *p - *(p - 21);
      *lnum = *(lnum - 1);
      lsum ++;
      lnum ++;
    }
    for( p = src + M22; p <= src + M13; p++ ){
      *lsum = *(lsum - 1) - *p;
      *lnum = *(lnum - 1) - 1;
      lsum ++;
      lnum ++;
    }
    src += IP_HALF_WIDTH;
  }
}

static int ConstFrac( int size, int low, unsigned char *Img, double thfrac )
{
  unsigned char *p;
  int i, BrHist[256];
  int total = 0, frac = 0;
  double thr;

  memset( BrHist, 0x00, 256 * sizeof(int) );
  for( p = Img; p < Img + size; p++ )
    if( *p > 0 )
      BrHist[*p]++;

  for( i = low; i < 256; i++ )
    total += BrHist[i];
  thr = thfrac * total;
  for( i = 255; i >= low; i-- ){
    frac += BrHist[i];
    if (frac > thr) break;
  }

  return i;
}

#define FRAC4THR 0.5
static int LocalThresh( unsigned char *Dist, unsigned char *Src )
{
  static unsigned short LineSum[IP_HALF_HEIGHT][IP_HALF_WIDTH];
  static unsigned char  LineNum[IP_HALF_HEIGHT][IP_HALF_WIDTH];
  static unsigned short Sum[IP_HALF_HEIGHT][IP_HALF_WIDTH];
  static unsigned short Num[IP_HALF_HEIGHT][IP_HALF_WIDTH];
  unsigned short *sum, *lsum0, *lsum, *num;
  unsigned char *lnum0, *lnum, *p;
  int ix, iy, before21, T10;

  LineBrSum( (unsigned short *) LineSum, (unsigned char *) LineNum, Src );

  T10 = IP_HALF_WIDTH * 10;
  sum = (unsigned short *) Sum;
  num = (unsigned short *) Num;
  lsum0 = (unsigned short *) LineSum;
  lnum0 = (unsigned char *) LineNum;
  for( ix = 0; ix < IP_HALF_WIDTH; ix++ ){
    lnum = lnum0;
    *sum = *num = 0;
    for( lsum = lsum0; lsum <= lsum0 + T10; lsum += IP_HALF_WIDTH ){
      *sum += *lsum;
      *num += *lnum;
      lnum += IP_HALF_WIDTH;
    }
    sum ++;
    num ++;
    lsum0 ++;
    lnum0 ++;
  }
  lsum = (unsigned short *) LineSum + 11 * IP_HALF_WIDTH;
  lnum = (unsigned char *) LineNum + 11 * IP_HALF_WIDTH;
  for( iy = 1; iy <= 10; iy++ ){
    for( ix = 0; ix < IP_HALF_WIDTH; ix++ ){
      *sum = *(sum - IP_HALF_WIDTH) + *lsum;
      *num = *(num - IP_HALF_WIDTH) + *lnum;
      sum ++;
      num ++;
      lsum ++;
      lnum ++;
    }
  }
  before21 = 21 * IP_HALF_WIDTH;
  for( iy = 11; iy < IP_HALF_HEIGHT - 10; iy++ ){
    for( ix = 0; ix < IP_HALF_WIDTH; ix++ ){
      *sum = *(sum - IP_HALF_WIDTH) + *lsum - *(lsum - before21);
      *num = *(num - IP_HALF_WIDTH) + *lnum - *(lnum - before21);
      sum ++;
      num ++;
      lsum ++;
      lnum ++;
    }
  }
  lsum -= before21;
  lnum -= before21;
  for( iy = IP_HALF_HEIGHT - 10; iy < IP_HALF_HEIGHT; iy++ ){
    for( ix = 0; ix < IP_HALF_WIDTH; ix++ ){
      *sum = *(sum - IP_HALF_WIDTH) - *lsum;
      *num = *(num - IP_HALF_WIDTH) - *lnum;
      sum ++;
      num ++;
      lsum ++;
      lnum ++;
    }
  }

  sum = (unsigned short *) Sum;
  num = (unsigned short *) Num;
  for( p = Dist; p < Dist + IP_HALF_SIZE; p++ ){
    *p = *sum / (*num);
    sum ++;
    num ++;
  }

  return ConstFrac( IP_HALF_SIZE, 5, Dist, FRAC4THR );
}


#endif
static int GetInvRow( int thr, unsigned char *Image )
{ 
  // 領域設定

  if( SetAllWindow( DeviceID,0, 0, IP_DRAW_WIDTH-1, IP_DRAW_HEIGHT-1) != -0)
    return IP_ERROR;
  // カメラ入力
  if( IP_GetCamera( DeviceID, image_imgid ) != 0 )
    return IP_ERROR;

  // 画像を反転する。IP_Invert()でも良い。
  IP_SubConstAbs( DeviceID,image_imgid, image_imgid_tmp, 255 );
  IP_SubConst( DeviceID,image_imgid_tmp, image_imgid_inv, thr );
  ReadImg( DeviceID,image_imgid_inv, (char *)Image, IP_DRAW_SIZE );

  return IP_NORMAL;
	
}
#if 0

void DrawSegImage( int (*segimage)[SEGROW] )
{
  int ix, iy;
  int ibr;

  for( iy = 0; iy < IP_DRAW_HEIGHT; iy++ ){
    for( ix = 0; ix < IP_DRAW_WIDTH; ix++ ){
      ibr = segimage[iy / YSEGPIX][ix / XSEGPIX];
      TmpDraw[iy][ix] = (ibr > 255) ? 255 : ibr;
    }
  }

  NewImage( hWnd, hDC, Hi, NULL, (unsigned char *) TmpDraw );
}

static void staticShrinkBy2( unsigned char *Dist, unsigned char *Src )
{
  unsigned char *row0, *row1, *row10, *row11, *half;
  int ix, iy, br1, br2;

  row0 = Src;
  row1 = Src + 1;
  row10 = Src + IP_DRAW_WIDTH;
  row11 = row10 + 1;
  half = Dist;
  for( iy = 0; iy < IP_HALF_HEIGHT; iy++ ){
    for( ix = 0; ix < IP_HALF_WIDTH; ix++ ){
      br1 = (*row0 > *row1) ? *row0 : *row1;
      br2 = (*row10 > *row11) ? *row10 : *row11;
      *half = (br1 > br2) ? br1 : br2;
      half ++;
      row0  += 2;
      row1  += 2;
      row10 += 2;
      row11 += 2;
    }
    row0 = row10;
    row1 = row11;
    row10 += IP_DRAW_WIDTH;
    row11 += IP_DRAW_WIDTH;
  }
}
#endif
DLLEXPORT int __stdcall IP_MarkCenter( double *sx, double *sy, short size )
{
  int ix, iy, xcent, ycent, br, brsum;
  unsigned char *p, *p0;
  int ix1, ix2, iy1, iy2;

  GetInvRow(255 - 70, (unsigned char *) TmpDraw);
  p = (unsigned char *) TmpDraw;
  xcent = ycent = brsum = 0;
  for( iy = 0; iy < IP_DRAW_HEIGHT; iy++ ){
    for( ix = 0; ix < IP_DRAW_WIDTH; ix++ ){
      if( *p ){
		br = *p;
		xcent += ix * br;
		ycent += iy * br;
		brsum += br;
      }
      p++;
    }
  }

  if( brsum == 0 ){
    *sx = *sy = -1;
    return IP_ERROR;
  }
  xcent /= brsum;
  ycent /= brsum;

  ix1 = xcent - size;
  ix1 = ix1 > 0 ? ix1 : 0;
  ix2 = xcent + size;
  ix2 = ix2 < IP_DRAW_WIDTH ? ix2 : IP_DRAW_WIDTH;
  iy1 = ycent - size;
  iy1 = iy1 > 0 ? iy1 : 0;
  iy2 = ycent + size;
  iy2 = iy2 < IP_DRAW_HEIGHT ? iy2 : IP_DRAW_HEIGHT;
  xcent = ycent = brsum = 0;
  p0 = (unsigned char *) TmpDraw + IP_DRAW_WIDTH * iy1;
  for( iy = iy1; iy < iy2; iy++ ){
    p = p0 + ix1;
    for( ix = ix1; ix < ix2; ix++ ){
      if( *p ){
		br = *p;
		xcent += ix * br;
		ycent += iy * br;
		brsum += br;
      }
      p++;
    }
    p0 += IP_DRAW_WIDTH;
  }
  if( brsum == 0 ){
    *sx = *sy = -1;
    return IP_ERROR;
  }
  *sx = (float) ((double) xcent / brsum);
  *sy = (float) ((double) ycent / brsum);

  return IP_NORMAL;
}
#if 0
#define CONST4HIT1 45
#define CONST4HIT2 55

static int numseqp = -1;
static unsigned char *Seqp;
static unsigned char Seqphoto[IP_DRAW_SIZE * 3];
static unsigned char monoSeqphoto[IP_DRAW_SIZE];

DLLEXPORT void __stdcall IP_SeqPhotoLast( char *filename )
{
  memset( Seqphoto + IP_DRAW_SIZE   - IP_DRAW_WIDTH, 0x01, IP_DRAW_WIDTH2 );
  memset( Seqphoto + IP_DRAW_SIZE*2 - IP_DRAW_WIDTH, 0x01, IP_DRAW_WIDTH2 );	//draw line
  BmpSave( filename, bmRB->bmiColors, IP_DRAW_WIDTH, IP_DRAW_HEIGHT * 3,
	   Seqphoto );
  numseqp = -1;
  return;
}

DLLEXPORT void __stdcall IP_SeqPhoto()
{
  static unsigned char *Row, *Half, ThreshImage[IP_HALF_SIZE];
  unsigned char *seq, *mono, *row0;
  int br, brofs, thr;

  Row = (unsigned char *) TmpDraw;
  Half = (unsigned char *) TmpHalf1;
  GetInvRow(CONST4HIT2, Row);
  staticShrinkBy2(Half, Row);
  thr = LocalThresh(ThreshImage, Half) - 10;

  if( numseqp == -1 ){
    Seqp = Seqphoto;
    numseqp = 0;
  }
  if( numseqp == 0 ){
    memset(Seqp, 0x00, IP_DRAW_SIZE);
    memset(monoSeqphoto, 0x00, IP_DRAW_SIZE);
  }
  brofs = (numseqp % 4) * 64;
  seq = Seqp;
  mono = monoSeqphoto;
  Distinctive(thr, Half, Row, ThreshImage);
  Distinctive(thr, Half, Row + IP_DRAW_WIDTH, ThreshImage);
  for( row0 = Row; row0 < Row + IP_DRAW_SIZE; row0++ ){
    if( *row0 ){
      if( *row0 > *mono ){
	*mono = *row0;
	br = *row0 < 16 ? *row0 * 16 : 255;
	*seq = br / 4 + brofs;
      }
    }
    mono ++;
    seq ++;
  }
  numseqp++;
  if( numseqp % 4 == 0 ){
    numseqp = 0;
    Seqp += IP_DRAW_SIZE;
  }
}

DLLEXPORT void __stdcall IP_GetPhoto( int beginend, char *filename, int fimage, int going)
{
	static unsigned char  *Row ,**kido; //*Image;
    static FILE *fp20;
	char Name[100];

	int size_x=512, size_y=420, set=200, tset=1;  //set=200 kanda
	int i, x, y, Max, Min, up, down, meanbright, T=0, P, P2, x1, y1;
	unsigned char *p, *t;
	double sig=6.939, b1=208.366, b2=143.103;
	int	threshold1, threshold2;

	int nz = 1;
//kanda
	FILE  *fp12, *fp1, *fp2, *fp3, *fp4, *fp5, *fp6, *fp7, *fp10, *fp11, *fp13, *fp14;
//end
	threshold1 = 130;    //b1-3*sig;
	threshold2 = 90;   //b2-3*sig;
	IplImage *img, *img2;

	//fp1:hyoumen.txtに0or1を書き込み
	//fp2:kyoukai.txtに0or1を書き込み
	//fp3:P値の書き込み
	//fp4:P2値の書き込み
	//fp5:upの書き込み
	//fp6:downの書き込み
	
	//fp10:Max-Minの書き込み
	//fp11:枚数の書き出し
	//fp12:画像の保存時間の計測
	//fp13:画像読み込みからメモリに保存するまでの時間計測

 //ファイルポインタ
	if(beginend == 4){
		sprintf(Name,"%s.dat",filename);
		fp20 = fopen(Name,"wb");//kanda
		//fp20=fopen("M:\\sample.dat","wb");
		return ;
	}

	if(beginend == 5){
		fclose(fp20);//kanda
		return ;
	}


  //メモリの確保
	if (beginend == 0){
		kido = (unsigned char **) malloc (sizeof(unsigned char*) * set);
		Row =(unsigned char *) TmpDraw;

//kanda///

	  //"P","P2"の初期値が0となるテキストファイルを作る
		fp3=fopen("C:\\Documents and Settings\\stage3-user\\デスクトップ\\output\\P.txt","w");
		fprintf(fp3,"%d\n",0);
		fflush(fp3);//kanda
		fclose(fp3);
		fp4=fopen("C:\\Documents and Settings\\stage3-user\\デスクトップ\\output\\P2.txt","w");
		fprintf(fp4,"%d\n",0);
		fflush(fp4);//kanda
		fclose(fp4);

//	  //"hyoumen","kyoukai"の初期値が0となるテキストファイルを作る
		fp1=fopen("C:\\Documents and Settings\\stage3-user\\デスクトップ\\output\\hyoumen.txt","w");
		fprintf(fp1,"%d\n",0);
		fflush(fp1);//kanda
		fclose(fp1);
		fp2=fopen("C:\\Documents and Settings\\stage3-user\\デスクトップ\\output\\kyoukai.txt","w");
		fprintf(fp2,"%d\n",0);
		fflush(fp2);//kanda
		fclose(fp2);

		for (i=0; i<set; i++){
			kido[i] = (unsigned char *) malloc ((size_t )IP_DRAW_SIZE);
		}
	}
	
	else{
		Row = kido[fimage];
	}



  // 領域設定
	if( SetAllWindow( DeviceID,0, 0, IP_DRAW_WIDTH-1, IP_DRAW_HEIGHT-1) != -0 )
		return ;
  // カメラ入力
	if( IP_GetCamera(DeviceID,image_imgid) != 0 )
		return ;


	if ( beginend ==1){    //表面認識、画像をメモリに保存
		
//		t3=clock();
		
		ReadImg( DeviceID,image_imgid, (char*)Row, IP_DRAW_SIZE ); 

		t = (unsigned char *) TmpDraw;
		for( p = kido[fimage]; p < kido[fimage] + IP_DRAW_SIZE; p++ ){
			*t = *p;
			t++;
		}
		if ( going ==0){
		img = cvCreateImage(cvSize(512,440),IPL_DEPTH_8U,1);
		img2 = cvCreateImage(cvSize(512,440),IPL_DEPTH_8U,1);

		for (int y1=0; y1<IP_DRAW_HEIGHT; y1++){  
			for (int x1=0; x1<IP_DRAW_WIDTH; x1++){
				I(img,x1,y1)=TmpDraw[y1][x1];
			}
		}

		cvSmooth(img, img2, CV_GAUSSIAN, 35, 0, 0);  //画像の平滑化 kanda

		Min=I(img,0,0);
		Max=I(img,0,0);
		
		for (int y=0; y<IP_DRAW_HEIGHT; y++){ 
			for (int x=0; x<IP_DRAW_WIDTH; x++){
				
				I(img,x,y)=I(img,x,y)-I(img2,x,y) * 0.5 + 90;
				
				if (I(img,x,y)>=255){
					I(img,x,y)=255;	
				}
				
				if (I(img,x,y)<=0){
					I(img,x,y)=0;	
				}
						
				if (I(img,x,y)<Min){	
					Min=I(img,x,y);
				}
				
				if (I(img,x,y)>Max){
					Max=I(img,x,y);
				}
			}
		}

		cvReleaseImage(&img);
		cvReleaseImage(&img2);

		//"P","P2"の読み込み
		fp3=fopen("C:\\Documents and Settings\\stage3-user\\デスクトップ\\output\\P.txt","r");
		fscanf(fp3,"%d",&P);
		fclose(fp3);
		fp4=fopen("C:\\Documents and Settings\\stage3-user\\デスクトップ\\output\\P2.txt","r");
		fscanf(fp4,"%d",&P2);
		fclose(fp4);


		if (Max-Min>threshold1 & P==0 & P2==0){
			up=fimage;
		  
			//"up"の書き込み
			fp5=fopen("C:\\Documents and Settings\\stage3-user\\デスクトップ\\output\\up.txt","w");
			fprintf(fp5,"%d\n",up);
			fflush(fp5);//kanda
			fclose(fp5);

			//"P"を1にする
			fp3=fopen("C:\\Documents and Settings\\stage3-user\\デスクトップ\\output\\P.txt","w");
			fprintf(fp3,"%d\n",1);
			fflush(fp3);//kanda
			fclose(fp3);
		  
			//"hyoumen"を1にする
			fp1=fopen("C:\\Documents and Settings\\stage3-user\\デスクトップ\\output\\hyoumen.txt","w");
			fprintf(fp1,"%d\n",1);
			fflush(fp1);//kanda
			fclose(fp1);

				//"kyoukai"を1にする
			fp2=fopen("C:\\Documents and Settings\\stage3-user\\デスクトップ\\output\\kyoukai.txt","w");
			fprintf(fp2,"%d\n",1);
			fflush(fp2);//kanda
			fclose(fp2);

				//"P2"を1にする
			fp4=fopen("C:\\Documents and Settings\\stage3-user\\デスクトップ\\output\\P2.txt","w");
			fprintf(fp4,"%d\n",1);
			fflush(fp4);//kanda
			fclose(fp4);

		}

		if(Max-Min>=threshold2 & P==1 & P2==0){
			fp14=fopen("C:\\Documents and Settings\\stage3-user\\デスクトップ\\data\\gap2.txt","a"); //kanda
			fprintf(fp14,"%2d%5d%5d%5d\n",fimage,Max-Min,Max,Min);
			fflush(fp14);//kanda
			fclose(fp14);
		
		}


		if(Max-Min<=threshold2 & P==1 & P2==0){
			//down=fimage-1;

			//if(down>67){

				//"down"の書き込み
				//fp6=fopen("C:\\Documents and Settings\\Administrator\\デスクトップ\\output\\down.txt","w");
				//fprintf(fp6,"%d\n",down);
				//fclose(fp6);

				//"kyoukai"を1にする
				//fp2=fopen("C:\\Documents and Settings\\Administrator\\デスクトップ\\output\\kyoukai.txt","w");
				//fprintf(fp2,"%d\n",1);
				//fclose(fp2);

				//"P2"を1にする
				//fp4=fopen("C:\\Documents and Settings\\Administrator\\デスクトップ\\output\\P2.txt","w");
				//fprintf(fp4,"%d\n",1);
				//fclose(fp4);
			//}
		}

		}

	}
  
	if (beginend == 2){   //1セット目以降の画像を保存
		int a=0;
		int nz=-1;
		int maisuu;
		unsigned char apd;

		//"P","P2"を0にする	  
		fp3=fopen("C:\\Documents and Settings\\stage3-user\\デスクトップ\\output\\P.txt","w");	  
		fprintf(fp3,"%d\n",0);
		fflush(fp3);//kanda
		fclose(fp3);
		fp4=fopen("C:\\Documents and Settings\\stage3-user\\デスクトップ\\output\\P2.txt","w");
		fprintf(fp4,"%d\n",0);
		fflush(fp4);//kanda
		fclose(fp4);

		//"up","down"の読み込み
		fp5=fopen("C:\\Documents and Settings\\stage3-user\\デスクトップ\\output\\up.txt","r");
		fscanf(fp5,"%d",&up);
		fclose(fp5);
		fp6=fopen("C:\\Documents and Settings\\stage3-user\\デスクトップ\\output\\down.txt","r");
		fscanf(fp6,"%d",&down);
		fclose(fp6);

		maisuu = up;

		fp7=fopen("C:\\Documents and Settings\\stage3-user\\デスクトップ\\data\\maisuu.txt","a");
		fprintf(fp7,"%d\n",maisuu);
		fflush(fp7);//kanda
		fclose(fp7);

		for (i=up; i<=up+84; i++){
			t = (unsigned char *) TmpDraw;
			
			for( p = kido[i]; p < kido[i] + IP_DRAW_SIZE; p++ ){
				*t = *p;
				t++;
			}

           fwrite(kido[i],1,IP_DRAW_SIZE,fp20);
		   fflush(fp20);
		}

		return ;
	}

	if(beginend == 3){   //メモリ開放
		for (i=0; i<set; i++){
			free(kido[i]);
		}
		free(kido);
		
		return ;
	}

	return ;
}

extern "C"{
extern ImageData *imageData;

}

DLLEXPORT void __stdcall IP_ReadDumpImage()
{
  FILE	*fp;
  int	iid;

  fp = fopen( "image.dat", "rb" );
  for( iid = 0; iid < NumOfStep; iid++ )
    fread( imageData[iid].raw, 1, IP_DRAW_SIZE, fp );
  fclose(fp);
}

static int ShrinkImageBy2_CtoI( int *dest, unsigned char *src,
				int srcWidth, int srcHeight )
{
  int ix, iy;
  int i11, i12, i21, i22;
  int br;

  for( ix=0; ix<srcWidth/2; ix++ ){
    for( iy=0; iy<srcHeight/2; iy++ ){
      i11 = 2*ix + srcWidth*(2*iy);
      i12 = i11 + 1;
      i21 = i11 + srcWidth;
      i22 = i21 + 1;

      br  = src[i11];
      br += src[i12];
      br += src[i21];
      br += src[i22];

      dest[ ix + iy*(srcWidth/2) ] = br;
    }
  }

  return IP_NORMAL;
}
static int ShrinkImageBy2_ItoI( int *dest, int *src,
				int srcWidth, int srcHeight )
{
  int ix, iy;
  int i11, i12, i21, i22;
  int br;

  for( ix=0; ix<srcWidth/2; ix++ ){
    for( iy=0; iy<srcHeight/2; iy++ ){
      i11 = 2*ix + srcWidth*(2*iy);
      i12 = i11 + 1;
      i21 = i11 + srcWidth;
      i22 = i21 + 1;

      br  = src[i11];
      br += src[i12];
      br += src[i21];
      br += src[i22];

      dest[ ix + iy*(srcWidth/2) ] = br;
    }
  }

  return IP_NORMAL;
}
int MakeShrinkData( void )
{
  int i;

  for( i=0; i<NumOfStep; i++ ){
    ImageData *img = &(imageData[i]);
    ShrinkImageBy2_CtoI( img->half,    img->raw,  IP_DRAW_WIDTH, IP_DRAW_HEIGHT );
    ShrinkImageBy2_ItoI( img->quarter, img->half, IP_HALF_WIDTH, IP_HALF_HEIGHT );
  }
  return IP_NORMAL;
}
#endif
static int BrMedian( long Tbl[] )
{
  int sum0 = 0, sum = 0, sumhalf;
  int i, median;
  int Threshold;

  Threshold = 500;
  for( i = 1; i < 256; i++ )
    if( Tbl[i] > Threshold )
      sum0 += Tbl[i];

  sumhalf = sum0 / 2;
  for( median = 1; median < 256; median ++ ){
    if( Tbl[median] > Threshold ){
      sum += Tbl[median];
      if( sum > sumhalf ) break;
    }
  }

  return median;
}

static int BrAver;
DLLEXPORT int __stdcall IP_getAver()
{
  IPGOFeatureTbl feature;
  long table[256];

  if( SetAllWindow(DeviceID,0, 0, IP_DRAW_WIDTH - 1, IP_DRAW_HEIGHT - 1) != 0 )
    return IP_ERROR;
  if( IP_GetCamera(DeviceID,image_imgid) != 0 )
    return IP_ERROR;
  if( IP_Histogram(DeviceID,image_imgid, table, &feature, 0) != 0 )
    return IP_ERROR;

  BrAver = BrMedian( table );
  return BrAver;
}
#if 0
double GetMeanValue( void )
{
  int nPixel = 512 * 440;
  int brTotal = 0;
  int br = 0;
  IPGOFeatureTbl feature;
  long table[256];
  IMGID imageID = RowImageIDs[0];

  if( SetAllWindow(DeviceID,0, 0, IP_DRAW_WIDTH - 1, IP_DRAW_HEIGHT - 1) != 0 )
    return IP_ERROR;
  if( IP_GetCamera(DeviceID,imageID) != 0 )
    return IP_ERROR;
  if( IP_Histogram(DeviceID,imageID, table, &feature, 0) != 0 )
    return IP_ERROR;

  for( br = 1; br < 256; br++ ){
    brTotal += br * table[br];
  }
  
  return (double)brTotal / nPixel;
}
#endif
static int CalcContrast( IMGID img )
{
  static int coeff[] = { 7, 7, 7, 7, 7, 7, 7, 7, 7 };
  IPGOFeatureTbl feature;
  long table[256], *l;
  int contrast = 0;
  int ndust = 0;

  /* check dust */
  if( IP_Histogram( DeviceID, img, table, &feature, 0 ) != 0 )
    return IP_ERROR;

  for( l=table+1; l<table+image_thresh_br_dust; l++ )
    ndust += *l;

  switch( gTSP.type ){
  case EMThinType:
    if( ndust > image_thresh_dust_thin  )
      return IP_ERROR;
    break;
  case EMThickType:
    if( ndust > image_thresh_dust_thick )
      return IP_ERROR;
    break;
  }

  /* calc contrast */
  if( IP_SmoothFLT(DeviceID,img, image_imgid_tmp, 6, coeff) != 0 )
    return IP_ERROR;
  if( IP_Lapl8FLTAbs(DeviceID,image_imgid_tmp, image_imgid_laplacian) != 0 )
    return IP_ERROR;
  if( IP_Histogram(DeviceID,image_imgid_laplacian, table, &feature, 0) != 0 )
    return IP_ERROR;
  for( l=table+image_thresh_br_hit_number; l<table+256; l++ )
    contrast += *l;

  return contrast;
}

DLLEXPORT int __stdcall IP_getHitNum()
{
  if( SetAllWindow(DeviceID,0, 0, IP_DRAW_WIDTH - 1, IP_DRAW_HEIGHT - 1) != 0)
    return IP_ERROR;
  if( IP_GetCamera(DeviceID,image_imgid) != 0)
    return IP_ERROR;

  return CalcContrast(image_imgid);
}
#if 0
DLLEXPORT int __stdcall
IP_getHitNum0( short thr, short *hit1, short *hit2, short *hit3, short *hit4,
	       double *x, double *y )
{
#define RANGE 12
  IPGOFeatureTbl feature;
  long table[256];
  int hit = 0, br, brsum = 0;
  int i;
  int ix, iy, ihit1 = 0, ihit2 = 0, ihit3 = 0, ihit4 = 0;
  unsigned char *p, *p0;
  int x0 = 0, y0 = 0;

  if (thr < 0 || thr > 255) return IP_ERROR;

  /* get camera image and its histogram */
  if( SetAllWindow(DeviceID,0, 0, IP_DRAW_WIDTH - 1, IP_DRAW_HEIGHT - 1) != 0 )
    return IP_ERROR;
  if( IP_GetCamera(DeviceID,image_imgid) != 0 )
    return IP_ERROR;
  if( IP_Histogram(DeviceID,image_imgid, table, &feature, 0) != 0 )
    return IP_ERROR;
  if( ReadImg( DeviceID,image_imgid, (char *) TmpDraw, IP_DRAW_SIZE )
      != IP_DRAW_SIZE )
    return IP_ERROR;

  /* calc hit# */
  for( i = 1; i < thr; i++ )
    hit += table[i];
  if( hit == 0 )
    return hit;

  /* calc center */
  p = (unsigned char *) TmpDraw;
  for( iy = 0; iy < IP_DRAW_HEIGHT; iy ++ ){
    for( ix = 0; ix < IP_DRAW_WIDTH; ix ++ ){
      if( *p < thr ){
	br = thr - *p;
	brsum += br;
	x0 += ix * br;
	y0 += iy * br;
      }
      p++;
    }
  }
  *x = (double) x0 / brsum;
  *y = (double) y0 / brsum;

  /* calc hit# on 4 sides of the image */
  for( iy = 0; iy < RANGE; iy++ ){
    p = &(TmpDraw[iy][0]);
    for( p = &(TmpDraw[iy][0]); p < &(TmpDraw[iy][IP_DRAW_WIDTH]); p++ ){
      if( *p <= thr )
	ihit1++;
    }
  }
  *hit1 = ihit1;
  for( iy = IP_DRAW_HEIGHT - RANGE; iy < IP_DRAW_HEIGHT; iy ++ ){
    for( p = &(TmpDraw[iy][0]); p < &(TmpDraw[iy][IP_DRAW_WIDTH]); p++ ){
      if( *p < thr )
	ihit2 ++;
    }
  }
  *hit2 = ihit2;
  p0 = (unsigned char *) TmpDraw;
  for( iy = 0; iy < IP_DRAW_HEIGHT; iy++ ){
    for( p = p0; p < p0 + RANGE; p++ )
      if( *p < thr )
	ihit3++;
    p0 += IP_DRAW_WIDTH;
    for( p = p0 - RANGE; p < p0; p++ )
      if( *p < thr )
	ihit4++;
  }
  *hit3 = ihit3;
  *hit4 = ihit4;

  return hit;
}

static int Threshold0;
DLLEXPORT int __stdcall IP_ReadCurrentImage( LONG deviceID, LONG numOfImage  )
{
  LONG i=0;
  for( i=0; i<numOfImage; i++ ){
    if( ReadImg( deviceID, RowImageIDs[i], (char*)imageData[i].raw,
		IP_DRAW_SIZE)	!= IP_DRAW_SIZE ) {
		return -2;
	}
  }

  return 0;
}

DLLEXPORT int __stdcall
IP_ContImgCheck( short mode, short emtype, short iid, short *thr,
		 short *throffset )
{
  unsigned char *p, *brp1, *brp2, *segp1, *segp2;
  int ix, iy;
  static unsigned char ThreshImage1[IP_HALF_HEIGHT][IP_HALF_WIDTH];
  static unsigned char ThreshImage2[IP_HALF_HEIGHT][IP_HALF_WIDTH];

  if( iid < 0 || iid >= NumOfStep ) return IP_ERROR;

  switch( mode ){
  case 0:
    *thr = Threshold0;
    if( emtype == EMThickType ){
	  // 画像を反転する。IP_Invert()でも良い。
      IP_SubConstAbs( DeviceID,RowImageIDs[iid], image_imgid_tmp, 255 );
      IP_SubConst( DeviceID,image_imgid_tmp, image_imgid_inv, abs(*thr) );
      if( ReadImg( DeviceID,image_imgid_inv, (char *) TmpDraw,
		   IP_DRAW_SIZE) != IP_DRAW_SIZE )
	return IP_ERROR;// 画像メモリ読み出し
      brp1  = (unsigned char *) TmpDraw;
      brp2  = (unsigned char *) TmpDraw + IP_DRAW_WIDTH;
      segp1 = (unsigned char *) TmpHalf1;
      segp2 = (unsigned char *) TmpHalf2;
      for( iy = 0; iy < IP_HALF_HEIGHT; iy++ ){
	for( ix = 0; ix < IP_HALF_WIDTH; ix++ ){
	  *segp1 = (*brp1 > *(brp1 + 1)) ? *brp1 : *(brp1 + 1);
	  *segp2 = (*brp2 > *(brp2 + 1)) ? *brp2 : *(brp2 + 1);
	  brp1 += 2;
	  brp2 += 2;
	  segp1++;
	  segp2++;
	}
	brp1 = brp2;
	brp2 += IP_DRAW_WIDTH;
      }
      *throffset =
	LocalThresh((unsigned char *) ThreshImage1,(unsigned char *) TmpHalf1);
    }
    if( ReadImg( DeviceID,RowImageIDs[iid], (char *) TmpDraw,
		 IP_DRAW_SIZE)	!= IP_DRAW_SIZE )
      return IP_ERROR;	// 画像メモリ読み出し
    break;
  default:
    if( *thr < 0 || *thr > 255 )
      return IP_ERROR;
    // 画像を反転する。IP_Invert()でも良い。
    IP_SubConstAbs( DeviceID,RowImageIDs[iid], image_imgid_tmp, 255 );
    IP_SubConst( DeviceID,image_imgid_tmp, image_imgid_inv, abs(*thr) );
    if( ReadImg( DeviceID,image_imgid_inv, (char *) TmpDraw,
		 IP_DRAW_SIZE) != IP_DRAW_SIZE )
      return IP_ERROR;	// 画像メモリ読み出し
    if( emtype == EMThickType ){
      brp1  = (unsigned char *) TmpDraw;
      brp2  = (unsigned char *) TmpDraw + IP_DRAW_WIDTH;
      segp1 = (unsigned char *) TmpHalf1;
      segp2 = (unsigned char *) TmpHalf2;
      for( iy = 0; iy < IP_HALF_HEIGHT; iy++ ){
	for( ix = 0; ix < IP_HALF_WIDTH; ix++ ){
	  *segp1 = (*brp1 > *(brp1 + 1)) ? *brp1 : *(brp1 + 1);
	  *segp2 = (*brp2 > *(brp2 + 1)) ? *brp2 : *(brp2 + 1);
	  brp1 += 2;
	  brp2 += 2;
	  segp1++;
	  segp2++;
	}
	brp1 = brp2;
	brp2 += IP_DRAW_WIDTH;
      }
      LocalThresh( (unsigned char *)ThreshImage1,(unsigned char *)TmpHalf1 );
      Distinctive( *throffset, (unsigned char *)TmpHalf1,
		   (unsigned char *)TmpDraw, (unsigned char *)ThreshImage1 );
      LocalThresh( (unsigned char *) ThreshImage2,(unsigned char *) TmpHalf2 );
      Distinctive( *throffset, (unsigned char *)TmpHalf2,
		   (unsigned char *)TmpDraw + IP_DRAW_WIDTH,
		   (unsigned char *)ThreshImage2 );
    }
    if( mode == 1 ) break;
    for( p = (unsigned char *)TmpDraw;
	 p < (unsigned char *)TmpDraw + IP_DRAW_SIZE;
	 p++ )
      *p = *p > 0 ? 255 : 0;
  }
  NewImage( hWnd, hDC, Hi, NULL, (unsigned char *)TmpDraw );

  return IP_NORMAL;
}

DLLEXPORT int __stdcall IP_MonitorDisplayImage( unsigned char *buf )
{
  WriteImg(DeviceID,image_imgid_tmp, (char*)buf, IP_DRAW_SIZE);

  return DispImg(DeviceID,image_imgid_tmp );
}

DLLEXPORT int __stdcall IP_MonitorDisplayCameraImage()
{
  return DispCamera(DeviceID);
}

// IP_GetImageContly()で使うRowImageIDsを取り出す
DLLEXPORT void __stdcall IP_GetImageContlyBuffer( int *List, int num )
{
  int i;
  for( i=0; i<num; i++ )
    List[i] = RowImageIDs[i];
}

int *gThresholdTmp;
int gFlag = 0;
DLLEXPORT int __stdcall IP_GetImageContly( short thr_surface, short thr_track)
{
  int iid, braver;
  long table[256];
  IPGOFeatureTbl feature;
  
  //#ifdef __cplusplus
  //extern "C"{
  //#endif
  
emscan EMscanData;
  
  //#ifdef __cplusplus
  //}
  //#endif
  /* calc threshold */
  IP_Histogram( DeviceID,RowImageIDs[0], table, &feature, 0 );
  braver = BrMedian( table );
  Threshold0 = 255 - thr_track;
  if( Threshold0 <= 0 || Threshold0 > 200 ){
    if( emio_log != NULL ){
      fprintf( emio_log, "Error: Too Dark at Track %d-%d\n",
	       EMscanData.iTrack, EMscanData.sub );
      fflush( emio_log );
    }
    return -1;			// too dark
  }

  /* invert image and read image data */
  for( iid = 0; iid < NumOfStep; iid++ ){
    EnablePipeline(DeviceID);
    // invert image brightness. (equivarent to IP_Invert())
    IP_SubConstAbs(DeviceID,RowImageIDs[iid], image_imgid_tmp, 255);
    IP_SubConst(DeviceID,image_imgid_tmp, image_imgid_inv, Threshold0);
    DisablePipeline(DeviceID);

    // read image
    if( ReadImg( DeviceID,image_imgid_inv, (char*)imageData[iid].raw, IP_DRAW_SIZE )
	!= IP_DRAW_SIZE )
      return -2;
  }

  return braver;
}

DLLEXPORT int __stdcall IP_PreImageProcessing( short thr_surface, short thr_track, int emType )
{
  int status = IP_NORMAL;

  switch( emType ){
  case EMThinType:
    status = IP_ImageAutoRange( thr_surface, emType );
    if( status == IP_NORMAL ){
      status = IP_GetImageContly( thr_surface, thr_track );
    }
    break;
  case EMThickType:
    status = PreImageProcessing( emType );
    break;
  }

  return status;
}

/* extern FILE *fpDebug; */
DLLEXPORT int __stdcall IP_ImageAutoRange( short thr_surface, int emType )
{
  //	IP_DRAW_SIZE is equal to 512L*440L
  static unsigned char bufSrc [512L*512L];
  static unsigned char bufDest[512L*512L];
  int i;

  /* check the z-position of the plate */
  if( gTSP.type == EMThinType )
    if( CalcContrast( RowImageIDs[0] )           < thr_surface ||
	CalcContrast( RowImageIDs[NumOfStep-1] ) < thr_surface )
      return -99;

  for( i=0; i<NumOfStep; i++ ){
    ReadImg( DeviceID, RowImageIDs[i], (char*)bufSrc, IP_DRAW_SIZE );
    ImageAutoRange( bufDest, bufSrc, emType );
    WriteImg( DeviceID, RowImageIDs[i], (char*)bufDest, IP_DRAW_SIZE );
  }

  return 0;
}

static int GetImageMedian( unsigned char *data )
{
  IPGOFeatureTbl feature;
  long table[256];
  
  WriteImg( DeviceID,image_imgid_tmp,(char*) data, IP_DRAW_SIZE );
  IP_Histogram( DeviceID, image_imgid_tmp, table, &feature, 0 );
  
  return BrMedian( table );
}

void ImageAutoRange( unsigned char *dest, unsigned char *src, int emType )
{
  switch( emType ){
  case 1: // thin
    ImageAutoRangeThin( dest, src );
    break;
  case 2: // thick
    ImageAutoRangeThick( dest, src );
    break;
  }
}
static void ImageAutoRangeThin( unsigned char *dest, unsigned char *src )
{
  double factor;
  double val;
  int i, minVal=255, maxVal=0;

  minVal = 70;
  maxVal = GetImageMedian( src );

  factor = ( 255.0 - 0. ) / (maxVal - minVal);
  for( i=0; i<IP_DRAW_SIZE; i++ ){
    val = ( src[i] - minVal ) * factor + 0.;
    if     ( val > 255.0 ) val = 255.0;
    else if( val <   0.0 ) val =   0.0;

    dest[i] = (int)(val);
  }
}
static void ImageAutoRangeThick( unsigned char *dest, unsigned char *src )
{
  double factor;
  double val;
  int i, minVal=255, maxVal=0;
  long table[256];
  IPGOFeatureTbl feature;

/*   maxVal = GetImageMedian( src ); */

  WriteImg( DeviceID, image_imgid_tmp, (char*)src, IP_DRAW_SIZE );
  IP_Histogram( DeviceID, image_imgid_tmp, table, &feature, 0 );
  maxVal = feature.MAX_LEVEL;
  minVal = feature.MIN_LEVEL;

  factor = ( 255.0 - 0. ) / (maxVal - minVal);
  for( i=0; i<IP_DRAW_SIZE; i++ ){
    val = ( src[i] - minVal ) * factor + 0.;
    if     ( val > 255.0 ) val = 255.0;
    else if( val <   0.0 ) val =   0.0;

    dest[i] = (unsigned char)(val);
  }
}

static double* GetBokashiImageByGaus( int size, int* br )
{
  double *brSmooth = (double*)malloc( sizeof(double)*IP_HALF_SIZE );
  int ix0, iy0, ixy0;
  int ix, iy, ixy;

  memset( brSmooth, 0x00, sizeof(double)*IP_HALF_SIZE );
  for( ix0=size+1; ix0<IP_HALF_WIDTH-size; ix0++ ){
    for( iy0=size+1; iy0<IP_HALF_HEIGHT-size; iy0++ ){
      ixy0 = iy0*IP_HALF_WIDTH + ix0;

      for( ix=-size; ix<=size; ix++ ){
	for( iy=-size; iy<=size; iy++ ){
	  ixy = (iy0+iy)*IP_HALF_WIDTH + (ix0+ix);
	  brSmooth[ixy0] += br[ixy] * GetGausValue( ix, iy );
	}
      }
    }
  }

  return brSmooth;
}
static void GetImageProfile
( unsigned char *br, double *base, double *center, double *sigma )
{
  int i, j;
  int ix, iy, val;
  int dist[200];
  int maxVal, low, high;
  static const double factor = 0.425; // 1/(2*sqrt(2*log(2))) (FWHM->sigma)

  memset( dist, 0x00, sizeof(int)*200 );
  for( i=0; i<IP_DRAW_SIZE; i++ ){
    ix = (i%IP_DRAW_WIDTH)/2;
    iy = (i/IP_DRAW_WIDTH)/2;
    val = (int)(base[ iy*IP_HALF_WIDTH + ix ]/4) - br[i];
    if( -100<=val && val<100 ){
      dist[val+100]++;
    }
  }

  maxVal = 0;
  for( i=0; i<200; i++ ){
    if( maxVal < dist[i] ){
      maxVal = dist[i];
      *center = i-100;
    }
  }

  low  = -100;
  high =  100;
  j = 0;
  for( i=0; i<200; i++ ){
    if( dist[i] > maxVal/2 ){
      low = i-100;
      j = i;
      break;
    }
  }
  for( i=j; i<200; i++ ){
    if( dist[i] < maxVal/2 ){
      high= i-100-1;
      break;
    }
  }

  *sigma = factor * ( high - low );
}

int PreImageProcessing( int emType )
{
  int i;

  for( i=0; i<NumOfStep; i++ ){
    ReadImg( DeviceID, RowImageIDs[i], (char*)imageData[i].raw, IP_DRAW_SIZE );
  }

  switch( emType ){
  case EMThickType:
    MakeShrinkData();
    MakeGausValue( bokashi_size, bokashi_sigma );
    PreImageProcessingThick();
    break;
  default:
    return IP_ERROR;
  }

  return IP_NORMAL;
}
static void MakeEnhancedImage( unsigned char *br, double *base, double center, double sigma )
{
  int i, ix, iy, val;
  double thr;

  thr = center + 5.0 * sigma;
/*   thr = center + 3.5 * sigma; */
  for( i=0; i<IP_DRAW_SIZE; i++ ){
    ix = (i%IP_DRAW_WIDTH)/2;
    iy = (i/IP_DRAW_WIDTH)/2;
    val = (int)(base[ iy*IP_HALF_WIDTH + ix ]/4 - br[i] - thr);
    if( val > 0 ){
      br[i] = val;
    }
    else{
      br[i] = 0;
    }
  }
} 
static void PreImageProcessingThick( void )
{
  int i;
  double *brBase;
  double center, sigma;

  for( i=0; i<NumOfStep; i++ ){
    brBase = GetBokashiImageByGaus( bokashi_size, imageData[i].half );
    GetImageProfile( imageData[i].raw, brBase, &center, &sigma );
    MakeEnhancedImage( imageData[i].raw, brBase, center, sigma );
    free( brBase );
  }
}

void DumpCameraImage( char *filename, IMGID *imgID, int n )
{
  FILE *fp = fopen( filename, "wb" );
  int i;
  unsigned char data[IP_DRAW_SIZE];

  for( i=0; i<n; i++ ){
    ReadImg( DeviceID, imgID[i],(char*) data, IP_DRAW_SIZE );
    fwrite( data, 1, IP_DRAW_SIZE, fp );
  }

  fclose(fp);
}

void GetCameraImage( unsigned char *data )
{
  ReadImg( DeviceID, RowImageIDs[0], (char*)data, IP_DRAW_SIZE );
}

DLLEXPORT void __stdcall IP_MakePictureForConnection( char *filename )
{
  int i, j;
  unsigned char buf[IP_DRAW_SIZE];

  memset( buf, 0x00, IP_DRAW_SIZE );
  for( i=NumOfStep-1; i>=0; i-- ){
    for( j=0; j<IP_DRAW_SIZE; j++ ){
      if( imageData[i].raw[j] ){
	if( i<NumOfStep/2 ){
	  buf[j] = 127;
	}
	else{
	  buf[j] = 255;
	}
      }
    }
  }

  BmpSave( filename, bmRB->bmiColors, IP_DRAW_WIDTH, IP_DRAW_HEIGHT, buf );
}

// Following function is made for test of beam tracking at SearchSurface.
void CopyCameraImage( unsigned char *dest, int imageBuffer )
{
  ReadImg( DeviceID, imageBuffer, (char*)dest, IP_DRAW_SIZE );
}

ImageDataColl* GetImageDataColl( int id )
{
  return &(imageDataColl[id]);
}

int AllocateImageMemory( int id, int nImage )
{
  int iImage;
  ImageDataColl *imgColl = GetImageDataColl( id );

  imgColl->n = nImage;
  imgColl->data = (ImageData**) malloc( sizeof( ImageData* ) * nImage );
  for( iImage=0; iImage<nImage; iImage++ ){
    ImageData *img = (ImageData*) malloc( sizeof( ImageData ) );
    img->raw = (unsigned char*) malloc( sizeof(unsigned char) * IP_DRAW_SIZE );
    img->half = 0;
    img->quarter = 0;
    img->z = 0.;

    imgColl->data[iImage] = img;
  }

  return IP_NORMAL;
}

void ReleaseImageMemory( int ud )
{
  int iImage;
  ImageDataColl *imgColl = GetImageDataColl( ud );

  for( iImage=0; iImage<imgColl->n; iImage++ ){
    ImageData *img = imgColl->data[iImage];

    free( img->raw );
    free( img->half );
    free( img->quarter );
    free( img );
  }
  free( imgColl->data );
}

DLLEXPORT int __stdcall IP_CopyCameraImage( int ud, int imageID, int imageBuffer, double z )
{
  ImageDataColl *imgColl = GetImageDataColl( ud );

  if( imageID<0 || imageID>=imgColl->n ){
    return IP_ERROR;
  }

  CopyCameraImage( imgColl->data[imageID]->raw, imageBuffer );
  imgColl->data[imageID]->z = z;

  return IP_NORMAL;
}

void DumpImageDataCollTo( int id, int nImage, char *fnameI, char *fnameZ )
{
  int i;
  FILE *fpI, *fpZ;
  ImageDataColl *imgColl = GetImageDataColl( id );

  fpI = fopen( fnameI, "wb" );
  fpZ = fopen( fnameZ, "w" );

  for( i=0; i<nImage; i++ ){
/*   for( i=0; i<imgColl->n; i++ ){ */
    fwrite( imgColl->data[i]->raw, 1, IP_DRAW_SIZE, fpI );
    fprintf( fpZ, "%3d %10.4lf\n", i, imgColl->data[i]->z );
  }

  fclose( fpI );
  fclose( fpZ );
}
void DumpImageDataColl( int id )
{
  char *fnameI, *fnameZ;

  fnameI = (char*) malloc( sizeof(char) * STRLEN );
  fnameZ = (char*) malloc( sizeof(char) * STRLEN );
  sprintf( fnameI, "image%02d.dat", id );
  sprintf( fnameZ, "Z%02d.txt", id );

  DumpImageDataCollTo( id, GetImageDataColl(0)->n, fnameI, fnameZ );

  free( fnameI );
  free( fnameZ );
}

static void ReadDataPos( ImageDataColl *imgColl, char *fileName, int nImage)
{
  int i, id;
  double z;
  char *s=(char *)malloc(150);
  FILE *fp = fopen( fileName, "r" );
  for( i=0; i<nImage; i++ ){
    fscanf( fp, "%d %lf", &id, &z );
    imgColl->data[id]->z = z;
  }
  fclose(fp);
}
int ReadRawData( ImageDataColl *imgColl, char *fileNameImage, char* fileNamePos )
{
  FILE *fp = fopen( fileNameImage, "rb" );
  unsigned char data[IP_DRAW_SIZE];
  int i, nImage=0;

  while( fread( data, sizeof(unsigned char), IP_DRAW_SIZE, fp ) ){
    nImage++;
  }
  AllocateImageMemory( 0, nImage );
  fseek( fp, 0, SEEK_SET );
  for( i=0; i<nImage; i++ ){
    fread( data, sizeof(unsigned char), IP_DRAW_SIZE, fp );
    memcpy( imgColl->data[i]->raw, data, sizeof(unsigned char) * IP_DRAW_SIZE );
  }

  fclose(fp);

  ReadDataPos( imgColl, fileNamePos, nImage );

  return nImage;
}
#endif