using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NagaraStage {
    namespace IO {
        /// <summary>
        /// 顕微鏡から画像を撮影し，画面に投影するクラスです．
        /// </summary>
        /// <author email="o1007410@edu.gifu-u.ac.jp">Hirokazu Yokoyama</author>
        public class Camera : IDisposable {

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