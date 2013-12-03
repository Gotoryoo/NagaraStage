/*                                                                                        */
/*		+------------------------------------------------------------------------+        */
/*		+ Copyright (c) 2000-2004 Renesas  Northern Japan Semiconductor,Inc.     +        */
/*		+------------------------------------------------------------------------+        */
/*		Title : VP-900 Serise Image Procesing Function Prototype                          */
/*                                                                                        */
/*                                                                                        */

#ifndef		__ipxfnc_h__
#define		__ipxfnc_h__

#include "vpxdef.h"
#include "vpxsys.h"

#ifdef __cplusplus
extern "C"{
#endif

#ifdef _WIN32
	#define CCONV _stdcall
#else
	#define CCONV
#endif	/* _WIN32 */


/*    システム制御 １       */
int	CCONV IPDriverStartup(void);
int	CCONV NopIP(void);
long CCONV SetIPTimeOut(long);
int	CCONV ExceptionTest();
int	CCONV EndlessLoopTest();
int	CCONV RefReglockCtl(void);
int	CCONV SetReglockCtl(int lock);
int	CCONV RefTaskTimeout(int command);
int	CCONV SetTaskTimeout(int command,int timeout);
int	CCONV ISPWaitSemaphore(int command,int timeout);
int	CCONV ISPReleaseSemaphore(int command);
int	CCONV RefReglockWait(int command);
int CCONV SetReglockWait(int command,int wait);
int CCONV EnableErrorPrint(void);
int CCONV DisableErrorPrint(void);
int CCONV SetStdPort(int port);
int CCONV SetGraphicsRefreshMode(int mode);
int CCONV SetResetIPMode(int mode);

/*    システム制御 ２       */
void CCONV DbFlagON(void);
void CCONV DbFlagOFF(void);
long CCONV mallocIP(long);
void CCONV mfreeIP(long);
int CCONV GetIPVersionInfo(VERSIONINFO*);
int	CCONV GetIPBoardNo(void);

/*    ITRONタスク制御      */
int	CCONV CreateIPTask(CREATE_TASK_TBL *tbl);
int	CCONV CreateTask(int tskid,CREATE_TASK_TBL *tbl);
int	CCONV RegistIPTask(int,unsigned long,unsigned long,int);
int	CCONV StartIPTask(int);
int	CCONV StartIPTaskExt(int taskID,int extinf);
int	CCONV TerminateIPTask(int);
int	CCONV WakeupIPTask(int);
int	CCONV ResumeIPTask(int);
int	CCONV SuspendIPTask(int);
void CCONV ExitIPTask(void);
int	CCONV DeleteIPTask(int);
int	CCONV SendSignaltoPC(int,int,int);
int	CCONV CreateInterruptLink(INT_DEVICE_OBJ *obj,int taskID,int event,int opt);
int	CCONV DeleteInterruptLink(INT_DEVICE_OBJ *obj);
int	CCONV EnableInterruptObject(INT_DEVICE_OBJ *obj,int opt);
int	CCONV DisableInterruptObject(INT_DEVICE_OBJ *obj);
int	CCONV GetRegacyPIOTaskID(int pintskid);
int CCONV CreateParamBuffer(int taskid,unsigned long size);
int CCONV DeleteParamBuffer(int taskid);
int	CCONV GetWrkTaskInfo(int taskid,unsigned long *info);

/*    画像処理システムイニシャライズ    */
int	CCONV InitIPError(int,int,int,int,int);
int	CCONV CheckIPError(void);

/*    画像処理ウェイト制御    */
int CCONV ISP_BusyWait(unsigned long busy);
int	CCONV ISP_BusyCheck(unsigned long *sts);
int	CCONV ImgBusyWait(unsigned long busy,int ImgID);
int	CCONV Img2chBusyWait(unsigned long busy,int ImgID0,int ImgID1);
int	CCONV Img3chBusyWait(unsigned long busy,int ImgID0,int ImgID1,int ImgID2);
int	CCONV ClearImgBusy(int ImgID);

/*    画像メモリアクセス    */
long CCONV ReadImgReverse(int ImgID,char *ImgTbl,long count);
long CCONV WriteImgReverse(int ImgID,char *ImgTbl,long count);
int CCONV WriteImgPCMA(int);
int CCONV ReadImgPCMA(int);
int CCONV WriteImgBWOnly(int);
int CCONV ReadImgBWOnly(int);
int	CCONV ReadImgLine(int, LINETBL*, int, int, int, int);
int	CCONV WriteImgLine(int, LINETBL*, int, int, int, int);

/*    システム制御 ３    */
void CCONV SetCCR(int ccr);
int	CCONV GetCCR(void);

/*    画像処理サポート関数  */
int CCONV GetDispImgID();
int CCONV GetBitmapImgID();
int CCONV GetLineWindowPoint(LINEWINDOW *LineWindow,RECTPOINT *RectPoint);
long CCONV GetImInfo(int ImgID,unsigned long *tbl,int mode);
int	CCONV EnableHMRange(int start,int count);
int	CCONV DisableHMRange(void);

/*    画像メモリ領域管理    */
int	CCONV ReadIPWindowLeng(enum WindowType type,int *xlng,int *ylng);
int CCONV ReadImgDataType(int ImgID);
int CCONV AllocDispImg(enum ImageFrameSize size,int type);
int CCONV FreeDispImg(void);

/*    画像入力       */
int CCONV SetCameraSync(enum CameraSync sync);
int	CCONV SetShutterSpeed(enum ShutterSpeed mode);
int	CCONV TriggerOnCamera(void);
int	CCONV ClearInvalidFrame(void);
int	CCONV GetCameraSts(void);
int	CCONV GetFieldSTS(void);
int	CCONV SetPartialFrame(int vdly,int vsize);
int	CCONV SetPartialLineStart(int en,int sp,int clk,int dly,int wid);
int	CCONV SetPartialShutter(int en,int dly,int wid);
int	CCONV SetVFSize(int hsize,int vsize);
int	CCONV SetVFSizeMltPort(int vpid,int hsize,int vsize);
int	CCONV SetVFDelayMltPort(int vpid,int hdly,int vdly);
int	CCONV WriteVideoLUT(int *lutP0,int *lutP1,int lng,int mode);
int	CCONV WriteVideoLUTMltPort(int vpid,int *lutP0,int *lutP1,int lng,int mode);
int	CCONV SetVideoControlOpt(int mode,int opt1,int opt2);
int	CCONV SetVideoControlOptMltPort(int vpid,int mode,int opt1,int opt2);
int CCONV ResetCamera(enum CameraID id,enum CameraType type,int opt);

/*    画像表示       */
int	CCONV EnableLoopDisp(int ImgID);
int CCONV DisableLoopDisp(enum WaitMode mode);
int CCONV SuspendLoopDisp(enum WaitMode mode);
int CCONV ResumeLoopDisp(void);
int CCONV SetDFDelay(int HOffset,int VOffset);
int CCONV SetDispWindow(int sx,int sy,int xmag,int ymag);
int CCONV EnableHIREZDisp(void);
int CCONV DisableHIREZDisp(void);
int CCONV SetDFSize(int HSize,int VSize);
int CCONV SetDispVFrame(int vdly,int vsize);

/*    IP5000グラフィックス表示サポート       */
int	CCONV RefreshGraphics(void);

/*    オーバレイ制御        */
int CCONV BitmapOverlap(int mode);
int	CCONV SetOverlayLUT(short*);

/*   画像転送／アフィン変換   */
int	CCONV vpxIP_Rotate(int ImgSrc,int ImgDst,
			float angle,int xc,int yc,int dx,int dy,enum SPACE_OPT opt);

/*    正規化相関サーチ制御   */
int	CCONV vpxEnableCorrMask(void);
int	CCONV vpxDisableCorrMask(void);
int	CCONV vpxSetCorrBreakThr(int,int);
int	CCONV vpxEnableCorrBreak(void);
int	CCONV vpxDisableCorrBreak(void);
int	CCONV vpxSetSearchDistance(int,int,int);
int CCONV vpxSetCorrMode(enum IPCorrMode mode);
int CCONV vpxSetCorrPrecise(enum IPCorrPrecise precise);

/*    正規化相関サーチ   */
int	CCONV vpxIP_CorrStep(int,int,CORRTBL*,int,int,int);
int	CCONV vpxIP_CorrPoint(int,int,CORRTBL*,int,CORRTBL*,int,int,int,int,int);
int	CCONV vpxIP_CorrPrecise(int,int,CORRTBL*,FCORRTBL*);
int	CCONV vpxIP_CorrExt(int,int,CORRTBL*,CORREXT*);
int	CCONV vpxIP_CorrStepDualTmp(int,int,int,int,int,
                           int,CORRTBL*,int,int,int);
int	CCONV vpxIP_CorrPointDualTmp(int,int,int,int,
             int,int,CORRTBL*,int,CORRTBL*,int,int,int,int,int);
int	CCONV vpxIP_CorrPrecDualTmp(int,int,int,
                           int,int,CORRTBL*,FCORRTBL*);
int	CCONV vpxGetCorrMapSize(int,int,int,int,int*,int*,int*);
int	CCONV vpxIP_CorrMap(int,int,float*,int,int,int);

/*    正規化相関サーチ（パケットベース）   */
int	CCONV vpxIP_CorrStepPac(int,CORRTEMPLATE*,CORRTBL*,int,int,int);
int	CCONV vpxIP_CorrPointPac(int,CORRTEMPLATE*,CORRTBL*,int,CORRTBL*,int,int,int,int,int);
int	CCONV vpxIP_CorrPrecisePac(int,CORRTEMPLATE*,CORRTBL*,FCORRTBL*);
int	CCONV vpxIP_CorrExtPac(int,CORRTEMPLATE*,CORRTBL*,CORREXT*);
int	CCONV vpxIP_CorrStepDualTmpPac(int,CORRTEMPLATE*,int,int,int,
             CORRTEMPLATE*,CORRTBL*,int,int,int);
int	CCONV vpxIP_CorrPointDualTmpPac(int,CORRTEMPLATE*,int,int,
             int,CORRTEMPLATE*,CORRTBL*,int,CORRTBL*,int,int,int,int,int);
int	CCONV vpxIP_CorrPrecDualTmpPac(int,CORRTEMPLATE*,int,
             int,CORRTEMPLATE*,CORRTBL*,FCORRTBL*);
int	CCONV vpxGetCorrMapSizePac(int,CORRTEMPLATE*,int,int,int*,int*,int*);
int	CCONV vpxIP_CorrMapPac(int,CORRTEMPLATE*,float*,int,int,int);

/*    正規化相関サーチセットアップ   */
int	CCONV vpxAllocCorrTemplate(int);
void CCONV vpxFreeCorrTemplate(void);
int	CCONV vpxReadCorrTemplate(int,CORRTEMPLATE*);
int	CCONV vpxWriteCorrTemplate(int,CORRTEMPLATE*);
int	CCONV vpxSetCorrTemplate(int,int,int,int);
int	CCONV vpxSetCorrTemplateExt(int,int,int,int);

/*    正規化相関サーチセットアップ(パケットベース)   */
int	CCONV vpxSetCorrTemplatePac(int,CORRTEMPLATE*,int,int);
int	CCONV vpxSetCorrTemplateExtPac(int,CORRTEMPLATE*,int,int);

/*		ＶＰ８１０オリジナル  */
int	CCONV vpxWriteConvertLUT(short *lut);
int	CCONV vpxWriteConvertLUTExt(short *lut,int number,int start);
int	CCONV vpxIP_Rotate(int ImgSrc,int ImgDst,
			float angle,int xc,int yc,int dx,int dy,enum SPACE_OPT opt);
int	CCONV vpxIP_SmoothFLT(int ImgSrc,int ImgDst,int scale,short *COEFF);
int	CCONV vpxIP_EdgeFLT(int ImgSrc,int ImgDst,int scale,short *COEFF);
int	CCONV vpxIP_EdgeFLTAbs(int ImgSrc,int ImgDst,int scale,short *COEFF);
int	CCONV vpxIP_LineFLT(int ImgSrc,int ImgDst,int scale,short *COEFF);
int	CCONV vpxIP_LineFLTAbs(int ImgSrc,int ImgDst,int scale,short *COEFF);
int	CCONV vpxIP_SmoothFLTF(int ImgSrc,int ImgDst,int scale,short *COEFF);
int	CCONV vpxIP_SmoothFLT9x9(int ImgSrc,int ImgDst,int scale,short *COEFF);
int CCONV vpxIP_ExtractLOArea(int ImgSrc,long *Tbl);
int CCONV vpxIP_ExtractLORegionX(int ImgSrc,short *TblMinX,short *TblMaxX);
int CCONV vpxIP_ExtractLORegionY(int ImgSrc,short *TblMinY,short *TblMaxY);
int CCONV vpxIP_HistogramShort(int ImgSrc,short *Tbl,IPGOFeatureTbl *RegTbl,int opt);
int CCONV vpxIP_ProjectGO(int ImgSrc,unsigned short *TblX,unsigned short *TblY,IPGOFeatureTbl *RegTbl);
int CCONV vpxIP_ProjectGOMinValue(int ImgSrc,short *TblX,short *TblY,IPGOFeatureTbl *RegTbl);
int CCONV vpxIP_ProjectGOMaxValue(int ImgSrc,short *TblX,short *TblY,IPGOFeatureTbl *RegTbl);
int CCONV vpxIP_ProjectBO(int ImgSrc,short *TblX,short *TblY);
int CCONV vpxIP_ProjectBORegionX(int ImgSrc,short *TblMinX,short *TblMaxX);
int CCONV vpxIP_ProjectBORegionY(int ImgSrc,short *TblMinY,short *TblMaxY);

int	CCONV vpxInitIP(int cfg,int mode,int opt);

int	CCONV vpxEnableLoopCamera(int ImgID);
int CCONV vpxDisableLoopCamera(enum WaitMode mode);
int CCONV vpxSuspendLoopCamera(enum WaitMode mode);
int CCONV vpxResumeLoopCamera(void);
int CCONV vpxSetCameraSync(enum CameraSync sync);
int CCONV vpxEnableCameraPrefetch(int ImgID0,int ImgID1);
int CCONV vpxDisableCameraPrefetch(void);
int	CCONV vpxResetCameraPrefetch(void);
int	CCONV vpxGetCameraPrefetch(int *ImgID);

/*    ２値化閾値算出支援     */
int CCONV HistAnalyze(HISTANATBL *HistAnaTbl,int lower,int upper);

/*    ハフ変換             */
int CCONV GetHoughLine(POINTTBL* PointTbl,LINERANGE* AngleRange,HOUGHLINE* HoughLine);
int CCONV GetHoughLineRow(POINTTBL* PointTbl,LINERANGE* AngleRange,
                                       HOUGHLINE* HoughLine,int mode);
int CCONV HsGetHoughLine(POINTTBL* PointTbl,LINERANGE* AngleRange,HOUGHLINE* HoughLine,int step);
int CCONV NoGetHoughLine(POINTTBL* PointTbl,LINERANGE* AngleRange,HOUGHLINE* HoughLine);
int	CCONV NoGetHoughLine2nd(POINTTBL* PointTbl,LINERANGE* AngleRange,
                         HOUGHLINE* HoughLine1st,HOUGHLINE* HoughLine2nd);
int	CCONV GetHoughLinePrecise(POINTTBL* PointTbl,LINERANGE* RowRange,HOUGHLINE* HoughLine,int angle);
int	CCONV GetSideHoughLine(POINTTBL* PointTbl,float angle,
                     HOUGHLINE* HoughLine,int thr,int mode);
int CCONV GetHoughLineRowExt(POINTTBL* PointTbl,LINERANGE* AngleRange,
                                  HOUGHLINE* HoughLine,int thr,int mode);

/*    矩形領域算出         */
int	CCONV GetCrossPoint(HOUGHLINE*,HOUGHLINE*,FPOINT*);
int	CCONV GetRectPoint(HOUGHLINE*,HOUGHLINE*,HOUGHLINE*,HOUGHLINE*,FRECTPOINT*);
int	CCONV GetRectCenter(FPOINT*,FPOINT*,FPOINT*,FPOINT*,FPOINT*);
int	CCONV GetAnglePoint4(FPOINT*,FPOINT*,FPOINT*,FPOINT*,float*);
int	CCONV GetAnglePoint2(FPOINT*,FPOINT*,float*);

/*   キャリパー          */
int	CCONV ProjectLine(int ImgID,LINETBL *linetbl,int mode,LINEWINDOW *LineWindow);
int CCONV LineEdgeFilter(LINETBL *ProjectTbl,LINETBL *EdgeTbl);
int CCONV LineEdgeFilterExt(LINETBL *ProjectTbl,LINETBL *EdgeTbl,
                        int offset,int scale,int size,short *COEFF);
int CCONV LineCaliper(LINETBL *LineTbl,LINECALIPERTBL *CaliperTbl,enum IPCaliperMode mode,int score);
int CCONV CaliperLPtoSP(LINECALIPERTBL* LineCaliperTbl,CALIPERTBL* CaliperTbl,
	                                 LINEWINDOW* LineWindow,CALIBTBL* Calib);
int CCONV GetCaliperScore(LINETBL *LineTbl);
int CCONV SetCaliperWidth(CALIBTBL* Calib,LINEWINDOW* LineWindow,float width,int mode);

/*   エッジファインダー   */
int CCONV LineEdgeFinder(LINETBL* LineTbl,
            LINEEDGEFINDERTBL* EdgeTbl,int mode,int minthd,int minscore,int score);
int CCONV EdgeFinderLPtoSP(LINEEDGEFINDERTBL* LineEdgeTbl,
            EDGEFINDERTBL* EdgeTbl,LINEWINDOW* LineWindow);

/*   リード検査          */
int CCONV WidthFilter(CALIPERTBL* CaliperTbl,ARRANGETBL* ArrangeTbl,LIMITTBL* LimitTbl,
                MMDATA* WidthData,POINTTBL* PointTbl,int mode);
int CCONV CPitchFilter(CALIPERTBL* CaliperTbl,ARRANGETBL* ArrangeTbl,LIMITTBL* LimitTbl,
                 MMDATA* PitchData,PITCHTBL* PitchTbl,POINTTBL* PointTbl,int mode);
int CCONV ArrangeAnalyzer(CALIPERTBL* CaliperTbl,PITCHTBL* PitchTbl,ARRANGETBL* ArrangeTbl,
                    LIMITTBL* LimitTbl,POINTTBL* PointTbl,int mode);
int CCONV GetPointGrave(CALIPERTBL* CaliperTbl,ARRANGETBL* ArrangeTbl,FPOINT* Point,
                  float Pitch,int mode);

int CCONV GetLeadnum(unsigned char *arang,int n);
int CCONV GetLeadoff(unsigned char *arang,int n);
int CCONV GetGridnum(unsigned char *arang,int n,int leadNo);
int CCONV GetLeadloss(unsigned char *arang,int n,int leadNo,int *loss);
int CCONV GridArrange_to_LeadArrange(char *GridArrange,int leadNo,char *LeadArrange);
int CCONV GridArrange_to_HexArrange(char *GridArrange,int leadNo,int offs,unsigned char *arng);

/*    Ｉ／Ｏ入出力           */
int	CCONV InIPPort(int opt);
int	CCONV OutIPPort(int val,int opt);

/*    描画関数            */
int	CCONV PutLine(int ImgID,enum IPDrawLineMode mode,int sx,int sy,int ex,int ey,int col);
int	CCONV PutCross(int ImgID,int mode,int xp,int yp,int col);
int	CCONV PutLineWindow(int ImgID,LINEWINDOW *LineWindow,int col);
int	CCONV PutPolygon(int ImgID,enum IPDrawLineMode mode,WPOINT *point,int num,int col);
int CCONV PutCircle(int ImgID,enum IPDrawLineMode mode,int xp,int yp,int r,int col);
int CCONV PutArc(int ImgID,enum IPDrawLineMode mode,int xp,int yp,
									int sx,int sy,float angle,int col);
int CCONV PutRectangle(int ImgID,enum IPDrawLineMode mode,int xp,int yp,
                           int lengh,int lengv,int round,float angle,int col);
int CCONV PutTriangle(int ImgID,enum IPDrawLineMode mode,int xp,int yp,
                          int lengh,int lengv,int round,float angle,int col);
int CCONV PutDiamond(int ImgID,enum IPDrawLineMode mode,int xp,int yp,
               int lengh,int lengv,int round,float angle,int col);
int CCONV PutCrossRect(int ImgID,enum IPDrawLineMode mode,int xp,int yp,
                 int lengh,int lengv,int width,int round,float angle,int col);
int CCONV PutTwinRectangle(int ImgID,enum IPDrawLineMode mode,int xp,int yp,
                     int lengh,int lengv,int width,int round,float angle,int col);

int CCONV PutArcExt(int ImgID,enum IPDrawLineMode mode,
					float xp,float yp,float sx,float sy,float angle,int col);
void CCONV WriteBitmap(int ImgID,int x,int y,int col);
int	CCONV PutFreeRoundRectangle(int ImgID,enum IPDrawLineMode mode,float xp,float yp,
                           float lengh,float lengv,float* round,float angle,int col);
int	CCONV PutEllipse(int ImgID,enum IPDrawLineMode  mode,float cx,float cy,
                           float width,float height,float angle,int col);

/*    文字描画関数        */
int CCONV PutAnkString(int ImgID,char *string,int mode,int sx,int sy,int col);
int CCONV PutHalfString(int ImgID,char *string,int mode,int sx,int sy,int col);
int CCONV PutAnkChar(int ImgID,char code,int mode,int sx,int sy,int xleng,int yleng,int col);
int CCONV PutHalfChar(int ImgID,char code,int mode,int sx,int sy,int xleng,int yleng,int col);
int CCONV PutPattern(int ImgID,unsigned char *pat,int patsizex,int patsizey,
                         int mode,int sx,int sy,int Hsize,int Vsize,int col);
int CCONV PutKanji(int ImgID,int code,int mode,int sx,int sy,int xleng,int yleng,int col);
int CCONV PutString(int ImgID,char *string,int mode,int sx,int sy,int col);

/*    マウスカーソル描画関数   */
int	CCONV InitIPMouseCursor(void);
int	CCONV PutIPMouseCursor(int,int,int,int,int);
int	CCONV PutIPBackCursor(int);

/*    画面描画関数   */
int	CCONV ClearBitmap(int ImgID,int sx,int sy,int xleng,int yleng,int col);
int	CCONV ClearScreen(int ImgID);
int CCONV ZoomOutBox(int ImgSrc,int ImgDst,int sx,int sy,
		                      int dsx,int dsy,int xlng,int ylng,int xmag,int ymag);

/*    ビットマップ描画関数（VP710互換）    */
int	CCONV BM_PutLine(int BitmapID,enum IPDrawLineMode mode,int sx,int sy,int ex,int ey,int col);
int	CCONV BM_PutCross(int BitmapID,int mode,int xp,int yp,int col);
int	CCONV BM_PutLineWindow(int BitmapID,LINEWINDOW *LineWindow,int col);
int	CCONV BM_PutPolygon(int BitmapID,enum IPDrawLineMode mode,WPOINT *point,int num,int col);
int	CCONV BM_PutCircle(int BitmapID,enum IPDrawLineMode mode,int xp,int yp,int r,int col);
int	CCONV BM_PutArc(int BitmapID,enum IPDrawLineMode mode,int xp,int yp,
											int sx,int sy,float angle,int col);
int	CCONV CCONV BM_PutRectangle(int BitmapID,enum IPDrawLineMode mode,int xp,int yp,
											int lengh,int lengv,int round,float angle,int col);
int	CCONV BM_PutTriangle(int BitmapID,enum IPDrawLineMode mode,int xp,int yp,
											int lengh,int lengv,int round,float angle,int col);
int	CCONV BM_PutCrossRect(int BitmapID,enum IPDrawLineMode mode,int xp,int yp,
											int lengh,int lengv,int width,int round,float angle,int col);
int	CCONV BM_PutDiamond(int BitmapID,enum IPDrawLineMode mode,int xp,int yp,
											int lengh,int lengv,int round,float angle,int col);
int	CCONV BM_PutTwinRectangle(int BitmapID,enum IPDrawLineMode mode,int xp,int yp,
											int lengh,int lengv,int width,int round,float angle,int col);

/*    ビットマップ文字描画関数（VP710互換）    */
int	CCONV BM_PutAnkString(int BitmapID,char *string,int mode,int sx,int sy,int col);
int	CCONV BM_PutHalfString(int BitmapID,char *string,int mode,int sx,int sy,int col);
int	CCONV BM_PutAnkChar(int BitmapID,char code,int mode,int sx,int sy,int xleng,int yleng,int col);
int	CCONV BM_PutHalfChar(int BitmapID,char code,int mode,int sx,int sy,int xleng,int yleng,int col);

/*   グラフィックメモリ制御（VP710互換）      */
void CCONV BM_ClearBitmap(int sx,int sy,int xleng,int yleng,int col);
void CCONV BM_ClearScreen(void);
void CCONV BM_EnableBitmap(int);
void CCONV BM_DisableBitmap(int);

/*   映像出力制御（VP710互換）       */
void CCONV VP_DispON(void);
void CCONV VP_DispOFF(void);
void CCONV VP_BitmapON(int BitmapID);
void CCONV VP_BitmapOFF(int BitmapID);
int CCONV GetBWCamera(int ImgID);
int	CCONV GetBWCameraExt(int,int,int);
int CCONV DispBWImg(int ImgID);

#if 0
/*    VP710互換      */
int	CCONV SetVideoFrameLA(int,int);
int	CCONV SelectCameraFLD(int,int);
int CCONV DispCameraDirect(void);
int CCONV IP_Label(int ImgSrc,int ImgDst,enum IP_Label_opt opt);
int CCONV IP_LabelwithAreaFLT(int ImgSrc,int ImgDst,int thrmin,int thrmax,
								enum IP_Label_opt opt);
int CCONV SetLabelWorkImg(int ImWork);
#endif

int CCONV HsHistAnalyze(HISTANATBL *HistAnaTbl,int lower,int upper);

/*   テストフラグコントロール  */
int	CCONV ClearIPPortFlag(void);
int	CCONV GetIPPortFlag(void);
int	CCONV SetIPPortFlag(int val);

/*   モジュールコントロール   */
int	CCONV RegistInteliIPModule(int id,unsigned long addr,int opt);
int	CCONV ExecuteInteliIPModule(int module,MODULE_PARAM_TBL *tbl,void* ovl);
int	CCONV InitIPParamTable(MODULE_PARAM_TBL *tbl,int ParamCount,
                                     enum IPModuleType type,int taskid,int opt);
int	CCONV SetIntegerParam(MODULE_PARAM_TBL *tbl,enum IPModuleParamNo paramNo,int val);
int	CCONV SetFloatParam(MODULE_PARAM_TBL *tbl,enum IPModuleParamNo paramNo,float val);
int	CCONV SetParamTable(MODULE_PARAM_TBL *tbl,enum IPModuleParamNo paramNo,void *pctbl,int n);
int	CCONV AllocParamTable(MODULE_PARAM_TBL *tbl,enum IPModuleParamNo paramNo,int n);
int	CCONV GetParamTable(MODULE_PARAM_TBL *tbl,enum IPModuleParamNo paramNo,void *pctbl,int n);
int	CCONV SetParamTableExt(MODULE_PARAM_TBL *tbl,enum IPModuleParamNo paramNo,
                                   unsigned long iptbl_addr,void *pctbl,int n);
int	CCONV AllocParamTableExt(MODULE_PARAM_TBL *tbl,
                           enum IPModuleParamNo paramNo,unsigned long iptbl_addr,int n);
int	CCONV GetParamTableExt(MODULE_PARAM_TBL *tbl,enum IPModuleParamNo paramNo,
                                            unsigned long iptbl_addr,void *pctbl,int n);
int	CCONV SetIPTaskParamTable(int taskID,MODULE_PARAM_TBL *tbl);
int	CCONV StartIPTaskwithParam(int taskID,MODULE_PARAM_TBL *tbl);
int	CCONV WakeupIPTaskwithParam(int taskID,MODULE_PARAM_TBL *tbl,int opt);
int	CCONV WakeupIPTaskExt(int taskID,MODULE_PARAM_TBL *tbl,int opt,void* ovl);
int	CCONV WaitforIPTaskSignal(int, int, int, void*);
int	CCONV CancelIPTaskWait(int, int);

int	CCONV ReadyforWaitIPTaskSignal(int command);
int	CCONV CancelReadyWait(int command);

int	CCONV ReciveIPTaskSignal(int,int,int*,int*,int*,void*,unsigned long);
int	CCONV ResetIPTaskSignal(int,int,int*);

int	CCONV StartIPTaskMessenger(HWND hWnd,unsigned long wMsg,long lEvent);
int	CCONV EndIPTaskMessenger();

/*   ループバック処理   */
int	CCONV IP_ConstLPB(int ImgDst,int sx,int sy,int xlng,int ylng,int col);
int	CCONV IP_CopyLPB(int ImgSrc,int ImgDst,int sx,int sy,int dsx,int dsy,int xlng,int ylng);
int	CCONV DispImgLPB(int ImgID);
int	CCONV ClearBitmapLPB(int ImgID,int sx,int sy,int xlng,int ylng,int col);
int	CCONV ClearScreenLPB(int ImgID);

/*   ＳＣＩコントロール   */
int	CCONV InitSCI(enum SCICh ch,enum SCIBitRate rate,enum SCIChrlng lng,
                enum SCIParity par,enum SCIStopBit stop);
int	CCONV SetSCIBuffer(enum SCICh ch,unsigned long buff,int n);
int	CCONV FlushSCIBuffer(enum SCICh ch);
int	CCONV ReadSCIBuffer(enum SCICh ch,char* buff,int n);
int	CCONV ReadSCIBufferWithTimeout(enum SCICh ch,char* buff,int n,long timeout);
int	CCONV WriteSCIBuffer(enum SCICh ch,char* buff,int n);
int	CCONV IsSCIBuffer(enum SCICh ch);
int	CCONV IsSCIError(enum SCICh ch);
int	CCONV StartSCI(enum SCICh ch);
int	CCONV TerminateSCI(enum SCICh ch);
int	CCONV IsSCIBufferFull(enum SCICh ch);
int	CCONV IsSCIwtptr(enum SCICh ch);
int	CCONV IsSCIrdptr(enum SCICh ch);

/*   タスクコントロール */
int	CCONV ChangeIPTaskPriority(int taskID,enum IPTaskPriority priority);
int	CCONV EnablePIOInterrupt(int taskID);
int	CCONV DisablePIOInterrupt(int taskID);
int	CCONV SleepIPTask(void);
int	CCONV WaitIPTask(long time);
int	CCONV GetIPTaskStatus(int taskID,RTSK_INFO* info);
int	CCONV WaitSemaphore(int semid);
int	CCONV SignalSemaphore(int semid);
int	CCONV SemStatus(int semid,long *semcnt,short *wtskid);
int	CCONV CancelWakeup(int);
int	CCONV CreateIPSemaphore(CSEM_INFO *info);
int	CCONV DeleteIPSemaphore(int semid);
int	CCONV WaitSemaphoreWithTimeOut(int semid,int tmout);
int	CCONV ReadSemaphore(int semid,RSEM_INFO *info);
int	CCONV CreateEventFlag(CFLG_INFO *info);
int	CCONV DeleteEventFlag(int flgid);
int	CCONV SetEventFlag(int flgid,int setptn);
int	CCONV ClearEventFlag(int flgid,int clrptn);
int	CCONV WaitEventFlag(int flgid,unsigned long waiptn,int mode,unsigned long *flgptn);
int	CCONV WaitEventFlagWithTimeOut(int flgid,unsigned long waiptn,
				int mode,unsigned long *flgptn,int tmout);
int	CCONV ReadEventFlag(int flgid,RFLG_INFO *info);
int	CCONV LockIPCmd(void);
int	CCONV UnlockIPCmd(void);

/* 画像ファイリング */
int CCONV WriteBMPImg(int ImgID,void *info,char *tbl);
int CCONV ReadBMPImg(int ImgID,char *tbl,enum BITMAP_MODE mode);
int	CCONV WriteBMPImgExt(unsigned long *imreg,void *info,char *tbl);
int	CCONV ReadBMPImgExt(unsigned long *imreg,char *tbl,enum BITMAP_MODE mode);

/*	濃淡画像平均特徴量抽出 */
int	CCONV IP_AverageProjectOnX(int ImgSrc,int leng,
                         float xmag,float ymag,int step,LINETBL* tbl);
int	CCONV IP_AverageProjectOnY(int ImgSrc,int leng,
                         float xmag,float ymag,int step,LINETBL* tbl);
int	CCONV IP_AverageMinMaxValue(int ImgSrc,int xleng,int yleng,
                 float xmag,float ymag,int xstep,int ystep,MIN_MAX_VALUE* tbl);
int CCONV IP_ExtractSumFeatures(int ImgID,IPSumFeatureTbl *tbl,int opt);

/*	時間計測関数  */
int CCONV SetIPSystemTimer(int timerid,int tmode,int smode,void *func);
int CCONV IPSleep(unsigned long wait);
int CCONV startGetIPTime(void);
int	CCONV SetIPTimeMode(enum IPTimeMode mode);

int	CCONV ipxsleep(unsigned long wait);
int	CCONV regsleep(unsigned long wait);

unsigned long CCONV timeGetIPTimeUsec(void);
unsigned long CCONV timeGetIPTimeMsec(void);
unsigned long CCONV timeGetIPTime(void);
int CCONV stopGetIPTime(void);
int CCONV SuspendSystemTimer(int);
int CCONV ResumeSystemTimer(int);

int	CCONV SleepPreciseUsec(unsigned long);

/*	ＲＴＣ関数  */
int	CCONV SetIPTime(IP_SYSTEM_TIME* time);
int	CCONV GetIPTime(IP_SYSTEM_TIME* time);
int	CCONV ResetRTC();
int	CCONV InitRTC(IP_SYSTEM_TIME *time);
int	CCONV SetRTCTime(IP_SYSTEM_TIME* time);
int	CCONV GetRTCTime(IP_SYSTEM_TIME* time);
int	CCONV CheckRTC();

/*    CPUメモリアクセス    */
int	CCONV ReadIPMemDatabyLong(long*, unsigned long, unsigned long);
int	CCONV ReadIPMemDatabyWord(short*, unsigned long, unsigned long);
int	CCONV ReadIPMemDatabyByte(char*, unsigned long, unsigned long);
int	CCONV WriteIPMemDatabyLong(long*, unsigned long, unsigned long);
int	CCONV WriteIPMemDatabyWord(short*, unsigned long, unsigned long);
int	CCONV WriteIPMemDatabyByte(char*, unsigned long, unsigned long);

/* フラッシュメモリファイルアクセス */
#ifndef		__flashdrv_h__
int CCONV fmWriteData(unsigned long addr, unsigned short data);
int CCONV fmChipErase(void);
int CCONV fmSectorErase(unsigned long addr);
int CCONV fmDataPolling(unsigned long addr, unsigned short data);
int CCONV fmErasePolling(unsigned long addr, unsigned long timeout);
unsigned short CCONV fmReadData(unsigned long addr);
void CCONV fmRamReset(void);
int CCONV fmReadDeviceCode(void);
int CCONV fmReadManufactureCode(void);
int CCONV fmWriteDataBuffer(unsigned short *buff, unsigned long addr, unsigned long count);
int CCONV fmReadDataBuffer(unsigned short *buff, unsigned long addr, unsigned long count);
int CCONV fmVerifyDataBuffer(unsigned short *buff, unsigned long addr, unsigned long count);
int	CCONV fmReset(void);
int CCONV fmInit(void);
FM_HFILE CCONV fmOpen(char *filename, int mode);
int CCONV fmClose(FM_HFILE hFlie);
int CCONV fmRead(FM_HFILE hFile, char *buff, unsigned long size);
int CCONV fmWrite(FM_HFILE hFile, char *buff, unsigned long size);
int CCONV fmSeek(FM_HFILE hFile,long offset,int origin);
unsigned long CCONV fmGetFileSize(FM_HFILE hFile);
int CCONV fmDelete(char *filename);
int CCONV fmRename(char *srcfilename, char *dstfilename);
int	CCONV fmChgAttr(char *srcfilename, int attr, int flg);
int	CCONV fmSetAttr(FM_HFILE hFile, int attr, int flg);
int CCONV fmFileList(FM_FLIST *filelist, unsigned int count);
int	CCONV fmFindFile(char *filename);
int CCONV fmFormat(void);
unsigned long CCONV fmDiskSize(void);
unsigned long CCONV fmDiskFree(void);
int CCONV fmLoadProgram(char *filename);
int CCONV fmFileCopy(int, char*, char*);
int	CCONV fmWriteProgram(char *FileName);
int	CCONV fmWriteFileData(char *srcFileName, unsigned long addr);
int	CCONV fmSaveMemData(char *dstFileName, unsigned long addr, unsigned long size);
int CCONV fmReadBMPFileInfo(char *filename,IPBMPInfoTbl *Tbl);
int CCONV fmLoadBMPFile(int ImgID, char *filename);
int CCONV fmSaveBMPFile(int ImgID, char *filename, enum BITMAP_MODE mode);
#endif /* !__flashdrv_h__ */

int	CCONV QueryROMBoot(void);

/*	マルチポートカメラ入力 */
int	CCONV SetCameraPortConfig(int type,int mode,int opt);
int	CCONV SetVideoFrameMltPort(int vpid,enum Interlace mode,enum VideoFrameSize size);
int	CCONV SelectCameraMltPort(int vpid,enum CameraID id,enum CameraType type);
int CCONV GetCameraMltPort(int vpid,int ImgID);
int	CCONV DispCameraMltPort(int vpid,int ImgID);
int	CCONV SetTriggerModeMltPort(int vpid,int mode);
int	CCONV EnableLoopCameraMltPort(int vpid,int ImgID);
int	CCONV SuspendLoopCameraMltPort(int vpid,enum WaitMode mode);
int CCONV GetCameraWithSelectPort(enum CameraID id,enum CameraType type,int ImgID);
int CCONV AttachRGBWorkImg(int imgid);
int CCONV DetachRGBWorkImg(void);
int CCONV TriggerOnCameraMltPort(int vpid);
int CCONV ClearInvalidFrameMltPort(int vpid);
int CCONV SetShutterSpeedMltPort(int vpid,enum ShutterSpeed speed);
int CCONV Get2CameraMltPort(int vpid,int ImgID1,int ImgID2);
int CCONV Get4Camera(int ImgID1,int ImgID2,int ImgID3,int ImgID4);
int	CCONV GetFieldSTSMltPort(int vpid);
int	CCONV SetPartialFrameMltPort(int vpid,int vdly,int vsize);
int CCONV SetPartialLineStartMltPort(int vpid,int en,int sp,int clk,int dly,int wid);
int CCONV SetPartialShutterMltPort(int vpid,int en,int dly,int wid);

/* ダミー関数 */
int	CCONV CloseIPDev();
int	CCONV OpenIPDev(enum IPBoardNo boardNo, int opt);
int	CCONV OpenIPDevExt(int, OPEN_IP_INFO*);
int CCONV ResetIPSys();
int CCONV LoadIPSys(char* filename, int opt);
int	CCONV BootIPSys();
int	CCONV CheckIPVersion(int mode);
int	CCONV GetIPDevNumber(void);
int	CCONV GetIPDevCount(void);
int	CCONV GetOpenIPDevCount(void);
int	CCONV EnumAttachIPDev(int*);
int	CCONV QueryAttachIPDev(int);



unsigned long CCONV GetImPhysicalAddress(int imgid,int cache);
unsigned long CCONV GetImVirtualAddress(int imgid,int cache);
void CCONV CacheFlush(void* addr,unsigned long size);
void CCONV ImCacheFlush(int imgid);

/* 画像処理テスト */
int CCONV IP_InvertAndConvertLUTProjectGOonX( 
		int ImgSrc0,int ImgSrc1,int ImgDst,long *TblX, IPGOFeatureTbl *RegTbl);

/* 画像処理システムエラー処理 */
void CCONV SetIPSysErrRoutine( void (*func)());
int	CCONV GetIPSysErrSts( IPSYSERRSTS*);

/* コンボリューション拡張 */
int	CCONV IP_SmoothFLT5x5Ext(int ,int ,int ,enum IPConvExtOpt,int*);
int	CCONV IP_SmoothFLT7x7Ext(int ,int ,int ,enum IPConvExtOpt,int*);
int	CCONV IP_EdgeFLT5x5Ext(int ,int ,int ,enum IPConvExtOpt,int*);
int	CCONV IP_EdgeFLT7x7Ext(int ,int ,int ,enum IPConvExtOpt,int*);

/* ITRONサービスコール追加 */
int	CCONV DefIPIntHdr( int inhno,DEFHDR_INFO *info);
int	CCONV SetIPRLvl( int device,int level);
int	CCONV CreateMBX( CMBX_INFO*);
int	CCONV DeleteMBX( int);
int CCONV tgetmpl(size_t, void**, int);

/* 分散値抽出コマンド */
int	CCONV IP_ExtractVAR(int ,float* ,float*);
int	CCONV IP_ExtractVARwithMask(int ,int , float* ,float*);
int	CCONV IP_ExtractVARfast(int ,float* ,float*);
int	CCONV IP_ExtractVARfastwithMask(int ,int ,float* ,float*);
int	CCONV IP_ExtractVARfromHM(long *Tbl,float *typ,float *var);
int	CCONV IP_ExtractVARfromHMwithMask(long *Tbl,float *typ,float *var);

/* フラッシュメモリ操作関数 */
int CCONV fmCreateDirectory( char *pathName,int attr);
int CCONV fmRemoveDirectory(char *pathName);
FM_HFIND CCONV fmFindFirstFile(char *pathName,FM_FIND_TBL *tbl);
int CCONV fmFindNextFile(FM_HFIND hFind,FM_FIND_TBL *tbl);
int CCONV fmFindClose(FM_HFIND hFind);
int CCONV fmEnumDriveType(FM_DRIVE_TYPE* drv,int count);
int CCONV fmDriveFormat(char* name);
unsigned long CCONV fmDriveSize(char* name);
unsigned long CCONV fmDriveFree(char* name);
int CCONV fmCopyFile(char* SrcFileName,char* DstFileName);
int CCONV fmMoveFile(char* SrcFileName,char* DstFileName);
int CCONV fmGetFileTime(FM_HFILE hFile, IP_FILE_TIME* time);
int CCONV fmSetFileTime(FM_HFILE hFile, IP_FILE_TIME* time);
int	CCONV fmGetAttr(FM_HFILE hFile, int *attr);
int CCONV fmCreateRAMDisk(char* name,unsigned long size);
int CCONV fmRemoveDisk(char* DriveName);
int CCONV GetIPFileTime(IP_FILE_TIME* FileTime);
int	CCONV FileTimeToIPTime(IP_FILE_TIME* FileTime,IP_SYSTEM_TIME* time);
int	CCONV IPTimeToFileTime(IP_SYSTEM_TIME* time,IP_FILE_TIME* FileTime);
int CCONV FilePathToFileName(char *pathname,char *name);
int CCONV FilePathToPathName(char *pathname,char *name);
int CCONV FilePathToDeviceName(char *pathname,char *name);
int	CCONV fmDefrag(char *name,int mode);
int	CCONV fmSetDiskOpt(char *name,int optno,void *optval,int optlen);
int	CCONV fmGetDiskOpt(char *name,int optno,void *optval,int optlen);

/* システム制御２ */
int	CCONV StartWDT(int mode,int timeout);
int	CCONV StopWDT(void);
int	CCONV GetWDTStatus(void);
int	CCONV ResetWDT(void);

/* ＩＯコマンド */
int	CCONV SetMONISEL(int sel);
int	CCONV SetAOCNT(int ch,int mode,float voltage);
int	CCONV SetCOMMode(int mode);

/* Ｉ２Ｃデバイスアクセス */
int	CCONV ReadI2C(int dev,int adr,unsigned char *data);
int	CCONV WriteI2C(int dev,int adr,unsigned char data);

/* ＳＨアドレス空間アクセス */
int	CCONV ReadMapDevice(unsigned long addr,int size,void *data,int count);
int	CCONV WriteMapDevice(unsigned long addr,int size,void *data,int count);

/* IP7500追加コマンド */
int	CCONV InitPRGCamera(void);
int	CCONV SetVideoFrameExt(enum CameraID id,enum Interlace mode,enum VideoFrameSize size);
int	CCONV GetCameraExt(enum CameraID id,int ImgID,enum GetCameraOpt opt);
int	CCONV SetTrigerModeExt(enum CameraID id,enum TrigerMode mode);
int	CCONV CheckTrigerExt(enum CameraID id);
int	CCONV ReadDI(int *DI);
int	CCONV WriteDO(int DO);
int	CCONV ReadDO(int *DO);

/* 映像入出力制御 */
int	CCONV SetVideoOpt(int mode,int opt,void *optval,int optlen);
int	CCONV GetVideoOpt(int mode,int opt,void *optval,int optlen);
int	CCONV GetInitVideoInfo(INIT_VIDEO_INFO *info);
int	CCONV SetInitVideoInfo(INIT_VIDEO_INFO *info);
int	CCONV SetDPSlaveMode(int mode);
int	CCONV Get2CameraOpt( int ImgID1, int ImgID2 );
int	CCONV vpxEnable2CameraPrefetch( int ImgID0,int ImgID1,int opt);
int	CCONV vpxGet2CameraPrefetch( int *ImgID0,int *ImgID1);

/* 線分化関数 */
int CCONV ExtractPolyline(int ImgID,int sx,int sy,int col,int opt,POLY_TBL* poly,int maxnum);
int CCONV PolyArea(POLY_TBL* poly,double* area);
int CCONV PolyPerim(POLY_TBL* poly,double* perim);
int CCONV PolyGrav(POLY_TBL* poly,float* gx,float* gy);
int CCONV PolyFeatures(POLY_TBL* poly,POLY_FEATURE* tbl,int opt);

/* RGBLUT変換関数 */
HRGBLUT CCONV CreateRGBLUT(int mode,int opt);
int CCONV DeleteRGBLUT(HRGBLUT hLUT);
int CCONV SetRGBLUT(HRGBLUT hLUT,int r,int g,int b,int v);
int CCONV GetRGBLUT(HRGBLUT hLUT,int r,int g,int b,int *v);
unsigned long CCONV GetRGBLUTAddr(HRGBLUT hLUT);
int CCONV GetRGBLUTSize(HRGBLUT hLUT);
int CCONV WriteRGBLUT(HRGBLUT hLUT,void* rgblut,int size);
int CCONV ReadRGBLUT(HRGBLUT hLUT,void* rgblut,int size);
int CCONV IP_ConvertRGBLUT(int ImgSrc,int ImgDst,int opt,HRGBLUT hLUT);
int CCONV IP_ConvertRGBLUTEx(int ImgR,int ImgG,int ImgB,int ImgDst,int opt,HRGBLUT hLUT);

/* Fill Hole関数 */
int CCONV IP_FillHole(int ImgSrc,int ImgDst,int opt,int col,OBJ_AREA_OPT *hole_opt);
int CCONV IP_FillHoleExt(int ImgSrc,int ImgDst,int opt,int col,OBJ_AREA_OPT *hole_opt,OBJ_AREA_OPT *obj_opt);

/* 微分関数 */
int CCONV IP_Prewitt(int ImgSrc,int ImgDst,int gain);
int CCONV IP_PrewittBinarize(int ImgSrc,int ImgDst,int thr);
int CCONV IP_Roberts(int ImgSrc,int ImgDst,int gain);
int CCONV IP_RobertsBinarize(int ImgSrc,int ImgDst,int thr);
int CCONV IP_Gradient(int ImgSrc,int ImgDst,int gain);
int CCONV IP_GradientBinarize(int ImgSrc,int ImgDst,int thr);

/* 特徴量抽出 */
int CCONV IP_HistogramFeatures(int ImgSrc,long* Tbl,HISTOGRAM_FEATURE *RegTbl,int opt);

/* 圧縮 */
int CCONV JpegEncodeImg(int ImgID,int sx,int sy,int w,int h,unsigned char *jbuff,int *jn,int mode);
int CCONV JpegDecodeImg(int ImgID,int sx,int sy,int w,int h,unsigned char *jbuff,int jn,int mode);
int CCONV WriteJPEGImg(int ImgID,void *info,char *tbl,int opt);
int CCONV ReadJPEGImg(int ImgID,char *tbl,int opt,enum BITMAP_MODE mode);
int CCONV WriteJPEGImgExt(int ImgID,unsigned long *imreg,void *info,char *tbl,int opt);
int CCONV ReadJPEGImgExt(int ImgID,unsigned long *imreg,char *tbl,int opt,enum BITMAP_MODE mode);

/* システムオプション設定 */
int	CCONV SetSystemOpt(int mode,int opt,void *optval,int optlen);
int	CCONV GetSystemOpt(int mode,int opt,void *optval,int optlen);

/* システム制御 */
int	CCONV VP_BusyWait();

/* 画像メモリ領域管理 */
int	CCONV ReadIPDataType();

/* タスクコントロール */
int	CCONV SignalIPSemaphore(int semid);

/* ラベリング拡張 */
int CCONV IP_Label4withAreaFLTExt(int,int,long,long,enum IP_Label_opt,int*);
int CCONV IP_Label8withAreaFLTExt(int,int,long,long,enum IP_Label_opt,int*);

// エラーハンドラ登録
int CCONV DefIPErrorHdr(void (*func)(int devid,int ErrorCode,char* ErrorRoutine));

// KPF120CL専用コマンド
int CCONV SetupKPF120CL(int opt);
int CCONV KPF120CL_GetCamera(int ImgID,int mode);

// ビデオデータロードコマンド
int CCONV LoadVideoFile(char *filename,int type,int opt);
int CCONV GetVideoSize(enum VideoFrameSize size,int *xlng,int *ylng);
int CCONV GetDispSize(enum DispFrameSize size,int *xlng,int *ylng);
int CCONV GetFreeSizeToVideoSize(int xlng,int ylng,int opt);
int CCONV GetVideoSizeToImgSize(enum VideoFrameSize size,int opt);
int CCONV SetVideoFrameOpt(void *tbl,int lng);
int CCONV SetVideoFrameOptMltPort(int vpid,void *tbl,int lng);
int CCONV SetDispFrameOpt(void *tbl,int lng);
int CCONV SetDispFrameSize(int itl,int size);
int CCONV GetDispSizeToImgSize(enum DispFrameSize size,int opt);
int CCONV GetImgSize(enum ImageFrameSize size,int *xlng,int *ylng);

#ifdef __cplusplus
}
#endif

#endif /* !__ipxfnc_h__ */

