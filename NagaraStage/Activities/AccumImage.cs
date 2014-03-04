using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media.Imaging;

using NagaraStage;
using NagaraStage.Parameter;
using NagaraStage.IO;

namespace NagaraStage {
    namespace Activities {
        /// <summary>
        /// モータを駆動させながら，撮影画像を複数枚確保するクラスです．
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public class AccumImage : Activity, IActivity {
            /// <summary>
            /// 連続撮影できる最大距離(mm)
            /// </summary>
            public const double MaxDistance = 0.5;

            private static AccumImage instance;
            private ParameterManager parameterManager;
            private Thread shootingThread;
            private List<string> imagesUri = new List<string>();
            private List<double> shotPoint = new List<double>();
            private double startPoint = -1;
            private double endPoint = -1;
            private double interval = -1;
            private int numOfShot;

            /// <summary>
            /// 撮影処理を開始したときのイベント
            /// </summary>
            public event EventHandler<EventArgs> Started;

            /// <summary>
            /// 撮影したときのイベント
            /// </summary>
            public event EventHandler<ActivityEventArgs> OnShort;
            /// <summary>
            /// 処理が終了したときのイベント
            /// </summary>
            public event ActivityEventHandler Exited;

            /// <summary>
            /// 撮影枚数を取得します．
            /// </summary>
            public int NumOfShots {
                get { return numOfShot; }
            }

            /// <summary>
            /// 撮影済み画像のZ軸方向の撮影座標のリストを取得します．
            /// </summary>
            public List<double> ShotPoint {
                get { return shotPoint; }
            }

            /// <summary>
            /// 撮影開始地点の座標を設定，または取得します．
            /// </summary>
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            public double StartPoint {
                get { return startPoint; }
                set { startPoint = value; }
            }

            /// <summary>
            /// 撮影終了地点の座標を設定，または取得します．
            /// </summary>
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            public double EndPoint {
                get { return endPoint; }
                set { endPoint = value; }
            }

            /// <summary>
            /// 撮影間隔(micro meter)を設定，または取得します．
            /// </summary>
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            public double IntervalUm {
                get { return interval; }
                set {
                    if (value < 0) {
                        string message = string.Format(Properties.Strings.ValMustPositiveNotZero, Properties.Strings.Interval);
                        throw new ArgumentOutOfRangeException(message);
                    }
                    interval = value;
                }
            }

            /// <summary>
            /// 撮影の完了した割合(%)を取得します．
            /// </summary>
            public double CompletePercent {
                get {
                    double allLength = Math.Abs(EndPoint - StartPoint);
                    double presentteLength = Math.Abs(ShotPoint[ShotPoint.Count - 1] - startPoint);
                    return (allLength == 0 ? 0 : presentteLength / allLength * 100);
                }
            }

            /// <summary>
            /// 撮影した画像の一覧を取得します．
            /// </summary>
            /// <exception cref="System.ArgumentNullException">キャッシュの読み込みに失敗した場合</exception>
            public List<BitmapImage> ShootParaImages {
                get {
                    List<BitmapImage> retval = new List<BitmapImage>(NumOfShots);
                    for (int i = 0; i < NumOfShots; ++i) {
                        retval.Add(new BitmapImage(new Uri(imagesUri[i], UriKind.RelativeOrAbsolute)));
                    }
                    return retval;
                }
            }

            private AccumImage(ParameterManager _parameterManager)
                : base(_parameterManager) {
                this.parameterManager = _parameterManager;
            }

            /// <summary>
            /// インスタンスを取得します．
            /// </summary>
            /// <returns>インスタンス</returns>
            public static AccumImage GetInstance(ParameterManager _parameterManager = null) {
                if (instance == null) {
                    instance = new AccumImage(_parameterManager);
                }
                return instance;
            }

            /// <summary>
            /// 撮影した画像を取得します．
            /// </summary>
            /// <param name="n">撮影した画像のインデックス</param>
            /// <returns>画像</returns>
            /// <exception cref="System.ArgumentException">引数の値が範囲外の場合</exception>
            /// <exception cref="System.IO.FileNotFoundException">一時ファイルが見つからない場合</exception>
            public BitmapSource GetShotImage(int n) {
                if (n < 0 || n >= imagesUri.Count) {
                    throw new ArgumentException();
                }
                if (!System.IO.File.Exists(imagesUri[n])) {
                    throw new System.IO.FileNotFoundException();
                }

                return new BitmapImage(new Uri(imagesUri[n], UriKind.RelativeOrAbsolute));
            }

            /// <summary>
            /// 撮影した一時ファイルを削除します．
            /// </summary>
            /// <seealso cref="System.IO.File.Delete" />
            public void Clean() {
                foreach (string uri in imagesUri) {
                    System.IO.File.Delete(uri);
                }

            }

            /// <summary>
            /// 撮影を開始します．
            /// </summary>
            /// <exception cref="System.ArgumentOutOfRangeException"></exception>
            /// <exception cref="InActionException"></exception>
            /// <exception cref="MotorActiveException"></exception>
            public void Start() {
                MotorControler mc = MotorControler.GetInstance(parameterManager);
                if (mc.IsMoving) {
                    throw new MotorActiveException();
                }
                if (IsActive) {
                    throw new InActionException();
                }
                if (interval <= 0) {
                    throw new ArgumentOutOfRangeException(Properties.Strings.ValMustPositiveNotZero);
                }

                // キャッシュを削除
                Clean();

                shootingThread = Create(new ThreadStart(delegate() {
                    ActivityEventArgs eventArgs = new ActivityEventArgs();
                    try {
                        shootingThreadTask();
                        eventArgs.IsCompleted = true;
                    } catch (ThreadAbortException ex) {
                        eventArgs.IsAborted = true;
                        eventArgs.Exception = ex;
                    } catch (Exception ex) {
                        eventArgs.IsCompleted = false;
                        eventArgs.Exception = ex;
                    } finally {
                        mc.StopInching(MechaAxisAddress.ZAddress);
                        mc.StopInching(MechaAxisAddress.XAddress);
                        mc.StopInching(MechaAxisAddress.YAddress);
                        if (Exited != null) {
                            Exited(this, eventArgs);
                        }
                    }
                }));
                shootingThread.IsBackground = true;
                shootingThread.Start();
            }



            /// <summary>
            /// 撮影処理を行います．
            /// <para>別スレッドのタスクとして実行してください．直接呼び出さないでください．</para>
            /// </summary>
            private void shootingThreadTask() {
                if (Started != null) {
                    Started(this, new EventArgs());
                }

                imagesUri = new List<string>();
                shotPoint = new List<double>();

                MotorControler mc = MotorControler.GetInstance(parameterManager);
                Camera camera = Camera.GetInstance();
                camera.Start();
                numOfShot = 0;

                // 撮影開始地点へ移動する
                mc.MovePointZ(startPoint);
                mc.Join();

                // 撮影終了地点に移動しながら画像を確保する
                double pnInterval = (startPoint > endPoint ? -interval : interval);
                double presentPoint = mc.GetPoint().Z;
                while (isShootContinue(presentPoint)) {
                    mc.MoveDistance(pnInterval, VectorId.Z);
                    mc.Join();
                    camera.Stop();
                    BitmapSource image = camera.Image;                    
                    saveTemp(image);
                    camera.Start();
                    shotPoint.Add(presentPoint);
                    ++numOfShot;
                    if (OnShort != null)
                    {
                        ActivityEventArgs e = new ActivityEventArgs();
                        e.ZValue = presentPoint;
                        OnShort(this, e);
                    }
#if false//20140225 yokoyama,jyoshida

                    mc.MoveDistance(pnInterval, VectorId.Z, delegate {
                        presentPoint = mc.GetPoint().Z;
                        Camera c = Camera.GetInstance();
                        saveTemp(c.Image);
                        shotPoint.Add(presentPoint);
                        ++numOfShot;
                        if (OnShort != null) {
                            ActivityEventArgs e = new ActivityEventArgs();
                            e.ZValue = presentPoint;
                            OnShort(this, e);
                        }
                    });
#endif
                                                          mc.Join();
                    presentPoint = mc.GetPoint().Z;
                }

                mc.StopInching(MechaAxisAddress.ZAddress);
            }

            private void saveTemp(BitmapSource image) {
                string name = System.IO.Path.GetTempFileName() + ".png";
                ImageUtility.Save(image, name);
                imagesUri.Add(name);
            }

            private bool isShootContinue(double point) {
                bool flag = false;
                if (endPoint >= startPoint) {
                    flag = (endPoint - point >= 0);
                } else {
                    flag = (endPoint - point <= 0);
                }
                return flag;
            }

            /// <summary>
            /// 撮影終了地点に到着してるかどうかを判定します．
            /// </summary>
            /// <param name="direction">移動方向</param>
            /// <returns>true:到着してる, false:到着していない</returns>
            private bool isOnEndPoint(PlusMinus direction) {
                bool flag = false;
                MotorControler mc = MotorControler.GetInstance(parameterManager);
                double point = mc.GetPoint().Z;
                switch (direction) {
                    case PlusMinus.Plus:
                        flag = (startPoint <= endPoint);
                        break;
                    case PlusMinus.Minus:
                        flag = (startPoint >= endPoint);
                        break;
                }
                return flag;
            }
        }
    }
}
