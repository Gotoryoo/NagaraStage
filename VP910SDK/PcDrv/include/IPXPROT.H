#ifndef __IPXPROT_H__
#define __IPXPROT_H__
#include "ipxdef.h"

#include "ipxfunc.h"

/******************************************************************************
		VPSeries Header file
	Copyright (C) 2000-2004 Renesas  Northern Japan Semiconductor,Inc.
	  All rights reserved
******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif /* __cplusplus */


/*******************************************
	CIPAdmin Class プロトタイプ宣言
*******************************************/

/* システム制御 */
int   WINAPI InitIP( );
int   WINAPI InitIPExt( );
int   WINAPI InitPRGCamera( );
int   WINAPI SetIPOperationSpeed( enum ClockSpeedMode mode );
int   WINAPI SetSVOperationMode( enum SVOpreationMode mode );
int   WINAPI SetSVBusReserveMode( enum SVBusReserveMode mode, int xlng );

int   WINAPI ReadIPErrorTable( IPErrorTbl *Tbl );
int   WINAPI ClearIPError( );
int   WINAPI DisableIPErrorMessage( );
int   WINAPI EnableIPErrorMessage( );
int   WINAPI ReadIPErrorMessage( );
int   WINAPI DisableIPErrorLog( );
int   WINAPI EnableIPErrorLog( );
int   WINAPI ReadIPErrorLog( );
int   WINAPI ActiveIP( unsigned int IP_ID );

float WINAPI IPLibVersion( );
int   WINAPI IPLibType( int *Type, float *Version );
int   WINAPI IPBoardVersion( IPBoardTable *Tbl );
int   WINAPI IPBoardType( int *Type );

int   WINAPI DisableIDConflict( );
int   WINAPI EnableIDConflict( );
int   WINAPI ReadIDConflict( );


/* 画像メモリ管理 */
int   WINAPI AllocImg( enum ImageFrameSize size );
int   WINAPI AllocImgExt( enum ImageFrameSize size );
int   WINAPI AllocLockImg( enum ImageFrameSize size, enum CHID chid, enum PageX_Y px, enum PageX_Y py );
int   WINAPI AllocYUVImg( enum ImageFrameSize size );
int   WINAPI AllocOptImg( enum ImageFrameType type, IMG_FRAME_SIZE *size );
int   WINAPI AllocRGBImg( enum ImageFrameSize size );
int   WINAPI FreeImg( int ImgID );
int   WINAPI FreeAllImg( );
int   WINAPI ReadImgTable( int ImgID, IMGTBL *tbl );
int   WINAPI ReadImgTableType( int ImgID );
int   WINAPI ReadYUVImgTable( int ImgYUV, IMGTBL *Ytbl, IMGTBL *UVtbl );
int   WINAPI GetUVImgID( int );
int   WINAPI ReadRGBImgTable( int ImgRGB, IMGTBL *Rtbl, IMGTBL *Gtbl, IMGTBL *Btbl );
int   WINAPI GetGImgID( int ImgRGBID );
int   WINAPI GetBImgID( int ImgRGBID );

int   WINAPI LockImg( int ImgID );
int   WINAPI LockAllImg( );
int   WINAPI UnlockImg( int ImgID );
int   WINAPI UnlockAllImg( );


/*******************************************
	CIPInput Class プロトタイプ宣言
*******************************************/

/* 映像入力制御 */
int   WINAPI SetVideoFrame( enum Interlace, enum VideoFrameSize );
int   WINAPI SetVFDelay( int, int );
int   WINAPI SelectCamera( enum CameraID, enum CameraType );
int   WINAPI GetBWCamera( int );
int   WINAPI GetCamera( int );
int   WINAPI EnableCameraFlash( );
int   WINAPI DisableCameraFlash( );
int   WINAPI SetCameraFlashMode( enum FlashMode, int );
int   WINAPI EnableCameraPrefetch( int * );
int   WINAPI DisableCameraPrefetch( );
int   WINAPI ActiveVideoPort( enum VideoPortID vpid );

int   WINAPI SetTrigerMode( enum TrigerMode );
int   WINAPI CheckTriger( );
int   WINAPI EnableLoopCamera( );
int   WINAPI DisableLoopCamera( );
int   WINAPI Get2Camera( int, int );
int   WINAPI Enable2CameraPrefetch( int *ImgID01, int *ImgID02 );
int   WINAPI VP_BusyWait( );
int   WINAPI GetCameraSYNC( enum VideoPortID vpid, int *SyncStatus, int *GC_VSCount, int *VSCount, int *HSCount );

int   WINAPI SetFlashSignal( enum IPSignalMode mode );
int   WINAPI SetRRSignal( enum IPSignalMode mode );
int   WINAPI SetHspllSignal( enum IPSignalMode mode );
int   WINAPI SetVPVSSignal( enum IPSignalMode mode );
int   WINAPI SetVPHSSignal( enum IPSignalMode mode );
int   WINAPI SetVPFFSignal( enum IPSignalMode mode );
int   WINAPI SetVpclkSignal( enum IPSignalMode mode );

int   WINAPI ReadFlashSignal( );
int   WINAPI ReadRRSignal( );
int   WINAPI ReadHspllSignal( );
int   WINAPI ReadVPVSSignal( );
int   WINAPI ReadVPHSSignal( );
int   WINAPI ReadVPFFSignal( );
int   WINAPI ReadVpclkSignal( );

int   WINAPI SetConfigCamera( enum ConfigType type, ConfigCameraPara *para );
int   WINAPI ReadConfigCamera( enum ConfigType *type, ReadConfigCameraPara *para );
int   WINAPI SetCameraNo( int no );
int   WINAPI ExitIPCamera( );


/*******************************************
	CIPOutput Class プロトタイプ宣言
*******************************************/

/* 映像出力制御 */
int   WINAPI SetDispFrame( enum Interlace, enum DispFrameSize );
int   WINAPI SetDFDelay( int, int );
int   WINAPI SelectDisp( enum DispType );
int   WINAPI SetDispMode( enum DispMode );
int   WINAPI DispBWImg( int );
int   WINAPI DispImg( int );
int   WINAPI DispImgSnapShot( int ImgID );
int   WINAPI DispCameraDirect( );
int   WINAPI DispCamera( );
int   WINAPI DispBWOverlap( int, enum OverlapMode );
int   WINAPI DispOverlap( int, enum OverlapMode );
int   WINAPI NoDisp( );
int   WINAPI SuspendDisp( enum WaitMode );
int   WINAPI ResumeDisp( );

int   WINAPI SetDpclkSignal( enum IPSignalMode mode );
int   WINAPI SetDPVSSignal( enum IPSignalMode mode );
int   WINAPI SetDPHSSignal( enum IPSignalMode mode );
int   WINAPI SetDPFFSignal( enum IPSignalMode mode );

int   WINAPI ReadDpclkSignal( );
int   WINAPI ReadDPVSSignal( );
int   WINAPI ReadDPHSSignal( );
int   WINAPI ReadDPFFSignal( );

int   WINAPI SetConfigDisp( enum ConfigType type, ConfigDispPara *para );
int   WINAPI ReadConfigDisp( enum ConfigType *type, ReadConfigDispPara *para );
int   WINAPI ExitIPView( );
int   WINAPI ReadIPViewWindow( IPViewWindow *window );

int   WINAPI DispImgCombine( int ImgSrc0, int ImgSrc1, enum CombineOpt opt );


/*******************************************
	CIPExec Class プロトタイプ宣言
*******************************************/

/* システム制御 */
int   WINAPI SetIPDataType( enum DataType type );
int   WINAPI ReadIPDataType( );

int   WINAPI StartIPProfile( );
int   WINAPI EndIPProfile( PIPProfileTbl profile );


/* 画像メモリ管理 */
int   WINAPI ChangeImgDataType( int ImgID, enum DataType type );
int   WINAPI ReadImgDataType( int ImgID );

int   WINAPI DisableIPWindow( );
int   WINAPI EnableIPWindow( );
int   WINAPI ReadIPWindow( );
int   WINAPI SetWindow( enum WindowType type, int sx, int sy, int ex, int ey );
int   WINAPI SetAllWindow( int sx, int sy, int ex, int ey );
int   WINAPI ResetAllWindow( );
int   WINAPI ReadWindow( enum WindowType type, int *sx, int *sy, int *ex, int *ey );
int   WINAPI SelectDefaultWindow( enum DefaultIPWindowMode mode );
int   WINAPI ReadDefaultWindowMode( );
int   WINAPI SetDefaultWindow( int sx, int sy, int ex, int ey );


/* 画像メモリ制御 */
int   WINAPI OpenImg( enum WaitMode mode );
int   WINAPI OpenImgExt( int ImgID , enum WaitMode mode );
int   WINAPI CloseImg( );
long  WINAPI ReadImg( int ImgID, unsigned char *ImgTbl, long count );
long  WINAPI WriteImg( int ImgID, unsigned char *ImgTbl, long count );

int   WINAPI SetPixelPointer( int ImgID );
int   WINAPI ReadPixel( int x, int y, char *data );
int   WINAPI WritePixel( int x, int y, char data );
int   WINAPI ReadPixelContinue( char *data );
int   WINAPI WritePixelContinue( char data );

int   WINAPI GetLine( int ImgID, int sx, int sy, int ex, int ey, unsigned char *ImgTbl );

int   WINAPI GetLineExt( int ImgID, int sx, int sy, int ex, int ey, unsigned char *ImgTbl, float *length );

int   WINAPI RefreshImg( int ImgID );
int   WINAPI OpenImgDirect( int ImgID, int *xsize, int *ysize, char **address );
int   WINAPI CloseImgDirect( int ImgID );


/* 画像処理 */
int   WINAPI DisablePipeline( );
int   WINAPI EnablePipeline( );
int   WINAPI ReadPipeline( );

int   WINAPI IP_ClearAllImg( );
int   WINAPI IP_ClearCHImg( enum CHID ch );
int   WINAPI IP_ClearImg( int ImgDst );
int   WINAPI IP_ClearColor( int ImgYUV );

int   WINAPI IP_Const( int ImgDst, int constant );
int   WINAPI IP_Copy( int ImgSrc, int ImgDst );
int   WINAPI IP_Binarize( int ImgSrc, int ImgDst, int thr );
int   WINAPI IP_BinarizeExt( int ImgSrc, int ImgDst, int thrmin, int thrmax, int opt );

int   WINAPI IP_Invert( int, int );
int   WINAPI IP_Minus( int, int );
int   WINAPI IP_Abs( int, int );
int   WINAPI IP_AddConst( int, int, int );
int   WINAPI IP_SubConst( int, int, int );
int   WINAPI IP_SubConstAbs( int, int, int );
int   WINAPI IP_MultConst( int, int, int, int );
int   WINAPI IP_MinConst( int, int, int );
int   WINAPI IP_MaxConst( int, int, int );

int   WINAPI WriteConvLUT( CNVLUT * );
int   WINAPI WriteConvertLUT( CNVLUT * );
int   WINAPI IP_ConvertLUT( int, int );
int   WINAPI IP_ConvetLUT( int, int );

int   WINAPI IP_Zoom( int, int, float );
int   WINAPI IP_ZoomExt( int, int, float, float );
int   WINAPI IP_ZoomS( int, int, float, int, int, enum SPACE_OPT );
int   WINAPI IP_Shift( int, int, int, int, enum SPACE_OPT );

int   WINAPI IP_Add( int, int, int );
int   WINAPI IP_Sub( int, int, int );
int   WINAPI IP_SubAbs( int, int, int );
int   WINAPI IP_Comb( int, int, int, int, int, int );
int   WINAPI IP_CombAbs( int, int, int, int, int, int );
int   WINAPI IP_CombDrop( int ImgSrc0, int ImgSrc1, int ImgDst, int scale, int a, int b );
int   WINAPI IP_Mult( int, int, int, int );
int   WINAPI IP_Average( int, int, int );
int   WINAPI IP_Min( int, int, int );
int   WINAPI IP_Max( int, int, int );
int   WINAPI IP_Divide( int ImgSrc0, int ImgSrc1, int ImgDst, int a );
int   WINAPI IP_DivideConst( int ImgSrc, int ImgDst, int a, int div );

int   WINAPI IP_And( int, int, int );
int   WINAPI IP_Or( int, int, int );
int   WINAPI IP_Xor( int, int, int );
int   WINAPI IP_InvertAnd( int, int, int );
int   WINAPI IP_InvertOr( int, int, int );
int   WINAPI IP_Xnor( int, int, int );

int   WINAPI IP_PickNoise4( int, int );
int   WINAPI IP_PickNoise8( int, int );
int   WINAPI IP_Outline4( int, int );
int   WINAPI IP_Outline8( int, int );
int   WINAPI IP_Dilation4( int, int );
int   WINAPI IP_Dilation8( int, int );
int   WINAPI IP_Erosion4( int, int );
int   WINAPI IP_Erosion8( int, int );

int   WINAPI IP_SmoothFLT( int, int, int, int * );
int   WINAPI IP_SmoothFLTF( int, int, int, int * );
int   WINAPI IP_EdgeFLT( int, int, int, int * );
int   WINAPI IP_EdgeFLTAbs( int, int, int, int * );
int   WINAPI IP_Lapl4FLT( int, int );
int   WINAPI IP_Lapl8FLT( int, int );
int   WINAPI IP_Lapl4FLTAbs( int, int );
int   WINAPI IP_Lapl8FLTAbs( int, int );
int   WINAPI IP_LineFLT( int, int, int, int * );
int   WINAPI IP_LineFLTAbs( int, int, int, int * );

int   WINAPI IP_MinFLT( int, int, int );
int   WINAPI IP_MinFLT4( int, int );
int   WINAPI IP_MinFLT8( int, int );
int   WINAPI IP_MaxFLT( int, int, int );
int   WINAPI IP_MaxFLT4( int, int );
int   WINAPI IP_MaxFLT8( int, int );
int   WINAPI IP_LineMinFLT( int, int, int );
int   WINAPI IP_LineMaxFLT( int, int, int );

int   WINAPI IP_Rotate( int, int, float, float, int, int, int, int, enum SPACE_OPT );

int   WINAPI IP_SubConstAbsAdd( int, int, int, int, int, int );
int   WINAPI IP_SubConstMultAdd( int, int, int, int, int, int );
int   WINAPI IP_SubConstMult( int, int, int, int, int, int );

int   WINAPI IP_RankFLT( int, int, int, int );
int   WINAPI IP_Rank4FLT( int, int, int );
int   WINAPI IP_Rank8FLT( int, int, int );
int   WINAPI IP_MedFLT( int, int, int );
int   WINAPI IP_Med4FLT( int, int );
int   WINAPI IP_Med8FLT( int, int );

int   WINAPI IP_Shrink4( int, int );
int   WINAPI IP_Shrink8( int, int );

int   WINAPI IP_Thin4( int, int );
int   WINAPI IP_Thin8( int, int );

int   WINAPI IP_TrsPipelineFLT( int, int );
int   WINAPI SetTrsPipelineFLTMode( enum PipelineMode, PipelineStageMode * );
int   WINAPI IP_BinMatchFLT( int, int, int, enum IPBinMatchMode );
int   WINAPI SetBinMatchTemplate( int *, int );

int   WINAPI IP_ShiftDown( int, int, int );
int   WINAPI IP_ShiftUp( int, int, int );
int   WINAPI IP_AndConst( int, int, int );

int   WINAPI IP_BinarizePTM3x3( int, int, int );
int   WINAPI IP_BinarizePTM5x5( int, int, int );
int   WINAPI IP_BinarizePTM7x7( int, int, int );
int   WINAPI IP_BinarizePTM9x9( int, int, int );
int   WINAPI IP_BinarizePTM3x5( int, int, int );
int   WINAPI IP_BinarizePTM5x7( int, int, int );
int   WINAPI IP_BinarizePTM7x9( int, int, int );

int   WINAPI IP_Reverse( int, int );
int   WINAPI IP_ReverseExt( int, int, float, float );
int   WINAPI IP_Mirror( int, int, enum IPMirrorOpt );
int   WINAPI IP_MirrorExt( int, int, enum IPMirrorOpt, float, float );

int   WINAPI IP_AddNoclip( int ImgSrc0, int ImgSrc1, int ImgDst );
int   WINAPI IP_AddShiftDown8( int ImgSrc0, int ImgSrc1, int ImgDst );
int   WINAPI WriteConvertLUT2( CNVLUT *lut );
int   WINAPI IP_SubLUT2( int ImgSrc0, int ImgSrc1, int ImgDst );
int   WINAPI IP_XnorLUT2( int ImgSrc0, int ImgSrc1, int ImgDst );
int   WINAPI IP_SubLUT( int ImgSrc0, int ImgSrc1, int ImgDst );
int   WINAPI IP_MultLUT( int ImgSrc0, int ImgSrc1, int ImgDst, int scale );
int   WINAPI IP_MinLUT( int ImgSrc0, int ImgSrc1, int ImgDst );
int   WINAPI IP_MaxLUT( int ImgSrc0, int ImgSrc1, int ImgDst );
int   WINAPI IP_XnorLUT( int ImgSrc0, int ImgSrc1, int ImgDst );
int   WINAPI IP_AddLUT( int ImgSrc0, int ImgSrc1, int ImgDst );
int   WINAPI IP_SubAbsLUT( int ImgSrc0, int ImgSrc1, int ImgDst );
int   WINAPI IP_XorLUT( int ImgSrc0, int ImgSrc1, int ImgDst );
int   WINAPI IP_ConvertLUTMult( int ImgSrc0, int ImgSrc1, int ImgDst, int scale );
int   WINAPI IP_ConvertLUTAnd( int ImgSrc0, int ImgSrc1, int ImgDst );

int   WINAPI IP_SmoothFLT5x5( int ImgSrc, int ImgDst, int scale, int *COEFF );
int   WINAPI IP_SmoothFLT7x7( int ImgSrc, int ImgDst, int scale, int *COEFF );
int   WINAPI IP_EdgeFLT5x5( int ImgSrc, int ImgDst, int scale, int *COEFF );
int   WINAPI IP_EdgeFLT7x7( int ImgSrc, int ImgDst, int scale, int *COEFF );
int   WINAPI IP_MinFLT5x5( int ImgSrc, int ImgDst, int calptn );
int   WINAPI IP_MinFLT44( int ImgSrc, int ImgDst );
int   WINAPI IP_MinFLT48( int ImgSrc, int ImgDst );
int   WINAPI IP_MinFLT88( int ImgSrc, int ImgDst );
int   WINAPI IP_MaxFLT5x5( int ImgSrc, int ImgDst, int calptn );
int   WINAPI IP_MaxFLT44( int ImgSrc, int ImgDst );
int   WINAPI IP_MaxFLT48( int ImgSrc, int ImgDst );
int   WINAPI IP_MaxFLT88( int ImgSrc, int ImgDst );


/* ラベリング */
int   WINAPI SetLabelWorkImg( int );
int   WINAPI IP_Label( int, int, enum IP_Label_opt );
int   WINAPI IP_Label4( int, int, enum IP_Label_opt );
int   WINAPI IP_Label8( int, int, enum IP_Label_opt );
int   WINAPI IP_LabelwithAreaFLT( int, int, int, int, enum IP_Label_opt );
int   WINAPI IP_Label4withAreaFLT( int, int, long, long, enum IP_Label_opt );
int   WINAPI IP_Label8withAreaFLT( int, int, long, long, enum IP_Label_opt );
int   WINAPI IP_Label4withAreaFLTSort( int, int, long, long, enum IP_Label_opt, enum IP_Label_Sort_opt );
int   WINAPI IP_Label8withAreaFLTSort( int, int, long, long, enum IP_Label_opt, enum IP_Label_Sort_opt );

int   WINAPI IP_Label4byRL( int, int, enum IP_Label_opt, IPLabelBasicTbl *, enum IP_Label_opt2 );
int   WINAPI IP_Label8byRL( int, int, enum IP_Label_opt, IPLabelBasicTbl *, enum IP_Label_opt2 );
int   WINAPI IP_Label4byRLwithAreaFLT( int, int, long, long, enum IP_Label_opt, IPLabelBasicTbl *, enum IP_Label_opt2 );
int   WINAPI IP_Label8byRLwithAreaFLT( int, int, long, long, enum IP_Label_opt, IPLabelBasicTbl *, enum IP_Label_opt2 );
int   WINAPI IP_Label4byRLwithAreaFLTSort( int, int, long, long, enum IP_Label_opt, enum IP_Label_Sort_opt, IPLabelBasicTbl *, enum IP_Label_opt2 );
int   WINAPI IP_Label8byRLwithAreaFLTSort( int, int, long, long, enum IP_Label_opt, enum IP_Label_Sort_opt, IPLabelBasicTbl *, enum IP_Label_opt2 );
int   WINAPI IP_Label4byRLExt( int, int, enum IP_Label_opt, IPLabelExtTbl *, enum IP_Label_opt2 );
int   WINAPI IP_Label8byRLExt( int, int, enum IP_Label_opt, IPLabelExtTbl *, enum IP_Label_opt2 );
int   WINAPI IP_Label4byRLwithAreaFLTExt( int, int, long, long, enum IP_Label_opt, IPLabelExtTbl *, enum IP_Label_opt2 );
int   WINAPI IP_Label8byRLwithAreaFLTExt( int, int, long, long, enum IP_Label_opt, IPLabelExtTbl *, enum IP_Label_opt2 );
int   WINAPI IP_Label4byRLwithAreaFLTSortExt( int, int, long, long, enum IP_Label_opt, enum IP_Label_Sort_opt, IPLabelExtTbl *, enum IP_Label_opt2 );
int   WINAPI IP_Label8byRLwithAreaFLTSortExt( int, int, long, long, enum IP_Label_opt, enum IP_Label_Sort_opt, IPLabelExtTbl *, enum IP_Label_opt2 );
int   WINAPI IP_ExtractRunLength( int, int );
int   WINAPI IP_LabelCombine( int, int, enum IPCombineType, IPBinarizeThr *, IPLabelCtl *, IPCombineCtl *, IPCombineTbl *, IPCombineTblExt * );
int   WINAPI SetRunLengthControl( IPRunLengthControl *cnt );


/* ヒストグラム */
int   WINAPI IP_ExtractBOArea( int ImgSrc, long *Area );
int   WINAPI IP_ExtractBOFeatures( int, IPBOFeatureTbl *, enum IPBOFeatureOpt );
int   WINAPI IP_ProjectBO( int, int *, int * );
int   WINAPI IP_ProjectBORegionY( int, int *, int * );
int   WINAPI IP_ProjectBORegionX( int, int *, int * );
int   WINAPI IP_ExtractGOFeatures( int, IPGOFeatureTbl *, enum IPGOFeatureOpt );
int   WINAPI IP_Histogram( int, long *, IPGOFeatureTbl *, int );
int   WINAPI IP_HistogramShort( int, int *, IPGOFeatureTbl *, int );
int   WINAPI IP_ProjectGO( int, int *, int *, IPGOFeatureTbl * );
int   WINAPI IP_ProjectGOonX( int, long *, IPGOFeatureTbl * );
int   WINAPI IP_ProjectGOonY( int, long *, IPGOFeatureTbl * );
int   WINAPI IP_ProjectGOMaxValue( int, int *, int *, IPGOFeatureTbl * );
int   WINAPI IP_ProjectGOMinValue( int, int *, int *, IPGOFeatureTbl * );
int   WINAPI IP_ExtractLOArea( int, int * );
int   WINAPI IP_ExtractLOAreaExt( int, long * );
int   WINAPI IP_ExtractLOGravity( int, IPLOGravityTbl * );
int   WINAPI IP_ExtractLORegionX( int, int *, int * );
int   WINAPI IP_ExtractLORegionY( int, int *, int * );

int   WINAPI EnableRotateProject( float );
int   WINAPI DisableRotateProject( );

int   WINAPI IP_ProjectBlockBO( int, long *, IPDivideTbl * );
int   WINAPI IP_ProjectBlockGO( int, long *, IPGOFeatureTbl *, IPDivideTbl * );
int   WINAPI IP_ProjectBlockGOMinMaxValue( int, IPGOMinMaxTbl *, IPDivideTbl * );
int   WINAPI IP_ProjectLabelGO( int, int, long * );
int   WINAPI IP_ProjectLabelGOMinMaxValue( int, int, IPGOMinMaxTbl * );


/* 正規化相関 */
int   WINAPI SetCorrMode( int SearchMode, int count, float CorrThr );
int   WINAPI DisableCorrMask( );
int   WINAPI EnableCorrMask( );
int   WINAPI ReadCorrMask( );
int   WINAPI SetCorrBreakThr( int thr );
int   WINAPI DisableCorrBreak( );
int   WINAPI EnableCorrBreak( );
int   WINAPI ReadCorrBreak( );
int   WINAPI SetCorrControl( IPCorrControl *cnt );
int   WINAPI SetCorrControlExt( IPCorrControlExt *cnt );

int   WINAPI IP_Corr( int, int, IPCorrTbl * );
int   WINAPI SetCorrTemplate( int, int );
int   WINAPI SetCorrTemplateExt( int, int, int, int );
int   WINAPI IP_CorrPrecise( int, int, int, int, IPCorrPreciseTbl *, enum IPCorrPreciseOpt );
int   WINAPI IP_PointCorr( int ImgSrc, int TmpID, CorrSearchTbl *Tbl, int n );
int   WINAPI IP_PointCorrExt( int ImgSrc, int TmpID, CorrSearchTblExt *Tbl, int n );

int   WINAPI IP_OptFlow( int, int, int, IPFlowPoint * );
int   WINAPI SetOptFlowMode( enum IP_Opt_SearchMode SearchMode, IPFlowSize tempsize, int variance, int thr_c_value );
int   WINAPI SetOptFlowControl( int SearchMode, float SkipThrAdjust1, float SkipThrAdjust2, float CenterMachAdjust  );

int   WINAPI IP_StereoVision( int ImgRight, int ImgLeft, int num, IPFlowPoint *Tbl );
int   WINAPI SetStereoVisionMode( int SearchMode, IPFlowSize tempsize, int variance, int thr_c_value );


/* グラフィックス */
int   WINAPI SetDrawMode( int, enum IPX_DRAW_MODE, enum IPX_DRAW_COLOR );
int   WINAPI SetLineAttributes( int, enum IPX_LINE_STYLE, int *, int );
int   WINAPI SetStringAttributes( enum IPX_STRING_SIZE, int, int );

int   WINAPI DrawString( int, int, char * );

int   WINAPI DrawLine( int, int, int, int );
int   WINAPI DrawSegments( IPX_SEGMENT *, int );
int   WINAPI DrawLines( IPX_POINT *, int );
int   WINAPI DrawRectangle( int, int, int, int );
int   WINAPI DrawPolygon( IPX_POINT *, int );

int   WINAPI DrawArc( int, int, int, int, int, int );

int   WINAPI FillRectangle( int, int, int, int );
int   WINAPI FillPolygon( IPX_POINT *, int );

int   WINAPI FillArc( int, int, int, int, int, int, enum IPX_FILL_MODE );
int   WINAPI FillEllipse( int, int, int, int );


/* 画像ファイリング */
int   WINAPI ReadBMPFileInfo( char *, IPBMPInfoTbl * );
int   WINAPI LoadBMPFile( int, char * );
int   WINAPI SaveBMPFile( int, char *, enum BITMAP_MODE );


/* カーソル表示 */
int   WINAPI PutCursor( int, int, int, int, char * );
int   WINAPI PutCrossCursor( int, int, int, int );
int   WINAPI PutBoxCursor( int, int, int, int );
int   WINAPI EraseCursor( );
int   WINAPI EraseCrossCursor( );
int   WINAPI EraseBoxCursor( );
int   WINAPI InitCursor( int );


/* カラー処理 */
int   WINAPI IP_Mask( int ImgSrc, int ImgMask, int ImgDst );
int   WINAPI IP_ConvertRho( int ImgYUV, int ImgDst, int RhoScale, enum ColorOpt opt );
int   WINAPI IP_ConvertTheta( int ImgYUV, int ImgDst, enum ColorOpt opt );
int   WINAPI IP_ConvertRhoTheta( int ImgYUV, int ImgDstR, int ImgDstT, int RhoScale, enum ColorOpt opt );
int   WINAPI IP_ExtractColor( int ImgYUV, int ImgDst, int Ythrmin, int Ythrmax, int Uthrmin, int Uthrmax, int Vthrmin, int Vthrmax, enum ColorOpt opt );
int   WINAPI IP_ExtractColorRhoTheta( int ImgYUV, int ImgDst, int Ythrmin, int Ythrmax, int Rthrmin, int Rthrmax, int Tthrmin, int Tthrmax, enum ColorOpt opt );

int   WINAPI IP_ConvertYUVtoRGB( int ImgYUV, int ImgRID, int ImgGID, int ImgBID );
int   WINAPI IP_ConvertRGBtoYUV( int ImgRID, int ImgGID, int ImgBID, int ImgYUV );

int   WINAPI IP_ConvertYUVtoRGBfast( int ImgYUV, int ImgRGB );
int   WINAPI IP_ConvertRGBtoYUVfast( int ImgRGB, int ImgYUV );

int   WINAPI IP_ExtractColorRGB( int ImgRGB, int ImgDst, int Rthrmin, int Rthrmax, int Gthrmin, int Gthrmax, int Bthrmin, int Bthrmax );
int   WINAPI IP_ExtractColorHSI( int ImgRGB, int ImgDst, int Hthrmin, int Hthrmax, int Sthrmin, int Sthrmax, int Ithrmin, int Ithrmax );
int   WINAPI IP_ConvertHue( int ImgRGB, int ImgDst );
int   WINAPI IP_ConvertSaturation( int ImgRGB, int ImgDst );
int   WINAPI IP_ConvertSaturationExt( int ImgRGB, int ImgDst );
int   WINAPI IP_ConvertIntensity( int ImgRGB, int ImgDst );

int   WINAPI OpenMultiColor( enum IPMultiMethod method );
int   WINAPI CloseMultiColor( enum IPMultiMethod method );
int   WINAPI ClearMultiColorYRT( int ColorNum );
int   WINAPI SetMultiColorYRT( int ColorNum, int OutNum, int Ythrmin, int Ythrmax, int Rthrmin, int Rthrmax, int Tthrmin, int Tthrmax );
int   WINAPI IP_ExtractMultiColorRhoTheta( int ImgYUV, int ImgDst, enum ColorOpt opt, enum MultiColorOpt multiOpt );
int   WINAPI ClearMultiColor( int ColorNum );
int   WINAPI SetMultiColor( int ColorNum, int OutNum, int Ythrmin, int Ythrmax, int Uthrmin, int Uthrmax, int Vthrmin, int Vthrmax );
int   WINAPI IP_ExtractMultiColor( int ImgYUV, int ImgDst, enum ColorOpt opt, enum MultiColorOpt multiOpt );
int   WINAPI ClearMultiColorHSI( int ColorNum );
int   WINAPI SetMultiColorHSI( int ColorNum, int OutNum, int Hthrmin, int Hthrmax, int Sthrmin, int Sthrmax, int Ithrmin, int Ithrmax );
int   WINAPI IP_ExtractMultiColorHSI( int ImgRGB, int ImgDst, enum MultiColorOpt multiOpt );
int   WINAPI ClearMultiColorRGB( int ColorNum );
int   WINAPI SetMultiColorRGB( int ColorNum, int OutNum, int Rthrmin, int Rthrmax, int Gthrmin, int Gthrmax, int Bthrmin, int Bthrmax );
int   WINAPI IP_ExtractMultiColorRGB( int ImgRGB, int ImgDst, enum MultiColorOpt multiOpt );


/* 直線検出 */
int   WINAPI SetHoughControl( int AngleResolution, int DistanceMask );
int   WINAPI IP_DetectLine( int ImgID, PredictLine *pl, DetectLine *dl, DetectLineControl *cnt );


/* エッジ方向抽出 */
int   WINAPI SetExtractEdgeOperater( int *, int * );
int   WINAPI IP_ExtractEdgeRhoTheta( int, int, int, int, int, enum ExtractEdgeMethod, int );
int   WINAPI IP_ExtractEdgeRhoThetaExt( int, int, int, enum ExtractEdgeMethod, int );


/* 拡張画像処理 */
int   WINAPI IP_Distance( int ImgSrc, int ImgDst );

int   WINAPI GenerateGaussian1DhvWeight( int knl_size, float sh, float sv, short *htbl, short *vtbl );
int   WINAPI IP_1DhvConv( int ImgSrc, int ImgDst, int diviser, int knl_size, enum IP2DConv opt, short *htbl, short *vtbl );
int   WINAPI IP_1DhvLOGFLT( int ImgSrc, int ImgDst, float sigma, float a, int diviser, int knl_size, enum IP2DConv opt );
int   WINAPI IP_1DhConv( int ImgSrc, int ImgDst, int diviser, int knl_size, enum IP2DConv opt, short *tbl );
int   WINAPI IP_1DvConv( int ImgSrc, int ImgDst, int diviser, int knl_size, enum IP2DConv opt, short *tbl );

int   WINAPI GenerateLOGWeight( int knl_size, float sigma, float a, short *tbl );
int   WINAPI GenerateGaussianWeight( int knl_size, float sh, float sv, short *tbl );
int   WINAPI GenerateUnSharpWeight( int knl_size, float sh, float sv, float k, short *tbl );
int   WINAPI GenerateEllipticalWeight( int knl_size, float width, float height, short *tbl );
int   WINAPI IP_2DConv( int ImgSrc, int ImgDst, int diviser, int knl_size, enum IP2DConv opt, short *tbl );

int   WINAPI IP_Sobel( int ImgSrc, int ImgDst, int gain );
int   WINAPI IP_SobelBinarize( int ImgSrc, int ImgDst, int thr );

int   WINAPI IP_Mosaic( int ImgSrc, int ImgDst, int width, int height );

int   WINAPI IP_ZeroCross( int ImgSrc, int ImgDst, float gain, int thr );
int   WINAPI IP_ZeroCrossExt( int ImgSrc, int ImgDst, unsigned char *thetaTbl, enum IPZeroCrossThetaMethod method, float gain, int thr );
int   WINAPI IP_ZeroCrossCheck( int ImgSrc, int ImgDst, int thr );

int   WINAPI IP_ConvertMorphicLUT( int ImgSrc, int ImgDst, int *tbl );
int   WINAPI GenerateMorphicLUT( char *ptn, int *tbl, int n );

int   WINAPI GenerateXYTblfromBinTempMask( int TmpID, IPCountPixelTbl *tbl );
int   WINAPI IP_CountPixel( int ImgSrc, int ImgDst, int tmp_size, int n, IPCountPixelTbl *tbl, enum IPCountPixelOpt opt );

int   WINAPI IP_ZoomOut( int ImgSrc, int ImgDst, int mag );
int   WINAPI IP_ZoomOutExt( int ImgSrc, int ImgDst, int xmag, int ymag );
int   WINAPI IP_ZoomOutwithFLT( int ImgSrc, int ImgDst, int mag, int scale, int *COEFF );
int   WINAPI IP_ZoomOutAndFLT( int ImgSrc, int ImgDst, int mag, int scale, int *COEFF );
int   WINAPI IP_ZoomIn( int ImgSrc, int ImgDst, int mag );
int   WINAPI IP_ZoomInExt( int ImgSrc, int ImgDst, int xmag, int ymag );


/* 拡張マクロ画像処理 */
int   WINAPI GenerateLUTLinear( float a, float b, CNVLUT *lut );
int   WINAPI GenerateLUTCubic( int stratvalue, float startgrad, int endvalue, float endgrad, CNVLUT *lut );
int   WINAPI GenerateLUTGumma( float gconst, CNVLUT *lut );
int   WINAPI GenerateLUTComb( int n, CNVLUT *lut );
int   WINAPI GenerateLUTClip( int level1, int level2, enum IPMGenerateClipMethod method, CNVLUT *lut );
int   WINAPI GenerateLUTRecursiveFLT( int level1, int level2, int value, CNVLUT *lut );
int   WINAPI GenerateLUTRecursiveFLTBG( int level1, int level2, int value, int min, int max, CNVLUT *lut );
int   WINAPI GenerateLUTEdgeFLTExt( int level1, int level2, int value, int min, int max, CNVLUT *lut );

int   WINAPI IP_RegisterLUT( int lutno, CNVLUT *lut );
int   WINAPI IP_GrayLevelConvert( int ImgSrc, int ImgDst, int lutno );
int   WINAPI IP_SmoothFLTExt( int ImgSrc, int ImgDst, enum IPMSmoothMethod method, int n );
int   WINAPI IP_EdgeFLTAbsExt( int ImgSrc, int ImgDst, enum IPMEdgeMethod method, float gain, int lutno );
int   WINAPI IP_RecursiveFLT( int ImgSrc, int ImgDst, int lutno );
int   WINAPI IP_RecursiveFLTMax( int ImgSrc, int ImgDst, float a );
int   WINAPI IP_RecursiveFLTMin( int ImgSrc, int ImgDst, float a );
int   WINAPI IP_RecursiveFLTTyp( int ImgSrc, int ImgDst, float a );
int   WINAPI IP_OritaFLT( int ImgSrc, int ImgDst, int thr, int n );
int   WINAPI IP_ZeroCrossFLT( int ImgSrc, int ImgDst, enum IPMZeroXSmoothMethod smooth_method, int n, enum IPMZeroXLapMethod lap_method, int EdgeGain, int thr );
int   WINAPI IP_BinarizeNoiseFLT( int ImgSrc, int ImgDst, int thr, int n );

int   WINAPI GenerateLUTHistogramEQ( long *Tbl, CNVLUT *lut );
int   WINAPI IP_HistogramEQ( int ImgSrc, int ImgDst, long *Tbl, CNVLUT *lut );
int   WINAPI GenerateLUTHistogramID( int ImgSrc0, int ImgSrc1, CNVLUT *lut );

int   WINAPI IP_BinarizePercentile( int ImgSrc, int ImgDst, int percent, int thr, long *Tbl, enum IPMBinPercentMethod method );

int   WINAPI IP_Draw3DGraph( int ImgID, unsigned char *table, int xcount, int ycount, int dx, int dy, int sx, int sy, int ex, int ey );

int   WINAPI IP_PseudoColor( int ImgID, int ImgYUVID, unsigned char *Rtbl, unsigned char *Gtbl, unsigned char *Btbl );
int   WINAPI SetPseudoColor( unsigned char *Rtbl, unsigned char *Gtbl, unsigned char *Btbl );
int   WINAPI SetPseudoColorExt( unsigned char *Rtbl, unsigned char *Gtbl, unsigned char *Btbl, PseudoColorTBL cnt );

int	  WINAPI Get2CameraMltPort(int ,int ,int);
int	  WINAPI Get4Camera(int ,int ,int ,int);


/************************************************

	SuperVchip レジスタ・アクセス関数群

*************************************************/

#ifndef	__svcreg_h__

/* ＳＹＳＰレジスタ */
void          WINAPI WriteCNF( unsigned long );
unsigned long WINAPI ReadCNF( );
void          WINAPI WriteDIDO_CTL( unsigned long ) ;
unsigned long WINAPI ReadDIDO_CTL( ) ;
void          WINAPI WriteDIDO_DT( unsigned long ) ;
unsigned long WINAPI ReadDIDO_DT( ) ;
void          WINAPI WriteDI_INT_CTL( unsigned long ) ;
unsigned long WINAPI ReadDI_INT_CTL( ) ;
unsigned long WINAPI ReadS_STS( ) ;
void          WINAPI WriteS_INT_ST( unsigned long );
unsigned long WINAPI ReadS_INT_ST( );
void          WINAPI WriteS_INT_EN( unsigned long );
unsigned long WINAPI ReadS_INT_EN( );
void          WINAPI WriteCOMS2P_SET( unsigned long );
void          WINAPI WriteCOMS2P_FLG( unsigned long );
unsigned long WINAPI ReadCOMS2P_FLG( );
unsigned long WINAPI ReadCOMP2S( );
void          WINAPI WriteCOMP2S_CLR( unsigned long );
void          WINAPI WriteGBUSUAR( unsigned long );
unsigned long WINAPI ReadGBUSUAR( );
unsigned long WINAPI ReadGBUSRDT( );
void          WINAPI WriteGBUSLAR( unsigned long );
void          WINAPI WriteGBUSADT( unsigned long );
unsigned long WINAPI ReadGBUSADT( );
unsigned long WINAPI ReadP_STS( );
void          WINAPI WriteP_INT_ST( unsigned long );
unsigned long WINAPI ReadP_INT_ST( );
void          WINAPI WriteP_INT_EN( unsigned long );
unsigned long WINAPI ReadP_INT_EN( );
void          WINAPI WriteCOMP2S_SET( unsigned long );
void          WINAPI WriteCOMP2S_FLG( unsigned long );
unsigned long WINAPI ReadCOMP2S_FLG( );
unsigned long WINAPI ReadCOMS2P( );
void          WINAPI WriteCOMS2P_CLR( unsigned long );

/* ＡＰレジスタ */
void          WINAPI WriteIMCNF( unsigned long );
unsigned long WINAPI ReadIMCNF( );
void          WINAPI WriteAPSASP( unsigned long );
unsigned long WINAPI ReadAPSASP( );
void          WINAPI WriteAPSBSP( unsigned long );
unsigned long WINAPI ReadAPSBSP( );
void          WINAPI WriteAPDSP( unsigned long );
unsigned long WINAPI ReadAPDSP( );
void          WINAPI WriteAPLNG( unsigned long );
unsigned long WINAPI ReadAPLNG( );
void          WINAPI WriteAPDLY( unsigned long );
unsigned long WINAPI ReadAPDLY( );
void          WINAPI WriteAPSIZE_SA( unsigned long );
unsigned long WINAPI ReadAPSIZE_SA( );
void          WINAPI WriteAPSIZE_SB( unsigned long );
unsigned long WINAPI ReadAPSIZE_SB( );
void          WINAPI WriteAPSIZE_DST( unsigned long );
unsigned long WINAPI ReadAPSIZE_DST( );
void          WINAPI WriteAPMAG( unsigned long );
unsigned long WINAPI ReadAPMAG( );
unsigned long WINAPI ReadAPDLENCNT( );
void          WINAPI WriteBUS_SHARE( unsigned long );
unsigned long WINAPI ReadBUS_SHARE( );

/* ＩＭレジスタ */
void          WINAPI WriteDMA_GO( unsigned long );
unsigned long WINAPI ReadDMA_GO( );
void          WINAPI WriteIM_PTR( unsigned long );
unsigned long WINAPI ReadIM_PTR( );
void          WINAPI WriteIM_X_LNG( unsigned long );
unsigned long WINAPI ReadIM_X_LNG( );
void          WINAPI WriteIM_SPC( unsigned long );
unsigned long WINAPI ReadIM_SPC( );
void          WINAPI WriteDMA_Y_COUNT( unsigned long );
unsigned long WINAPI ReadDMA_Y_COUNT( );
void          WINAPI WriteIM_DATA( unsigned long );
unsigned long WINAPI ReadIM_DATA( );
void          WINAPI WriteIM_DATA_1B( unsigned long );
void          WINAPI WriteDMA_DATA( unsigned long );
unsigned long WINAPI ReadDMA_DATA( );

/* ＶＰ１レジスタ */
void          WINAPI WriteVP1CNF( unsigned long regdata );
unsigned long WINAPI ReadVP1CNF( );
void          WINAPI WriteVP1CNF2( unsigned long regdata );
unsigned long WINAPI ReadVP1CNF2( );
unsigned long WINAPI ReadVP1STS( );
void          WINAPI WriteVP1SP( unsigned long );
unsigned long WINAPI ReadVP1SP( );
void          WINAPI WriteVP1SP2( unsigned long );
unsigned long WINAPI ReadVP1SP2( );
void          WINAPI WriteVP1SIZE( unsigned long );
unsigned long WINAPI ReadVP1SIZE( );
void          WINAPI WriteVP1DLY( unsigned long );
unsigned long WINAPI ReadVP1DLY( );
void          WINAPI WriteVP1FLDDET( unsigned long );
unsigned long WINAPI ReadVP1FLDDET( );
void          WINAPI WriteVP1HSPLL( unsigned long );
unsigned long WINAPI ReadVP1HSPLL( );
void          WINAPI WriteVP1FLSHCTL( unsigned long );
unsigned long WINAPI ReadVP1FLSHCTL( );
void          WINAPI WriteVP1FLSHLNG( unsigned long );
unsigned long WINAPI ReadVP1FLSHLNG( );
void          WINAPI WriteVP1VSCNT( unsigned long );
unsigned long WINAPI ReadVP1VSCNT( );
void          WINAPI WriteVP1HSCNT( unsigned long );
unsigned long WINAPI ReadVP1HSCNT( );
void          WINAPI WriteVPPCCTL1( unsigned long );
unsigned long WINAPI ReadVPPCCTL1( );
void          WINAPI WriteVPPCCTL2( unsigned long );
unsigned long WINAPI ReadVPPCCTL2( );
void          WINAPI WriteVP1CLMPCTL( unsigned long );
unsigned long WINAPI ReadVP1CLMPCTL( );
void          WINAPI WriteVP1CMPTR( unsigned long );
unsigned long WINAPI ReadVP1CMPTR( );
void          WINAPI WriteVP1CMDATA( unsigned long );
unsigned long WINAPI ReadVP1CMDATA( );
void          WINAPI WriteVP1TRGCTL( unsigned long );
unsigned long WINAPI ReadVP1TRGCTL( );
void          WINAPI WriteVP1TRGCTL2( unsigned long );
unsigned long WINAPI ReadVP1TRGCTL2( );
void          WINAPI WriteVP1LSCTL( unsigned long );
unsigned long WINAPI ReadVP1LSCTL( );
void          WINAPI WriteVP1PRTCTL( unsigned long );
unsigned long WINAPI ReadVP1PRTCTL( );
void          WINAPI WriteVP1SSPCTL( unsigned long );
unsigned long WINAPI ReadVP1SSPCTL( );
void          WINAPI WriteVP1HSGCTL( unsigned long );
unsigned long WINAPI ReadVP1HSGCTL( );
void          WINAPI WriteVP1VSGCTL( unsigned long );
unsigned long WINAPI ReadVP1VSGCTL( );
void          WINAPI WriteVP1IMSIZE( unsigned long );
unsigned long WINAPI ReadVP1IMSIZE( );

/* ＤＰレジスタ */
void          WINAPI WriteDPCNF( unsigned long );
unsigned long WINAPI ReadDPCNF( );
void          WINAPI WriteDPSP( unsigned long );
unsigned long WINAPI ReadDPSP( );
void          WINAPI WriteDPSP2( unsigned long );
unsigned long WINAPI ReadDPSP2( );
void          WINAPI WriteDPSIZE( unsigned long );
unsigned long WINAPI ReadDPSIZE( );
void          WINAPI WriteDPDLY( unsigned long );
unsigned long WINAPI ReadDPDLY( );
void          WINAPI WriteDPFLDDET( unsigned long );
unsigned long WINAPI ReadDPFLDDET( );
void          WINAPI WriteDPIMSIZE( unsigned long );
unsigned long WINAPI ReadDPIMSIZE( );
void          WINAPI WriteDPCMPTR( unsigned long );
unsigned long WINAPI ReadDPCMPTRT( );
void          WINAPI WriteDPCMDATA( unsigned long );
unsigned long WINAPI ReadDPCMDATA( );

/* ＶＰ２レジスタ */
void          WINAPI WriteVP2CNF( unsigned long regdata );
unsigned long WINAPI ReadVP2CNF( );
void          WINAPI WriteVP2CNF2( unsigned long regdata );
unsigned long WINAPI ReadVP2CNF2( );
unsigned long WINAPI ReadVP2STS( );
void          WINAPI WriteVP2SP( unsigned long );
unsigned long WINAPI ReadVP2SP( );
void          WINAPI WriteVP2SP2( unsigned long );
unsigned long WINAPI ReadVP2SP2( );
void          WINAPI WriteVP2SIZE( unsigned long );
unsigned long WINAPI ReadVP1SIZE( );
void          WINAPI WriteVP2DLY( unsigned long );
unsigned long WINAPI ReadVP2DLY( );
void          WINAPI WriteVP2FLDDET( unsigned long );
unsigned long WINAPI ReadVP2FLDDET( );
void          WINAPI WriteVP2HSPLL( unsigned long );
unsigned long WINAPI ReadVP2HSPLL( );
void          WINAPI WriteVP2FLSHCTL( unsigned long );
unsigned long WINAPI ReadVP2FLSHCTL( );
void          WINAPI WriteVP2FLSHLNG( unsigned long );
unsigned long WINAPI ReadVP2FLSHLNG( );
void          WINAPI WriteVP2VSCNT( unsigned long );
unsigned long WINAPI ReadVP2VSCNT( );
void          WINAPI WriteVP2HSCNT( unsigned long );
unsigned long WINAPI ReadVP2HSCNT( );
void          WINAPI WriteVP2CLMPCTL( unsigned long );
unsigned long WINAPI ReadVP2CLMPCTL( );
void          WINAPI WriteVP2CMPTR( unsigned long );
unsigned long WINAPI ReadVP2CMPTR( );
void          WINAPI WriteVP2CMDATA( unsigned long );
unsigned long WINAPI ReadVP2CMDATA( );
void          WINAPI WriteVP2TRGCTL( unsigned long );
unsigned long WINAPI ReadVP2TRGCTL( );
void          WINAPI WriteVP2TRGCTL2( unsigned long );
unsigned long WINAPI ReadVP2TRGCTL2( );
void          WINAPI WriteVP2LSCTL( unsigned long );
unsigned long WINAPI ReadVP2LSCTL( );
void          WINAPI WriteVP2PRTCTL( unsigned long );
unsigned long WINAPI ReadVP2PRTCTL( );
void          WINAPI WriteVP2SSPCTL( unsigned long );
unsigned long WINAPI ReadVP2SSPCTL( );
void          WINAPI WriteVP2HSGCTL( unsigned long );
unsigned long WINAPI ReadVP2HSGCTL( );
void          WINAPI WriteVP2VSGCTL( unsigned long );
unsigned long WINAPI ReadVP2VSGCTL( );
void          WINAPI WriteVP2IMSIZE( unsigned long );
unsigned long WINAPI ReadVP2IMSIZE( );

/* ＨＰレジスタ */
void          WINAPI WriteHPCTL( unsigned long );
unsigned long WINAPI ReadHPCTL( );
unsigned long WINAPI ReadHPSTS( );
void          WINAPI WriteHMPTR( unsigned long );
unsigned long WINAPI ReadHMPTR( );
void          WINAPI WriteHMDATA( unsigned long );
unsigned long WINAPI ReadHMDATA( );
void          WINAPI WriteHPCMD( unsigned long );
unsigned long WINAPI ReadHPCMD( );
void          WINAPI WriteHMCNT( unsigned long );
unsigned long WINAPI ReadHMCNT( );
void          WINAPI WriteHPRGN( unsigned long );
unsigned long WINAPI ReadHPRGN( );
void          WINAPI WriteHPWINSP( unsigned long );
unsigned long WINAPI ReadHPWINSP( );
void          WINAPI WriteHPWINLNG( unsigned long );
unsigned long WINAPI ReadHPWINLNG( );
unsigned long WINAPI ReadHPMINSP( );
unsigned long WINAPI ReadHPMAXEP( );
unsigned long WINAPI ReadHPACC( );
unsigned long WINAPI ReadHPMNMX( );

/* ＩＰレジスタ */
void          WINAPI WriteIPFUN( unsigned long );
unsigned long WINAPI ReadIPFUN( );
void          WINAPI WriteCNST( unsigned long );
unsigned long WINAPI ReadCNST( );
void          WINAPI WriteBINTHR( unsigned long );
unsigned long WINAPI ReadBINTHR( );
void          WINAPI WriteCOEFF02( unsigned long );
unsigned long WINAPI ReadCOEFF02( );
void          WINAPI WriteCOEFF35( unsigned long );
unsigned long WINAPI ReadCOEFF35( );
void          WINAPI WriteCOEFF68( unsigned long );
unsigned long WINAPI ReadCOEFF68( );
void          WINAPI WriteKNLMSK( unsigned long );
unsigned long WINAPI ReadKNLMSK( );
unsigned long WINAPI ReadLABCNT( );
void          WINAPI WriteCMPTR( unsigned long );
unsigned long WINAPI ReadCMPTR( );
void          WINAPI WriteCMBDATA( unsigned long );
unsigned long WINAPI ReadCMBDATA( );
unsigned long WINAPI ReadTMSUM01( );
unsigned long WINAPI ReadTMSUM23( );
unsigned long WINAPI ReadTMSUM45( );
unsigned long WINAPI ReadTMSUM67( );
unsigned long WINAPI ReadTMSUM8( );
void          WINAPI WriteTMSUMT( unsigned long );
unsigned long WINAPI ReadTMSUMT( );
unsigned long WINAPI ReadMSKCNT( );
unsigned long WINAPI ReadSUM8( );
unsigned long WINAPI ReadSQSUM8( );
unsigned long WINAPI ReadCRSUM8( );
unsigned long WINAPI ReadSUM7( );
unsigned long WINAPI ReadSQSUM7( );
unsigned long WINAPI ReadCRSUM7( );
unsigned long WINAPI ReadSUM6( );
unsigned long WINAPI ReadSQSUM6( );
unsigned long WINAPI ReadCRSUM6( );
unsigned long WINAPI ReadSUM5( );
unsigned long WINAPI ReadSQSUM5( );
unsigned long WINAPI ReadCRSUM5( );
unsigned long WINAPI ReadSUM4( );
unsigned long WINAPI ReadSQSUM4( );
unsigned long WINAPI ReadCRSUM4( );
unsigned long WINAPI ReadSUM3( );
unsigned long WINAPI ReadSQSUM3( );
unsigned long WINAPI ReadCRSUM3( );
unsigned long WINAPI ReadSUM2( );
unsigned long WINAPI ReadSQSUM2( );
unsigned long WINAPI ReadCRSUM2( );
unsigned long WINAPI ReadSUM1( );
unsigned long WINAPI ReadSQSUM1( );
unsigned long WINAPI ReadCRSUM1( );
unsigned long WINAPI ReadSUM0( );
unsigned long WINAPI ReadSQSUM0( );
unsigned long WINAPI ReadCRSUM0( );
void          WINAPI WriteCOEFF911( unsigned long );
unsigned long WINAPI ReadCOEFF911( );
void          WINAPI WriteCOEFF1214( unsigned long );
unsigned long WINAPI ReadCOEFF1214( );
void          WINAPI WriteCOEFF15( unsigned long );
unsigned long WINAPI ReadCOEFF15( );
unsigned long WINAPI ReadTMSUM9( );
unsigned long WINAPI ReadTMSUM1011( );
unsigned long WINAPI ReadTMSUM1213( );
unsigned long WINAPI ReadTMSUM1415( );
unsigned long WINAPI ReadSUM9( );
unsigned long WINAPI ReadSQSUM9( );
unsigned long WINAPI ReadCRSUM9( );
unsigned long WINAPI ReadSUM10( );
unsigned long WINAPI ReadSQSUM10( );
unsigned long WINAPI ReadCRSUM10( );
unsigned long WINAPI ReadSUM11( );
unsigned long WINAPI ReadSQSUM11( );
unsigned long WINAPI ReadCRSUM11( );
unsigned long WINAPI ReadSUM12( );
unsigned long WINAPI ReadSQSUM12( );
unsigned long WINAPI ReadCRSUM12( );
unsigned long WINAPI ReadSUM13( );
unsigned long WINAPI ReadSQSUM13( );
unsigned long WINAPI ReadCRSUM13( );
unsigned long WINAPI ReadSUM14( );
unsigned long WINAPI ReadSQSUM14( );
unsigned long WINAPI ReadCRSUM14( );
unsigned long WINAPI ReadSUM15( );
unsigned long WINAPI ReadSQSUM15( );
unsigned long WINAPI ReadCRSUM15( );

/* ＯＦＰレジスタ */
void          WINAPI WriteOFPTHR( unsigned long );
unsigned long WINAPI ReadOFPTHR( );
void          WINAPI WriteOFPSIZE( unsigned long );
unsigned long WINAPI ReadOFPSIZE( );
unsigned long WINAPI ReadOFPMIN( );
unsigned long WINAPI ReadOFPMINNO( );

/* 起動レジスタ */
void          WINAPI WriteAPCMD( unsigned long );
unsigned long WINAPI ReadAPCMD( );
void          WINAPI WriteVPCMD( unsigned long );
unsigned long WINAPI ReadVPCMD( );
void          WINAPI WriteVDPCLKCNF( unsigned long );
unsigned long WINAPI ReadVDPCLKCNF( );
void          WINAPI WriteDPCMD( unsigned long );
unsigned long WINAPI ReadDPCMD( );
void          WINAPI WriteVP1CMD( unsigned long );
unsigned long WINAPI ReadVP1CMD( );
void          WINAPI WriteVP2CMD( unsigned long );
unsigned long WINAPI ReadVP2CMD( );

#endif	/* !__svcreg_h__ */

#ifdef __cplusplus
}
#endif /* __cplusplus */

#endif /* !__IPXPROT_H__ */
