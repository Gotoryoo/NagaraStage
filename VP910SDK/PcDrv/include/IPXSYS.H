#ifndef __IPXSYS_H__
#define __IPXSYS_H__
#include "ipxdef.h"
/******************************************************************************
		VPSeries Header file
	Copyright (C) 2000-2004 Renesas  Northern Japan Semiconductor,Inc.
	  All rights reserved
******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif /* __cplusplus */

/******************************************************************************
			画像処理ライブラリ構造体の定義
******************************************************************************/
typedef int  IMGID;
typedef int CNVLUT;

/* エラー情報読み出しテーブル */
typedef struct {
	int                     ErrorCode; /* エラー番号       */
	char ErrorRoutin[MAX_IP_FUNCNAME]; /* エラー発生関数名 */
	int                     ErrorINFO; /* エラー詳細情報   */
} IPErrorTbl;

/******************************************************************************
			ヒストグラム処理
******************************************************************************/
typedef struct {
	int       LEFT_UP_POINT_X; /* HPX0      */
	int       LEFT_UP_POINT_Y; /* HPY0      */
	int    RIGHT_DOWN_POINT_X; /* HPX1      */
	int    RIGHT_DOWN_POINT_Y; /* HPY1      */
	long                 AREA; /* HPACC     */
	int             REGION_X0; /* HPMIN     */
	int             REGION_Y0; /* HPY0      */
	int             REGION_X1; /* HPMAX     */
	int             REGION_Y1; /* HPY1      */
	float CENTER_of_GRAVITY_X; /* 重心X座標 */
	float CENTER_of_GRAVITY_Y; /* 重心Y座標 */
	float               ANGLE; /* 傾き(時計周り[rad])*/
	float      MOMENT_SIGMA_X;
	float      MOMENT_SIGMA_Y;
	float     MOMENT_SIGMA_XX;
	float     MOMENT_SIGMA_XY;
	float     MOMENT_SIGMA_YY;
} IPBOFeatureTbl;

typedef struct {
	int           MIN_LEVEL_X; /* HPX0  */
	int           MIN_LEVEL_Y; /* HPY0  */
	int           MAX_LEVEL_X; /* HPX1  */
	int           MAX_LEVEL_Y; /* HPY1  */
	long            ACC_LEVEL; /* HPACC */
	int             MIN_LEVEL; /* HPMIN */
	int             MAX_LEVEL; /* HPMAX */
	int         TYPICAL_LEVEL;
} IPGOFeatureTbl;

typedef struct {
	float                   X; /* 重心Ｘ座標 */
	float                   Y; /* 重心Ｙ座標 */
	long                 AREA; /* ラベル毎の面積 */
} IPLOGravityTbl;

typedef struct {
	int              Divide_X; /* 領域毎抽出X方向分割数 */
	int              Divide_Y; /* 領域毎抽出Y方向分割数 */
} IPDivideTbl;

typedef struct {
	int              MIN_LEVEL; /* 領域毎、ラベル毎最小濃度値 */
	int              MAX_LEVEL; /* 領域毎、ラベル毎最大濃度値 */
} IPGOMinMaxTbl;


/******************************************************************************
			正規化相関処理
******************************************************************************/
typedef struct {
	float   VALUE; /* 正規化相関値            */
	int         X; /* Ｘ座標 ( WINDOW相対値 ) */
	int         Y; /* Ｙ座標 ( WINDOW相対値 ) */
} IPCorrTbl;

typedef struct{
	float       X; /* X座標：画面相対アドレス */
	float       Y; /* Y座標：画面相対アドレス */
} IPCorrPreciseTbl;

typedef struct{
	int      x, y; /* 処理ポイント */
	float   value; /* 演算結果( r*r がセットされる) */
} CorrSearchTbl;

typedef struct {
	int         x; /* サーチポイント（画面相対アドレス） */
	int         y; /* サーチポイント（画面相対アドレス） */
	float   value; /* 相関値（実行結果）                 */
	long     fsum; /* 画像の濃度総和（実行結果）         */
	long   fsqsum; /* 画像の濃度２乗総和（実行結果）     */
	long  ftcrsum; /* 画像とテンプレートとの乗算総和（実行結果）*/
	long   ftdsub; /* 画像とテンプレートとの濃度差分の絶対値総和（実行結果）*/
} CorrSearchTblExt;

typedef struct {
	int SignOperation;                /* ０：相関値を絶対値で処理（デフォルト） */
	                                  /* １：相関値を符号付きで処理 */
	                                  /* ２：相関値は絶対値で処理するが、戻り値には符号を付ける */
	int ReturnValueOfSetCorrTemplate; /* ０：正常終了＝０、エラー＝－１（デフォルト） */
	                                  /* １：テンプレートの分散値を返す、エラー時 －１ */
} IPCorrControl;  /*正規化相関制御情報設定用構造体*/

typedef struct {
	float FloatingCoeffRange; /* 変動係数打ち切り幅。ﾃﾞﾌｫﾙﾄ：1/1.5 ～ x1.5 */
	float ProjectCorrThr;    /* 濃度累積投影の相関値による打ち切りしきい値補正。ﾃﾞﾌｫﾙﾄ： 0.9 */
	float FinalSearchRatio;   /* 全サーチポイント数に対する２次サーチ候補数比率指定。ﾃﾞﾌｫﾙﾄ：0.01 */
	int   FinalSearchMax;     /* ２次サーチ候補数上限。ﾃﾞﾌｫﾙﾄ：1000 */
	int   FinalSearchMin;     /* ２次サーチ候補数下限。ﾃﾞﾌｫﾙﾄ：4 */
	int   SkipArea;           /* １次サーチでのスキップエリア指定。 */
} IPCorrControlExt;  /*正規化相関拡張制御情報設定用構造体*/


/*****************************************************************************
			オプティカルフロー
*****************************************************************************/
typedef struct {
	int       X; /* 領域Ｙ成分 */
	int       Y; /* 領域Ｙ成分 */
} IPFlowSize;

typedef struct {
	int         X; /* 第ｎ番目の計測位置のＸ座標：画面相対アドレス  */
	int         Y; /* 第ｎ番目の計測位置のＹ座標：画面相対アドレス  */
	int    flow_x; /* 第ｎ番目での計測フローベクトル（Ｘ成分）      */
	int    flow_y; /* 第ｎ番目での計測フローベクトル（Ｙ成分）      */
	int   value; /* 第ｎ番目で計測された差分累積値                */
	int c_value; /* 第ｎ番目の中心での差分累積値                  */
	int     eva; /* 第ｎ番目の計測評価値（分散値＊ﾃﾝﾌﾟﾚｰﾄ画素数） */
} IPFlowPoint;


/******************************************************************************
			グラフィック処理
******************************************************************************/
typedef struct {
	int sx, sy; /* 線分の始点座標（システムウインドウ相対）*/
	int ex, ey; /* 線分の終点座標（システムウインドウ相対）*/
} IPX_SEGMENT;

typedef struct {
	int  x, y; /* 線分の始点及び終点座標（システムウインドウ相対）*/
} IPX_POINT;

/******************************************************************************
			画像ファイリング処理
******************************************************************************/
typedef struct {
	         long  int  width; /* 画像データのＸ方向の長さ      */
	         long  int height; /* 画像データのＹ方向の長さ      */
	unsigned short int  color; /* 画像データ                    */
	                           /* 0 : ＢＷ画像                      */
	                           /* 1 : ＲＧＢ画像                    */
	                           /* 2 : ＹＵＶ画像（専用フォーマット）*/
	                           /* 3 : ２値画像                      */
} IPBMPInfoTbl;


/******************************************************************************
			直線検出処理管理テーブル
******************************************************************************/
typedef struct {
	int  sx, sy; /* 検出エリアの中心軸始点座標（画面相対） */
	int  ex, ey; /* 検出エリアの中心軸終点座標（画面相対） */
} PredictLine;

typedef struct {
	int  sx, sy; /* 検出した直線の始点座標（画面相対） */
	int  ex, ey; /* 検出した直線の終点座標（画面相対） */
} DetectLine;

typedef struct {
	short int  vote; /* 投票数    */
	short int theta; /* 角度（°）*/
	float       rho; /* 距離      */
} HOUGH_TBL;

enum DetectEdgeOPT {
	DETECT_EDGE_FULL    /* しきい値を越えた全てのエッジを採用する */
};
enum DetectLineOPT {
	DETECT_LINE_HOUGH,  /* ハフ変換によりもとめたラインを検出結果とする  */
	DETECT_LINE_PRECISE /* さらに高制度な直線検出を行う,当面サポートせず */
};

typedef struct {
	int                 Awide; /* 検出エリアの幅（片側）            */
	int                 point; /* サンプリング点数（２～１００）    */
	int                 Lwide; /* サンプリングのライン幅（１～１０）、当面１固定 */
	int               EdgeThr; /* エッジ検出時の微分値しきい値      */
	enum DetectEdgeOPT  DEopt; /* エッジ検出オプション              */
	float           DetectThr; /* 直線検出しきい値、サンプリング点数の何％を含めば */
	                           /* 直線と見なすかを指定する                         */
	enum DetectLineOPT  DLopt; /* 直線検出オプション                */
	HOUGH_TBL          *HgTbl;
	int            HgTbl_size;
} DetectLineControl;


/******************************************************************************
			ランレングスラベリング
******************************************************************************/
typedef struct {
	long                  AREA; /* 面積      */
	short      LEFT_UP_POINT_X; /* 最小X座標 */
	short      LEFT_UP_POINT_Y; /* 最小Y座標 */
	short   RIGHT_DOWN_POINT_X; /* 最大X座標 */
	short   RIGHT_DOWN_POINT_Y; /* 最大Y座標 */
	float  CENTER_of_GRAVITY_X; /* 重心X座標 */
	float  CENTER_of_GRAVITY_Y; /* 重心Y座標 */
} IPLabelBasicTbl;

typedef struct {
	long                  AREA; /* 面積                */
	short      LEFT_UP_POINT_X; /* 最小X座標           */
	short      LEFT_UP_POINT_Y; /* 最小Y座標           */
	short   RIGHT_DOWN_POINT_X; /* 最大X座標           */
	short   RIGHT_DOWN_POINT_Y; /* 最大Y座標           */
	float  CENTER_of_GRAVITY_X; /* 重心X座標           */
	float  CENTER_of_GRAVITY_Y; /* 重心Y座標           */
	float                ANGLE; /* 傾き(時計周り[rad]) */
	float            AXIS_LONG; /* 長軸の長さ          */
	float           AXIS_SHORT; /* 短軸の長さ          */
	long          Round_Length; /* 輪郭長              */
} IPLabelExtTbl;


/******************************************************************************
			統合ラベリング
******************************************************************************/
/* ２値化管理テーブル */
typedef struct {
	int   thrmin; /* 最小しきい値（  0    ～ thrmax） */
	int   thrmax; /* 最大しきい値（thrmin ～  255  ） */
} IPBinarizeThr;

/* ラベリング管理テーブル */
typedef struct {
	int                     max_num; /* 出力最大数（ 1 ～ 4000） */
	enum IPLabelConnect     connect; /* 連結手法 */
	int                    area_min; /* 最小面積しきい値（   1   ～ thrmax ） */
	int                    area_max; /* 最大面積しきい値（thrmin ～ INT_MAX） */
	enum IP_Label_Sort_opt     sort; /* ソート手法 */
} IPLabelCtl;

/* 統合管理テーブル */
typedef struct {
	int                   width; /* 統合幅範囲    （0 ～ 横ｳｨﾝﾄﾞｳANDサイズ）*/
	                             /*                 = 0 : 統合なし         */
	int                  height; /* 統合高さ範囲  （0 ～ 縦ｳｨﾝﾄﾞｳANDサイズ）*/
	enum IPCombineInc including; /* 包含による統合                         */
} IPCombineCtl;

/* 基本特徴量テーブル */
typedef struct {
	int          area; /* 物体面積 */
	int          xmin; /* 物体を囲む最小X座標 */
	int          ymin; /* 物体を囲む最小Y座標 */
	int          xmax; /* 物体を囲む最大X座標 */
	int          ymax; /* 物体を囲む最大Y座標 */
	int        length; /* 物体周囲長 */
} IPCombineTbl;

/* 拡張特徴量テーブル */
typedef struct {
	float       xmean; /* 重心x座標 */
	float       ymean; /* 重心y座標 */
	float        tilt; /* 物体傾き（ -90°～ 90°） */
	float   axs_ratio; /* 短軸の長さ÷長軸の長さ */
} IPCombineTblExt;


/* 画像メモリ管理テーブル */
typedef struct {
	int               ch; /* 画像メモリチャネル番号                       */
	int               sx; /* 画面開始Ｘ座標（物理アドレス）               */
	int               sy; /* 画面開始Ｙ座標（物理アドレス）               */
	int             size; /* 画面サイズ番号                               */
	int             xlng; /* 画面のＸ方向の長さ                           */
	int             ylng; /* 画面のＹ方向の長さ                           */
	int             dtyp; /* 画面のデータタイプ                           */
	int             lock; /* 画面ロック制御フラグ                         */
	int             open; /* オープン制御フラグ                           */
	int            dirty; /* 画面更新制御フラグ                           */
	int            color; /* 画面属性管理情報                             */
	int          arrange; /* 画面配置管理情報                             */
	int          next_id; /* システム管理（ＵＶ）画面番号                 */
	char            *top; /* 画像メモリ先頭アドレス                       */
	int            cache; /* キャッシュ制御フラグ                         */
	int          work_id; /* システム管理（ワーク）画面番号               */
	int           change; /* 画像メモリ管理テーブル情報入れ替え制御フラグ */
	int             type; /* 画像メモリ領域管理フラグ                     */
	char           *vtop; /* 画像メモリ先頭アドレス（仮想アドレス）       */
	int            width; /* 画像処理領域のＸ方向の長さ（SOFTIP互換）     */
	int           height; /* 画像処理領域のＹ方向の長さ（SOFTIP互換）     */
	unsigned long   busy; /* 画面に対する映像入力・処理ビジーフラグ       */
} IMGTBL;

/* 画像メモリ画面サイズ */
typedef struct _IMG_FRAME_SIZE {
	int            xsize; /* Ｘ方向の画面サイズ */
	int            ysize; /* Ｙ方向の画面サイズ */
} IMG_FRAME_SIZE;

/* 画像処理ボード情報管理テーブル */
typedef struct {
	unsigned long      BusNo;
	unsigned long     SlotNo;
	unsigned long  BaseAddr0;
	unsigned long  BaseAddr1;
	unsigned long   VendorID;
	unsigned long   DeviceID;
	unsigned long RevisionID;
	unsigned long   DrvRevNo;
	unsigned long BoardCount;
} IPBoardTable;


/******************************************************************************
			２値画像マッチングフィルタ制御テーブル
******************************************************************************/
/* xyテンプレート位置テーブル */
typedef struct {
	short       x; /* カーネル中心からのx方向のずれ */
	short       y; /* カーネル中心からのy方向のずれ */
} IPCountPixelTbl;


/* ８段２値パイプライン管理テーブル */
typedef struct {
	int    Stage0;
	int    Stage1;
	int    Stage2;
	int    Stage3;
	int    Stage4;
	int    Stage5;
	int    Stage6;
	int    Stage7;
} PipelineStageMode;

/* ８段２値パイプライン管理テーブル */
struct IPTrsPipeInfo {
	int               pmode;
	PipelineStageMode fmode;
};


/******************************************************************************
			映像入出力構成制御テーブル
******************************************************************************/
typedef struct _IMG_FILE_CONFIG { /* 画像ファイル情報テーブル */
	char                    *dir; /* ファイル格納先ディレクトリ                        */
	char               *filename; /* 画像ファイル名称                                  */
	int                 start_no; /* 開始ファイル番号(0～9999)                         */
	                              /* 開始ファイル番号(４文字)＋拡張子(.bmp)自動付加    */
	                              /* 例）filename=aabbcc, start_no=9                   */
	                              /* １回目のGetCamera : aabbcc0009.bmp                */
	                              /* ２回目のGetCamera : aabbcc0010.bmp                */
	int                loop_mode; /* 画像ファイルが見つからないときの制御              */
	                              /* 0 : １回目のGetCameraで指定した画像ファイルに戻る */
	                              /* 1 : エラーメッセージ出力                          */
	int                alloc_num; /* 画像メモリ確保数(0～255)                          */
	enum ImageFrameType img_type; /* 画像メモリ画面属性                                */
	enum ImageFrameSize img_size; /* 画像メモリ画面領域                                */
} IMG_FILE_CONFIG;

typedef struct _VFW_CONFIG {     /* Video for Windows情報テーブル            */
	enum CameraType  cameratype; /* カメラ種別                               */
	int                    xlng; /* 映像入力画面のＸ方向の長さ（ 1 ～ 640 ） */
	int                    ylng; /* 映像入力画面のＹ方向の長さ（ 1 ～ 480 ） */
} VFW_CONFIG;

typedef struct _ConfigCameraPara { /* 映像入力構成テーブル          */
	IMG_FILE_CONFIG     ImgConfig; /* 画像ファイル情報テーブル      */
	VFW_CONFIG          VfwConfig; /* Video for Windows情報テーブル */
} ConfigCameraPara, *PConfigCameraPara;


typedef struct _RIMG_FILE_CONFIG { /* 画像ファイル情報テーブル（読み出し）      */
	char            dir[MAX_PATH]; /* ファイル格納先ディレクトリ                */
	char       filename[MAX_PATH]; /* 画像ファイル名称                          */
	int                  start_no; /* 開始ファイル番号(0～9999)                 */
	int              getcamera_no; /* GetCamera()実行時に読み込んだファイル番号 */
	                               /* GetCamera()未実行時は"-1"を返す           */
	int                 loop_mode; /* 画像ファイルが見つからないときの制御      */
} RIMG_FILE_CONFIG;

typedef struct _ReadConfigCameraPara { /* 映像入力構成制御テーブル（読み出し）*/
	RIMG_FILE_CONFIG        ImgConfig; /* 画像ファイル情報テーブル            */
	VFW_CONFIG              VfwConfig; /* Video for Windows情報テーブル       */
} ReadConfigCameraPara, *PReadConfigCameraPara;


typedef struct _ConfigDispPara { /* 映像出力構成テーブル（設定＆読み出し）*/
	enum DispType      disptype; /* 表示種別                              */
	int                    xlng; /* 映像出力画面のＸ方向の長さ            */
	int                    ylng; /* 映像出力画面のＹ方向の長さ            */
} ConfigDispPara, *PConfigDispPara, ReadConfigDispPara, *PReadConfigDispara;

typedef struct _IPViewWindow { /* 表示プロセスウインドウ情報テーブル */
	int                  flag; /* ウインドウ設定状態管理フラグ       */
	int                    sx; /* ウインドウ始点Ｘ座標               */
	int                    sy; /* ウインドウ始点Ｙ座標               */
	int                    ex; /* ウインドウ終点Ｘ座標               */
	int                    ey; /* ウインドウ終点Ｙ座標               */
} IPViewWindow;


/******************************************************************************
			プロファイル制御テーブル
******************************************************************************/
typedef struct {
	char name[MAX_IP_FUNCNAME]; /* コマンド名                 */
	int                    num; /* コマンド（name）の実行回数 */
	int                   area; /* 画像処理領域の総和         */
} IPProfileTbl[MAX_PROFILE], *PIPProfileTbl;


/******************************************************************************
			ランレングス・ラベリング制御テーブル
******************************************************************************/
typedef struct {
	int       enable; /* 制御テーブル有効。ﾃﾞﾌｫﾙﾄ：0 */
	int  preLabelNum; /* 仮ラベル数。ﾃﾞﾌｫﾙﾄ：511 */
	int realLabelNum; /* 真ラベル数。ﾃﾞﾌｫﾙﾄ：255 */
} IPRunLengthControl;  /*正規化相関拡張制御情報設定用構造体*/


#ifdef __cplusplus
}
#endif /* __cplusplus */


#ifdef STD_LIB
#include "ipxmacro.h"
#ifndef __IPSOFT_H__
#include "ipxprot.h"
#endif /* !__IPSOFT_H__ */
#else  /* STD_LIB */
#include "ipxmac.h"
#endif /* STD_LIB */

#endif /* !__IPXSYS_H__ */
