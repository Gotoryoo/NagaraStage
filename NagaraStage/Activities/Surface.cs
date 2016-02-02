using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Threading;

using NagaraStage;
using NagaraStage.IO;
using NagaraStage.Parameter;

using OpenCvSharp;
using OpenCvSharp.CPlusPlus;

namespace NagaraStage.Activities {
	/// <summary>
	/// 表面認識を行うクラスです．
	/// <para>ゲル，ベース，その他の部分との境界面の認識)を行い，その座標を記録します.</para>
	/// </summary>
	/// <author>Hirokazu Yokoyama</author>
	/// <example>
	/// <code>
	/// /* このクラスを用いて表面認識を行う例を示します． */
	/// public class SurfaceExample {>
	///     public void Main() {
	///         ParameterManager parameterManager = new ParameterManager();
	///         parameterManager.Initialize();
	///         
	///         Surface surface = Surface.GetInstance(parameterManager);
	///         
	///         /* 各イベントハンドラを設定 */
	///         // イベントは表面認識処理を行っているスレッドで実行されます．
	///         // イベントハンドラで時間の掛かる処理を行ってしまうと表面認識処理に支障を来す恐れがあります．
	///         // また，ユーザーインターフェイススレッドとは別になっているためイベントハンドラから
	///         // ユーザーインターフェイスにアクセスするときはDispatcherを経由させるなどをする必要があります．
	///         surface.OnMotorBottomLimited += surface_MotorBottomLimited;
	///         surface.LowBottomRecognized += surface_LowBottomRecognized;
	///         surface.LowTopRecognized += surface_LowTopRecognized;
	///         surface.UpBottomRecognized += surface_UpBottomRecognized;
	///         surface.UpTopRecognized += surface_UpTopRecognized;
	///         surface.Exited += surface_Exited;
	///         
	///         // 念のため，アクティビティがすでに実行中でないかを確認します．
	///         if(surface.IsActive) {
	///             MessageBoxResult result = MessageBox.Show(
	///                 "アクティビティがすでに実行されています．現在実行中のアクティビティを中止して，表面認識を開始しますか？",
	///                 "中止しますか？",
	///                 MessageBoxButton.YesNo);
	///             if(result == MessageBoxResult.Yes) {
	///                 surface.Abort();
	///             } else {
	///                 // 中止
	///                 return;
	///             }
	///         }
	///         
	///         // 表面認識処理を開始します．
	///         surface.Start();
	///     }
	///     
	///     private void surface_MotorBottomLimited(object sender, EventArgs e) {
	///         System.Console.WriteLine("モータが最下点に到着しました．");
	///     }
	///     
	///     private void surface_LowBottomRecognized(object sender, EventArgs e) {
	///         string message = "下ゲル下面を見つけました．位置は" + e.ZValue + "です．";
	///         System.Console.WriteLine(message);
	///     }
	///     
	///     private void surface_LowTopRecognized(object sender, EventArgs e) {
	///         string message = "下ゲル上面を見つけました．位置は" + e.ZValue + "です．";
	///         System.Console.WriteLine(message);
	///     }
	///     
	///     private void surface_UpBottomRecognized(object sender, EventArgs e) {
	///         string message = "上ゲル下面を見つけました．位置は" + e.ZValue + "です．";
	///         System.Console.WriteLine(message);
	///     }
	///     
	///     private void surface_UpTopRecognized(object sender, EventArgs e) {
	///         string message = "上ゲル上面を見つけました．位置は" + e.ZValue + "です．";
	///         System.Console.WriteLine(message);
	///     }
	///     
	///     private void surface_Exited(object sender, ActivityEventArgs e) {
	///         if(e.IsAborted) {
	///             System.Console.WriteLine("表面認識処理は中止されました．");
	///         } else if(e.IsCompleted) {
	///             System.Console.WriteLine("表面認識処理は完了しました．");
	///         } else if(!e.IsCompleted && !e.IsAborted) {
	///             System.Console.WriteLine("表面認識処理は失敗しました．");
	///         }
	///     }
	/// }
	/// </code>
	/// </example>
	public class Surface : Activity, IActivity, ISurface {
		/// <summary>NumOfSicePixcelの初期値</summary>
		public const int NumOfSidePixcelDefault = 9;
		/// <summary>BrightnessThresholdUTの初期値</summary>
		public const int BrightnessThresholdUTDefault = 11000;
		/// <summary>BrightnessThresholdUBの初期値</summary>
		public const int BrightnessThresholdUBDefault = 11000;
		/// <summary>BrightnessThresholdLTの初期値</summary>
		public const int BrightnessThresholdLTDefault = 11000;
		/// <summary>BrightnessThresholdLBの初期値</summary>
		public const int BrightnessThresholdLBDefault = 11000;
		/// <summary>輝度値の微分に対する二値化のしきい値の初期値</summary>
		public const int BinarizeThresholdDefault = 10;
		/// <summary>StartRowの初期値</summary>
		public const int StartRowDefault = 215;
		/// <summary>EndRowの初期値</summary>
		public const int EndRowDefault = 224;
		/// <summary>最下点からどれだけ上方向に境界面を探索するかの初期値</summary>
		public const double DistanceOutDefault = 800;
		/// <summary>境界面判定に用いる多数決の初期値</summary>
		public const int NumOfVotingDefault = 5;
		/// <summary>画素の微分値の累乗値</summary>
		public const double PowerOfDifferenceDefault = 1;
		/// <summary>遅延時間の初期値(msec)</summary>
		public const int DelayTimeDefault = 100;
		/// <summary>表面認識を行うときのモータの移動速度の初期値</summary>
		public const double DefaultSpeed = 0.25;
		/// <summary>厚型エマルションにおける下降距離(mm)の初期値</summary>
		public const double LoweringDistanceThickDefault = 0.3;
		/// <summary>薄型エマルションにおける下降距離(mm)の初期値</summary>
		public const double LoweringDistanceThinDefault = 0.6;

		/// <summary>
		/// 表面認識処理が開催されたときのイベント
		/// </summary>
		public event EventHandler<EventArgs> Started;
		/// <summary>
		/// モータが最下点にたどり着いたときのイベント
		/// </summary>
		public event EventHandler<EventArgs> OnMotorBottomLimited;
		/// <summary>
		/// 撮影及びその入力画像の確認が行われたときのイベント
		/// <para>このイベントのイベントハンドラは表面認識処理を行っているスレッドとも
		/// ユーザーインターフェイススレッドとも異なるスレッドで実行されます．</para>
		/// </summary>
		public event EventHandler<SurfaceEventArgs> Shooting;
		/// 下ゲル下部の表面認識をしたときのイベント
		/// </summary>
		public event EventHandler<ActivityEventArgs> LowBottomRecognized;
		/// <summary>
		/// 下ゲル上部の表面認識をしたときのイベント
		/// </summary>
		public event EventHandler<ActivityEventArgs> LowTopRecognized;
		/// <summary>
		/// 上ゲル下部の表面認識をしたときのイベント
		/// </summary>
		public event EventHandler<ActivityEventArgs> UpBottomRecognized;
		/// <summary>
		/// 下ゲル上部の表面認識をしたときのイベント
		/// </summary>
		public event EventHandler<ActivityEventArgs> UpTopRecognized;



		/// <summary>
		/// 表面認識処理が終了したときのイベント
		/// <para>正常に完了した場合，中止された場合，エラーが発生した場合のすべてを含みます．</para>
		/// </summary>
		public event ActivityEventHandler Exited;

		private static Surface instance;
		private ParameterManager parameterManager;
		private DateTime dateTime;
		private double[] surfaces = new double[4];
		private int numOfSidePixcel = Properties.Settings.Default.SurfNumSide;
		private int brightnessThreshold = Properties.Settings.Default.SurfThreshold;
		private int binarizeThreshold = Properties.Settings.Default.SurfBinThreshold;
		private int startRow = Properties.Settings.Default.SurfStartRow;
		private int endRow = Properties.Settings.Default.SurfEndRow;
		private double distanceOut = Properties.Settings.Default.SurfDistance;
		private double powerOfDiffrence = Properties.Settings.Default.SurfacePower;
		private int numOfVoting = Properties.Settings.Default.SurfaceVoting;
		private double loweringDistance = 0;
		private int brightness = new int();
		private int delayTime = Properties.Settings.Default.SurfaceDelay;
		private double motorSpeed = Properties.Settings.Default.SurfaceMotorSpeed;
		private Vector2 latestCoord = new Vector2();
        private Vector3 speedBeforeStart = new Vector3();
		private System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

		/// <summary>
		/// 表面認識処理を行った日時を取得します．
		/// </summary>
		public DateTime DateTime {
			get { return dateTime; }
		}

		/// <summary>
		/// 表面処理の上昇にかかった秒数を取得します．
		/// 
		/// </summary>
		public double Time {
			get { return stopWatch.Elapsed.TotalSeconds; }
		}

		/// <summary>
		/// 分析において，平均を取る正方形の辺の長さ(pixel数)を設定，または取得します．
		/// <para>分析は，輝度値の平均の合計がBrightnessThresholdを上回るかどうかで行われます．
		/// このプロパティの値は，その輝度値の平均を算出する範囲を表しています．この値が9であれば
		/// 9px * 9px = 81pxの面積で平均が行われます．
		/// </para>
		/// <para>この値は，表面認識処理を実行中に変更することはできません．実行中に
		/// 変更しようとした場合，InActionExceptionが投げられます．</para>
		/// </summary>
		/// <exception cref="NagaraStage.Activities.InActionException"></exception>
		/// <exception cref="System.ArgumentException"></exception>
		public int NumOfSidePixcel {
			get { return numOfSidePixcel; }
			set {
				if (IsActive) {
					throw new InActionException();
				}
				// 値の確認
				if (value <= 0) {
					string message = string.Format(
						Properties.Strings.ValMustPositive,
						Properties.Strings.NumOfSidePixcel);
					throw new ArgumentException(message);
				} else if (value < 2) {
					string message = string.Format(
						Properties.Strings.ValMustOrMore,
						Properties.Strings.NumOfSidePixcel,
						2);
					throw new ArgumentException(message);
				}
				numOfSidePixcel = value;
				Properties.Settings.Default.SurfNumSide = value;
			}
		}

		/// <summary>
		/// 判定に用いいる，輝度値のしきい値を取得，または設定します．
		/// <para>エマルションの輝度値の平均の合計を算出した結果とこの値を比較して，判定を行います．</para>
		/// <para>この値は，表面認識処理を実行中に変更することはできません．</para>
		/// </summary>
		/// <exception cref="NagaraStage.Activities.InActionException"></exception>
		/// <exception cref="System.ArgumentException"></exception>
		public int BrightnessThreshold {
			get { return brightnessThreshold; }
			set {
				if (IsActive) {
					throw new InActionException();
				}
				if (value <= 0) {
					throw new ArgumentException(Properties.Strings.ValMustPositive);
				}
				brightnessThreshold = value;
				Properties.Settings.Default.SurfThreshold = value;
			}
		}

		/// <summary>
		/// 輝度値の微分に対する二値化のしきい値を取得，または設定します．
		/// <para>0以下の値を指定した場合，二値化は行われなくなります．</para>
		/// </summary>
		public int BinarizeThreshold {
			get { return binarizeThreshold; }
			set {
				if (IsActive) {
					throw new InActionException();
				}
				binarizeThreshold = value;
				Properties.Settings.Default.SurfBinThreshold = value;
			}
		}

		/// <summary>
		/// 判定に用いる範囲の，開始行を取得，または設定します．
		/// <para>この値は，表面認識処理を実行中に変更することはできません．</para>
		/// </summary>
		/// <exception cref="NagaraStage.Activities.InActionException"></exception>
		/// <exception cref="System.ArgumentException"></exception>
		public int StartRow {
			get { return startRow; }
			set {
				if (IsActive) {
					throw new InActionException();
				}
				if (value < 0) {
					throw new ArgumentException(Properties.Strings.ValMustPositive);
				}
				startRow = value;
				Properties.Settings.Default.SurfStartRow = value;
			}
		}

		/// <summary>
		/// 判定に用いる範囲の，終了行の取得，または設定します．
		/// <para>この値は，表面認識処理を実行中に変更することはできません．</para>
		/// </summary>
		/// <exception cref="NagaraStage.Activities.InActionException"></exception>
		/// <exception cref="System.ArgumentException"></exception>
		public int EndRow {
			get { return endRow; }
			set {
				if (IsActive) {
					throw new InActionException();
				}
				if (value < 0) {
					throw new ArgumentException(Properties.Strings.ValMustPositive);
				}
				endRow = value;
				Properties.Settings.Default.SurfEndRow = value;
			}
		}

		/// <summary>
		/// 表面認識処理中，上昇時に，処理を中止するための距離を取得，または設定します．
		/// <para>この値は，表面認識処理を実行中に変更することはできません．</para>
		/// </summary>
		/// <exception cref="NagaraStage.Activities.InActionException"></exception>
		/// <exception cref="System.ArgumentException"></exception>
		public double DistanceOut {
			get { return distanceOut; }
			set {
				if (IsActive) {
					throw new InActionException();
				}
				if (value <= 0) {
					throw new ArgumentException(Properties.Strings.ValMustPositive);
				}
				distanceOut = value;
				Properties.Settings.Default.SurfDistance = value;
			}
		}

		/// <summary>
		/// 微分値をいくつ累乗するするかを設定，または取得します．
		/// </summary>
		/// <exception cref="InActionException">アクティビティの実行中に設定された場合</exception>
		/// <exception cref="System.ArgumentException">0以下の値が指定された場合</exception>
		public double PowerOfDiffrence {
			get { return powerOfDiffrence; }
			set {
				if (IsActive) {
					throw new InActionException();
				}
				if (value <= 0) {
					throw new ArgumentException(Properties.Strings.ValMustPositive);
				}
				powerOfDiffrence = value;
				Properties.Settings.Default.SurfacePower = value;
			}
		}

		/// <summary>
		/// ゲル確認で多数決をとる回数を設定，または取得します．
		/// <para>設定する値は奇数でなければいけません．</para>
		/// </summary>
		/// <exception cref="InActionException">アクティビティの実行中に設定された場合</exception>
		/// <exception cref="System.ArgumentException">
		/// 0以下の値が設定された場合，もしくは偶数が設定された場合</exception>
		public int NumOfVoting {
			get { return numOfVoting; }
			set {
				if (IsActive) {
					throw new InActionException();
				}
				if (value <= 0) {
					throw new ArgumentException(Properties.Strings.ValMustPositive);
				}
				if (value % 2 != 1) {
					throw new ArgumentException(Properties.Strings.ValMustOdd);
				}
				numOfVoting = value;
				Properties.Settings.Default.SurfaceVoting = value;
			}
		}

		/// <summary>
		/// ゲル確認の毎回の遅延時間をミリ秒単位で設定，または取得します．
		/// </summary>
		public int DelayTime {
			get { return delayTime; }
			set {
				if (delayTime < 0) {
					throw new ArgumentOutOfRangeException(Properties.Strings.ValMustPositive);
				}
				delayTime = value;
				Properties.Settings.Default.SurfaceDelay = value;
			}
		}

		/// <summary>
		/// 表面認識を行うときのモータの移動速度を設定,または取得します.
		/// <para>値は0より大きい値でなければいけません．</para>
		/// </summary>
		/// <exception cref="NagaraStage.Activities.InActionException"></exception>
		/// <exception cref="System.ArgumentException"></exception>
		public double MotorSpeed {
			get { return motorSpeed; }
			set {
				if (IsActive) {
					throw new InActionException();
				}
				if (value <= 0) {
					throw new ArgumentException(Properties.Strings.ValMustPositiveNotZero);
				}
				motorSpeed = value;
				Properties.Settings.Default.SurfaceMotorSpeed = value;
			}
		}

		/// <summary>
		/// 厚型エマルション使用時の下がりの距離を設定，または取得します．
		/// </summary>
		public double LoweringDistanceThick {
			get { return Properties.Settings.Default.SurfaceLoweringThick; }
			set {
				Properties.Settings.Default.SurfaceLoweringThick = value;
			}
		}

		/// <summary>
		/// 薄型エマルション使用時の下がりの距離を設定，または取得します．
		/// </summary>
		public double LoweringDistanceThin {
			get { return Properties.Settings.Default.SurfaceLoweringThin; }
			set { Properties.Settings.Default.SurfaceLoweringThin = value; }
		}

		/// <summary>
		/// 設定されたプロパティの値が適切であるかどうかを取得します．
		/// </summary>
		public Boolean IsAvairableValue {
			get {
				bool flag = true;
				flag = (StartRow < EndRow);

				int height = ParameterManager.ImageResolution.Height;
				flag &= (StartRow > (NumOfSidePixcel / 2));
				flag &= (EndRow + (NumOfSidePixcel / 2) <= height);
				return flag;
			}
		}

		/// <summary>
		/// ゲル内にいるのかどうかの指標となる輝度値を取得します．
		/// </summary>
		public int Brightness {
			get {
				return brightness;
			}
		}

		/// <summary>上ゲル上面の座標を取得します．</summary>
		public double UpTop {
			get { return surfaces[0]; }
		}

		/// <summary>上ゲル下面の座標を取得します．</summary>
		public double UpBottom {
			get { return surfaces[1]; }
		}

		/// <summary>下ゲル上面の座標を取得します．</summary>
		public double LowTop {
			get { return surfaces[2]; }
		}

		/// <summary>下ゲル下面の座標を取得します．</summary>
		public double LowBottom {
			get { return surfaces[3]; }
		}

		/// <summary>表面認識を最後に行った位置を取得します．</summary>
		public Vector2 LatestCoord {
			get { return latestCoord; }
		}

		private Surface(ParameterManager _parameterManager)
			: base(_parameterManager) {
			if (_parameterManager == null) {
				throw new ArgumentNullException();
			}
			this.parameterManager = _parameterManager;
			initializeSurfaces();
			Exited += delegate(object sender, ActivityEventArgs e) {
				MotorControler mc = MotorControler.GetInstance(parameterManager);
				mc.SetMotorSpeed(
					speedBeforeStart.X,
					speedBeforeStart.Y,
					speedBeforeStart.Z);
			};
		}

		/// <summary>
		/// インスタンスを取得します．
		/// </summary>
		/// <returns>インスタンス</returns>
		public static Surface GetInstance(ParameterManager parameterManager = null) {
			if (instance == null) {
				instance = new Surface(parameterManager);
			}
			return instance;
		}

		/// <summary>
		/// 表面認識処理を開始します．
		/// </summary>
		/// <param name="fast">
		/// trueを指定した場合，最初の下降距離がエマルションのおおよその厚さ分になり，
		/// 高速に表面認識を行うことができます．
		/// ただし，開始前にエマルションの上ゲル上面に位置していることが前提の表面認識です．
		/// </param>
		/// <exception cref="NagaraStage.ArgumentException">プロパティの設定値が不正な場合</exception>
		public void Start(bool fast) {
			if (!IsAvairableValue) {
				throw new ArgumentException();
			}

			if (fast) {
				loweringDistance = (parameterManager.EmulsionType == EmulsionType.ThickType
					? Properties.Settings.Default.SurfaceLoweringThick
					: Properties.Settings.Default.SurfaceLoweringThin);
			} else {
				loweringDistance = 0;
			}
			initializeSurfaces();
			Thread recognizeThread = Create(recogThread_Task, Exited);
			recognizeThread.Start();
		}

		/// <summary>
		/// 表面認識処理を開始します．
		/// </summary>
		[Obsolete("アクティビティキューイングに移行するため、そのうち廃止します。")]
		public void Start() {
			Start(false);
		}

		/// <summary>
		/// 現在撮影中の位置がゲルの中であるかどうかを取得します．
		/// </summary>
		/// <returns>True: ゲルの中, False: ゲルの外</returns>
		public bool IsInGel() {
			Camera camera = Camera.GetInstance();
			byte[] b = camera.ArrayImage;
			Mat mat0 = new Mat(440, 512, MatType.CV_8U, b);
            Mat mat = mat0.Clone();

			Cv2.GaussianBlur(mat, mat, Cv.Size(3, 3), -1);

			Mat gau = mat.Clone();
			Cv2.GaussianBlur(gau, gau, Cv.Size(31, 31), -1);
			Cv2.Subtract(gau, mat, mat);
			Cv2.Threshold(mat, mat, BinarizeThreshold, 1, ThresholdType.Binary);
			brightness = Cv2.CountNonZero(mat);

			return (brightness > BrightnessThreshold);    
		}


        /// <summary>
        /// 現在撮影している画像をDiffence of Gaussianm→二値化し、hitピクセルの数を数えます。
        /// </summary>
        /// <returns>hitピクセルの数</returns>
        public int CountHitPixels() {
            Camera camera = Camera.GetInstance();
            byte[] b = camera.ArrayImage;
            Mat mat0 = new Mat(440, 512, MatType.CV_8U, b);
            Mat mat = mat0.Clone();

            Cv2.GaussianBlur(mat, mat, Cv.Size(3, 3), -1);

            Mat gau = mat.Clone();
            Cv2.GaussianBlur(gau, gau, Cv.Size(31, 31), -1);
            Cv2.Subtract(gau, mat, mat);
            Cv2.Threshold(mat, mat, BinarizeThreshold, 1, ThresholdType.Binary);
            brightness = Cv2.CountNonZero(mat);

            return brightness;
        }


        /// <summary>
        /// 表面の位置を記録した配列を初期化します．
        /// </summary>
		private void initializeSurfaces() {
			for (int i = 0; i < surfaces.Length; ++i) {
				surfaces[i] = new double();
			}
		}


		/// <summary>
		/// recogThreadの行う処理です．このメソッドを直接呼び出さないでください．
		/// </summary>
		private void recogThread_Task() {
			if (Started != null) {
				Started(this, new EventArgs());
			}

			Camera camera = Camera.GetInstance();
			camera.Start();

			/* Z軸をSpeed1(低速)でマイナス方向に動かして，最下点もしくは下降距離分だけ移動させる．*/
			MotorControler mc = MotorControler.GetInstance(parameterManager);
			speedBeforeStart = mc.Speed;


            bool flag;
            double temp_z;

            mc.Inch(PlusMinus.Minus, motorSpeed, VectorId.Z);
            flag = true;
            while (flag) {
                int brightness = CountHitPixels();
                if (brightness > 8000) flag = false;
            }
            /*
			UpTopRecognized(this, new eventArgs());
            LowBottomRecognized(this, new eventArgs());
			LowTopRecognized(this, new eventArgs());
			UpBottomRecognized(this, new eventArgs());
        	*/
            mc.SlowDownStop(VectorId.Z);
            surfaces[0] = mc.GetPoint().Z;
            System.Diagnostics.Debug.WriteLine(string.Format("{0}", mc.GetPoint().Z));
            Thread.Sleep(100);


            mc.Move(new Vector3(0.0, 0.0, -0.2));
            mc.Join();
            Thread.Sleep(100);


            mc.Inch(PlusMinus.Minus, motorSpeed, VectorId.Z);
            flag = true;
            while (flag) {
                int brightness = CountHitPixels();
                if (brightness < 4000) flag = false;
            }


            surfaces[2] = mc.GetPoint().Z;
            System.Diagnostics.Debug.WriteLine(string.Format("{0}", mc.GetPoint().Z));

            
            flag = true;
            while (flag) {
                int brightness = CountHitPixels();
                if (brightness > 4000) flag = false;
            }


            mc.SlowDownStop(VectorId.Z);
            surfaces[1] = mc.GetPoint().Z;
            System.Diagnostics.Debug.WriteLine(string.Format("{0}", mc.GetPoint().Z));
            Thread.Sleep(100);

            mc.Move(new Vector3(0.0, 0.0, -0.2));
            mc.Join();
            Thread.Sleep(100);

            mc.Inch(PlusMinus.Minus, motorSpeed, VectorId.Z);

            flag = true;
            while (flag) {
                int brightness = CountHitPixels();
                if (brightness < 2000) flag = false;
            }

            mc.SlowDownStop(VectorId.Z);
            surfaces[3] = mc.GetPoint().Z;
            System.Diagnostics.Debug.WriteLine(string.Format("{0}", mc.GetPoint().Z));
            Thread.Sleep(100);

            temp_z = (surfaces[0] - 0.05) - mc.GetPoint().Z;
            mc.Move(new Vector3(0.0, 0.0, temp_z));
            mc.Join();
            Thread.Sleep(100);


            /*
			// モータZ軸をマイナス方向(下方向)に稼働させる
			try {
				// 下降の開始
				mc.SetMotorSpeed(IO.MotorSpeed.Speed3);
				mc.Inch(MechaAxisAddress.ZAddress, PlusMinus.Plus);
			} catch (MotorAxisException) {
				// すでに最下点に位置していた場合は，catchのスコープが実行される．
				isOnBottomLimit = true;
			}

			// 最下点もしくは下降距離分に下降したかを監視する．
			double startPoint = mc.GetPoint().Z;
			while (!isOnBottomLimit) {
				// 移動距離の確認
				if (loweringDistance != 0) {
					isOnBottomLimit = (loweringDistance <= Math.Abs(mc.GetPoint().Z - startPoint));
				}

				// モータの移動可能最下点到達か否かの確認
				isOnBottomLimit |=
					(mc.GetAbnomalState(MechaAxisAddress.ZAddress)
					== MotorAbnomalState.AxisLimitPlus);
			}
			// 最下点に到達したため，モータの移動を止める．
			// 最下点に到達時のイベントを発生させる
			mc.StopInching(MechaAxisAddress.ZAddress);
			if (OnMotorBottomLimited != null) {
				OnMotorBottomLimited(this, new EventArgs());
			}

			// 遅延を行わないとモータドライバが動作不良を起こす場合がある.
			Thread.Sleep(300);

             * */
            


			/* -------------------- 下降の完了 ---------------- */

			/* 上昇しながら撮影中の画像を分析する */
			// 最下点からZ軸をプラス方向に動かす．
			// 撮影中の画像がゲルの中か否かを判定する．
			// ベース及びその他の場所と，ゲルの中の境界である座標を記録する．


/*


			mc.Inch(PlusMinus.Plus, motorSpeed, VectorId.Z);
			startPoint = mc.GetPoint().Z;


           // bool flag = true;

            while (flag) {
                //Thread.Sleep(delayTime);

                byte[] b = camera.ArrayImage;
                Mat src = new Mat(440, 512, MatType.CV_8U, b);
                Mat mat = src.Clone();

                Cv2.GaussianBlur(mat, mat, Cv.Size(3, 3), -1);
                Mat gau = mat.Clone();
                Cv2.GaussianBlur(gau, gau, Cv.Size(31, 31), -1);
                Cv2.Subtract(gau, mat, mat);
                Cv2.Threshold(mat, mat, 10, 1, ThresholdType.Binary);
                int brightness = Cv2.CountNonZero(mat);

                if (brightness > 3000) flag = false;
            }

            mc.StopInching(MechaAxisAddress.ZAddress);
            surfaces[3] = mc.GetPoint().Z;
            System.Diagnostics.Debug.WriteLine(string.Format("{0}", mc.GetPoint().Z));



          //  Vector3 speed = new Vector3(30, 30, 0.4);//つかわないけど
           // Vector3 tolerance = new Vector3(0.001, 0.001, 0.001);//つかわないけど
          //  Vector3 distance = new Vector3(0, 0, 0.3);
          //  mc.Move(distance, speed, tolerance);

            Thread.Sleep(100);


            */

            /*


            mc.Inch(PlusMinus.Plus, MotorSpeed, VectorId.Z);
            double currentPoint = mc.GetPoint().Z;

            bool dice = true;

            while (dice) {

                double nowPoint = mc.GetPoint().Z;

                double renge = nowPoint - currentPoint;

                if (renge > 0.20) dice = false;




             }
            mc.StopInching(MechaAxisAddress.ZAddress);
            
            Thread.Sleep(100);
             * 
             * 
             * /


          //  mc.MoveDistance(0.2, VectorId.Z);
         //   mc.Join();
           // Thread.Sleep(2000);
          //  System.Diagnostics.Debug.WriteLine(string.Format("{0}", mc.GetPoint().Z));
/*

            mc.Inch(PlusMinus.Plus, motorSpeed, VectorId.Z);
            flag = true;
            while (flag) {

                byte[] b = camera.ArrayImage;
                Mat src = new Mat(440, 512, MatType.CV_8U, b);
                Mat mat = src.Clone();

                Cv2.GaussianBlur(mat, mat, Cv.Size(3, 3), -1);
                Mat gau = mat.Clone();
                Cv2.GaussianBlur(gau, gau, Cv.Size(31, 31), -1);
                Cv2.Subtract(gau, mat, mat);
                Cv2.Threshold(mat, mat, 10, 1, ThresholdType.Binary);
                int brightness = Cv2.CountNonZero(mat);

                if (brightness < 4000) flag = false;
            }


            surfaces[2] = mc.GetPoint().Z;
            System.Diagnostics.Debug.WriteLine(string.Format("{0}", mc.GetPoint().Z));

            */


/*


            flag = true;
            while (flag) {

                byte[] b = camera.ArrayImage;
                Mat src = new Mat(440, 512, MatType.CV_8U, b);
                Mat mat = src.Clone();

                Cv2.GaussianBlur(mat, mat, Cv.Size(3, 3), -1);
                Mat gau = mat.Clone();
                Cv2.GaussianBlur(gau, gau, Cv.Size(31, 31), -1);
                Cv2.Subtract(gau, mat, mat);
                Cv2.Threshold(mat, mat, 10, 1, ThresholdType.Binary);
                int brightness = Cv2.CountNonZero(mat);

                if (brightness > 4000) flag = false;
            }

            mc.StopInching(MechaAxisAddress.ZAddress);
            surfaces[1] = mc.GetPoint().Z;
            System.Diagnostics.Debug.WriteLine(string.Format("{0}", mc.GetPoint().Z));


            //mc.Move(distance, speed, tolerance);


         




            // 遅延を行わないとモータドライバが動作不良を起こす場合がある.
            Thread.Sleep(100);


            */

/*
            
            mc.Inch(PlusMinus.Plus, MotorSpeed, VectorId.Z);
            double current2Point = mc.GetPoint().Z;

            bool dice2 = true;

            while (dice2) {

                double nowPoint = mc.GetPoint().Z;

                double renge = nowPoint - current2Point;

                if (renge > 0.20) dice2 = false;




             }

            mc.StopInching(MechaAxisAddress.ZAddress);

            Thread.Sleep(100);

            */

           // mc.MoveDistance(0.2, VectorId.Z);
           // mc.Join();
            
          //  Thread.Sleep(2000);
           // System.Diagnostics.Debug.WriteLine(string.Format("{0}", mc.GetPoint().Z));

  /*          mc.Inch(PlusMinus.Plus, motorSpeed, VectorId.Z);
            flag = true;
            while (flag) {

                byte[] b = camera.ArrayImage;
                Mat src = new Mat(440, 512, MatType.CV_8U, b);
                Mat mat = src.Clone();

                Cv2.GaussianBlur(mat, mat, Cv.Size(3, 3), -1);
                Mat gau = mat.Clone();
                Cv2.GaussianBlur(gau, gau, Cv.Size(31, 31), -1);
                Cv2.Subtract(gau, mat, mat);
                Cv2.Threshold(mat, mat, 10, 1, ThresholdType.Binary);
                int brightness = Cv2.CountNonZero(mat);

                if (brightness < 12000) flag = false;
            }
            mc.StopInching(MechaAxisAddress.ZAddress);
            surfaces[0] = mc.GetPoint().Z;
            System.Diagnostics.Debug.WriteLine(string.Format("{0}", mc.GetPoint().Z));


             
           */
              
          
            //////////////

          //  mc.AAAAAA();
            
            
            /*
			stopWatch = new System.Diagnostics.Stopwatch();
			stopWatch.Start();

			int idNo = 0;
			int index = 3;
#if false
			bool[] votes = new bool[numOfVoting];
			int[] values = new int[numOfVoting];
			double[] points = new double[numOfVoting];

				Thread.Sleep(delayTime);

				// 今回の入力画像の演算結果を多数決配列の最後尾に設定する．
				// 同時に今回の判定値やモータの座標もそれぞれ保持する．
				votes[votes.Length - 1] = IsInGel();
				values[values.Length - 1] = brightness;
				points[points.Length - 1] = mc.GetPoint().Z;

				// 今回と過去一定回数分の結果で多数決をとる．
				presentResult = Vote(votes);

				// 多数決の結果配列を一つずつシフトする
				for (int i = 0; i < values.Length; ++i) {
					votes[i] = (i == votes.Length - 1 ? false : votes[i + 1]);
					values[i] = (i == values.Length - 1 ? 0 : values[i + 1]);
					points[i] = (i == points.Length - 1 ? 0 : points[i + 1]);
				}
#endif

			double previousZVal, presentZVal;
			int previousBrightness , presentBrightness;
			bool previousResult = false, presentResult = false;
			while (index >= 0) {
				Thread.Sleep(delayTime);
				presentZVal = Math.Round(mc.GetPoint().Z, 5);
				presentResult = IsInGel();
				bool isBorder = isBoudarySurface(previousResult, presentResult, index);

				// 撮像・入力画像の確認のイベントを発生させる．
				SurfaceEventArgs eventArgs = new SurfaceEventArgs();
				eventArgs.Id = idNo;
#if false
				eventArgs.ZValue = points[0];
				eventArgs.Brightness = values[0];
				eventArgs.Distance = Math.Abs(points[0] - startPoint);
#endif

				presentBrightness = brightness;
				eventArgs.ZValue = presentZVal;
				eventArgs.Brightness = presentBrightness;
				eventArgs.Distance = Math.Abs(presentZVal - startPoint);
				eventArgs.IsInGel = presentResult;
				eventArgs.IsBoundary = isBorder;
				eventArgs.Second = Time;
				++idNo;
				if (Shooting != null) {
					Thread t = new Thread(new ThreadStart(delegate() {
						Shooting(this, eventArgs);
					}));
					t.IsBackground = true;
					t.Start();
				}

				if (isBorder) {
					// 境界面であった場合，多数決に用いたZ座標の値の内，最も古い値を境界面の座標として採用する．
					// ここでは境界面として，その座標値を保持している．
					//surfaces[index] = points[0];
					surfaces[index] = presentZVal;

					// 境界面認識時のイベントを発生させる．                    
					switch (index) {
						case 3:
							if (LowBottomRecognized != null) {
								LowBottomRecognized(this, eventArgs);
							}
							break;
						case 2:
							if (LowTopRecognized != null) {
								LowTopRecognized(this, eventArgs);
							}
							break;
						case 1:
							if (UpBottomRecognized != null) {
								UpBottomRecognized(this, eventArgs);
							}
							break;
						case 0:
							if (UpTopRecognized != null) {
								UpTopRecognized(this, eventArgs);
							}
							break;
					}

					--index;
				}
				previousResult = presentResult;
				previousBrightness = presentBrightness;
				previousZVal = presentZVal;

				// 距離によるエラー終了
				if (mc.GetPoint().Z - startPoint > distanceOut) {
					mc.StopInching(MechaAxisAddress.ZAddress);
					throw new SurfaceFailedException("Distance limit out");
				}
			}

			mc.StopInching(MechaAxisAddress.ZAddress);
			stopWatch.Stop();
			dateTime = DateTime.Now;
			latestCoord = new Vector2(mc.GetPoint().X, mc.GetPoint().Y);
            */
		}

		/// <summary>
		/// ゲルの境界面であるかを取得します．
		/// <para>境界面であればtrue</para>
		/// </summary>
		/// <param name="previousCall">前回の判定結果</param>
		/// <param name="presentCall">今回の判定結果</param>
		/// <param name="index">現在の位置を表すインデックス</param>
		/// <returns></returns>
		private bool isBoudarySurface(bool previousCall, bool presentCall, int index) {
			bool flag = false;
			switch (index) {
				case 1:
				case 3:
					flag = presentCall && !previousCall;
					break;
				case 2:
				case 0:
					flag = !presentCall && previousCall;
					break;
			}
			return flag;
		}

		/// <summary>
		/// 与えられたbool型配列を多い方を返します.
		/// </summary>
		/// <param name="votes"></param>
		/// <returns></returns>
		public static bool Vote(bool[] votes) {
			if (votes.Length % 2 != 1) {
				throw new ArgumentException(Properties.Strings.ValMustOdd);
			}
			int num = 0;
			for (int i = 0; i < votes.Length; ++i) {
				if (votes[i]) {
					++num;
				} else {
					num -= 2;
				}
			}
			return (num >= 0);
		}

		/// <summary>
		/// 表面認識の実行スレッドを作成します。
		/// このクラスはActivityManagerからのみ呼び出すようにしてください。
		/// </summary>
		/// <returns></returns>
		public List<Thread> CreateTask() {
			List<Thread> taskList = new List<Thread>();
			taskList.Add(Create(new ThreadStart(recogThread_Task)));
			return taskList;
		}
	}

}