using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using NagaraStage;
using NagaraStage.Activities;
using NagaraStage.IO;
using NagaraStage.IO.Driver;
using NagaraStage.Parameter;

using OpenCvSharp;
using OpenCvSharp.CPlusPlus;

namespace NagaraStage.Ui {

    /// <summary>
    /// Stage.xaml の相互作用ロジック
    /// </summary>
    public partial class Stage : KeyHandleWorkspace {
        private IMainWindow window;
        private ParameterManager parameterManager;
        private CoordManager coordManager;
        private DispatcherTimer parametersTimer;
        private MessageList messages;
        private Action<object, MouseButtonEventArgs> viewMouseDownAction;

        /// <summary>
        /// エマルションビュワー上でMouseDownイベント発生時に行う処理を設定，または取得します．
        /// <para>引数はエマルションビュワー上の描画用のレイヤーが与えられます．</para>
        /// </summary>
        public Action<object, MouseButtonEventArgs> ViewerMouseDownAction {
            get { return viewMouseDownAction; }
            set { viewMouseDownAction = value; }
        }

        /// <summary>
        /// 設定されているステージコンテントを取得，または設定します．
        /// </summary>
        public object StageContent {
            get { return stageContent.Content; }
            set { stageContent.Content = value; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mainWindow">親コントロール</param>
        public Stage(IMainWindow mainWindow)
            : base(mainWindow) {
            this.window = mainWindow;
            this.parameterManager = window.ParameterManager;
            this.coordManager = window.CoordManager;
            InitializeComponent();

            this.KeyDown += KeyHandleStageControl_KeyDown_1;

            this.parameterManager.PlateNoChanged += emulsionNumber_Changed;
            this.parameterManager.ModuleNoChanged += emulsionNumber_Changed;
            this.parameterManager.EmulsionTypeChanged += emulsionType_Changed;
            this.parameterManager.LensTypeChanged += lensType_Changed;
            this.window.RibbonTabSelected += windowRibbonTab_Selected;
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            mc.SpiralMoved += spiralMoved_Completed;

            this.parametersTimer = new DispatcherTimer(DispatcherPriority.Normal);
            this.parametersTimer.Interval = new TimeSpan(ParameterManager.ParamtersIntervalMilliSec * 1000);
            this.parametersTimer.Tick += parametersTimer_Tick;            
            this.messages = new MessageList();
            this.messages.CallbackOfAdd = delegate(string latestStr) {
                Dispatcher.BeginInvoke(new Action(delegate() {
                    string wrapCode = System.Environment.NewLine;
                    infoTextBox.Text = latestStr + wrapCode + infoTextBox.Text;
                }), null);
            };
            this.emulsionViewerCanvas.MouseDown += delegate(object sender, MouseButtonEventArgs e) {
                if (viewMouseDownAction != null) {
                    viewMouseDownAction(sender, e);
                }
            };

            parametersTimer.Start();
#if !NoHardware
            Led led = Led.GetInstance();
            led.SetDcPower(0);
            led.OnPulse();
#endif
            this.Focus();

            //Mat mat = new Mat(512, 440, MatType.CV_8U);
            //mat.ImWrite(@"c:\aaaaaa.bmp");
            Ipt.SetHyperFineXY(0.04, 0.01);
        }

        /// <summary>
        /// windowのタブが選択されたときのイベントハンドラです．
        /// </summary>
        private void windowRibbonTab_Selected(object sender, RibbonTabEventArgs e) {
            switch (e.SelectedTabName) {
                case "HomeTab":
                    windowHomeTab_Selected(sender, e);
                    break;
                case "coordTab":
                    windowCoordTab_Selected(sender, e);
                    break;
                case "fullscanTab":
                    break;
                case "imageEnhanceTab":
                    windowImageEnhanceTab_Selected(sender, e);
                    break;
            }
        }

        /// <summary>
        /// windowのホームタブが選択されたときのイベントハンドラです.
        /// </summary>
        private void windowHomeTab_Selected(object sender, RibbonTabEventArgs e) {
            ViewerMouseDownAction = null;
            emulsionViewerCanvas.Cursor = Cursors.Arrow;

            StageContent = new TrackStageControl(window);
        }

        /// <summary>
        /// windowのCoordTabが選択されたときのイベントハンドラです．
        /// </summary>
        private void windowCoordTab_Selected(object sender, RibbonTabEventArgs e) {
            StageContent = new CoordControl(window);

            // エマルションビュワー上でマウスのクリックイベントに，グリッドマークを
            // 決定する処理を行うイベントを追加
            emulsionViewerCanvas.Cursor = Cursors.Pen;
            ViewerMouseDownAction = delegate(object s, MouseButtonEventArgs e2) {
                // レンズタイプが10倍でなければメッセージを表示して中止
                if (parameterManager.Magnification != ParameterManager.LensMagnificationOfGridMarkSearch) {
                    string message = Properties.Strings.LensTypeException01
                        + string.Format(Properties.Strings.CorrectLens,
                            ParameterManager.LensMagnificationOfGridMarkSearch.ToString());
                    MessageBox.Show(message, Properties.Strings.Error);
                    return;
                }
                // クリックされた座標にバツ印を表示して，グリッドマークを定義するダイ
                // アログボックスを表示
                double clickedX = e2.GetPosition(emulsionViewerCanvas).X;
                double clickedY = e2.GetPosition(emulsionViewerCanvas).Y;
                ShowGridMarkDifinitionUi(clickedX, clickedY);
            };
        }

        private void windowImageEnhanceTab_Selected(object sender, RibbonTabEventArgs e) {
        }

        /// <summary>
        /// らせん移動が完了したときのイベントハンドラです．
        /// </summary>
        private void spiralMoved_Completed(object sender, SpiralEventArgs e) {
            string str = string.Format("{0:0}, {1:0}", e.X, e.Y);
            Dispatcher.BeginInvoke(new Action(delegate {
                spiralLabel.Content = str;
            }));
        }

        /* --------------------------------------------------------------------
         * 表面認識のイベントハンドラ
         --------------------------------------------------------------------*/

        /// <summary>
        /// 表面認識が開始されたときのイベントハンドラです．
        /// </summary>
        private void surface_Started(object sender, EventArgs e) {
            Dispatcher.BeginInvoke(new Action(delegate {

            }), null);
        }

        /// <summary>
        /// 表面認識時にモータ移動が最下点に到達したときのイベントハンドラです．
        /// </summary>
        private void surfaceOnMotorBottom_Limited(object sender, EventArgs e) {
            Dispatcher.BeginInvoke(new Action(delegate {
                WriteLine(Properties.Strings.SurfaceOnBttomLimited);
            }), null);
        }

        /// <summary>
        /// 表面認識処理にて，下ゲル下部を認識したときのイベントハンドラです．
        /// </summary>
        private void surfaceLowBottom_Recognized(object sender, ActivityEventArgs e) {
            Dispatcher.BeginInvoke(new Action(delegate {
                string message = string.Format(Properties.Strings.SurfaceDownBottomRecognized, e.ZValue);
                WriteLine(message);
            }), null);
        }

        /// <summary>
        /// 表面認識処理にて，下ゲル上部を認識したときのイベントハンドラです．
        /// </summary>
        private void surfaceLowTop_Recognized(object sender, ActivityEventArgs e) {
            Dispatcher.BeginInvoke(new Action(delegate {
                string message = string.Format(Properties.Strings.SurfaceDownTopRecognized, e.ZValue);
                WriteLine(message);
            }), null);
        }

        /// <summary>
        /// 表面認識処理にて，上ゲル下部を認識したときのイベントハンドラです．
        /// </summary>
        private void surfaceUpBottom_Recognized(object sender, ActivityEventArgs e) {
            Dispatcher.BeginInvoke(new Action(delegate {
                string message = string.Format(Properties.Strings.SurfaceUpBottomRecognized, e.ZValue);
                WriteLine(message);
            }), null);
        }

        /// <summary>
        /// 表面認識処理にて，上ゲル上部を認識したときのイベントハンドラです．
        /// </summary>
        private void surfaceUpTop_Recognized(object sender, ActivityEventArgs e) {
            Dispatcher.BeginInvoke(new Action(delegate {
                string message = string.Format(Properties.Strings.SurfaceUpTopRecognizing, e.ZValue);
                WriteLine(message);
            }), null);
        }

        /// <summary>
        /// 表面認識処理が終了したときのイベントハンドラです．
        /// </summary>
        private void surface_Exited(object sender, ActivityEventArgs e) {
            string message = "";
            if (e.IsAborted) {
                message = Properties.Strings.AbortedSurfaceRecognizing;
            } else if (e.IsCompleted) {
                message = Properties.Strings.SurfaceComplete;
            } else if (!e.IsCompleted && !e.IsAborted) {
                message = string.Format(
                    "{0} {1}: {2}",
                    Properties.Strings.Surface,
                    Properties.Strings.Error,
                    e.Exception.Message);
            }
            Dispatcher.BeginInvoke(new Action(delegate {
                WriteLine(message);
                window.IsTabsEnabled = true;
            }), null);
            IsControllable = true;
        }

        /* --------------------------------------------------------------------
         * グリッドマーク探索のイベントハンドラ
         --------------------------------------------------------------------*/

        /// <summary>
        /// グリッドマーク自動検索処理が開始されたときのイベントハンドラです．
        /// </summary>
        private void gridMark_Started(object sender, EventArgs e) {
            Dispatcher.BeginInvoke(new Action(delegate {
                WriteLine(Properties.Strings.PressAnyKeyForAbort);
                WriteLine(Properties.Strings.GridMarkSearchStart);
            }));
        }

        /// <summary>
        /// グリッドマーク自動検索処理でグリッドマークが見つかったときのイベントハンドラです．
        /// </summary>
        private void gridMark_Found(object sender, GridMarkEventArgs e) {
            Dispatcher.BeginInvoke(new Action(delegate {
                DrawCrossMark(e.ViewerPoint.X, e.ViewerPoint.Y);
                string gridmark = GridMarkDefinitionUtil.ToString(e.GridMarkPoint);
                string message = string.Format(Properties.Strings.FoundNow, gridmark, e.EncoderPoint.X, e.EncoderPoint.Y);
                WriteLine(message);
            }));
        }

        /// <summary>
        /// グリッドマーク自動検索処理が終了したときのイベントハンドラです．
        /// </summary>
        private void gridMark_Exited(object sender, ActivityEventArgs e) {
            Dispatcher.BeginInvoke(new Action(delegate {
                if (e.IsAborted) {
                    WriteLine(Properties.Strings.AbortGridMarkSearch);
                } else if (e.IsCompleted) {
                    WriteLine(Properties.Strings.GridMarkSearchComplete);
                }
            }));
        }

        /// <summary>
        /// グリッドマークを定義するために描画，ダイアログボックスなどのUIを提供し，
        /// グリッドマークを定義します．
        /// <para>ステージのビュワーに，グリッドマークとして定義する座標にバツ印を描画します．
        /// そして，どのグリッドマークとして定義するかをユーザに確認し，
        /// 選択されたグリッドマークとして定義します．
        /// </para>
        /// </summary>
        /// <param name="x">定義するグリッドマークのX座標(ビュワー上の座標)</param>
        /// <param name="y">定義するグリッドマークのY座標(ビュワー上の座標)</param>
        public void ShowGridMarkDifinitionUi(double x, double y) {
            Line[] lines = DrawCrossMark(x, y);
            // Grid markを定義する位置を確認するダイアログボックスを表示する
            GridMarkDefinitionUtil util = new GridMarkDefinitionUtil(coordManager);
            SelectGridMarkWindow sgWindow = new SelectGridMarkWindow();
            sgWindow.GridMarkPoint = util.NextPoint;
            // ユーザーがGrid markを定義する位置を選択したならば，その箇所でGridf markを定義する．
            if ((Boolean)sgWindow.ShowDialog()) {
                Camera camera = Camera.GetInstance();
                Vector2 emulsionSystemPoint = coordManager.TransToEmulsionCoord((int)x, (int)y);
                coordManager.SetGridMark(emulsionSystemPoint, sgWindow.GridMarkPoint, camera.ArrayImage);
                WriteLine(string.Format(Properties.Strings.DefineGridMark, 
                          sgWindow.GridMarkPoint, emulsionSystemPoint.X, emulsionSystemPoint.Y));
            }
            // バツ印を消去
            emulsionViewerCanvas.Children.Remove(lines[0]);
            emulsionViewerCanvas.Children.Remove(lines[1]);
        }

        /// <summary>
        /// エマルションビューキャンバスにバツ印を描画します．
        /// </summary>
        /// <param name="x">描画する座標X</param>
        /// <param name="y">描画する座標Y</param>
        /// <param name="radius">バツ印の半径</param>
        /// <param name="thickness">バツ印の太さ</param>
        /// <returns>描画したLineオブジェクトの配列</returns>
        public Line[] DrawCrossMark(double x, double y, double radius = 10, double thickness = 1) {
            Line line0 = new Line();
            Line line1 = new Line();
            line0.Stroke = Brushes.Red;
            line1.Stroke = Brushes.Red;
            line0.StrokeThickness = thickness;
            line1.StrokeThickness = thickness;
            line0.X1 = x - radius;
            line0.X2 = x + radius;
            line0.Y1 = y - radius;
            line0.Y2 = y + radius;
            line1.X1 = x - radius;
            line1.X2 = x + radius;
            line1.Y1 = y + radius;
            line1.Y2 = y - radius;
            emulsionViewerCanvas.Children.Add(line0);
            emulsionViewerCanvas.Children.Add(line1);
            return new Line[] { line0, line1 };
        }

        private void stage_Loaded(object sender, RoutedEventArgs e) {
            // 各イベントハンドラを実行して初期化する．
            emulsionNumber_Changed(this, new IntEventArgs(0));
            emulsionType_Changed(this, new EmulsionEventArgs(parameterManager.EmulsionType));
            lensType_Changed(this, new LensEventArgs(parameterManager.Magnification));

            // 選択されているリボンタブに従ってイベントハンドラを実行
            switch (window.SelectedRibbonTabName) {
                case "HomeTab":
                    windowHomeTab_Selected(this, new RibbonTabEventArgs());
                    break;
                case "coordTab":
                    windowCoordTab_Selected(this, new RibbonTabEventArgs());
                    break;
                case "indexMeasTab":
                    windowCoordTab_Selected(this, new RibbonTabEventArgs());
                    break;
                case "imageEnhanceTab":
                    windowCoordTab_Selected(this, new RibbonTabEventArgs());
                    break;
                default:
                    throw new ArgumentException();
            }

            Surface surface = Surface.GetInstance(parameterManager);
            surface.OnMotorBottomLimited += surfaceOnMotorBottom_Limited;
            surface.LowBottomRecognized += surfaceLowBottom_Recognized;
            surface.LowTopRecognized += surfaceLowTop_Recognized;
            surface.UpBottomRecognized += surfaceUpBottom_Recognized;
            surface.UpTopRecognized += surfaceUpTop_Recognized;
            surface.Exited += surface_Exited;

            GridMarkSearch gs = GridMarkSearch.GetInstance(coordManager, parameterManager);
            gs.Started += gridMark_Started;
            gs.Found += gridMark_Found;
            gs.Exited += gridMark_Exited;

            emulsionViewerCanvas.Start();

            Focusable = true;
            Keyboard.Focus(this);
        }

        private void emulsionType_Changed(object sender, EmulsionEventArgs e) {
            emulsionTypeLabel.Content = (
                parameterManager.EmulsionType == EmulsionType.ThickType
                ? Properties.Strings.ThickType
                : Properties.Strings.ThinType);
        }

        private void emulsionNumber_Changed(object sender, IntEventArgs e) {
            string str = string.Format(
                "{0}: {1:00}-#{2:00}",
                Properties.Strings.Emulsion,
                parameterManager.ModuleNo,
                parameterManager.PlateNo);
            emulsionNoLabel.Content = str;
        }

        /// <summary>
        /// レンズタイプが変更されたときのイベントハンドラです．
        /// </summary>
        private void lensType_Changed(object sender, LensEventArgs e) {
            string str = Properties.Strings.LensType + ": " + e.Magnification.ToString();
            lensTypeLabel.Content = str;
        }

        /// <summary>
        /// ステージ上に表示するステータスメッセージを追加します．
        /// </summary>
        /// <param name="message">メッセージテキスト</param>
        public override void WriteLine(string message) {
            messages.Add(message);
        }

        private void parametersTimer_Tick(object sender, EventArgs e) {
            // モータの座標値を更新する
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Vector3 presentPosition = mc.GetPoint();
            drawPositionOnViewer(presentPosition);

            // モータ稼働中は速度の変更を禁止にする
            speedComboBox.IsEnabled = (!mc.IsMoving);
        }

        /// <summary>
        /// エマルションビュワーに表示する座標値を設定します．
        /// </summary>
        /// <param name="value">表示する座標値</param>
        private void drawPositionOnViewer(Vector3 value) {
            motorPositionLabel.Content = string.Format(
                "X:{0,9:+0,000;-0,000;0,000}um , Y:{1,9:+0,000;-0,000;0,000}um , Z:{2,10:+0,000.0;-0,000.0;0,000.0}um",
                value.X*1000, value.Y*1000, value.Z*1000);
        }

        private void ledSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            ledTextBox.Text = ((int)ledSlider.Value).ToString();
#if !NoHardware
            Led led = Led.GetInstance();
            led.DAout((int)ledSlider.Value, parameterManager);
#endif
        }

        private void ledTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                int ledValue = int.Parse(ledTextBox.Text);
                if (ledValue < 0 || ledValue > 255) {
                    throw new ArgumentOutOfRangeException();
                }
                ledSlider.Value = ledValue;

#if !NoHardware
                Led led = Led.GetInstance();
                led.DAout(ledValue, parameterManager);
#endif
            } catch (Exception) {
                ledTextBox.Background = Brushes.Pink;
            }
        }

        private void adjustLedButton_Click(object sender, RoutedEventArgs e) {
            try {
                int resultVal = 0;
#if !NoHardware
                Led led = Led.GetInstance();
                resultVal = led.AdjustLight(parameterManager);
#endif
                ledTextBox.Text = resultVal.ToString();
                ledSlider.Value = resultVal;
            } catch (Exception ex) {
                messages.Add(ex.Message);
            }
        }     

        private void stage_Unloaded(object sender, RoutedEventArgs e) {
#if !NoHardware
            Activity activity = new Activity(parameterManager);
            if(activity.IsActive) {
                activity.Abort();
                WriteLine(Properties.Strings.ActivityAbort);
            }
#endif
            Surface surface = Surface.GetInstance(parameterManager);
            surface.OnMotorBottomLimited -= surfaceOnMotorBottom_Limited;
            surface.LowBottomRecognized -= surfaceLowBottom_Recognized;
            surface.LowTopRecognized -= surfaceLowTop_Recognized;
            surface.UpBottomRecognized -= surfaceUpBottom_Recognized;
            surface.UpTopRecognized -= surfaceUpTop_Recognized;
            surface.Exited -= surface_Exited;

            GridMarkSearch gs = GridMarkSearch.GetInstance(coordManager, parameterManager);
            gs.Started -= gridMark_Started;
            gs.Found -= gridMark_Found;
            gs.Exited -= gridMark_Exited;
        }

        private void contextSpeed1_Selected(object sender, RoutedEventArgs e) {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            mc.SetMotorSpeed(MotorSpeed.Speed1);
        }

        private void contextSpeed2_Selected(object sender, RoutedEventArgs e) {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            mc.SetMotorSpeed(MotorSpeed.Speed2);
        }

        private void contextSpeed3_Selected(object sender, RoutedEventArgs e) {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            mc.SetMotorSpeed(MotorSpeed.Speed3);
        }

        private void contextSpeed4_Selected(object sender, RoutedEventArgs e) {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            mc.SetMotorSpeed(MotorSpeed.Speed4);
        }

        private void KeyHandleStageControl_KeyDown_1(object sender, KeyEventArgs e) {            
            if (e.Key == Key.D1) {
                contextSpeed1.IsSelected = true;
            } else if (e.Key == Key.D2) {
                contextSpeed2.IsSelected = true;
            } else if (e.Key == Key.D3) {
                contextSpeed3.IsSelected = true;
            } else if (e.Key == Key.D4) {
                contextSpeed4.IsSelected = true;
            }
        }
    }

}