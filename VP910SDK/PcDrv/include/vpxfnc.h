/*                                                                                        */
/*      +------------------------------------------------------------------------+        */
/*      + Copyright (c) 2000-2004 Renesas Northern Japan Semiconductor,Inc.      +        */
/*      +------------------------------------------------------------------------+        */
/*      Title : VP-900 Serise Image Procesing Function Prototype                          */
/*                                                                                        */
/*                                                                                        */


#ifndef		__vpxfnc_h__
#define		__vpxfnc_h__

#ifdef __cplusplus
extern "C"{
#endif

#ifdef _WIN32
	#define CCONV _stdcall
#else
	#define CCONV 
#endif	/* _WIN32 */


/*    画像処理システムイニシャライズ    */
DEVID CCONV OpenIPDevExt(int, OPEN_IP_INFO*);
DEVID CCONV OpenIPDev(int, int);
int	CCONV CloseIPDev(DEVID);
int	CCONV ResetIPSys(DEVID);
int	CCONV _ResetIPSys(DEVID devID, int mode);
int	CCONV LoadIPSys(DEVID,char*, enum IPFileOption);
int	CCONV BootIPSys(DEVID);
int	CCONV GetOpenIPDevCount(DEVID);
int	CCONV GetIPDevCount(void);
int	CCONV GetIPDevNumber(void);
int	CCONV GetIPSysFilePath(char*);
long CCONV SetIPTimeOut(DEVID,long);
int	CCONV GetIPBoardNumber(void);
int	CCONV GetIPDevError(int);
int	CCONV GetIPDevName(int);
int	CCONV GetDefaultBoardNo(DEVID);
int	CCONV EnumAttachIPDev(int*);
int	CCONV QueryAttachIPDev(int);
int	CCONV GetBootStatus(DEVID);
long CCONV GetIPLastError(DEVID,void*);
int	CCONV RefReglockCtl(DEVID devID);
int	CCONV SetReglockCtl(DEVID devID,int lock);
int	CCONV RefTaskTimeout(DEVID devID,int command);
int	CCONV SetTaskTimeout(DEVID devID,int command,int timeout);
int	CCONV ISPWaitSemaphore(DEVID devID,int command,int timeout);
int	CCONV ISPReleaseSemaphore(DEVID devID,int command);
int	CCONV RefReglockWait(DEVID devID,int command);
int CCONV SetReglockWait(DEVID devID,int command,int wait);
int CCONV EnableErrorPrint(DEVID devID);
int CCONV DisableErrorPrint(DEVID devID);
int CCONV SetStdPort(DEVID devID,int port);
int CCONV SetGraphicsRefreshMode(DEVID devID,int mode);
int CCONV SetResetIPMode(DEVID devID,int mode);
int	CCONV IPDriverStartup(void);

/*    画像処理システムエラー処理    */
void CCONV SetIPSysErrRoutine(DEVID, void (*func)());//ダミー、VP910では使わない
int	CCONV GetIPSysErrSts(DEVID, IPSYSERRSTS*);      //ダミー、VP910では使わない

/*    システムテスト        */
int	CCONV ExceptionTest(DEVID);
int	CCONV EndlessLoopTest(DEVID);

/*    タイマー制御   */
int	CCONV InitIPSystemTimer(DEVID);
int	CCONV SetIPSystemTimer(DEVID, int, int, int, void*);
int	CCONV IPSleep(DEVID, unsigned long);
int	CCONV SetIPTimeMode(DEVID, enum IPTimeMode mode);
int	CCONV startGetIPTime(DEVID);
unsigned long CCONV timeGetIPTime(DEVID);
unsigned long CCONV timeGetIPTimeUsec(DEVID);
unsigned long CCONV timeGetIPTimeMsec(DEVID);
int	CCONV stopGetIPTime(DEVID);
int	CCONV ipxsleep(DEVID, unsigned long);
int CCONV SuspendSystemTimer(DEVID, int);
int CCONV ResumeSystemTimer(DEVID, int);
int	CCONV SleepPreciseUsec(DEVID, unsigned long);

/*    ＲＴＣ関数   */
int	CCONV GetIPTime(DEVID,IP_SYSTEM_TIME*);
int	CCONV SetIPTime(DEVID,IP_SYSTEM_TIME*);
int CCONV ResetRTC(DEVID devID);
int	CCONV InitRTC(DEVID devID,IP_SYSTEM_TIME* time);
int	CCONV SetRTCTime(DEVID devID,IP_SYSTEM_TIME* time);
int	CCONV GetRTCTime(DEVID devID,IP_SYSTEM_TIME* time);
int CCONV CheckRTC(DEVID devID);

/*    CPUメモリアクセス    */
int	CCONV ReadIPMemDatabyLong(DEVID, long*, unsigned long, unsigned long);
int	CCONV ReadIPMemDatabyWord(DEVID, short*, unsigned long, unsigned long);
int	CCONV ReadIPMemDatabyByte(DEVID, char*, unsigned long, unsigned long);
int	CCONV WriteIPMemDatabyLong(DEVID, long*, unsigned long, unsigned long);
int	CCONV WriteIPMemDatabyWord(DEVID, short*, unsigned long, unsigned long);
int	CCONV WriteIPMemDatabyByte(DEVID, char*, unsigned long, unsigned long);

/*    システム制御          */
int	CCONV NopIP(DEVID);
void CCONV DbFlagON(DEVID);
void CCONV DbFlagOFF(DEVID);
long CCONV mallocIP(DEVID, long);
void CCONV mfreeIP(DEVID, long);
int	CCONV GetIPVersionInfo(DEVID, VERSIONINFO*);
int	CCONV GetIPBoardNo(DEVID);
int	CCONV CheckIPVersion(DEVID, int);

/*    構成制御          */
int CCONV InitIP(DEVID);
int CCONV InitIPExt(DEVID);
int	CCONV InitIPError(DEVID, int, int, int, int, int);
int CCONV ReadIPErrorTable(DEVID, IPErrorTbl*);
int CCONV ClearIPError(DEVID);
int	CCONV CheckIPError(DEVID);
int CCONV EnableIPErrorMessage(DEVID);
int CCONV DisableIPErrorMessage(DEVID);
int	CCONV vpxSetIPErrorInfo(DEVID, int,char*);
int CCONV ISP_BusyWait(DEVID, unsigned long);
int CCONV ImgBusyWait(DEVID, unsigned long,int);
int CCONV Img2chBusyWait(DEVID, unsigned long, int,int);
int CCONV ISP_BusyCheck(DEVID, unsigned long*);
int	CCONV ClearImgBusy(DEVID, int);
int CCONV Img3chBusyWait(DEVID, unsigned long, int,int,int);

/*    画像処理サポート     */
int	CCONV WriteConvertLUT(DEVID,int *);
int	CCONV vpxWriteConvertLUT(DEVID, short *);
int	CCONV vpxWriteConvertLUTExt(DEVID, short *, int, int);
int CCONV EnablePipeline(DEVID);
int CCONV DisablePipeline(DEVID);
int CCONV EnableRotateProject(DEVID, float);
int CCONV DisableRotateProject(DEVID);
int CCONV vpxEnableCameraPrefetch(DEVID, int, int);
int CCONV vpxDisableCameraPrefetch(DEVID);
int CCONV vpxResetCameraPrefetch(DEVID);
int	CCONV vpxGetCameraPrefetch(DEVID devID, int *ImgID);
int CCONV GetDispImgID(DEVID);
int CCONV GetBitmapImgID(DEVID);
int CCONV GetLineWindowPoint(LINEWINDOW*, RECTPOINT*);
long CCONV GetImInfo(DEVID, int, unsigned long*, int);
int CCONV EnableIPWindow(DEVID);
int CCONV DisableIPWindow(DEVID);
int	CCONV EnableHMRange(DEVID,int,int);
int	CCONV DisableHMRange(DEVID);

/*    画像メモリ領域管理    */
int CCONV AllocImg(DEVID, enum ImageFrameSize);
int CCONV AllocLockImg(DEVID, enum ImageFrameSize, enum CHID chid, enum PageX_Y, enum PageX_Y);
int CCONV FreeImg(DEVID, int);
int CCONV FreeAllImg(DEVID);
int CCONV SetIPDataType(DEVID, enum DataType);
int CCONV ChangeImgDataType(DEVID, int, enum DataType);
int CCONV SetWindow(DEVID, enum WindowType, int, int, int, int);
int CCONV ReadWindow(DEVID, enum WindowType, int*, int*, int*, int*);
int	CCONV ReadIPWindowLeng(DEVID, enum WindowType, int*, int*);
int CCONV SetAllWindow(DEVID, int, int, int, int);
int CCONV ResetAllWindow(DEVID);
int CCONV ReadImgTable(DEVID, int, IMGTBL*);
int CCONV ReadImgDataType(DEVID, int);
int CCONV AllocWorkImg(DEVID, enum ImageFrameSize);
int CCONV AllocDispImg(DEVID, enum ImageFrameSize, enum DispType);
int CCONV FreeDispImg(DEVID);
int CCONV LockImg(DEVID, int);
int CCONV LockAllImg(DEVID);
int CCONV UnlockImg(DEVID, int);
int CCONV UnlockAllImg(DEVID);

/*    画像メモリアクセス    */
long CCONV ReadImg(DEVID, int, char*, long);
long CCONV WriteImg(DEVID, int, char*, long);
long CCONV ReadImgReverse(DEVID, int, char*, long);
long CCONV WriteImgReverse(DEVID, int,char*, long);
int CCONV SetPixelPointer(DEVID, int);
int CCONV ReadPixel(DEVID, int, int, char*);
int CCONV WritePixel(DEVID, int, int, char);
int CCONV ReadPixelContinue(DEVID, char*);
int CCONV WritePixelContinue(DEVID, char);
int	CCONV ReadImgLine(DEVID, int, LINETBL*, int, int, int, int);
int	CCONV WriteImgLine(DEVID, int, LINETBL*, int, int, int, int);
//int CCONV SetPixelPointerExt(DEVID, int, int, int, int*, int*);
long CCONV ReadImgExt(DEVID, unsigned long*, char*, long);
long CCONV WriteImgExt(DEVID, unsigned long*, char*, long);
long CCONV ReadImgReverseExt(DEVID, unsigned long*, char*, long);
long CCONV WriteImgReverseExt(DEVID, unsigned long*, char*, long);
int	CCONV OpenImg(DEVID,enum WaitMode);
int CCONV OpenImgExt(DEVID,int,enum WaitMode);
int	CCONV CloseImg(DEVID);
int	CCONV RefreshImg(DEVID,int);
int CCONV WriteImgPCMA(DEVID,int);
int CCONV ReadImgPCMA(DEVID,int);
int CCONV WriteImgBWOnly(DEVID,int);
int CCONV ReadImgBWOnly(DEVID,int);


/*    画像入力            */
int CCONV SetVideoFrame(DEVID, enum Interlace, enum VideoFrameSize);
int CCONV SelectCamera(DEVID, enum CameraID, enum CameraType);
int CCONV GetCamera(DEVID, int);
int CCONV GetCameraWithSelectPort(DEVID, enum CameraID, enum CameraType,int);
int	CCONV EnableLoopCamera(DEVID);
int	CCONV vpxEnableLoopCamera(DEVID, int);
int	CCONV DisableLoopCamera(DEVID);
int CCONV vpxDisableLoopCamera(DEVID, enum WaitMode);
int CCONV vpxSuspendLoopCamera(DEVID, enum WaitMode);
int CCONV vpxResumeLoopCamera(DEVID);
int CCONV SetCameraSync(DEVID, enum CameraSync);
int	CCONV SelectCameraType(DEVID, enum CameraType, int);
int CCONV SetVFDelay(DEVID, int, int);
int CCONV EnableCameraFlash(DEVID);
int CCONV DisableCameraFlash(DEVID);
int CCONV SetCameraFlashMode(DEVID, enum FlashMode, int);
int	CCONV SetShutterSpeed(DEVID, enum ShutterSpeed mode);
int	CCONV SetTrigerMode(DEVID, enum TrigerMode mode);
int CCONV Get2Camera(DEVID, int, int);
int	CCONV TriggerOnCamera(DEVID);
int	CCONV ClearInvalidFrame(DEVID);
int	CCONV GetCameraSts(DEVID);
int	CCONV ActiveVideoPort(DEVID,enum VideoPortID vpid);
int	CCONV GetFieldSTS(DEVID);
int	CCONV SetPartialFrame(DEVID devID,int vdly,int vsize);
int	CCONV SetPartialLineStart(DEVID devID,int en,int sp,int clk,int dly,int wid);
int	CCONV SetPartialShutter(DEVID devID,int en,int dly,int wid);
int	CCONV SetVFSize(DEVID devID,int hsize,int vsize);
int	CCONV SetVFSizeMltPort(DEVID devID,int vpid,int hsize,int vsize);
int	CCONV SetVFDelayMltPort(DEVID devID,int vpid,int hdly,int vdly);
int	CCONV WriteVideoLUT(DEVID devID,int *lutP0,int *lutP1,int lng,int mode);
int	CCONV WriteVideoLUTMltPort(DEVID devID,int vpid,int *lutP0,int *lutP1,int lng,int mode);
int	CCONV SetVideoControlOpt(DEVID devID,int mode,int opt1,int opt2);
int	CCONV SetVideoControlOptMltPort(DEVID devID,int vpid,int mode,int opt1,int opt2);
int CCONV ResetCamera(DEVID devID,enum CameraID id,enum CameraType type,int opt);

/*    画像表示             */
int CCONV DispImg(DEVID, int);
int CCONV DispCamera(DEVID);
int	CCONV EnableLoopDisp(DEVID, int);
int CCONV DisableLoopDisp(DEVID, enum WaitMode);
int CCONV SuspendLoopDisp(DEVID, enum WaitMode);
int CCONV ResumeLoopDisp(DEVID);
int CCONV SetDFDelay(DEVID, int, int);
int CCONV SetDispFrame(DEVID, enum Interlace, enum DispFrameSize);
int CCONV SelectDispType(DEVID, enum DispType, int);
int CCONV SetDispMode(DEVID, enum DispMode);
int CCONV NoDisp(DEVID);
int CCONV SetDispWindow(DEVID devID,int sx,int sy,int xmag,int ymag);
int CCONV EnableHIREZDisp(DEVID devID);
int CCONV DisableHIREZDisp(DEVID devID);
int CCONV SetDFSize(DEVID devID,int HSize,int VSize);
int CCONV SetDispVFrame(DEVID devID,int vdly,int vsize);

/*    オーバレイ制御    */
int CCONV BitmapOverlap(DEVID, int);
int	CCONV SetOverlayLUT(DEVID, short*);

/*    画像クリア           */
int CCONV IP_ClearAllImg(DEVID);
int CCONV IP_ClearCHImg(DEVID, enum CHID);
int CCONV IP_ClearImg(DEVID, int);
int CCONV IP_Const(DEVID, int, int);

/*    アフィン変換/画像転送    */
int CCONV IP_Copy(DEVID, int, int);
int	CCONV IP_Zoom(DEVID, int, int, float);
int	CCONV IP_ZoomExt(DEVID, int, int, float, float);
int	CCONV IP_ZoomOut(DEVID, int, int, int);
int	CCONV IP_ZoomOutExt(DEVID, int, int, int, int);
int	CCONV IP_ZoomS(DEVID, int, int, float, int, int, enum SPACE_OPT);
int	CCONV IP_Shift(DEVID, int, int, int, int, enum SPACE_OPT);
int	CCONV IP_Rotate(DEVID,int, int, float, float, int, int, int, int, enum SPACE_OPT);
int	CCONV vpxIP_Rotate(DEVID, int, int, float, int, int, int, int, enum SPACE_OPT);
int CCONV IP_ZoomOutwithFLT( DEVID devID, int ImgSrc, int ImgDst, int mag, int scale, int *COEFF );
int CCONV IP_ZoomOutAndFLT( DEVID devID, int ImgSrc, int ImgDst, int mag, int scale, int *COEFF );
int CCONV IP_ZoomIn( DEVID devID, int ImgSrc, int ImgDst, int mag );
int CCONV IP_ZoomInExt( DEVID devID, int ImgSrc, int ImgDst, int xmag, int ymag );


/*    ２値画素変換        */
int CCONV IP_Binarize(DEVID, int, int, int);
int CCONV IP_BinarizeExt(DEVID, int, int, int, int, int);

/*    画素変換            */
int CCONV IP_Invert(DEVID, int, int);
int CCONV IP_Minus(DEVID, int, int);
int CCONV IP_Abs(DEVID, int, int);
int CCONV IP_AddConst(DEVID, int, int, int);
int CCONV IP_SubConst(DEVID, int, int, int);
int CCONV IP_SubConstAbs(DEVID, int, int, int);
int CCONV IP_MultConst(DEVID, int, int, int, int);
int CCONV IP_MinConst(DEVID, int, int, int);
int CCONV IP_MaxConst(DEVID, int, int, int);
int	CCONV IP_ConvertLUT(DEVID, int, int);
int	CCONV IP_ShiftDown(DEVID, int, int, int);
int	CCONV IP_ShiftUp(DEVID, int, int, int);
int	CCONV IP_AndConst(DEVID, int, int, int);

/*    画像間算術演算      */
int	CCONV IP_Add(DEVID, int, int, int);
int	CCONV IP_Sub(DEVID, int, int, int);
int	CCONV IP_SubAbs(DEVID, int, int, int);
int	CCONV IP_Comb(DEVID, int, int, int, int, int, int);
int	CCONV IP_CombAbs(DEVID, int, int, int, int, int, int);
int	CCONV IP_Mult(DEVID, int, int, int, int);
int	CCONV IP_Average(DEVID, int, int, int);
int	CCONV IP_Min(DEVID, int, int, int);
int	CCONV IP_Max(DEVID, int, int, int);
int	CCONV IP_SubConstAbsAdd(DEVID, int, int, int, int, int, int);
int	CCONV IP_SubConstMultAdd(DEVID, int, int, int, int, int, int);
int	CCONV IP_SubConstMult(DEVID, int, int, int, int, int, int);

/*    画像間論理演算      */
int	CCONV IP_And(DEVID, int, int, int);
int	CCONV IP_Or(DEVID, int, int, int);
int	CCONV IP_Xor(DEVID, int, int, int);
int	CCONV IP_InvertAnd(DEVID, int, int, int);
int	CCONV IP_InvertOr(DEVID, int, int, int);
int	CCONV IP_Xnor(DEVID, int, int, int);

/*    ２値画像形状変換    */
int	CCONV IP_PickNoise4(DEVID, int, int);
int	CCONV IP_PickNoise8(DEVID, int, int);
int	CCONV IP_Outline4(DEVID, int, int);
int	CCONV IP_Outline8(DEVID, int, int);
int	CCONV IP_Dilation4(DEVID, int, int);
int	CCONV IP_Dilation8(DEVID, int, int);
int	CCONV IP_Erosion4(DEVID, int, int);
int	CCONV IP_Erosion8(DEVID, int, int);
int	CCONV IP_Shrink4(DEVID, int, int);
int	CCONV IP_Shrink8(DEVID, int, int);
int	CCONV IP_Thin4(DEVID, int, int);
int	CCONV IP_Thin8(DEVID, int, int);

/*    コンボリューション  */
int	CCONV IP_SmoothFLT(DEVID,int, int, int, int *);
int	CCONV vpxIP_SmoothFLT(DEVID, int, int, int, short*);
int	CCONV IP_EdgeFLT(DEVID,int, int, int, int *);
int	CCONV vpxIP_EdgeFLT(DEVID, int, int, int, short*);
int	CCONV IP_EdgeFLTAbs(DEVID,int, int, int, int *);
int	CCONV vpxIP_EdgeFLTAbs(DEVID, int, int, int, short*);
int	CCONV IP_Lapl4FLT(DEVID, int, int);
int	CCONV IP_Lapl8FLT(DEVID, int, int);
int	CCONV IP_Lapl4FLTAbs(DEVID, int, int);
int	CCONV IP_Lapl8FLTAbs(DEVID, int, int);
int	CCONV IP_LineFLT(DEVID,int, int, int, int *);
int	CCONV vpxIP_LineFLT(DEVID, int, int, int, short*);
int	CCONV IP_LineFLTAbs(DEVID,int, int, int, int *);
int	CCONV vpxIP_LineFLTAbs(DEVID, int, int, int, short*);
int	CCONV IP_SmoothFLTF(DEVID,int, int, int, int *);
int	CCONV vpxIP_SmoothFLTF(DEVID, int, int, int, short*);
int	CCONV IP_SmoothFLT9x9(DEVID, int, int, int, int*);
int	CCONV vpxIP_SmoothFLT9x9(DEVID, int, int, int, short*);

/*    MIN・MAXフィルタ    */
int	CCONV IP_MinFLT(DEVID, int, int, int);
int	CCONV IP_MinFLT4(DEVID, int, int);
int	CCONV IP_MinFLT8(DEVID, int, int);
int	CCONV IP_MaxFLT(DEVID, int, int, int);
int	CCONV IP_MaxFLT4(DEVID, int, int);
int	CCONV IP_MaxFLT8(DEVID, int, int);
int	CCONV IP_LineMinFLT(DEVID, int, int, int);
int	CCONV IP_LineMaxFLT(DEVID, int, int, int);

/*    ランクフィルタ    */
int	CCONV IP_RankFLT(DEVID, int, int, int, int);
int	CCONV IP_Rank4FLT(DEVID, int, int, int);
int	CCONV IP_Rank8FLT(DEVID, int, int, int);
int	CCONV IP_MedFLT(DEVID, int, int, int);
int	CCONV IP_Med4FLT(DEVID, int, int);
int	CCONV IP_Med8FLT(DEVID, int, int);

/*    ラベリング         */
int CCONV IP_Label4(DEVID, int, int, enum IP_Label_opt);
int CCONV IP_Label8(DEVID, int, int, enum IP_Label_opt);
int CCONV IP_Label4withAreaFLT(DEVID, int, int, long, long, enum IP_Label_opt);
int CCONV IP_Label8withAreaFLT(DEVID, int, int,long, long, enum IP_Label_opt);
int CCONV IP_Label4withAreaFLTSort(DEVID, int, int, long, long, enum IP_Label_opt, enum IP_Label_Sort_opt);
int CCONV IP_Label8withAreaFLTSort(DEVID, int, int, long, long, enum IP_Label_opt, enum IP_Label_Sort_opt);
int	CCONV IP_ExtractLORegionX(DEVID,int, int *, int *);
int	CCONV IP_ExtractLORegionY(DEVID,int, int *, int *);
int CCONV IP_ExtractLOArea(DEVID, int, int*);
int CCONV IP_ExtractLOAreaExt(DEVID, int, long*);
int CCONV IP_ExtractLOGravity(DEVID, int, IPLOGravityTbl*);
int CCONV vpxIP_ExtractLORegionX(DEVID, int, short*, short*);
int CCONV vpxIP_ExtractLORegionY(DEVID, int, short*, short*);
int CCONV vpxIP_ExtractLOArea(DEVID, int, long*);

/*    濃淡画像特徴量抽出   */
int CCONV IP_ExtractGOFeatures(DEVID, int, IPGOFeatureTbl*, enum IPGOFeatureOpt);
int CCONV IP_Histogram(DEVID, int, long*, IPGOFeatureTbl*, int);
int	CCONV IP_HistogramShort(DEVID,int, int*, IPGOFeatureTbl *, int);
int CCONV vpxIP_HistogramShort(DEVID, int, short*, IPGOFeatureTbl*, int);
int	CCONV IP_ProjectGO(DEVID,int, int *, int *, IPGOFeatureTbl *);
int CCONV vpxIP_ProjectGO(DEVID,int,unsigned short*,unsigned short*,IPGOFeatureTbl*);
int CCONV IP_ProjectGOonX(DEVID, int, long*, IPGOFeatureTbl*);
int CCONV IP_ProjectGOonY(DEVID, int, long*, IPGOFeatureTbl*);
int	CCONV IP_ProjectGOMinValue(DEVID,int, int *, int *, IPGOFeatureTbl *);
int CCONV vpxIP_ProjectGOMinValue(DEVID, int, short*, short*, IPGOFeatureTbl*);
int	CCONV IP_ProjectGOMaxValue(DEVID,int, int *, int *, IPGOFeatureTbl *);
int CCONV vpxIP_ProjectGOMaxValue(DEVID, int, short*, short*, IPGOFeatureTbl*);
int CCONV IP_ProjectBlockGO(DEVID, int, long*, IPGOFeatureTbl*, IPDivideTbl*);
int CCONV IP_ProjectBlockGOMinMaxValue(DEVID, int, IPGOMinMaxTbl*, IPDivideTbl*);
int CCONV IP_ProjectLabelGO(DEVID, int, int, long*);
int CCONV IP_ProjectLabelGOMinMaxValue(DEVID, int, int, IPGOMinMaxTbl*);

/*    ２値画像特徴量抽出   */
int CCONV IP_ExtractBOFeatures(DEVID, int, IPBOFeatureTbl*, enum IPBOFeatureOpt);
int	CCONV IP_ProjectBO(DEVID,int, int *, int *);
int CCONV vpxIP_ProjectBO(DEVID, int, short*, short*);
int	CCONV IP_ProjectBORegionX(DEVID,int, int *, int *);
int CCONV vpxIP_ProjectBORegionX(DEVID, int, short*, short*);
int	CCONV IP_ProjectBORegionY(DEVID,int, int *, int *);
int CCONV vpxIP_ProjectBORegionY(DEVID, int, short*, short*);
int CCONV IP_ProjectBlockBO(DEVID, int, long*, IPDivideTbl*);

/*    正規化相関サーチ制御   */
int	CCONV vpxEnableCorrMask(DEVID);
int	CCONV vpxDisableCorrMask(DEVID);
int	CCONV vpxSetCorrBreakThr(DEVID, int, int);
int	CCONV vpxEnableCorrBreak(DEVID);
int	CCONV vpxDisableCorrBreak(DEVID);
int	CCONV vpxSetSearchDistance(DEVID, int, int, int);
int CCONV vpxSetCorrMode(DEVID, enum IPCorrMode);
int CCONV vpxSetCorrPrecise(DEVID, enum IPCorrPrecise);

/*    正規化相関サーチ   */
int	CCONV vpxIP_CorrStep(DEVID, int, int, CORRTBL*, int, int, int);
int	CCONV vpxIP_CorrPoint(DEVID, int, int, CORRTBL*, int, CORRTBL*, int, int, int, int, int);
int	CCONV vpxIP_CorrPrecise(DEVID, int, int, CORRTBL*, FCORRTBL*);
int	CCONV vpxIP_CorrExt(DEVID, int, int, CORRTBL*, CORREXT*);
int	CCONV vpxIP_CorrStepDualTmp(DEVID, int, int, int, int, int, int, CORRTBL*, int, int, int);
int	CCONV vpxIP_CorrPointDualTmp(DEVID, int, int, int, int, int,int,
								  CORRTBL*, int, CORRTBL*, int, int, int, int, int);
int	CCONV vpxIP_CorrPrecDualTmp(DEVID, int, int, int, int, int, CORRTBL*, FCORRTBL*);
int	CCONV vpxGetCorrMapSize(DEVID,int,int,int,int,int*,int*,int*);
int	CCONV vpxIP_CorrMap(DEVID,int,int,float*,int,int,int);
int	CCONV vpxIP_CorrStepPac(DEVID, int, CORRTEMPLATE*, CORRTBL*, int, int, int);
int	CCONV vpxIP_CorrPointPac(DEVID, int, CORRTEMPLATE*, CORRTBL*, int, CORRTBL*, int, int, int, int, int);
int	CCONV vpxIP_CorrPrecisePac(DEVID, int, CORRTEMPLATE*, CORRTBL*, FCORRTBL*);
int	CCONV vpxIP_CorrExtPac(DEVID, int, CORRTEMPLATE*, CORRTBL*, CORREXT*);
int	CCONV vpxIP_CorrStepDualTmpPac(DEVID, int, CORRTEMPLATE*, int, int, int,
									CORRTEMPLATE*,CORRTBL*,int,int,int);
int	CCONV vpxIP_CorrPointDualTmpPac(DEVID, int, CORRTEMPLATE*, int, int, int,
									 CORRTEMPLATE*, CORRTBL*,int, CORRTBL*, int, int, int, int, int);
int	CCONV vpxIP_CorrPrecDualTmpPac(DEVID, int, CORRTEMPLATE*, int, int, CORRTEMPLATE*, CORRTBL*, FCORRTBL*);
int	CCONV vpxGetCorrMapSizePac(DEVID,int,CORRTEMPLATE*,int,int,int*,int*,int*);
int	CCONV vpxIP_CorrMapPac(DEVID,int,CORRTEMPLATE*,float*,int,int,int);

/*    正規化相関サーチセットアップ   */
int	CCONV vpxAllocCorrTemplate(DEVID, int);
void CCONV vpxFreeCorrTemplate(DEVID);
int	CCONV vpxReadCorrTemplate(DEVID, int, CORRTEMPLATE*);
int	CCONV vpxWriteCorrTemplate(DEVID, int, CORRTEMPLATE*);
int	CCONV vpxSetCorrTemplate(DEVID, int, int, int, int);
int	CCONV vpxSetCorrTemplateExt(DEVID, int, int, int, int);
int	CCONV vpxSetCorrTemplatePac(DEVID, int, CORRTEMPLATE*, int, int);
int	CCONV vpxSetCorrTemplateExtPac(DEVID, int, CORRTEMPLATE*, int, int);

/*    ２値画像パイプラインフィルタ  */
int	CCONV IP_TrsPipelineFLT(DEVID, int, int);
int	CCONV SetTrsPipelineFLTMode(DEVID, enum PipelineMode, PipelineStageMode*);

/*    ２値画像マッチングフィルタ  */
int	CCONV IP_BinMatchFLT(DEVID, int, int, int, enum IPBinMatchMode);
int	CCONV SetBinMatchTemplate(DEVID, int*, int);
int	CCONV IP_BinarizePTM3x3(DEVID, int, int, int);
int	CCONV IP_BinarizePTM5x5(DEVID, int, int, int);
int	CCONV IP_BinarizePTM7x7(DEVID, int, int, int);
int	CCONV IP_BinarizePTM9x9(DEVID, int, int, int);

/*    ２値化閾値算出支援         */
int CCONV HistAnalyze(DEVID,HISTANATBL*, int, int);

/*    ハフ変換             */
int CCONV GetHoughLine(DEVID, POINTTBL*, LINERANGE*, HOUGHLINE*);
int CCONV GetHoughLineRow(DEVID, POINTTBL*, LINERANGE*, HOUGHLINE*, int);
int CCONV HsGetHoughLine(DEVID, POINTTBL*, LINERANGE*, HOUGHLINE*, int);
int CCONV NoGetHoughLine(DEVID, POINTTBL*, LINERANGE*, HOUGHLINE*);
int	CCONV NoGetHoughLine2nd(DEVID, POINTTBL*, LINERANGE*, HOUGHLINE*, HOUGHLINE*);
int	CCONV GetHoughLinePrecise(DEVID, POINTTBL*, LINERANGE*, HOUGHLINE*, int);
int	CCONV GetSideHoughLine(DEVID, POINTTBL*, float, HOUGHLINE*, int, int);
int CCONV GetHoughLineRowExt(DEVID, POINTTBL*, LINERANGE*, HOUGHLINE*, int, int);

/*    矩形領域算出         */
int	CCONV GetCrossPoint(DEVID, HOUGHLINE*, HOUGHLINE*, FPOINT*);
int	CCONV GetRectPoint(DEVID, HOUGHLINE*, HOUGHLINE*, HOUGHLINE*,HOUGHLINE*, FRECTPOINT*);
int	CCONV GetRectCenter(DEVID, FPOINT*, FPOINT*, FPOINT*, FPOINT*, FPOINT*);
int	CCONV GetAnglePoint4(DEVID, FPOINT*, FPOINT*, FPOINT*, FPOINT*, float*);
int	CCONV GetAnglePoint2(DEVID, FPOINT*, FPOINT*, float*);

/*   キャリパー          */
int	CCONV ProjectLine(DEVID, int, LINETBL*, int, LINEWINDOW*);
int CCONV LineEdgeFilter(DEVID, LINETBL*, LINETBL*);
int CCONV LineEdgeFilterExt(DEVID, LINETBL*, LINETBL*, int, int, int, short*);
int CCONV LineCaliper(DEVID, LINETBL*, LINECALIPERTBL*, enum IPCaliperMode, int);
int CCONV GetCaliperScore(DEVID, LINETBL*);
int CCONV CaliperLPtoSP(DEVID, LINECALIPERTBL*, CALIPERTBL*, LINEWINDOW*, CALIBTBL*);
int CCONV SetCaliperWidth(DEVID, CALIBTBL*, LINEWINDOW*, float, int);

/*   エッジファインダー  */
int CCONV LineEdgeFinder(DEVID, LINETBL*, LINEEDGEFINDERTBL*, int, int, int, int);
int CCONV EdgeFinderLPtoSP(DEVID, LINEEDGEFINDERTBL*, EDGEFINDERTBL*, LINEWINDOW*);

/*   リード検査          */
int CCONV WidthFilter(DEVID, CALIPERTBL*, ARRANGETBL*, LIMITTBL*, MMDATA*, POINTTBL*, int);
int CCONV CPitchFilter(DEVID, CALIPERTBL*, ARRANGETBL*, LIMITTBL*, MMDATA*, PITCHTBL*, POINTTBL*, int);
int CCONV ArrangeAnalyzer(DEVID, CALIPERTBL*, PITCHTBL*, ARRANGETBL*, LIMITTBL*, POINTTBL*, int);
int CCONV GetPointGrave(DEVID, CALIPERTBL*, ARRANGETBL*, FPOINT*, float, int);
int	CCONV GetLeadnum(unsigned char*, int);
int	CCONV GetLeadoff(unsigned char*, int);
int	CCONV GetGridnum(unsigned char*, int, int);
int	CCONV GetLeadloss(unsigned char*, int, int, int*);
int	CCONV GridArrange_to_LeadArrange(char*, int, char*);
int	CCONV GridArrange_to_HexArrange(char*, int, int, unsigned char*);

/*    Ｉ／Ｏ入出力           */
int	CCONV InIPPort(DEVID, int);
int	CCONV OutIPPort(DEVID, int, int);

/*    描画関数            */
int	CCONV PutLine(DEVID, int, enum IPDrawLineMode, int, int, int, int, int);
int	CCONV PutCross(DEVID, int, int, int, int, int);
int	CCONV PutLineWindow(DEVID, int, LINEWINDOW*, int);
int	CCONV PutPolygon(DEVID, int, enum IPDrawLineMode, WPOINT*, int, int);
int CCONV PutCircle(DEVID, int, enum IPDrawLineMode, int, int, int ,int);
int CCONV PutArc(DEVID, int, enum IPDrawLineMode, int, int, int, int, float, int);
int CCONV PutRectangle(DEVID, int, enum IPDrawLineMode, int, int, int, int, int, float, int);
int CCONV PutTriangle(DEVID, int, enum IPDrawLineMode, int, int, int, int, int, float, int);
int CCONV PutDiamond(DEVID , int, enum IPDrawLineMode, int, int, int, int, int, float, int);
int CCONV PutCrossRect(DEVID, int, enum IPDrawLineMode, int, int, int, int, int, int, float, int);
int CCONV PutTwinRectangle(DEVID, int, enum IPDrawLineMode, int, int, int, int, int, int, float, int);
int CCONV PutArcExt(DEVID, int, enum IPDrawLineMode, float, float, float, float, float, int);
void CCONV WriteBitmap(DEVID, int, int,int, int);
int	CCONV PutFreeRoundRectangle(DEVID, int, enum IPDrawLineMode, float, float, float, float, float*, float, int);
int	CCONV PutEllipse(DEVID,int,enum IPDrawLineMode,float,float,float,float,float,int);

/*    文字描画関数        */
int CCONV PutAnkString(DEVID, int, char*, int, int, int, int);
int CCONV PutHalfString(DEVID, int, char*, int, int, int, int);
int CCONV PutAnkChar(DEVID, int, char, int, int, int, int, int, int);
int CCONV PutHalfChar(DEVID, int, char, int, int, int, int, int, int);
int CCONV PutPattern(DEVID, int, unsigned char*, int, int, int, int, int, int, int, int);
int CCONV PutKanji(DEVID, int, int, int, int, int, int, int, int);
int CCONV PutString(DEVID, int, char*, int, int, int, int);

/*    マウスカーソル描画関数   */
int	CCONV InitIPMouseCursor(DEVID);
int	CCONV PutIPMouseCursor(DEVID, int, int, int, int, int);
int	CCONV PutIPBackCursor(DEVID, int);

/*    画面描画関数   */
int	CCONV ClearBitmap(DEVID, int ,int, int, int, int, int);
int	CCONV ClearScreen(DEVID, int);
int CCONV ZoomOutBox(DEVID devID,int ImgSrc,int ImgDst,int sx,int sy,
		                      int dsx,int dsy,int xlng,int ylng,int xmag,int ymag);

/*    ビットマップ描画関数（VP710互換）   */
int	CCONV BM_PutLine(DEVID, int, int, int, int, int, int, int);
int	CCONV BM_PutCross(DEVID, int, int, int, int, int);
int	CCONV BM_PutLineWindow(DEVID, int, LINEWINDOW*, int);
int	CCONV BM_PutPolygon(DEVID, int, int, WPOINT*, int, int);
int	CCONV BM_PutCircle(DEVID, int, int, int ,int, int, int);
int	CCONV BM_PutArc(DEVID, int, int, int, int, int, int, float, int);
int	CCONV BM_PutRectangle(DEVID, int, int, int, int, int, int, int, float, int);
int	CCONV BM_PutTriangle(DEVID, int, int, int, int, int, int, int, float, int);
int	CCONV BM_PutCrossRect(DEVID, int, int, int, int, int, int, int, int, float, int);
int	CCONV BM_PutDiamond(DEVID, int, int, int, int, int, int, int, float, int);
int	CCONV BM_PutTwinRectangle(DEVID, int, int, int, int, int, int, int, int, float, int);

/*    文字描画関数（VP710互換）   */
int	CCONV BM_PutAnkString(DEVID, int, char*, int, int, int, int);
int	CCONV BM_PutHalfString(DEVID, int, char*, int, int, int, int);
int	CCONV BM_PutAnkChar(DEVID, int, char, int, int, int, int, int, int);
int	CCONV BM_PutHalfChar(DEVID, int, char, int, int, int, int, int, int);

/*    画面描画関数（VP710互換）   */
void CCONV BM_ClearBitmap(DEVID, int, int, int, int, int);
void CCONV BM_ClearScreen(DEVID);
void CCONV BM_EnableBitmap(DEVID, int);
void CCONV BM_DisableBitmap(DEVID, int);

/*   映像出力制御（VP710互換）    */
void CCONV VP_DispON(DEVID);
void CCONV VP_DispOFF(DEVID);
void CCONV VP_BitmapON(DEVID, int);
void CCONV VP_BitmapOFF(DEVID, int);

/*    ＶＰ７１０ レガシー  */
int CCONV SetVideoFrameLA(DEVID, int, int);
int CCONV SelectCameraFLD(DEVID, int, int);
int CCONV GetBWCamera(DEVID, int);
int CCONV GetBWCameraExt(DEVID, int, int, int);
int CCONV DispBWImg(DEVID, int);
int CCONV DispCameraDirect(DEVID);
int CCONV SetLabelWorkImg(DEVID, int);
int CCONV IP_Label(DEVID, int, int, int);
int CCONV IP_LabelwithAreaFLT(DEVID, int, int, int, int, int);
int CCONV HsHistAnalyze(DEVID,HISTANATBL*, int, int);
int	CCONV SH_TaskRegist(DEVID, int, unsigned long, unsigned long, int);
int	CCONV SH_TaskStart(DEVID, int);
int	CCONV SH_TaskTerminate(DEVID, int);
int	CCONV SH_TaskWakeup(DEVID, int);
int	CCONV SH_TaskResume(DEVID, int);
int	CCONV SH_TaskSuspend(DEVID, int);
int	CCONV SH_TaskDelete(DEVID, int);

/*   テストフラグコントロール  */
int	CCONV ClearIPMouseFlag(DEVID devID);
int	CCONV GetIPMouseFlag(DEVID devID);
int	CCONV ClearIPPortFlag(DEVID devID);
int	CCONV GetIPPortFlag(DEVID devID);
int	CCONV SetIPPortFlag(DEVID devID, int val);

/*    ITRONタスク制御      */
int	CCONV RegistIPTask(DEVID, int, unsigned long, unsigned long, int);
int	CCONV StartIPTask(DEVID, int);
int	CCONV WakeupIPTask(DEVID, int);
int	CCONV TerminateIPTask(DEVID, int);
int	CCONV ResumeIPTask(DEVID, int);
int	CCONV SuspendIPTask(DEVID, int);
int	CCONV DeleteIPTask(DEVID, int);
int	CCONV SetIPTaskParamTable(DEVID, int, MODULE_PARAM_TBL*);
int	CCONV StartIPTaskwithParam(DEVID, int, MODULE_PARAM_TBL*);
int	CCONV WakeupIPTaskwithParam(DEVID, int, MODULE_PARAM_TBL*, int);
int	CCONV WakeupIPTaskExt(DEVID, int,MODULE_PARAM_TBL*, int,void*);
int	CCONV WaitforIPTaskSignal(DEVID, int, int, int, void*);
int	CCONV CancelIPTaskWait(DEVID, int, int);
int	CCONV GetPendingErrorInfo(DEVID);
int	CCONV WakeupTrigger(DEVID, int);
int	CCONV RegistInteliIPModule(DEVID, int, unsigned long,int);
int	CCONV ExecuteInteliIPModule(DEVID, int, MODULE_PARAM_TBL*, void*);
int	CCONV GetIPModuleReturnCode(DEVID);
int	CCONV InitIPParamTable(DEVID, MODULE_PARAM_TBL*, int,enum IPModuleType, int, int);
int	CCONV SetIntegerParam(DEVID, MODULE_PARAM_TBL*l, enum IPModuleParamNo, int);
int	CCONV SetFloatParam(DEVID, MODULE_PARAM_TBL*, enum IPModuleParamNo, float);
int	CCONV SetParamTable(DEVID, MODULE_PARAM_TBL*, enum IPModuleParamNo, void*, int);
int	CCONV AllocParamTable(DEVID, MODULE_PARAM_TBL*, enum IPModuleParamNo, int);
int	CCONV GetParamTable(DEVID, MODULE_PARAM_TBL*, enum IPModuleParamNo, void*, int);
int	CCONV SetParamTableExt(DEVID, MODULE_PARAM_TBL*, enum IPModuleParamNo, unsigned long, void*, int);
int	CCONV AllocParamTableExt(DEVID, MODULE_PARAM_TBL*, enum IPModuleParamNo, unsigned long, int);
int	CCONV GetParamTableExt(DEVID, MODULE_PARAM_TBL*, enum IPModuleParamNo, unsigned long, void*, int);
int	CCONV ChangeIPTaskPriority(DEVID, int, enum IPTaskPriority);
int	CCONV EnablePIOInterrupt(DEVID, int);
int	CCONV DisablePIOInterrupt(DEVID, int);
int	CCONV SignalIPSemaphore(DEVID, int);
void CCONV ExitIPTask( void);
int	CCONV StartIPTaskExt(DEVID, int taskID, int extinfo);

int	CCONV ReadyforWaitIPTaskSignal(DEVID devID,int command);
int	CCONV CancelReadyWait(DEVID devID,int command);

int	CCONV ReciveIPTaskSignal(DEVID,int,int,int*,int*,int*,void*,unsigned long);
int	CCONV ResetIPTaskSignal(DEVID,int,int,int*);

int	CCONV StartIPTaskMessenger(int board,HWND hWnd,unsigned long wMsg,long lEvent);
int	CCONV EndIPTaskMessenger(int board);

/*   ドライバ拡張機能   */
int	CCONV ClearIPInterruptCount(DEVID,int command);
int	CCONV GetIPInterruptCount(DEVID,int command,long*);
int	CCONV IsIPInterrupt(DEVID,int command);
int	CCONV SetISPSystemStatus(DEVID, int, int);
int	CCONV GetISPSystemStatus(DEVID, int, int*);

/*   ループバック処理   */
int	CCONV IP_ConstLPB(DEVID, int, int, int, int, int, int);
int	CCONV IP_CopyLPB(DEVID, int, int, int, int, int, int, int, int);
int CCONV DispImgLPB(DEVID, int);
int	CCONV ClearBitmapLPB(DEVID, int, int, int, int, int, int);
int	CCONV ClearScreenLPB(DEVID, int);

/*   ＳＨ側ダミー関数   */
int	CCONV SendSignaltoPC(int, int, int);
int	CCONV SleepIPTask(void);
int	CCONV WaitIPTask(long);
int	CCONV GetIPTaskStatus(int, RTSK_INFO* info);
int	CCONV CancelWakeup(int);
int	CCONV WaitSemaphore(int);
int	CCONV SignalSemaphore(int);
int	CCONV SemStatus(int, long*, short*);

/*   ＳＣＩコントロール   */
int	CCONV InitSCI(DEVID, enum SCICh, enum SCIBitRate, enum SCIChrlng, enum SCIParity, enum SCIStopBit);
int	CCONV SetSCIBuffer(DEVID, enum SCICh, unsigned long, int);
int	CCONV FlushSCIBuffer(DEVID, enum SCICh);
int	CCONV ReadSCIBuffer(DEVID, enum SCICh, char*, int);
int	CCONV ReadSCIBufferWithTimeout(DEVID, enum SCICh, char*, int, long);
int	CCONV WriteSCIBuffer(DEVID,enum SCICh, char*, int);
int	CCONV IsSCIBuffer(DEVID, enum SCICh);
int	CCONV IsSCIError(DEVID, enum SCICh);
int	CCONV StartSCI(DEVID, enum SCICh);
int	CCONV TerminateSCI(DEVID, enum SCICh);
int	CCONV IsSCIBufferFull(DEVID, enum SCICh);
int	CCONV IsSCIwtptr(DEVID, enum SCICh);
int	CCONV IsSCIrdptr(DEVID, enum SCICh);

/*   Ｉ／Ｏマルチボードコントロール */
int	CCONV EnableMultiIO(DEVID);
int	CCONV DisableMultiIO(DEVID);
void CCONV SetMultiIOSignal(DEVID);
void CCONV ResetMultiIOSignal(DEVID);
void CCONV SetCAMSEL2(DEVID);
void CCONV ResetCAMSEL2(DEVID);
int	CCONV QueryMultiIO(DEVID, unsigned long*);
int	CCONV UseMultiIO(DEVID, int);
int	CCONV SuspendIORequest(DEVID, int, void*);
int	CCONV IP_INPB(DEVID,int, unsigned char*);
int	CCONV IP_OUTPB(DEVID,int, unsigned char);
int	CCONV IP_INPW(DEVID,int, unsigned short*);
int	CCONV IP_OUTPW(DEVID,int, unsigned short);
//int CCONV GetIspPciInfo(DEVID, PISP_INFO_ITEM);
int	CCONV EnableMultiIOControl(DEVID);
int	CCONV DisableMultiIOControl(DEVID);
void CCONV SetCAMSEL3(DEVID);
void CCONV ResetCAMSEL3(DEVID);

/*   各種レジスタセーブ／ロード */
void CCONV SAVE_ISP_REGISTER(unsigned long*);
void CCONV LOAD_ISP_REGISTER(unsigned long*);
void CCONV SAVE_FPU_REGISTER(unsigned long*);
void CCONV LOAD_FPU_REGISTER(unsigned long*);

/*	濃淡画像平均特徴量抽出 */
int	CCONV IP_AverageProjectOnX(DEVID, int, int, float, float, int, LINETBL*);
int	CCONV IP_AverageProjectOnY(DEVID, int, int, float, float, int, LINETBL*);
int	CCONV IP_AverageMinMaxValue(DEVID, int, int, int, float, float, int, int, MIN_MAX_VALUE*);
int CCONV IP_ExtractSumFeatures(DEVID, int, IPSumFeatureTbl*, int);

/* フラッシュメモリファイルアクセス */
int CCONV fmWriteData(DEVID, unsigned long, unsigned short);
unsigned short CCONV fmReadData(DEVID, unsigned long);
int CCONV fmChipErase(DEVID);
int CCONV fmSectorErase(DEVID, unsigned long);
int CCONV fmDataPolling(DEVID, unsigned long,unsigned short);
int CCONV fmErasePolling(DEVID, unsigned long,unsigned long);
void CCONV fmRamReset(DEVID);
int CCONV fmReadDeviceCode(DEVID);
int CCONV fmReadManufactureCode(DEVID);
int CCONV fmWriteDataBuffer(DEVID, unsigned short*, unsigned long,unsigned long);
int CCONV fmReadDataBuffer(DEVID, unsigned short*, unsigned long,unsigned long);
int CCONV fmVerifyDataBuffer(DEVID, unsigned short*, unsigned long,unsigned long);
int CCONV _fmWriteDataBuffer(DEVID, unsigned short*, unsigned long,unsigned long);
int CCONV _fmReadDataBuffer(DEVID, unsigned short*, unsigned long,unsigned long);
int CCONV _fmVerifyDataBuffer(DEVID, unsigned short*, unsigned long,unsigned long);
int	CCONV fmReset(DEVID);
int CCONV fmInit(DEVID);
FM_HFILE CCONV fmOpen(DEVID, char*,int);
int CCONV fmClose(DEVID, FM_HFILE);
int CCONV fmRead(DEVID, FM_HFILE, char*, unsigned long);
int CCONV fmWrite(DEVID, FM_HFILE, char*, unsigned long);
int CCONV _fmRead(DEVID, FM_HFILE, char*, unsigned long);
int CCONV _fmWrite(DEVID, FM_HFILE, char*, unsigned long);
int CCONV fmSeek(DEVID devID,FM_HFILE hFile,long offset,int origin);
unsigned long CCONV fmGetFileSize(DEVID, FM_HFILE);
int CCONV fmDelete(DEVID, char*);
int CCONV fmRename(DEVID, char*, char*);
int	CCONV fmChgAttr(DEVID, char*, int, int);
int	CCONV fmSetAttr(DEVID, FM_HFILE, int, int);
int CCONV fmFileList(DEVID, FM_FLIST*, unsigned int);
int	CCONV fmFindFile(DEVID, char*);
int CCONV fmFormat(DEVID);
unsigned long CCONV fmDiskSize(DEVID);
unsigned long CCONV fmDiskFree(DEVID);
int CCONV fmLoadProgram(DEVID, char*);
int CCONV fmFileCopy(DEVID, int, char*, char*);
int	CCONV fmWriteProgram(DEVID devID, char *FileName);
int	CCONV fmWriteFileData(DEVID devID, char *srcFileName, unsigned long addr);
int	CCONV fmSaveMemData(DEVID devID, char *dstFileName, unsigned long addr, unsigned long size);
int CCONV fmReadBMPFileInfo(DEVID devID, char *filename,IPBMPInfoTbl *Tbl);
int CCONV fmLoadBMPFile(DEVID devID,int ImgID, char *filename);
int CCONV fmSaveBMPFile(DEVID devID,int ImgID, char *filename, enum BITMAP_MODE mode);



/*********************************/
/*          新規コマンド         */
/*********************************/
/* システム制御 */
int CCONV vpxInitIP(DEVID,int,int,int);
/* 画像メモリ管理 */
int	CCONV AllocYUVImg(DEVID,enum ImageFrameSize);
int	CCONV ReadYUVImgTable(DEVID devID,int,IMGTBL*,IMGTBL*);
int	CCONV GetUVImgID(DEVID devID,int);
int	CCONV AllocRGBImg(DEVID devID,enum ImageFrameSize);
int CCONV ReadRGBImgTable(DEVID devID,int,IMGTBL*,IMGTBL*,IMGTBL*);
int CCONV GetGImgID(DEVID devID,int);
int CCONV GetBImgID(DEVID devID,int);
void CCONV CacheFlush(DEVID devID,void*,unsigned long);
void CCONV ImCacheFlush(DEVID ,int);
unsigned long CCONV GetImPhysicalAddress(DEVID ,int,int);
unsigned long CCONV GetImVirtualAddress(DEVID, int ,int );
/* 映像入力 */
int CCONV AttachRGBWorkImg(DEVID devID,int);
int CCONV DetachRGBWorkImg(DEVID devID);
/* マルチポート映像入力 */
int CCONV SetCameraPortConfig(DEVID	devID,int,int,int);
int CCONV SetVideoFrameMltPort(DEVID devID,int,enum Interlace,enum VideoFrameSize);
int CCONV SelectCameraMltPort(DEVID devID,int,enum CameraID,enum CameraType);
int CCONV SetTriggerModeMltPort(DEVID devID,int,enum TrigerMode);
int CCONV EnableLoopCameraMltPort(DEVID devID,int,int);
int CCONV SuspendLoopCameraMltPort(DEVID devID,int,enum WaitMode mode);
int CCONV DispCameraMltPort(DEVID devID,int,int);
int	CCONV GetCameraMltPort(DEVID devID,int,int);
int	CCONV SetShutterSpeedMltPort(DEVID devID,int vpid,enum ShutterSpeed mode);
int	CCONV Get2CameraMltPort(DEVID,int ,int ,int);
int	CCONV GetFieldSTSMltPort(DEVID devID,int vpid);
int	CCONV SetPartialFrameMltPort(DEVID devID,int vpid,int vdly,int vsize);
int CCONV SetPartialLineStartMltPort(DEVID devID,int vpid,int en,int sp,int clk,int dly,int wid);
int CCONV SetPartialShutterMltPort(DEVID devID,int vpid,int en,int dly,int wid);
/* カラー処理 */
int	CCONV IP_Mask(DEVID,int,int,int);
int	CCONV IP_ClearColor(DEVID devID,int);
int	CCONV IP_ConvertRho(DEVID,int,int,int,enum ColorOpt);
int	CCONV IP_ConvertTheta(DEVID,int,int,enum ColorOpt);
int	CCONV IP_ConvertRhoTheta(DEVID,int,int,int,int,enum ColorOpt);
int	CCONV IP_ExtractColor(DEVID,int,int,int,int,int,int,int,int,enum ColorOpt);
int CCONV IP_ExtractColorRhoTheta(DEVID,int,int,int,int,int,int,int,int,enum ColorOpt);
int CCONV IP_ConvertYUVtoRGB(DEVID,int,int,int,int);
int	CCONV IP_ConvertRGBtoYUV(DEVID,int,int,int,int);
/* PIO割込モジュール */
int	CCONV CreateIPTask(DEVID devID,CREATE_TASK_TBL *tbl);
int	CCONV CreateTask(DEVID devID,int tskid,CREATE_TASK_TBL *tbl);
/* モジュールサポート */
int CCONV CreateParamBuffer(DEVID devID,int,unsigned long);
int CCONV DeleteParamBuffer(DEVID devID,int);
int CCONV GetWrkTaskInfo(DEVID devID,int,unsigned long*);
/* タスクコントロール */
int	CCONV CreateInterruptLink(DEVID devID,INT_DEVICE_OBJ*,int,int,int);
int	CCONV DeleteInterruptLink(DEVID devID,INT_DEVICE_OBJ*);
int	CCONV EnableInterruptObject(DEVID devID,INT_DEVICE_OBJ *obj,int opt);
int	CCONV DisableInterruptObject(DEVID devID,INT_DEVICE_OBJ *obj);
int	CCONV GetRegacyPIOTaskID(DEVID devID,int);
int	CCONV CreateIPSemaphore(DEVID devID,CSEM_INFO*);
int	CCONV DeleteIPSemaphore(DEVID devID,int);
int CCONV ReadSemaphore(DEVID devID,int,RSEM_INFO*);
int	CCONV WaitSemaphoreWithTimeOut(DEVID devID,int,int);
int	CCONV CreateEventFlag(DEVID devID,CFLG_INFO*);
int	CCONV DeleteEventFlag(DEVID devID,int flgid);
int CCONV SetEventFlag(DEVID devID,int,int);
int CCONV ClearEventFlag(DEVID devID,int,int);
int CCONV WaitEventFlag(DEVID devID,int,unsigned long,int,unsigned long*);
int CCONV WaitEventFlagWithTimeOut(DEVID devID,int,unsigned long,int,unsigned long*,int);
int CCONV ReadEventFlag(DEVID devID,int,RFLG_INFO*);
/* 画像ファイリング */
int	CCONV ReadBMPFileInfo(char*,IPBMPInfoTbl*);
int	CCONV LoadBMPFile(DEVID,int ,char*);
int	CCONV SaveBMPFile(DEVID,int ,char*,enum BITMAP_MODE);
int CCONV WriteBMPImg(DEVID devID,int ImgID,void *info,char *tbl);
int CCONV ReadBMPImg(DEVID devID,int ImgID,char *tbl,enum BITMAP_MODE mode);
int	CCONV WriteBMPImgExt(DEVID devID,unsigned long *imreg,void *info,char *tbl);
int	CCONV ReadBMPImgExt(DEVID devID,unsigned long *imreg,char *tbl,enum BITMAP_MODE mode);

/* カラー処理 */
int	CCONV IP_ConvertYUVtoRGBfast(DEVID devID,int ImgYUV,int ImgRGB);
int	CCONV IP_ConvertRGBtoYUVfast(DEVID devID,int ImgRGB,int ImgYUV);
int	CCONV IP_ExtractColorHSI(DEVID devID,int ImgRGB,int ImgDst,
                int Hthrmin,int Hthrmax,int Sthrmin,int Sthrmax,int Ithrmin,int Ithrmax);
int	CCONV IP_ConvertHue(DEVID devID,int ImgRGB,int ImgDst);
int	CCONV IP_ConvertSaturation(DEVID devID,int ImgRGB,int ImgDst);
int	CCONV IP_ConvertSaturationExt(DEVID devID,int ImgRGB,int ImgDst);
int	CCONV IP_ConvertIntensity(DEVID devID,int ImgRGB,int ImgDst);
int	CCONV IP_ExtractColorRGB(DEVID devID,int,int,int,int,int,int,int,int);
int	CCONV OpenMultiColor(DEVID devID,enum IPMultiMethod);
int	CCONV CloseMultiColor(DEVID devID,enum IPMultiMethod);
int	CCONV ClearMultiColorYRT(DEVID devID,int);
int	CCONV SetMultiColorYRT(DEVID devID,int,int,int,int,int,int,int,int);
int	CCONV IP_ExtractMultiColorRhoTheta(DEVID devID,int,int,enum ColorOpt,enum MultiColorOpt);
int	CCONV ClearMultiColor(DEVID devID,int);
int	CCONV SetMultiColor(DEVID devID,int,int,int,int,int,int,int,int);
int	CCONV IP_ExtractMultiColor(DEVID devID,int,int,enum ColorOpt,enum MultiColorOpt);
int	CCONV ClearMultiColorHSI(DEVID devID,int);
int	CCONV SetMultiColorHSI(DEVID devID,int,int,int,int,int,int,int,int);
int	CCONV IP_ExtractMultiColorHSI(DEVID devID,int,int,enum MultiColorOpt);
int	CCONV ClearMultiColorRGB(DEVID devID,int);
int	CCONV SetMultiColorRGB(DEVID devID,int,int,int,int,int,int,int,int);
int	CCONV IP_ExtractMultiColorRGB(DEVID devID,int,int,enum MultiColorOpt);

/* 正規化相関(日研) */
int CCONV IP_Corr(DEVID devID, int, int, IPCorrTbl*);
int CCONV SetCorrTemplate(DEVID devID, int, int); 
int CCONV SetCorrTemplateExt(DEVID devID, int, int, int, int); 
int CCONV SetCorrMode(DEVID devID,int, int, float); 
int CCONV EnableCorrMask(DEVID devID);
int CCONV DisableCorrMask(DEVID devID);
int CCONV ReadCorrMask(DEVID devID);
int CCONV SetCorrBreakThr(DEVID devID, int);
int CCONV EnableCorrBreak(DEVID devID);
int CCONV DisableCorrBreak(DEVID devID);
int CCONV ReadCorrBreak(DEVID devID);
int CCONV IP_CorrPrecise(DEVID devID, int, int, int, int, IPCorrPreciseTbl*,enum IPCorrPreciseOpt);
int	CCONV SetCorrControl(DEVID devID,IPCorrControl*);
int	CCONV SetCorrControlExt(DEVID devID,IPCorrControlExt*);
int	CCONV IP_PointCorr(DEVID devID,int,int,CorrSearchTbl*,int);
int	CCONV IP_PointCorrExt(DEVID devID,int,int,CorrSearchTblExt*,int); 
int	CCONV IP_BinarizePTM3x5(DEVID devID,int ,int ,int);
int	CCONV IP_BinarizePTM5x7(DEVID devID,int ,int ,int);
int	CCONV IP_BinarizePTM7x9(DEVID devID,int ,int ,int);
int	CCONV SetOptFlowMode(DEVID devID,enum IP_Opt_SearchMode,IPFlowSize,int,int);
int	CCONV SetOptFlowControl(DEVID devID,int ,float ,float ,float);
int	CCONV IP_OptFlow(DEVID devID,int ,int ,int ,IPFlowPoint*);

/***********************************/
/*         (日研)追加コマンド      */
/***********************************/
/* システム制御 */
float CCONV IPLibVersion(DEVID _devID);
int CCONV IPLibType(DEVID devID,int* ,float* );
int CCONV IPBoardVersion(DEVID devID,IPBoardTable* );
int	CCONV IPBoardType(DEVID devID,int* );
int CCONV DisableIDConflict(DEVID devID);
int CCONV EnableIDConflict(DEVID devID);
int CCONV ReadIDConflict(DEVID devID);
int CCONV ReadIPDataType(DEVID devID);
int CCONV StartIPProfile(DEVID devID);
int CCONV EndIPProfile(DEVID devID, PIPProfileTbl);
int CCONV VP_BusyWait(DEVID devID);

/* 画像メモリ管理 */
int	CCONV AllocImgExt(DEVID devID,enum ImageFrameSize);
int	CCONV AllocOptImg(DEVID devID,enum ImageFrameType,IMG_FRAME_SIZE*);
int	CCONV ReadImgTableType(DEVID devID,int);
int	CCONV SetDefaultWindow(DEVID devID,int ,int ,int ,int);

/* 画像メモリ制御 */
int	CCONV OpenImgDirect(DEVID devID,int ,int* ,int* ,char**);
int	CCONV CloseImgDirect(DEVID devID,int);

/* 映像入力（日研）*/
int CCONV EnableLoopCamera(DEVID);
int CCONV DisableLoopCamera(DEVID);
int CCONV EnableCameraPrefetch(DEVID,int*);
int CCONV DisableCameraPrefetch(DEVID);
int CCONV Enable2CameraPrefetch(DEVID,int*,int*);
int CCONV CheckTriger(DEVID);
int	CCONV Get4Camera(DEVID,int ,int ,int ,int);

/* 映像出力 */
int	CCONV DispImgSnapShot(DEVID devID,int);
int	CCONV DispBWOverlap(DEVID devID,int,enum OverlapMode);
int	CCONV DispOverlap(DEVID devID,int ,enum OverlapMode);
int	CCONV SuspendDisp(DEVID devID,enum WaitMode);
int	CCONV ResumeDisp(DEVID devID);
int	CCONV SelectDisp(DEVID devID,enum DispType);

/* 画像転送アフェイン変換 */
int	CCONV IP_Reverse(DEVID devID,int ,int);
int	CCONV IP_ReverseExt(DEVID devID,int ,int ,float ,float);
int	CCONV IP_Mirror(DEVID devID,int ,int ,enum IPMirrorOpt);
int	CCONV IP_MirrorExt(DEVID devID,int ,int ,enum IPMirrorOpt ,float ,float);

/* 画像換算術演算 */
int	CCONV IP_CombDrop(DEVID devID,int ,int ,int ,int ,int ,int);
int	CCONV IP_Divide(DEVID devID,int ,int ,int ,int);

/* ラベリング */
int	CCONV IP_Label4byRL(DEVID devID,int ImgSrc,int ImgDst,enum IP_Label_opt opt,
									IPLabelBasicTbl *BasicTbl,enum IP_Label_opt2 opt2);
int	CCONV IP_Label8byRL(DEVID devID,int ImgSrc,int ImgDst,enum IP_Label_opt opt,
									IPLabelBasicTbl *BasicTbl, enum IP_Label_opt2 opt2);
int	CCONV IP_Label4byRLwithAreaFLT(DEVID devID,int ImgSrc,int ImgDst,long thrmin,long thrmax,
								 enum IP_Label_opt opt,IPLabelBasicTbl *BasicTbl,enum IP_Label_opt2 opt2);
int	CCONV IP_Label8byRLwithAreaFLT(DEVID devID,int ImgSrc,int ImgDst,long thrmin,long thrmax,
								  enum IP_Label_opt opt,IPLabelBasicTbl *BasicTbl,enum IP_Label_opt2 opt2);
int	CCONV IP_Label4byRLwithAreaFLTSort(DEVID devID,int ImgSrc,int ImgDst,long thrmin,long thrmax, 
								  enum IP_Label_opt opt,enum IP_Label_Sort_opt sort,
							      IPLabelBasicTbl *BasicTbl,enum IP_Label_opt2 opt2 );
int	CCONV IP_Label8byRLwithAreaFLTSort(DEVID devID,int ImgSrc,int ImgDst,long thrmin,long thrmax, 
								  enum IP_Label_opt opt,enum IP_Label_Sort_opt sort,
							      IPLabelBasicTbl *BasicTbl,enum IP_Label_opt2 opt2 );
int	CCONV IP_Label4byRLExt(DEVID devID,int ImgSrc,int ImgDst,enum IP_Label_opt opt,
								 IPLabelExtTbl *ExtTbl, enum IP_Label_opt2 opt2 );
int	CCONV IP_Label8byRLExt(DEVID devID,int ImgSrc,int ImgDst,enum IP_Label_opt opt,
								  IPLabelExtTbl *ExtTbl, enum IP_Label_opt2 opt2);
int	CCONV IP_Label4byRLwithAreaFLTExt(DEVID devID,int ImgSrc,int ImgDst,long thrmin,long thrmax,
								 enum IP_Label_opt opt,IPLabelExtTbl *ExtTbl,enum IP_Label_opt2 opt2);
int	CCONV IP_Label8byRLwithAreaFLTExt(DEVID devID,int ImgSrc,int ImgDst,long thrmin,long thrmax,
								 enum IP_Label_opt opt,IPLabelExtTbl *ExtTbl,enum IP_Label_opt2 opt2);
int	CCONV IP_Label4byRLwithAreaFLTSortExt(DEVID devID,int ImgSrc,int ImgDst,long thrmin,long thrmax, 
								  enum IP_Label_opt opt,enum IP_Label_Sort_opt sort,
							      IPLabelExtTbl *ExtTbl,enum IP_Label_opt2 opt2 );
int	CCONV IP_Label8byRLwithAreaFLTSortExt(DEVID devID,int ImgSrc,int ImgDst,long thrmin,long thrmax, 
								  enum IP_Label_opt opt,enum IP_Label_Sort_opt sort,
								  IPLabelExtTbl *ExtTbl,enum IP_Label_opt2 opt2 );
int	CCONV IP_LabelCombine(DEVID devID,int ImgSrc,int ImgDst,enum IPCombineType type,
                                 IPBinarizeThr *binarizeThr, IPLabelCtl *labelCtl,
                                 IPCombineCtl *combineCtl, IPCombineTbl *combineTbl,
                                 IPCombineTblExt *combineTblExt);
int CCONV SetRunLengthControl( DEVID devID, IPRunLengthControl *cnt );
int CCONV IP_ExtractRunLength( DEVID devID, int ImgSrc, int ImgDst );

/* ヒストグラム */
int	CCONV IP_ExtractBOArea(DEVID devID,int ,long*);
int	CCONV IP_ProjectBlockBO(DEVID devID,int ,long*,IPDivideTbl*);
int	CCONV IP_ProjectBlockGO(DEVID devID,int ,long*,IPGOFeatureTbl*,IPDivideTbl*);
int	CCONV IP_ProjectBlockGOMinMaxValue(DEVID devID,int,IPGOMinMaxTbl*,IPDivideTbl*);
int	CCONV IP_ProjectLabelGO(DEVID devID,int ,int ,long*);
int	CCONV IP_ProjectLabelGOMinMaxValue(DEVID devID,int,int,IPGOMinMaxTbl*);

/* パターン作成(日研) */
int	CCONV SetDrawMode(DEVID,int,enum IPX_DRAW_MODE, enum IPX_DRAW_COLOR);
int	CCONV SetLineAttributes(DEVID,int,enum IPX_LINE_STYLE ,int*, int);
int	CCONV SetStringAttributes(DEVID,enum IPX_STRING_SIZE ,int, int);
int	CCONV RefreshGraphics(DEVID);
int	CCONV DrawString(DEVID,int,int,char*);
int	CCONV DrawLine(DEVID,int,int,int,int);
int	CCONV DrawSegments(DEVID,IPX_SEGMENT*,int);
int	CCONV DrawLines(DEVID,IPX_POINT*,int);
int	CCONV DrawRectangle(DEVID,int,int,int,int);
int	CCONV DrawArc(DEVID,int,int,int,int,int,int);
int	CCONV DrawPolygon(DEVID,IPX_POINT*,int);
int	CCONV FillRectangle(DEVID,int,int,int,int);
int	CCONV FillPolygon(DEVID,IPX_POINT*,int);

/* 直線検出 */
int	CCONV SetHoughControl(DEVID devID,int ,int);
int	CCONV IP_DetectLine(DEVID devID,int ,PredictLine*,DetectLine*,DetectLineControl*);

/* ｺﾝﾎﾞﾘｭｰｼｮﾝ　ﾌｨﾙﾀ拡張 */
int	CCONV IP_SmoothFLT5x5(DEVID devID,int ,int ,int ,int*);
int	CCONV IP_SmoothFLT7x7(DEVID devID,int ,int ,int ,int*);
int	CCONV IP_EdgeFLT5x5(DEVID devID,int ,int ,int ,int*);
int	CCONV IP_EdgeFLT7x7(DEVID devID,int ,int ,int ,int*);
int	CCONV IP_MinFLT5x5(DEVID devID,int ,int ,int);
int	CCONV IP_MinFLT44(DEVID devID,int ,int);
int	CCONV IP_MinFLT48(DEVID devID,int ,int);
int	CCONV IP_MinFLT88(DEVID devID,int ,int);
int	CCONV IP_MaxFLT5x5(DEVID devID,int ,int ,int);
int	CCONV IP_MaxFLT44(DEVID devID,int ,int);
int	CCONV IP_MaxFLT48(DEVID devID,int ,int);
int	CCONV IP_MaxFLT88(DEVID devID,int ,int);
int	CCONV IP_SmoothFLTExt(DEVID devID,int ,int ,enum IPMSmoothMethod, int);
int	CCONV IP_EdgeFLTAbsExt(DEVID devID,int ,int ,enum IPMEdgeMethod ,float , int);

/* 画像処理テスト */
int	CCONV IP_InvertAndConvertLUTProjectGOonX(DEVID devID,
							int ,int ,int ,long*,IPGOFeatureTbl*);
int	CCONV IP_AddNoclip(DEVID devID,int,int,int);
int	CCONV IP_AddShiftDown8(DEVID devID, int, int, int);
int	CCONV IP_SubLUT(DEVID devID, int, int, int);
int	CCONV IP_MultLUT(DEVID devID, int, int, int, int);
int	CCONV IP_MinLUT(DEVID devID, int, int, int);
int	CCONV IP_MaxLUT(DEVID devID, int, int, int);
int	CCONV IP_XnorLUT(DEVID devID, int, int, int);
int	CCONV IP_ConvertLUTMult(DEVID devID, int, int, int, int);
int	CCONV IP_ConvertLUTAnd(DEVID devID, int, int, int);
int	CCONV IP_Sobel(DEVID devID, int, int, int);
int	CCONV IP_SobelBinarize(DEVID devID, int, int, int);
int	CCONV IP_GrayLevelConvert(DEVID devID, int, int, int);
int	CCONV IP_RecursiveFLT(DEVID devID, int, int, int);
int	CCONV IP_RecursiveFLTMax(DEVID devID, int, int, float);
int	CCONV IP_RecursiveFLTMin(DEVID devID, int, int, float);
int	CCONV IP_RecursiveFLTTyp(DEVID devID, int, int, float);
int	CCONV IP_OritaFLT(DEVID devID, int, int, int, int);
int	CCONV IP_ZeroCrossFLT(DEVID devID, int, int, enum IPMZeroXSmoothMethod ,
									int, enum IPMZeroXLapMethod, int, int);
int	CCONV IP_BinarizeNoiseFLT(DEVID devID, int, int, int, int);
int	CCONV GenerateLUTHistogramEQ(DEVID devID, long *Tbl, CNVLUT *lut);
int	CCONV IP_HistogramEQ(DEVID devID, int, int, long*, CNVLUT*);
int	CCONV GenerateLUTHistogramID(DEVID devID, int, int, CNVLUT *);
int	CCONV IP_BinarizePercentile(DEVID devID, int, int, int,
                                       int, long *, enum IPMBinPercentMethod);
int	CCONV IP_PseudoColor(DEVID devID, int, int,unsigned char*,unsigned char*, unsigned char*);
int	CCONV SetPseudoColor(DEVID devID, unsigned char*, unsigned char*, unsigned char*);
int	CCONV SetPseudoColorExt(DEVID devID, unsigned char *, unsigned char *, 
												unsigned char *, PseudoColorTBL);

/* 拡張濃度変換 */								
int	CCONV IP_RegisterLUT(DEVID devID, int, CNVLUT*);		

/* 濃度変換テーブル作成 */
int	CCONV GenerateLUTRecursiveFLT(DEVID devID ,int ,int ,int ,CNVLUT*);
int	CCONV GenerateLUTRecursiveFLTBG(DEVID devID, int, int, int, int, int, CNVLUT*);

/* コンボリュ-ションフィルタ拡張 */
int	CCONV IP_SmoothFLT5x5Ext(DEVID devID,int ,int ,int , enum IPConvExtOpt, int*);
int	CCONV IP_SmoothFLT7x7Ext(DEVID devID,int ,int ,int , enum IPConvExtOpt, int*);
int	CCONV IP_EdgeFLT5x5Ext(DEVID devID,int ,int ,int , enum IPConvExtOpt, int*);
int	CCONV IP_EdgeFLT7x7Ext(DEVID devID,int ,int ,int , enum IPConvExtOpt, int*);

/* 拡張画像処理コマンド */
int	CCONV IP_DivideConst(DEVID,int ,int ,int ,int );
int	CCONV SetExtractEdgeOperater(DEVID ,int*,int*);
int	CCONV IP_ExtractEdgeRhoTheta(DEVID ,int ,int ,int ,int ,int ,
									enum ExtractEdgeMethod ,int);
int	CCONV IP_ExtractEdgeRhoThetaExt(DEVID ,int ,int ,int ,enum ExtractEdgeMethod ,int);

/* ITRONサービスコール追加 */
int	CCONV DefIPIntHdr(DEVID devID,int inhno,DEFHDR_INFO *info);
int	CCONV SetIPRLvl(DEVID devID,int device,int level);
int	CCONV CreateMBX(DEVID ,CMBX_INFO*);
int	CCONV DeleteMBX(DEVID ,int);
int CCONV tgetmpl(DEVID ,size_t,void**, int);
void CCONV reallocmpl(DEVID , void *, size_t );

/* 分散値抽出関数 */
int	CCONV IP_ExtractVAR(DEVID ,int ,float* ,float*);
int	CCONV IP_ExtractVARwithMask(DEVID, int, int,float*, float*);
int	CCONV IP_ExtractVARfast(DEVID ,int ,float* ,float*);
int	CCONV IP_ExtractVARfastwithMask(DEVID, int, int,float*, float*);
int	CCONV IP_ExtractVARfromHM(DEVID devID,long *Tbl,float *typ,float *var);
int	CCONV IP_ExtractVARfromHMwithMask(DEVID devID,long *Tbl,float *typ,float *var);

/* フラッシュメモリ */
int	CCONV fmCreateDirectory(DEVID devID,char *pathName,int attr);
int	CCONV fmRemoveDirectory(DEVID devID,char *pathName);
FM_HFIND CCONV fmFindFirstFile(DEVID devID,char *pathName,FM_FIND_TBL *tbl);
int	CCONV fmFindNextFile(DEVID devID,FM_HFIND hFind,FM_FIND_TBL *tbl);
int	CCONV fmFindClose(DEVID devID,FM_HFIND hFind);
int	CCONV fmEnumDriveType(DEVID devID,FM_DRIVE_TYPE* ,int count);
int	CCONV fmDriveFormat(DEVID devID,char* name);
unsigned long CCONV fmDriveSize(DEVID devID,char* name);
unsigned long CCONV fmDriveFree(DEVID devID,char* name);
int	CCONV fmCopyFile(DEVID devID,char* SrcFileName,char* DstFileName);
int	CCONV fmMoveFile(DEVID devID,char* SrcFileName,char* DstFileName);
int	CCONV fmGetFileTime(DEVID devID,FM_HFILE hFile, IP_FILE_TIME* time);
int	CCONV fmSetFileTime(DEVID devID,FM_HFILE hFile, IP_FILE_TIME* time);
int	CCONV fmGetAttr(DEVID devID,FM_HFILE hFile, int *attr);
int	CCONV fmCreateRAMDisk(DEVID devID,char* name,unsigned long size);
int	CCONV fmRemoveDisk(DEVID devID,char* DriveName);
int	CCONV GetIPFileTime(DEVID devID,IP_FILE_TIME* FileTime);
int	CCONV FileTimeToIPTime(IP_FILE_TIME* FileTime,IP_SYSTEM_TIME* time);
int	CCONV IPTimeToFileTime(IP_SYSTEM_TIME* time,IP_FILE_TIME* FileTime);
int	CCONV FilePathToFileName(char *pathname,char *name);
int	CCONV FilePathToPathName(char *pathname,char *name);
int	CCONV FilePathToDeviceName(char *pathname,char *name);
int	CCONV fmDefrag(DEVID devID,char *name,int mode);
int	CCONV fmSetDiskOpt(DEVID devID,char *name,int optno,void *optval,int optlen);
int	CCONV fmGetDiskOpt(DEVID devID,char *name,int optno,void *optval,int optlen);

/* 画像入力 */
int	CCONV GetCameraSYNC(DEVID devID, enum VideoPortID vpid, int *SyncStatus,
                               int *GC_VSCount, int *VSCount, int *HSCount);

/* システム制御 */
int	CCONV StartWDT(DEVID devID,int mode,int timeout);
int	CCONV StopWDT(DEVID devID);
int	CCONV GetWDTStatus(DEVID devID);
int	CCONV ResetWDT(DEVID devID);

/* 拡張画像処理２ */
int	CCONV IP_AddLUT(DEVID devID, int ImgSrc0, int ImgSrc1, int ImgDst);
int	CCONV IP_SubAbsLUT(DEVID devID, int ImgSrc0, int ImgSrc1, int ImgDst);
int	CCONV IP_XorLUT(DEVID devID,int ImgSrc0, int ImgSrc1, int ImgDst);

/* ＩＯコマンド */
int	CCONV SetMONISEL(DEVID devID, int sel);
int	CCONV SetAOCNT(DEVID devID, int ch,int mode,float voltage);
int	CCONV SetCOMMode(DEVID devID,int mode);

/* Ｉ２Ｃデバイスアクセス */
int	CCONV ReadI2C(DEVID devID,int dev,int adr,unsigned char *data);
int	CCONV WriteI2C(DEVID devID,int dev,int adr,unsigned char data);

/* ＳＨアドレス空間アクセス */
int	CCONV ReadMapDevice(DEVID devID,unsigned long addr,int size,void *data,int count);
int	CCONV WriteMapDevice(DEVID devID,unsigned long addr,int size,void *data,int count);

/* IP7500追加コマンド */
int	CCONV GetLine(DEVID devID,int ImgID,int sx,int sy,int ex,int ey,unsigned char *ImgTbl);
int	CCONV GetLineExt(DEVID devID,int ImgID,int sx,int sy,int ex,int ey,unsigned char *ImgTbl,float *length);
int	CCONV InitPRGCamera(DEVID devID);
int	CCONV GenerateLUTLinear(DEVID devID, float a, float b, CNVLUT *lut );
int	CCONV GenerateLUTCubic(DEVID devID,int stratvalue,float startgrad,int endvalue,float endgrad,CNVLUT *lut);
int	CCONV GenerateLUTGumma(DEVID devID,float gconst,CNVLUT *lut);
int	CCONV GenerateLUTComb(DEVID devID,int n,CNVLUT *lut);
int	CCONV GenerateLUTClip(DEVID devID,int level1,int level2,enum IPMGenerateClipMethod method,CNVLUT *lut);
int	CCONV GenerateLUTEdgeFLTExt(DEVID devID,int level1,int level2,int value,int min,int max,CNVLUT *lut);
int	CCONV SetVideoFrameExt(DEVID devID,enum CameraID id,enum Interlace mode,enum VideoFrameSize size);
int	CCONV GetCameraExt(DEVID devID,enum CameraID id,int ImgID,enum GetCameraOpt opt);
int	CCONV SetTrigerModeExt(DEVID devID,enum CameraID id,enum TrigerMode mode);
int	CCONV CheckTrigerExt(DEVID devID, enum CameraID id);
int	CCONV ReadDI(DEVID devID, int *DI);
int	CCONV WriteDO(DEVID devID, int DO);
int	CCONV ReadDO(DEVID devID, int *DO);

/* 映像入出力制御 */
int	CCONV SetVideoOpt(DEVID devID,int mode,int opt,void *optval,int optlen);
int	CCONV GetVideoOpt(DEVID devID,int mode,int opt,void *optval,int optlen);
int	CCONV GetInitVideoInfo(DEVID devID,INIT_VIDEO_INFO *info);
int	CCONV SetInitVideoInfo(DEVID devID,INIT_VIDEO_INFO *info);
int	CCONV SetDPSlaveMode(DEVID devID,int mode);
int	CCONV Get2CameraOpt(DEVID devID,int ImgID1, int ImgID2 );
int	CCONV vpxEnable2CameraPrefetch(DEVID devID,int ImgID0,int ImgID1,int opt);
int	CCONV vpxGet2CameraPrefetch(DEVID devID,int *ImgID0,int *ImgID1);

/* 線分化関数 */
int CCONV ExtractPolyline(DEVID devID,int ImgID,int sx,int sy,int col,int opt,POLY_TBL* poly,int maxnum);
int CCONV PolyArea(DEVID devID,POLY_TBL* poly,double* area);
int CCONV PolyPerim(DEVID devID,POLY_TBL* poly,double* perim);
int CCONV PolyGrav(DEVID devID,POLY_TBL* poly,float* gx,float* gy);
int CCONV PolyFeatures(DEVID devID,POLY_TBL* poly,POLY_FEATURE* tbl,int opt);

/* RGBLUT変換関数 */
HRGBLUT CCONV CreateRGBLUT(DEVID devID,int mode,int opt);
int CCONV DeleteRGBLUT(DEVID devID,HRGBLUT hLUT);
int CCONV SetRGBLUT(DEVID devID,HRGBLUT hLUT,int r,int g,int b,int v);
int CCONV GetRGBLUT(DEVID devID,HRGBLUT hLUT,int r,int g,int b,int *v);
unsigned long CCONV GetRGBLUTAddr(DEVID devID,HRGBLUT hLUT);
int CCONV GetRGBLUTSize(DEVID devID,HRGBLUT hLUT);
int CCONV WriteRGBLUT(DEVID devID,HRGBLUT hLUT,void* rgblut,int size);
int CCONV ReadRGBLUT(DEVID devID,HRGBLUT hLUT,void* rgblut,int size);
int CCONV IP_ConvertRGBLUT(DEVID devID,int ImgSrc,int ImgDst,int opt,HRGBLUT hLUT);
int CCONV IP_ConvertRGBLUTEx(DEVID devID,int ImgR,int ImgG,int ImgB,int ImgDst,int opt,HRGBLUT hLUT);

/* Fill Hole関数 */
int CCONV IP_FillHole(DEVID devID,int ImgSrc,int ImgDst,int opt,int col,OBJ_AREA_OPT *hole_opt);
int CCONV IP_FillHoleExt(DEVID devID,int ImgSrc,int ImgDst,int opt,int col,OBJ_AREA_OPT *hole_opt,OBJ_AREA_OPT *obj_opt);

/* 微分関数 */
int CCONV IP_Prewitt(DEVID devID,int ImgSrc,int ImgDst,int gain);
int CCONV IP_PrewittBinarize(DEVID devID,int ImgSrc,int ImgDst,int thr);
int CCONV IP_Roberts(DEVID devID,int ImgSrc,int ImgDst,int gain);
int CCONV IP_RobertsBinarize(DEVID devID,int ImgSrc,int ImgDst,int thr);
int CCONV IP_Gradient(DEVID devID,int ImgSrc,int ImgDst,int gain);
int CCONV IP_GradientBinarize(DEVID devID,int ImgSrc,int ImgDst,int thr);

/* 特徴量抽出 */
int CCONV IP_HistogramFeatures(DEVID devID,int ImgSrc,long* Tbl,HISTOGRAM_FEATURE *RegTbl,int opt);

/* 圧縮 */
int CCONV JpegEncodeImg(DEVID devID,int ImgID,int sx,int sy,int w,int h,unsigned char *jbuff,int *jn,int mode);
int CCONV JpegDecodeImg(DEVID devID,int ImgID,int sx,int sy,int w,int h,unsigned char *jbuff,int jn,int mode);
int CCONV WriteJPEGImg(DEVID devID,int ImgID,void *info,char *tbl,int mode);
int CCONV ReadJPEGImg(DEVID devID,int ImgID,char *tbl,int opt,enum BITMAP_MODE mode);
int CCONV WriteJPEGImgExt(DEVID devID,int ImgID,unsigned long *imreg,void *info,char *tbl,int opt);
int CCONV ReadJPEGImgExt(DEVID devID,int ImgID,unsigned long *imreg,char *tbl,int opt,enum BITMAP_MODE mode);

/* システムオプション設定 */
int	CCONV SetSystemOpt(DEVID devID,int mode,int opt,void *optval,int optlen);
int	CCONV GetSystemOpt(DEVID devID,int mode,int opt,void *optval,int optlen);

/* ラベリング拡張 */
int CCONV IP_Label4withAreaFLTExt(DEVID,int,int,long,long,enum IP_Label_opt,int*);
int CCONV IP_Label8withAreaFLTExt(DEVID,int,int,long,long,enum IP_Label_opt,int*);

// エラーハンドラ登録
int CCONV DefIPErrorHdr(DEVID devID,void (*func)(int devid,int ErrorCode,char* ErrorRoutine));

// KPF120CL専用コマンド
int CCONV SetupKPF120CL(DEVID devID,int opt);
int CCONV KPF120CL_GetCamera(DEVID devID,int ImgID,int mode);

// ビデオデータロードコマンド
int CCONV LoadVideoFile(DEVID devID,char *filename,int type,int opt);
int CCONV GetVideoSize(DEVID devID,enum VideoFrameSize size,int *xlng,int *ylng);
int CCONV GetDispSize(DEVID devID,enum DispFrameSize size, int *xlng,int *ylng);
int CCONV GetFreeSizeToVideoSize(DEVID devID,int xlng,int ylng,int opt);
int CCONV GetVideoSizeToImgSize(DEVID devID,enum VideoFrameSize size,int opt);
int CCONV SetVideoFrameOpt(DEVID devID,void *tbl,int lng);
int CCONV SetVideoFrameOptMltPort(DEVID devID,int vpid,void *tbl,int lng);
int CCONV SetDispFrameOpt(DEVID devID,void *tbl,int lng);
int CCONV SetDispFrameSize(DEVID devID,int itl,int size);
int CCONV GetDispSizeToImgSize(DEVID devID,enum DispFrameSize size,int opt);
int CCONV GetImgSize(DEVID devID,enum ImageFrameSize size,int *xlng,int *ylng);

#ifdef __cplusplus
}    
#endif

#ifdef		VP810CFG
	#include	"cnv810.h"
#endif


#endif /* !__vpxfnc_h__ */

