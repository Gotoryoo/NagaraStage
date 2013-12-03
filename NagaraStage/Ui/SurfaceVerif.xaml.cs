/**
 * @author Hirokazu Yokoyama <o1007410@edu.gifu-u.ac.jp>
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using NagaraStage.Activities;
using NagaraStage.Parameter;

namespace NagaraStage.Ui {
    /// <summary>
    /// 表面認識を検証するためのユーザーインターフェイスを提供するクラスです．
    /// <para>SurfaceVerif.xaml の相互作用ロジック</para>
    /// </summary>
    public partial class SurfaceVerif : Workspace {
        private IMainWindow window;
        private ParameterManager parameterManager;
        private Line baseLine = new Line();
        private List<Line> brightnessLines = new List<Line>();
        private List<SurfaceEventArgs> eventList = new List<SurfaceEventArgs>();
        private DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal);
        private volatile int max = 0;

        private static double lineWidth = 6;
        private static double interval = 0;

        public SurfaceVerif(IMainWindow _window)
            : base(_window) {
            this.window = _window;
            this.parameterManager = window.ParameterManager;
            InitializeComponent();
            timer.Interval = new TimeSpan(ParameterManager.ParamtersIntervalMilliSec * 1000);
            timer.Tick += timer_Tick;
            Loaded += SurfaceVerif_Loaded;
            Unloaded += SurfaceVerif_Unloaded;
        }

        void timer_Tick(object sender, EventArgs e) {
            IO.MotorControler mc = IO.MotorControler.GetInstance(parameterManager);
            Vector3 p = mc.GetPoint();
            coordLabel.Content = string.Format(
                "X:{0:000.0000}, Y:{1:000.00000}, Z:{2:000.0000}", p.X, p.Y, p.Z);
        }

        /// <summary>
        /// (検証結果を消去して)キャンバスなどを初期化します．
        /// </summary>
        public void Initialize() {
            resultCanvas.Children.Clear();
            baseLine.Stroke = Brushes.Black;
            baseLine.StrokeThickness = 2;
            baseLine.X1 = 0;
            baseLine.Y1 = resultCanvas.ActualHeight - 15;
            baseLine.X2 = resultCanvas.ActualWidth;
            baseLine.Y2 = resultCanvas.ActualHeight - 15;
            resultCanvas.Children.Add(baseLine);
            brightnessLines = new List<Line>();
            eventList = new List<SurfaceEventArgs>();
        }

        /// <summary>
        /// キャンバスに描画します．
        /// </summary>
        public void Draw() {
            if (max <= 0) {
                return;
            }

            for (int i = 0; i < eventList.Count; ++i) {
                Line line = createValueBar(eventList[i]);
                Line backgroundLine = createBackgroundBar(eventList[i]);
                resultCanvas.Children.Add(backgroundLine);
                resultCanvas.Children.Add(line);
            }
            resultCanvas.Width = eventList.Count * (lineWidth + interval);
        }

        /// <summary>
        /// 棒グラフの輝度値を示す棒を作成し，返します．
        /// </summary>
        /// <returns>輝度値を示す棒</returns>
        /// <exception cref="System.DivideByZeroException">eventListがゼロの時</exception>
        private Line createValueBar(SurfaceEventArgs e) {
            Line retLine = new Line();
            retLine.Stroke = (e.IsBoundary ? Brushes.Blue : Brushes.Red);
            retLine.StrokeThickness = lineWidth;
            retLine.X1 = e.Id * (retLine.StrokeThickness + interval);
            retLine.X2 = retLine.X1;
            retLine.Y1 = baseLine.Y1;
            double height = resultCanvas.ActualHeight * ((double)e.Brightness / ((double)max));
            retLine.Y2 = resultCanvas.ActualHeight - height - 15;
            retLine.ToolTip = createToolTipString(e);
            retLine.Cursor = Cursors.Hand;
            retLine.ContextMenu = createBarContextMenu(e);
            retLine.MouseDown += delegate(object sender, MouseButtonEventArgs e2) {
                createMouseDownEvent(e.ZValue);
            };
            return retLine;
        }

        private Line createBackgroundBar(SurfaceEventArgs e) {
            Line line = new Line();
            line.Stroke = Brushes.White;
            line.StrokeThickness = lineWidth;
            line.X1 = e.Id * (line.StrokeThickness + interval);
            line.X2 = line.X1;
            line.Y1 = 0;
            line.Y2 = resultCanvas.ActualHeight - 15;
            line.ToolTip = createToolTipString(e);
            line.Cursor = Cursors.Hand;
            line.ContextMenu = createBarContextMenu(e);
            line.MouseDown += delegate(object sender, MouseButtonEventArgs e2) {
                createMouseDownEvent(e.ZValue);
            };
            line.MouseEnter += delegate(object sender, MouseEventArgs e3) {
                line.Stroke = Brushes.Pink;
            };
            line.MouseLeave += delegate(object sender, MouseEventArgs e4) {
                line.Stroke = Brushes.White;
            };
            return line;
        }

        private void createMouseDownEvent(double zValue) {
            IO.MotorControler mc = IO.MotorControler.GetInstance();
            mc.MovePointZ(zValue);
        }

        private ContextMenu createBarContextMenu(SurfaceEventArgs e) {
            ContextMenu menu = new ContextMenu();
            MenuItem[] boundaryItems = new MenuItem[4];
            boundaryItems[0] = new MenuItem();
            boundaryItems[0].Header = string.Format(Properties.Strings.SetXFlag, Properties.Strings.UpperGelTop);
            boundaryItems[0].Click += delegate(object sender, RoutedEventArgs e2) {
                e.Note += Logger.SurfaceLogger.GetBoundaryNoteString(Properties.Strings.UpperGelTop);
                boundaryItems[0].IsEnabled = false;
            };
            menu.Items.Add(boundaryItems[0]);

            boundaryItems[1] = new MenuItem();
            boundaryItems[1].Header = string.Format(Properties.Strings.SetXFlag, Properties.Strings.UpperGelBottom);
            boundaryItems[1].Click += delegate(object sender, RoutedEventArgs e2) {
                e.Note += Logger.SurfaceLogger.GetBoundaryNoteString(Properties.Strings.UpperGelBottom);
                boundaryItems[1].IsEnabled = false;
            };
            menu.Items.Add(boundaryItems[1]);

            boundaryItems[2] = new MenuItem();
            boundaryItems[2].Header = string.Format(Properties.Strings.SetXFlag, Properties.Strings.LowerGelTop);
            boundaryItems[2].Click += delegate(object sender, RoutedEventArgs e2) {
                e.Note += Logger.SurfaceLogger.GetBoundaryNoteString(Properties.Strings.LowerGelTop);
                boundaryItems[2].IsEnabled = false;
            };
            menu.Items.Add(boundaryItems[2]);

            boundaryItems[3] = new MenuItem();
            boundaryItems[3].Header = string.Format(Properties.Strings.SetXFlag, Properties.Strings.LowerGelBottom);
            boundaryItems[3].Click += delegate(object sender, RoutedEventArgs e2) {
                e.Note += Logger.SurfaceLogger.GetBoundaryNoteString(Properties.Strings.LowerGelBottom);
                boundaryItems[3].IsEnabled = false;
            };
            menu.Items.Add(boundaryItems[3]);

            return menu;
        }        

        private void SurfaceVerif_Loaded(object sender, RoutedEventArgs e) {
            Surface surface = Surface.GetInstance(parameterManager);
            surface.Started += new EventHandler<EventArgs>(surface_Started);
            surface.Shooting += surface_Shoot;
            surface.Exited += new ActivityEventHandler(surface_Exited);
            statusTextBox.Text = string.Format("#{0}-{1}, {2}: {3}, {4}: {5}, {6}: {7}",
                parameterManager.ModuleNo, parameterManager.PlateNo,
                Properties.Strings.BinarizeThreshold, surface.BinarizeThreshold,
                Properties.Strings.BrightnessThreshold, surface.BrightnessThreshold,
                Properties.Strings.Speed, surface.MotorSpeed);
            Initialize();
            timer.Start();
            viewerCanvas.Start();
        }

        void surface_Started(object sender, EventArgs e) {
            max = 0;
        }

        void SurfaceVerif_Unloaded(object sender, RoutedEventArgs e) {
            Surface surface = Surface.GetInstance();
            surface.Started -= surface_Started;
            surface.Shooting -= surface_Shoot;
            surface.Exited -= surface_Exited;
            viewerCanvas.Stop();
        }

        void surface_Exited(object sender, ActivityEventArgs e) {
            Dispatcher.BeginInvoke(new Action(delegate() {
                Draw();
            }), null);
            if (e.Exception != null) {
                MessageBox.Show(e.Exception.Message);
            }
        }

        private string createToolTipString(SurfaceEventArgs e) {
            // 番号
            // 距離: e.Distance
            // 輝度値: e.Brightness
            // ゲル内: e.IsInGel
            // e.Note
            return string.Format("{0}\n{1}: {2}\n{3}: {4}\n{5}: {6}\n{7}",
                e.Id,
                Properties.Strings.Distance, e.Distance,
                Properties.Strings.Brightness, e.Brightness,
                Properties.Strings.InGel, e.IsInGel,
                e.Note
                );
        }

        /// <summary>
        /// 表面認識でカメラから入力画像を処理したときのイベントハンドラです．
        /// </summary>
        private void surface_Shoot(object sender, SurfaceEventArgs e) {
            Dispatcher.BeginInvoke(new Action(delegate() {
                eventList.Add(e);
                max = Math.Max(max, e.Brightness);
            }), null);
        }

        private void closeButton_Click(object sender, RoutedEventArgs e) {
            Surface surface = Surface.GetInstance();
            surface.Abort();
            Finish();
        }

        private void startButton_Click(object sender, RoutedEventArgs e) {
            Initialize();
            Surface surface = Surface.GetInstance();
            try {
                surface.Start();
            } catch (Exception ex) {
                if (ex.Message != null) {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void fastButton_Click(object sender, RoutedEventArgs e) {
            Initialize();
            Surface surface = Surface.GetInstance();
            try {
                surface.Start(true);
            } catch (Exception ex) {
                if (ex.Message != null) {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void abortButton_Click(object sender, RoutedEventArgs e) {
            Surface surface = Surface.GetInstance();
            surface.Abort();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog dialog = new SaveFileDialog(SaveFileDialogMode.XmlFile);
            if (dialog.ShowDialog()) {
                try {
                    outputToLogFile(dialog.FileName);
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void outputToLogFile(string path) {
            Surface surface = Surface.GetInstance(parameterManager);
            Logger.SurfaceArgs args = new Logger.SurfaceArgs();
            args.ModuleNo = parameterManager.ModuleNo;
            args.PlateNo = parameterManager.PlateNo;
            args.MotorSpeed = surface.MotorSpeed;
            args.BinarizeThreshold = surface.BinarizeThreshold;
            args.BrightnessThreshold = surface.BrightnessThreshold;
            args.Time = surface.Time;
#if _NoHardWare
            args.SetEventList(Logger.SurfaceLogger.CreateSurfEventArgs());
#else 
            args.SetEventList(eventList);
#endif
            Logger.SurfaceLogger logger = new Logger.SurfaceLogger();
            logger.FileName = path;
            logger.Save(args);
        }

        private void configButton_Click(object sender, RoutedEventArgs e) {
            SurfaceConfig config = new SurfaceConfig(window);
            config.NextControl = this;
            window.SetElementOnWorkspace(config);
        }
    }
}
