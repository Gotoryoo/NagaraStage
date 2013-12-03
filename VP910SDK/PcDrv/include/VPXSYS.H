/*                                                                                        */
/*		+------------------------------------------------------------------------+        */
/*		+ Copyright (c) 2000-2004 Renesas  Northern Japan Semiconductor,Inc.     +        */
/*		+------------------------------------------------------------------------+        */
/*		Title : VP-900 Serise Struct And Union Define                                     */
/*                                                                                        */
/*                                                                                        */

#ifndef		__vpxsys_h__
#define		__vpxsys_h__

#include "ipxsys.h"
#include "vpxadm.h"

/******************************************************************************
			VP-900 画像処理ライブラリ構造体の定義
******************************************************************************/
typedef int		DEVID;

/******************************************************************************
			デバイスオープン処理
******************************************************************************/
typedef struct {
	int		cfg;			// コンフィギュレーションモード
	int		control_opt;	// ビデオコントロールオプション
	int		interlace;		// インタレースモード
	int		video_size;		// ビデオサイズ
	int		camera_port;	// カメラポート
	int		camera_type;	// カメラタイプ
	int		disp_type;		// 表示タイプ
	int		select_camera;	// カメラ選択モード
	int		disp_camera;	// スルー表示モード
	int		disp_overlap;	// オーバラップモード
} INIT_VIDEO_INFO;

typedef struct {
	int		OpenMode;		// オープンモード
	int		OpenOpt;		// オープンオプション
	int		ResetOpt;		// リセットオプション
	int		BootOpt;		// ブートオプション
	int		Reserved[4];	// リザーブ
} OPEN_IP_INFO;

/******************************************************************************
			正規化相関処理
******************************************************************************/

typedef struct{
	long			score;
	long			x;
	long			y;
} CORRTBL;

typedef struct{
	long			score;
	float			x;
	float			y;
} FCORRTBL;

typedef struct{
	short			sx;
	short			sy;
	short			ex;
	short			ey;
	short			xleng;
	short			yleng;
} RECTWINDOW;

typedef struct{
	RECTWINDOW		Win;
	short			xleng;
	short			yleng;
	short			v;
	short			ImgID;
	short			filter;
	short			opt;
	short			affsrc;
	short			afftmp;
	int				xmag;
	int				ymag;
	float			dx;
	float			dy;
	unsigned long	tn;
	unsigned long	sum;
	unsigned long	sqsum;
} CORRTEMPLATE;


/******************************************************************************
			その他
******************************************************************************/

typedef struct{
	short			x;
	short			y;
} WPOINT;

typedef struct{
	long			x;
	long			y;
} LPOINT;

typedef struct{
	float			x;
	float			y;
} FPOINT;

typedef struct{
	WPOINT			p0;
	WPOINT			p1;
	WPOINT			p2;
	WPOINT			p3;
} RECTPOINT;

typedef struct{
	FPOINT			p0;
	FPOINT			p1;
	FPOINT			p2;
	FPOINT			p3;
} FRECTPOINT;

typedef struct{
	short			lower;
	short			num;
	short			data[512];
} LINETBL;

typedef struct{
	short			num;
	short			opt;
	WPOINT			point[512];
} POINTTBL;

typedef struct{
	long			HistTbl[256];
	long			n1;
	long			n2;
	short			m1;
	short			m2;
	short			thr;
	short			ct;
} HISTANATBL;

typedef struct{
	short			num;
	short			opt;
	float			uppoint[256];
	float			dwpoint[256];
	float			ctpoint[256];
	float			width[256];
} LINECALIPERTBL;

typedef struct{
	short			sx;
	short			sy;
	short			ex;
	short			ey;
	short			leng;
	short			opt;
} LINEWINDOW;

typedef struct{
	float			cx;
	float			cy;
	float			angle;
	float			pixelx;
	float			pixely;
} CALIBTBL;

typedef struct{
	float			pixelx;
	float			pixely;
	float			angle;
	float			sinFactor;
	float			cosFactor;
	float			divFactor;
	float			divSinFactor;
	float			divCosFactor;
} CFACTOR;

typedef struct{
	float			sin;
	float			cos;
	float			Factor;
} DFACTOR;

typedef struct{
	LINEWINDOW		LineWindow;
	CFACTOR			calib;
	DFACTOR			disp;
	short			num;
	short			mode;
	FPOINT			uppoint[256];
	FPOINT			dwpoint[256];
	FPOINT			ctpoint[256];
	float			width[256];
	char			flag[256];
} CALIPERTBL;

typedef struct{
	float			row;
	float			angle;
} HOUGHLINE;

typedef struct{
	short			lower;
	short			upper;
} LINERANGE;

typedef struct{
	float			limit;
	float			value;
} LIMITTBL;

typedef struct{
	short			num;
	short			opt;
	float			min;
	float			max;
	float			avg;
} MMDATA;

typedef struct{
	short			num;
	short			opt;
	float			tbl[256];
	char			flag[256];
} PITCHTBL;

typedef struct{
	short			offset;
	short			opt;
	short			lead;
	short			byte;
unsigned char		tbl[16];
} ARRANGETBL;

typedef struct{
	unsigned long	IPCMD_Env;
	unsigned long	BOARD_Info;
	unsigned long	IPCMD_Version;
	unsigned long	IPCMD_Update;
	unsigned long	ROM_Version;
	unsigned long	ROM_Update;
	unsigned long	PCDRV_Version;
	unsigned long	PCDRV_Update;
	unsigned long	DEVDRV_Version;
	unsigned long	DEVDRV_OS;
	unsigned long	Reserve[6];
} VERSIONINFO;

typedef struct{
	long			bitsts;
	long			vector;
	long			error;
	long			func;
} VPSYSERRSTS;

typedef struct{
	long			error;
	long			func;
	long			errorNo;
	long			opt;
} IPSYSERRSTS;

typedef struct{
	long			tn;
	long			Gsum;
	long			SQGsum;
	long			Fsum;
	long			SQFsum;
	long			CRsum;
	long			TMsub;
} CORREXT;

typedef struct{
	FPOINT			uppoint;
	FPOINT			dwpoint;
	FPOINT			ctpoint;
	float			width;
	short			code;
	short			flag;
} IMGCALIPERTBL;

typedef struct{
	short			sx;
	short			sy;
	short			ex;
	short			ey;
} RECTW;

typedef struct{
	float			rr;
	float			x;
	float			y;
} FCORREXT;

typedef struct{
	short			num;
	short			opt;
	float			pos[256];
	short			pol[256];
	short			score[256];
} LINEEDGEFINDERTBL;

typedef struct{
	short			num;
	short			opt;
	FPOINT			pos[256];
	short			pol[256];
	short			score[256];
} EDGEFINDERTBL;

typedef struct {
	float		min;			/* 最大濃度平均値              */
	WPOINT		minwin;			/* 最大濃度平均値の(sx,sy)座標 */
	float		max;			/* 最小濃度平均値              */
	WPOINT		maxwin;			/* 最大濃度平均値の(sx,sy)座標 */
} MIN_MAX_VALUE;

typedef struct {
	unsigned long	tn;			/* 総画素数                    */
	unsigned long	sum;		/* 濃度総和                    */
	unsigned long	sqsum;		/* 濃度二乗総和                */
	unsigned long	mask;		/* ０レベル画素数              */
} IPSumFeatureTbl;

/******************************************************************************
			インテリジェントモジュールパラメータ
******************************************************************************/
#ifndef __vpxdrv_h__

	typedef union{
		int				idt;
		long			ldt;
		unsigned long	uldt;
		float			fdt;
		struct{
			short	Low;
			short	High;
		} wdt;
	} CNVWORD;

#endif	// !__vpxdrv_h__

typedef struct {
	unsigned long		module_type;
	unsigned long		task_id;
	unsigned long		param_tbl;
	unsigned long		pbuff_addr;
	unsigned long		pbuff_size;
	unsigned long		funcNo;
	unsigned long		count;
	unsigned long		format;
	unsigned long		reserve;
	CNVWORD				param[32];
} MODULE_PARAM_TBL;

typedef struct {
	unsigned long		task_addr;
	int					priority;
	unsigned long		pbuff_size;
	unsigned long		stack_size;
	int					param_opt;
	int					task_opt;
} CREATE_TASK_TBL;

typedef struct {
	int					intdev;
	int					intbit;
	int					evtpri;
	int					opt;
	long				reserve[12];
} INT_DEVICE_OBJ;

typedef struct {
	unsigned long	attr;
	unsigned long	count;
	unsigned long	maxsem;
} CSEM_INFO;

typedef struct {
	int				wtskid;
	unsigned long	semcnt;
} RSEM_INFO;

typedef struct {
	unsigned long	attr;
	unsigned long	flgptn;
} CFLG_INFO;

typedef struct {
	int				wtskid;
	unsigned long	flgptn;
} RFLG_INFO;

typedef struct {
	unsigned long	tskstat;
	short			tskpri;
	short			tskbpri;
	unsigned long	tstwait;
	short			wobjid;
	long			lefttmo;
	unsigned long	actcnt;
	unsigned long	wupcnt;
	unsigned long	suscnt;
	unsigned long	tskmode;
	unsigned long	tflptn;
} RTSK_INFO;

typedef struct {
	unsigned long	attr;
	int				maxpri;
} CMBX_INFO;

typedef struct {
	void			*msghead;
} MBX_MSG;

typedef struct {
	MBX_MSG			msgque;
	int				msgpri;
} MBX_MSG_PRI;

typedef struct {
	long	timeout;
	long	reserv[15];
} WAIT_SIGNAL_INFO;

typedef struct {
	long	command;
	long	waitcmd;
	long	taskid;
	long	opt;
	long	reserve[4];
} SELF_WAKEUP_TBL;

/******************************************************************************
			時刻情報の登録／読み出し
******************************************************************************/
typedef struct {
	short	year;				/* 西暦           */
	short	month;				/* 月             */
	short	day_of_week;		/* 曜日           */
	short	day;				/* 日             */
	short	hour;				/* 時(24時間)     */
	short	minute;				/* 分             */
	short	second;				/* 秒             */
	short	milliseconds;		/* mS             */
} IP_SYSTEM_TIME;

typedef struct {
	long			utime;		/* 上位16bit      */
	unsigned long	ltime;		/* 下位32bit      */
} IP_FILE_TIME;

/******************************************************************************
			フラッシュメモリファイルアクセス
******************************************************************************/
#ifndef	__flashdrv_h__

#define	C_FSZ_PATHNAME		(512)				/*  ﾊﾟｽ名ｻｲｽﾞ(文字数=byte)   */
#define	C_FSZ_FILENAME		(128)				/* ﾌｧｲﾙ名ｻｲｽﾞ(文字数=byte)   */
#define	C_FSZ_DEVNAME		(8)					/*  ﾃﾞﾊﾞｲｽ名ｻｲｽﾞ(文字数=byte)*/

#define SYS_DISK_NAME		"sys"				/* ﾌﾗｯｼｭﾒﾓﾘ ｼｽﾃﾑﾄﾞﾗｲﾌﾞ名     */

typedef		unsigned long		FM_HFILE;		/* ﾌｧｲﾙﾊﾝﾄﾞﾙ                 */
typedef		unsigned long		FM_HFIND;		/* ﾌｧｲﾙﾊﾝﾄﾞﾙ                 */

typedef struct {								/* ﾌｧｲﾙﾘｽﾄ構造体             */
	char			name[C_FSZ_FILENAME];		/*  ﾌｧｲﾙ名(128文字(NULL含む))*/
	unsigned long	size;						/*  ﾌｧｲﾙｻｲｽﾞ                 */
	unsigned long	attr;						/*  ﾌｧｲﾙ属性                 */
	IP_FILE_TIME	time;						/*  最終書き込み時間 (8B)    */
} FM_FLIST;

typedef struct {
	unsigned long	attr;						/*  ﾌｧｲﾙ属性                 */
	unsigned long	size;						/*  ﾌｧｲﾙｻｲｽﾞ                 */
	IP_FILE_TIME	time;						/*  最終書き込み時間 (8B)    */
	unsigned long	reserved[2];				/*  ﾘｻﾞｰﾌﾞ                   */
	char			name[C_FSZ_FILENAME];		/*  ﾌｧｲﾙ名(128文字(NULL含む))*/
} FM_FIND_TBL;

typedef struct {
	char			name[16];					/* ﾄﾞﾗｲﾌﾞ名                  */
	int				type;						/* ﾄﾞﾗｲﾌﾞ ﾀｲﾌﾟ               */
	unsigned long	attr;						/* ﾄﾞﾗｲﾌﾞ属性                */
	unsigned long	size;                       /* ﾄﾞﾗｲﾌﾞ容量                */
	unsigned long	sector;						/* ｾｸﾀ数                     */
} FM_DRIVE_TYPE;

#endif /* !__flashdrv_h__ */


/******************************************************************************
			映像入出力制御
******************************************************************************/
typedef struct {
	int		port;	/* ビデオ入力部濃度変換制御 　　*/
	int		bank;	/* 濃度変換テーブルのバンク指定 */
}VIDEO_OPT_LUTCTL;

typedef struct {
	int		control;/* 制御 0:auto / 1:manual   　　*/
	float	gain;	/* ゲイン 1.0 ～5.0             */
}VIDEO_OPT_CAMERAGAIN;

typedef struct{
	int 	srcNo; 	/* カメラデータコピー元カメラ番号	*/
	int 	dstNo;  /* カメラデータコピー先カメラ番号	*/
}VIDEO_OPT_CAMCPYTBL;

/******************************************************************************
			線分化
******************************************************************************/
typedef struct {
	int		num;
	WPOINT* pos;
} POLY_TBL;

typedef struct {
	int LEFT_UP_POINT_X;		// 物体が最初に出現する点のＸ座標
	int LEFT_UP_POINT_Y;		// 物体が最初に出現する点のＹ座標
	int REGION_X0;				// 物体が存在する最小Ｘ座標
	int REGION_Y0;				// 物体が存在する最小Ｙ座標
	int REGION_X1;				// 物体が存在する最大Ｘ座標
	int REGION_Y1;				// 物体が存在する最大Ｙ座標
	double PERIMETER;			// 周囲長
	double AREA;				// 面積
	double CENTER_of_GRAVITY_X;	// 重心Ｘ座標
	double CENTER_of_GRAVITY_Y;	// 重心Ｙ座標
	double ANGLE;				// 慣性主軸とＸ軸とのなす角度（°)
	double MOMENT_SIGMA_X;		// Ｘ軸への１次モーメント
	double MOMENT_SIGMA_Y;		// Ｙ軸への１次モーメント
	double MOMENT_SIGMA_XX;		// Ｘ軸への２次モーメント
	double MOMENT_SIGMA_XY;		// 共役モーメント
	double MOMENT_SIGMA_YY;		// Ｙ軸への２次モーメント
	float AXIS_LENG;			// 慣性主軸の軸長
	float AXIS_WIDTH;			// 慣性主軸の軸長を挟む２本の直線間の距離
	int AXIS_LENG_X0;			// 慣性主軸の軸長を示す対角線X座標
	int AXIS_LENG_Y0;			// 慣性主軸の軸長を示す対角線Y座標
	int AXIS_LENG_X1;			// 慣性主軸の軸長を示す対角線X座標
	int AXIS_LENG_Y1;			// 慣性主軸の軸長を示す対角線Y座標
	float MAX_LENG;				// 絶対最大長
	float WIDTH;				// 絶対最大長を挟む２本の直線間の距離
	float ORIENTATION;			// 絶対最大長の方向と水平線との角度
	int MAX_LENG_X0;			// 絶対最大長を示す対角線X座標
	int MAX_LENG_Y0;			// 絶対最大長を示す対角線Y座標
	int MAX_LENG_X1;			// 絶対最大長を示す対角線X座標
	int MAX_LENG_Y1;			// 絶対最大長を示す対角線Y座標
} POLY_FEATURE;

/******************************************************************************
			RGBLUT
******************************************************************************/
typedef int HRGBLUT;

typedef struct {
	int		mode;
	int		opt;
	int		reserved;
	int		size;
} RGBLUT_HEADER;

typedef struct {
	RGBLUT_HEADER	header;
	unsigned char	lut[4];
} RGBLUT_8BIT,*PRGBLUT_8BIT;

/******************************************************************************
			FILL HOLE
******************************************************************************/
typedef struct {
	int		min;
	int		max;
} OBJ_AREA_OPT;

/******************************************************************************
			特徴量抽出
******************************************************************************/
typedef struct {
	int MIN_LEVEL_X;	// 最小濃度値をとる点のＸ座標
	int MIN_LEVEL_Y;	// 最小濃度値をとる点のＹ座標
	int MAX_LEVEL_X;	// 最大濃度値をとる点のＸ座標
	int MAX_LEVEL_Y;	// 最大濃度値をとる点のＹ座標
	long ACC_LEVEL;		// 濃度累積
	int MIN_LEVEL;		// 最小濃度値
	int MAX_LEVEL;		// 最大濃度値
	int TYPICAL_LEVEL;	// 予備
	long N;				// 総画素数
	float MEAN;			// 平均濃度値
	float VARIANCE;		// 分散値
	float DEVIATION;	// 標準偏差
} HISTOGRAM_FEATURE;

#endif /* !__vpxsys_h__ */

