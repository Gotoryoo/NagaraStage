using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace NagaraStage {
    namespace IO {
        namespace Driver {
            /// <summary>
            /// VP910のドライバDLLをラッピングするクラスです．
            /// </summary>
            /// <author email="o1007410@edu.gifu-u.ac.jp">Hirokazu Yokoyama</author>
            public class VP910Define {

                /*---------------------------
                 * 4. 画像メモリ領域管理コマンド
                 * --------------------------*/

                [DllImport("VP900CMD.dll")]
                public static extern int AllocImg(int deviceId, int size);

                [DllImport("VP900CMD.dll")]
                public static extern int FreeImg(int deviceId, int bufferId);

                [DllImport("VP900CMD.dll")]
                public static extern int SetWindow(int deviceId, int windowType, int sx, int sy, int ex, int ey);

                [DllImport("VP900CMD.dll")]
                public static extern int SetAllWindow(int deviceId, int sx, int sy, int ex, int ey);

                [DllImport("VP900CMD.dll")]
                public static extern int ResetAllWindow(int deviceId);

                [DllImport("VP900CMD.dll")]
                public static extern int EnableIPWindow(int deviceId);

                [DllImport("VP900CMD.dll")]
                public static extern int DisableIPWindow(int deviceId);

                [DllImport("VP900CMD.dll")]
                public static extern int AllocDispImg(int deviceId, int size, int dispType);

                [DllImport("VP900CMD.dll")]
                public static extern int FreeDispImg(int deviceId);

                /*-------------------------------------------
                 * 5. 映像入力コマンド
                 * -----------------------------------------*/

                [DllImport("VP900CMD.dll")]
                public static extern int SetVideoFrame(int deviceId, int interlace, int videoFrameSize);

                [DllImport("VP900CMD.dll")]
                public static extern int GetCamera(int deviceId, int bufferId);

                [DllImport("VP900CMD.dll")]
                public static extern int SelectCamera(int deviceId, int cameraId, int cameraType);

                [DllImport("VP900CMD.dll")]
                public static extern int SetVFDelay(int deviceId, int hdly, int vdly);

                [DllImport("VP900CMD.dll")]
                public static extern int SetShutterSpeed(int deviceId, int speed);

                [DllImport("VP900CMD.dll")]
                public static extern int SetTrigerMode(int deviceId, int mode);

                [DllImport("VP900CMD.dll")]
                public static extern int Get2Camera(int deviceId, int bufferId1, int bufferId2);

                [DllImport("VP900CMD.dll")]
                public static extern int GetCameraSts(int deviceId);

                /*-------------------------------------------
                 * 6. 映像表示コマンド
                 * ------------------------------------------*/

                [DllImport("VP900CMD.dll")]
                public static extern int DispCamera(int deviceId);

                [DllImport("VP900CMD.dll")]
                public static extern int DispImg(int deviceId, int bufferId);

                [DllImport("VP900CMD.dll")]
                public static extern int NoDisp(int dieviceId);

                [DllImport("VP900CMD.dll")]
                public static extern int SetDispFrame(int deviceId, int mode, int size);

                /*------------------------------------------
                 * 画像転送・アフィン変換コマンド
                 * -----------------------------------------*/

                [DllImport("VP900CMD.dll")]
                public static extern int IP_Copy(int deviceId, int imgSrc, int imgDst);

                /*------------------------------------------
                 * 2値化コマンド
                 * -----------------------------------------*/

                [DllImport("VP900CMD.dll")]
                public static extern int IP_Binarize(int deviceId, int imgSrc, int imgDst, int thr);

                /*-------------------------------------------
                 * 10. 画像変換コマンド
                 * ------------------------------------------*/

                [DllImport("VP900CMD.dll")]
                public static extern int IP_Invert(int deviceId, int imgSrc, int imgDst);

                [DllImport("VP900CMD.dll")]
                public static extern int IP_Minus(int deviceId, int imgSrc, int imgDst);

                [DllImport("VP900CMD.dll")]
                public static extern int IP_Abs(int deviceId, int imgSrc, int imgDst);

                [DllImport("VP900CMD.dll")]
                public static extern int IP_AddConst(int deviceId, int imgSrc, int imgDst, int constant);

                [DllImport("VP900CMD.dll")]
                public static extern int IP_SubConst(int deviceId, int imgSrc, int imgDst, int constant);

                [DllImport("VP900CMD.dll")]
                public static extern int IP_SubConstAbs(int deviceId, int imgSrc, int imgDst, int constant);

                [DllImport("VP900CMD.dll")]
                public static extern int IP_MultiConst(int deviceId, int imgSrc, int imgDst, int constant, int downShift);

                [DllImport("VP900CMD.dll")]
                public static extern int IP_AndConst(int deviceId, int imgSrc, int imgDst, int constant);





                /// <param name="lut">型に自信ない．．．</param>
                [DllImport("VP900CMD.dll")]
                public static extern int WriteConvertLUT(int deviceId, ref int lut);

                [DllImport("VP900CMD.dll")]
                public static extern int IP_ConvertLUT(int deviceId, int imgSrc, int imgDst);

                /*-------------------------------------
                 * 11. 画像館算術演算コマンド
                 * -----------------------------------*/

                [DllImport("VP900CMD.dll")]
                public static extern int IP_Add(int deviceId, int imgSrc0, int imgSrc1, int imgDst);

                [DllImport("VP900CMD.dll")]
                public static extern int IP_Sub(int deviceId, int imgSrc0, int imgSrc1, int imgDst);

                [DllImport("VP900CMD.dll")]
                public static extern int IP_Averate(int deviceId, int imgSrc0, int imgSrc1, int imgDst);

                /*------------------------------------
                 * 画像管論理演算コマンド
                 * ----------------------------------*/

                [DllImport("VP900CMD.dll")]
                public static extern int IP_And(int deviceId, int imgSrc0, int imgSrc1, int imgDst);

                [DllImport("VP900CMD.dll")]
                public static extern int IP_Or(int deviceId, int imgSrc0, int imgSrc1, int imgDst);

                [DllImport("VP900CMD.dll")]
                public static extern int IP_Xor(int deviceId, int imgSrc0, int imgSrc1, int imgDst);

                [DllImport("VP900CMD.dll")]
                public static extern int IP_InvertAnd(int deviceId, int imgsrc0, int imgSrc1, int imgDst);

                [DllImport("VP900CMD.dll")]
                public static extern int IP_InvertOr(int deviceId, int imgSrc0, int imgSrc1, int imgDst);

                [DllImport("VP900CMD.dll")]
                public static extern int IP_Xnor(int deviceId, int imgSrc0, int imgSrc1, int imgDst);

                /*--------------------------------------
                 * 2地画像形状変換コマンド
                 * -------------------------------------*/

                [DllImport("VP900CMD.dll")]
                public static extern int IP_PickNoise4(int deviceId, int imgSrc, int imgDst);

                /*----------------------------------------
                 * コンボリューションコマンド
                 * ---------------------------------------*/

                [DllImport("VP900CMD.dll")]
                public static extern int IP_Lapl4FLT(int deviceId, int imgSrc, int imgDst);

                [DllImport("VP900CMD.dll")]
                public static extern int IP_Lapl8FLT(int deviceId, int imgSrc, int imgDst);

                [DllImport("VP900CMD.dll")]
                public static extern int IP_Lapl4FLTAbs(int deviceId, int imgSrc, int imgDst);

                [DllImport("VP900CMD.dll")]
                public static extern int IP_Lapl8FLTAbs(int deviceId, int imgSrc, int imgDst);

                /// <param name="COEFF">型に自信ない</param>       
                [DllImport("VP900CMD.dll")]
                public static extern int IP_SmoothFLT(int deviceId, int imgSrc, int imgDst, int downShift, ref int COEFF);

                /// <param name="COEFF">型に自信ない</param>
                [DllImport("VP900CMD.dll")]
                public static extern int IP_LineFLT(int deviceId, int imgSrc, int imgDst, int downShift, ref int COEFF);

                /*-----------------------------------------------
                 * 濃淡画像特徴抽出コマンド
                 * ----------------------------------------------*/

                /// <summary>Is the p_ histogram.</summary>
                /// <param name="deviceId">The device id.</param>
                /// <param name="imgSrc">The img SRC.</param>
                /// <param name="tbl">型に自信ない</param>
                /// <param name="regtbl">型に自信ない</param>
                /// <param name="opt">The opt.</param>
                /// <returns></returns>
                [DllImport("VP900CMD.dll")]
                public static extern int IP_Histogram(int deviceId, int imgSrc, ref int tbl, ref IPG0FeatureTbl regtbl, int opt);

                /*--------------------------------------------------
                 * 画像メモリアクセスコマンド
                 * ------------------------------------------------*/

                [DllImport("VP900CMD.dll")]
                public static extern int ReadImg(int deviceId, int bufferId, ref byte data, ref int count);

                [DllImport("VP900CMD.dll")]
                public static extern int ReadImgReverse(int deviceId, int bufferId, ref byte data, int count);

                [DllImport("VP900CMD.dll")]
                public static extern int WriteImg(int deviceId, int bufferId, ref byte data, int count);

                [DllImport("VP900CMD.dll")]
                public static extern int WriteImgReverse(int deviceId, int bufferId, ref byte data, int count);

                /*-------------------------------------------------
                 * 画像ファイリングコマンド
                 * -----------------------------------------------*/

                [DllImport("VP900CMD.dll")]
                public static extern int SaveBMPFile(int deviceId, int imageId, string fileName, int bitmapMode);

                [DllImport("VP900CMD.dll")]
                public static extern int LoadBMPFile(int deviceId, int imageId, string fileName);

                /*------------------------------------------------
                 * ビデオ拡張コマンド
                 * ----------------------------------------------*/

                [DllImport("VP900CMD.dll")]
                public static extern int WriteVideoLUT(int deviceId, ref int p0lut, ref int p1lust, int leng, int mode);

                [DllImport("VP900CMD.dll")]
                public static extern int SetVideoOpt(int deviceId, int mode, int opt, ref int optVal, int optlen);

                /*---------------------------
                 * シャッタースピードの設定値
                 * -------------------------*/

                /// <summary>1/4000 (0.25ms)</summary>
                public const int SHUT_4000 = 0;

                /// <summary>1/2000 (0.5 ms)</summary>
                public const int SHUT_2000 = 1;

                /// <summary>1/1000 (1   ms)</summary>
                public const int SHUT_1000 = 2;

                /// <summary>1/500  (2   ms)</summary>
                public const int SHUT_500 = 3;

                /// <summary>1/250  (4   ms)</summary>
                public const int SHUT_250 = 4;

                /// <summary>1/125  (8   ms)</summary>
                public const int SHUT_125 = 5;

                /*----------------------------------
                 * カメラタイプ
                 * ---------------------------------*/

                /// <summary>SONY XC-HR300 2I Mode</summary>
                public const int SONY_XCHR300 = 9;

                /// <summary>2I 59MHz Mode</summary>
                public const int SONY_XCHR300_59M = 17;

                /// <summary>1N 59MHz Mode</summary>
                public const int SONY_XCHR300_1N_59M = 30;

                /*------------------------------------
                 * Image size type
                 * ----------------------------------*/
                public const int IMG_FS_256H_256V = 0;
                public const int IMG_FS_512H_256V = 1;
                public const int IMG_FS_256H_512V = 2;
                public const int IMG_FS_512H_512V = 3;
                public const int IMG_FS_640H_256V = 4;
                public const int IMG_FS_640H_512V = 5;
                public const int IMG_FS_1024H_512V = 6;
                public const int IMG_FS_1024H_1024V = 7;
                public const int IMG_FS_1024H_256V = 8;
                public const int IMG_FS_1280H_1024V = 9;
                public const int IMG_FS_1280H_256V = 10;
                public const int IMG_FS_1280H_512V = 11;
                public const int IMG_FS_1024H_768V = 12;
                public const int IMG_FS_2048H_512V = 13;
                public const int IMG_FS_5248H_512V = 14;
                public const int IMG_FS_8192H_512V = 15;
                public const int IMG_FS_2048H_2048V = 16;
                public const int IMG_FS_2048H_1024V = 17;
                public const int IMG_FS_1920H_1024V = 18;

            }

            [StructLayout(LayoutKind.Sequential)]
            public struct IPG0FeatureTbl {
                public int minLebelX;
                public int minLebelY;
                public int maxLebelX;
                public int maxLebelY;
                public int accLebel;
                public int minLebel;
                public int maxLebel;
                public int typicalLebel;
            }
        }
    }
}