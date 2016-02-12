using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

using NagaraStage.IO.Driver;
using NagaraStage.Parameter;


namespace NagaraStage {
    namespace IO {
        /// <summary>
        /// 顕微鏡から画像を撮影し，画面に投影するクラスです．
        /// </summary>
        /// <author email="o1007410@edu.gifu-u.ac.jp">Hirokazu Yokoyama</author>
        public class Camera : IDisposable {

            private int deviceId;

            /// <summary>
            /// 撮影する画素数(横)
            /// </summary>
            public const int Width = 512;

            /// <summary>
            /// 撮影する画素数(縦)
            /// </summary>
            public const int Height = 440;

            /// <summary>
            /// 撮影する画像のストライド
            /// </summary>
            public const int Stride = Width * 1;

            /// <summary>
            /// 使用しているカメラタイプ
            /// </summary>
            public static CameraType CameraType {
                get { return CameraType.SONY_XC_HR3000_2I_59MHz; }
            }

            private static Camera instance;
            private Boolean flagBreakCaptureLoop;
            private Thread capturingThread;
            private byte[] captruredBuffer;
            private BitmapSource bitmapBuffer;
            private Action action = null;
            private Thread actionThread = null;
#if NoHardware
        private Random random = new Random();
#endif
            /// <summary>
            /// キャプチャ処理が実行中であるかを取得します．
            /// <para>true: 実行中, false: 停止中</para>
            /// </summary>
            public Boolean IsRunning {
                get { return flagBreakCaptureLoop; }
            }



            public const string ParameterFilePath = "Configure\\videolut.prm";

            /// <summary>
            /// ビデオ取り込みのLUTを設定します．
            /// <para>このメソッドはSony XCHR300の21モードの時のみ使用してください．</para>
            /// </summary>
            /// <param name="videoDeviceId">使用するカメラデバイス</param>
            /// <param name="fileName">パラメータファイルへのパス</param>
            /// <exception cref="IOException">ファイルの読み込みに失敗した場合</exception>
            /// <exception cref="System.Exception"></exception>
            public void InitializeLUT(int videoDeviceId, string fileName = ParameterFilePath) {
                string line;
                const int Length = 256;
                char[] delimiterChars = { ' ', '\t' };
                int[] p0lut = new int[Length];
                int[] p1lut = new int[Length];
                int sts;
                int[] optVals = { 3, 0 };
                string[] args = new string[2];


                try {
                    StreamReader sr = File.OpenText(fileName);
                    sr.ReadLine();

                    for (int i = 0; i < Length; ++i) {
                        line = sr.ReadLine();
                        args = line.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                        p0lut[i] = short.Parse(args[0]);
                        p1lut[i] = short.Parse(args[1]);
                    }
                } catch (Exception ex) {
                    throw ex;
                }

                sts = VP910Define.WriteVideoLUT(videoDeviceId, ref p0lut[0], ref p1lut[0], 256, 0);
                if (sts == 0) {
                    sts = VP910Define.SetVideoOpt(videoDeviceId, 0, 0, ref optVals[0], 8);
                } else {
                    throw new Exception();
                }
            }



            public void SetShutterMode(EmulsionType emulsionType) {
                int cameraShutterSpeed = 0;
                switch (emulsionType) {
                    case EmulsionType.ThinType:
                        cameraShutterSpeed = 3;
                        break;
                    case EmulsionType.ThickType:
                        cameraShutterSpeed = 5;
                        break;
                    default:
                        // 該当なし
                        throw new Exception("Emulsion type value is not correct.");
                }
                VP910Define.SetShutterSpeed(deviceId, cameraShutterSpeed);
            }



            /// <summary>
            /// 初期化します
            /// </summary>
            /// <exception cref="System.Exception">ボードの初期化に失敗した場合</exception>
            public void Initialize() {
#if !_NoHardWare
                try {
                    deviceId = 0;

                    int retCode = new int();
                    retCode = Ipt.Initialize(deviceId, ref retCode);
                    if (retCode==-1) {
                        throw new Exception("Initializing VP910 was failed.");
                    }

                    Ipt.SetCameraType((int)Camera.CameraType);

                    deviceId = Ipt.GetDeviceId();

                    VP910Define.SelectCamera(deviceId, 0, (int)Camera.CameraType);
                    VP910Define.SetTrigerMode(deviceId, 2);

                } catch (Exception) {
                    throw new Exception("Initializing VP910 was failed.");
                }
#endif
            }



            /// <summary>
            /// 撮影中の画像を取得します．
            /// </summary>
            public BitmapSource Image {
                get {
                    BitmapSource bitmapBuffer;
                    bitmapBuffer = BitmapSource.Create(
                        Width, Height,
                        96, 96,
                        PixelFormats.Gray8,
                        BitmapPalettes.Gray256,
                        captruredBuffer,
                        Width);
                    return bitmapBuffer;
                }
            }

            /// <summary>
            /// 撮影中の画像をbyte配列で取得します．
            /// </summary>
            public byte[] ArrayImage {
                get { return captruredBuffer; }
            }

            /// <summary>
            /// キャプチャ中に行う処理を取得，または設定します．
            /// <para>このプロパティに設定されたメソッドは，カメラ駆動時に毎回実行されます．
            /// 実行を停止する場合はnullを設定してください．
            /// </para>
            /// </summary>
            public Action Action {
                get { return action; }
                set {
                    if (actionThread != null) {
                        if (actionThread.IsAlive) {
                            actionThread.Join();
                        }
                    }
                    action = value;
                }
            }

            /// <summary>
            /// Captureクラスのインスタンスを取得します。        
            /// </summary>        
            /// <returns>Captureのインスタンス</returns>
            public static Camera GetInstance() {
                // インスタンスがnullであったら生成
                if (instance == null) {
                    instance = new Camera();
                    instance.initialize();
                }
                return instance;
            }


            /// <summary>
            /// コンストラクタ
            /// </summary>
            private Camera() {
                captruredBuffer = new byte[Stride * Height];
                for (int i = 0; i < captruredBuffer.Length; ++i) {
                    captruredBuffer[i] = new byte();
                }
            }

            private void initialize() {
                Stop();
                flagBreakCaptureLoop = false;
            }

            /// <summary>
            /// 撮影を開始します．
            /// </summary>
            public void Start() {
                if (!flagBreakCaptureLoop) {
                    flagBreakCaptureLoop = true;
                    capturingThread = new Thread(new ThreadStart(captureLoop));
                    capturingThread.IsBackground = true;
                    capturingThread.Start();
                }
            }

            /// <summary>
            /// 撮影を終了します．
            /// </summary>
            public void Stop() {
                flagBreakCaptureLoop = false;
                if (capturingThread != null) {
                    if (capturingThread.IsAlive) {
                        capturingThread.Abort();
                        capturingThread.Join();
                    }
                }

            }

            /// <summary>
            /// 撮影処理を行うループです．
            /// <para>Startメソッドで開始し，Stopメソッドで停止させてください．</para>
            /// <para>画像撮影のみを行います．モータ制御動作を廃止しましした．</para>
            /// </summary>
            private void captureLoop() {
                while (flagBreakCaptureLoop) {
                    // 撮影画像を取得し，描画する．
#if !_NoHardWare
                    try {
                        captruredBuffer = Ipt.CaptureMain();

                    } catch (Exception ex) {
                        System.Console.WriteLine(ex.Message);
                    }
#else
                // ハードウェアがないデバッグ時は適当な映像を描写
                for (int i = 0; i < captruredBuffer.Length; ++i) {
                    captruredBuffer[i] = (byte)(random.Next(255));
                }
                Thread.Sleep(10);
#endif

                    if (action != null) {
                        actionThread = new Thread(new ThreadStart(action));
                        actionThread.IsBackground = true;
                        actionThread.Start();
                        actionThread.Join();
                    }
                    Thread.Sleep(10);
                }
            }

            public void Dispose() {
                System.Console.WriteLine("Camera is disposed");
            }
        }
    }
}