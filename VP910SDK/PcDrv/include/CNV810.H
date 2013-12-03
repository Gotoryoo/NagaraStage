/*                                                                                        */
/*      +------------------------------------------------------------------------+        */
/*      + Copyright (c) 2000-2004 Renesas  Northern Japan Semiconductor,Inc.     +        */
/*      +------------------------------------------------------------------------+        */
/*      Title : VP-900 Serise Image Procesing Function Prototype                          */
/*                                                                                        */
/*                                                                                        */


#ifndef		__cnv810_h__
#define		__cnv810_h__

#define		SetIPWindow					SetWindow
#define		SetAllIPWindow				SetAllWindow
#define		ResetAllIPWindow			ResetAllWindow
#define		ReadIPWindow				ReadWindow
#define		InitIP(a)					vpxInitIP(a,VP810_CONFIG,0,INIT_ONLY)
#define		EnableLoopCamera			vpxEnableLoopCamera	
#define		DisableLoopCamera			vpxDisableLoopCamera
#define		SuspendLoopCamera			vpxSuspendLoopCamera
#define		ResumeLoopCamera			vpxResumeLoopCamera
#define		EnableCameraPrefetch		vpxEnableCameraPrefetch		
#define		DisableCameraPrefetch		vpxDisableCameraPrefetch
#define		ResetCameraPrefetch			vpxResetCameraPrefetch
#define		GetCameraPrefetch			vpxGetCameraPrefetch
#define		IP_Rotate					vpxIP_Rotate
#define		WriteConvertLUT				vpxWriteConvertLUT
#define		WriteConvertLUTExt			vpxWriteConvertLUTExt
#define		IP_SmoothFLT				vpxIP_SmoothFLT
#define		IP_EdgeFLT					vpxIP_EdgeFLT
#define		IP_EdgeFLTAbs				vpxIP_EdgeFLTAbs
#define		IP_LineFLT					vpxIP_LineFLT
#define		IP_LineFLTAbs				vpxIP_LineFLTAbs
#define		IP_SmoothFLTF				vpxIP_SmoothFLTF
#define		IP_SmoothFLT9x9				vpxIP_SmoothFLT9x9
#define		IP_ExtractLOArea			vpxIP_ExtractLOArea
#define		IP_ExtractLORegionX			vpxIP_ExtractLORegionX
#define		IP_ExtractLORegionY			vpxIP_ExtractLORegionY
#define		IP_HistogramShort			vpxIP_HistogramShort
#define		IP_ProjectGO				vpxIP_ProjectGO
#define		IP_ProjectGOMinValue		vpxIP_ProjectGOMinValue
#define		IP_ProjectGOMaxValue		vpxIP_ProjectGOMaxValue
#define		IP_ProjectBO				vpxIP_ProjectBO
#define		IP_ProjectBORegionX			vpxIP_ProjectBORegionX
#define		IP_ProjectBORegionY			vpxIP_ProjectBORegionY
#define		AllocCorrTemplate			vpxAllocCorrTemplate
#define		FreeCorrTemplate			vpxFreeCorrTemplate
#define		ReadCorrTemplate			vpxReadCorrTemplate
#define		WriteCorrTemplate			vpxWriteCorrTemplate
#define		EnableCorrMask				vpxEnableCorrMask
#define		DisableCorrMask				vpxDisableCorrMask
#define		SetCorrTemplate				vpxSetCorrTemplate
#define		SetCorrTemplateExt			vpxSetCorrTemplateExt
#define		IP_CorrStep					vpxIP_CorrStep
#define		IP_CorrPoint				vpxIP_CorrPoint
#define		IP_CorrPrecise				vpxIP_CorrPrecise
#define		EnableCorrBreak				vpxEnableCorrBreak	
#define		DisableCorrBreak			vpxDisableCorrBreak	
#define		SetCorrBreakThr				vpxSetCorrBreakThr
#define		SetSearchDistance			vpxSetSearchDistance
#define		SetCorrMode					vpxSetCorrMode
#define		SetCorrPrecise				vpxSetCorrPrecise
#define		IP_CorrExt					vpxIP_CorrExt
#define		IP_CorrStepDualTmp			vpxIP_CorrStepDualTmp
#define		IP_CorrPointDualTmp			vpxIP_CorrPointDualTmp
#define		IP_CorrPrecDualTmp			vpxIP_CorrPrecDualTmp
#define		IP_CorrStepPac				vpxIP_CorrStepPac
#define		IP_CorrPointPac				vpxIP_CorrPointPac
#define		IP_CorrPrecisePac			vpxIP_CorrPrecisePac
#define		IP_CorrExtPac				vpxIP_CorrExtPac
#define		IP_CorrStepDualTmpPac		vpxIP_CorrStepDualTmpPac
#define		IP_CorrPointDualTmpPac		vpxIP_CorrPointDualTmpPac
#define		IP_CorrPrecDualTmpPac		vpxIP_CorrPrecDualTmpPac
#define		SetCorrTemplatePac			vpxSetCorrTemplatePac
#define		SetCorrTemplateExtPac		vpxSetCorrTemplateExtPac

#endif /* !__cnv810_h__ */




