#ifndef __IPXMACRO_H__
#define __IPXMACRO_H__
/******************************************************************************
		VPSeries SDK Macro Library Header File
	Copyright (C) 2000-2004 Renesas  Northern Japan Semiconductor,Inc.
	  All rights reserved
******************************************************************************/
#include "ipxdef.h"

/* ２値化処理方式 */
enum IPMBinPercentMethod {
	BINPERCENT_BIG,   /* 大きい方を１にする */
	BINPERCENT_SMALL  /* 小さい方を１にする */
};

#define CNVLUT_MAX 8 /* 濃度変換テーブル最大登録数 */

/* 平滑化処理方式 */
enum IPMSmoothMethod {
	SMOOTHNOOP,
	SMOOTH1,
	SMOOTH2,
	SMOOTH3,
	SMOOTH4,
	MEDIAN8,
	MEDIAN4,
	MEDIAN84,
	MEDIAN48,
	SENSUP2,
	SENSUP4,
	MAX8,
	MIN8
};

/* 輪郭強調処理方式 */
enum IPMEdgeMethod {
	EDGENOOP,
	EDGEMAXVH,
	EDGEADDVH,
	EDGEV,
	EDGEH,
	EDGE_SOBEL_DASH,
	EDGE_ZEROCROSS8_ALL,
	EDGE_ZEROCROSS4_ALL,
	EDGE_ZEROCROSS8_SEQ,
	EDGE_ZEROCROSS4_SEQ,
	EDGE_MINMAX,
	EDGE_SOBEL
};

/* 濃度変換方式 */
enum IPMGenerateClipMethod {
	CLIP_Y_X,
	CLIP_0_255
};

/* ゼロクロッシング */
/* 平滑化処理方式 */
enum IPMZeroXSmoothMethod {
	ZEROCR_SMOOTHNOOP,
	ZEROCR_SMOOTH1,
	ZEROCR_SMOOTH2,
	ZEROCR_SMOOTH3,
	ZEROCR_SMOOTH4
};

/* 輪郭強調フィルタ処理方式 */
enum IPMZeroXLapMethod {
	ZEROCR_LAP_EDGE4,
	ZEROCR_LAP_EDGE8
};

enum HoughOPT {
	SORT_DISABLE = 0x0,    /* ハフ空間の頻度でソートを無効  */
	SORT_ENABLE  = 0x1,    /* ハフ空間の頻度でソートを有効  */
	HOUGH_BINARY = 0x0,    /* 処理対象画像が二値画像        */
	HOUGH_GRAY   = 0x2     /* 処理対象画像が濃淡画像        */
};


typedef struct {
	int      blue; /* 青色(0～256) */
	int lightblue; /* 水色(0～256) */
	int     green; /* 緑色(0～256) */
	int    yellow; /* 黄色(0～256) */
	int       red; /* 赤色(0～256) */
} PseudoColorTBL;

typedef struct{
	float theta;    /* 傾き角度      */
	float rho;      /* 中心からの距離 */
	int frequency;  /* 頻度          */
} IPMHoughTbl;


#ifdef __cplusplus
extern "C"{
#endif

/* IPM_3DGraph.C */
int WINAPI IPM_Draw3DGraph( DEVID, int, unsigned char *, int, int, int, int, int, int, int, int );

/* IPM_PseudoColor.C */
int WINAPI IPM_PseudoColor( DEVID, int, int, unsigned char *, unsigned char *, unsigned char * );
int WINAPI IPM_RGBtoYUV( DEVID, unsigned char *, unsigned char *, unsigned char *, unsigned char *, unsigned char *, unsigned char *, int );
int WINAPI IPM_SetPseudoColor( DEVID, unsigned char *, unsigned char *, unsigned char * );
int WINAPI IPM_SetPseudoColorExt( DEVID, unsigned char *, unsigned char *, unsigned char *, PseudoColorTBL );

/* IPMBinarize.C */
int WINAPI IPM_BinarizePercentile( DEVID, int, int, int, int, long *, enum IPMBinPercentMethod );

/* IPMFLT1.C */
int WINAPI IPM_GenerateLUTLinear( DEVID, float, float, CNVLUT * );
int WINAPI IPM_GenerateLUTCubic( DEVID, int, float, int, float, CNVLUT * );
int WINAPI IPM_GenerateLUTGumma( DEVID, float, CNVLUT * );
int WINAPI IPM_GenerateLUTComb( DEVID, int, CNVLUT * );
int WINAPI IPM_GenerateLUTClip( DEVID, int, int, enum IPMGenerateClipMethod, CNVLUT * );
int WINAPI IPM_GenerateLUTRecursiveFLT( DEVID, int, int, int, CNVLUT * );
int WINAPI IPM_GenerateLUTRecursiveFLTBG( DEVID, int, int, int, int, int, CNVLUT * );
int WINAPI IPM_GenerateLUTEdgeFLTExt( DEVID, int, int, int, int, int, CNVLUT * );

/* IPMFLT2.C */
int WINAPI IPM_RegisterLUT( DEVID, int, CNVLUT * );
int WINAPI IPM_GrayLevelConvert( DEVID, int, int, int );
int WINAPI IPM_SmoothFLTExt( DEVID, int, int, enum IPMSmoothMethod, int );
int WINAPI IPM_EdgeFLTAbsExt( DEVID, int, int, enum IPMEdgeMethod, float, int );
int WINAPI IPM_RecursiveFLT( DEVID, int, int, int );
int WINAPI IPM_RecursiveFLTMax( DEVID, int, int, float );
int WINAPI IPM_RecursiveFLTMin( DEVID, int, int, float );
int WINAPI IPM_RecursiveFLTTyp( DEVID, int, int, float );
int WINAPI IPM_OritaFLT( DEVID, int, int, int, int );
int WINAPI IPM_BinarizeNoiseFLT( DEVID, int, int, int, int );
int WINAPI IPM_ZeroCrossFLT( DEVID, int, int, enum IPMZeroXSmoothMethod, int, enum IPMZeroXLapMethod, int, int );

/* IPMHistEQ.C */
int WINAPI IPM_GenerateLUTHistogramEQ( DEVID, long *, CNVLUT * );
int WINAPI IPM_HistogramEQ( DEVID, int, int, long *, CNVLUT * );
int WINAPI IPM_GenerateLUTHistogramID( DEVID, int, int, CNVLUT * );

/* 関数宣言 */
int WINAPI IPM_HoughTransBasic( DEVID, IMGID, float, float, int, int *, int *, int *, int *, enum HoughOPT );
int WINAPI IPM_HoughTrans( DEVID, IMGID, int, int, int, int, int, int, IPMHoughTbl *, enum  HoughOPT );

#ifdef __cplusplus
}    
#endif

#endif /* !__IPXMACRO_H__ */
