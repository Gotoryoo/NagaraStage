#ifndef		__vpxadm_h__
#define		__vpxadm_h__
/*                                                                                   */
/*    +------------------------------------------------------------------------+     */
/*    + Copyright (c) 2000-2004 Renesas  Northern Japan Semiconductor,Inc.     +     */
/*    +------------------------------------------------------------------------+     */
/*    Title : VP-900 Series Header File For Administrator                            */
/*                                                                                   */
/*                                                                                   */

/***************************************************************************
      DEFINE
****************************************************************************/

/**** ＳＨ例外コード ***/
#define	EXPCOD_NMI			0x01C0			/* NMI割込                     */
#define	EXPCOD_IRL0			0x0240			/* IRL0割込                    */
#define	EXPCOD_IRL1			0x02A0			/* IRL1割込                    */
#define	EXPCOD_IRL2			0x0300			/* IRL2割込                    */
#define	EXPCOD_IRL3			0x0360			/* IRL3割込                    */
#define	EXPCOD_TMU0_TUNI	0x0400			/* TMU0-TUNI割込               */
#define	EXPCOD_TMU1_TUNI	0x0420			/* TMU1-TUNI割込               */
#define	EXPCOD_TMU2_TUNI	0x0440			/* TMU2-TUNI割込               */
#define	EXPCOD_TMU2_TICPI	0x0460			/* TMU2-TICPI割込              */
#define	EXPCOD_RTC_ATI		0x0480			/* RTC-ATI割込                 */
#define	EXPCOD_RTC_PRI		0x04A0			/* RTC-PRI割込                 */
#define	EXPCOD_RTC_CUI		0x04C0			/* RTC-CUI割込                 */
#define	EXPCOD_SCI_ERI		0x04E0			/* SCI-ERI割込                 */
#define	EXPCOD_SCI_RXI		0x0500			/* SCI-RXI割込                 */
#define	EXPCOD_SCI_TXI		0x0520			/* SCI-TXI割込                 */
#define	EXPCOD_SCI_TEI		0x0540			/* SCI-TEI割込                 */
#define	EXPCOD_WDT_ITI		0x0560			/* WDT-ITI割込                 */
#define	EXPCOD_REF_RCMI		0x0580			/* REF-RCMI割込                */
#define	EXPCOD_REF_ROVI		0x05A0			/* REF-ROVI割込                */
#define	EXPCOD_HUDI			0x0600			/* HUDI割込                    */
#define	EXPCOD_DMAC_DMTE0	0x0640			/* DMAC-DMTE0割込              */
#define	EXPCOD_DMAC_DMTE1	0x0660			/* DMAC-DMTE1割込              */
#define	EXPCOD_DMAC_DMTE2	0x0680			/* DMAC-DMTE2割込              */
#define	EXPCOD_DMAC_DMTE3	0x06A0			/* DMAC-DMTE3割込              */
#define	EXPCOD_DMAC_DMAE	0x06C0			/* DMAC-DMAE割込               */
#define	EXPCOD_SCIF_ERI		0x0700			/* SCIF-ERI割込                */
#define	EXPCOD_SCIF_RXI		0x0720			/* SCIF-RXI割込                */
#define	EXPCOD_SCIF_BRI		0x0740			/* SCIF-BRI割込                */
#define	EXPCOD_SCIF_TXI		0x0760			/* SCIF-TXI割込                */

/*** デバイス名 ***/
#define	SH_TMU0				0				/* タイマユニット＃０          */
#define	SH_TMU1				1				/* タイマユニット＃１          */
#define	SH_TMU2				2				/* タイマユニット＃２          */
#define	SH_RTC				3				/* リアルタイムクロック        */
#define	SH_WDT				4				/* ウォッチドッグタイマ        */
#define	SH_REF				5				/* リフレッシュコントローラ    */
#define	SH_SCI				6				/* シリアルCOM                 */
#define	SH_RESERVE			7				/* リザーブ                    */
#define	SH_GPIO				8				/* Ｉ／Ｏポート                */
#define	SH_DMAC				9				/* ＤＭＡコントローラ          */
#define	SH_SCIF				10				/* SCIF付きシリアルCOM         */
#define	SH_HUDI				11				/* H-UDI                       */
#define	SH_IRL0				12				/* 外部割込＃０                */
#define	SH_IRL1				13				/* 外部割込＃１                */
#define	SH_IRL2				14				/* 外部割込＃２                */
#define	SH_IRL3				15				/* 外部割込＃３                */

/***************************************************************************
      STRUCT
****************************************************************************/

/*** 割込ハンドラ情報テーブル ***/
typedef struct {
	void			(*inthdr)();			/* 割込ハンドラアドレス        */
	unsigned long	intmsk;					/* 割込マスクレベル            */
} DEFHDR_INFO;


#endif /* !__vpxadm_h__ */

