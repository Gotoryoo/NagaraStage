/*                                                                                   */
/*    +------------------------------------------------------------------------+     */
/*    + Copyright (c) 2000-2004 Renesas  Northern Japan Semiconductor,Inc.     +     */
/*    +------------------------------------------------------------------------+     */
/*    Title : VP-900 Serise Constant Define                                          */
/*                                                                                   */
/*                                                                                   */


#ifndef		__vpxdef_h__
#define		__vpxdef_h__

#include "ipxdef.h"

/* ボード番号 */
enum IPBoardNo {
	IPBOARD_0,						/* ボード番号 0 */
	IPBOARD_1,						/* ボード番号 1 */
	IPBOARD_2,						/* ボード番号 2 */
	IPBOARD_3,						/* ボード番号 3 */
	IPBOARD_4,						/* ボード番号 4 */
	IPBOARD_5,						/* ボード番号 5 */
	IPBOARD_6,						/* ボード番号 6 */
	IPBOARD_7						/* ボード番号 7 */
};
/* コマンドファイル名 */
#define		IPCOMMAND				"ipcmd.sys"
/* ファイルオプション指定 */
enum IPFileOption {
	DIRECT_PATH		= 0x00000000,	/* ファイルパス直接指定 */
	REGISTRY_PATH	= 0x00000010	/* ファイルパスレジストリ指定 */
};
/* ＰＩＯ割込みモジュールＩＤ */
enum IPTaskID {
	PIO_INT0_TASK		= 0x00010000,	/* PIO-bit0 割込み起動タスク */
	PIO_INT1_TASK		= 0x00010001,	/* PIO-bit1 割込み起動タスク */
	PIO_INT2_TASK		= 0x00010002,	/* PIO-bit2 割込み起動タスク */
	PIO_INT3_TASK		= 0x00010003,	/* PIO-bit3 割込み起動タスク */
	PIO_INT4_TASK		= 0x00010004,	/* PIO-bit4 割込み起動タスク */
	PIO_INT5_TASK		= 0x00010005,	/* PIO-bit5 割込み起動タスク */
	PIO_INT6_TASK		= 0x00010006,	/* PIO-bit6 割込み起動タスク */
	PIO_INT7_TASK		= 0x00010007	/* PIO-bit7 割込み起動タスク */
};
/* IPデバイスオープンモード */
#define	OPEN_VREGLOCK_ENABLE		0x00000001	/* Vchipレジスタロック許可    */
#define	OPEN_SHCACHE_DISABLE		0x00000002	/* SHキャッシュフラッシュ禁止 */
#define	OPEN_RESET_POWERON			0x00000004	/* パワーオンリセットモード   */
#define	OPEN_INT_SIG_ENABLE			0x00000008	/* 割込シグナル許可           */
#define	OPEN_ISIGLOCK_ENABLE		0x00000010	/* 割込シグナルレジスタロック */
#define	OPEN_ASYNC_EXECUTION		0x00000019	/* PC-SH 非同期動作モード     */
/* 自タスクタスクＩＤ */
#define	TASK_SELF					0
/* ヌルコード                          */
#define	ISPX_NULL					0
/* 初期化環境                          */
#define	IP5K_CONFIG					0
#define	VP810_CONFIG				1
#define	VP910_CONFIG				2
#define	USER_VIDEO_CONFIG			3
/* 初期化モード                        */
#define	DISPCAMERA_DISABLE			1
#define ALLOCDISPIMG_DISABLE		2
#define DISPOVERLAP_DISABLE			4
#define CAMERA_DISABLE				8
/* 初期化オプション                    */
#define INIT_ONLY					0
#define SELF_TEST					1

/* ビデオ入力オプションモード          */
#define	VIDEO_OPT_VP0				0			/* ビデオポート＃０       */
#define	VIDEO_OPT_VP1				1			/* ビデオポート＃１       */
#define	VIDEO_OPT_DP				2			/* DPモード               */
#define	VIDEO_OPT_CAMERA			3			/* カメラデータ登録       */
#define	VIDEO_OPT_CAMCPY			4			/* カメラデータコピー     */
#define	VIDEO_OPT_CAMNUM			4			/* カメラ数参照           */
/* ビデオ入力ＬＵＴバンク              */
#define	VIDEO_LUT_BANK0				0			/* バンク＃０             */
#define	VIDEO_LUT_BANK1				1			/* バンク＃１             */
/* ビデオ入力オプション                */
#define	VIDEO_LUT_CTL				0			/* 濃度変換モード         */
#define	MIRROR_CTL					1			/* ミラー反転入力         */
#define	VIDEO_SYNC_CTL				2			/* ビデオ同期選択         */
#define	VIDEO_CLKPOLE_CTL			3			/* クロック極性選択       */
#define POWER_SAVE					0x100		/* パワーセーブモード     */
#define SYNC_SEL					0x101		/* 同期信号選択           */
#define	CAMERA_GAIN_CTL				0x102		/* カメラゲイン制御       */
#define	CLK_STOP_CTL				0x103		/* クロック供給制御       */
#define	CAMERA_PORT_CTL				0x104		/* カメラポート制御       */
/* ミラー反転オプション(opt1)          */
#define	NO_MIRROR					0			/* ミラー反転無し         */
#define	H_MIRROR					1			/* 水平方向ミラー反転     */
#define	V_MIRROR					2			/* 垂直方向ミラー反転     */
#define	HV_MIRROR					3			/* 水平垂直方向ミラー反転 */
/* DPオプション                       */
#define	DP_SLAVE_CTL				0			/* ＤＰスレーブモード */
/* ビデオ同期選択オプション           */
#define AUTO_SYNC_SEL				0			/* 自律同期選択           */
#define CAMERA_SYNC_SEL				1			/* カメラ選択             */
/* ビデクロック極性選択オプション     */
#define CLKPOLE_POSITIVE			0			/* 正極性                 */
#define CLKPOLE_NEGATIVE			1			/* 負極性(反転)           */
/* カメラデータコピー                */
#define	CAMERA_COPY_CTL				0			/* カメラコピーモード     */
/* カメラ数参照                      */
#define	CAMERA_NUM_CTL				0			/* カメラ数モード         */

/* システム制御オプションモード       */
#define SYSTEM_OPT_RUNSTS			0			/* システム動作状態       */
/* システムRUN-LED制御                */
#define SYSTEM_LED_BLINK			0			/* RUN-LED点滅制御        */
#define SYSTEM_LED_CTL				1			/* RUN-LED状態変更        */
/* システムRUN-LEDコントロール        */
#define RUNLED_BLINK_STOP			0			/* RUN-LED点滅制御停止    */
#define RUNLED_BLINK_START			1			/* RUN-LED点滅制御開始    */
#define RUNLED_OFF					0			/* RUN-LED消灯            */
#define RUNLED_ON					1			/* RUN-LED点灯            */

/* カメラの同期タイプ */
enum CameraSync {
	EXTERNAL_SYNC,					/* 外部同期       */
	CAMERA_SYNC						/* カメラ内部同期 */
};

/* カメラポートのコンフィギュレーションタイプ */
enum CameraPortConfig {
	PORTCFG_NORMAL,					/* ノーマルモード                    */
	PORTCFG_CROSS,					/* P0(VP#1)/P1(VP#2) クロス          */
	PORTCFG_YUV_ONLY,				/* YUVｶﾒﾗ VP#2-4CH                   */
	PORTCFG_BOTH_BW_YUV,			/* VP#1-BWｶﾒﾗ VP#2-YUVｶﾒﾗ            */
	PORTCFG_BOTH_P0BW_P1YUV,		/* P0(VP#1)-BWｶﾒﾗ P1(VP#2)-YUVｶﾒﾗ    */
	PORTCFG_BOTH_P0YUV_P1BW,		/* P1(VP#1)-BWｶﾒﾗ P0(VP#2)-YUVｶﾒﾗ    */
	PORTCFG_RGB,					/* RGBｶﾒﾗ 1CH                        */
	PORTCFG_4CH,					/* BWｶﾒﾗ 4CH同時入力                 */
	PORTCFG_ACTIVE_VIDEO_PORT		/* ActiveVideoPort 選択              */
};

#define	BITMAP_OFF			0		/* ﾋﾞｯﾄﾏｯﾌﾟｵｰﾊﾞｰﾗｯﾌﾟ表示しない   */
#define	BITMAP_ON			1		/* ﾋﾞｯﾄﾏｯﾌﾟｵｰﾊﾞｰﾗｯﾌﾟ表示する     */

/* ISP_BusyWaitのマスクパターンデータ */
#define		NOBUSY					0
#define		BSY_BUSY				0x00003313	/* HP | DMA | VP | IP | LPB | IM | AP */
#define		AP_BUSY					0x00000000	/* アドレスプロセッサ                 */
#define		IM_BUSY					0x00000000	/* IMアクセス                         */
#define		LPB_BUSY				0x00000000	/* ループバック                       */
#define		DP_BUSY					0x00030000	/* 映像表示( DPCMD -> EX )            */
#define		DPST2_BUSY				0x00020000	/* 映像表示( DPST2 -> EX )            */
#define		DPST1_BUSY				0x00010000	/* 映像表示( DPST1 -> EX )            */
#define		VP_BUSY					0x00003300	/* 映像入力( VPCMD -> EX )            */
#define		VP2_BUSY				0x00003000	/* 映像入力2( VP2CMD -> EX )          */
#define		VP2ST2_BUSY				0x00002000	/* 映像入力2( VP2ST2 -> EX )          */
#define		VP2ST1_BUSY				0x00001000	/* 映像入力2( VP2ST1 -> EX )          */
#define		VP1_BUSY				0x00000300	/* 映像入力1( VP1CMD -> EX )          */
#define		VP1ST2_BUSY				0x00000200	/* 映像入力1( VP1ST2 -> EX )          */
#define		VP1ST1_BUSY				0x00000100	/* 映像入力1( VP1ST1 -> EX )          */
#define		DMA_BUSY				0x00000010	/* ＤＭＡ処理                         */
#define		IP_BUSY					0x00000001	/* 画像処理( APCMD -> EX )            */
#define		GBUS_BUSY				0x00000000	/* GBUS                               */

/* キャリパオプション */
enum IPCaliperMode {
	CALIPER_BRIGHT,					/* 明るいオブジェクト */
	CALIPER_DARK					/* 暗いオブジェクト   */
};
/* リード検査オプション */
enum IPLeadInsMode {
	INS_MODE,						/* 検査モード */
	ANA_MODE,						/* 抽出モード */
	PITCH_MODE						/* ピッチ＋検査 */
};
/* 描画モード */
enum IPDrawLineMode {
	DRAW_LINE,						/* 線描画               */
	DRAW_RECT,						/* 矩形描画             */
	DRAW_FILL,						/* フィル               */
	DRAW_CROSS_RECT,				/* 中心線付矩形         */
	DRAW_LEFT_TOP_RECT,				/* 左上ポイント付矩形   */
	DRAW_RIGHT_BOTTOM_RECT,			/* 右下ポイント付矩形   */
	DRAW_LINE_R,					/* 内側ラウンド付線描画 */
	DRAW_FILL_R,					/* 内側ラウンド付フィル */
	DRAW_CHECK	=			0x0100,	/* チェックモード       */
	DRAW_TRI_CENTER =		0x1000	/* 三角形重なり中心     */
};
/* 文字描画モード */
enum IPFontColorMode {
	BACKCOL_NOWRITE,				/* 文字部分のみcolF色で描画 */
	BACKCOL_NOWRITE_REVERSE,		/* 文字背景のみcolB色で描画 */
	BACKCOL_WRITE,					/* 文字部分をcolF色、背景をcolBで描画 */
	BACKCOL_WRITE_REVERSE			/* 文字部分をcolB色、背景をcolFで描画 */
};
/* 文字描画サイズ */
enum IPFontDrawSize {
	STR_SIZE_1,						/* 標準サイズ */
	STR_SIZE_2,						/* ２倍角     */
	STR_SIZE_3,						/* ３倍角     */
	STR_SIZE_4						/* ４倍角     */
};
/* フォントデータサイズ */
enum IPFontDataSize {
	STRDOT_SIZE_8,					/* フォントデータ８ドット   */
	STRDOT_SIZE_16,					/* フォントデータ１６ドット */
	STRDOT_SIZE_32,					/* フォントデータ３２ドット */
	STRDOT_SIZE_48					/* フォントデータ４８ドット */
};
/* ライン検出モード */
enum IPHoughRowMode {
	MIN_LINE,						/* rhoの最小値を検出 */
	MAX_LINE						/* rhoの最大値を検出 */
};
/* ライン検出モード */
enum IPHoughLineRhoMode {
	MIN_RHO_LINE,					/* rhoの最小値を検出 */
	MAX_RHO_LINE					/* rhoの最大値を検出 */
};
/* 表示ﾓｰﾄﾞ */
enum IPDispSwitch {
	DISP_OFF,						/* 表示 OFF */
	DISP_ON							/* 表示 ON  */
};
/* ｼｬｯﾀｰｽﾋﾟｰﾄﾞ */
enum ShutterSpeed {
	SHUT_4000,						/* ｼｬｯﾀｰｽﾋﾟｰﾄﾞ1/4000(0.25mS) */
	SHUT_2000,						/* ｼｬｯﾀｰｽﾋﾟｰﾄﾞ1/2000( 0.5mS) */
	SHUT_1000,						/* ｼｬｯﾀｰｽﾋﾟｰﾄﾞ1/1000( 1.0mS) */
	SHUT_500,						/* ｼｬｯﾀｰｽﾋﾟｰﾄﾞ1/500 ( 2.0mS) */
	SHUT_250,						/* ｼｬｯﾀｰｽﾋﾟｰﾄﾞ1/250 ( 4.0mS) */
	SHUT_125						/* ｼｬｯﾀｰｽﾋﾟｰﾄﾞ1/125 ( 8.0mS) */
};
/* 正規化相関サーチモード */
enum IPCorrMode {
	CORR_NORMAL,					/* ﾉｰﾏﾙﾓｰﾄﾞ         */
	CORR_SINGLE_KERNEL,				/* 1ｶｰﾈﾙﾓｰﾄﾞ        */
	CORR_PARALLEL_KERNEL			/* 3x3ｶｰﾈﾙﾓｰﾄﾞ      */
};
/* 正規化相関小数点出力桁数 */
enum IPCorrPrecise {
	CORR_PRECISE_1,					/* 小数点１桁 */
	CORR_PRECISE_2,					/* 小数点２桁 */
	CORR_PRECISE_3,					/* 小数点３桁 */
	CORR_PRECISE_4					/* 小数点４桁 */
};
/* インテリジェントモジュールＩＤ  */
enum IPModuleID {
	MODULE_0,						/* モジュール＃０   */
	MODULE_1,						/* モジュール＃１   */
	MODULE_2,						/* モジュール＃２   */
	MODULE_3,						/* モジュール＃３   */
	MODULE_4,						/* モジュール＃４   */
	MODULE_5,						/* モジュール＃５   */
	MODULE_6,						/* モジュール＃６   */
	MODULE_7,						/* モジュール＃７   */
	MODULE_8,						/* モジュール＃８   */
	MODULE_9,						/* モジュール＃９   */
	MODULE_10,						/* モジュール＃１０ */
	MODULE_11,						/* モジュール＃１１ */
	MODULE_12,						/* モジュール＃１２ */
	MODULE_13,						/* モジュール＃１３ */
	MODULE_14,						/* モジュール＃１４ */
	MODULE_15						/* モジュール＃１５ */
};
/* パラメータ設定番号 */
enum IPModuleParamNo {
	PARAM_1,						/* 第１パラメータ   */
	PARAM_2,						/* 第２パラメータ   */
	PARAM_3,						/* 第３パラメータ   */
	PARAM_4,						/* 第４パラメータ   */
	PARAM_5,						/* 第５パラメータ   */
	PARAM_6,						/* 第６パラメータ   */
	PARAM_7,						/* 第７パラメータ   */
	PARAM_8,						/* 第８パラメータ   */
	PARAM_9,						/* 第９パラメータ   */
	PARAM_10,						/* 第１０パラメータ */
	PARAM_11,						/* 第１１パラメータ */
	PARAM_12,						/* 第１２パラメータ */
	PARAM_13,						/* 第１３パラメータ */
	PARAM_14,						/* 第１４パラメータ */
	PARAM_15,						/* 第１５パラメータ */
	PARAM_16,						/* 第１６パラメータ */
	PARAM_17,						/* 第１７パラメータ */
	PARAM_18,						/* 第１８パラメータ */
	PARAM_19,						/* 第１９パラメータ */
	PARAM_20,						/* 第２０パラメータ */
	PARAM_21,						/* 第２１パラメータ */
	PARAM_22,						/* 第２２パラメータ */
	PARAM_23,						/* 第２３パラメータ */
	PARAM_24,						/* 第２４パラメータ */
	PARAM_25,						/* 第２５パラメータ */
	PARAM_26,						/* 第２６パラメータ */
	PARAM_27,						/* 第２７パラメータ */
	PARAM_28,						/* 第２８パラメータ */
	PARAM_29,						/* 第２９パラメータ */
	PARAM_30,						/* 第３０パラメータ */
	PARAM_31,						/* 第３１パラメータ */
	PARAM_32						/* 第３２パラメータ */
};
/* モジュールタイプ */
enum IPModuleType {
	INTELI_MODULE,					/* インテリジェントモジュール  */
	PIOINT_MODULE					/* ＰＩＯ割込み起動モジュール  */
};
/* ウェイクアップモード */
enum IPWakeupMode {
	SELF_WAKEUP,					/* セルフ(自己)ウェイクアップ  */
	PIO_WAKEUP,						/* ＰＩＯ割込みからのｳｪｲｸｱｯﾌﾟ  */
	SELF_WAKEUP_EXT					/* 拡張セルフウェイクアップ    */
};
/* 拡張画像メモリアクセス用情報取得関数GetImInfo()のモード設定値   */
enum GetImInfoMode {
	IMG_NO_WAIT		=			0x0000,	/* 画像処理ウェイトしない  */
	IMG_BUSY_WAIT	=			0x0001,	/* 画像処理ウェイトする    */
	READ_REVERSE	=			0x0010,	/* リードリバース          */
	WRITE_REVERSE	=			0x0020	/* ライトリバース          */
};
/* 時間計測モード */
enum IPTimeMode {
	U_SEC,							/* uS */
	M_SEC,							/* mS */
	S_SEC							/* S  */
};
/* ＳＣＩチャネル       */
enum SCICh {
	SCI_CH0       = 0,				/* SCI CH0       */
	SCI_CH1       = 1,				/* SCI CH1       */
	SCI_CH0_WRITE = 0,				/* SCI CH0 WRITE */
	SCI_CH0_READ  = 1,				/* SCI CH0 READ  */
	SCI_CH1_WRITE = 2,				/* SCI CH1 WRITE */
	SCI_CH1_READ  = 3				/* SCI CH1 READ  */
};
/* ＳＣＩボーレート     */
enum SCIBitRate {
	SCI_4800,						/*  4800 bit/sec */
	SCI_9600,						/*  9600 bit/sec */
	SCI_19200,						/* 19200 bit/sec */
	SCI_38400,						/* 38400 bit/sec */
	SCI_57600,						/* 57600 bit/sec */
	SCI_2400 = 0xff,				/*  2400 bit/sec */
	SCI_1200 = 0xfe					/*  1400 bit/sec */
};
/* ＳＣＩキャラクタ長   */
enum SCIChrlng {
	SCI_8Bit,						/* 8 bit データ */
	SCI_7Bit						/* 7 bit データ */
};
/* ＳＣＩパリティ       */
enum SCIParity {
	SCI_NonParity,					/* パリティ無し */
	SCI_NonParity1,					/* パリティ無し */
	SCI_EvenParity,					/* 偶数パリティ */
	SCI_OddParity					/* 奇数パリティ */
};
/* ＳＣＩストップビット */
enum SCIStopBit {
	SCI_StopBit1,					/* １ストップビット */
	SCI_StopBit2					/* ２ストップビット */
};
/* ユーザシグナル番号 */
enum PCISignalCom {
	PCI_SYSTEM_DOWN   = 0,			/* システムダウンシグナル     */
	PCI_SIGWAIT_CANCEL= 1,			/* ウェイトキャンセルシグナル */
	PCI_SIGNAL_USER_0 = 16,			/* ユーザシグナル＃０         */
	PCI_SIGNAL_USER_1 = 17,			/* ユーザシグナル＃１         */
	PCI_SIGNAL_USER_2 = 18,			/* ユーザシグナル＃２         */
	PCI_SIGNAL_USER_3 = 19,			/* ユーザシグナル＃３         */
	PCI_SIGNAL_USER_4 = 20,			/* ユーザシグナル＃４         */
	PCI_SIGNAL_USER_5 = 21,			/* ユーザシグナル＃５         */
	PCI_SIGNAL_USER_6 = 22,			/* ユーザシグナル＃６         */
	PCI_SIGNAL_USER_7 = 23			/* ユーザシグナル＃７         */
};
/* ユーザシグナルビットマスク */
#define	SIGMSK_SYSDWN		0x00000001
#define	SIGMSK_CANCEL		0x00000002
#define	SIGMSK_USER_0		0x00010000
#define	SIGMSK_USER_1		0x00020000
#define	SIGMSK_USER_2		0x00040000
#define	SIGMSK_USER_3		0x00080000
#define	SIGMSK_USER_4		0x00100000
#define	SIGMSK_USER_5		0x00200000
#define	SIGMSK_USER_6		0x00400000
#define	SIGMSK_USER_7		0x00800000
#define	SIGMSK_USER_ALL		0x00ff0001
/* タスクプライオリティ */
enum IPTaskPriority {
	TASK_PRI_HIGHEST		= -2,	/* ノーマル -2      */
	TASK_PRI_ABOVE_NORMAL	= -1,	/* ノーマル -1      */
	TASK_PRI_NORMAL			= 0,	/* ノーマル  0      */
	TASK_PRI_BELOW_NORMAL	= 1,	/* ノーマル  1      */
	TASK_PRI_LOWEST			= 2		/* ノーマル  2      */
};
/* タスクオプション */
enum IPTaskOption {
	TASK_NO_OPTION		= 0,		/* オプションなし            */
	CANCEL_WAKEUP		= 0x0001,	/* キャンセルウェイクアップ  */
	DISABLE_INTERRUPT	= 0x0100	/* ＰＩＯ割込禁止            */
};
// 割込デバイス
#define		INTDEV_UNLINK			0
#define		INTDEV_PCI				1
#define 	INTDEV_PIO				2
// 割込イベント
#define		INTEVENT_NOP			0
#define		INTEVENT_WAKEUP			1
/* タスクステータス */
#define		TASK_STS_RUN			0x00000001	/* 実行状態                     */
#define		TASK_STS_RDY			0x00000002	/* 実行可能状態                 */
#define		TASK_STS_WAI			0x00000004	/* 待ち状態                     */
#define		TASK_STS_SUS			0x00000008	/* 強制待ち状態                 */
#define		TASK_STS_WAS			0x0000000C	/* 二重待ち状態                 */
#define		TASK_STS_DMT			0x00000010	/* 休止状態                     */
#define		TASK_STS_STK			0x40000000	/* 共有スタック解放待ち状態     */
#define		TASK_WAIT_SLP			0x00000001	/* スリープタスクによる待ち     */
#define		TASK_WAIT_DLY			0x00000002	/* ディレイタスクによる待ち     */
#define		TASK_WAIT_SEM			0x00000004	/* ウェイトセマフォによる待ち   */
#define		TASK_WAIT_FLG			0x00000008	/* ウェイトフラグによる待ち     */
#define		TASK_WAIT_SDTQ			0x00000010	/* SNDDTQによる待ち             */
#define		TASK_WAIT_RDTQ			0x00000020	/* RCVDTQによる待ち             */
#define		TASK_WAIT_MBX			0x00000040	/* レシーブメッセージによる待ち */
#define		TASK_WAIT_MTX			0x00000080	/* ミューテックスによる待ち     */
#define		TASK_WAIT_SMBF			0x00000100	/* SNDMBFによる待ち             */
#define		TASK_WAIT_RMBF			0x00000200	/* RCVMBFによる待ち             */
#define		TASK_WAIT_MPF			0x00002000	/* ゲットブロックによる待ち     */
#define		TASK_WAIT_MPL			0x00004000	/* ゲットブロックによる待ち     */
#define		TASK_WAIT_TFL			0x00008000	/* VWAITFLによる待ち            */
#define		TASK_ECM_SUS			0x00000001	/* 強制待ち要求がマスクされている  */
#define		TASK_ECM_TER			0x00000002	/* 強制終了要求がマスクされている  */
#define		TASK_PND_SUS			0x00000004	/* 強制待ち要求が遅延されている    */
#define		TASK_PND_TER			0x00000008	/* 強制終了要求が遅延されている    */
/* セマフォＩＤ */
enum IPSemaphoreID {
	IP_SEMID_0 = 5,					/* セマフォ＃０ */
	IP_SEMID_1 = 6,					/* セマフォ＃１ */
	IP_SEMID_2 = 7,					/* セマフォ＃２ */
	IP_SEMID_3 = 8					/* セマフォ＃３ */
};
/* タイマＩＤ */
enum IPTimerID {
	TIMERID_0,						/* μＩＴＲＯＮ */
	TIMERID_1,						/* スリープ     */
	TIMERID_2						/* 時間計測     */
};
/* セマフォ，イベントフラグ  */
#define	TASK_ATTR_TFIFO		0x00000000	// 待ちタスクのキューイングはFIFO
#define	TASK_ATTR_TPRI		0x00000001	// 待ちタスクのキューイングは優先度順
#define	TASK_ATTR_MFIFO		0x00000000	// メッセージのキューイングはFIFO
#define	TASK_ATTR_MPRI		0x00000002	// メッセージのキューイングは優先度順
#define	TASK_ATTR_WSGL		0x00000000	// 複数タスクの待ちを許さない
#define	TASK_ATTR_WMUL		0x00000002	// 複数タスクの待ちを許す
#define	TASK_ATTR_CLR		0x00000004	// クリア指定
#define	WFLG_AND_WAIT		0x00000000	// ＡＮＤ待ち
#define	WFLG_OR_WAIT		0x00000001	// ＯＲ待ち

/* フラッシュメモリファイルコピー */
enum FlashFileCopyMode {
	HDD_TO_FLASH,					/* ＨＤＤ->フラッシュメモリ   */
	FLASH_TO_HDD,					/* フラッシュメモリ->ＨＤＤ   */
	FLASH_TO_FLASH					/* ﾌﾗｯｼｭﾒﾓﾘ -> ﾌﾗｯｼｭﾒﾓﾘ       */
};

/* 映像表示画面マクロ */
#define		DISPLY_IMGID			GetDispImgID()
#define		BITMAP_IMGID			GetBitmapImgID()
/* ＰＩＯ割込モジュール  ＰＣ－Ｉ／Ｆシグナルコード */
#define		TASK_COMPLETE_REQUEST	0x00	/* タスク終了コード   */
#define		TASK_COMPLETE_PENDING	0x01	/* タスクペンディング */
/* シグナルウェイトステータス */
#define 	WAIT_SIGNAL_DETECT		0	/* シグナル検出 */
#define 	WAIT_SIGNAL_TIMEOUT		1	/* タイムアウト */

/* フラッシュメモリオープンモード */
#define		FM_DEFAULT				0x00		/* デフォルト         */
#define		FM_RDONLY				0x01		/* リードオンリ       */
#define		FM_WRONLY				0x02		/* ライトオンリ       */
#define		FM_RDWR					0x04		/* リードライト       */
#define		FM_CREATE				0x08		/* クリエイト         */
#define		FM_TRUNC				0x10		/* クリア             */
#define		FM_APPEND				0x20		/* アッペンド         */

/* フラッシュメモリファイル属性 */
#define		FM_ATTR_DEFAULT			0x00000000	/* デフォルト         */
#define		FM_ATTR_RDONLY			0x00000001	/* リードオンリ       */
#define		FM_ATTR_WRONLY			0x00000002	/* ライトオンリ       */
#define		FM_ATTR_RDWR			0x00000004	/* リードライト       */
#define		FM_ATTR_CREATE			0x00000008	/* クリエイト         */
#define		FM_ATTR_SECRET			0x00000010	/* シークレット       */
#define		FM_ATTR_DIRECTORY		0x00001000	/* ディレクトリ       */

#define		FM_RESET				0			/* リセット           */
#define		FM_SET					1			/* セット             */

/* シークモード                        */
#define		FM_SEEK_CUR				0			/* ファイル ポインタの位置 */
#define 	FM_SEEK_END				1			/* ファイルの終端     */
#define		FM_SEEK_SET				2			/* ファイルの先頭     */

/* ビジーウェイト制御モード */
#define		BUSY_WAIT_POLLING		0			/* ポーリングによるウェイト */
#define		BUSY_WAIT_INTERRUPT		1			/* 割込によるウェイト       */

/* RGBLUTモード */
#define	RGBLUT888			0		/* R:G:B = 8:8:8 モード  */
#define RGBLUT777			1		/* R:G:B = 7:7:7 モード  */
#define RGBLUT666			2		/* R:G:B = 6:6:6 モード  */
#define RGBLUT887			3		/* R:G:B = 8:8:7 モード  */
#define RGBLUT878			4		/* R:G:B = 8:7:8 モード  */
#define RGBLUT788			5		/* R:G:B = 7:8:8 モード  */
#define RGBLUT787			6		/* R:G:B = 7:8:7 モード  */
#define RGBLUT2CH88			7		/* R:G(2CH) = 8:8 モード */

/* RGBLUTテーブルアクセスマクロ */
#define	GetRGBLUT888(lut,r,g,b)			lut[r << 16 | g << 8 | b]
#define	GetRGBLUT777(lut,r,g,b)			lut[(r & 0xfe) << 13 | (g & 0xfe) << 6 | b >> 1]
#define	GetRGBLUT666(lut,r,g,b)			lut[(r & 0xfc) << 10 | (g & 0xfc) << 4 | b >> 2]
#define	GetRGBLUT887(lut,r,g,b)			lut[r << 15 | g << 7 | b >> 1]
#define	GetRGBLUT878(lut,r,g,b)			lut[r << 15 | (g & 0xfe) << 7 | b]
#define	GetRGBLUT788(lut,r,g,b)			lut[(r & 0xfe) << 15 | g << 8 | b]
#define	GetRGBLUT787(lut,r,g,b)			lut[(r & 0xfe) << 14 | g << 7 | b >> 1]
#define	GetRGBLUT2CH88(lut,r,g)			lut[r << 8 | g]

#define	SetRGBLUT888(lut,r,g,b,d)		lut[r << 16 | g << 8 | b]= (d)
#define	SetRGBLUT777(lut,r,g,b,d)		lut[(r & 0xfe) << 13 | (g & 0xfe) << 6 | b >> 1]= (d)
#define	SetRGBLUT666(lut,r,g,b,d)		lut[(r & 0xfc) << 10 | (g & 0xfc) << 4 | b >> 2]= (d)
#define	SetRGBLUT887(lut,r,g,b,d)		lut[r << 15 | g << 7 | b >> 1]= (d)
#define	SetRGBLUT878(lut,r,g,b,d)		lut[r << 15 | (g & 0xfe) << 7 | b]= (d)
#define	SetRGBLUT788(lut,r,g,b,d)		lut[(r & 0xfe) << 15 | g << 8 | b]= (d)
#define	SetRGBLUT787(lut,r,g,b,d)		lut[(r & 0xfe) << 14 | g << 7 | b >> 1]= (d)
#define	SetRGBLUT2CH88(lut,r,g,d)		lut[r << 8 | g]= (d)

/* 線分列特徴量抽出オプション */
#define POLY_CENTER_of_GRAVITY		0		/* 外接座標＋周囲長＋面積＋重心   */
#define POLY_MOMENT					1		/* ＋モーメント＋慣性主軸角度     */
#define POLY_AXIS_LENG				2		/* ＋慣性主軸長＋パターン幅       */
#define POLY_MAX_LENG				3		/* ＋絶対最大長＋パターン幅       */

#endif		/* !__vpxdef_h__ */

