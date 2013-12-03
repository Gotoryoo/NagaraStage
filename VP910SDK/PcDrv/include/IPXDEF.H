#ifndef __IPXDEF_H__
#define __IPXDEF_H__
#if defined(WIN32) || defined(_WIN32_WCE)
#include <windows.h>
#else /* WIN32 || _WIN32_WCE */
#ifndef WINAPI
#define WINAPI
#endif /* !WINAPI */
#endif /* !(WIN32 || _WIN32_WCE) */
/******************************************************************************
		VPSeries Header file
	Copyright (C) 2000-2004 Renesas  Northern Japan Semiconductor,Inc.
	  All rights reserved
******************************************************************************/
/* 画像処理コマンド・ライブラリ識別コード */
/* IP4000 Series : 0x00004000 */
/* IP5000 Series : 0x00005000 */
/* SoftIP Series : 0x01000000 */
#define IPXCMD_LIB_CODE       (0x00005001)

/* 画像処理コマンド・バージョン識別コード */
#define IPXCMD_VERSION_CODE   (0x00010600)
#define GET_MAJOR( )          ((IPXCMD_VERSION_CODE>>16)&0xff)
#define GET_MINOR( )          ((IPXCMD_VERSION_CODE>> 8)&0xff)
#define GET_REV( )            ( IPXCMD_VERSION_CODE     &0xff)

/* 画像処理ボード種別 */
#define IP5000_BOARD          (0) /* モノクロボード */
#define IP5005_BOARD          (1) /* カラーボード   */
#define IP5010_BOARD          (0) /* 高精細ボード   */

/* IP2000CD互換 */
#ifdef IP2000
#define Version()         IPLibVersion()
#define RefreshGraphics() CloseImg()
#endif

/* ラベルオーバーフロー発生時の戻り値 */
#define LABEL_OVERFLOW    (0x7fff)

/* 正規化相関（SetCorrMode）の統合範囲 */
#define CORR_MERGE( x )  ( x << 8 )

/* 正規化相関（SetCorrMode）の疎密ランク */
#define COARSE_FINE( x )

/* 画像処理コマンド名最大文字数 */
#define MAX_IP_FUNCNAME    (32)

#ifndef MAX_PATH
#define MAX_PATH          (260)
#endif /* !MAX_PATH */

/* 画像メモリ管理テーブル最大値 */
//#define MAXIMGID          (256) /* 画面管理番号（最大数）   */
#define MAXIMGID          (506) /* 画面管理番号（最大数）   */

/* ２値画像マッチングフィルタ最大値 */
#define MAX_BMTMP          (64) /* テンプレートの最大登録数 */

/* 最大プロファイル登録数 */
#define MAX_PROFILE       (256)

#ifdef __IPXCMD_H__

/* 画像メモリ管理テーブル */
#define IMG_BW              (0) /* モノクロ画面                         */
#define IMG_YUV_Y           (1) /* 輝度Y画面                            */
#define IMG_YUV_UV          (2) /* 色差UV画面                           */
#define IMG_RGB_R           (3) /* R画面                                */
#define IMG_RGB_G           (4) /* G画面                                */
#define IMG_RGB_B           (5) /* B画面                                */
#define IMG_RGB_MIX16       (6) /* RGB画面16（RGB混合）                 */
#define IMG_RGB_MIX32       (7) /* RGB画面32（RGB混合）                 */

#define IMG_BASIC           (0) /* 基本画像メモリ領域（chart型/1バイト）*/
#define IMG_EXT             (1) /* リザーブ                             */
#define IMG_SHORT           (2) /* 基本画像メモリ領域（short型/2バイト）*/
#define IMG_LONG            (3) /* 基本画像メモリ領域（ long型/4バイト）*/
#define IMG_FLOAT           (4) /* 基本画像メモリ領域（float型/4バイト）*/

#define IMG_FS_FREE_SIZE   (-1) /* 任意画面サイズ                       */

#endif /* __IPXCMD_H__ */


/* 画像メモリ画面サイズ */
enum ImageFrameSize {
	IMG_FS_256H_256V,    /*  256H x 256V */
	IMG_FS_512H_256V,    /*  512H x 256V */
	IMG_FS_256H_512V,    /*  256H x 512V */
	IMG_FS_512H_512V,    /*  512H x 512V */
	IMG_FS_640H_256V,    /*  640H x 256V */
	IMG_FS_640H_512V,    /*  640H x 512V */
	IMG_FS_1024H_512V,   /* 1024H x 512V */
	IMG_FS_1024H_1024V,  /* 1024H x1024V */
	IMG_FS_1024H_256V,   /* 1024H x 256V */
	IMG_FS_1280H_1024V,  /* 1280H x1024V */
	IMG_FS_1280H_256V,   /* 1280H x 256V */
	IMG_FS_1280H_512V,   /* 1280H x 512V */
	IMG_FS_1024H_768V,   /* 1024H x 768V */
	IMG_FS_2048H_512V,   /* 2048H x 512V */
	IMG_FS_5248H_512V,   /* 5248H x 512V */
	IMG_FS_8192H_512V,   /* 8192H x 512V */
	IMG_FS_2048H_2048V,  /* 2048H x2048V */
	IMG_FS_2048H_1024V,  /* 2048H x1024V */
	IMG_FS_1920H_1024V   /* 1920H x1024V */
};

/* 映像入力画面サイズ */
enum VideoFrameSize {
	VIDEO_FS_256H_220V,  /*  256H x  220V       */
	VIDEO_FS_512H_220V,  /*  512H x  220V       */
	VIDEO_FS_256H_440V,  /*  256H x  440V(220V) */
	VIDEO_FS_512H_440V,  /*  512H x  440V(220V) */
	VIDEO_FS_256H_240V,  /*  256H x  240V       */
	VIDEO_FS_320H_240V,  /*  320H x  240V       */
	VIDEO_FS_512H_240V,  /*  512H x  240V       */
	VIDEO_FS_640H_240V,  /*  640H x  240V       */
	VIDEO_FS_256H_480V,  /*  256H x  480V(240V) */
	VIDEO_FS_320H_480V,  /*  320H x  480V(240V) */
	VIDEO_FS_512H_480V,  /*  512H x  480V(240V) */
	VIDEO_FS_640H_480V,  /*  640H x  480V(240V) */
	VIDEO_FS_512H_512V,  /*  512H x  512V       */
	VIDEO_FS_1008H_470V, /* 1008H x  470V       */
	VIDEO_FS_1024H_1024V,/* 1024H x 1024V       */
	VIDEO_FS_1024H_240V, /* 1024H x  240V       */
	VIDEO_FS_1024H_480V, /* 1024H x  480V(240V) */
	VIDEO_FS_1280H_1024V,/* 1280H x 1024V       */
	VIDEO_FS_1280H_240V, /* 1280H x  240V       */
	VIDEO_FS_1280H_480V, /* 1280H x  480V       */
	VIDEO_FS_1024H_768V, /* 1024H x  768V       */
	VIDEO_FS_1920H_1024V,/* 1920H x 1024V       */
	VIDEO_FS_2048H_1024V,/* 2048H x 1024V       */
	VIDEO_FS_1280H_256V  /* 1280H x  256V       */
};

/* 映像表示画面サイズ */
enum DispFrameSize {
	DISP_FS_256H_220V,   /*  256H x  220V(440V) */
	DISP_FS_512H_220V,   /*  512H x  220V(440V) */
	DISP_FS_256H_440V,   /*  256H x  440V       */
	DISP_FS_512H_440V,   /*  512H x  440V       */
	DISP_FS_256H_240V,   /*  256H x  240V(480V) */
	DISP_FS_320H_240V,   /*  320H x  240V(480V) */
	DISP_FS_512H_240V,   /*  512H x  240V(480V) */
	DISP_FS_640H_240V,   /*  640H x  240V(480V) */
	DISP_FS_256H_480V,   /*  256H x  480V       */
	DISP_FS_320H_480V,   /*  320H x  480V       */
	DISP_FS_512H_480V,   /*  512H x  480V       */
	DISP_FS_640H_480V,   /*  640H x  480V       */
	DISP_FS_512H_512V,   /*  512H x  512V       */
	DISP_FS_1008H_470V,  /*  1008H x 470V       */
	DISP_FS_1024H_1024V, /*  1024H x 1024V      */
	DISP_FS_1024H_240V,  /*  1024H x 240V(480V) */
	DISP_FS_1024H_480V,  /*  1024H x 480V       */
	DISP_FS_1280H_1024V, /*  1280H x 1024V      */
	DISP_FS_1280H_240V,  /*  1280H x  240V      */
	DISP_FS_1280H_480V,  /*  1280H x  480V      */
	DISP_FS_1024H_768V,  /*  1024H x  768V      */
	DISP_FS_1920H_1024V, /*  1920H x 1024V      */
	DISP_FS_2048H_1024V, /*  2048H x 1024V      */
	DISP_FS_1280H_256V   /*  1280H x  256V      */
};

/* 画像メモリ画面属性 */
enum ImageFrameType {
	IMG_FS_CHAR,      /* モノクロ画像データ（１バイト）*/
	IMG_FS_SHORT,     /* モノクロ画像データ（２バイト）*/
	IMG_FS_LONG,      /* モノクロ画像データ（４バイト）*/
	IMG_FS_FLOAT,     /* モノクロ画像データ（４バイト）*/
	IMG_FS_YUV,       /* ＹＵＶ画像データ              */
	IMG_FS_RGB,       /* ＲＧＢ画像データ              */
	IMG_FS_RGB_MIX16, /* ＲＧＢ画像データ１６（混合）  */
	IMG_FS_RGB_MIX32  /* ＲＧＢ画像データ３２（混合）  */
};

enum CHID {
	IMG_CH0, /* 画像メモリチャネル#0 */
	IMG_CH1, /* 画像メモリチャネル#1 */
	IMG_CH2, /* 画像メモリチャネル#2 */
	IMG_CH3, /* 画像メモリチャネル#3 */
	IMG_CH4  /* 画像メモリチャネル#4 */
};

enum PageX_Y {
	PAGE_0,  /* SX or SY =    0 */
	PAGE_1,  /* SX or SY =  256 */
	PAGE_2,  /* SX or SY =  512 */
	PAGE_3,  /* SX or SY =  768 */
	PAGE_4,  /* SX or SY = 1024 */
	PAGE_5,  /* SX or SY = 1280 */
	PAGE_6,  /* SX or SY = 1536 */
	PAGE_7,  /* SX or SY = 1792 */
	PAGE_8,  /* SX or SY = 2048 */
	PAGE_9,  /* SX or SY = 2304 */
	PAGE_10, /* SX or SY = 2560 */
	PAGE_11, /* SX or SY = 2816 */
	PAGE_12, /* SX or SY = 3072 */
	PAGE_13, /* SX or SY = 3328 */
	PAGE_14, /* SX or SY = 3584 */
	PAGE_15  /* SX or SY = 3840 */
};

/* データタイプ種別 */
enum DataType {
	SIGN8_DATA,    /* 濃淡画像sign8    */
	UNSIGN8_DATA,  /* 濃淡画像unsign8  */
	BINARY_DATA,   /* ２値画像         */
	UV_DATA,       /* 色差画像         */
	SIGN16_DATA,   /* 濃淡画像sign16   */
	UNSIGN16_DATA, /* 濃淡画像unsign16 */
	SIGN32_DATA,   /* 濃淡画像sign32   */
	UNSIGN32_DATA, /* 濃淡画像unsign32 */
	FLOAT_DATA     /* 濃淡画像float    */
};

/* ウインドウ種別 */
enum WindowType {
	SRC0_WIN,    /* 画像処理のソース＃０画面ウインドウ           */
	SRC1_WIN,    /* 画像処理のソース＃１ 画面ウインドウ          */
	SRC2_WIN,    /* Reserved                                     */
	DST_WIN,     /* 画像処理のデスティネーション画面ウインドウ   */
	SYS_WIN,     /* システムプロセッサアクセス画面ウインドウ     */
	EXT_WIN,     /* Reserved                                     */
	DST_EXT_WIN  /* 画像処理のデストネーション拡張画面ウインドウ */
};

enum WaitMode {
	WAIT_FOREVER, /*  オープン状態になるまでロック */
	NOWAIT        /*  現在の状態を返して戻る       */
};

enum Interlace {
	NONINTERLACE,		/* ノンインタレース */
	INTERLACE,			/* インタレース     */
	NONINTERLACE_FAST,	/* ノンインタレース＆任意フィールド */
	NONINTERLACE_1N,	/* プログレッシブスキャン */
	KEEP_INTERLACE = -1	/* インタレースモードを変更しない */
};

enum CameraID {
	CAMERA_PORT0, /* カメラポート番号０ */
	CAMERA_PORT1, /* カメラポート番号１ */
	CAMERA_PORT2, /* カメラポート番号２ */
	CAMERA_PORT3  /* カメラポート番号３ */
};

enum CameraType {
	BW_CAMERA					= 0x00000000,	/* モノクロカメラ                              */
	RGB_CAMERA					= 0x00000000,	/* ＲＧＢカラーカメラ(未サポート)              */
	RGB_S_CAMERA				= 0x00000000,	/* ＲＧＢカラー（Ｓビデオ）カメラ(未サポート)  */
	YUV_CAMERA					= 0x00000001,	/* CVBSカラーカメラ                            */
	YUV_S_CAMERA				= 0x00000001,	/* ＹＵＶカラー（Ｓビデオ）カメラ(未サポート)  */
	HIREZ_CAMERA				= 0x00000000,	/* 高精細カメラ(未サポート)                    */
	PRG_CAMERA					= 0x00000000,   /* プログレッシブカメラ(未サポート)            */
	BW_CAMERA_HS				= 0x00000000,	/* モノクロカメラ(未サポート)                  */
	NEC_TI480A					= 0x00000002,	/* 1N倍速ｶﾒﾗ : NEC TI-480A ﾉｰﾏﾙﾓｰﾄﾞ            */
	TELI_CS3910					= 0x00000003, 	/* 高精細ｶﾒﾗ：東京電子 CS3910 ﾉｰﾏﾙﾓｰﾄﾞ         */
	TELI_CS6900					= 0x00000004, 	/* RGB-1Iｶﾒﾗ：東京電子 CS6900 ﾉｰﾏﾙﾓｰﾄﾞ         */
	TELI_CS3720					= 0x00000005,	/* 2I高速ｶﾒﾗ : TELI CS3720 ﾉｰﾏﾙﾓｰﾄﾞ            */
	TELI_CS3720_2N				= 0x00000005,	/* 2I高速ｶﾒﾗ : TELI CS3720 ﾉｰﾏﾙﾓｰﾄﾞ            */
	NEC_TI150A					= 0x00000006, 	/* 2Iﾌﾟﾛｸﾞﾚｯｼﾌﾞｶﾒﾗ : NEC TI-150A ﾉｰﾏﾙﾓｰﾄﾞ      */
	SONY_XC55					= 0x00000007,	/* 1Nﾌﾟﾛｸﾞﾚｯｼﾌﾞｶﾒﾗ : SONY XC-55  ﾉｰﾏﾙﾓｰﾄﾞ      */
	SONY_XC551N					= 0x00000007,	/* 1Nﾌﾟﾛｸﾞﾚｯｼﾌﾞｶﾒﾗ : SONY XC-55  ﾉｰﾏﾙﾓｰﾄﾞ      */
	PULNIX_TMC6700				= 0x00000008, 	/* RGB-1N倍速ｶﾒﾗ：PULNIX TMC6700 ﾉｰﾏﾙﾓｰﾄﾞ      */
	SONY_XCHR300				= 0x00000009,	/* 2I高速ｶﾒﾗ : SONY XC-HR300 ﾉｰﾏﾙﾓｰﾄﾞ          */
	HITACHI_KPF3W				= 0x0000000A,	/* 1N倍速ｶﾒﾗ : HITACHI KP-F3W ﾉｰﾏﾙﾓｰﾄﾞ         */
	CIS_VCC8300					= 0x0000000B,	/* RGB-1Nｶﾒﾗ：CIS VCC8300 ﾉｰﾏﾙﾓｰﾄﾞ             */
	TELI_CS8531					= 0x0000000C,	/* 2I高速ｶﾒﾗ : TELI CS8531 ﾉｰﾏﾙﾓｰﾄﾞ            */
	NEC_TI400A					= 0x0000000D,	/* 1Nﾌﾟﾛｸﾞﾚｯｼﾌﾞｶﾒﾗ : NEC TI-400A ﾉｰﾏﾙﾓｰﾄﾞ      */
	HITACHI_KPF2A				= 0x0000000E,	/* 1Nﾌﾟﾛｸﾞﾚｯｼﾌﾞｶﾒﾗ : HITACHI KP-F2A ﾉｰﾏﾙﾓｰﾄﾞ   */
	HITACHI_KPF2				= 0x0000000E,	/* 1Nﾌﾟﾛｸﾞﾚｯｼﾌﾞｶﾒﾗ : HITACHI KP-F2A ﾉｰﾏﾙﾓｰﾄﾞ   */
	TAKEX_FC1300				= 0x0000000F, 	/* 高精細ｶﾒﾗ：竹中 FC1300 ﾉｰﾏﾙﾓｰﾄﾞ             */
	TELI_CS3910H_BS				= 0x00000010, 	/* 高精細ｶﾒﾗ：東京電子 CS3910H 内部同期        */
	SONY_XCHR300_59M			= 0x00000011,	/* 2I高速ｶﾒﾗ : SONY XC-HR300 59M-CLK           */
	HITACHI_KPF120A				= 0x00000012,	/* 高精細ｶﾒﾗ : HITACHI KP-F120A                */
	HITACHI_KPF120				= 0x00000012,	/* 高精細ｶﾒﾗ : HITACHI KP-F120A                */
	ADIMEC_1000M_SINGLE_8BIT	= 0x00000013,	/* CameraLink : ADIMEC 1000M ｼﾝｸﾞﾙ 8bit        */
	ADIMEC_1000M_SINGLE_10BIT	= 0x00000014,	/* CameraLink : ADIMEC 1000M ｼﾝｸﾞﾙ 10bit       */
	ADIMEC_1000M_DUAL_8BIT		= 0x00000015,	/* CameraLink : ADIMEC 1000M ﾃﾞｭｱﾙ 8bit        */
	ADIMEC_1000M_DUAL_10BIT		= 0x00000016,	/* CameraLink : ADIMEC 1000M ﾃﾞｭｱﾙ 10bit       */
	SONY_XC55_TVD				= 0x00000017,	/* 1Nﾌﾟﾛｸﾞﾚｯｼﾌﾞｶﾒﾗ : SONY XC-55 ﾄﾘｶﾞ出力VS回避 */
	TI_MX1000PF					= 0x00000018,	/* 高精細ｶﾒﾗ : TI MX-1000PF ﾉｰﾏﾙﾓｰﾄﾞ           */
	CIS_VCC8350CLT				= 0x00000019,	/* CameraLink : CIS VCC8350CLT (倍速RGB)       */
	CIS_VCC8350CL				= 0x00000019,	/* CameraLink : CIS VCC8350CL  (倍速RGB)       */
	CIS_VCC8350MCL				= 0x0000001A,	/* CameraLink : CIS VCC8350MCL (倍速ﾓﾉｸﾛ)      */
	CIS_VCC850A					= 0x0000001B,	/* 高精細ｶﾒﾗ : CIS VCC850A (1024×768)         */
	IKEGAMI_SKC141P				= 0x0000001C,	/* 高精細ｶﾒﾗ : 池上通信機 SKC141P (1280×1024) */
	SONY_XCHR300_1N				= 0x0000001D,	/* 2I高速ｶﾒﾗ : SONY XC-HR300 1Nﾓｰﾄﾞ            */
	SONY_XCHR300_1N_59M			= 0x0000001E,	/* 2I高速ｶﾒﾗ : SONY XC-HR300 1Nﾓｰﾄﾞ 59M-CLK    */
	BW_FRESET_CAMERA			= 0x0000001F,	/* ﾓﾉｸﾛｶﾒﾗ : 外部同期ﾌﾚｰﾑﾘｾｯﾄﾓｰﾄﾞ              */
	HITACHI_KPF100ACL			= 0x00000020,	/* CameraLink : HITACHI_KPF100ACL (固定ｼｬｯﾀ)   */
	HITACHI_KPF100ACL_ONETRIG	= 0x00000021,	/* CameraLink : HITACHI_KPF100ACL (ONEﾄﾘｶﾞ)    */
	JAI_CV_A33					= 0x00000022,	/* CameraLink : JAI CV-A33 (640×480)          */
	SONY_XCHR50					= 0x00000023,	/* 1N高速ｶﾒﾗ : SONY XC-HR50                    */
	TELI_CS8550D				= 0x00000024,	/* 1N高速ｶﾒﾗ : TELI CS8550D                    */
	KOS_SE1004					= 0x00000025,	/* 1Nﾌﾟﾛｸﾞﾚｯｼﾌﾞｶﾒﾗ : KOS_SE1004                */
	HITACHI_KPFY1				= 0x00000026,	/* 1N倍速ｶﾒﾗ : HITACHI KP-FY1                  */
	CIS_VCCG20X30				= 0x00000027,	/* 高精細ｶﾒﾗ : CIS VCC-G20X30 (1024×768)      */
	PULNIX_TM2016_8				= 0x00000028,	/* 高精細ｶﾒﾗ : PULNIX TM-2016-8(1920×1024)    */
	PULNIX_TM2016_8CL			= 0x00000029,	/* CameraLink : PULNIX TM-2016-8CL(1920×1024) */
	HITACHI_KPF120CL			= 0x0000002A,	/* CameraLink : HITACHI KPF120CL ﾃﾞｭｱﾙ 10bit   */
	CIS_VCC8550CL				= 0x0000002B,	/* CameraLink : CIS VCC8550CL  (XGA倍速RGB)    */
	HITACHI_KPF120CL_S6			= 0x0000002C,	/* CameraLink : HITACHI KPF120CL S6ｼﾝｸﾞﾙ 10bit */
	TELI_CS8550DI				= 0x0000002D,	/* 1N高速ｶﾒﾗ : TELI CS8550Di                   */
	TELI_CS8550I01				= 0x0000002E,	/* 1Nﾌﾟﾛｸﾞﾚｯｼﾌﾞｶﾒﾗ : TELI CS8550i-01           */
	TOSHIBA_IK52V				= 0x0000002F,	/* 1N高速ｶﾒﾗ : 東芝 IK-52V                     */
	TOSHIBA_IK53V				= 0x00000030,	/* 1N高速ｶﾒﾗ : 東芝 IK-53V                     */
	CIS_VCCG20V30				= 0x00000031,	/* 1N高速ｶﾒﾗ : CIS VCC-G20V30                  */
	CIS_VCCG20S20				= 0x00000032,	/* 高精細ｶﾒﾗ : CIS VCC-G20S20 (1280×1024)     */
	USER_CAMERA_1				= 0x00000033,	/* ユーザ設定カメラ＃１                        */
	USER_CAMERA_2				= 0x00000034,	/* ユーザ設定カメラ＃２                        */
	USER_CAMERA_3				= 0x00000035,	/* ユーザ設定カメラ＃３                        */
	USER_CAMERA_4				= 0x00000036	/* ユーザ設定カメラ＃４                        */
};

enum FlashMode {
	NOFLASH,            /* フラッシュ出力なし           */
	SINGLE_PULSE,       /* シングルパルスモード         */
	LONG_PULSE,         /* ロングパルスモード           */
	DELAYED_LONG_PULSE, /* ディレイ１ロングパルスモード */
	NLONG_PULSE,        /* Nロングパルスモード          */
	TWIN_PULSE,         /* ツインパルスモード           */
	SHUTTER_PULSE       /* シャッターパルスモード       */
};

enum DispMode {
	DISP_POSI_DATA=0,      /* 正値のみ表示 */
	DISP_POSI_NEGA_DATA=3  /* 正負値表示   */
};

enum OverlapMode {
	OVERLAP_MAX=0,  /* ＭＡＸ値出力                       */
	OVERLAP_ADD=4,  /* ＡＤＤ値出力（オーバフロー対処有） */
	OVERLAP_OPT1, /* Reserved */
	OVERLAP_OPT2  /* Reserved */
};

enum DispType {
	BW_DISPLAY, /* モノクロディスプレイ */
	YUV_DISPLAY, /* カラーディスプレイ   */
	BMP_DISPLAY /* オーバレイ表示ディスプレイ */
};


enum SPACE_OPT {
	SPACE_0CLEAR,   /* 周辺を０クリアする     */
	SPACE_NOTOUCH,  /* 周辺は変化させない     */
	SPACE_128CLEAR  /* 周辺を１２８クリアする */
};

enum CombineOpt {
	COMBINE_AND,
	COMBINE_OR,
	COMBINE_XOR,
	COMBINE_InvertAND,
	COMBINE_InvertOR
};

/* 二値画像基本特徴量抽出オプション */
enum IPBOFeatureOpt {
	BINARY_REGISTER,          /* ＨＰ並列特徴量のみ */
	BINARY_CENTER_of_GRAVITY, /* +重心              */
	BINARY_MOMENT             /* +2次モーメント     */
};

/* 濃淡画像基本特徴量抽出オプション */
enum IPGOFeatureOpt {
	GRAY_REGISTER  /* 特徴量のみ抽出する */
};


/* ラベリング処理オプション */
enum IP_Label_opt {
	LABEL_OBJ, /* 白（画素１）を物体としてラベリング */
	LABEL_BKG  /* 黒（画素０）を物体としてラベリング */
};

enum IP_Label_Sort_opt {
	LABEL_SORT_MAX, /* 面積でのソートを大きい順に行う */
	LABEL_SORT_MIN, /* 面積でのソートを小さい順に行う */
	LABEL_SORT_NON  /* ソートなし */
};

/* ランレングス・ラベリング処理オプション */
enum IP_Label_opt2 {
	LABEL_NO_DST, /* ラベリング結果をデスティネーション画面に出力しない */
	LABEL_DST     /* ラベリング結果をデスティネーション画面に出力する   */
};

/* ランレングス・ラベリング */
enum IP_Label_GetType {
	LABEL_4     = 1,
	LABEL_8     = 2,
	FILTER      = 4,
	FILTER_SORT = 8
};

/* 統合ランレングス手法 */
enum IPLabelConnect {
	LABEL4, /* 4連結 */
	LABEL8  /* 8連結 */
};
enum IPCombineType {
	NORMAL_FEATURE,       /* 基本：面積，開始・終了座標 ＆ ラベル画像出力 */
	EXTRA_FEATURE,        /* 拡張：基本＋重心座標，角度，短軸の長さ÷長軸の長さ ＆ ラベル画像出力 */
	NORMAL_FEATURE_NODST, /* 基本・ラベル画像出力なし（高速化） */
	EXTRA_FEATURE_NODST   /* 拡張・ラベル画像出力なし（高速化） */
};
enum IPCombineInc { /* 包含による統合 */
	INCLUDE,   /* 包含有効 */
	NO_INCLUDE /* 包含無効 */
};

/* ゼロクロッシング手法 */
enum IPZeroCrossThetaMethod {
	OUTPUT_THETA    = 0x0, /* モデルベースト ZeroCross7x7,θテーブル出力あり */
	NO_OUTPUT_THETA = 0x1  /* モデルベースト ZeroCross7x7,θテーブル出力なし */
};


/* 正規化相関でのサーチモード */
enum IP_Corr_SearchMode {
	RASTER,               /* 全画面をラスタスキャン               */
	RASTER_STOP,          /* 同上、但し指定個数検出するとストップ */
	SPIRAL,               /* 全画面をスパイラル型にサーチ         */
	SPIRAL_STOP,          /* 同上、但し指定個数検出するとストップ */
	CORR_2ND_SEARCH4 = 0, /* 4近傍サーチ (デフォルト)             */
	CORR_2ND_SEARCH8 = 4, /* 8近傍サーチ                          */
	CORR_SKIP_SEARCH = 0, /* 飛び越しサーチ (デフォルト)          */
	CORR_FULL_SEARCH = 8, /* 全面サーチ                           */
	CORR_SORT        = 0, /* 相関値の大きい順にｿｰﾄ(ﾃﾞﾌｫﾙﾄ)        */
	CORR_NONSORT     = 16 /* ソートしない                         */
};

/* IP_CorrPreciseのオプション */
enum  IPCorrPreciseOpt{
	CRP_CENTER, /* (xc,yc)を中心座標とみなす */
	CRP_MAXVAL  /* (xc,yc)と上下左右の相関値を求めて最大となった位置を */
	            /* 中心座標とみなす                                    */
};

/* SetOptFlowModeのサーチオプション */
enum  IP_Opt_SearchMode {
	OPT_FULL_SEARCH_3  =  0, /* 上左　２画素、下右　３画素 */
	OPT_FULL_SEARCH_6  =  1, /* 上左　５画素、下右　６画素 */
	OPT_FULL_SEARCH_9  =  2, /* 上左　８画素、下右　９画素 */
	OPT_FULL_SEARCH_12 =  3, /* 上左１１画素、下右１２画素 */
	OPT_FULL_SEARCH_15 =  4, /* 上左１４画素、下右１５画素 */
	OPT_FULL_SEARCH_7  = 10, /* 上左　６画素、下右　７画素 */
	OPT_FULL_SEARCH_14 = 11, /* 上左１３画素、下右１４画素 */
	OPT_FULL_SEARCH_21 = 12, /* 上左２０画素、下右２１画素 */
	OPT_FULL_SEARCH_28 = 13, /* 上左２７画素、下右２８画素 */
	OPT_FULL_SEARCH_35 = 14  /* 上左３４画素、下右３５画素 */
};

/* SetStereoVisionModeのサーチオプション */
enum  IP_Str_SearchMode {
	STR_FULL_SEARCH_9   = 100,
	STR_FULL_SEARCH_18  = 101,
	STR_FULL_SEARCH_27  = 102,
	STR_FULL_SEARCH_36  = 103,
	STR_FULL_SEARCH_45  = 104,
	STR_FULL_SEARCH_54  = 105,
	STR_FULL_SEARCH_63  = 106,
	STR_FULL_SEARCH_72  = 107,
	STR_FULL_SEARCH_81  = 108,
	STR_FULL_SEARCH_90  = 109,
	STR_FULL_SEARCH_108 = 111,
	STR_FULL_SEARCH_126 = 113,
	STR_FULL_SEARCH_144 = 115,
	STR_FULL_SEARCH_162 = 117,
	STR_FULL_SEARCH_180 = 119,
	STR_FULL_SEARCH_198 = 121,
	STR_FULL_SEARCH_225 = 124,
	STR_FULL_SEARCH_252 = 127,
	STR_FULL_SEARCH_279 = 130,
	STR_FULL_SEARCH_306 = 133
};

/* 描画モード */
enum IPX_DRAW_MODE {
	DRAW_DIRECT,    /* 上書き                */
	DRAW_EOR,       /* 元の画素とEORして描画 */
	DRAW_DIRECT_EXT /* 描画データのみ上書き  */
};

/* 描画カラー */
enum IPX_DRAW_COLOR {
	COLOR_WHITE, /* 白 */
	COLOR_BLACK  /* 黒 */
};

/* 線種 */
enum IPX_LINE_STYLE {
	LINE_SOLID, /* 実線（デフォルト）:（注）当面デフォルト値のみサポート */
	LINE_DASH   /* 破線，鎖線 */
};

/* 文字サイズ */
enum IPX_STRING_SIZE {
	STRING_16x16, /* 標準サイズ（デフォルト） */
	STRING_32x32, /* 縦横２倍サイズ           */
	STRING_48x48, /* 縦横３倍サイズ           */
	STRING_64x64, /* 縦横４倍サイズ           */
	STRING_32x16, /* Reserved                 */
	STRING_xxxxx  /* Reserved                 */
};


/* 円弧塗りつぶし処理パラメータ */
enum IPX_FILL_MODE {
	BOW,    /* 弓形 */
	FAN     /* 扇形 */
};

/* 画像メモリからBMPﾌｧｲﾙへ転送する画像データ処理オプション */
enum BITMAP_MODE {
	BW_BITMAP,  	/* ＢＷ画像                      */
	RGB_BITMAP, 	/* ＲＧＢ画像                    */
	YUV_BITMAP, 	/* ＹＵＶ画像（専用フォーマット）*/
	BIN_BITMAP,		/* ２値画像                      */
	RGBtoBW_BITMAP	/* ＲＧＢをＢＷに変換してセーブ  */
};

enum TrigerMode {
	NOTRIGER,  /* ノーマル取り込み   */
	RR_TRIGER, /* RRトリガ取り込み   */
	TRIGER,    /* VIトリガ取り込み   */
	EXT_TRIGER /* 外部トリガ取り込み */
};

/* カラー抽出処理オプション */
enum ColorOpt {
	NORMAL_EXTRACT,        /* 抽出結果を圧縮せずに出力する */
	XCOMPRESS_EXTRACT,     /* 抽出結果をX方向へ1/2圧縮して出力する */
	XYCOMPRESS_EXTRACT     /* 抽出結果をXY方向へ1/2圧縮して出力する */
};

/* マルチカラー抽出処理オプション */
enum MultiColorOpt {
	MULTICOLOR_LABEL, /* 色別ラベル画像 */
	MULTICOLOR_BIN    /* 2値画像 */
};

/* ８段２値フィルタ処理オプション */
enum PipelineMode {
	PIPELINE0,   /* パイプライン処理０段目出力     */
	PIPELINE1,   /* パイプライン処理１段目出力     */
	PIPELINE2,   /* パイプライン処理２段目出力     */
	PIPELINE3,   /* パイプライン処理３段目出力     */
	PIPELINE4,   /* パイプライン処理４段目出力     */
	PIPELINE5,   /* パイプライン処理５段目出力     */
	PIPELINE6,   /* パイプライン処理６段目出力     */
	PIPELINE7,   /* パイプライン処理７段目出力     */
	PIPELINEAND, /* パイプライン処理４段目AND出力  */
	PIPELINEOR,  /* パイプライン処理４段目OR出力   */
	PIPELINEXOR, /* パイプライン処理４段目XOR出力  */
	PIPELINEXNOR /* パイプライン処理４段目XNOR出力 */
};

/* 形状変換パイプライン処理オプション */
enum StageFunction {
	STAGE_NOOP,   /* 0x00, 処理無し             */
	STAGE_NOOP1,  /* 0x01, 処理無し             */
	DELETENOISE4, /* 0X02, ノイズ除去４連結     */
	DELETENOISE8, /* 0X03, ノイズ除去８連結     */
	DILATION4,    /* 0X04, ４連結膨張           */
	DILATION8,    /* 0X05, ８連結膨張           */
	EROSION4,     /* 0X06, ４連結収縮           */
	EROSION8,     /* 0X07, ８連結収縮           */
	OUTLINE4,     /* 0X08, ４連結輪郭抽出       */
	OUTLINE8,     /* 0X09, ８連結輪郭抽出       */
	DILATION4X,   /* 0X0A, ４連結Ｘ方向膨張     */
	DILATION8X,   /* 0X0B, ８連結Ｘ方向膨張     */
	EROSION4X,    /* 0X0C, ４連結Ｘ方向収縮     */
	EROSION8X,    /* 0X0D, ８連結Ｘ方向収縮     */
	DILATION4Y,   /* 0X0E, ４連結Ｙ方向膨張     */
	DILATION8Y,   /* 0X0F, ８連結Ｙ方向膨張     */
	EROSION4Y,    /* 0X10, ４連結Ｙ方向収縮     */
	EROSION8Y,    /* 0X11, ８連結Ｙ方向収縮     */
	DILATION4RU,  /* 0X12, ４連結右上り方向膨張 */
	DILATION8RU,  /* 0X13, ８連結右上り方向膨張 */
	EROSION4RU,   /* 0X14, ４連結右上り方向収縮 */
	EROSION8RU,   /* 0X15, ８連結右上り方向収縮 */
	DILATION4RD,  /* 0X16, ４連結右下り方向膨張 */
	DILATION8RD,  /* 0X17, ８連結右下り方向膨張 */
	EROSION4RD,   /* 0X18, ４連結右下り方向収縮 */
	EROSION8RD    /* 0X19  ８連結右下り方向収縮 */
};

/* 2値画像マッチングフィルタ処理オプション */
enum IPBinMatchMode {
	MATCH_AND, /* AND処理  */
	MATCH_XNOR /* XNOR処理 */
};

/* 画素数カウント手法 */
enum IPCountPixelOpt {
	PIXEL_1_COUNT, /* 「1」をカウントする */
	PIXEL_0_COUNT  /* 「0」をカウントする（未サポート） */
};


/* 基本動作クロック処理オプション */
enum ClockSpeedMode {
	LOW_SPEED,    /*  62MHz */
	NORMAL_SPEED, /*  75MHz */
	HIGH_SPEED,   /*  87MHz */
	SUPER_SPEED   /* 100MHz */
};

/* 画像反転処理オプション */
enum IPMirrorOpt {
	VERTICAL_DIRECTION,   /* 垂直方向反転          */
	HORIZONTAL_DIRECTION, /* 水平方向反転          */
	REVERSE               /* 垂直+水平反転（180°）*/
};

/* エッジ抽出処理方式 */
enum ExtractEdgeMethod {
	EDGE_USER_OPERATER, /* ユーザ定義オペレータ */
	EDGE_OPERATER0,     /* オペレータ０         */
	EDGE_OPERATER1,     /* オペレータ１         */
	EDGE_OPERATER2,     /* オペレータ２         */
	EDGE_OPERATER3      /* オペレータ３         */
};

/* 画像処理出力モード（正規化処理） */
enum IPRoundMode {
	IP_ROUND,     /* 四捨五入 */
	IP_ROUNDDOWN  /* 切り捨て */
};

/* ビデオ同期入出力信号の極性 */
enum IPSignalMode {
	POSITIVE, /* ポジティブ（立ち上がり）*/
	NEGATIVE  /* ネガティブ（立ち下がり）*/
};

/* 映像入出力種別 */
enum ConfigType {
	IP5K_CAMERA,  /* IP5000画像処理ボードからの映像取り込み                             */
	VFW_CAMERA,   /* Video for Windows（キャプチャカード）からの映像取り込み            */
	IMG_FILE,     /* 映像入力：コンフィギュレーションデータで指定したファイルを取り込む */
	DISP_VGA,     /* 映像表示：画像番号の画像データをＶＧＡ画面に表示する               */
	NO_INOUTPUT,  /* 入出力処理をスルー（実行しない）状態にする（デフォルト設定）       */
	PLUGIN,
	VP810_CAMERA, /* VP810画像処理ボード（日立米沢）からの映像取り込み                  */
	AVI_FILE,     /* 映像入力：コンフィギュレーションデータで指定したＡＶＩファイルを取り込む */
	IMG_FILE_TO_IM /* 映像入力：ConfigCameraParaで指定したファイルをＩＭに取り込む      */
};

/* １，２次元コンボリューション */
enum IP2DConv {
	SIDE_NULL, /* 周辺「0」 */
	SIDE_OVER, /* 周辺ソース画像コピー */
	SIDE_CONV  /* 周辺コンボリューション */
};

/* デフォルト（リセット）ウインドウモード */
enum DefaultIPWindowMode {
 	VIDEO_FRAME_SIZE, /* ビデオ画面サイズにリセット     */
 	USER_FRAME_SIZE   /* ユーザ指定画面サイズにリセット */
};


/* SuperVchip動作モード */
enum SVOpreationMode {
	SV_XLNG8N,  /* ８の整数倍モード */
	SV_XLNGFREE /* 制限なし         */
};

/* ＳＨバスリザーブモード */
enum SVBusReserveMode {
	NOBUSRSV,      /*１バッファ間でバスリザーブは行わない      */
	SA2SBRSV,      /* ソースＡ、ソースＢ間でバスをリザーブする */
	ONDEMANDBUSRSV /* バス要求がある限りリザーブする           */
};


/* ビデオポート番号 */
enum VideoPortID {
	VIDEO_PORT0, /* ビデオポート＃０ */
	VIDEO_PORT1  /* ビデオポート＃１ */
};


/* マルチカラー抽出手法 */
enum IPMultiMethod {
	IPMultiYRT, /* Y/ρ/θ でのマルチカラー抽出                  */
	IPMultiYUV, /* Y/U/V でのマルチカラー抽出                    */
	IPMultiHSI, /* Hue/Saturation/Intensity でのマルチカラー抽出 */
	IPMultiRGB  /* Red/Green/Blue でのマルチカラー抽出           */
};


/* 拡張コンボリューションオプション */
enum IPConvExtOpt {
	H_MODE_NEGA = 32, /* 負対称（水平方向） */
	V_MODE_NEGA = 64, /* 負対称（垂直方向） */
	RD_DISABLE  =  1, /* 右下無効 */
	LD_DISABLE  =  2, /* 左下無効 */
	RU_DISABLE  =  4, /* 右上無効 */
	LU_DISABLE  =  8  /* 左上無効 */
};


enum IPZoomMag {
	ZOOM_MAG_1   = 1, /* 1/1 倍 */
	ZOOM_MAG_1_2 = 2, /* 1/2 倍 */
	ZOOM_MAG_1_3 = 3, /* 1/3 倍 */
	ZOOM_MAG_1_4 = 4, /* 1/4 倍 */
	ZOOM_MAG_1_5 = 5, /* 1/5 倍 */
	ZOOM_MAG_1_6 = 6, /* 1/6 倍 */
	ZOOM_MAG_1_7 = 7, /* 1/7 倍 */
	ZOOM_MAG_1_8 = 8, /* 1/8 倍 */
	ZOOM_MAG_2   = 2, /*   2 倍 */
	ZOOM_MAG_3   = 3, /*   3 倍 */
	ZOOM_MAG_4   = 4, /*   4 倍 */
	ZOOM_MAG_5   = 5, /*   5 倍 */
	ZOOM_MAG_6   = 6, /*   6 倍 */
	ZOOM_MAG_7   = 7, /*   7 倍 */
	ZOOM_MAG_8   = 8  /*   8 倍 */
};


/* カメラ入力（ミラー反転） */
enum GetCameraOpt {
	NONE=0,
	VP_HORIZONTAL_DIRECTION=1,
	VP_VERTICAL_DIRECTION=2,
	VP_REVERSE=3,
	VP_CONVERTLUT=4
};

#include "ipxsys.h"
#endif /* !__IPXDEF_H__ */
