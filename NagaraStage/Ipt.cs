using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using NagaraStage.Parameter;

namespace NagaraStage {
    /// <summary>
    /// ipt.dll をインポートしラッピングを行うクラスです.ipl.dllが持つ関数及び定数を
    /// 静的なメソッドとして提供します。
    /// </summary>
    /// <author email="hirokazu.online@gmail.com">Hirokazu Yokoyama</author>
    public class Ipt {
        public const int PositionTrans = 1;

        [DllImport("ipt.dll", EntryPoint="IP_Init")]
        public static extern int Initialize(int borderNumber, ref int retCode);

        [DllImport("ipt.dll", EntryPoint = "IP_CapMain")]
        private static extern int captureMain(int hWnd, int hDC, IntPtr buffer, int flagDrawingOnPC);

        /// <summary>
        /// 顕微鏡カメラから撮影している映像を取得します．
        /// </summary>
        /// <returns>撮影中の画像の配列</returns>
        /// <exception cref="System.Exception">撮影に失敗した場合</exception>
        public static byte[] CaptureMain() {
            int width = ParameterManager.ImageResolution.Width;
            int height = ParameterManager.ImageResolution.Height;
            byte[] buffer = new byte[width * height];

            GCHandle gcH = GCHandle.Alloc(buffer, GCHandleType.Pinned);            

            int retVal = captureMain(0, 0, gcH.AddrOfPinnedObject(), 0);
            if (retVal != 0) {
                throw new Exception("Error: IP_CapMain");
            }
            gcH.Free();

            return buffer;
        }

        [DllImport("ipt.dll", EntryPoint = "IP_DataPrint")]
        public static extern void printData(int hWnd, int hDC, int hi, byte buffer);

        [DllImport("ipt.dll", EntryPoint = "IP_NichiPrint")]
        public static extern void printNichi(int hWnd, int hDC, int hi, int level, byte src, byte bak, int emType );        

        [DllImport("ipt.dll", EntryPoint = "IP_ReadSocketIniFile")]
        public static extern int ReadSocketIniFile();

        [DllImport("ipt.dll", EntryPoint = "IP_GetDeviceID")]
        public static extern int GetDeviceId();

        [DllImport("ipt.dll", EntryPoint = "IP_SetCameraType")]
        public static extern void SetCameraType(int type);

        [DllImport("ipt.dll", EntryPoint = "IP_EMtype")]
        public static extern void SetEmulsionType(int emType, double min, double max, double step);

        [DllImport("ipt.dll", EntryPoint = "IP_CCDreso")]
        public static extern void SetCcdResolution(double x, double y);

        [DllImport("ipt.dll", EntryPoint = "IP_getAver")]
        public static extern int GetAver();

        [DllImport("ipt.dll", EntryPoint = "IP_RWGridData")]
        public static extern int ReadWriteGridData(string mode, string fineName);

        [DllImport("ipt.dll", EntryPoint="IP_RWGrFine")]
        public static extern int ReadWriteGridFine(string mode, string fineName, ref double emulsionThick);

        [DllImport("ipt.dll", EntryPoint = "IP_MakeBeamCo")]
        public static extern void MakeBeamCo(
            ref double sita, ref double magnit, ref double x0, ref double y0, ref double dx);

        [DllImport("ipt.dll", EntryPoint = "IP_InitIchiLib")]
        public static extern int InitializeIchiLib(int emulsionType, int numSpte0,
            double emulsionIndexUpperGel, double emmulsionIndexLowerGel, int hWnd, int hDc, int hi);                 

        [DllImport("ipt.dll")]
        private static extern void IP_SetEmulsionIndex(double indexUp, double indexDown);

        [DllImport("ipt.dll", EntryPoint = "IP_SetEmulsionIndexUp")]
        public static extern void SetEmulsionIndexUp(double index);

        [DllImport("ipt.dll", EntryPoint = "IP_SetEmulsionIndexDown")]
        public static extern void SetEmulsionIndexDown(double index);

        [DllImport("ipt.dll", EntryPoint = "IP_InitCo")]
        public static extern void InitializeCoodination(int mode, int iPlate);

        [DllImport("ipt.dll", EntryPoint = "IP_SetGridLocal")]
        public static extern void SetGridLocal(
            short iPlate, double magnit, double sita, double emIndexUpperGel, double emIndexLowerGel);        

        [DllImport("ipt.dll", EntryPoint = "IP_GtoM")]
        public static extern void GToM(string mode, double gx, double gy, ref double msx, ref double msy);

        [DllImport("ipt.dll", EntryPoint = "IP_MtoG")]
        public static extern void MtoG(int mode, double msx, double msy, ref double gx, ref double gy);

        [DllImport("ipt.dll", EntryPoint = "IP_OpenFile")]
        public static extern int OpenFile(int mode, string file1, string file2, string wamode);

        [DllImport("ipt.dll", EntryPoint = "IP_SetHyperFineXY")]
        public static extern void SetHyperFineXY(double dx, double dy);

        [DllImport("ipt.dll", EntryPoint = "IP_GetNextTrack")]
        public static extern int GetNextTrack(
            double deltaZ, ref short trackId, ref short direction, 
            ref double gx, ref double gdxdz, ref double gy, ref double gdydz, char comment);

        [DllImport("ipt.dll", EntryPoint = "IP_GetTrackOfID")]
        public static extern int GetTrackOfId(
            double deltaZ, ref short trackId, ref short direction,
            ref double gx, ref double gdxdz, ref double gy, ref double gdydz, char comment);

        [DllImport("ipt.dll", EntryPoint = "IP_SendMessage")]
        public static extern int SendMessage(string message);

        [DllImport("ipt.dll", EntryPoint = "IP_MarkCenter")]
        public static extern int MarkCenter(ref double xCenter, ref double yCenter, int size);

        [DllImport("ipt.dll", EntryPoint = "IP_SetThreshold")]
        public static extern void SetThreshold(int brightHit, int brightDust, int dust1, int dust2);

        [DllImport("ipt.dll", EntryPoint = "IP_getHitNum")]
        public static extern int GetHitNum();

        [DllImport("ipt.dll", EntryPoint = "IP_getHitNum0")]
        public static extern int GetHitNum0(short threshold, ref short hit1, ref short hit2, ref short hit3, ref short hit4, ref double x, ref double y);

        [DllImport("ipt.dll", EntryPoint = "IP_PrintErr")]
        public static extern void PrintError();

        [DllImport("ipt.dll", EntryPoint = "IP_StageToGrid")]
        public static extern Vector2 StageToGrid(int mode, ref Vector2 stage);

        public static void SetEmulsionIndex(EmulsionIndex emulsionIndex) {
            IP_SetEmulsionIndex(emulsionIndex.Up, emulsionIndex.Down);
        }

        public static void SetEmulsionIndex(double upIndex, double downIndex) {
            IP_SetEmulsionIndex(upIndex, downIndex);
        }
    }
}
