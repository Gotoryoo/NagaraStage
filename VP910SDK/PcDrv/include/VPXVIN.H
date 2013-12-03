/*                                                                                        */
/*		+------------------------------------------------------------------------+        */
/*		+ Copyright (c) 2000-2004 Renesas  Northern Japan Semiconductor,Inc.     +        */
/*		+------------------------------------------------------------------------+        */
/*		Title : VP-900 Serise Video Data Table Struct                                     */
/*                                                                                        */
/*                                                                                        */

#ifndef		__vpxvin_h__
#define		__vpxvin_h__


/******************************************************************************
			ビデオデータテーブル構造体の定義
******************************************************************************/
typedef struct {
	unsigned long		VPCNFreg;
	unsigned long		VPCNF2reg;
	unsigned long		VPCMDreg;
	unsigned long		VPCLMPCTLreg;
	unsigned long		VPTRGCTLreg;
	unsigned long		VPHSPLLreg;
	unsigned long		VPFLDDETreg;
	unsigned long		VPSSPCTLreg;
	unsigned long		VPHSGCTLreg;
	unsigned long		VPVSGCTLreg;
} VIDEO_VP_TBL, *PVIDEO_VP_TBL;

typedef struct {
	unsigned char		CTYPEreg;
	unsigned char		VP2CLKreg;
	unsigned char		CVBSCNTreg;
	unsigned char		RGBCNTreg;
	unsigned char		CamInfo;
	unsigned char		RRInfo;
	unsigned char		OptInfo;
	unsigned char		Reserve;
	float				Hcycle;
	unsigned char		CLMODEreg;
	unsigned char		CLCLKreg;
	unsigned char		CC12SELreg;		// ノーマルモード
	unsigned char		CC34SELreg;		// ノーマルモード
	unsigned char		TrgCC12SELreg;	// トリガモード時CC12SEL
	unsigned char		TrgCC34SELreg;	// トリガモード時CC34SEL
} VIDEO_PLD_TBL, *PVIDEO_PLD_TBL;

typedef struct {
	unsigned long		DPCNFreg;
	unsigned long		DPCMDreg;
	unsigned long		DPFLDDETreg;
} VIDEO_DP_TBL, *PVIDEO_DP_TBL;

typedef struct {
	unsigned short		VDLY;
	unsigned short		HDLY;
} VIDEO_DLY, *PVIDEO_DLY;

typedef struct {
	unsigned short		VSIZE;
	unsigned short		HSIZE;
} VIDEO_SIZE, *PVIDEO_SIZE;

typedef struct {
	VIDEO_DLY			fs512H_440V;
	VIDEO_DLY			fs512H_480V;
	VIDEO_DLY			fs640H_480V;
	VIDEO_SIZE			fsDEFAULT;
} VIDEO_DLY_TBL, *PVIDEO_DLY_TBL;

typedef struct {
	unsigned short		HMASKCNT;
	unsigned short		VSMPLCNT;
} VIDEO_PLLCNT_TBL, *PVIDEO_PLLCNT_TBL;


#endif /* !__vpxvin_h__ */

