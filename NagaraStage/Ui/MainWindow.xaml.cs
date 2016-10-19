using System;
using System.IO;
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
using System.Globalization;
using System.Diagnostics;
using Microsoft.Windows.Controls.Ribbon;
using System.Windows.Media.Animation;
using System.Threading;


using NagaraStage;
using NagaraStage.IO;
using NagaraStage.Ui;
using NagaraStage.IO.Driver;
using NagaraStage.Activities;
using NagaraStage.Parameter;
//using NagaraStage.ImageEnhancement;

using OpenCvSharp;
using OpenCvSharp.CPlusPlus;



namespace NagaraStage.Ui {
    /// <summary>
    /// アプリケーションのメインウィンドウとなるユーザーインタフェイスを提供するクラスです．
    /// <para>Interaction logic for MainWindow.xaml</para>
    /// </summary>
    public partial class MainWindow : RibbonWindow, IMainWindow {
        private Control presentStageControl;
        private Stage stage;
        private ParameterManager parameterManager;
        private CoordManager coordManager;
        private bool isTabsEnabled;
        private string selectedTabName;

        /// <summary>
        /// <see cref="NagaraStage.Ui.IMainWindow.RibbonTabSelected"/>
        /// </summary>
        public event EventHandler<RibbonTabEventArgs> RibbonTabSelected;

        /// <summary>
        /// StageFieldのControlがセットされたときのイベント
        /// </summary>
        public event EventHandler<StageFieldEventArgs> StageFieldSet;
        /// <summary>
        /// StageFieldのControlが削除されたときのイベント
        /// </summary>
        public event EventHandler<EventArgs> StageFieldRemoved;

        /*---------------------------------------------------------------------
        * プロパティ
         * -------------------------------------------------------------------*/

        /// <summary>
        /// ソフトウェア全体を通して用いるParameterManagerを取得します．
        /// </summary>
        public ParameterManager ParameterManager {
            get { return parameterManager; }
        }

        /// <summary>
        /// ソフトウェア全体を通して用いるCoordManagerを取得します．
        /// </summary>
        public CoordManager CoordManager {
            get { return coordManager; }
        }

        /// <summary>
        /// 選択されているリボンタブを取得，または設定します．
        /// </summary>
        public string SelectedRibbonTabName {
            get { return selectedTabName; }
            set {
                switch (value) {
                    case "HomeTab":
                        HomeTab.IsSelected = true;
                        break;
                    case "coordTab":
                        coordTab.IsSelected = true;
                        break;
                    case "overallScanTab":
                        overallScanTab.IsSelected = true;
                        break;
                    case "imageEnhanceTab":
                        imageEnhanceTab.IsSelected = true;
                        break;
                    //case "retrackfollowTab":
                    //    retrackfollowTab.IsSelected = true;
                    //    break;
                    default:
                        string message = Properties.Strings.Error + ": Parameter is unavailable.";
                        throw new ArgumentException(message);
                }
            }
        }

        /// <summary>
        /// ワークスペースに表示されているコンポーネントを取得します．
        /// <para>object型で返すため，該当クラスにキャストする必要があります．</para>
        /// </summary>
        /// <example>
        /// // 該当クラスにキャストして取得(as によるキャストを推奨)
        /// Stage stage = mainWindow.WorkSpaceContent as Stage;
        /// if(stage != null) {
        ///    処理...
        /// }
        /// </example>
        public object WorkspaceContent {
            get { return workspace.Content; }
        }

        /// <summary>
        /// リボンのタブメニューが有効であるかどうかを設定，または取得します．
        /// </summary>
        public bool IsTabsEnabled {
            get { return isTabsEnabled; }
            set {
                isTabsEnabled = value;
                HomeTab.IsEnabled = value;
                coordTab.IsEnabled = value;
                overallScanTab.IsEnabled = value;
                autoTrackingTab.IsEnabled = value;
                imageEnhanceTab.IsEnabled = value;
                //retrackfollowTab.IsSelected = value;
            }
        }

        /* --------------------------------------------------------------------
        * コンストラクタ，初期化関連
         * ----------------------------------------------------------------- */

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_parameterManager">アプリケーション全体を通して用いるParameterManagerのインスタンス</param>
        public MainWindow(ParameterManager _parameterManager) {
            InitializeComponent();
            this.parameterManager = _parameterManager;
            coordManager = new CoordManager(parameterManager);

            // 各イベントハンドラを設定
            StageFieldSet += delegate(object o, StageFieldEventArgs e) {
                IsTabsEnabled = (WorkspaceContent as Stage != null);
            };
            StageFieldRemoved += delegate {
                IsTabsEnabled = false;
            };
            Surface surface = Surface.GetInstance(parameterManager);
            surface.Started += surface_Started;
            surface.Exited += surface_Exited;
            IsTabsEnabled = false;
            surfaceRecogAbortButton.IsEnabled = false;
        }

        /// <summary>
        /// Loadイベントのイベントハンドラ
        /// </summary>
        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e) {
            /* アプリケーションメニューの言語選択メニューを構築する．*/
            // いわゆる言語パックが存在してる言語を調べ，アプリケーションメニューの
            // languageItemSelect以下に追加する．
            for (int i = 0; i < CultureResources.Cultures.Count; ++i) {
                CultureInfo culture = CultureResources.Cultures[i] as CultureInfo;
                RibbonApplicationMenuItem item = new RibbonApplicationMenuItem();
                item.Header = CultureResources.Cultures[i].NativeName;
                item.Click += delegate(object s, RoutedEventArgs e2) {
                    MessageBoxResult r = MessageBox.Show(
                        string.Format(Properties.Strings.LangSelect01, culture.NativeName),
                        Properties.Strings.Attention,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    if (r == MessageBoxResult.Yes) {
                        Properties.Settings.Default.Language = culture.ToString();
                        MessageBox.Show(Properties.Strings.LangSelect02);
                    }
                };
                languageItemSelect.Items.Add(item);
            }

            /* アプリケーションメニューのレンズタイプ設定メニューを構築する */
            double[] magList = parameterManager.GetLensMagList();
            for (int i = 0; i < parameterManager.NumOfLens; ++i) {
                double thisMag = magList[i];
                RibbonApplicationMenuItem item = new RibbonApplicationMenuItem();
                item.Header = string.Format(Properties.Strings.SetLensXMag, magList[i]);
                item.ImageSource = new BitmapImage(new Uri("../Images/lens.png", UriKind.Relative));
                item.Click += delegate(object s, RoutedEventArgs e2) {
                    string message = string.Format(Properties.Strings.SetLensDialog, thisMag);
                    MessageBoxResult r = MessageBox.Show(
                        message,
                        Properties.Strings.LensTypeSetting,
                        MessageBoxButton.OKCancel);
                    if (r == MessageBoxResult.OK) {
                        try {
                            parameterManager.Magnification = thisMag;
                        } catch (Exception ex) {
                            MessageBox.Show(ex.Message);
                        }
                    }
                };
                lensSettingItem.Items.Add(item);
            }
        }

        private void noizeCheckButton_cliked(object sender, RoutedEventArgs e) {
            MessageBox.Show("abc");
        }

        /*---------------------------------------------------------------------
         * Publicメソッド: ワークスペースのコントロールの追加と削除
         * ------------------------------------------------------------------*/

        /// <summary>
        /// WPF要素をメインウィンドウのワークスペースに表示します．
        /// <para>これまでセットされていたWPF要素は削除されます．</para>
        /// </summary>
        public void SetElementOnWorkspace(Workspace control) {
            presentStageControl = control;
            workspace.Content = control;

            if (control == null) {
                return;
            }
            var animation = new DoubleAnimation {
                From = 0,
                To = workspace.ActualWidth,
                Duration = new Duration(TimeSpan.FromMilliseconds(300))
            };
            control.IsEnabled = false;
            control.BeginAnimation(Control.WidthProperty, animation);
            control.Focus();
            control.IsEnabled = true;
            if (StageFieldSet != null) {
                StageFieldSet(this, new StageFieldEventArgs(control));
            }
        }

        /// <summary>
        /// ステージを作成して，ワークスペースにセットします．
        /// <para>各コントロールの再設定も行います．</para>
        /// </summary>
        public void CreateStage() {
            stage = new Stage(this);
            SetElementOnWorkspace(stage);
        }

        /// <summary>
        /// リボンタブのいずれかが選択されたときのイベントを発生させます．
        /// <para>各タブのイベントを発生させます．</para>
        /// </summary>
        private void Ribbon_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            RibbonTab tab = e.AddedItems[0] as RibbonTab;
            if (tab == null) {
                return;
            }

            RibbonTabEventArgs eventArgs = new RibbonTabEventArgs();
            eventArgs.SelectedTabName = tab.Name;
            selectedTabName = tab.Name;
            if (RibbonTabSelected != null) {
                RibbonTabSelected(this, eventArgs);
            }
        }

        



        /*---------------------------------------------------------------------
         * Home tab 内のコントロールのイベントハンドラ
         * ------------------------------------------------------------------*/

        private void stopButton_Click(object sender, RoutedEventArgs e) {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
#if !NoHardware
            mc.AbortMoving();
#endif
            stage.WriteLine(Properties.Strings.MotorStop01);
        }

        private void SpiralStartButton_Click(object sender, RoutedEventArgs e) {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            try {
                mc.MoveInSpiral();
            } catch (MotorActiveException) {
                stage.WriteLine(Properties.Strings.MotorActiveException02);
            }
        }

        private void SpiralBackButton_Click(object sender, RoutedEventArgs e) {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            try {
                mc.BackToSpiralCenter();
            } catch (MotorActiveException) {
                stage.WriteLine(Properties.Strings.MotorActiveException02);
            }
        }

        private void SpiralOriginButton_Click(object sender, RoutedEventArgs e) {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            mc.SetSpiralCenterPoint();
        }

        private void setOriginButton_Click(object sender, RoutedEventArgs e) {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
#if !NoHardware
            mc.InitializeMotorControlCounter(VectorId.X);
            mc.InitializeMotorControlCounter(VectorId.Y);
            mc.InitializeMotorControlCounter(VectorId.Z);
#endif
            stage.WriteLine(Properties.Strings.OriginChanged);
        }

        private void backOriginButton_Click(object sender, RoutedEventArgs e) {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            if (mc.IsMoving) {
                MessageBoxResult r = MessageBox.Show(
                    Properties.Strings.MotorActive01,
                    Properties.Strings.Abort + "?",
                    MessageBoxButton.OKCancel);
                if (r == MessageBoxResult.OK) {
                    mc.AbortMoving();
                } else {
                    return;
                }
            }

            stage.WriteLine(Properties.Strings.Moving);
            mc.MovePointXY(0, 0, delegate {
                stage.WriteLine(Properties.Strings.MovingComplete);
            });
        }



        private void surfaceRecogButton_Click(object sender, RoutedEventArgs e) {
            // モータが稼働中であれば停止するかどうかを尋ねる．
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            if (mc.IsMoving) {
                MessageBoxResult r = MessageBox.Show(
                    Properties.Strings.SurfaceException01,
                    Properties.Strings.Abort + "?",
                    MessageBoxButton.YesNo);
                if (r == MessageBoxResult.Yes) {
                    mc.AbortMoving();
                } else {
                    return;
                }
            }

            // すでに表面認識が実行中であれば停止するかどうか尋ねる．
            Surface surface = Surface.GetInstance(parameterManager);
            if (surface.IsActive) {
                MessageBoxResult r = MessageBox.Show(
                    Properties.Strings.SurfaceException02,
                    Properties.Strings.Abort + "?",
                    MessageBoxButton.YesNo);
                if (r == MessageBoxResult.Yes) {
                    surface.Abort();
                } else {
                    return;
                }
            }

            try {
                surface.Start(true);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, Properties.Strings.Error);
            }
        }

        private void SurfaceRecogBottomButton_Click(object sender, RoutedEventArgs e) {
            // モータが稼働中であれば停止するかどうかを尋ねる．
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            if (mc.IsMoving) {
                MessageBoxResult r = MessageBox.Show(
                    Properties.Strings.SurfaceException01,
                    Properties.Strings.Abort + "?",
                    MessageBoxButton.YesNo);
                if (r == MessageBoxResult.Yes) {
                    mc.AbortMoving();
                } else {
                    return;
                }
            }

            // すでに表面認識が実行中であれば停止するかどうか尋ねる．
            Surface surface = Surface.GetInstance(parameterManager);
            if (surface.IsActive) {
                MessageBoxResult r = MessageBox.Show(
                    Properties.Strings.SurfaceException02,
                    Properties.Strings.Abort + "?",
                    MessageBoxButton.YesNo);
                if (r == MessageBoxResult.Yes) {
                    surface.Abort();
                } else {
                    return;
                }
            }

            try {
                surface.Start(false);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, Properties.Strings.Error);
            }
        }

        private void surface_Started(Object sender, EventArgs e) {
            Dispatcher.BeginInvoke(new Action(delegate() {
                // ユーザーインターフェイスを制限する
                IsTabsEnabled = false;
                HomeTab.IsEnabled = true;
                controlGroup.IsEnabled = false;
                shootingGrop.IsEnabled = false;
                surfaceRecogAbortButton.IsEnabled = true;
            }), null);
        }

        private void surface_Exited(object sender, ActivityEventArgs e) {
            Dispatcher.BeginInvoke(new Action(delegate() {
                // 制限していたUIを元に戻す
                IsTabsEnabled = true;
                controlGroup.IsEnabled = true;
                shootingGrop.IsEnabled = true;
                surfaceRecogAbortButton.IsEnabled = false;
            }), null);
        }

        private void surfaceRecogConfigButton_Click(object sender, RoutedEventArgs e) {
            SurfaceConfig surfaceConfig = new SurfaceConfig(this);
            surfaceConfig.NextControl = SurfaceConfig.PresentControl;
            SetElementOnWorkspace(surfaceConfig);
        }


        private void surfaceRecogAbortButton_Click(object sender, RoutedEventArgs e) {
            Surface surface = Surface.GetInstance(parameterManager);
            surface.Abort();
            stage.WriteLine(Properties.Strings.AbortedSurfaceRecognizing);
        }

        private void inGelButton_Click(object sender, RoutedEventArgs e) {
            Surface surface = Surface.GetInstance(parameterManager);
            Boolean flag = false;
            try {
                flag = surface.IsInGel();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, Properties.Strings.Error);
            }
            System.Console.WriteLine(flag.ToString());
            string message = (flag ?
                Properties.Strings.InsideGelNow
                : Properties.Strings.OutsideGelNow)
                + Properties.Strings.IndicativeBrightness
                + ":" + surface.Brightness.ToString();
            stage.WriteLine(message);
        }

        private void moveToUpperGelTopButton_Click(object sender, RoutedEventArgs e) {
            Surface surface = Surface.GetInstance(parameterManager);
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            mc.MovePointZ(surface.UpTop);
        }

        private void moveTopUpperGelBottomButton_Click(object sender, RoutedEventArgs e) {
            Surface surface = Surface.GetInstance(parameterManager);
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            mc.MovePointZ(surface.UpBottom);
        }

        private void moveToLowerGelTopButton_Click(object sender, RoutedEventArgs e) {
            Surface surface = Surface.GetInstance(parameterManager);
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            mc.MovePointZ(surface.LowTop);
        }

        private void moveToLowerGelBottomButton_Click(object sender, RoutedEventArgs e) {
            Surface surface = Surface.GetInstance(parameterManager);
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            mc.MovePointZ(surface.LowBottom);
        }

        private void SurfaceLandingButton_Click(object sender, RoutedEventArgs e) {
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            SurfaceLanding sl = SurfaceLanding.GetInstance(parameterManager);
            manager.Enqueue(sl);
            manager.Start();
        }

        private void openInFile(OpenInFileMode mode) {
            OpenFileDialog openDialog = null;
            SaveFileDialog saveDialog = null;
            Boolean opened = false, saved = false, append = false;

            switch (mode) {
                case OpenInFileMode.Prediction:
                    openDialog = new OpenFileDialog(OpenFileDialogMode.PredictionFile);
                    saved = true;
                    break;
                case OpenInFileMode.ScanData:
                    openDialog = new OpenFileDialog(OpenFileDialogMode.ScanDataFile);
                    saveDialog = new SaveFileDialog(SaveFileDialogMode.ScanDataFile);
                    break;
                case OpenInFileMode.VertexData:
                    openDialog = new OpenFileDialog(OpenFileDialogMode.VertexDataFile);
                    saveDialog = new SaveFileDialog(SaveFileDialogMode.ScanDataFile);
                    break;
            }

            opened = openDialog.ShowDialog();

            if (opened & saveDialog != null) {
                if (saveDialog.ShowDialog()) {
                    if (System.IO.File.Exists(saveDialog.FileName)) {
                        MessageBoxResult r = MessageBox.Show(
                            string.Format(Properties.Strings.FileAlreadyExist, saveDialog.FileName)
                            + Properties.Strings.AppendSave,
                            Properties.Strings.Append + "?",
                            MessageBoxButton.YesNo);
                        if (r == MessageBoxResult.Yes) {
                            append = true;
                            saved = true;
                        } else if (r == MessageBoxResult.No) {
                            append = false;
                            saved = true;
                        } else {
                            saved = false;
                        }
                    } else {
                        saved = true;
                    }
                }
            }

            if (opened & saved) {
                parameterManager.OpenInFile(mode, openDialog.FileName, saveDialog.FileName, append);
            }
        }

        /// <summary>
        /// Grid Original Data Fine(gorg)ファイルをダイアログを開いて読み込み，
        /// 値を適応させる一連の流れを行います．
        /// </summary>
        /// <exception cref="Exception">ファイルが開かれなかった場合、失敗した場合</exception>
        private void loadGridOriginalFine(int mode = 2) {
            GridParameter gridParam = parameterManager.GridParameter;
            OpenFileDialog openDialog = new OpenFileDialog(OpenFileDialogMode.GridOriginalDataFile);
            if (openDialog.ShowDialog()) {
                gridParam.ReadGridOriginalFine(mode, openDialog.FileName);
            } else {
                throw new Exception();
            }
        }

        /// <summary>
        /// Grid Fine Parameter File (gofs)ファイルをダイアログを開いて読み込み，
        /// 値を適応させる一連の流れを行います．
        /// </summary>
        /// <param name="readAnyTime">Coodinater.ReadGridFineParamterの引数</param>
        /// <returns>成功すればtrue, 失敗すればfalse</returns>
        private void loadGridFineParameter(Boolean readAnyTime) {
            GridParameter gridParameter = parameterManager.GridParameter;
            OpenFileDialog openDialog = new OpenFileDialog(OpenFileDialogMode.GridFineParameterFile);

            if (openDialog.ShowDialog()) {
                gridParameter.ReadGridFineParameter(openDialog.FileName, false);
            } else {
                throw new Exception();
            }
        }

        /// <summary>
        /// Grid Data (gorg)ファイルを開くためのダイアログを開いて読み込み,
        /// 値を適応させる一連の流れを行います．
        /// </summary>               
        private void loadGridData() {
            GridParameter gridParameter = parameterManager.GridParameter;
            OpenFileDialog openDialog = new OpenFileDialog(OpenFileDialogMode.GridOriginalDataFile);

            if (!gridParameter.DefinedBeamCo & openDialog.ShowDialog()) {
                gridParameter.ReadGrid(openDialog.FileName);
            } else {
                throw new Exception();
            }
        }

        /// <summary>
        /// Grid Track output Data FIle(*.gkt)ファイルを開くためのダイアログを開いて読み込み,
        /// 値を適応させる一連の流れを行います。
        /// </summary>
        private void loadGridTrack() {
            GridParameter gridParameter = parameterManager.GridParameter;
            OpenFileDialog openDialog = new OpenFileDialog(OpenFileDialogMode.GridTrackOutputDataFile);
            if (openDialog.ShowDialog()) {
                gridParameter.ReadGridTrack(openDialog.FileName, parameterManager.PlateNo);
            } else {
                throw new Exception();
            }
        }

        /// <summary>
        /// Grid Data File(*.grid)ファイルを開くためのダイアログを開いて読み込み,
        /// 値を適応させる一連の流れを行います.
        /// </summary>
        private void loadGridFile() {
            GridParameter gridParameter = parameterManager.GridParameter;
            OpenFileDialog openDialog = new OpenFileDialog(OpenFileDialogMode.GridDataFile);
            if (openDialog.ShowDialog()) {
                gridParameter.ReadGrid(openDialog.FileName);
            } else {
                throw new Exception();
            }
        }

        /*---------------------------------------------------------------------
         * Coordinate tab のコントロールのイベントハンドラ
         * -------------------------------------------------------------------*/

        private void clearGridMarksButton_Click(object sender, RoutedEventArgs e) {
            // ユーザーに本当にグリッドマークを削除して良いか確認する．．
            MessageBoxResult result = MessageBox.Show(
                Properties.Strings.InitGridMarkReally,
                Properties.Strings.Attention,
                MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes) {
                coordManager.CleaerGridMark();
            }
        }

        private void createCoordButton_Click(object sender, RoutedEventArgs e) {
            // 定義済みのグリッドマーク数が少なすぎる場合はメッセージを表示して，終了
            GridMarkDefinitionUtil util = new GridMarkDefinitionUtil(coordManager);
            if (!util.IsMinimumDefined) {
                MessageBox.Show(Properties.Strings.GridMarkNotDefinedMin, Properties.Strings.Error);
                return;
            }

            // 全てのグリッドマークが定義されていない場合は，続行するか確認する
            if (coordManager.DefinedGridMarkNum == 9) {
                MessageBoxResult result = MessageBox.Show(
                    Properties.Strings.CreateCoordSystem,
                    Properties.Strings.Attention,
                    MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes) {
                    return;
                }
            } else {
                MessageBoxResult result = MessageBox.Show(
                    Properties.Strings.WarnCreateCoord01,
                    Properties.Strings.Attention,
                    MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes) {
                    return;
                }

            }

            try {
                coordManager.CreateCoordSystem();
                stage.WriteLine(String.Format("mag={0}, theta={1}", coordManager.MagnitOfGrid, coordManager.AngleOfGrid));
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, Properties.Strings.Error);
            }
        }


        private void readMagThetaButton_Click(object sender, RoutedEventArgs e) {
            try {
                coordManager.readMagTheta();
                stage.WriteLine(String.Format("mag={0}, theta={1}", coordManager.MagnitOfGrid, coordManager.AngleOfGrid));
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, Properties.Strings.Error);
            }
        }

        /*---------------------------------------------------------------------
         * アプリケーションメニューアイテムのイベントハンドラ
         * ------------------------------------------------------------------*/

        private void newItem_Click(object sender, RoutedEventArgs e) {
            // ステージがすでに実行中であれば，実行中のステージを終了するか尋ねる．
            Stage runningStage = WorkspaceContent as Stage;
            if (runningStage != null) {
                MessageBoxResult r = MessageBox.Show(Properties.Strings.StageRunAbort, "", MessageBoxButton.YesNo);
                if (r == MessageBoxResult.Yes) {
                    runningStage.Finish();
                } else {
                    return;
                }
            }

            NewStage newStage = new NewStage(this);
            newStage.Unloaded += delegate(object o, RoutedEventArgs e2) {
                if (newStage.Result == MessageBoxResult.OK || newStage.Result == MessageBoxResult.Yes) {
                    CreateStage();
                }
            };
            SetElementOnWorkspace(newStage);
        }

        private void systemConfItem_Click(object sender, RoutedEventArgs e) {
            SystemConfControl control = new SystemConfControl(this);
            control.NextControl = Workspace.PresentControl;
            SetElementOnWorkspace(control);
        }

        private void versionInfoItem_Click(object sender, RoutedEventArgs e) {
            VersionInfo versionInfo = new VersionInfo(this);
            versionInfo.NextControl = Workspace.PresentControl;
            SetElementOnWorkspace(versionInfo);
        }


        private void scanFileItem_Click(object sender, RoutedEventArgs e) {
            try {
                loadGridOriginalFine();
                loadGridFineParameter(false);
                if (!parameterManager.GridParameter.DefinedBeamCo) {
                    loadGridFile();
                }
            } catch (System.IO.FileLoadException ex) {
                MessageBox.Show(ex.Message);
            } catch (Exception) {
            }

            try {
                openInFile(OpenInFileMode.ScanData);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void exitItem_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void RibbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
#if !NoHardware
            Led led = Led.GetInstance();
            led.DAout(0, parameterManager);
            //MotorControler mc = MotorControler.GetInstance(parameterManager);
            //mc.MovePoint(0, 0, 0);
#endif
            Properties.Settings.Default.Save();
        }

        private void takePluralityButton_Click(object sender, RoutedEventArgs e) {
            ShootingStage shootingStage = new ShootingStage(this, ShootingMode.Plurality);
            shootingStage.NextControl = Workspace.PresentControl;
            SetElementOnWorkspace(shootingStage);
        }

        private void AutomaticGridDetecetorButton_Click(object sender, RoutedEventArgs e) {
            try {
                IGridMarkRecognizer GridMarkRecognizer = coordManager;
                Vector2 viewerPoint = GridMarkRecognizer.SearchGridMark();
                if (viewerPoint.X < 0 || viewerPoint.Y < 0) {
                    System.Diagnostics.Debug.WriteLine(String.Format("grid mark not found"));
                } else {
                    System.Diagnostics.Debug.WriteLine(String.Format("{0}  {1}", viewerPoint.X, viewerPoint.Y));
                }
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(String.Format("ex"));
            }
        }



        private void startDetectGridMarkButton_Click(object sender, RoutedEventArgs e) {
            bool continuFlag = true;
            GridMarkSearch gridmarkSearch = GridMarkSearch.GetInstance(coordManager, parameterManager);
            if (gridmarkSearch.IsActive) {
                continuFlag = (MessageBox.Show(
                    Properties.Strings.ActivityActiveException01,
                    Properties.Strings.Abort + "?",
                    MessageBoxButton.YesNo
                    ) == MessageBoxResult.Yes);
            }

            //if (!gridmarkSearch.IsGridMarkedToStart) {
            //    MessageBox.Show(
            //        Properties.Strings.GridMarkNotDefinedMin,
            //        Properties.Strings.Error);
            //    continuFlag = false;
            //}

            if (parameterManager.Magnification
                != ParameterManager.LensMagnificationOfGridMarkSearch) {
                string message = Properties.Strings.LensTypeException01
                    + string.Format(Properties.Strings.CorrectLens, ParameterManager.LensMagnificationOfGridMarkSearch);
                MessageBox.Show(message, Properties.Strings.Error);
                continuFlag = false;
            }

            if (continuFlag) {
                gridmarkSearch.Abort();
                gridmarkSearch.Start();
            }
        }

        private void stopDetectGridMarkButton_Click(object sender, RoutedEventArgs e) {
            GridMarkSearch gms = GridMarkSearch.GetInstance(coordManager, parameterManager);
            gms.Abort();
        }

        private void nextGridMarkButton_Click(object sender, RoutedEventArgs e) {
            GridMarkDefinitionUtil util = new GridMarkDefinitionUtil(coordManager);
            Vector2 p = util.GetGridMarkCoord(util.NextPoint);
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            mc.MovePointXY(p, delegate() {
                mc.SetSpiralCenterPoint();
                Led led = Led.GetInstance();
                led.AdjustLight(parameterManager);
            });

        }

        private void otherGridMarkButton_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            // グリッドマークの画像を取得
            Image[] images = new Image[CoordManager.AllGridMarksNum];
            for (int i = 0; i < CoordManager.AllGridMarksNum; ++i) {
                GridMark mark = coordManager.GetGridMark((GridMarkPoint)i);
                if (mark.Existed && mark.Image != null) {
                    images[i] = new Image();
                    images[i].Source = BitmapSource.Create(
                        512, 440,
                        96, 96,
                        PixelFormats.Gray8, BitmapPalettes.Gray256,
                        coordManager.GetGridMark((GridMarkPoint)i).Image,
                        512);
                }
            }

            // ボタンに画像を設定
            GridMarkLeftTopButton.Content = (images[(int)GridMarkPoint.LeftTop] != null ? images[(int)GridMarkPoint.LeftTop] : null);
            GridMarkCenterTopButton.Content = (images[(int)GridMarkPoint.CenterTop] != null ? images[(int)GridMarkPoint.CenterTop] : null);
            GridMarkRightTopButton.Content = (images[(int)GridMarkPoint.RightTop] != null ? images[(int)GridMarkPoint.RightTop] : null);
            GridMarkLeftMiddleButton.Content = (images[(int)GridMarkPoint.LeftMiddle] != null ? images[(int)GridMarkPoint.LeftMiddle] : null);
            GridMarkCenterMiddleButton.Content = (images[(int)GridMarkPoint.CenterMiddle] != null ? images[(int)GridMarkPoint.CenterMiddle] : null);
            GridMarkRightMiddleButton.Content = (images[(int)GridMarkPoint.RightMiddle] != null ? images[(int)GridMarkPoint.RightMiddle] : null);
            GridMarkLeftBottomButton.Content = (images[(int)GridMarkPoint.LeftBottom] != null ? images[(int)GridMarkPoint.LeftBottom] : null);
            GridMarkCenterBottomButton.Content = (images[(int)GridMarkPoint.CenterBottom] != null ? images[(int)GridMarkPoint.CenterBottom] : null);
            GridMarkRightBottomButton.Content = (images[(int)GridMarkPoint.RightBottom] != null ? images[(int)GridMarkPoint.RightBottom] : null);
        }

        /// <summary>
        /// 指定したグリッドマークに移動します．
        /// <para>グリッドマークが未定義の場合や他の処理が実行中の場合はダイアログな
        /// どでユーザーに確認をとります．移動先をスパイラルの原点にします。</para>
        /// </summary>
        /// <param name="mark"></param>
        private void moveToGridMark(GridMarkPoint mark) {
            bool continueFlag = true;

            // 指定されたグリッドマークが未定義の場合，予測値で移動させるかどうかを確認
            // ユーザーの選択が「いいえ(No)」の場合はここで終了
            if (!coordManager.GetGridMark(mark).Existed) {
                continueFlag = (MessageBox.Show(
                    Properties.Strings.GridMarkNotExistedPredMove,
                    "",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes);
            }

            Activity activity = new Activity(parameterManager);
            if (activity.IsActive) {
                continueFlag = (MessageBox.Show(
                    Properties.Strings.ActivityActiveException01,
                    "",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes);
            }

            if (continueFlag) {
                activity.Abort();
                GridMarkDefinitionUtil utility = new GridMarkDefinitionUtil(coordManager);
                MotorControler mc = MotorControler.GetInstance(parameterManager);
                mc.MovePointXY(utility.GetGridMarkCoord(mark));
                mc.SetSpiralCenterPoint();
            }
        }

        private void GridMarkLeftTopButton_Click(object sender, RoutedEventArgs e) {
            moveToGridMark(GridMarkPoint.LeftTop);
        }

        private void GridMarkCenterTopButton_Click(object sender, RoutedEventArgs e) {
            moveToGridMark(GridMarkPoint.CenterTop);
        }

        private void GridMarkRightTopButton_Click(object sender, RoutedEventArgs e) {
            moveToGridMark(GridMarkPoint.RightTop);
        }

        private void GridMarkLeftMiddleButton_Click(object sender, RoutedEventArgs e) {
            moveToGridMark(GridMarkPoint.LeftMiddle);
        }

        private void GridMarkCenterMiddleButton_Click(object sender, RoutedEventArgs e) {
            moveToGridMark(GridMarkPoint.CenterMiddle);
        }

        private void GridMarkRightBottomButton_Click(object sender, RoutedEventArgs e) {
            moveToGridMark(GridMarkPoint.RightMiddle);
        }

        private void GridMarkCenterBottomButton_Click(object sender, RoutedEventArgs e) {
            moveToGridMark(GridMarkPoint.CenterBottom);
        }

        private void GridMarkLeftBottomButton_Click(object sender, RoutedEventArgs e) {
            moveToGridMark(GridMarkPoint.LeftBottom);
        }

        private void GridMarkRightMiddleButton_Click(object sender, RoutedEventArgs e) {
            moveToGridMark(GridMarkPoint.RightMiddle);
        }

        private void GridMarkCenterMiddleButton_Click_1(object sender, RoutedEventArgs e) {
            moveToGridMark(GridMarkPoint.CenterMiddle);
        }

        private void GridMarkLeftMiddleButton_Click_1(object sender, RoutedEventArgs e) {
            moveToGridMark(GridMarkPoint.LeftMiddle);
        }

        private void GridMarkCenterTopButton_Click_1(object sender, RoutedEventArgs e) {
            moveToGridMark(GridMarkPoint.CenterTop);
        }

        private void gridMarksRecogButton_Click(object sender, RoutedEventArgs e) {
            // レンズが50倍に設定されていない場合は例外を返すようにしたいがやり方が分からん(20140724)
            try {
                IGridMarkRecognizer GridMarkRecognizer = coordManager;
                Vector2 viewerPoint = GridMarkRecognizer.SearchGridMarkx50();
                if (viewerPoint.X < 0 || viewerPoint.Y < 0) {
                    System.Diagnostics.Debug.WriteLine(String.Format("grid mark not found"));
                } else {
                    System.Diagnostics.Debug.WriteLine(String.Format("{0}  {1}", viewerPoint.X, viewerPoint.Y));
                }
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(String.Format("ex"));
            }
        }

        private void gridMarksSpiralSearchButton_Click(object sender, RoutedEventArgs e) {
            // レンズが50倍に設定されていない場合は例外を返すようにしたいがやり方が分からん(20140724)

            //現在地からスパイラルサーチ30視野でグリッドマークを検出する
            //検出したら視野の真ん中に持ってくる
            try {
                MotorControler mc = MotorControler.GetInstance(parameterManager);
                IGridMarkRecognizer GridMarkRecognizer = coordManager;
                mc.SetSpiralCenterPoint();
                Led led = Led.GetInstance();
                Vector2 encoderPoint = new Vector2(-1, -1);
                encoderPoint.X = mc.GetPoint().X;
                encoderPoint.Y = mc.GetPoint().Y;//おこられたのでしかたなくこうする　吉田20150427
                Vector2 viewerPoint = new Vector2(-1, -1);

                bool continueFlag = true;
                while (continueFlag) {
                    led.AdjustLight(parameterManager);
                    viewerPoint = GridMarkRecognizer.SearchGridMarkx50();
                    if (viewerPoint.X < 0 || viewerPoint.Y < 0) {
                        System.Diagnostics.Debug.WriteLine(String.Format("grid mark not found"));
                        mc.MoveInSpiral(true);
                        mc.Join();
                        continueFlag = (mc.SpiralIndex < 30);
                    } else {
                        System.Diagnostics.Debug.WriteLine(String.Format("******** {0}  {1}", viewerPoint.X, viewerPoint.Y));
                        encoderPoint = coordManager.TransToEmulsionCoord(viewerPoint);
                        mc.MovePointXY(encoderPoint);
                        mc.Join();
                        continueFlag = false;
                    }
                } // while

                mc.MovePointXY(encoderPoint);
                mc.Join();
                viewerPoint = GridMarkRecognizer.SearchGridMarkx50();
                encoderPoint = coordManager.TransToEmulsionCoord(viewerPoint);
                mc.MovePointXY(encoderPoint);
                mc.Join();

            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine("exception");
            }
        }

        private void goTheNearestGridMarkButton_Click(object sender, RoutedEventArgs e) {
            try {
                MotorControler mc = MotorControler.GetInstance(parameterManager);
                GridMark nearestMark = coordManager.GetTheNearestGridMark(mc.GetPoint());
                System.Diagnostics.Debug.WriteLine(String.Format("{0},  {1}", nearestMark.x, nearestMark.y));
                mc.MovePointXY(nearestMark.x, nearestMark.y);
                mc.Join();
            } catch (GridMarkNotFoundException ex) {
                System.Diagnostics.Debug.WriteLine(String.Format("{0}", ex.ToString()));
            }
        }


        private void SetHyperFineParam_Click(object sender, RoutedEventArgs e) {
            try {
                MotorControler mc = MotorControler.GetInstance(parameterManager);
                Vector3 CurrentCenterPoint = mc.GetPoint();
                GridMark nearestMark = coordManager.GetTheNearestGridMark(CurrentCenterPoint);
                System.Diagnostics.Debug.WriteLine(String.Format("{0},  {1}", CurrentCenterPoint.X, CurrentCenterPoint.Y));
                coordManager.HFDX = CurrentCenterPoint.X - nearestMark.x;
                coordManager.HFDY = CurrentCenterPoint.Y - nearestMark.y;
            } catch (EntryPointNotFoundException ex) {
                MessageBox.Show("エントリポイントが見当たりません。 " + ex.Message);
                System.Diagnostics.Debug.WriteLine("エントリポイントが見当たりません。 " + ex.Message);
            }
        }

        private void SeqHyperFineButton_Click(object sender, RoutedEventArgs e) {
            // レンズが50倍に設定されていない場合は例外を返すようにしたいがやり方が分からん(20140724)
            try {
                MotorControler mc = MotorControler.GetInstance(parameterManager);
                GridMark nearestMark = coordManager.GetTheNearestGridMark(mc.GetPoint());
                System.Diagnostics.Debug.WriteLine(String.Format("{0},  {1}", nearestMark.x, nearestMark.y));
                mc.MovePointXY(nearestMark.x, nearestMark.y);
                mc.Join();
            } catch (GridMarkNotFoundException ex) {
                System.Diagnostics.Debug.WriteLine(String.Format("{0}", ex.ToString()));
            }
            try {
                MotorControler mc = MotorControler.GetInstance(parameterManager);
                IGridMarkRecognizer GridMarkRecognizer = coordManager;
                mc.SetSpiralCenterPoint();
                Led led = Led.GetInstance();
                Vector2 encoderPoint = new Vector2(-1, -1);
                encoderPoint.X = mc.GetPoint().X;
                encoderPoint.Y = mc.GetPoint().Y;//おこられたのでしかたなくこうする　吉田20150427
                Vector2 viewerPoint = new Vector2(-1, -1);

                bool continueFlag = true;
                while (continueFlag) {
                    led.AdjustLight(parameterManager);
                    viewerPoint = GridMarkRecognizer.SearchGridMarkx50();
                    if (viewerPoint.X < 0 || viewerPoint.Y < 0) {
                        System.Diagnostics.Debug.WriteLine(String.Format("grid mark not found"));
                        mc.MoveInSpiral(true);
                        mc.Join();
                        continueFlag = (mc.SpiralIndex < 30);
                    } else {
                        System.Diagnostics.Debug.WriteLine(String.Format("******** {0}  {1}", viewerPoint.X, viewerPoint.Y));
                        encoderPoint = coordManager.TransToEmulsionCoord(viewerPoint);
                        mc.MovePointXY(encoderPoint);
                        mc.Join();
                        continueFlag = false;
                    }
                } // while

                //重心検出と移動を2回繰り返して、グリッドマークを視野中心にもっていく
                mc.MovePointXY(encoderPoint);
                mc.Join();
                viewerPoint = GridMarkRecognizer.SearchGridMarkx50();
                encoderPoint = coordManager.TransToEmulsionCoord(viewerPoint);
                mc.MovePointXY(encoderPoint);
                mc.Join();

            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine("exception");
            }
            try {
                MotorControler mc = MotorControler.GetInstance(parameterManager);
                Vector3 CurrentCenterPoint = mc.GetPoint();
                GridMark nearestMark = coordManager.GetTheNearestGridMark(CurrentCenterPoint);
                System.Diagnostics.Debug.WriteLine(String.Format("{0},  {1}", CurrentCenterPoint.X, CurrentCenterPoint.Y));
                coordManager.HFDX = CurrentCenterPoint.X - nearestMark.x;
                coordManager.HFDY = CurrentCenterPoint.Y - nearestMark.y;
            } catch (EntryPointNotFoundException ex) {
                MessageBox.Show("エントリポイントが見当たりません。 " + ex.Message);
                System.Diagnostics.Debug.WriteLine("エントリポイントが見当たりません。 " + ex.Message);
            }
        }


        private void BeamDetectionButton_Click(object sender, RoutedEventArgs e) {
            Camera camera = Camera.GetInstance();
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Vector3 CurrentPoint = mc.GetPoint();
            Vector3 p = new Vector3();
            int BinarizeThreshold = 10;
            int BrightnessThreshold = 7;
            Mat sum = Mat.Zeros(440, 512, MatType.CV_8UC1);

            string datfileName = string.Format(@"c:\img\{0}.dat", System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"));
            BinaryWriter writer = new BinaryWriter(File.Open(datfileName, FileMode.Create));

            for (int i = 0; i < 10; i++) {
                byte[] b = camera.ArrayImage;
                writer.Write(b);
                p = mc.GetPoint();
                Mat mat = new Mat(440, 512, MatType.CV_8U, b);
                mat.ImWrite(String.Format(@"c:\img\{0}_{1}_{2}_{3}.bmp",
                        System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"),
                        (int)(p.X * 1000),
                        (int)(p.Y * 1000),
                        (int)(p.Z * 1000)));
                Cv2.GaussianBlur(mat, mat, Cv.Size(3, 3), -1);
                Mat gau = mat.Clone();
                Cv2.GaussianBlur(gau, gau, Cv.Size(31, 31), -1);
                Cv2.Subtract(gau, mat, mat);
                Cv2.Threshold(mat, mat, BinarizeThreshold, 1, ThresholdType.Binary);
                Cv2.Add(sum, mat, sum);
                mc.MoveDistance(-0.003, VectorId.Z);
                mc.Join();
            }

            Cv2.Threshold(sum, sum, BrightnessThreshold, 1, ThresholdType.Binary);

            //Cv2.FindContoursをつかうとAccessViolationExceptionになる(Release/Debug両方)ので、C-API風に書く
            using (CvMemStorage storage = new CvMemStorage()) {
                using (CvContourScanner scanner = new CvContourScanner(sum.ToIplImage(), storage, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxSimple)) {
                    //string fileName = string.Format(@"c:\img\{0}.txt",
                    //        System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"));
                    string fileName = string.Format(@"c:\img\u.txt");

                    foreach (CvSeq<CvPoint> c in scanner) {
                        CvMoments mom = new CvMoments(c, false);
                        if (c.ElemSize < 2) continue;
                        if (mom.M00 == 0.0) continue;
                        double mx = mom.M10 / mom.M00;
                        double my = mom.M01 / mom.M00;
                        File.AppendAllText(fileName, string.Format("{0:F} {1:F}\n", mx, my));
                    }
                }
            }

            sum *= 255;
            sum.ImWrite(String.Format(@"c:\img\{0}_{1}_{2}.bmp",
                System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"),
                (int)(p.X * 1000),
                (int)(p.Y * 1000)));


            Vector2 encoderPoint = new Vector2(-1, -1);
            encoderPoint.X = mc.GetPoint().X;
            encoderPoint.Y = mc.GetPoint().Y;//おこられたのでしかたなくこうする　吉田20150427
            Vector2 viewerPoint = new Vector2(-1, -1);

            if (TigerPatternMatch.PatternMatch(ref viewerPoint)) {
                encoderPoint = coordManager.TransToEmulsionCoord(viewerPoint);
                mc.MovePointXY(encoderPoint);
                mc.Join();
            }

            //Vector2 vshift = new Vector2();
            //if(TigerPatternMatch.PatternMatch(vshift)){
            //    mc.MovePointXY(vshift);            
            //}
            /*
            OpenCvSharp.CPlusPlus.Point[][] contours;
            HiearchyIndex[] hierarchyindexes;
            try {
                Cv2.FindContours(sum, out contours, out hierarchyindexes, ContourRetrieval.External, ContourChain.ApproxSimple);

                string fileName = string.Format(@"c:\img\{0}_{1}_{2}.txt",
                                System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"),
                                (int)(p.X * 1000),
                                (int)(p.Y * 1000));
                for (int i = 0; i < contours.Length; i++) {
                    Moments mom = new Moments(contours[i]);
                    if (contours[i].Length < 2) continue;
                    if (mom.M00 == 0.0) continue;
                    double mx = mom.M10 / mom.M00;
                    double my = mom.M01 / mom.M00;
                    File.WriteAllText(fileName, string.Format("{0:F},{1:F}", mx, my));
                }

                sum *= 255;
                sum.ImWrite(String.Format(@"c:\img\{0}_{1}_{2}.bmp",
                    System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"),
                    (int)(p.X * 1000),
                    (int)(p.Y * 1000)));
            } catch (AccessViolationException ex) {
                System.Diagnostics.Debug.WriteLine("exception" + ex.Message);
            }
            */
            /*
            mc.Inch(MechaAxisAddress.ZAddress, PlusMinus.Minus);           
            bool flag = true;
            while (flag) {
                byte[] b = camera.ArrayImage;
                Mat mat = new Mat(440, 512, MatType.CV_8U, b);
                mat.ImWrite(String.Format(@"c:\img\{0}_seq.bmp", System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")));
                Vector3 p = mc.GetPoint();
                if ((CurrentPoint.Z - p.Z) > 0.1) flag = false;
            }
            mc.StopInching(MechaAxisAddress.ZAddress);
            mc.Join();
            */
        }


        private void moveToCoordButton_Click(object sender, RoutedEventArgs e) {
            // これから表示するダイアログのテキストボックスのTextChangedイベントハンドラ
            Predicate<string> func = delegate(string val) {
                bool retVal = true;
                try {
                    double.Parse(val);
                } catch (Exception) {
                    retVal = false;
                }
                return retVal;
            };

            CoordWindow coordWindow = new CoordWindow();
            coordWindow.UnitName = "mm";
            coordWindow.X_TextChanged = func;
            coordWindow.Y_TextChanged = func;
            if ((bool)coordWindow.ShowDialog()) {
                double x = double.Parse(coordWindow.X);
                double y = double.Parse(coordWindow.Y);
                MotorControler mc = MotorControler.GetInstance(parameterManager);
                mc.MovePointXY(x, y);
            }
        }

        private void takeAccumImagesButton_Click(object sender, RoutedEventArgs e) {
            ShootingStage ss = new ShootingStage(this, ShootingMode.Accumlative);
            ss.NextControl = Workspace.PresentControl;
            SetElementOnWorkspace(ss);
        }

        private void takeSingleImageButton_Click(object sender, RoutedEventArgs e) {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();

            byte[] b = camera.ArrayImage;
            Mat image = new Mat(440, 512, MatType.CV_8U, b);
            Vector3 CenterPoint = mc.GetPoint();
            image.ImWrite(string.Format(@"C:\test\single_pics\{0}_{1}_{2}.png", CenterPoint.X, CenterPoint.Y, CenterPoint.Z));

            return;
        }



        public struct mm {
            public double white_x;
            public double white_y;
            public double white_kido;
            public double white_dx;
            public double white_dy;
        }


        static double common_dx;
        static double common_dy;

        static double over_dx;
        static double over_dy;

        private void start_auto_following_Click(object sender, RoutedEventArgs e) {
            TracksManager tm = parameterManager.TracksManager;
            Track myTrack = tm.GetTrack(tm.TrackingIndex);
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();



            ///////////////////////////////////////////////////////////////////////

            //        for (int i = 0; i < 65 ;i++ ) 
            //        {
            //            byte[] b = camera.ArrayImage;
            //            Mat image = new Mat(440, 512, MatType.CV_8U, b);
            //            Mat imagec = image.Clone();
            //            imagec.ImWrite(string.Format(@"E:\track_circle\7601_135\d\{0}.bmp", i));
            //            mc.MoveDistance(-0.0020, VectorId.Z);
            //            mc.Join();
            //        }
            //        return;

            //return;



            /*
            double now_xx = mc.GetPoint().X;
            double now_yy = mc.GetPoint().Y;
            double now_zz = mc.GetPoint().Z;

            double now_zzz = mc.GetPoint().Z;

            int i = 1;

           while(now_zzz > -0.4234) 
            {
               System.IO.StreamWriter sw = new
               System.IO.StreamWriter("E:\\5201_126_down.txt",true);
               sw.WriteLine("{0} {1} {2}",mc.GetPoint().X,mc.GetPoint().Y,mc.GetPoint().Z);
               sw.Close();
               
               
               
               byte[] b = camera.ArrayImage;
               Mat image = new Mat(440, 512, MatType.CV_8U, b);
               Mat imagec = image.Clone();
               imagec.ImWrite(string.Format(@"E:\move_xyz\5201_126\down\{0}.bmp", i-1));

               double next_x = now_xx - i * myTrack.MsDX * 0.0020 * 2.264;//2μm間隔で撮影
               double next_y = now_yy - i * myTrack.MsDY * 0.0020 * 2.264;//Shrinkage Factor は2.2で計算(仮)
               double next_z = now_zz - i * 0.0020;

                mc.MovePoint(next_x, next_y,next_z);

                //mc.MoveDistance(-0.0020, VectorId.Z);
                mc.Join();
                i++;
                now_zzz = mc.GetPoint().Z;
            }
            return;


            */



            int nshot = 70;
            byte[] bb = new byte[440 * 512 * nshot];

            Vector3 now_p = mc.GetPoint();         //スライス画像を取るための残し

            DateTime starttime = System.DateTime.Now;
            string datfileName = string.Format(@"C:\Documents and Settings\stage1-user\デスクトップ\window_model\7601_135\d.dat");
            BinaryWriter writer = new BinaryWriter(File.Open(datfileName, FileMode.Create));

            string txtfileName = string.Format(@"C:\Documents and Settings\stage1-user\デスクトップ\window_model\7601_135\d.txt");
            StreamWriter twriter = File.CreateText(txtfileName);
            string stlog = "";



            int kk = 0;
            while (kk < nshot) {
                byte[] b = camera.ArrayImage;//画像を取得
                //Mat image = new Mat(440, 512, MatType.CV_8U, b);
                //Mat image_clone = image.Clone();
                b.CopyTo(bb, 440 * 512 * kk);

                //image_clone.ImWrite(string.Format(@"E:\20141015\5201_136\{0}.bmp", kk));
                stlog += String.Format("{0} {1} {2} {3} {4}\n",
                    System.DateTime.Now.ToString("HHmmss\\.fff"),
                    (now_p.X * 1000).ToString("0.0"),
                    (now_p.Y * 1000).ToString("0.0"),
                    (now_p.Z * 1000).ToString("0.0"),
                    kk);

                kk++;
                //mc.MovePointZ(now_p.Z - 0.0020);
                mc.MoveDistance(-0.0020, VectorId.Z);
                mc.Join();
            }


            twriter.Write(stlog);
            writer.Write(bb);
            writer.Flush();
            writer.Close();



            return;


            Surface surface = Surface.GetInstance(parameterManager);//表面認識から境界値を取得
            double uptop = surface.UpTop;
            double upbottom = surface.UpBottom;
            double lowtop = surface.LowTop;
            double lowbottom = surface.LowBottom;

            while (mc.GetPoint().Z >= upbottom + 0.030)//上ゲル内での連続移動
            {
                Follow();
            }

            if (mc.GetPoint().Z >= upbottom + 0.024) {
                double now_x = mc.GetPoint().X;
                double now_y = mc.GetPoint().Y;
                double now_z = mc.GetPoint().Z;
                mc.MovePoint(now_x - common_dx * (now_z - (upbottom + 0.024)) * 2.2, now_y - common_dy * (now_z - (upbottom + 0.024)) * 2.2, upbottom + 0.024);
            } else {
                double now_x = mc.GetPoint().X;
                double now_y = mc.GetPoint().Y;
                double now_z = mc.GetPoint().Z;
                mc.MovePoint(now_x - common_dx * ((upbottom + 0.024) - now_z) * 2.2, now_y - common_dy * ((upbottom + 0.024) - now_z) * 2.2, upbottom + 0.024);
            }

            while (mc.GetPoint().Z >= upbottom)//上ゲル内での連続移動
            {
                Follow();
            }

            mc.MovePoint(mc.GetPoint().X - common_dx * (upbottom - lowtop)
                , mc.GetPoint().Y - common_dy * (upbottom - lowtop)
                , lowtop);//Base下側への移動

        }

        static int number_of_follow;

        private void Follow() {
            TracksManager tm = parameterManager.TracksManager;
            Track myTrack = tm.GetTrack(tm.TrackingIndex);
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            List<Mat> image_set = new List<Mat>();
            List<Mat> image_set_reverse = new List<Mat>();

            double now_x = mc.GetPoint().X;
            double now_y = mc.GetPoint().Y;
            double now_z = mc.GetPoint().Z;

            number_of_follow++;

            if (number_of_follow == 1) {
                common_dx = myTrack.MsDX;
                common_dy = myTrack.MsDY;
            } else if (number_of_follow == 2) {
                common_dx = myTrack.MsDX + ((0.265625 * over_dx * 3) / (0.024 * 2.2 * 1000));
                common_dy = myTrack.MsDY - ((0.265625 * over_dy * 3) / (0.024 * 2.2 * 1000));
            } else {
                common_dx = common_dx + ((0.265625 * over_dx * 3) / (0.024 * 2.2 * 1000));
                common_dy = common_dy - ((0.265625 * over_dy * 3) / (0.024 * 2.2 * 1000));
            }


            for (int i = 0; i < 8; i++) {//myTrack.MsD○はdz1mmあたりのd○の変位mm
                double next_x = now_x - i * common_dx * 0.003 * 2.2;//3μm間隔で撮影
                double next_y = now_y - i * common_dy * 0.003 * 2.2;//Shrinkage Factor は2.2で計算(仮)
                mc.MovePoint(next_x, next_y, now_z - 0.003 * i);
                mc.Join();

                byte[] b = camera.ArrayImage;
                Mat image = new Mat(440, 512, MatType.CV_8U, b);
                Mat imagec = image.Clone();
                image_set.Add(imagec);
            }

            for (int i = 7; i >= 0; i--) {
                image_set_reverse.Add(image_set[i]);
            }



            int n = image_set.Count();//１回分の取得画像の枚数

            Mat cont = new Mat(440, 512, MatType.CV_8U);
            Mat gau_1 = new Mat(440, 512, MatType.CV_8U);
            Mat gau_2 = new Mat(440, 512, MatType.CV_8U);
            Mat sub = new Mat(440, 512, MatType.CV_8U);
            Mat bin = new Mat(440, 512, MatType.CV_8U);

            double Max_kido;
            double Min_kido;

            OpenCvSharp.CPlusPlus.Point maxloc;
            OpenCvSharp.CPlusPlus.Point minloc;

            List<Mat> two_set = new List<Mat>();
            List<Mat> Part_img = new List<Mat>();

            for (int i = 0; i < image_set.Count(); i++) {
                Cv2.GaussianBlur((Mat)image_set_reverse[i], gau_1, Cv.Size(3, 3), -1);//パラメータ見ないといけない。
                Cv2.GaussianBlur(gau_1, gau_2, Cv.Size(51, 51), -1);//パラメータ見ないといけない。
                Cv2.Subtract(gau_2, gau_1, sub);
                Cv2.MinMaxLoc(sub, out Min_kido, out Max_kido, out minloc, out maxloc);
                cont = (sub - Min_kido) * 255 / (Max_kido - Min_kido);
                cont.ImWrite(string.Format(@"C:\set\cont_{0}.bmp", i));
                Cv2.Threshold(cont, bin, 115, 1, ThresholdType.Binary);//パラメータ見ないといけない。
                two_set.Add(bin);
            }

            List<mm> white_area = new List<mm>();
            int x0 = 256;
            int y0 = 220;//視野の中心


            for (int delta_xx = -1; delta_xx <= 1; delta_xx++)//一番下の画像よりどれだけずらすか
                for (int delta_yy = -1; delta_yy <= 1; delta_yy++) {
                    {
                        //    //積層写真の型作り（行列の中身は０行列）
                        //    Mat superimposed = Mat.Zeros(440 + (n - 1) * Math.Abs(delta_yy), 512 + (n - 1) * Math.Abs(delta_xx), MatType.CV_8UC1);
                        //
                        //    //各写真の型作り
                        //    for (int i = 0; i < two_set.Count; i++) {
                        //        Mat Part = Mat.Zeros(440 + (n - 1) * Math.Abs(delta_yy), 512 + (n - 1) * Math.Abs(delta_xx), MatType.CV_8UC1);
                        //        Part_img.Add(Part);
                        //    }

                        //積層写真の型作り（行列の中身は０行列）
                        Mat superimposed = Mat.Zeros(440 + 3 * Math.Abs(delta_yy), 512 + 3 * Math.Abs(delta_xx), MatType.CV_8UC1);


                        //各写真の型作り
                        for (int i = 0; i < two_set.Count; i++) {
                            Mat Part = Mat.Zeros(440 + 3 * Math.Abs(delta_yy), 512 + 3 * Math.Abs(delta_xx), MatType.CV_8UC1);
                            Part_img.Add(Part);
                        }//2枚を１セットにしてずらす場合




                        if (delta_xx >= 0 && delta_yy >= 0)//画像の右下への移動
                        {
                            for (int i = 0; i < two_set.Count; i++) {
                                if (i == 0 || i == 1) {
                                    Part_img[i][
                                             0
                                         , 440
                                         , 0
                                         , 512
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 2 || i == 3) {
                                    Part_img[i][
                                             0 + Math.Abs(delta_yy)  //yの値のスタート地点
                                         , 440 + Math.Abs(delta_yy)  //yの値のゴール地点
                                         , 0 + Math.Abs(delta_xx)    //xの値のスタート地点
                                         , 512 + Math.Abs(delta_xx)  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 4 || i == 5) {
                                    Part_img[i][
                                             0 + 2 * Math.Abs(delta_yy)  //yの値のスタート地点
                                         , 440 + 2 * Math.Abs(delta_yy) //yの値のゴール地点
                                         , 0 + 2 * Math.Abs(delta_xx)    //xの値のスタート地点
                                         , 512 + 2 * Math.Abs(delta_xx)  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 6 || i == 7) {
                                    Part_img[i][
                                                 0 + 3 * Math.Abs(delta_yy)  //yの値のスタート地点
                                             , 440 + 3 * Math.Abs(delta_yy)  //yの値のゴール地点
                                             , 0 + 3 * Math.Abs(delta_xx)    //xの値のスタート地点
                                             , 512 + 3 * Math.Abs(delta_xx)  //xの値のゴール地点
                                             ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                            }
                            for (int i = 0; i < Part_img.Count(); i++) {
                                superimposed += Part_img[i];
                            }

                            Cv2.Threshold(superimposed, superimposed, 5, 255, ThresholdType.ToZero);//パラメータ見ないといけない。

                            superimposed.SubMat(0
                                                    , 440
                                                    , 0
                                                    , 512).CopyTo(superimposed);//１枚目の画像の大きさ、場所で切り取る
                        }



                        if (delta_xx >= 0 && delta_yy < 0)//画像の右上への移動
                        {
                            for (int i = 0; i < two_set.Count; i++) {
                                if (i == 0 || i == 1) {
                                    Part_img[i][
                                             0 + 3
                                         , 440 + 3
                                         , 0
                                         , 512
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 2 || i == 3) {
                                    Part_img[i][
                                             0 + 3 - 1  //yの値のスタート地点
                                         , 440 + 3 - 1  //yの値のゴール地点
                                         , 0 + Math.Abs(delta_xx)    //xの値のスタート地点
                                         , 512 + Math.Abs(delta_xx)  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 4 || i == 5) {
                                    Part_img[i][
                                             0 + 3 - 2  //yの値のスタート地点
                                         , 440 + 3 - 2  //yの値のゴール地点
                                         , 0 + 2 * Math.Abs(delta_xx)    //xの値のスタート地点
                                         , 512 + 2 * Math.Abs(delta_xx)  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 6 || i == 7) {
                                    Part_img[i][
                                                 0 + 3 - 3  //yの値のスタート地点
                                             , 440 + 3 - 3  //yの値のゴール地点
                                             , 0 + 3 * Math.Abs(delta_xx)    //xの値のスタート地点
                                             , 512 + 3 * Math.Abs(delta_xx)  //xの値のゴール地点
                                             ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                            }
                            for (int i = 0; i < Part_img.Count(); i++) {
                                superimposed += Part_img[i];
                            }

                            Cv2.Threshold(superimposed, superimposed, 5, 255, ThresholdType.ToZero);//パラメータ見ないといけない。

                            superimposed.SubMat(0 + 3
                                                    , 440 + 3
                                                    , 0
                                                    , 512).CopyTo(superimposed);//１枚目の画像の大きさ、場所で切り取る
                        }



                        if (delta_xx < 0 && delta_yy < 0)//画像の左上への移動 
                        {
                            for (int i = 0; i < two_set.Count; i++) {
                                if (i == 0 || i == 1) {
                                    Part_img[i][
                                             0 + 3
                                         , 440 + 3
                                         , 0 + 3
                                         , 512 + 3
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 2 || i == 3) {
                                    Part_img[i][
                                             0 + 3 - 1  //yの値のスタート地点
                                         , 440 + 3 - 1  //yの値のゴール地点
                                         , 0 + 3 - 1    //xの値のスタート地点
                                         , 512 + 3 - 1  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 4 || i == 5) {
                                    Part_img[i][
                                             0 + 3 - 2  //yの値のスタート地点
                                         , 440 + 3 - 2  //yの値のゴール地点
                                         , 0 + 3 - 2    //xの値のスタート地点
                                         , 512 + 3 - 2  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 6 || i == 7) {
                                    Part_img[i][
                                                 0 + 3 - 3  //yの値のスタート地点
                                             , 440 + 3 - 3  //yの値のゴール地点
                                             , 0 + 3 - 3    //xの値のスタート地点
                                             , 512 + 3 - 3  //xの値のゴール地点
                                             ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                            }
                            for (int i = 0; i < Part_img.Count(); i++) {
                                superimposed += Part_img[i];
                            }

                            Cv2.Threshold(superimposed, superimposed, 5, 255, ThresholdType.ToZero);//パラメータ見ないといけない。

                            superimposed.SubMat(0 + 3
                                                    , 440 + 3
                                                    , 0 + 3
                                                    , 512 + 3).CopyTo(superimposed);//１枚目の画像の大きさ、場所で切り取る
                        }


                        if (delta_xx < 0 && delta_yy >= 0)//画像の左下への移動
                        {
                            for (int i = 0; i < two_set.Count; i++) {
                                if (i == 0 || i == 1) {
                                    Part_img[i][
                                             0
                                         , 440
                                         , 0 + 3
                                         , 512 + 3
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 2 || i == 3) {
                                    Part_img[i][
                                             0 + Math.Abs(delta_yy)  //yの値のスタート地点
                                         , 440 + Math.Abs(delta_yy)  //yの値のゴール地点
                                         , 0 + 3 - 1    //xの値のスタート地点
                                         , 512 + 3 - 1  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 4 || i == 5) {
                                    Part_img[i][
                                             0 + 2 * Math.Abs(delta_yy)  //yの値のスタート地点
                                         , 440 + 2 * Math.Abs(delta_yy)  //yの値のゴール地点
                                         , 0 + 3 - 2    //xの値のスタート地点
                                         , 512 + 3 - 2  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 6 || i == 7) {
                                    Part_img[i][
                                                 0 + 3 * Math.Abs(delta_yy)  //yの値のスタート地点
                                             , 440 + 3 * Math.Abs(delta_yy)  //yの値のゴール地点
                                             , 0 + 3 - 3    //xの値のスタート地点
                                             , 512 + 3 - 3 //xの値のゴール地点
                                             ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                            }
                            for (int i = 0; i < Part_img.Count(); i++) {
                                superimposed += Part_img[i];
                            }

                            Cv2.Threshold(superimposed, superimposed, 5, 255, ThresholdType.ToZero);//パラメータ見ないといけない。

                            superimposed.SubMat(0
                                                    , 440
                                                    , 0 + 3
                                                    , 512 + 3).CopyTo(superimposed);//１枚目の画像の大きさ、場所で切り取る
                        }




                        //                       if (delta_xx < 0 && delta_yy >= 0) {
                        //                           for (int i = 0; i < two_set.Count; i++) {   //元画像を大きな型の左下において、そこから他の画像を右上に動かしながらおいていく。
                        //                               //下の式は、（元の画像の位置）に他の位置を引いていく。
                        //                               Part_img[i][
                        //                                       0 + (n - 1) * Math.Abs(delta_yy) - delta_yy * i  //yの値のスタート地点
                        //                                   , 440 + (n - 1) * Math.Abs(delta_yy) - delta_yy * i  //yの値のゴール地点
                        //                                   , 0 - delta_xx * i    //xの値のスタート地点
                        //                                   , 512 - delta_xx * i  //xの値のゴール地点
                        //                                   ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                        //                           }
                        //                       }
                        //
                        //                       if (delta_xx < 0 && delta_yy < 0) {
                        //                           for (int i = 0; i < two_set.Count; i++) {    //元画像を大きな型の左上において、そこから他の画像を右下に動かしながらおいていく。
                        //                               //下の式は、（元の画像の位置）に他の位置を足していく。
                        //                               Part_img[i][
                        //                                      0 - delta_yy * i
                        //                                  , 440 - delta_yy * i
                        //                                  , 0 - delta_xx * i
                        //                                  , 512 - delta_xx * i
                        //                                  ] = two_set[i];
                        //                           }
                        //                       }
                        //
                        //                       if (delta_xx >= 0 && delta_yy >= 0) {
                        //                           for (int i = 0; i < two_set.Count; i++) {    //元画像を大きな型の左上において、そこから他の画像を左上に動かしながらおいていく。
                        //                               //下の式は、（元の画像の位置）に他の位置を足していく。
                        //                               Part_img[i][
                        //                                       0 + (n - 1) * Math.Abs(delta_yy) - delta_yy * i  //yの値のスタート地点
                        //                                   , 440 + (n - 1) * Math.Abs(delta_yy) - delta_yy * i  //yの値のゴール地点
                        //                                  , 0 + (n - 1) * Math.Abs(delta_xx) - delta_xx * i
                        //                                  , 512 + (n - 1) * Math.Abs(delta_xx) - delta_xx * i
                        //                                  ] = two_set[i];
                        //                           }
                        //                       }
                        //
                        //                       if (delta_xx >= 0 && delta_yy < 0) {
                        //                           for (int i = 0; i < two_set.Count; i++) {    //元画像を大きな型の左上において、そこから他の画像を左下に動かしながらおいていく。
                        //                               //下の式は、（元の画像の位置）に他の位置を足していく。
                        //                               Part_img[i][
                        //                                       0 - delta_yy * i  //yの値のスタート地点
                        //                                   , 440 - delta_yy * i  //yの値のゴール地点
                        //                                  , 0 + (n - 1) * Math.Abs(delta_xx) - delta_xx * i
                        //                                  , 512 + (n - 1) * Math.Abs(delta_xx) - delta_xx * i
                        //                                  ] = two_set[i];
                        //                           }
                        //                       }







                        Mat one1 = Mat.Ones(y0 - 20, 512, MatType.CV_8UC1);//視野の中心からどれだけの窓を開けるか
                        Mat one2 = Mat.Ones(41, x0 - 20, MatType.CV_8UC1);
                        Mat one3 = Mat.Ones(41, 491 - x0, MatType.CV_8UC1);
                        Mat one4 = Mat.Ones(419 - y0, 512, MatType.CV_8UC1);

                        superimposed[0, y0 - 20, 0, 512] = one1 * 0;
                        superimposed[y0 - 20, y0 + 21, 0, x0 - 20] = one2 * 0;
                        superimposed[y0 - 20, y0 + 21, x0 + 21, 512] = one3 * 0;
                        superimposed[y0 + 21, 440, 0, 512] = one4 * 0;//中心から○μｍの正方形以外は黒くする。

                        superimposed.ImWrite("C:\\set\\superimposed25_1.bmp");





                        using (CvMemStorage storage = new CvMemStorage()) {
                            using (CvContourScanner scanner = new CvContourScanner(superimposed.ToIplImage(), storage, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxSimple)) {
                                foreach (CvSeq<CvPoint> c in scanner) {
                                    CvMoments mom = new CvMoments(c, false);
                                    if (c.ElemSize < 2) continue;
                                    if (mom.M00 == 0.0) continue;
                                    double mx = mom.M10 / mom.M00;
                                    double my = mom.M01 / mom.M00;
                                    mm koko = new mm();
                                    koko.white_x = mx;
                                    koko.white_y = my;
                                    koko.white_kido = mom.M00;
                                    koko.white_dx = delta_xx;
                                    koko.white_dy = delta_yy;
                                    white_area.Add(koko);
                                    stage.WriteLine(String.Format("mx={0:f2} , my={1:f2} , dx={2:f2} , dy={3:f2} , M={4:f2}", mx, my, delta_xx, delta_yy, mom.M00));
                                }
                            }
                        }
                        Part_img.Clear();

                    }//pixel移動x
                }//pixel移動y










            if (white_area.Count > 0) {
                double center_x = 0;
                double center_y = 0;
                double center_dx = 0;
                double center_dy = 0;
                double kido_sum = 0;
                for (int i = 0; i < white_area.Count; i++) {
                    kido_sum += white_area[i].white_kido;
                    center_x += white_area[i].white_x * white_area[i].white_kido;
                    center_y += white_area[i].white_y * white_area[i].white_kido;
                    center_dx += white_area[i].white_dx * white_area[i].white_kido;
                    center_dy += white_area[i].white_dy * white_area[i].white_kido;
                }
                center_x = center_x / kido_sum;
                center_y = center_y / kido_sum;
                center_dx = center_dx / kido_sum;
                center_dy = center_dy / kido_sum;

                int c_o_g_x;
                int c_o_g_y;
                if (center_x >= 0) {
                    c_o_g_x = (int)(center_x + 0.5);
                } else {
                    c_o_g_x = (int)(center_x - 0.5);
                }

                if (center_x >= 0) {
                    c_o_g_y = (int)(center_y + 0.5);
                } else {
                    c_o_g_y = (int)(center_y - 0.5);
                }

                int dx_pixel = c_o_g_x - x0;
                int dy_pixel = c_o_g_y - y0;

                double dx_micron = dx_pixel * 0.265625 / 1000;
                double dy_micron = dy_pixel * 0.265625 / 1000;

                double now_x2 = mc.GetPoint().X;
                double now_y2 = mc.GetPoint().Y;
                mc.MovePointXY(now_x2 - dx_micron, now_y2 + dy_micron);//pixelの軸とstageの軸の関係から
                mc.Join();

                over_dx = center_dx;
                over_dy = center_dy;
            }

        }


        private void OverallScanButton_Click(object sender, RoutedEventArgs e) {
            if (parameterManager.Magnification != 20) {
                MessageBox.Show(String.Format(Properties.Strings.LensTypeException02, 20));
                return;
            }
            OverAllScanConfigureDialog dialog = new OverAllScanConfigureDialog();
            if ((bool)dialog.ShowDialog()) {
                ActivityManager manager = ActivityManager.GetInstance(parameterManager);
                OverallScan os = OverallScan.GetInstance(parameterManager);
                os.NumOfViewX = dialog.NumOfViewX;
                os.NumOfViewY = dialog.NumOfViewY;
                os.DirectoryPath = dialog.DirectoryPath;
                manager.Enqueue(os);
                manager.Start();
            }
        }

        private void TigerPointScanButton_Click(object sender, RoutedEventArgs e) {
            if (parameterManager.Magnification != 50) {
                MessageBox.Show(String.Format(Properties.Strings.LensTypeException02, 50));
                return;
            }
            OverAllScanConfigureDialog dialog = new OverAllScanConfigureDialog();
            if ((bool)dialog.ShowDialog()) {
                ActivityManager manager = ActivityManager.GetInstance(parameterManager);
                TigerPointScan os = TigerPointScan.GetInstance(parameterManager);
                os.DirectoryPath = dialog.DirectoryPath;
                manager.Enqueue(os);
                manager.Start();
            }
        }


        private void ScaleMeasureActivityButton_Click(object sender, RoutedEventArgs e) {

            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            ScaleMeasure sm = ScaleMeasure.GetInstance(parameterManager);

            manager.Enqueue(sm);
            manager.Start();
        }

        private void ScaleFasterActivityButton_Click(object sender, RoutedEventArgs e) {

            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            ScaleFaster sm = ScaleFaster.GetInstance(parameterManager);

            manager.Enqueue(sm);
            manager.Start();
        }

        private void MCTestButton_Click(object sender, RoutedEventArgs e) {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            mc.DisplayStat();
            Vector3 currentpoint = new Vector3();

            for (int i = 0; i < 3; i++) {
                Vector3 distance = new Vector3(1, 1, 0.01);
                mc.Move(distance);
                mc.Join();
                currentpoint = mc.GetPoint();
                System.Console.WriteLine(string.Format("{0},{1},{2}", currentpoint.X, currentpoint.Y, currentpoint.Z));
            }
            mc.DisplayStat();
        }


        private void GridMeasureButton_Click(object sender, RoutedEventArgs e) {
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            GridMeasure07 gm = GridMeasure07.GetInstance(parameterManager);
            manager.Enqueue(gm);
            manager.Start();
        }

        private void GridMeasureButton373_Click(object sender, RoutedEventArgs e) {
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            GridMeasure373 gm = GridMeasure373.GetInstance(parameterManager);
            manager.Enqueue(gm);
            manager.Start();
        }

        private void Class1Button_Click(object sender, RoutedEventArgs e) {
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            Class1 c1 = Class1.GetInstance(parameterManager);
            manager.Enqueue(c1);
            manager.Start();
        }

        private void Class2Button_Click(object sender, RoutedEventArgs e) {
            if (parameterManager.Magnification != 50) {
                MessageBox.Show(String.Format(Properties.Strings.LensTypeException02, 50));
                return;
            }
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            Class2 c2 = Class2.GetInstance(parameterManager);

            manager.Enqueue(c2);
            manager.Start();
        }



        private void DATFileButton_Click(object sender, RoutedEventArgs e) {
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            DATFile dat = DATFile.GetInstance(parameterManager);

            manager.Enqueue(dat);
            manager.Start();
        }


        private void GridTakingButton_Click(object sender, RoutedEventArgs e) {
            if (parameterManager.Magnification != 10) {
                MessageBox.Show(String.Format(Properties.Strings.LensTypeException02, 10));
                return;
            }
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            GridTaking gt = GridTaking.GetInstance(parameterManager);

            manager.Enqueue(gt);
            manager.Start();
        }


        private void DownBeamButton_Click(object sender, RoutedEventArgs e) {
            if (parameterManager.Magnification != 50) {
                MessageBox.Show(String.Format(Properties.Strings.LensTypeException02, 50));
                return;
            }
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            DownBeam downbeam = DownBeam.GetInstance(parameterManager);

            manager.Enqueue(downbeam);
            manager.Start();
        }

        private void UpBeamButton_Click(object sender, RoutedEventArgs e) {
            if (parameterManager.Magnification != 50) {
                MessageBox.Show(String.Format(Properties.Strings.LensTypeException02, 50));
                return;
            }
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            UpBeam upbeam = UpBeam.GetInstance(parameterManager);

            manager.Enqueue(upbeam);
            manager.Start();
        }



        private void start_following_Click(object sender, RoutedEventArgs e) {//Ξ追跡アルゴリズム
            TracksManager tm = parameterManager.TracksManager;
            Track myTrack = tm.GetTrack(tm.TrackingIndex);
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            List<Mat> image_set = new List<Mat>();
            List<Mat> image_set_reverse = new List<Mat>();

            Surface surface = Surface.GetInstance(parameterManager);//表面認識から境界値を取得
            double uptop = surface.UpTop;
            double upbottom = surface.UpBottom;
            double lowtop = surface.LowTop;
            double lowbottom = surface.LowBottom;

            double now_x = mc.GetPoint().X;
            double now_y = mc.GetPoint().Y;
            double now_z = mc.GetPoint().Z;


            common_dx = myTrack.MsDX + ((0.265625 * over_dx * 3) / (0.024 * 2.2 * 1000));
            common_dy = myTrack.MsDY - ((0.265625 * over_dy * 3) / (0.024 * 2.2 * 1000));


            for (int i = 0; i < 8; i++) {//myTrack.MsD○はdz1mmあたりのd○の変位mm
                double next_x = now_x - i * common_dx * 0.003 * 2.2;//3μm間隔で撮影
                double next_y = now_y - i * common_dy * 0.003 * 2.2;//Shrinkage Factor は2.2で計算(仮)
                mc.MovePoint(next_x, next_y, now_z - 0.003 * i);
                mc.Join();

                byte[] b = camera.ArrayImage;
                Mat image = new Mat(440, 512, MatType.CV_8U, b);
                Mat imagec = image.Clone();
                image_set.Add(imagec);
            }

            for (int i = 7; i >= 0; i--) {
                image_set_reverse.Add(image_set[i]);
            }



            int n = image_set.Count();//１回分の取得画像の枚数

            Mat cont = new Mat(440, 512, MatType.CV_8U);
            Mat gau_1 = new Mat(440, 512, MatType.CV_8U);
            Mat gau_2 = new Mat(440, 512, MatType.CV_8U);
            Mat sub = new Mat(440, 512, MatType.CV_8U);
            Mat bin = new Mat(440, 512, MatType.CV_8U);

            double Max_kido;
            double Min_kido;

            OpenCvSharp.CPlusPlus.Point maxloc;
            OpenCvSharp.CPlusPlus.Point minloc;

            List<Mat> two_set = new List<Mat>();
            List<Mat> Part_img = new List<Mat>();

            for (int i = 0; i < image_set.Count(); i++) {
                Cv2.GaussianBlur((Mat)image_set_reverse[i], gau_1, Cv.Size(3, 3), -1);//パラメータ見ないといけない。
                Cv2.GaussianBlur(gau_1, gau_2, Cv.Size(51, 51), -1);//パラメータ見ないといけない。
                Cv2.Subtract(gau_2, gau_1, sub);
                Cv2.MinMaxLoc(sub, out Min_kido, out Max_kido, out minloc, out maxloc);
                cont = (sub - Min_kido) * 255 / (Max_kido - Min_kido);
                cont.ImWrite(string.Format(@"C:\set\cont_{0}.bmp", i));
                Cv2.Threshold(cont, bin, 115, 1, ThresholdType.Binary);//パラメータ見ないといけない。
                two_set.Add(bin);
            }

            List<mm> white_area = new List<mm>();
            int x0 = 256;
            int y0 = 220;//視野の中心


            for (int delta_xx = -1; delta_xx <= 1; delta_xx++)//一番下の画像よりどれだけずらすか
                for (int delta_yy = -1; delta_yy <= 1; delta_yy++) {
                    {
                        //    //積層写真の型作り（行列の中身は０行列）
                        //    Mat superimposed = Mat.Zeros(440 + (n - 1) * Math.Abs(delta_yy), 512 + (n - 1) * Math.Abs(delta_xx), MatType.CV_8UC1);
                        //
                        //    //各写真の型作り
                        //    for (int i = 0; i < two_set.Count; i++) {
                        //        Mat Part = Mat.Zeros(440 + (n - 1) * Math.Abs(delta_yy), 512 + (n - 1) * Math.Abs(delta_xx), MatType.CV_8UC1);
                        //        Part_img.Add(Part);
                        //    }

                        //積層写真の型作り（行列の中身は０行列）
                        Mat superimposed = Mat.Zeros(440 + 3 * Math.Abs(delta_yy), 512 + 3 * Math.Abs(delta_xx), MatType.CV_8UC1);


                        //各写真の型作り
                        for (int i = 0; i < two_set.Count; i++) {
                            Mat Part = Mat.Zeros(440 + 3 * Math.Abs(delta_yy), 512 + 3 * Math.Abs(delta_xx), MatType.CV_8UC1);
                            Part_img.Add(Part);
                        }//2枚を１セットにしてずらす場合




                        if (delta_xx >= 0 && delta_yy >= 0)//画像の右下への移動
                        {
                            for (int i = 0; i < two_set.Count; i++) {
                                if (i == 0 || i == 1) {
                                    Part_img[i][
                                             0
                                         , 440
                                         , 0
                                         , 512
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 2 || i == 3) {
                                    Part_img[i][
                                             0 + Math.Abs(delta_yy)  //yの値のスタート地点
                                         , 440 + Math.Abs(delta_yy)  //yの値のゴール地点
                                         , 0 + Math.Abs(delta_xx)    //xの値のスタート地点
                                         , 512 + Math.Abs(delta_xx)  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 4 || i == 5) {
                                    Part_img[i][
                                             0 + 2 * Math.Abs(delta_yy)  //yの値のスタート地点
                                         , 440 + 2 * Math.Abs(delta_yy) //yの値のゴール地点
                                         , 0 + 2 * Math.Abs(delta_xx)    //xの値のスタート地点
                                         , 512 + 2 * Math.Abs(delta_xx)  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 6 || i == 7) {
                                    Part_img[i][
                                                 0 + 3 * Math.Abs(delta_yy)  //yの値のスタート地点
                                             , 440 + 3 * Math.Abs(delta_yy)  //yの値のゴール地点
                                             , 0 + 3 * Math.Abs(delta_xx)    //xの値のスタート地点
                                             , 512 + 3 * Math.Abs(delta_xx)  //xの値のゴール地点
                                             ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                            }
                            for (int i = 0; i < Part_img.Count(); i++) {
                                superimposed += Part_img[i];
                            }

                            Cv2.Threshold(superimposed, superimposed, 5, 255, ThresholdType.ToZero);//パラメータ見ないといけない。

                            superimposed.SubMat(0
                                                    , 440
                                                    , 0
                                                    , 512).CopyTo(superimposed);//１枚目の画像の大きさ、場所で切り取る
                        }



                        if (delta_xx >= 0 && delta_yy < 0)//画像の右上への移動
                        {
                            for (int i = 0; i < two_set.Count; i++) {
                                if (i == 0 || i == 1) {
                                    Part_img[i][
                                             0 + 3
                                         , 440 + 3
                                         , 0
                                         , 512
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 2 || i == 3) {
                                    Part_img[i][
                                             0 + 3 - 1  //yの値のスタート地点
                                         , 440 + 3 - 1  //yの値のゴール地点
                                         , 0 + Math.Abs(delta_xx)    //xの値のスタート地点
                                         , 512 + Math.Abs(delta_xx)  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 4 || i == 5) {
                                    Part_img[i][
                                             0 + 3 - 2  //yの値のスタート地点
                                         , 440 + 3 - 2  //yの値のゴール地点
                                         , 0 + 2 * Math.Abs(delta_xx)    //xの値のスタート地点
                                         , 512 + 2 * Math.Abs(delta_xx)  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 6 || i == 7) {
                                    Part_img[i][
                                                 0 + 3 - 3  //yの値のスタート地点
                                             , 440 + 3 - 3  //yの値のゴール地点
                                             , 0 + 3 * Math.Abs(delta_xx)    //xの値のスタート地点
                                             , 512 + 3 * Math.Abs(delta_xx)  //xの値のゴール地点
                                             ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                            }
                            for (int i = 0; i < Part_img.Count(); i++) {
                                superimposed += Part_img[i];
                            }

                            Cv2.Threshold(superimposed, superimposed, 5, 255, ThresholdType.ToZero);//パラメータ見ないといけない。

                            superimposed.SubMat(0 + 3
                                                    , 440 + 3
                                                    , 0
                                                    , 512).CopyTo(superimposed);//１枚目の画像の大きさ、場所で切り取る
                        }



                        if (delta_xx < 0 && delta_yy < 0)//画像の左上への移動 
                        {
                            for (int i = 0; i < two_set.Count; i++) {
                                if (i == 0 || i == 1) {
                                    Part_img[i][
                                             0 + 3
                                         , 440 + 3
                                         , 0 + 3
                                         , 512 + 3
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 2 || i == 3) {
                                    Part_img[i][
                                             0 + 3 - 1  //yの値のスタート地点
                                         , 440 + 3 - 1  //yの値のゴール地点
                                         , 0 + 3 - 1    //xの値のスタート地点
                                         , 512 + 3 - 1  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 4 || i == 5) {
                                    Part_img[i][
                                             0 + 3 - 2  //yの値のスタート地点
                                         , 440 + 3 - 2  //yの値のゴール地点
                                         , 0 + 3 - 2    //xの値のスタート地点
                                         , 512 + 3 - 2  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 6 || i == 7) {
                                    Part_img[i][
                                                 0 + 3 - 3  //yの値のスタート地点
                                             , 440 + 3 - 3  //yの値のゴール地点
                                             , 0 + 3 - 3    //xの値のスタート地点
                                             , 512 + 3 - 3  //xの値のゴール地点
                                             ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                            }
                            for (int i = 0; i < Part_img.Count(); i++) {
                                superimposed += Part_img[i];
                            }

                            Cv2.Threshold(superimposed, superimposed, 5, 255, ThresholdType.ToZero);//パラメータ見ないといけない。

                            superimposed.SubMat(0 + 3
                                                    , 440 + 3
                                                    , 0 + 3
                                                    , 512 + 3).CopyTo(superimposed);//１枚目の画像の大きさ、場所で切り取る
                        }


                        if (delta_xx < 0 && delta_yy >= 0)//画像の左下への移動
                        {
                            for (int i = 0; i < two_set.Count; i++) {
                                if (i == 0 || i == 1) {
                                    Part_img[i][
                                             0
                                         , 440
                                         , 0 + 3
                                         , 512 + 3
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 2 || i == 3) {
                                    Part_img[i][
                                             0 + Math.Abs(delta_yy)  //yの値のスタート地点
                                         , 440 + Math.Abs(delta_yy)  //yの値のゴール地点
                                         , 0 + 3 - 1    //xの値のスタート地点
                                         , 512 + 3 - 1  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 4 || i == 5) {
                                    Part_img[i][
                                             0 + 2 * Math.Abs(delta_yy)  //yの値のスタート地点
                                         , 440 + 2 * Math.Abs(delta_yy)  //yの値のゴール地点
                                         , 0 + 3 - 2    //xの値のスタート地点
                                         , 512 + 3 - 2  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                } else if (i == 6 || i == 7) {
                                    Part_img[i][
                                                 0 + 3 * Math.Abs(delta_yy)  //yの値のスタート地点
                                             , 440 + 3 * Math.Abs(delta_yy)  //yの値のゴール地点
                                             , 0 + 3 - 3    //xの値のスタート地点
                                             , 512 + 3 - 3 //xの値のゴール地点
                                             ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                            }
                            for (int i = 0; i < Part_img.Count(); i++) {
                                superimposed += Part_img[i];
                            }

                            Cv2.Threshold(superimposed, superimposed, 5, 255, ThresholdType.ToZero);//パラメータ見ないといけない。

                            superimposed.SubMat(0
                                                    , 440
                                                    , 0 + 3
                                                    , 512 + 3).CopyTo(superimposed);//１枚目の画像の大きさ、場所で切り取る
                        }




                        //                       if (delta_xx < 0 && delta_yy >= 0) {
                        //                           for (int i = 0; i < two_set.Count; i++) {   //元画像を大きな型の左下において、そこから他の画像を右上に動かしながらおいていく。
                        //                               //下の式は、（元の画像の位置）に他の位置を引いていく。
                        //                               Part_img[i][
                        //                                       0 + (n - 1) * Math.Abs(delta_yy) - delta_yy * i  //yの値のスタート地点
                        //                                   , 440 + (n - 1) * Math.Abs(delta_yy) - delta_yy * i  //yの値のゴール地点
                        //                                   , 0 - delta_xx * i    //xの値のスタート地点
                        //                                   , 512 - delta_xx * i  //xの値のゴール地点
                        //                                   ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                        //                           }
                        //                       }
                        //
                        //                       if (delta_xx < 0 && delta_yy < 0) {
                        //                           for (int i = 0; i < two_set.Count; i++) {    //元画像を大きな型の左上において、そこから他の画像を右下に動かしながらおいていく。
                        //                               //下の式は、（元の画像の位置）に他の位置を足していく。
                        //                               Part_img[i][
                        //                                      0 - delta_yy * i
                        //                                  , 440 - delta_yy * i
                        //                                  , 0 - delta_xx * i
                        //                                  , 512 - delta_xx * i
                        //                                  ] = two_set[i];
                        //                           }
                        //                       }
                        //
                        //                       if (delta_xx >= 0 && delta_yy >= 0) {
                        //                           for (int i = 0; i < two_set.Count; i++) {    //元画像を大きな型の左上において、そこから他の画像を左上に動かしながらおいていく。
                        //                               //下の式は、（元の画像の位置）に他の位置を足していく。
                        //                               Part_img[i][
                        //                                       0 + (n - 1) * Math.Abs(delta_yy) - delta_yy * i  //yの値のスタート地点
                        //                                   , 440 + (n - 1) * Math.Abs(delta_yy) - delta_yy * i  //yの値のゴール地点
                        //                                  , 0 + (n - 1) * Math.Abs(delta_xx) - delta_xx * i
                        //                                  , 512 + (n - 1) * Math.Abs(delta_xx) - delta_xx * i
                        //                                  ] = two_set[i];
                        //                           }
                        //                       }
                        //
                        //                       if (delta_xx >= 0 && delta_yy < 0) {
                        //                           for (int i = 0; i < two_set.Count; i++) {    //元画像を大きな型の左上において、そこから他の画像を左下に動かしながらおいていく。
                        //                               //下の式は、（元の画像の位置）に他の位置を足していく。
                        //                               Part_img[i][
                        //                                       0 - delta_yy * i  //yの値のスタート地点
                        //                                   , 440 - delta_yy * i  //yの値のゴール地点
                        //                                  , 0 + (n - 1) * Math.Abs(delta_xx) - delta_xx * i
                        //                                  , 512 + (n - 1) * Math.Abs(delta_xx) - delta_xx * i
                        //                                  ] = two_set[i];
                        //                           }
                        //                       }







                        Mat one1 = Mat.Ones(y0 - 20, 512, MatType.CV_8UC1);//視野の中心からどれだけの窓を開けるか
                        Mat one2 = Mat.Ones(41, x0 - 20, MatType.CV_8UC1);
                        Mat one3 = Mat.Ones(41, 491 - x0, MatType.CV_8UC1);
                        Mat one4 = Mat.Ones(419 - y0, 512, MatType.CV_8UC1);

                        superimposed[0, y0 - 20, 0, 512] = one1 * 0;
                        superimposed[y0 - 20, y0 + 21, 0, x0 - 20] = one2 * 0;
                        superimposed[y0 - 20, y0 + 21, x0 + 21, 512] = one3 * 0;
                        superimposed[y0 + 21, 440, 0, 512] = one4 * 0;//中心から○μｍの正方形以外は黒くする。

                        superimposed.ImWrite("C:\\set\\superimposed25_1.bmp");





                        using (CvMemStorage storage = new CvMemStorage()) {
                            using (CvContourScanner scanner = new CvContourScanner(superimposed.ToIplImage(), storage, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxSimple)) {
                                foreach (CvSeq<CvPoint> c in scanner) {
                                    CvMoments mom = new CvMoments(c, false);
                                    if (c.ElemSize < 2) continue;
                                    if (mom.M00 == 0.0) continue;
                                    double mx = mom.M10 / mom.M00;
                                    double my = mom.M01 / mom.M00;
                                    mm koko = new mm();
                                    koko.white_x = mx;
                                    koko.white_y = my;
                                    koko.white_kido = mom.M00;
                                    koko.white_dx = delta_xx;
                                    koko.white_dy = delta_yy;
                                    white_area.Add(koko);
                                    stage.WriteLine(String.Format("mx={0:f2} , my={1:f2} , dx={2:f2} , dy={3:f2} , M={4:f2}", mx, my, delta_xx, delta_yy, mom.M00));
                                }
                            }
                        }
                        Part_img.Clear();

                    }//pixel移動x
                }//pixel移動y







            if (white_area.Count > 0) {
                double center_x = 0;
                double center_y = 0;
                double center_dx = 0;
                double center_dy = 0;
                double kido_sum = 0;
                for (int i = 0; i < white_area.Count; i++) {
                    kido_sum += white_area[i].white_kido;
                    center_x += white_area[i].white_x * white_area[i].white_kido;
                    center_y += white_area[i].white_y * white_area[i].white_kido;
                    center_dx += white_area[i].white_dx * white_area[i].white_kido;
                    center_dy += white_area[i].white_dy * white_area[i].white_kido;
                }
                center_x = center_x / kido_sum;
                center_y = center_y / kido_sum;
                center_dx = center_dx / kido_sum;
                center_dy = center_dy / kido_sum;

                int c_o_g_x;
                int c_o_g_y;
                if (center_x >= 0) {
                    c_o_g_x = (int)(center_x + 0.5);
                } else {
                    c_o_g_x = (int)(center_x - 0.5);
                }

                if (center_x >= 0) {
                    c_o_g_y = (int)(center_y + 0.5);
                } else {
                    c_o_g_y = (int)(center_y - 0.5);
                }

                int dx_pixel = c_o_g_x - x0;
                int dy_pixel = c_o_g_y - y0;

                double dx_micron = dx_pixel * 0.265625 / 1000;
                double dy_micron = dy_pixel * 0.265625 / 1000;

                double now_x2 = mc.GetPoint().X;
                double now_y2 = mc.GetPoint().Y;
                mc.MovePointXY(now_x2 - dx_micron, now_y2 + dy_micron);//pixelの軸とstageの軸の関係から
                mc.Join();

                over_dx = center_dx;
                over_dy = center_dy;
            }


        }

        private void OverallAbortButton_Click(object sender, RoutedEventArgs e) {
            ActivityManager am = ActivityManager.GetInstance();
            am.Abort();
            MessageBox.Show(Properties.Strings.ActivityAbort);



        }
        private void GotoUpTop_button(object sender, RoutedEventArgs e) {

            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            TracksManager tm = parameterManager.TracksManager;
            Track myTrack = tm.GetTrack(tm.TrackingIndex);
            Surface surface = Surface.GetInstance(parameterManager);
            int mod = parameterManager.ModuleNo;
            int pl = parameterManager.PlateNo;
            if (mc.IsMoving) {
                MessageBoxResult r = MessageBox.Show(
                    Properties.Strings.SurfaceException01,
                    Properties.Strings.Abort + "?",
                    MessageBoxButton.YesNo);
                if (r == MessageBoxResult.Yes) {
                    mc.AbortMoving();
                } else {
                    return;
                }
            }

            // すでに表面認識が実行中であれば停止するかどうか尋ねる．
            //Surface surface = Surface.GetInstance(parameterManager);
            if (surface.IsActive) {
                MessageBoxResult r = MessageBox.Show(
                    Properties.Strings.SurfaceException02,
                    Properties.Strings.Abort + "?",
                    MessageBoxButton.YesNo);
                if (r == MessageBoxResult.Yes) {
                    surface.Abort();
                } else {
                    return;
                }
            }

            try {
                surface.Start(true);

            } catch (Exception ex) {
                MessageBox.Show(ex.Message, Properties.Strings.Error);
            }

            mc.Join();

            try {

                string datarootdirpath = string.Format(@"C:\test\bpm\{0}", mod);
                System.IO.DirectoryInfo mydir = System.IO.Directory.CreateDirectory(datarootdirpath);
                string[] sp = myTrack.IdString.Split('-');
                string uptxt = string.Format(@"c:\test\bpm\{0}\{1}-{2}-{3}-{4}_up.txt", mod, mod, pl, sp[0], sp[1]);
                string dwtxt = string.Format(@"c:\test\bpm\{0}\{1}-{2}-{3}-{4}_dw.txt", mod, mod, pl - 1, sp[0], sp[1]);
                BeamDetection(uptxt, true);

                BeamPatternMatch bpm = new BeamPatternMatch(8, 200);
                bpm.ReadTrackDataTxtFile(dwtxt, false);

                bpm.ReadTrackDataTxtFile(uptxt, true);
                bpm.DoPatternMatch();

                stage.WriteLine(String.Format("pattern match dx,dy = {0}, {1}", bpm.GetPeakX() * 0.2625 * 0.001, bpm.GetPeakY() * 0.2625 * 0.001));
                Vector3 BfPoint = mc.GetPoint();
                mc.MoveDistance(bpm.GetPeakX() * 0.2625 * 0.001, VectorId.X);
                mc.Join();
                mc.MoveDistance(-bpm.GetPeakY() * 0.2625 * 0.001, VectorId.Y);
                mc.Join();
                Led led = Led.GetInstance();
                led.AdjustLight(parameterManager);
                Vector3 AfPoint = mc.GetPoint();
                stage.WriteLine(String.Format("Move dx,dy = {0}, {1}", BfPoint.X - AfPoint.X, BfPoint.Y - AfPoint.Y));

            } catch (ArgumentOutOfRangeException) {
                MessageBox.Show("ID numver is not existed。 ");
            } catch (System.Exception) {
                MessageBox.Show("No beam battern。 ");
            }

            /*   Vector3 cc = mc.GetPoint();
               double Zp = surface.UpTop;
               mc.MoveTo(new Vector3(cc.X, cc.Y, Zp));                
               mc.Join();      */

        }



        private void coordinate_record(object sender, RoutedEventArgs e) {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Vector3 CenterPoint = mc.GetPoint();

            string fileName = string.Format(@"c:\test\coordinate.txt");
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fileName, true, System.Text.Encoding.GetEncoding("shift_jis"));
            string coordinate = string.Format("{0} {1} {2}\n", CenterPoint.X, CenterPoint.Y, CenterPoint.Z);
            sw.Write(coordinate);
            sw.Close();           
        }



        private void coordinate_init(object sender, RoutedEventArgs e) {
            string fileName = string.Format(@"c:\test\coordinate.txt");
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fileName, false, System.Text.Encoding.GetEncoding("shift_jis"));
            string coordinate = string.Format("");
            sw.Write(coordinate);
            sw.Close();
        }



//       ....................2016/5/18.........../MKS for automatic track following combination////////////////

        //.. Approximatestraightbase
        static Point2d ApproximateStraightBase(double sh, double sh_low, Point3d ltrack, Point3d ltrack2, Point3d ltrack3, Surface surface) {
            Point2d p = new Point2d();

            int n = 3;
            double ltrack2_sh;
            double ltrack3_sh;

            //ltrack2の座標とbase上面、下ゲル上面の距離の差を求める。二乗してあるのは負の値にならないようにするためである。
            double lt2_upbottom_dis = ltrack2.Z - surface.UpBottom;
            double lt2_upb_pow = Math.Pow(lt2_upbottom_dis, 2);

            double lt2_lowtop_dis = ltrack2.Z - surface.LowTop;
            double lt2_lowt_pow = Math.Pow(lt2_lowtop_dis, 2);


            if (lt2_upb_pow < lt2_lowt_pow)//ltrack2の座標とbase上面の距離が、ltrack2の座標と下ゲル上面の距離より小さい場合、ltrack2はbase上にあることになる。
            {
                ltrack2_sh = (ltrack2.Z - ltrack.Z) * sh + ltrack.Z;//ltrack2とltrack3の間にbaseがある場合。
                ltrack3_sh = (ltrack3.Z - surface.LowTop) * sh_low + (surface.LowTop - ltrack2.Z) + (ltrack2.Z - ltrack.Z) * sh + ltrack.Z;
            } else//ltrack2の座標とbase上面の距離が、ltrack2の座標と下ゲル上面の距離より大きい場合、ltrack2は下ゲル上にあることになる。
            {
                ltrack2_sh = (ltrack2.Z - surface.LowTop) * sh_low + (surface.LowTop - ltrack.Z) + ltrack.Z;//ltrackとltrack2の間にbaseがある場合。
                ltrack3_sh = (ltrack3.Z - ltrack2.Z) * sh_low + (ltrack2.Z - surface.LowTop) * sh_low + (surface.LowTop - ltrack.Z) + ltrack.Z;
            }

            double sum_X = ltrack.X + ltrack2.X + ltrack3.X;
            double sum_Y = ltrack.Y + ltrack2.Y + ltrack3.Y;
            double sum_Z = ltrack.Z + ltrack2_sh + ltrack3_sh;
            double sum_ZZ = ltrack.Z * ltrack.Z + ltrack2_sh * ltrack2_sh + ltrack3_sh * ltrack3_sh;
            double sum_XZ = ltrack.Z * ltrack.X + ltrack2_sh * ltrack2.X + ltrack3_sh * ltrack3.X;

            double sum_YZ = ltrack.Z * ltrack.Y + ltrack2_sh * ltrack2.Y + ltrack3_sh * ltrack3.Y;

            double angle_x = (n * sum_XZ - sum_X * sum_Z) / (n * sum_ZZ - sum_Z * sum_Z);
            double angle_y = (n * sum_YZ - sum_Y * sum_Z) / (n * sum_ZZ - sum_Z * sum_Z);


            p = new Point2d(angle_x, angle_y);

            return p;
        }

        //...................................................................
        //ApproximateStraight
        static Point2d ApproximateStraight(double sh, Point3d ltrack, Point3d ltrack2, Point3d ltrack3) {
            Point2d p = new Point2d();

            int n = 3;

            double ltrack2_sh = (ltrack2.Z - ltrack.Z) * sh + ltrack.Z;
            double ltrack3_sh = (ltrack3.Z - ltrack2.Z) * sh + (ltrack2.Z - ltrack.Z) * sh + ltrack.Z;

            double sum_X = ltrack.X + ltrack2.X + ltrack3.X;
            double sum_Y = ltrack.Y + ltrack2.Y + ltrack3.Y;
            double sum_Z = ltrack.Z + ltrack2_sh + ltrack3_sh;
            double sum_ZZ = ltrack.Z * ltrack.Z + ltrack2_sh * ltrack2_sh + ltrack3_sh * ltrack3_sh;
            double sum_XZ = ltrack.Z * ltrack.X + ltrack2_sh * ltrack2.X + ltrack3_sh * ltrack3.X;

            double sum_YZ = ltrack.Z * ltrack.Y + ltrack2_sh * ltrack2.Y + ltrack3_sh * ltrack3.Y;

            double angle_x = (n * sum_XZ - sum_X * sum_Z) / (n * sum_ZZ - sum_Z * sum_Z);
            double angle_y = (n * sum_YZ - sum_Y * sum_Z) / (n * sum_ZZ - sum_Z * sum_Z);


            p = new Point2d(angle_x, angle_y);

            return p;
        }

        //.........................................................

        public struct rawmicrotrack {//Raw microtrack

            public int ph;
            public int pv;
            public int ax;
            public int ay;
            public int cx;
            public int cy;
        }
        //...............................................

        // track detection....................///////


        static OpenCvSharp.CPlusPlus.Point TrackDetection_naka(List<Mat> mats, int px, int py, int shiftx = 2, int shifty = 2, int shiftpitch = 4, int windowsize = 40, int phthresh = 5, double dx = 1.2, double dy = 1.2, bool debugflag = false) {
            int x0 = px - 256;
            int y0 = py - 220;

            List<rawmicrotrack> rms = new List<rawmicrotrack>();

            // Point2d pixel_cen = TrackDetection(binimages, 256, 220, 3, 3, 4, 90, 3);

            double phi = Math.Atan2(dy, dx);
            double angle = phi * (180 / Math.PI);
            double tant = Math.Sqrt(dx * dx + dy * dy);
            double pix = 0.26;//pix_to_micron

            //Making a mask
            double A = 90;
            double B = 180;
            double C = 20;
            double D = 40;

            int mask_x = (int)(A + tant * B);//if tantheta=0.38, mask_x=30
            int mask_y = (int)(C);
            if (mask_x < mask_y) {
                mask_x = mask_y;
            }
            int mask_x2 = (int)(A + tant * B);//if tantheta=0.38, mask_x=30
            int mask_y2 = (int)(D);
            if (mask_x < mask_y) {
                mask_x = mask_y;
            }

            int counter = 0;
           
                    using (Mat big = Mat.Zeros(600, 600, MatType.CV_8UC1))
                    using (Mat imgMask = Mat.Zeros(big.Height, big.Width, MatType.CV_8UC1)) {

                        //make the size of mask
                        int ystart = big.Height / 2 - mask_y / 2;
                        int xstart = big.Width / 2 - mask_x / 2;
                        int ystart2 = big.Height / 2 - mask_y2 / 2;
                        int xstart2 = big.Width / 2 - mask_x2 / 2;

                        OpenCvSharp.CPlusPlus.Rect recMask1 = new OpenCvSharp.CPlusPlus.Rect(xstart, ystart, mask_x, mask_y);
                        Cv2.Rectangle(imgMask, recMask1, 255, -1);
                        Mat imgmask1 = imgMask.Clone();

                    /*    OpenCvSharp.CPlusPlus.Rect recMask2 = new OpenCvSharp.CPlusPlus.Rect(xstart2, ystart2, mask_x, mask_y);
                        Cv2.Rectangle(imgMask, recMask2, 255, -1);
                        Mat imgmask2 = imgMask.Clone();*/


                        //Mat affineMask = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(big.Width / 2, big.Height / 2), -angle, 1);
                        //Cv2.WarpAffine(imgMask, imgMask, affineMask, imgMask.Size());


                        for (int p = 0; p < mats.Count; p++) {
                            int startx = big.Width / 2 - mats[p].Width / 2 ;
                            int starty = big.Height / 2 - mats[p].Height / 2 ;
                            //int startx = big.Width / 2 - mats[p].Width / 2 + (int)(p * ax * shiftpitch / 10.0);
                            // int starty = big.Height / 2 - mats[p].Height / 2 + (int)(p * ay * shiftpitch / 10.0);
                            Cv2.Add(
                                big[starty, starty + mats[p].Height, startx, startx + mats[p].Width],
                                mats[p],
                                big[starty, starty + mats[p].Height, startx, startx + mats[p].Width]);
                        }



                            Mat big_c = big.Clone();// {
                            Mat big1 = big*40;
                            Cv2.ImShow("superimpose", big * 40);
                            big1.ImWrite(String.Format(@"c:\MKS_test\test\Nagara\naka20\superimpose.png"));

                            Mat affineMask = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(big.Width / 2, big.Height / 2), -angle, 1);
                            Cv2.WarpAffine(big, big, affineMask, big.Size());
                            Cv2.ImShow("superimpose_roi", big * 40);
                            Mat big2 = big*40;
                            big2.ImWrite(String.Format(@"c:\MKS_test\test\Nagara\naka20\superimposeroi.png"));

                            Cv2.BitwiseAnd(big, imgMask, big);
                            Cv2.ImShow("imgcut", big * 40);
                            Mat big3 = big*40;
                            big3.ImWrite(String.Format(@"c:\MKS_test\test\Nagara\naka20\imgcut.png"));
                           
                            Cv2.Threshold(big, big, phthresh, 255, ThresholdType.ToZero);                          
                            Cv2.ImShow("bin", big * 40);
                            Mat big4 = big * 40;
                            big4.ImWrite(String.Format(@"c:\MKS_test\test\Nagara\naka20\bin.png"));

                            Mat affineMask2 = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(big.Width / 2, big.Height / 2), angle, 1);
                            Cv2.WarpAffine(big, big, affineMask2, big.Size());
                             
                            Cv2.WaitKey(0);


                           
                          //  if (debugflag == true) {//
                               
                           // }
                      //  }//using big_c

                        using (CvMemStorage storage = new CvMemStorage())
                        using (CvContourScanner scanner = new CvContourScanner(big.ToIplImage(), storage, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxSimple)) {
                            foreach (CvSeq<CvPoint> c in scanner) {
                                CvMoments mom = new CvMoments(c, false);
                                if (c.ElemSize < 2) continue;
                                if (mom.M00 < 1.0) continue;
                                double mx = mom.M10 / mom.M00;
                                double my = mom.M01 / mom.M00;
                                rawmicrotrack rm = new rawmicrotrack();
                                rm.ax = 0;
                                rm.ay = 0;
                                rm.cx = (int)(mx - big.Width / 2);
                                rm.cy = (int)(my - big.Height / 2);
                                rm.pv = (int)(mom.M00);
                                rms.Add(rm);
                                //Console.WriteLine(string.Format("{0}   {1} {2}   {3} {4}", rm.pv, ax, ay, rm.cx, rm.cy ));
                            }
                        }//using contour

                        //big_c.Dispose();

                        counter++;


                    }//using Mat



            OpenCvSharp.CPlusPlus.Point trackpos = new OpenCvSharp.CPlusPlus.Point(0, 0);
            if (rms.Count > 0) {
                rawmicrotrack rm = new rawmicrotrack();
                double meancx = 0;
                double meancy = 0;
                double meanax = 0;
                double meanay = 0;
                double meanph = 0;
                double meanpv = 0;
                double sumpv = 0;

                for (int i = 0; i < rms.Count; i++) {
                    meanpv += rms[i].pv * rms[i].pv;
                    meancx += rms[i].cx * rms[i].pv;
                    meancy += rms[i].cy * rms[i].pv;
                    meanax += rms[i].ax * rms[i].pv;
                    meanay += rms[i].ay * rms[i].pv;
                    sumpv += rms[i].pv;
                }

                meancx /= sumpv;//重心と傾きを輝度値で重み付き平均
                meancy /= sumpv;
                meanax /= sumpv;
                meanay /= sumpv;
                meanpv /= sumpv;

                trackpos = new OpenCvSharp.CPlusPlus.Point(
                    (int)(meancx) + 256 ,
                    (int)(meancy) + 220
                    );

               // double anglex = (meanax * shiftpitch * 0.267) / (3.0 * 7.0 * 2.2);
               // double angley = (meanay * shiftpitch * 0.267) / (3.0 * 7.0 * 2.2);
              //  Console.WriteLine(string.Format("{0:f4} {1:f4}", anglex, angley));
            } else {
                trackpos = new OpenCvSharp.CPlusPlus.Point(-1, -1);
            }


            return trackpos;
        }//track detection

        static OpenCvSharp.CPlusPlus.Point TrackDetection_naka_(List<Mat> mats, int px, int py, int shiftx = 2, int shifty = 2, int shiftpitch = 4, int windowsize = 40, int phthresh = 5, double dx = 1.2, double dy = 1.2, bool debugflag = false) {
            int x0 = px - 256;
            int y0 = py - 220;

            List<rawmicrotrack> rms = new List<rawmicrotrack>();

            // Point2d pixel_cen = TrackDetection(binimages, 256, 220, 3, 3, 4, 90, 3);

            double phi = Math.Atan2(dy, dx);
            double angle = phi * (180 / Math.PI);
            double tant = Math.Sqrt(dx * dx + dy * dy);
            double pix = 0.26;//pix_to_micron

            //Making a mask
            double A = 90;
            double B = 180;
            double C = 20;
            double D = 90;

            int mask_x = (int)(A + tant * B);//if tantheta=0.38, mask_x=30
            int mask_y = (int)(C);
            if (mask_x < mask_y) {
                mask_x = mask_y;
            }
            int mask_x2 = (int)(A + tant * B);//if tantheta=0.38, mask_x=30
            int mask_y2 = (int)(D);
            if (mask_x2 < mask_y2) {
                mask_x2 = mask_y2;
            }

            int counter = 0;
           
                    Mat big = Mat.Zeros(600, 600, MatType.CV_8UC1);
                    Mat imgMask = Mat.Zeros(big.Height, big.Width, MatType.CV_8UC1);
                    Mat imgMask1 = imgMask.Clone();
                    Mat imgMask2 = imgMask.Clone();
                        //make the size of mask
                    int ystart = big.Height / 2 - mask_y / 2;
                    int xstart = big.Width / 2 - mask_x / 2;
                    int ystart2 = big.Height / 2 - mask_y2 / 2;
                    int xstart2 = big.Width / 2 - mask_x2 / 2;

                    OpenCvSharp.CPlusPlus.Rect recMask1 = new OpenCvSharp.CPlusPlus.Rect(xstart, ystart, mask_x, mask_y);
                    Cv2.Rectangle(imgMask1, recMask1, 255, -1);
                    

                    OpenCvSharp.CPlusPlus.Rect recMask2 = new OpenCvSharp.CPlusPlus.Rect(xstart2, ystart2, mask_x2, mask_y2);
                    Cv2.Rectangle(imgMask2, recMask2, 255, -1);
                    


                     
                    for (int p = 0; p < mats.Count; p++) {
                        int startx = big.Width / 2 - mats[p].Width / 2 ;
                        int starty = big.Height / 2 - mats[p].Height / 2 ;
                        //int startx = big.Width / 2 - mats[p].Width / 2 + (int)(p * ax * shiftpitch / 10.0);
                        // int starty = big.Height / 2 - mats[p].Height / 2 + (int)(p * ay * shiftpitch / 10.0);
                        Cv2.Add(
                            big[starty, starty + mats[p].Height, startx, startx + mats[p].Width],
                            mats[p],
                            big[starty, starty + mats[p].Height, startx, startx + mats[p].Width]);
                    }



                                                     
                     Cv2.ImShow("superimpose", big * 40);
                           
             // Rotate image
                     Mat affineMask = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(big.Width / 2, big.Height / 2), -angle, 1);
                     Cv2.WarpAffine(big, big, affineMask, big.Size());
                     Mat big_ = big.Clone();
                     Cv2.ImShow("superimpose_roi", big * 40);
                     Cv2.ImShow("big_", big_ * 40);
                   
              // Cut image with mask          
                     Cv2.BitwiseAnd(big, imgMask1, big);
                     Cv2.ImShow("imgcut", big * 40);
                     Mat big_L1 = big.Clone();
                    
              // binalized
                     Cv2.Threshold(big_L1, big_L1, phthresh, 255, ThresholdType.ToZero);                          
                     Cv2.ImShow("bin", big_L1 * 40);
                           

                     Mat affineMask2 = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(big.Width / 2, big.Height / 2), angle, 1);
                     Cv2.WarpAffine(big_L1, big_L1, affineMask2, big_L1.Size());

                     
             /*...............
                     Cv2.BitwiseAnd(big_, imgMask2, big_);
                     Cv2.ImShow("imgcut_", big_ * 40);

                     Cv2.WarpAffine(big_, big_, affineMask2, big_.Size());

                     Cv2.Threshold(big_, big_, phthresh, 255, ThresholdType.ToZero);
                     Cv2.ImShow("bin", big_ * 40);*/




                     Cv2.WaitKey(0);

                     using ( CvMemStorage storage = new CvMemStorage())
                     using (CvContourScanner scanner = new CvContourScanner(big_L1.ToIplImage(), storage, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxSimple)) {


                         if (scanner.Count() != 0) {
                             foreach (CvSeq<CvPoint> c in scanner) {
                                 CvMoments mom = new CvMoments(c, false);
                                 if (c.ElemSize < 2) continue;
                                 if (mom.M00 < 1.0) continue;
                                 double mx = mom.M10 / mom.M00;
                                 double my = mom.M01 / mom.M00;
                                 rawmicrotrack rm = new rawmicrotrack();
                                 rm.ax = 0;
                                 rm.ay = 0;
                                 rm.cx = (int)(mx - big_L1.Width / 2);
                                 rm.cy = (int)(my - big_L1.Height / 2);
                                 rm.pv = (int)(mom.M00);
                                 rms.Add(rm);

                             }
                         } else {

                             Cv2.BitwiseAnd(big_, imgMask2, big_);
                             Cv2.ImShow("imgcut_", big_ * 40);
                             Mat big_L2 = big_.Clone();

                             Cv2.WarpAffine(big_L2, big_L2, affineMask2, big_L2.Size());

                             Cv2.Threshold(big_L2, big_L2, phthresh, 255, ThresholdType.ToZero);
                             Cv2.ImShow("bin_", big_L2 * 40);

                             Cv2.WaitKey(0);

                            using ( CvMemStorage storage2 = new CvMemStorage())
                            using (CvContourScanner scanner2 = new CvContourScanner(big_L2.ToIplImage(), storage2, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxSimple)) {
                                if (scanner2.Count() != 0) {
                                    foreach (CvSeq<CvPoint> c2 in scanner2) {
                                        CvMoments mom2 = new CvMoments(c2, false);
                                        if (c2.ElemSize < 2) continue;
                                        if (mom2.M00 < 1.0) continue;
                                        double mx = mom2.M10 / mom2.M00;
                                        double my = mom2.M01 / mom2.M00;
                                        rawmicrotrack rm = new rawmicrotrack();
                                        rm.ax = 0;
                                        rm.ay = 0;
                                        rm.cx = (int)(mx - big_L2.Width / 2);
                                        rm.cy = (int)(my - big_L2.Height / 2);
                                        rm.pv = (int)(mom2.M00);
                                        rms.Add(rm);
                                        //Console.WriteLine(string.Format("{0}   {1} {2}   {3} {4}", rm.pv, ax, ay, rm.cx, rm.cy ));
                                    }
                                } else {

                                    return new OpenCvSharp.CPlusPlus.Point(-1, -1);
                                }
                            }
                         }
                     }
                             //Console.WriteLine(string.Format("{0}   {1} {2}   {3} {4}", rm.pv, ax, ay, rm.cx, rm.cy ));
                         
                        

                      

                


            OpenCvSharp.CPlusPlus.Point trackpos = new OpenCvSharp.CPlusPlus.Point(0, 0);
            if (rms.Count > 0) {
                rawmicrotrack rm = new rawmicrotrack();
                double meancx = 0;
                double meancy = 0;
                double meanax = 0;
                double meanay = 0;
                double meanph = 0;
                double meanpv = 0;
                double sumpv = 0;

                for (int i = 0; i < rms.Count; i++) {
                    meanpv += rms[i].pv * rms[i].pv;
                    meancx += rms[i].cx * rms[i].pv;
                    meancy += rms[i].cy * rms[i].pv;
                    meanax += rms[i].ax * rms[i].pv;
                    meanay += rms[i].ay * rms[i].pv;
                    sumpv += rms[i].pv;
                }

                meancx /= sumpv;//重心と傾きを輝度値で重み付き平均
                meancy /= sumpv;
                meanax /= sumpv;
                meanay /= sumpv;
                meanpv /= sumpv;

                trackpos = new OpenCvSharp.CPlusPlus.Point(
                    (int)(meancx) + 256 ,
                    (int)(meancy) + 220
                    );

               // double anglex = (meanax * shiftpitch * 0.267) / (3.0 * 7.0 * 2.2);
               // double angley = (meanay * shiftpitch * 0.267) / (3.0 * 7.0 * 2.2);
              //  Console.WriteLine(string.Format("{0:f4} {1:f4}", anglex, angley));
            } else {
                trackpos = new OpenCvSharp.CPlusPlus.Point(-1, -1);
            }


            return trackpos;
        }//track detection

 static Mat GetMask(Mat img, int height, int width)
        {
            Mat mask = Mat.Zeros(img.Height, img.Width, MatType.CV_8UC1);
            int x0 = mask.Width / 2 - width / 2;
            int y0 = mask.Height / 2 - height / 2;
            OpenCvSharp.CPlusPlus.Rect rect = new OpenCvSharp.CPlusPlus.Rect(x0, y0, width, height);
            Cv2.Rectangle(mask, rect, 255, -1);
            return mask;
        }

        static OpenCvSharp.CPlusPlus.Point TrackDetection_naka_2(List<Mat> mats, int px, int py, int shiftx = 2, int shifty = 2, int shiftpitch = 4, int windowsizeH = 40, int phthresh = 5, double dx = 1.2, double dy = 1.2, bool debugflag = false) 
            
            
        {
            Mat img = Mat.Zeros(600, 600, MatType.CV_8UC1);
            Mat big = img.Clone();
            for (int p = 0; p < mats.Count; p++) {
                int startx = img.Width / 2 - mats[p].Width / 2;
                int starty = img.Height / 2 - mats[p].Height / 2;
                Cv2.Add(
                    img[starty, starty + mats[p].Height, startx, startx + mats[p].Width],
                    mats[p],
                    img[starty, starty + mats[p].Height, startx, startx + mats[p].Width]);
            }

            bool debug = true;
            //rotationparam
            double phi = Math.Atan2(dy, dx);
            double angle = phi * (180 / Math.PI);
            double tant = Math.Sqrt(dx * dx + dy * dy);
            double pix = 0.26;//pix_to_micron


            // Rotate image
            Mat affineMask = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(img.Width / 2, img.Height / 2), -angle, 1);
            Cv2.WarpAffine(img, img, affineMask, img.Size());

           
            //Making a mask
            int heightL1 = 15;
            int heightL2 = 30;

            int width = (int)(90 + tant * 180);
            width = (width > heightL1) ? width : heightL1;

            Mat maskL1 = GetMask(img, heightL1, width);
            Mat maskL2 = GetMask(img, heightL2, width);
            

            // Cut image with mask, thresholding for L1-area         
            Mat imgL1 = img.Clone();
            Cv2.BitwiseAnd(imgL1, maskL1, imgL1);
            if (debug) {
                Cv2.ImShow("imgL1", imgL1*40);
                Cv2.WaitKey(0);
            }
            
            Cv2.Threshold(imgL1, imgL1, phthresh, 255, ThresholdType.ToZero);

            //inverse rotation for L1-area
            Mat affineMask2 = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(imgL1.Width / 2, imgL1.Height / 2), angle, 1);
            Cv2.WarpAffine(imgL1, imgL1, affineMask2, imgL1.Size());

            if (debug)
            {
                Cv2.ImShow("imgL1", imgL1);
                Cv2.WaitKey(0);
            }

            //raw micro tracks
            List<rawmicrotrack> rms = new List<rawmicrotrack>();


            //contour detection for L1-area
            using (CvMemStorage storage = new CvMemStorage())
            using (CvContourScanner scanner = new CvContourScanner(imgL1.ToIplImage(), storage, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxSimple))
            {

                foreach (CvSeq<CvPoint> c in scanner)
                {
                    CvMoments mom = new CvMoments(c, false);
                    if (c.ElemSize < 2) continue;
                    if (mom.M00 < 1.0) continue;
                    double mx = mom.M10 / mom.M00;
                    double my = mom.M01 / mom.M00;
                    rawmicrotrack rm = new rawmicrotrack();
                    rm.ax = 0;
                    rm.ay = 0;
                    rm.cx = (int)(mx - imgL1.Width / 2);
                    rm.cy = (int)(my - imgL1.Height / 2);
                    rm.pv = (int)(mom.M00);
                    rms.Add(rm);
                }                        
            }


            //if nocandidate, spread ROI 
            // Cut image with mask, thresholding for L2-area
            Mat imgL2 = img.Clone();
            Cv2.BitwiseAnd(imgL2, maskL2, imgL2);
            if (debug) {
                Cv2.ImShow("cutL2", imgL2);
                Cv2.WaitKey(0);
            }
            Cv2.Threshold(imgL2, imgL2, phthresh, 255, ThresholdType.ToZero);
            if (debug)
            {
                Cv2.ImShow("imgL2", imgL2);
                Cv2.WaitKey(0);
            }

            //inverse rotation for L2-area
            Mat affineMaskL2 = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(imgL2.Width / 2, imgL2.Height / 2), angle, 1);
            Cv2.WarpAffine(imgL2, imgL2, affineMask2, imgL2.Size());


            //contour detection for L2-area
            //if nocandidate, not found 
            using (CvMemStorage storageL2 = new CvMemStorage())
            using (CvContourScanner scannerL2 = new CvContourScanner(imgL2.ToIplImage(), storageL2, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxSimple))
            {

                if (scannerL2.Count() != 0)
                {
                    foreach (CvSeq<CvPoint> c in scannerL2)
                    {
                        CvMoments mom = new CvMoments(c, false);
                        if (c.ElemSize < 2) continue;
                        if (mom.M00 < 1.0) continue;
                        double mx = mom.M10 / mom.M00;
                        double my = mom.M01 / mom.M00;
                        rawmicrotrack rm = new rawmicrotrack();
                        rm.ax = 0;
                        rm.ay = 0;
                        rm.cx = (int)(mx - imgL2.Width / 2);
                        rm.cy = (int)(my - imgL2.Height / 2);
                        rm.pv = (int)(mom.M00);
                        rms.Add(rm);
                    }
                }
                else
                {
                    //not detected
                    return new OpenCvSharp.CPlusPlus.Point(-1, -1);
                }

            }//using L2 contour 


            //calc average of raw-microtracks
            OpenCvSharp.CPlusPlus.Point trackpos = new OpenCvSharp.CPlusPlus.Point(0, 0);
            if (rms.Count > 0)
            {
                rawmicrotrack rm = new rawmicrotrack();
                double meancx = 0;
                double meancy = 0;
                double meanax = 0;
                double meanay = 0;
                double meanph = 0;
                double meanpv = 0;
                double sumpv = 0;

                for (int i = 0; i < rms.Count; i++)
                {
                    meanpv += rms[i].pv * rms[i].pv;
                    meancx += rms[i].cx * rms[i].pv;
                    meancy += rms[i].cy * rms[i].pv;
                    meanax += rms[i].ax * rms[i].pv;
                    meanay += rms[i].ay * rms[i].pv;
                    sumpv += rms[i].pv;
                }

                meancx /= sumpv;//weighted average
                meancy /= sumpv;
                meanax /= sumpv;
                meanay /= sumpv;
                meanph /= sumpv;
                meanpv /= sumpv;

                trackpos = new OpenCvSharp.CPlusPlus.Point(
                    (int)(meancx) + 256,
                    (int)(meancy) + 220
                    );
                // double anglex = (meanax * shiftpitch * 0.267) / (3.0 * 7.0 * 2.2);
                // double angley = (meanay * shiftpitch * 0.267) / (3.0 * 7.0 * 2.2);
                //  Console.WriteLine(string.Format("{0:f4} {1:f4}", anglex, angley));
                return trackpos;
            }
            else
            {
                return trackpos;
            }
        }

        static OpenCvSharp.CPlusPlus.Point TD_naka_1(List<Mat> mats, int px, int py, int shiftx = 2, int shifty = 2, int shiftpitch = 4, int windowsizeH = 40, int phthresh = 5, double dx = 1.2, double dy = 1.2, int j=1,  bool debugflag = false)
        {
            int x0 = px - 256;
            int y0 = py - 220;

            List<rawmicrotrack> rms = new List<rawmicrotrack>();

            // Point2d pixel_cen = TrackDetection(binimages, 256, 220, 3, 3, 4, 90, 3);

            double phi = Math.Atan2(dy, dx);
            double angle = phi * (180 / Math.PI);
            double tant = Math.Sqrt(dx * dx + dy * dy);
            double pix = 0.26;//pix_to_micron

            //Making a mask
            //double A = 90;ver ソウ
            double A = 20;//ver GTR
            //double B = 180;ver ソウ
            double B = 155;//ver GTR
            double C = windowsizeH;
          

            int mask_x = (int)(A + tant * B);//if tantheta=0.38, mask_x=30
            //int mask_y = (int)(C + tant * (2*C));//+ tant * (80));ver ソウ
            int mask_y = (int)C;//ver GTR
            if (mask_x < mask_y) {
                mask_x = mask_y;
            }
        
     
                    using (Mat big = Mat.Zeros(600, 600, MatType.CV_8UC1))
                    using (Mat imgMask = Mat.Zeros(big.Height, big.Width, MatType.CV_8UC1)) {

                        //make the size of mask
                        int ystart = big.Height / 2 - mask_y / 2;
                        int xstart = big.Width / 2 - mask_x / 2;
                        

                        OpenCvSharp.CPlusPlus.Rect recMask1 = new OpenCvSharp.CPlusPlus.Rect(xstart, ystart, mask_x, mask_y);
                        Cv2.Rectangle(imgMask, recMask1, 255, -1);

                        for (int p = 0; p < mats.Count; p++) {
                            int startx = big.Width / 2 - mats[p].Width / 2 ;
                            int starty = big.Height / 2 - mats[p].Height / 2 ;
                            //int startx = big.Width / 2 - mats[p].Width / 2 + (int)(p * ax * shiftpitch / 10.0);
                            // int starty = big.Height / 2 - mats[p].Height / 2 + (int)(p * ay * shiftpitch / 10.0);
                            Cv2.Add(
                                big[starty, starty + mats[p].Height, startx, startx + mats[p].Width],
                                mats[p],
                                big[starty, starty + mats[p].Height, startx, startx + mats[p].Width]);
                        }                           
                        
                            Cv2.ImShow("superimpose", big * 40);
                          
                          // big_.ImWrite(String.Format(@"c:\MKS_test\test\Nagara\g\superimpose_{0}-{1}-{2}.png", counter, ax, ay));
                            // Rotation
                            Mat affineMask = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(big.Width / 2, big.Height / 2), -angle, 1);
                            Cv2.WarpAffine(big, big, affineMask, big.Size());
                          
                            // image cut
                            Cv2.BitwiseAnd(big, imgMask, big);
                            Cv2.ImShow("imgcut", big * 40);
                            Mat big_ = big * 40;
                            big_.ImWrite(String.Format(@"c:\MKS_test\test\Nagara\pl6\superimpose_{0}.png", j));
                            // threshold
                            Cv2.Threshold(big, big, phthresh, 255, ThresholdType.ToZero);                          
                            Cv2.ImShow("bin", big * 40);
                            Mat big4 = big * 40;
                           
                            //  Inverse rotation
                            Mat affineMask2 = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(big.Width / 2, big.Height / 2), angle, 1);
                            Cv2.WarpAffine(big, big, affineMask2, big.Size());
                             
                            Cv2.WaitKey(0);


                          //  if (debugflag == true) {//
                               
                           // }
                      //  }//using big_c

                        using (CvMemStorage storage = new CvMemStorage())
                        using (CvContourScanner scanner = new CvContourScanner(big.ToIplImage(), storage, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxSimple)) {
                            foreach (CvSeq<CvPoint> c in scanner) {
                                CvMoments mom = new CvMoments(c, false);
                                if (c.ElemSize < 2) continue;
                                if (mom.M00 < 1.0) continue;//
                                double mx = mom.M10 / mom.M00;
                                double my = mom.M01 / mom.M00;
                                if ( Math.Abs(my- big.Height / 2) > Math.Abs(C/2)) continue;
                                rawmicrotrack rm = new rawmicrotrack();
                                rm.ax = 0;
                                rm.ay = 0;
                                rm.cx = (int)(mx - big.Width / 2);
                                rm.cy = (int)(my - big.Height / 2);
                                rm.pv = (int)(mom.M00);
                                rms.Add(rm);
                                
                            }
                        }//using contour


                    }//using Mat



            OpenCvSharp.CPlusPlus.Point trackpos = new OpenCvSharp.CPlusPlus.Point(0, 0);
            if (rms.Count > 0) {
                rawmicrotrack rm = new rawmicrotrack();
                double meancx = 0;
                double meancy = 0;
                double meanax = 0;
                double meanay = 0;
                double meanph = 0;
                double meanpv = 0;
                double sumpv = 0;

                for (int i = 0; i < rms.Count; i++) {
                    meanpv += rms[i].pv * rms[i].pv;
                    meancx += rms[i].cx * rms[i].pv;
                    meancy += rms[i].cy * rms[i].pv;
                    meanax += rms[i].ax * rms[i].pv;
                    meanay += rms[i].ay * rms[i].pv;
                    sumpv += rms[i].pv;
                }

                meancx /= sumpv;//重心と傾きを輝度値で重み付き平均
                meancy /= sumpv;
                meanax /= sumpv;
                meanay /= sumpv;
                meanpv /= sumpv;

                trackpos = new OpenCvSharp.CPlusPlus.Point(
                    (int)(meancx) + 256 ,
                    (int)(meancy) + 220
                    );

               // double anglex = (meanax * shiftpitch * 0.267) / (3.0 * 7.0 * 2.2);
               // double angley = (meanay * shiftpitch * 0.267) / (3.0 * 7.0 * 2.2);
              //  Console.WriteLine(string.Format("{0:f4} {1:f4}", anglex, angley));
            } else {
                trackpos = new OpenCvSharp.CPlusPlus.Point(-1, -1);
            }


            return trackpos;
        }

        static OpenCvSharp.CPlusPlus.Point TD_naka_2(List<Mat> mats, int px, int py, int shiftx = 2, int shifty = 2, int shiftpitch = 4, int windowsizeH = 40, int phthresh = 5, double dx = 1.2, double dy = 1.2, bool debugflag = false) {
            int x0 = px - 256;
            int y0 = py - 220;

            List<rawmicrotrack> rms = new List<rawmicrotrack>();

            // Point2d pixel_cen = TrackDetection(binimages, 256, 220, 3, 3, 4, 90, 3);

            double phi = Math.Atan2(dy, dx);
            double angle = phi * (180 / Math.PI);
            double tant = Math.Sqrt(dx * dx + dy * dy);
            double pix = 0.26;//pix_to_micron

            //Making a mask
            //double A = 90;ver ソウ
            double A = 20;//ver GTR
            //double B = 180;ver ソウ
            double B = 155;//ver GTR
            double C = windowsizeH;


            int mask_x = (int)(A + tant * B);//if tantheta=0.38, mask_x=30
            //int mask_y = (int)(C + tant * (2*C));//+ tant * (80));ver ソウ
            int mask_y = (int)C * 2;//ver GTR
            if (mask_x < mask_y) {
                mask_x = mask_y;
            }


            using (Mat big = Mat.Zeros(600, 600, MatType.CV_8UC1))
            using (Mat imgMask = Mat.Zeros(big.Height, big.Width, MatType.CV_8UC1)) {

                //make the size of mask
                int ystart = big.Height / 2 - mask_y / 2;
                int xstart = big.Width / 2 - mask_x / 2;


                OpenCvSharp.CPlusPlus.Rect recMask1 = new OpenCvSharp.CPlusPlus.Rect(xstart, ystart, mask_x, mask_y);
                Cv2.Rectangle(imgMask, recMask1, 255, -1);

                for (int p = 0; p < mats.Count; p++) {
                    int startx = big.Width / 2 - mats[p].Width / 2;
                    int starty = big.Height / 2 - mats[p].Height / 2;
                    //int startx = big.Width / 2 - mats[p].Width / 2 + (int)(p * ax * shiftpitch / 10.0);
                    // int starty = big.Height / 2 - mats[p].Height / 2 + (int)(p * ay * shiftpitch / 10.0);
                    Cv2.Add(
                        big[starty, starty + mats[p].Height, startx, startx + mats[p].Width],
                        mats[p],
                        big[starty, starty + mats[p].Height, startx, startx + mats[p].Width]);
                }

                Cv2.ImShow("superimpose", big * 40);

                // Rotation
                Mat affineMask = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(big.Width / 2, big.Height / 2), -angle, 1);
                Cv2.WarpAffine(big, big, affineMask, big.Size());

                // image cut
                Cv2.BitwiseAnd(big, imgMask, big);
                Cv2.ImShow("imgcut", big * 40);

                // threshold
                Cv2.Threshold(big, big, phthresh, 255, ThresholdType.ToZero);
                Cv2.ImShow("bin", big * 40);
                Mat big4 = big * 40;

                //  Inverse rotation
                Mat affineMask2 = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(big.Width / 2, big.Height / 2), angle, 1);
                Cv2.WarpAffine(big, big, affineMask2, big.Size());

                Cv2.WaitKey(0);



                //  if (debugflag == true) {//

                // }
                //  }//using big_c

                using (CvMemStorage storage = new CvMemStorage())
                using (CvContourScanner scanner = new CvContourScanner(big.ToIplImage(), storage, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxSimple)) {
                    foreach (CvSeq<CvPoint> c in scanner) {
                        CvMoments mom = new CvMoments(c, false);
                        if (c.ElemSize < 2) continue;//
                        if (mom.M00 < 1.0) continue;//
                        double mx = mom.M10 / mom.M00;
                        double my = mom.M01 / mom.M00;
                       // if (Math.Abs(my - big.Height / 2) > Math.Abs(C/2 - 3)) continue;
                        rawmicrotrack rm = new rawmicrotrack();
                        rm.ax = 0;
                        rm.ay = 0;
                        rm.cx = (int)(mx - big.Width / 2);
                        rm.cy = (int)(my - big.Height / 2);
                        rm.pv = (int)(mom.M00);
                        rms.Add(rm);

                    }
                }//using contour


            }//using Mat



            OpenCvSharp.CPlusPlus.Point trackpos = new OpenCvSharp.CPlusPlus.Point(0, 0);
            if (rms.Count > 0) {
                rawmicrotrack rm = new rawmicrotrack();
                double meancx = 0;
                double meancy = 0;
                double meanax = 0;
                double meanay = 0;
                double meanph = 0;
                double meanpv = 0;
                double sumpv = 0;

                for (int i = 0; i < rms.Count; i++) {
                    meanpv += rms[i].pv * rms[i].pv;
                    meancx += rms[i].cx * rms[i].pv;
                    meancy += rms[i].cy * rms[i].pv;
                    meanax += rms[i].ax * rms[i].pv;
                    meanay += rms[i].ay * rms[i].pv;
                    sumpv += rms[i].pv;
                }

                meancx /= sumpv;//重心と傾きを輝度値で重み付き平均
                meancy /= sumpv;
                meanax /= sumpv;
                meanay /= sumpv;
                meanpv /= sumpv;

                trackpos = new OpenCvSharp.CPlusPlus.Point(
                    (int)(meancx) + 256,
                    (int)(meancy) + 220
                    );

                // double anglex = (meanax * shiftpitch * 0.267) / (3.0 * 7.0 * 2.2);
                // double angley = (meanay * shiftpitch * 0.267) / (3.0 * 7.0 * 2.2);
                //  Console.WriteLine(string.Format("{0:f4} {1:f4}", anglex, angley));
            } else {
                trackpos = new OpenCvSharp.CPlusPlus.Point(-1, -1);
            }


            return trackpos;
        }


        static OpenCvSharp.CPlusPlus.Point TD_naka_3(List<Mat> mats, int px, int py, int shiftx = 2, int shifty = 2, int shiftpitch = 4, int windowsize = 40, int phthresh = 5, double dx = 1.2, double dy = 1.2, bool debugflag = false) {
            int x0 = px - 256;
            int y0 = py - 220;

            List<rawmicrotrack> rms = new List<rawmicrotrack>();

            // Point2d pixel_cen = TrackDetection(binimages, 256, 220, 3, 3, 4, 90, 3);

            double phi = Math.Atan2(dy, dx);
            double angle = phi * (180 / Math.PI);
            double tant = Math.Sqrt(dx * dx + dy * dy);
            double pix = 0.26;//pix_to_micron

            //Making a mask
            double A = 90;
            double B = 180;
            double C = 40;


            int mask_x = (int)(A + tant * B);//if tantheta=0.38, mask_x=30
            int mask_y = (int)(C);
            if (mask_x < mask_y) {
                mask_x = mask_y;
            }


            using (Mat big = Mat.Zeros(600, 600, MatType.CV_8UC1))
            using (Mat imgMask = Mat.Zeros(big.Height, big.Width, MatType.CV_8UC1)) {

                //make the size of mask
                int ystart = big.Height / 2 - mask_y;
                int xstart = big.Width / 2 - mask_x / 2;


                OpenCvSharp.CPlusPlus.Rect recMask1 = new OpenCvSharp.CPlusPlus.Rect(xstart, ystart, mask_x, mask_y);
                Cv2.Rectangle(imgMask, recMask1, 255, -1);

                for (int p = 0; p < mats.Count; p++) {
                    int startx = big.Width / 2 - mats[p].Width / 2;
                    int starty = big.Height / 2 - mats[p].Height / 2;
                    //int startx = big.Width / 2 - mats[p].Width / 2 + (int)(p * ax * shiftpitch / 10.0);
                    // int starty = big.Height / 2 - mats[p].Height / 2 + (int)(p * ay * shiftpitch / 10.0);
                    Cv2.Add(
                        big[starty, starty + mats[p].Height, startx, startx + mats[p].Width],
                        mats[p],
                        big[starty, starty + mats[p].Height, startx, startx + mats[p].Width]);
                }

                Cv2.ImShow("superimpose", big * 40);
              
                // Rotation
                Mat affineMask = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(big.Width / 2, big.Height / 2), -angle, 1);
                Cv2.WarpAffine(big, big, affineMask, big.Size());

                // image cut
                Cv2.BitwiseAnd(big, imgMask, big);
                Cv2.ImShow("imgcut", big * 40);

                // threshold
                Cv2.Threshold(big, big, phthresh, 255, ThresholdType.ToZero);
                Cv2.ImShow("bin", big * 40);
                Mat big4 = big * 40;

                //  Inverse rotation
                Mat affineMask2 = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(big.Width / 2, big.Height / 2), angle, 1);
                Cv2.WarpAffine(big, big, affineMask2, big.Size());

                Cv2.WaitKey(0);



                //  if (debugflag == true) {//

                // }
                //  }//using big_c

                using (CvMemStorage storage = new CvMemStorage())
                using (CvContourScanner scanner = new CvContourScanner(big.ToIplImage(), storage, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxSimple)) {
                    if (scanner.Count() != 0){
                    foreach (CvSeq<CvPoint> c in scanner) {
                       
                        CvMoments mom = new CvMoments(c, false);
                        if (c.ElemSize <5) continue;//
                        if (mom.M00 < 3.0) continue;//
                        double mx = mom.M10 / mom.M00;
                        double my = mom.M01 / mom.M00;
                        rawmicrotrack rm = new rawmicrotrack();
                        rm.ax = 0;
                        rm.ay = 0;
                        rm.cx = (int)(mx - big.Width / 2);
                        rm.cy = (int)(my - big.Height / 2);
                        rm.pv = (int)(mom.M00);
                        rms.Add(rm);
                       
                    }

                    } else {
                    
                        return new OpenCvSharp.CPlusPlus.Point(-1, -1);
                    }

                }//using contour


            }//using Mat



            OpenCvSharp.CPlusPlus.Point trackpos = new OpenCvSharp.CPlusPlus.Point(0, 0);
            if (rms.Count > 0) {
                rawmicrotrack rm = new rawmicrotrack();
                double meancx = 0;
                double meancy = 0;
                double meanax = 0;
                double meanay = 0;
                double meanph = 0;
                double meanpv = 0;
                double sumpv = 0;

                for (int i = 0; i < rms.Count; i++) {
                    meanpv += rms[i].pv * rms[i].pv;
                    meancx += rms[i].cx * rms[i].pv;
                    meancy += rms[i].cy * rms[i].pv;
                    meanax += rms[i].ax * rms[i].pv;
                    meanay += rms[i].ay * rms[i].pv;
                    sumpv += rms[i].pv;
                }

                meancx /= sumpv;//重心と傾きを輝度値で重み付き平均
                meancy /= sumpv;
                meanax /= sumpv;
                meanay /= sumpv;
                meanpv /= sumpv;

                trackpos = new OpenCvSharp.CPlusPlus.Point(
                    (int)(meancx) + 256,
                    (int)(meancy) + 220
                    );

                // double anglex = (meanax * shiftpitch * 0.267) / (3.0 * 7.0 * 2.2);
                // double angley = (meanay * shiftpitch * 0.267) / (3.0 * 7.0 * 2.2);
                //  Console.WriteLine(string.Format("{0:f4} {1:f4}", anglex, angley));
            } else {
                trackpos = new OpenCvSharp.CPlusPlus.Point(-1, -1);
            }


            return trackpos;
        }

        static OpenCvSharp.CPlusPlus.Point TD_naka_4(List<Mat> mats, int px, int py, int shiftx = 2, int shifty = 2, int shiftpitch = 4, int windowsize = 40, int phthresh = 5, double dx = 1.2, double dy = 1.2, bool debugflag = false) {
            int x0 = px - 256;
            int y0 = py - 220;

            List<rawmicrotrack> rms = new List<rawmicrotrack>();

            // Point2d pixel_cen = TrackDetection(binimages, 256, 220, 3, 3, 4, 90, 3);

            double phi = Math.Atan2(dy, dx);
            double angle = phi * (180 / Math.PI);
            double tant = Math.Sqrt(dx * dx + dy * dy);
            double pix = 0.26;//pix_to_micron

            //Making a mask
            double A = 90;
            double B = 180;
            double C = 40;


            int mask_x = (int)(A + tant * B);//if tantheta=0.38, mask_x=30
            int mask_y = (int)(C);
            if (mask_x < mask_y) {
                mask_x = mask_y;
            }


            using (Mat big = Mat.Zeros(600, 600, MatType.CV_8UC1))
            using (Mat imgMask = Mat.Zeros(big.Height, big.Width, MatType.CV_8UC1)) {

                //make the size of mask
                int ystart = big.Height / 2;
                int xstart = big.Width / 2 -mask_x/2;


                OpenCvSharp.CPlusPlus.Rect recMask1 = new OpenCvSharp.CPlusPlus.Rect(xstart, ystart, mask_x, mask_y);
                Cv2.Rectangle(imgMask, recMask1, 255, -1);

                for (int p = 0; p < mats.Count; p++) {
                    int startx = big.Width / 2 - mats[p].Width / 2;
                    int starty = big.Height / 2 - mats[p].Height / 2;
                    //int startx = big.Width / 2 - mats[p].Width / 2 + (int)(p * ax * shiftpitch / 10.0);
                    // int starty = big.Height / 2 - mats[p].Height / 2 + (int)(p * ay * shiftpitch / 10.0);
                    Cv2.Add(
                        big[starty, starty + mats[p].Height, startx, startx + mats[p].Width],
                        mats[p],
                        big[starty, starty + mats[p].Height, startx, startx + mats[p].Width]);
                }

                Cv2.ImShow("superimpose", big * 40);

                // Rotation
                Mat affineMask = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(big.Width / 2, big.Height / 2), -angle, 1);
                Cv2.WarpAffine(big, big, affineMask, big.Size());

                // image cut
                Cv2.BitwiseAnd(big, imgMask, big);
                Cv2.ImShow("imgcut", big * 40);

                // threshold
                Cv2.Threshold(big, big, phthresh, 255, ThresholdType.ToZero);
                Cv2.ImShow("bin", big * 40);
                Mat big4 = big * 40;

                //  Inverse rotation
                Mat affineMask2 = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(big.Width / 2, big.Height / 2), angle, 1);
                Cv2.WarpAffine(big, big, affineMask2, big.Size());

                Cv2.WaitKey(0);



                //  if (debugflag == true) {//

                // }
                //  }//using big_c

                using (CvMemStorage storage = new CvMemStorage())
                using (CvContourScanner scanner = new CvContourScanner(big.ToIplImage(), storage, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxSimple)) {
                    if (scanner.Count() != 0) {
                        foreach (CvSeq<CvPoint> c in scanner) {

                            CvMoments mom = new CvMoments(c, false);
                            if (c.ElemSize < 5) continue;
                            if (mom.M00 < 1.0) continue;
                            double mx = mom.M10 / mom.M00;
                            double my = mom.M01 / mom.M00;
                            rawmicrotrack rm = new rawmicrotrack();
                            rm.ax = 0;
                            rm.ay = 0;
                            rm.cx = (int)(mx - big.Width / 2);
                            rm.cy = (int)(my - big.Height / 2);
                            rm.pv = (int)(mom.M00);
                            rms.Add(rm);

                        }

                    } else {

                        return new OpenCvSharp.CPlusPlus.Point(-1, -1);
                    }

                }//using contour


            }//using Mat



            OpenCvSharp.CPlusPlus.Point trackpos = new OpenCvSharp.CPlusPlus.Point(0, 0);
            if (rms.Count > 0) {
                rawmicrotrack rm = new rawmicrotrack();
                double meancx = 0;
                double meancy = 0;
                double meanax = 0;
                double meanay = 0;
                double meanph = 0;
                double meanpv = 0;
                double sumpv = 0;

                for (int i = 0; i < rms.Count; i++) {
                    meanpv += rms[i].pv * rms[i].pv;
                    meancx += rms[i].cx * rms[i].pv;
                    meancy += rms[i].cy * rms[i].pv;
                    meanax += rms[i].ax * rms[i].pv;
                    meanay += rms[i].ay * rms[i].pv;
                    sumpv += rms[i].pv;
                }

                meancx /= sumpv;//重心と傾きを輝度値で重み付き平均
                meancy /= sumpv;
                meanax /= sumpv;
                meanay /= sumpv;
                meanpv /= sumpv;

                trackpos = new OpenCvSharp.CPlusPlus.Point(
                    (int)(meancx) + 256,
                    (int)(meancy) + 220
                    );

                // double anglex = (meanax * shiftpitch * 0.267) / (3.0 * 7.0 * 2.2);
                // double angley = (meanay * shiftpitch * 0.267) / (3.0 * 7.0 * 2.2);
                //  Console.WriteLine(string.Format("{0:f4} {1:f4}", anglex, angley));
            } else {
                trackpos = new OpenCvSharp.CPlusPlus.Point(-1, -1);
            }


            return trackpos;
        }




        static OpenCvSharp.CPlusPlus.Point TrackDetection(List<Mat> mats, int px, int py, int shiftx = 2, int shifty = 2, int shiftpitch = 4, int windowsize = 40, int phthresh = 5, double dx =1.2, double dy=1.2, bool debugflag = false) {
            int x0 = px - 256;
            int y0 = py - 220;

            List<rawmicrotrack> rms = new List<rawmicrotrack>();

            // Point2d pixel_cen = TrackDetection(binimages, 256, 220, 3, 3, 4, 90, 3);

            double phi = Math.Atan2(dy, dx);
            double angle = phi * (180 / Math.PI);
            double tant = Math.Sqrt(dx * dx + dy * dy);
            double pix = 0.26;//pix_to_micron

            //Making a mask
            double A = 90;
            double B =180;
            double C =90;
            double D = 1;

            int mask_x = (int)(A + tant * B);//if tantheta=0.38, mask_x=30
            int mask_y = (int)(C);
            if (mask_x < mask_y) {
                mask_x = mask_y;
            }
            
            int counter = 0;
            for (int ax = -shiftx; ax <= shiftx; ax++) {
                for (int ay = -shifty; ay <= shifty; ay++) {
                    using (Mat big = Mat.Zeros(600, 600, MatType.CV_8UC1))
                    using (Mat imgMask = Mat.Zeros(big.Height, big.Width, MatType.CV_8UC1)) {

                        //make the size of mask
                        int ystart = big.Height / 2  - mask_y / 2;
                        int yend = big.Height / 2 +  mask_y / 2;
                        int xstart = big.Width / 2  - mask_x / 2;
                        int xend = big.Width / 2 +  mask_x / 2;

                        OpenCvSharp.CPlusPlus.Rect recMask = new OpenCvSharp.CPlusPlus.Rect(xstart, ystart, mask_x, mask_y);
                        Cv2.Rectangle(imgMask, recMask, 255, -1);
                        Mat imgmask = imgMask.Clone();

                        Mat affineMask = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(big.Width / 2, big.Height / 2), 0, 1);
                        Cv2.WarpAffine(imgMask, imgMask, affineMask, imgMask.Size());
                        

                        for (int p = 0; p < mats.Count; p++) {
                            int startx = big.Width / 2 - mats[p].Width / 2 + (int)(p * ax * shiftpitch / 10.0);
                            int starty = big.Height / 2 - mats[p].Height / 2 + (int)(p * ay * shiftpitch / 10.0);
                            //int startx = big.Width / 2 - mats[p].Width / 2 + (int)(p * ax * shiftpitch / 10.0);
                           // int starty = big.Height / 2 - mats[p].Height / 2 + (int)(p * ay * shiftpitch / 10.0);
                            Cv2.Add(
                                big[starty, starty + mats[p].Height, startx, startx + mats[p].Width],
                                mats[p],
                                big[starty, starty + mats[p].Height, startx, startx + mats[p].Width]);
                        }

                        using (Mat big_c = big.Clone()) {
                            Mat big1 = big.Clone();
                            Cv2.ImShow("superimpose", big*40);
                            Mat big1_sup = big * 40;
                            big1_sup.ImWrite(String.Format(@"c:\MKS_test\test\Nagara\g\superimpose_{0}-{1}-{2}.png", counter, ax, ay));
                            Mat Big2 = big.Clone();

                            Cv2.BitwiseAnd(big, imgMask, big);
                            Cv2.ImShow("imgcut", big*40);

                            Mat affineMask2 = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(big.Width / 2, big.Height / 2), -angle, 1);
                            Cv2.WarpAffine(big1, big1, affineMask2, big1.Size());
                            Cv2.ImShow("RoiIMG", big1*40);
                            Mat Bigroi = big1*40;
                            Bigroi.ImWrite(String.Format(@"c:\MKS_test\test\Nagara\g\RoiImg_{0}-{1}-{2}.png", counter, ax, ay));

                            Cv2.BitwiseAnd(big1, imgmask, big1);
                            Cv2.ImShow("imgcut", big1 * 40);
                            Mat Bigimcut = big*40;
                            Bigimcut.ImWrite(String.Format(@"c:\MKS_test\test\Nagara\g\imgcut_{0}-{1}-{2}.png", counter, ax, ay));
                            

                            //Mat maskRoi = big.Clone();
                            //Mat Big1 = big.Clone();
                            Cv2.ImShow("Big2", Big2 * 40);
                            Cv2.Threshold(big, big, phthresh, 255, ThresholdType.ToZero);
                            Cv2.Threshold(big1, big1, phthresh, 255, ThresholdType.ToZero);
                            Mat Bigbin = big1 * 40;
                            Bigbin.ImWrite(String.Format(@"c:\MKS_test\test\Nagara\g\bin_{0}-{1}-{2}.png", counter, ax, ay));
                            //Cv2.BitwiseAnd(big, imgMask, big);

                            Cv2.ImShow("bin", big*40);
                           // Cv2.BitwiseAnd(big_c, imgMask, big_c);
                          //  Mat affineMask2 = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(0, 0),-angle, 1);
                         //   Cv2.WarpAffine(Big2, Big2, affineMask2, imgMask.Size());
                          //  Cv2.ImShow("big1", Big2*40);
                         //   Mat maskRoi = Big1.Clone();
                            //Cv2.BitwiseAnd(big_c, imgMask, big_c);
                          //  Mat big_ = big_c * 40;
                          //  Cv2.Threshold(big_c, big_c, phthresh, 255, ThresholdType.ToZero);
                          // 
                            Cv2.WaitKey(0);
                            
                             
                           // Mat maskcut_ = maskcut * 40;
                           // Mat Roi_ = maskRoi * 40;
                            if (debugflag == true) {//
                                //bigorg.ImWrite(String.Format(@"{0}_{1}_{2}.png",counter,ax,ay));
                                //Mat roiwrite = roi.Clone() * 30;
                                //roiwrite.ImWrite(String.Format(@"roi_{0}_{1}_{2}.png", counter, ax, ay));
                                //Cv2.Rectangle(big_c, recMask, 255, 1);//brightness=1, fill
                                //Cv2.ImShow("superimposed", big_c * 30);
                               // Cv2.ImShow("bigx30", maskRoi * 30);
                               // Cv2.ImShow("imgMask", Big1*40);
                               // Cv2.ImShow("bina", big * 30);
                               // Cv2.WaitKey(0);
                                
                               // Mat big1 = Big2 * 40;
                              //  Mat bin = big * 40;
                                //Mat maskroi = maskRoi * 40;
                               
                              //  Mat imgMask_ = imgMask * 40;
                               // big1.ImWrite(String.Format(@"c:\MKS_test\test\Nagara\a_1\superimpose_{0}-{1}-{2}.png",  counter, ax, ay));
                               // maskroi.ImWrite(String.Format(@"c:\MKS_test\test\Nagara\a_1\maskRoi_{0}-{1}-{2}.png", counter, ax, ay));
                              //  bin.ImWrite(String.Format(@"c:\MKS_test\test\Nagara\a\binaliz_{0}-{1}-{2}.png",  counter, ax, ay));
                              //  imgMask_.ImWrite(String.Format(@"c:\MKS_test\test\Nagara\a\Mask_{0}-{1}-{2}.png",  counter, ax, ay));
                            }
                        }//using big_c

                        using (CvMemStorage storage = new CvMemStorage())
                        using (CvContourScanner scanner = new CvContourScanner(big.ToIplImage(), storage, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxSimple)) {
                            foreach (CvSeq<CvPoint> c in scanner) {
                                CvMoments mom = new CvMoments(c, false);
                                if (c.ElemSize < 2) continue;
                                if (mom.M00 < 1.0) continue;
                                double mx = mom.M10 / mom.M00;
                                double my = mom.M01 / mom.M00;
                                rawmicrotrack rm = new rawmicrotrack();
                                rm.ax = ax;
                                rm.ay = ay;
                                rm.cx = (int)(mx - big.Width / 2);
                                rm.cy = (int)(my - big.Height / 2);
                                rm.pv = (int)(mom.M00);
                                rms.Add(rm);
                                //Console.WriteLine(string.Format("{0}   {1} {2}   {3} {4}", rm.pv, ax, ay, rm.cx, rm.cy ));
                            }
                        }//using contour

                        //big_c.Dispose();

                        counter++;


                    }//using Mat
                }//ay
            }//ax



            OpenCvSharp.CPlusPlus.Point trackpos = new OpenCvSharp.CPlusPlus.Point(0, 0);
            if (rms.Count > 0) {
                rawmicrotrack rm = new rawmicrotrack();
                double meancx = 0;
                double meancy = 0;
                double meanax = 0;
                double meanay = 0;
                double meanph = 0;
                double meanpv = 0;
                double sumpv = 0;

                for (int i = 0; i < rms.Count; i++) {
                    meanpv += rms[i].pv * rms[i].pv;
                    meancx += rms[i].cx * rms[i].pv;
                    meancy += rms[i].cy * rms[i].pv;
                    meanax += rms[i].ax * rms[i].pv;
                    meanay += rms[i].ay * rms[i].pv;
                    sumpv += rms[i].pv;
                }

                meancx /= sumpv;//重心と傾きを輝度値で重み付き平均
                meancy /= sumpv;
                meanax /= sumpv;
                meanay /= sumpv;
                meanpv /= sumpv;

                trackpos = new OpenCvSharp.CPlusPlus.Point(
                    (int)(meancx) + 256 - meanax * shiftpitch,
                    (int)(meancy) + 220 - meanay * shiftpitch
                    );

                double anglex = (meanax * shiftpitch * 0.267) / (3.0 * 7.0 * 2.2);
                double angley = (meanay * shiftpitch * 0.267) / (3.0 * 7.0 * 2.2);
                Console.WriteLine(string.Format("{0:f4} {1:f4}", anglex, angley));
            } else {
                trackpos = new OpenCvSharp.CPlusPlus.Point(-1, -1);
            }


            return trackpos;
        }//track detection


        static OpenCvSharp.CPlusPlus.Point TrackDetection_new(List<Mat> mats, int px, int py, int phthresh = 5, double dx = 0.12, double dy = 0.12, int mod = 5, int plt = 2, string sp0 = "", string sp1 = "", bool debugflag = true) {
            OpenCvSharp.CPlusPlus.Point center = new OpenCvSharp.CPlusPlus.Point(0, 0);
            int x0 = px - 256;
            int y0 = py - 220;

           // List<trackdata> rms = new List<trackdata>();


            double phi = Math.Atan2(dy, dx);
            double angle = phi * (180 / Math.PI);
            double tant = Math.Sqrt(dx * dx + dy * dy);
            double pix = 0.26;//pix_to_micron

            //Making a mask
            double A = 90;
            double B = 180;
            double C = 90;
            double D = 180;

            int mask_x = (int) (A + tant * B);//if tantheta=0.38, mask_x=30
            int mask_y = (int)(C + tant * D);
            if (mask_x < mask_y) {
                mask_x = mask_y;
            }

            int counter = 0;
            // Mat imgTemplt = Mat.Zeros(512, 440, MatType.CV_8UC1);
            //using (Mat big = Mat.Zeros(521, 440, MatType.CV_8UC1))
            using (Mat big = Mat.Zeros(600, 600, MatType.CV_8UC1))
            using (Mat imgMask = Mat.Zeros(big.Height, big.Width, MatType.CV_8UC1)) {


                int ystart = big.Height / 2 - mask_y / 2;
                int yend = big.Height / 2 + mask_y / 2;
                int xstart = big.Width / 2 - mask_x / 2;
                int xend = big.Width / 2 + mask_x / 2;
                OpenCvSharp.CPlusPlus.Rect recMask = new OpenCvSharp.CPlusPlus.Rect(xstart, ystart, mask_x, mask_y);
                Cv2.Rectangle(imgMask, recMask, 255, -1);//brightness=1, fill
                Mat imgmask = imgMask.Clone();
                Mat affineMask = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(big.Width / 2, big.Height / 2), angle, 1);
                Cv2.WarpAffine(imgmask, imgMask, affineMask, imgMask.Size());



                // Cv2.Rectangle(imgMask, recMask, 255, -1);//brightness=1, fill

                for (int p = 0; p < mats.Count; p++) {
                    int startx = big.Width / 2 - mats[p].Width / 2;
                    int starty = big.Height / 2 - mats[p].Height / 2;
                    Cv2.Add(
                        big[starty, starty + mats[p].Height, startx, startx + mats[p].Width],
                        mats[p],
                        big[starty, starty + mats[p].Height, startx, startx + mats[p].Width]);
                }

                using (Mat big_c = big.Clone()) {
                    Cv2.Threshold(big, big, phthresh, 255, ThresholdType.ToZero);
                    Cv2.BitwiseAnd(big, imgMask, big);
                    Mat maskcut = big.Clone();
                    // Mat affineMask2 = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(big.Width / 2, big.Height / 2), -angle, 1);
                    // Cv2.WarpAffine(big, big, affineMask2, imgMask.Size());
                    Mat maskRoi = big.Clone();


                    //  Cv2.BitwiseAnd(big, imgMask, big);

                }//using big_c

                //Making template
                double forcaldepth = 15;
                double E = 6;
                double e = 44;
                double F = 6;
                double f = 1;
                int templt_x = (int)(E + tant * e);//if tantheta=0.38, templt_x=22
                int templt_y = (int)(F + tant * f);
                if (templt_x < templt_y) {
                    templt_x = templt_y;
                }

                Mat imgTemplt = Mat.Zeros(200, 200, MatType.CV_8UC1);
                OpenCvSharp.CPlusPlus.Rect recTemplt = new OpenCvSharp.CPlusPlus.Rect(100, 100, templt_x, templt_y);
                Cv2.Rectangle(imgTemplt, recTemplt, 1, -1);//brightness=1, fill
                Mat affineTemplt = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(100, 100), angle, 1);
                Cv2.WarpAffine(imgTemplt, imgTemplt, affineTemplt, imgTemplt.Size(), Interpolation.Linear, BorderType.Constant, 0);


                Mat imgTempltTrim = new Mat();
                using (CvMemStorage storage = new CvMemStorage())
                using (CvContourScanner scanner = new CvContourScanner(imgTemplt.ToIplImage(), storage, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxSimple)) {
                    foreach (CvSeq<CvPoint> c in scanner) {
                        CvRect brect = Cv.BoundingRect(c);
                        int rowstart = brect.Top;
                        int rowend = brect.Bottom;
                        int colstart = brect.Left;
                        int colend = brect.Right;
                        imgTempltTrim = imgTemplt[rowstart, rowend, colstart, colend];
                    }
                }

                Mat temp = Mat.Zeros(big.Width - imgTemplt.Width + 1, big.Height - imgTemplt.Height + 1, MatType.CV_32FC1);
                Cv2.MatchTemplate(big, imgTempltTrim, temp, MatchTemplateMethod.CCorr);

                double minval = 0;
                double maxval = 0;
                OpenCvSharp.CPlusPlus.Point min_loc = new OpenCvSharp.CPlusPlus.Point();
                OpenCvSharp.CPlusPlus.Point max_loc = new OpenCvSharp.CPlusPlus.Point();
                Cv2.MinMaxLoc(temp, out minval, out maxval, out min_loc, out max_loc);

                OpenCvSharp.CPlusPlus.Point top_left = max_loc;
                OpenCvSharp.CPlusPlus.Rect rectframe = new OpenCvSharp.CPlusPlus.Rect(top_left.X, top_left.Y, imgTempltTrim.Width, imgTempltTrim.Height);

                if (debugflag) {

                    //Cv2.ImShow("maskroi_", maskRoi * 30);
                    Cv2.ImShow("bigx30", big * 30);
                    Cv2.ImShow("imgmask", imgmask);
                    Cv2.ImShow("imgMask", imgMask);
                    Cv2.ImShow("temp", temp * 30);
                    Cv2.WaitKey(0);
                }
                double W = top_left.X - ((big.Width / 2) - 256);
                double H = top_left.Y - ((big.Height / 2) - 220);

                if (maxval > templt_x * templt_y * 0.3) {
                    center = new OpenCvSharp.CPlusPlus.Point(W + imgTempltTrim.Width / 2, H + imgTempltTrim.Height / 2);
                } else {
                    center = new OpenCvSharp.CPlusPlus.Point(-1, -1);
                }

                big.Release();
                imgMask.Release();
                affineMask.Release();
                imgTemplt.Release();
                //affineTemplt.Release();
                imgTempltTrim.Release();
                temp.Release();
                //binorg.Release();
                //maskRoi.Release();
            }
            return center;
        }

        private void BeamDetection(string outputfilename, bool isup) {// beam Detection

            int BinarizeThreshold = 60;
            int BrightnessThreshold = 4;
            int nop = 7;

            double dz = 0;
            if (isup == true) {
                dz = -0.003;
            } else {
                dz = 0.003;
            }

            Camera camera = Camera.GetInstance();
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Vector3 InitPoint = mc.GetPoint();
            Vector3 p = new Vector3();
            TracksManager tm = parameterManager.TracksManager;
            int mod = parameterManager.ModuleNo;
            int pl = parameterManager.PlateNo;
            Track myTrack = tm.GetTrack(tm.TrackingIndex);
            string[] sp = myTrack.IdString.Split('-');

            //string datfileName = string.Format("{0}.dat", System.DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            string datfileName = string.Format(@"c:\test\bpm\{0}\{1}-{2}-{3}-{4}-{5}.dat", mod, mod, pl, sp[0], sp[1], System.DateTime.Now.ToString("ddHHmmss"));
            BinaryWriter writer = new BinaryWriter(File.Open(datfileName, FileMode.Create));
            byte[] bb = new byte[440 * 512 * nop];

            string fileName = string.Format("{0}", outputfilename);
            StreamWriter twriter = File.CreateText(fileName);
            string stlog = "";


            List<ImageTaking> LiIT = TakeSequentialImage(0.0, 0.0, dz, nop);

            Mat sum = Mat.Zeros(440, 512, MatType.CV_8UC1);
            for (int i = 0; i < LiIT.Count; i++) {
                Mat bin = (Mat)DogContrastBinalize(LiIT[i].img, 31, BinarizeThreshold);
                Cv2.Add(sum, bin, sum);
                //byte[] b = LiIT[i].img.ToBytes();//format is .png
                MatOfByte mob = new MatOfByte(LiIT[i].img);
                byte[] b = mob.ToArray();
                b.CopyTo(bb, 440 * 512 * i);
            }

            mc.MovePointZ(InitPoint.Z);
            mc.Join();


            Cv2.Threshold(sum, sum, BrightnessThreshold, 1, ThresholdType.Binary);


            //Cv2.FindContoursをつかうとAccessViolationExceptionになる(Release/Debug両方)ので、C-API風に書く
            using (CvMemStorage storage = new CvMemStorage()) {
                using (CvContourScanner scanner = new CvContourScanner(sum.ToIplImage(), storage, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxSimple)) {
                    //string fileName = string.Format(@"c:\img\{0}.txt",
                    //        System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"));

                    foreach (CvSeq<CvPoint> c in scanner) {
                        CvMoments mom = new CvMoments(c, false);
                        if (c.ElemSize < 2) continue;
                        if (mom.M00 == 0.0) continue;
                        double mx = mom.M10 / mom.M00;
                        double my = mom.M01 / mom.M00;
                        stlog += string.Format("{0:F} {1:F}\n", mx, my);
                    }
                }
            }

            twriter.Write(stlog);
            twriter.Close();

            writer.Write(bb);
            writer.Flush();
            writer.Close();

            sum *= 255;
            sum.ImWrite(String.Format(@"c:\img\{0}_{1}_{2}.bmp",
                System.DateTime.Now.ToString("yyyyMMdd_HHmmss"),
                (int)(p.X * 1000),
                (int)(p.Y * 1000)));

        }//BeamDetection

        //...................................

        static Mat DifferenceOfGaussian(Mat image, int kernel = 51) {
            Mat gau = new Mat(440, 512, MatType.CV_8U);
            Mat dst = new Mat(440, 512, MatType.CV_8U);

            Cv2.GaussianBlur(image, image, Cv.Size(3, 3), -1);//
            Cv2.GaussianBlur(image, gau, Cv.Size(kernel, kernel), -1);//パラメータ見ないといけない。
            Cv2.Subtract(gau, image, dst);

            gau.Dispose();
            return dst;
        }
        //.................................

        static Mat DogContrastBinalize(Mat image, int kernel = 51, int threshold = 100, ThresholdType thtype = ThresholdType.Binary) {
            Mat img = DifferenceOfGaussian(image, kernel);

            double Max_kido;
            double Min_kido;
            OpenCvSharp.CPlusPlus.Point maxloc;
            OpenCvSharp.CPlusPlus.Point minloc;
            Cv2.MinMaxLoc(img, out Min_kido, out Max_kido, out minloc, out maxloc);

            Cv2.ConvertScaleAbs(img, img, 255 / (Max_kido - Min_kido), -255 * Min_kido / (Max_kido - Min_kido));
            Cv2.Threshold(img, img, threshold, 1, thtype);

            return img;
        }

        //...........................................

        public struct ImageTaking {
            public Vector3 StageCoord;
            public Mat img;
        }
        //......................................

        List<ImageTaking> TakeSequentialImage(double ax, double ay, double dz, int nimage) {

            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();

            Vector3 initialpos = mc.GetPoint();
            List<ImageTaking> lit = new List<ImageTaking>();

            for (int i = 0; i < nimage; i++) {
                Vector3 dstpoint = new Vector3(
                    initialpos.X + ax * dz * i,
                    initialpos.Y + ay * dz * i,
                    initialpos.Z + dz * i
                );
                mc.MovePoint(dstpoint);
                mc.Join();

                byte[] b = camera.ArrayImage;
                Mat image = new Mat(440, 512, MatType.CV_8U, b);
                ImageTaking it = new ImageTaking();
                it.img = image.Clone();
                it.StageCoord = mc.GetPoint();
                lit.Add(it);

                //image.Release();
                //imagec.Release();
            }

            return lit;
        }

//........................................2016/06/08/ MKSOE/ ............................
        private void NearGridParameter() {
           
            try {
                MotorControler mc = MotorControler.GetInstance(parameterManager);
                GridMark nearestMark = coordManager.GetTheNearestGridMark(mc.GetPoint());
                System.Diagnostics.Debug.WriteLine(String.Format("{0},  {1}", nearestMark.x, nearestMark.y));
                mc.MovePointXY(nearestMark.x, nearestMark.y);
                mc.Join();
            } catch (GridMarkNotFoundException ex) {
                System.Diagnostics.Debug.WriteLine(String.Format("{0}", ex.ToString()));
            }
            try {
                MotorControler mc = MotorControler.GetInstance(parameterManager);
                IGridMarkRecognizer GridMarkRecognizer = coordManager;
                mc.SetSpiralCenterPoint();
                Led led = Led.GetInstance();
                Vector2 encoderPoint = new Vector2(-1, -1);
                encoderPoint.X = mc.GetPoint().X;
                encoderPoint.Y = mc.GetPoint().Y;
                Vector2 viewerPoint = new Vector2(-1, -1);

                bool continueFlag = true;
                while (continueFlag) {
                    led.AdjustLight(parameterManager);
                    viewerPoint = GridMarkRecognizer.SearchGridMarkx50();
                    if (viewerPoint.X < 0 || viewerPoint.Y < 0) {
                        System.Diagnostics.Debug.WriteLine(String.Format("grid mark not found"));
                        mc.MoveInSpiral(true);
                        mc.Join();
                        continueFlag = (mc.SpiralIndex < 30);
                    } else {
                        System.Diagnostics.Debug.WriteLine(String.Format("******** {0}  {1}", viewerPoint.X, viewerPoint.Y));
                        encoderPoint = coordManager.TransToEmulsionCoord(viewerPoint);
                        mc.MovePointXY(encoderPoint);
                        mc.Join();
                        continueFlag = false;
                    }
                } // while

                
                mc.MovePointXY(encoderPoint);
                mc.Join();
                viewerPoint = GridMarkRecognizer.SearchGridMarkx50();
                encoderPoint = coordManager.TransToEmulsionCoord(viewerPoint);
                mc.MovePointXY(encoderPoint);
                mc.Join();

            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine("exception");
            }
            try {
                MotorControler mc = MotorControler.GetInstance(parameterManager);
                Vector3 CurrentCenterPoint = mc.GetPoint();
                GridMark nearestMark = coordManager.GetTheNearestGridMark(CurrentCenterPoint);
                System.Diagnostics.Debug.WriteLine(String.Format("{0},  {1}", CurrentCenterPoint.X, CurrentCenterPoint.Y));
                coordManager.HFDX = CurrentCenterPoint.X - nearestMark.x;
                coordManager.HFDY = CurrentCenterPoint.Y - nearestMark.y;
            } catch (EntryPointNotFoundException ex) {
                MessageBox.Show("エントリポイントが見当たりません。 " + ex.Message);
                System.Diagnostics.Debug.WriteLine("エントリポイントが見当たりません。 " + ex.Message);
            }
        }
        //.......................................................................

        private void gotrack(Track myTrack) {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            double dstx = myTrack.MsX + coordManager.HFDX;
            double dsty = myTrack.MsY + coordManager.HFDY;
            mc.MovePointXY(dstx, dsty, delegate {
                stage.WriteLine(Properties.Strings.MovingComplete);
            });
           
        }
        //.......................................................................

        private void surfacerecog() {
            Surface surface = Surface.GetInstance(parameterManager);
            MotorControler mc = MotorControler.GetInstance(parameterManager);
/*
            string[] sp = myTrack.IdString.Split('-');

            //string datfileName = string.Format("{0}.dat", System.DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            string datfileName = string.Format(@"c:\test\bpm\{0}\{1}-{2}-{3}-{4}-{5}.dat", mod, mod, pl, sp[0], sp[1], System.DateTime.Now.ToString("ddHHmmss"));
            BinaryWriter writer = new BinaryWriter(File.Open(datfileName, FileMode.Create));
            */
           
            try {
                if (mc.IsMoving) {
                    MessageBoxResult r = MessageBox.Show(
                        Properties.Strings.SurfaceException01,
                        Properties.Strings.Abort + "?",
                        MessageBoxButton.YesNo);
                    if (r == MessageBoxResult.Yes) {
                        mc.AbortMoving();
                    } else {
                        return;
                    }
                }
                mc.Join();

                //Surface surface = Surface.GetInstance(parameterManager);

                try {
                    surface.Start(true);

                } catch (Exception ex) {
                    MessageBox.Show(ex.Message, Properties.Strings.Error);
                }
           } catch (Exception ex) {
                MessageBox.Show(ex.Message, Properties.Strings.Error);
           }
            mc.Join();

        }
        //........................................................................
        private void BPMW(Track myTrack, int mod, int pl) {

            MotorControler mc = MotorControler.GetInstance(parameterManager);
            try {

                string datarootdirpath = string.Format(@"C:\MKS_test\bpm\{0}-{1}", mod, pl);
                System.IO.DirectoryInfo mydir = System.IO.Directory.CreateDirectory(datarootdirpath);
                string[] sp = myTrack.IdString.Split('-');
                string uptxt = string.Format(@"c:\MKS_test\bpm\{0}-{1}\{2}-{3}-{4}-{5}_up.txt", mod, pl, mod, pl, sp[0], sp[1]);
                string dwtxt = string.Format(@"c:\MKS_test\bpm\{0}-{1}\{2}-{3}-{4}-{5}_dw.txt", mod, pl - 1, mod, pl - 1, sp[0], sp[1]);
                BeamDetection(uptxt, true);

                BeamPatternMatch bpm = new BeamPatternMatch(8, 200);
                bpm.ReadTrackDataTxtFile(dwtxt, false);

                bpm.ReadTrackDataTxtFile(uptxt, true);
                bpm.DoPatternMatch();

                stage.WriteLine(String.Format("pattern match dx,dy = {0}, {1}", bpm.GetPeakX() * 0.2625 * 0.001, bpm.GetPeakY() * 0.2625 * 0.001));
                Vector3 BfPoint = mc.GetPoint();
                mc.MoveDistance(bpm.GetPeakX() * 0.2625 * 0.001, VectorId.X);
                mc.Join();
                mc.MoveDistance(-bpm.GetPeakY() * 0.2625 * 0.001, VectorId.Y);
                mc.Join();
                Led led = Led.GetInstance();
                led.AdjustLight(parameterManager);
                Vector3 AfPoint = mc.GetPoint();
                stage.WriteLine(String.Format("Move dx,dy = {0}, {1}", BfPoint.X - AfPoint.X, BfPoint.Y - AfPoint.Y));

            } catch (ArgumentOutOfRangeException) {
                MessageBox.Show("ID numver is not existed。 ");
            } catch (System.Exception) {
                MessageBox.Show("No beam battern。 ");
            }
        }
        //........................................................................
        private void GoTopUp() {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Surface surface = Surface.GetInstance(parameterManager);
            try {
                Vector3 cc = mc.GetPoint();
                double Zp = surface.UpTop + 0.015;
                mc.MoveTo(new Vector3(cc.X, cc.Y, Zp));
                mc.Join();
            } catch (ArgumentOutOfRangeException) {
                MessageBox.Show("Cannot move to top surface of upperlayer ");
            }
        }
         private void GoToDown() {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Surface surface = Surface.GetInstance(parameterManager);
            try {
                Vector3 cc = mc.GetPoint();
                double Zp = surface.LowBottom-0.115;
                mc.MoveTo(new Vector3(cc.X, cc.Y, Zp));
                mc.Join();
            } catch (ArgumentOutOfRangeException) {
                MessageBox.Show("Cannot move to top surface of upperlayer ");
            }
        }
        //.......................................................................
        private void Detectbeam(Track myTrack, int mod, int pl) {
            try {
                string[] sp = myTrack.IdString.Split('-');
                string dwtxt = string.Format(@"c:\MKS_test\bpm\{0}-{1}\{2}-{3}-{4}-{5}_dw.txt", mod, pl, mod, pl, sp[0], sp[1]);
                string datarootdirpath = string.Format(@"C:\MKS_test\bpm\{0}-{1}", mod, pl);
                System.IO.DirectoryInfo mydir = System.IO.Directory.CreateDirectory(datarootdirpath);
                System.IO.DirectoryInfo mydir2 = System.IO.Directory.CreateDirectory(datarootdirpath);
                BeamDetection(dwtxt, false);
            } catch (ArgumentOutOfRangeException) {
                MessageBox.Show("ID is not exist ");
            }
        }
        //........................................................................
                   
        private void Tracking(Track myTrack, int mod, int pl, bool dubflag) {

            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            Surface surface = Surface.GetInstance(parameterManager);
            Vector3 initial = mc.GetPoint();
//            TimeLogger tl = new TimeLogger("savepath");

            //for up layer

            int number_of_images = 10;
            int hits = 5;
            double dx = myTrack.MsDX;
            double dy = myTrack.MsDY;
            string[] sp = myTrack.IdString.Split('-');          

            double Sh = 0.5 / (surface.UpTop - surface.UpBottom);
            double tansi = Math.Sqrt(myTrack.MsDX * myTrack.MsDX + myTrack.MsDY * myTrack.MsDY);
            double theta = Math.Atan(tansi);

            double dz;
            double dz_price_img = (6 * Math.Cos(theta) / Sh) / 1000;
            double dz_img = dz_price_img * (-1);

            string datarootdirpath = string.Format(@"C:\MKS_test\{0}", myTrack.IdString);//Open forder to store track information
            System.IO.DirectoryInfo mydir = System.IO.Directory.CreateDirectory(datarootdirpath);

            List<OpenCvSharp.CPlusPlus.Point2d> Msdxdy = new List<OpenCvSharp.CPlusPlus.Point2d>();
            Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(myTrack.MsDX, myTrack.MsDY));

            List<OpenCvSharp.CPlusPlus.Point3d> LStage = new List<OpenCvSharp.CPlusPlus.Point3d>();
            List<OpenCvSharp.CPlusPlus.Point> LPeak = new List<OpenCvSharp.CPlusPlus.Point>();
            List<Point3d> LTrack = new List<Point3d>();
            List<List<ImageTaking>> UpTrackInfo = new List<List<ImageTaking>>();

            //for down layer................................................................
//            tl.Rec("down");
            double Sh_low;
            Sh_low = 0.5 / (surface.LowTop - surface.LowBottom);

            List<OpenCvSharp.CPlusPlus.Point2d> Msdxdy_Low = new List<OpenCvSharp.CPlusPlus.Point2d>();
            List<OpenCvSharp.CPlusPlus.Point3d> LStage_Low = new List<OpenCvSharp.CPlusPlus.Point3d>();
            List<OpenCvSharp.CPlusPlus.Point> LPeak_Low = new List<OpenCvSharp.CPlusPlus.Point>();
            List<Point3d> LTrack_Low = new List<Point3d>();
            List<List<ImageTaking>> LowTrackInfo = new List<List<ImageTaking>>();
            dz_price_img = (6 * Math.Cos(theta) / Sh) / 1000;
            dz_img = dz_price_img * (-1);
            dz = dz_img;
            int gotobase = 0;
            int not_detect = 0;


            for (int i = 0; gotobase < 1; i++) {

                Vector3 initialpos = mc.GetPoint();
                double moverange = (number_of_images - 1) * dz_img;
                double predpoint = moverange + initialpos.Z;

                if (predpoint < surface.UpBottom) {
                    gotobase = 1;

                    dz = surface.UpBottom - initialpos.Z + (number_of_images - 1) * dz_price_img;
                }


                //gotobase = 1のときは、移動して画像を撮影するようにする。
                if (i != 0) {
                    Vector3 dstpoint = new Vector3(
                        LTrack[i - 1].X + Msdxdy[i].X * dz * Sh,
                        LTrack[i - 1].Y + Msdxdy[i].Y * dz * Sh,
                        LTrack[i - 1].Z + dz
                        );
                    mc.MovePoint(dstpoint);
                    mc.Join();
                }

                List<ImageTaking> LiITUpMid = TakeSequentialImage( //image taking
                    Msdxdy[i].X * Sh,//Dx
                    Msdxdy[i].Y * Sh,//Dy
                    dz_img,//Dz
                    number_of_images);//number of images


                LStage.Add(new OpenCvSharp.CPlusPlus.Point3d(LiITUpMid[number_of_images - 1].StageCoord.X, LiITUpMid[number_of_images - 1].StageCoord.Y, LiITUpMid[number_of_images - 1].StageCoord.Z));
                LiITUpMid[number_of_images - 1].img.ImWrite(datarootdirpath + string.Format(@"\img_l_up_{0}.png", i));
                UpTrackInfo.Add(LiITUpMid);


                List<Mat> binimages = new List<Mat>();
                for (int t = 0; t <= number_of_images - 1; t++) {
                    Mat bin = (Mat)DogContrastBinalize(LiITUpMid[t].img);

                    double xx = myTrack.MsDX * myTrack.MsDX;
                    double yy = myTrack.MsDY * myTrack.MsDY;
                    if (Math.Sqrt(xx + yy) >= 0.4) {//////////////////????????????????????
                        Cv2.Dilate(bin, bin, new Mat());
                    }
                    Cv2.Dilate(bin, bin, new Mat());
                    binimages.Add(bin);
                }

                
                //trackを重ねる処理を入れる。
                Point2d pixel_cen = TD_naka_1(binimages, 256, 220, 3, 3, 4, 90, hits, dx, dy);//, true);
                //Point2d pixel_cen = TrackDetection_new(binimages, 256, 220, hits, dx, dy, mod, pl, sp[0], sp[1], true);
                //Point2d pixel_cen = TrackDetection_new2(binimages, 256, 220, 3, 3, 4, 90, 5, dx, dy, mod, pl, sp[0], sp[1], true);

                if (pixel_cen.X == -1 & pixel_cen.Y == -1) {
                    Point2d pixel_cen2 = TD_naka_2(binimages, 256, 220, 3, 3, 4, 90, hits, dx, dy);
                    if (pixel_cen2.X == -1 & pixel_cen2.Y == -1) {
                        Point2d pixel_cen3 = TD_naka_3(binimages, 256, 220, 3, 3, 4, 90, hits, dx, dy);
                        if (pixel_cen3.X == -1 & pixel_cen3.Y == -1) {
                            Point2d pixel_cen4 = TD_naka_4(binimages, 256, 220, 3, 3, 4, 90, hits, dx, dy);
                            if (pixel_cen4.X == -1 & pixel_cen4.Y == -1) {
                                mc.Join();
                                not_detect = 1;
                                goto not_detect_track;
                            } else {
                                pixel_cen.X = pixel_cen4.X;
                                pixel_cen.Y = pixel_cen4.Y;
                            }

                        } else {
                            pixel_cen.X = pixel_cen3.X;
                            pixel_cen.Y = pixel_cen3.Y;
                        }
                    } else {
                        pixel_cen.X = pixel_cen2.X;
                        pixel_cen.Y = pixel_cen2.Y; 
                    
                    }
                }


                LPeak.Add(new OpenCvSharp.CPlusPlus.Point(pixel_cen.X - 256, pixel_cen.Y - 220));

                double firstx = LStage[i].X - LPeak[i].X * 0.000267;
                double firsty = LStage[i].Y + LPeak[i].Y * 0.000267;
                double firstz = LStage[i].Z;
                LTrack.Add(new Point3d(firstx, firsty, firstz));
                //

                //上側乳剤層上面の1回目のtrack検出によって、次のtrackの位置を検出する角度を求める。
                //その角度が、1回目のtrack検出の結果によって大きな角度にならないように調整をする。

                if (i == 0) {
                    Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(myTrack.MsDX, myTrack.MsDY));

                } else if (i == 1) {
                    List<Point3d> LTrack_ghost = new List<Point3d>();
                    double dzPrev = (LStage[0].Z - surface.UpTop) * Sh;
                    double Lghost_x = LTrack[0].X - Msdxdy[i].X * dzPrev;
                    double Lghost_y = LTrack[0].Y - Msdxdy[i].Y * dzPrev;
                    LTrack_ghost.Add(new Point3d(Lghost_x, Lghost_y, surface.UpTop));//上側乳剤層上面にtrackがあるならどの位置にあるかを算出する。

                    string txtfileName_ltrackghost = datarootdirpath + string.Format(@"\LTrack_ghost.txt");
                    StreamWriter twriter_ltrackghost = File.CreateText(txtfileName_ltrackghost);
                    twriter_ltrackghost.WriteLine("{0} {1} {2}", LTrack_ghost[0].X, LTrack_ghost[0].Y, LTrack_ghost[0].Z);
                    twriter_ltrackghost.Close();

                    OpenCvSharp.CPlusPlus.Point2d Tangle = ApproximateStraight(Sh, LTrack_ghost[0], LTrack[0], LTrack[1]);
                    Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle.X, Tangle.Y));

                } else {
                    OpenCvSharp.CPlusPlus.Point2d Tangle = ApproximateStraight(Sh, LTrack[i - 2], LTrack[i - 1], LTrack[i]);
                    Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle.X, Tangle.Y));
                }


            }//for　i-loop

            //baseまたぎ
            int ltrack_counter = LTrack.Count();
            int msdxdy_counter = Msdxdy.Count();

            mc.MovePoint(
                LTrack[ltrack_counter - 1].X + Msdxdy[msdxdy_counter - 1].X * (surface.LowTop - surface.UpBottom),
                LTrack[ltrack_counter - 1].Y + Msdxdy[msdxdy_counter - 1].Y * (surface.LowTop - surface.UpBottom),
                surface.LowTop
                );
            mc.Join();

            //////ここから下gelの処理
            Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Msdxdy[msdxdy_counter - 1].X, Msdxdy[msdxdy_counter - 1].Y));

            //今までのtrack追跡プログラムとは異なる角度等の使い方をする。
            dz_price_img = (6 * Math.Cos(theta) / Sh_low) / 1000;
            dz_img = dz_price_img * (-1);
            dz = dz_img;

            int goto_dgel = 0;

            for (int i = 0; goto_dgel < 1; i++) {
                ///////移動して画像処理をしたときに、下gelの下に入らないようにする。
                Vector3 initialpos = mc.GetPoint();
                double moverange = (number_of_images - 1) * dz_img;
                double predpoint = moverange + initialpos.Z;

                if (predpoint < surface.LowBottom)//もしもbaseに入りそうなら、8枚目の画像がちょうど下gelを撮影するようにdzを調整する。
                {
                    goto_dgel = 1;

                    dz = surface.LowBottom - initialpos.Z + (number_of_images - 1) * dz_price_img;
                }
                ////////

                //goto_dgel == 1のときは、移動して画像を撮影するようにする。
                if (i != 0) {
                    Vector3 dstpoint = new Vector3(
                    LTrack_Low[i - 1].X + Msdxdy_Low[i].X * dz * Sh_low,
                    LTrack_Low[i - 1].Y + Msdxdy_Low[i].Y * dz * Sh_low,
                    LTrack_Low[i - 1].Z + dz
                    );
                    mc.MovePoint(dstpoint);
                    mc.Join();
                }

                //今までのtrack追跡プログラムとは異なる角度等の使い方をする。
                List<ImageTaking> LiITLowMid = TakeSequentialImage(
                    Msdxdy[i].X * Sh_low,
                    Msdxdy[i].Y * Sh_low,
                    dz_img,
                    number_of_images);

                //画像・座標の記録
                LStage_Low.Add(new OpenCvSharp.CPlusPlus.Point3d(LiITLowMid[number_of_images - 1].StageCoord.X, LiITLowMid[number_of_images - 1].StageCoord.Y, LiITLowMid[number_of_images - 1].StageCoord.Z));
                LiITLowMid[number_of_images - 1].img.ImWrite(datarootdirpath + string.Format(@"\img_l_low_{0}.png", i));

                LowTrackInfo.Add(LiITLowMid);//撮影した8枚の画像と、撮影した位置を記録する。
                
                
                //撮影した画像をここで処理する。
                List<Mat> binimages = new List<Mat>();
                for (int t = 0; t <= number_of_images - 1; t++) {
                    Mat bin = (Mat)DogContrastBinalize(LiITLowMid[t].img);

                    double xx = myTrack.MsDX * myTrack.MsDX;
                    double yy = myTrack.MsDY * myTrack.MsDY;
                    if (Math.Sqrt(xx + yy) >= 0.4) {
                        Cv2.Dilate(bin, bin, new Mat());
                    }
                    Cv2.Dilate(bin, bin, new Mat());
                    binimages.Add(bin);
                }
                //trackを重ねる処理を入れる。
                Point2d pixel_cen = TD_naka_1(binimages, 256, 220, 3, 3, 4, 90, hits, dx, dy);//, true);//画像の8枚目におけるtrackのpixel座標を算出する。
               // Point2d pixel_cen = TrackDetection_new(binimages, 256, 220, hits, dx, dy, mod, pl, sp[0], sp[1], true);
              //  Point2d pixel_cen2 = TrackDetection_new2(binimages, 256, 220, 3, 3, 4, 90, 5, dx, dy, mod, pl, sp[0], sp[1], true);
                
               //////////////////////////////////////////////

                if (pixel_cen.X == -1 & pixel_cen.Y == -1) {
                    Point2d pixel_cen2 = TD_naka_2(binimages, 256, 220, 3, 3, 4, 90, hits, dx, dy);
                    if (pixel_cen2.X == -1 & pixel_cen2.Y == -1) {
                        Point2d pixel_cen3 = TD_naka_3(binimages, 256, 220, 3, 3, 4, 90, hits, dx, dy);
                        if (pixel_cen3.X == -1 & pixel_cen3.Y == -1) {
                            Point2d pixel_cen4 = TD_naka_4(binimages, 256, 220, 3, 3, 4, 90, hits, dx, dy);
                            if (pixel_cen4.X == -1 & pixel_cen4.Y == -1) {
                                mc.Join();
                                not_detect = 1;
                                goto not_detect_track;
                            } else {
                                pixel_cen.X = pixel_cen3.X;
                                pixel_cen.Y = pixel_cen3.Y;
                            }

                        } else {
                            pixel_cen.X = pixel_cen3.X;
                            pixel_cen.Y = pixel_cen3.Y;
                        }
                    } else {
                        pixel_cen.X = pixel_cen2.X;
                        pixel_cen.Y = pixel_cen2.Y;

                    }
                }
                //

                //検出したpixel座標をstage座標に変換するなどlistに追加する。
                LPeak_Low.Add(new OpenCvSharp.CPlusPlus.Point(pixel_cen.X - 256, pixel_cen.Y - 220));

                double firstx = LStage_Low[i].X - LPeak_Low[i].X * 0.000267;
                double firsty = LStage_Low[i].Y + LPeak_Low[i].Y * 0.000267;
                double firstz = LStage_Low[i].Z;
                LTrack_Low.Add(new Point3d(firstx, firsty, firstz));
                //

                //ここからは、最小二乗法で角度を算出するプログラムである。
                //上側乳剤層上面の1回目のtrack検出によって、次のtrackの位置を検出する角度を求める。
                //その角度が、1回目のtrack検出の結果によって大きな角度にならないように調整をする。
                if (i == 0) {
                    OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraightBase(Sh, Sh_low, LTrack[ltrack_counter - 2], LTrack[ltrack_counter - 1], LTrack_Low[i], surface);
                    Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                } else if (i == 1) {
                    OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraightBase(Sh, Sh_low, LTrack[ltrack_counter - 1], LTrack_Low[i - 1], LTrack_Low[i], surface);
                    Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                } else {
                    OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraight(Sh_low, LTrack_Low[i - 2], LTrack_Low[i - 1], LTrack_Low[i]);
                    Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                }


            }//i_loop

            //
            int ltrack_low_count = LTrack_Low.Count();
            mc.MovePointXY(LTrack_Low[ltrack_low_count - 1].X, LTrack_Low[ltrack_low_count - 1].Y);

            mc.Join();

              //検出に失敗した場合は、ループを抜けてここに来る。
        not_detect_track: ;//検出に失敗したと考えられる地点で画像を取得し、下ゲル下面まで移動する。(現在は下ゲル下面とするが、今後変更する可能性有。)

           /* if (not_detect != 0) {
               //Vector dzz = 
                Vector3 currentpoint = mc.GetPoint();
                Vector3 dstpoint_ = new Vector3(
                  currentpoint.X - Msdxdy[0].X * 0.05* Sh,
                  currentpoint.Y - Msdxdy[0].Y * 0.05 * Sh,
                  currentpoint.Z - 0.08
                   );
                mc.MovePoint(dstpoint_);
                mc.Join();
               List<ImageTaking> NotDetect = TakeSequentialImage(
                    Msdxdy[0].X * Sh,
                    Msdxdy[0].Y * Sh,
                    0.003,
                   40); */

                 if (not_detect != 0) {
               //Vector dzz = 
              /*   Vector3 currentpoint1 = mc.GetPoint();
                 Vector3 dstpoint_1 = new Vector3(
                   currentpoint1.X + Msdxdy[0].X * 0.05* Sh,
                   currentpoint1.Y + Msdxdy[0].Y * 0.05 * Sh,
                   currentpoint1.Z + 0.015
                    );
                 mc.MovePoint(dstpoint_1);
                 mc.Join();*/

                Vector3 currentpoint = mc.GetPoint();
                Vector3 dstpoint_ = new Vector3(
                  currentpoint.X,
                  currentpoint.Y,
                  currentpoint.Z - 0.10
                   );
                mc.MovePoint(dstpoint_);
                mc.Join();

                
                List<ImageTaking> NotDetect = TakeSequentialImage(
                        0,
                        0,
                        0.003,
                       50);

                SuperImposer si = new SuperImposer();
                List<Mat> binimages = new List<Mat>();
                for (int t = 0; t < NotDetect.Count; t++) {
                   // Cv2.ImShow("NotDetect[t].img", NotDetect[t].img);
                   // Cv2.WaitKey(0);
                    si.AddImg(NotDetect[t].img);
                }

                Mat bigimg_dog = new Mat();
                Mat bigimg_org = new Mat();

                si.MakeImg(out bigimg_dog, out bigimg_org);
                Cv2.ImShow("superimposed image org", bigimg_org);
                Cv2.ImShow("superimposed image dog", bigimg_dog);
                Cv2.WaitKey(0);


               /* string logtxt = string.Format(@"C:\MKS_test\WorkingTime\{0}\{1}-{2}_TTracking.txt", mod, mod, pl);
                SimpleLogger SL1 = new SimpleLogger(logtxt, sp1[0], sp1[1]);*/

                string txtfileName_t_not_detect = datarootdirpath + string.Format(@"\not_detect.txt");
                StreamWriter twriter_t_not_detect = File.CreateText(txtfileName_t_not_detect);
            //   for (int i = 0; i < NotDetect.Count; i++) {
                    bigimg_org.ImWrite(datarootdirpath + string.Format(@"\superimpsed_org.png"));
                    bigimg_dog.ImWrite(datarootdirpath + string.Format(@"\superimpsed_dog.png"));
                   // Vector3 p = NotDetect[i].StageCoord;
                    //twriter_t_not_detect.WriteLine("{0} {1} {2} {3}", i, p.X, p.Y, p.Z);
               // }
                //twriter_t_not_detect.Close();

                //mc.MovePointZ(surface.LowBottom);
                Vector3 cc = mc.GetPoint();
                double Zp = surface.UpTop;
                mc.MoveTo(new Vector3(cc.X, cc.Y, Zp));
                mc.Join();

                mc.Join();
            }



            //file write out up_gel
            string txtfileName_sh_up = datarootdirpath + string.Format(@"\Sh_up.txt");
            StreamWriter twriter_sh_up = File.CreateText(txtfileName_sh_up);
            twriter_sh_up.WriteLine("{0}", Sh);
            twriter_sh_up.Close();

            //file write out
            string txtfileName_t_info_up = datarootdirpath + string.Format(@"\location_up.txt");
            StreamWriter twriter_t_info_up = File.CreateText(txtfileName_t_info_up);
            for (int i = 0; i < UpTrackInfo.Count; i++) {
                for (int t = 0; t < UpTrackInfo[i].Count; t++) {
                    UpTrackInfo[i][t].img.ImWrite(datarootdirpath + string.Format(@"\img_t_info_up_{0}-{1}.png", i, t));
                    Vector3 p = UpTrackInfo[i][t].StageCoord;
                    twriter_t_info_up.WriteLine("{0} {1} {2} {3} {4}", i, t, p.X, p.Y, p.Z);
                }
            }
            twriter_t_info_up.Close();

            string txtfileName_lpeak = datarootdirpath + string.Format(@"\lpeak_up.txt");
            StreamWriter twriter_lpeak = File.CreateText(txtfileName_lpeak);
            for (int i = 0; i < LPeak.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point p = LPeak[i];
                twriter_lpeak.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_lpeak.Close();

            string txtfileName_ltrack = datarootdirpath + string.Format(@"\ltrack_up.txt");
            StreamWriter twriter_ltrack = File.CreateText(txtfileName_ltrack);
            for (int i = 0; i < LTrack.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point3d p = LTrack[i];
                twriter_ltrack.WriteLine("{0} {1} {2} {3}", i, p.X, p.Y, p.Z);
            }
            twriter_ltrack.Close();

            string txtfileName_msdxdy = datarootdirpath + string.Format(@"\msdxdy.txt");
            StreamWriter twriter_msdxdy = File.CreateText(txtfileName_msdxdy);
            for (int i = 0; i < Msdxdy.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point2d p = Msdxdy[i];
                twriter_msdxdy.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_msdxdy.Close();


            //file write out low_gel
            string txtfileName_sh_low = datarootdirpath + string.Format(@"\Sh_low.txt");
            StreamWriter twriter_sh_low = File.CreateText(txtfileName_sh_low);
            twriter_sh_low.WriteLine("{0}", Sh_low);
            twriter_sh_low.Close();

            string txtfileName_t_info_low = datarootdirpath + string.Format(@"\location_low.txt");
            StreamWriter twriter_t_info_low = File.CreateText(txtfileName_t_info_low);          
            for (int i = 0; i < LowTrackInfo.Count; i++) {
                for (int t = 0; t < LowTrackInfo[i].Count; t++) {
                    LowTrackInfo[i][t].img.ImWrite(datarootdirpath + string.Format(@"\img_t_info_low_{0}-{1}.png", i, t));
                    Vector3 p = LowTrackInfo[i][t].StageCoord;
                    twriter_t_info_low.WriteLine("{0} {1} {2} {3} {4}", i, t, p.X, p.Y, p.Z);
                    
                }
            }
            twriter_t_info_low.Close();

            string txtfileName_lpeak_low = datarootdirpath + string.Format(@"\lpeak_low.txt");
            StreamWriter twriter_lpeak_low = File.CreateText(txtfileName_lpeak_low);
            for (int i = 0; i < LPeak_Low.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point p = LPeak_Low[i];
                twriter_lpeak_low.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_lpeak_low.Close();

            string txtfileName_ltrack_low = datarootdirpath + string.Format(@"\ltrack_low.txt");
            StreamWriter twriter_ltrack_low = File.CreateText(txtfileName_ltrack_low);
            for (int i = 0; i < LTrack_Low.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point3d p = LTrack_Low[i];
                twriter_ltrack_low.WriteLine("{0} {1} {2} {3}", i, p.X, p.Y, p.Z);
            }
            twriter_ltrack_low.Close();

            string txtfileName_msdxdy_low = datarootdirpath + string.Format(@"\msdxdy_low.txt");
            StreamWriter twriter_msdxdy_low = File.CreateText(txtfileName_msdxdy_low);
            for (int i = 0; i < Msdxdy_Low.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point2d p = Msdxdy_Low[i];
                twriter_msdxdy_low.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_msdxdy_low.Close();


            if (dubflag == false){
                string TPC;
                int LL = LStage_Low.Count;
                if (LL == 0) {
                    TPC = "Nopass_topsurface";                   
                    
                } else {
                    double LZ = Math.Abs(LStage_Low[LL - 1].Z);
                    int UU = LStage.Count;
                    double UZ = Math.Abs(LStage[UU - 1].Z);
                   
                    if (LZ+0.003 >= Math.Abs(surface.LowBottom)) {
                        TPC = "Pass";
                    } else {

                        TPC = "NoPass";
                    }              
                    
                }
                string[] sp1 = myTrack.IdString.Split('-');
             /*   string logtxt_ = string.Format(@"c:\test\bpm\{0}\{1}-{2}_TCK.txt", mod, mod, pl);
                //string log_ = string.Format("{0} \n", sw.Elapsed);
                string log_ = string.Format("{0} {1} {2} \n", sp1[0], sp1[1], TPC);
                StreamWriter swr = new StreamWriter(logtxt_, true, Encoding.ASCII);
                swr.Write(log_);
                swr.Close();*/

                string logtxt = string.Format(@"C:\MKS_test\followingCheck\{0}\{1}-{2}_Trackingcheck.txt", mod, mod, pl);
                SimpleLogger SL1 = new SimpleLogger(logtxt, sp1[0], sp1[1]);
                SL1.Trackcheck(TPC);

            }
    }

        private void Tracking2(Track myTrack, int mod, int pl, bool dubflag) {

            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            Surface surface = Surface.GetInstance(parameterManager);
            Vector3 initial = mc.GetPoint();
            //            TimeLogger tl = new TimeLogger("savepath");

            //for up layer
            int window_H1 = 23;
            int window_H2 = 23;
            int number_of_images = 10;


            double dx = myTrack.MsDX;
            double dy = myTrack.MsDY;
            string[] sp = myTrack.IdString.Split('-');
            int hits = 6;
            double Sh = 0.5 / (surface.UpTop - surface.UpBottom);
            double tansi = Math.Sqrt(myTrack.MsDX * myTrack.MsDX + myTrack.MsDY * myTrack.MsDY);
            double theta = Math.Atan(tansi);

            double dz;
            double dz_price_img = (6 * Math.Cos(theta) / Sh) / 1000;
            double dz_img = dz_price_img * (-1);

            string datarootdirpath = string.Format(@"C:\MKS_test\{0}", myTrack.IdString);//Open forder to store track information
            System.IO.DirectoryInfo mydir = System.IO.Directory.CreateDirectory(datarootdirpath);

            List<OpenCvSharp.CPlusPlus.Point2d> Msdxdy = new List<OpenCvSharp.CPlusPlus.Point2d>();
            Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(myTrack.MsDX, myTrack.MsDY));

            List<OpenCvSharp.CPlusPlus.Point3d> LStage = new List<OpenCvSharp.CPlusPlus.Point3d>();
            List<OpenCvSharp.CPlusPlus.Point> LPeak = new List<OpenCvSharp.CPlusPlus.Point>();
            List<Point3d> LTrack = new List<Point3d>();
            List<List<ImageTaking>> UpTrackInfo = new List<List<ImageTaking>>();

            //for down layer................................................................
            //            tl.Rec("down");
            double Sh_low;
            Sh_low = 0.5 / (surface.LowTop - surface.LowBottom);

            List<OpenCvSharp.CPlusPlus.Point2d> Msdxdy_Low = new List<OpenCvSharp.CPlusPlus.Point2d>();
            List<OpenCvSharp.CPlusPlus.Point3d> LStage_Low = new List<OpenCvSharp.CPlusPlus.Point3d>();
            List<OpenCvSharp.CPlusPlus.Point> LPeak_Low = new List<OpenCvSharp.CPlusPlus.Point>();
            List<Point3d> LTrack_Low = new List<Point3d>();
            List<List<ImageTaking>> LowTrackInfo = new List<List<ImageTaking>>();        
            int gotobase = 0;
            int not_detect = 0;
            // int notrack = 0;
           // int ltrack_counter = LTrack.Count();
          //  int msdxdy_counter = Msdxdy.Count();

            for (int i = 0; gotobase < 1; i++) {
                dz = dz_img;
                int notrack = 0;
                hits = 6;
                dx = Msdxdy[i].X;
                dy = Msdxdy[i].Y;
            Retracking: ;
            //dz_img = dz;
               /*   if (notrack == 1){

                      dz_img = dz;
                }*/
                /*    if (notrack == 2){
                
                        goto not_detect_track;
                        not_detect = 1;
                    }*/

                Vector3 initialpos = mc.GetPoint();
                double moverange = (number_of_images - 1) * dz_img;
                double predpoint = moverange + initialpos.Z;

                if (predpoint < surface.UpBottom) {
                    gotobase = 1;

                    dz = surface.UpBottom - initialpos.Z + (number_of_images - 1) * dz_price_img;
                }


                //gotobase = 1のときは、移動して画像を撮影するようにする。
                if (i != 0) {
                    Vector3 dstpoint = new Vector3(
                        LTrack[i - 1].X + Msdxdy[i - 1].X * dz * Sh,
                        LTrack[i - 1].Y + Msdxdy[i - 1].Y * dz * Sh,
                        LTrack[i - 1].Z + dz
                        );
                    mc.MovePoint(dstpoint);
                    mc.Join();
                }

                List<ImageTaking> LiITUpMid = TakeSequentialImage( //image taking
                    Msdxdy[i].X * Sh,//Dx
                    Msdxdy[i].Y * Sh,//Dy
                    -0.003,//Dz                //////////////////
                    number_of_images);//number of images


                LStage.Add(new OpenCvSharp.CPlusPlus.Point3d(LiITUpMid[number_of_images - 1].StageCoord.X, LiITUpMid[number_of_images - 1].StageCoord.Y, LiITUpMid[number_of_images - 1].StageCoord.Z));
                LiITUpMid[number_of_images - 1].img.ImWrite(datarootdirpath + string.Format(@"\img_l_up_{0}.png", i));
                UpTrackInfo.Add(LiITUpMid);


                List<Mat> binimages = new List<Mat>();
                for (int t = 0; t <= number_of_images - 1; t++) {
                    Mat bin = (Mat)DogContrastBinalize(LiITUpMid[t].img);

                    double xx = myTrack.MsDX * myTrack.MsDX;
                    double yy = myTrack.MsDY * myTrack.MsDY;
                    if (Math.Sqrt(xx + yy) >= 0.2) { // if tantheta is grater than 0.4, Dilate the track
                        Cv2.Dilate(bin, bin, new Mat());
                    }
                    Cv2.Dilate(bin, bin, new Mat());
                    binimages.Add(bin);
                }

                //Track detection......................................................................................................
                // remakeTrP: ;
                Point2d pixel_cen = TD_naka_1(binimages, 256, 220, 3, 3, 4, window_H1, hits, dx, dy , i);//, true);
                //Point2d pixel_cen = TrackDetection_new(binimages, 256, 220, hits, dx, dy, mod, pl, sp[0], sp[1], true);
                //Point2d pixel_cen = TrackDetection_new2(binimages, 256, 220, 3, 3, 4, 90, 5, dx, dy, mod, pl, sp[0], sp[1], true);

                /*    if (pixel_cen.X == -1 & pixel_cen.Y == -1) {
                        Point2d pixel_cen2 = TD_naka_2(binimages, 256, 220, 3, 3, 4, window_H2, hits, dx, dy);


                        if (pixel_cen2.X == -1 & pixel_cen2.Y == -1) {
                        
                            hits = 5;
                            Point2d pixel_cen3 = TD_naka_1(binimages, 256, 220, 3, 3, 4, window_H1, hits, dx, dy);

                            if (pixel_cen3.X == -1 & pixel_cen3.Y == -1) {
                           
                                Point2d pixel_cen4 = TD_naka_2(binimages, 256, 220, 3, 3, 4, window_H2, hits, dx, dy);

                                if (pixel_cen4.X == -1 & pixel_cen4.Y == -1) {

                                    if (i != 0) {

                                        LStage.Remove(LStage[i]);
                                        UpTrackInfo.Remove(UpTrackInfo[i]);
                                        notrack++;

                                        if (notrack == 1) {
                                            dz = dz + 0.015;
                                            goto Retracking;
                                        } else if (notrack == 2) {
                                            mc.Join();
                                            not_detect = 1;
                                            goto not_detect_track;
                                        }

                                    } else if (i == 0) {
                                        mc.Join();
                                        not_detect = 1;
                                        goto not_detect_track;
                                    }
                                } else {
                                    pixel_cen.X = pixel_cen4.X;
                                    pixel_cen.Y = pixel_cen4.Y;
                                }
                            } else {
                                pixel_cen.X = pixel_cen3.X;
                                pixel_cen.Y = pixel_cen3.Y;
                            }
                        } else {
                            pixel_cen.X = pixel_cen2.X;
                            pixel_cen.Y = pixel_cen2.Y;
                        }
                    }
                    */

                
                if (pixel_cen.X == -1 & pixel_cen.Y == -1) {
                    Point2d pixel_cen2 = TD_naka_2(binimages, 256, 220, 3, 3, 4, window_H2, hits, dx, dy);


                    if (pixel_cen2.X == -1 & pixel_cen2.Y == -1) {

                        hits = 5;
                        Point2d pixel_cen3 = TD_naka_1(binimages, 256, 220, 3, 3, 4, window_H1, hits, dx, dy, i);

                        if (pixel_cen3.X == -1 & pixel_cen3.Y == -1) {

                            Point2d pixel_cen4 = TD_naka_2(binimages, 256, 220, 3, 3, 4, window_H2, hits, dx, dy);

                            if (pixel_cen4.X == -1 & pixel_cen4.Y == -1) {

                                if (i != 0) {

                                    LStage.Remove(LStage[i]);
                                    UpTrackInfo.Remove(UpTrackInfo[i]);
                                    notrack++;

                                    if (notrack == 1) {
                                        dz = dz + 0.015;
                                        goto Retracking;
                                    } else if (notrack == 2) {
                                        mc.Join();
                                        not_detect = 1;
                                        goto not_detect_track;
                                    }

                                } else if (i == 0) {
                                    mc.Join();
                                    not_detect = 1;
                                    goto not_detect_track;
                                }
                            } else {
                                pixel_cen.X = pixel_cen4.X;
                                pixel_cen.Y = pixel_cen4.Y;
                            }
                        } else {
                            pixel_cen.X = pixel_cen3.X;
                            pixel_cen.Y = pixel_cen3.Y;
                        }
                    } else {
                        pixel_cen.X = pixel_cen2.X;
                        pixel_cen.Y = pixel_cen2.Y;
                    }
                }


                //......................................................................................................


                LPeak.Add(new OpenCvSharp.CPlusPlus.Point(pixel_cen.X - 256, pixel_cen.Y - 220));

                double firstx = LStage[i].X - LPeak[i].X * 0.000267;
                double firsty = LStage[i].Y + LPeak[i].Y * 0.000267;
                double firstz = LStage[i].Z;
                LTrack.Add(new Point3d(firstx, firsty, firstz));


                if (i == 0) {
                    Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(myTrack.MsDX, myTrack.MsDY));

                } else if (i == 1) {
                    List<Point3d> LTrack_ghost = new List<Point3d>();
                    double dzPrev = (LStage[0].Z - surface.UpTop) * Sh;
                    double Lghost_x = LTrack[0].X - Msdxdy[i].X * dzPrev;
                    double Lghost_y = LTrack[0].Y - Msdxdy[i].Y * dzPrev;
                    LTrack_ghost.Add(new Point3d(Lghost_x, Lghost_y, surface.UpTop));//上側乳剤層上面にtrackがあるならどの位置にあるかを算出する。

                    string txtfileName_ltrackghost = datarootdirpath + string.Format(@"\LTrack_ghost.txt");
                    StreamWriter twriter_ltrackghost = File.CreateText(txtfileName_ltrackghost);
                    twriter_ltrackghost.WriteLine("{0} {1} {2}", LTrack_ghost[0].X, LTrack_ghost[0].Y, LTrack_ghost[0].Z);
                    twriter_ltrackghost.Close();

                    OpenCvSharp.CPlusPlus.Point2d Tangle = ApproximateStraight(Sh, LTrack_ghost[0], LTrack[0], LTrack[1]);
                    Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle.X, Tangle.Y));

                } else {
                    OpenCvSharp.CPlusPlus.Point2d Tangle = ApproximateStraight(Sh, LTrack[i - 2], LTrack[i - 1], LTrack[i]);
                    Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle.X, Tangle.Y));
                }


            }//for　i-loop

            //baseまたぎ
              int ltrack_counter = LTrack.Count();
              int msdxdy_counter = Msdxdy.Count();

            mc.MovePoint(
                LTrack[ltrack_counter - 1].X + Msdxdy[msdxdy_counter - 1].X * (surface.LowTop - surface.UpBottom),
                LTrack[ltrack_counter - 1].Y + Msdxdy[msdxdy_counter - 1].Y * (surface.LowTop - surface.UpBottom),
                surface.LowTop
                );
            mc.Join();


            //////For Down Layer//
            Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Msdxdy[msdxdy_counter - 1].X, Msdxdy[msdxdy_counter - 1].Y));
            dz_price_img = (6 * Math.Cos(theta) / Sh_low) / 1000;
            dz_img = dz_price_img * (-1);
            dz = dz_img;

            int goto_dgel = 0;

            for (int i = 0; goto_dgel < 1; i++) {
                int notrack_low = 0;
                dz = dz_img;
                dx = Msdxdy_Low[i].X;
                dy = Msdxdy_Low[i].Y;
            Retracking_low: ;

                
                Vector3 initialpos = mc.GetPoint();
                double moverange = (number_of_images - 1) * dz_img;
                double predpoint = moverange + initialpos.Z;

                if (predpoint < surface.LowBottom)//もしもbaseに入りそうなら、8枚目の画像がちょうど下gelを撮影するようにdzを調整する。
                {
                    goto_dgel = 1;

                    dz = surface.LowBottom - initialpos.Z + (number_of_images - 1) * dz_price_img;
                }
                ////////

                //goto_dgel == 1のときは、移動して画像を撮影するようにする。
                if (i != 0) {
                    Vector3 dstpoint = new Vector3(
                    LTrack_Low[i - 1].X + Msdxdy_Low[i].X * dz * Sh_low,
                    LTrack_Low[i - 1].Y + Msdxdy_Low[i].Y * dz * Sh_low,
                    LTrack_Low[i - 1].Z + dz
                    );
                    mc.MovePoint(dstpoint);
                    mc.Join();
                }

                //
                List<ImageTaking> LiITLowMid = TakeSequentialImage(
                    Msdxdy[i].X * Sh_low,
                    Msdxdy[i].Y * Sh_low,
                    -0.003,                   ////
                    number_of_images);

                //
                LStage_Low.Add(new OpenCvSharp.CPlusPlus.Point3d(LiITLowMid[number_of_images - 1].StageCoord.X, LiITLowMid[number_of_images - 1].StageCoord.Y, LiITLowMid[number_of_images - 1].StageCoord.Z));
                LiITLowMid[number_of_images - 1].img.ImWrite(datarootdirpath + string.Format(@"\img_l_low_{0}.png", i));
                LowTrackInfo.Add(LiITLowMid);

                
                List<Mat> binimages = new List<Mat>();
                for (int t = 0; t <= number_of_images - 1; t++) {
                    Mat bin = (Mat)DogContrastBinalize(LiITLowMid[t].img);

                    double xx = myTrack.MsDX * myTrack.MsDX;
                    double yy = myTrack.MsDY * myTrack.MsDY;
                    if (Math.Sqrt(xx + yy) >= 0.2) {
                        Cv2.Dilate(bin, bin, new Mat());
                    }
                    Cv2.Dilate(bin, bin, new Mat());
                    binimages.Add(bin);
                }
                //trackを重ねる処理を入れる。

                Point2d pixel_cen = TD_naka_1(binimages, 256, 220, 3, 3, 4, window_H1, hits, dx, dy ,i);//, true);
                //Point2d pixel_cen = TrackDetection_new(binimages, 256, 220, hits, dx, dy, mod, pl, sp[0], sp[1], true);
                //Point2d pixel_cen = TrackDetection_new2(binimages, 256, 220, 3, 3, 4, 90, 5, dx, dy, mod, pl, sp[0], sp[1], true);

                /*    if (pixel_cen.X == -1 & pixel_cen.Y == -1) {
                        Point2d pixel_cen2 = TD_naka_2(binimages, 256, 220, 3, 3, 4, window_H2, hits, dx, dy);

                        if (pixel_cen2.X == -1 & pixel_cen2.Y == -1) {

                            if (i != 0) {

                                LStage.Remove(LStage[i]);
                                UpTrackInfo.Remove(UpTrackInfo[i]);
                                notrack_low++;

                                if (notrack_low == 1) {
                                    dz = dz + 0.015;
                                    goto Retracking_low;
                                } else if (notrack_low == 2) {
                                    mc.Join();
                                    not_detect = 1;
                                    goto not_detect_track;
                                }

                            } else if (i == 0) {
                                mc.Join();
                                not_detect = 1;
                                goto not_detect_track;
                            }
                        } else {
                            pixel_cen.X = pixel_cen2.X;
                            pixel_cen.Y = pixel_cen2.Y;
                        }
                    }*/

                if (pixel_cen.X == -1 & pixel_cen.Y == -1) {
                    Point2d pixel_cen2 = TD_naka_2(binimages, 256, 220, 3, 3, 4, window_H2, hits, dx, dy);


                    if (pixel_cen2.X == -1 & pixel_cen2.Y == -1) {

                        hits = 5;
                        Point2d pixel_cen3 = TD_naka_1(binimages, 256, 220, 3, 3, 4, window_H1, hits, dx, dy, i);

                        if (pixel_cen3.X == -1 & pixel_cen3.Y == -1) {

                            Point2d pixel_cen4 = TD_naka_2(binimages, 256, 220, 3, 3, 4, window_H2, hits, dx, dy);

                            if (pixel_cen4.X == -1 & pixel_cen4.Y == -1) {

                                if (i != 0) {

                                    LStage_Low.Remove(LStage_Low[i]);
                                   LowTrackInfo.Remove(LowTrackInfo[i]);
                                    notrack_low++;

                                    if (notrack_low == 1) {
                                        dz = dz + 0.015;
                                        goto Retracking_low;
                                    } else if (notrack_low == 2) {
                                        mc.Join();
                                        not_detect = 1;
                                        goto not_detect_track;
                                    }

                                } else if (i == 0) {
                                    mc.Join();
                                    not_detect = 1;
                                    goto not_detect_track;
                                }
                            } else {
                                pixel_cen.X = pixel_cen4.X;
                                pixel_cen.Y = pixel_cen4.Y;
                            }
                        } else {
                            pixel_cen.X = pixel_cen3.X;
                            pixel_cen.Y = pixel_cen3.Y;
                        }
                    } else {
                        pixel_cen.X = pixel_cen2.X;
                        pixel_cen.Y = pixel_cen2.Y;
                    }
                }
                //

                //検出したpixel座標をstage座標に変換するなどlistに追加する。
                LPeak_Low.Add(new OpenCvSharp.CPlusPlus.Point(pixel_cen.X - 256, pixel_cen.Y - 220));

                double firstx = LStage_Low[i].X - LPeak_Low[i].X * 0.000267;
                double firsty = LStage_Low[i].Y + LPeak_Low[i].Y * 0.000267;
                double firstz = LStage_Low[i].Z;
                LTrack_Low.Add(new Point3d(firstx, firsty, firstz));
                //

                //ここからは、最小二乗法で角度を算出するプログラムである。
                //上側乳剤層上面の1回目のtrack検出によって、次のtrackの位置を検出する角度を求める。
                //その角度が、1回目のtrack検出の結果によって大きな角度にならないように調整をする。
                if (i == 0) {
                    OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraightBase(Sh, Sh_low, LTrack[ltrack_counter - 2], LTrack[ltrack_counter - 1], LTrack_Low[i], surface);
                    Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                } else if (i == 1) {
                    OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraightBase(Sh, Sh_low, LTrack[ltrack_counter - 1], LTrack_Low[i - 1], LTrack_Low[i], surface);
                    Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                } else {
                    OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraight(Sh_low, LTrack_Low[i - 2], LTrack_Low[i - 1], LTrack_Low[i]);
                    Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                }


            }//i_loop

            //
            int ltrack_low_count = LTrack_Low.Count();
            mc.MovePointXY(LTrack_Low[ltrack_low_count - 1].X, LTrack_Low[ltrack_low_count - 1].Y);

            mc.Join();

              //検出に失敗した場合は、ループを抜けてここに来る。
        not_detect_track: ;//検出に失敗したと考えられる地点で画像を取得し、下ゲル下面まで移動する。(現在は下ゲル下面とするが、今後変更する可能性有。)

            /* if (not_detect != 0) {
                //Vector dzz = 
                 Vector3 currentpoint = mc.GetPoint();
                 Vector3 dstpoint_ = new Vector3(
                   currentpoint.X - Msdxdy[0].X * 0.05* Sh,
                   currentpoint.Y - Msdxdy[0].Y * 0.05 * Sh,
                   currentpoint.Z - 0.08
                    );
                 mc.MovePoint(dstpoint_);
                 mc.Join();
                List<ImageTaking> NotDetect = TakeSequentialImage(
                     Msdxdy[0].X * Sh,
                     Msdxdy[0].Y * Sh,
                     0.003,
                    40); */

            if (not_detect != 0) {

                int pozi_flag = 0;
                //Vector dzz = 
                Vector3 currentpoint1 = mc.GetPoint();//ここで、もし境界面付近ならば、境界面によって処理を変更するように修正する。

                if (surface.UpBottom < currentpoint1.Z)//上ゲルにあるか、下ゲルで見失ったのかを判断する。 
                {
                    pozi_flag = 1;
                }
                if (surface.LowTop > currentpoint1.Z) 
                {
                    pozi_flag = 2;
                }

                Vector3 dstpoint_1 = new Vector3(
                    currentpoint1.X + Msdxdy[0].X * 0.015* Sh,
                    currentpoint1.Y + Msdxdy[0].Y * 0.015 * Sh,
                    currentpoint1.Z + 0.015
                    );
                mc.MovePoint(dstpoint_1);
                mc.Join();

                Vector3 currentpoint = mc.GetPoint();
                Vector3 dstpoint_ = new Vector3(
                  currentpoint.X,
                  currentpoint.Y,
                  currentpoint.Z - 0.08
                   );
                mc.MovePoint(dstpoint_);
                mc.Join();


                double surfacepointLow  = 0;
                if (pozi_flag == 1) 
                {
                    surfacepointLow = surface.UpBottom;
                }
                if (pozi_flag == 2) 
                {
                    surfacepointLow = surface.LowBottom;
                }


                if (surfacepointLow >　dstpoint_.Z) //もし、下に移動した結果、baseやゲルの外に移動してしまうのならば、境界面まで移動する。
                {
                    dstpoint_.Z = surfacepointLow;
                    mc.MovePoint(dstpoint_);
                }

                currentpoint = mc.GetPoint();//現在の位置に更新。(写真を撮り始める位置である。)

                double surfacepointUp = 0;
                if (pozi_flag == 1) 
                {
                    surfacepointUp = surface.UpTop;
                }
                if (pozi_flag == 2) 
                {
                    surfacepointUp = surface.LowTop;
                }

                int moveflag = 0;
                int takeImg = 50;
                if (currentpoint.Z + 0.15 > surfacepointUp) //画像を50枚?撮影した時に、baseの中やゲルの外を撮影するようならそうしないようにする。
                {
                    takeImg = (int)((surfacepointUp - currentpoint.Z) / 0.003);//これでゲル以外の部分を撮影するようなら適切な画像の枚数に変化することができる。

                    if (pozi_flag == 2) //もし、下ゲルの上面を飛び越そうとするのならば、上ゲルに関しても、積層画像を作成するようにする。
                    {
                        moveflag = 1;
                    }
                }

                List<ImageTaking> NotDetect = TakeSequentialImage(
                        0,
                        0,
                        0.003,
                        takeImg);

                SuperImposer si = new SuperImposer();
                List<Mat> binimages = new List<Mat>();
                for (int t = 0; t < NotDetect.Count; t++) {
                    // Cv2.ImShow("NotDetect[t].img", NotDetect[t].img);
                    // Cv2.WaitKey(0);
                    si.AddImg(NotDetect[t].img);
                }

                Mat bigimg_dog = new Mat();
                Mat bigimg_org = new Mat();

                si.MakeImg(out bigimg_dog, out bigimg_org);
                Cv2.ImShow("superimposed image org", bigimg_org);
                Cv2.ImShow("superimposed image dog", bigimg_dog);
                Cv2.WaitKey(0);


                /* string logtxt = string.Format(@"C:\MKS_test\WorkingTime\{0}\{1}-{2}_TTracking.txt", mod, mod, pl);
                 SimpleLogger SL1 = new SimpleLogger(logtxt, sp1[0], sp1[1]);*/

                // string txtfileName_t_not_detect = datarootdirpath + string.Format(@"\not_detect.txt");
                // StreamWriter twriter_t_not_detect = File.CreateText(txtfileName_t_not_detect);
                //   for (int i = 0; i < NotDetect.Count; i++) {
                bigimg_org.ImWrite(datarootdirpath + string.Format(@"\superimpsed_org.png"));
                bigimg_dog.ImWrite(datarootdirpath + string.Format(@"\superimpsed_dog.png"));
                // Vector3 p = NotDetect[i].StageCoord;
                //twriter_t_not_detect.WriteLine("{0} {1} {2} {3}", i, p.X, p.Y, p.Z);
                // }
                //twriter_t_not_detect.Close();

                if (moveflag == 1) 
                {
                    currentpoint.Z = surface.UpBottom;
                    mc.MovePoint(currentpoint);//base上面に移動する。
                    mc.Join();

                    List<ImageTaking> NotDetect2 = TakeSequentialImage(
                        0,
                        0,
                        0.003,
                        30);

                    SuperImposer si2 = new SuperImposer();
                    List<Mat> binimages2 = new List<Mat>();
                    for (int t = 0; t < NotDetect2.Count; t++) {
                        // Cv2.ImShow("NotDetect[t].img", NotDetect[t].img);
                        // Cv2.WaitKey(0);
                        si2.AddImg(NotDetect2[t].img);
                    }

                    Mat bigimg_dog2 = new Mat();
                    Mat bigimg_org2 = new Mat();

                    si2.MakeImg(out bigimg_dog2, out bigimg_org2);
                    Cv2.ImShow("superimposed image org", bigimg_org2);
                    Cv2.ImShow("superimposed image dog", bigimg_dog2);
                    Cv2.WaitKey(0);

                    bigimg_org2.ImWrite(datarootdirpath + string.Format(@"\superimpsed_org2.png"));
                    bigimg_dog2.ImWrite(datarootdirpath + string.Format(@"\superimpsed_dog2.png"));
                }




                //mc.MovePointZ(surface.LowBottom);
                Vector3 cc = mc.GetPoint();
                double Zp = surface.UpTop;
                mc.MoveTo(new Vector3(cc.X, cc.Y, Zp));
                mc.Join();

                mc.Join();
            }



            //file write out up_gel
            string txtfileName_sh_up = datarootdirpath + string.Format(@"\Sh_up.txt");
            StreamWriter twriter_sh_up = File.CreateText(txtfileName_sh_up);
            twriter_sh_up.WriteLine("{0}", Sh);
            twriter_sh_up.Close();

            //file write out
            string txtfileName_t_info_up = datarootdirpath + string.Format(@"\location_up.txt");
            StreamWriter twriter_t_info_up = File.CreateText(txtfileName_t_info_up);
            for (int i = 0; i < UpTrackInfo.Count; i++) {
                for (int t = 0; t < UpTrackInfo[i].Count; t++) {
                    UpTrackInfo[i][t].img.ImWrite(datarootdirpath + string.Format(@"\img_t_info_up_{0}-{1}.png", i, t));
                    Vector3 p = UpTrackInfo[i][t].StageCoord;
                    twriter_t_info_up.WriteLine("{0} {1} {2} {3} {4}", i, t, p.X, p.Y, p.Z);
                }
            }
            twriter_t_info_up.Close();

            string txtfileName_lpeak = datarootdirpath + string.Format(@"\lpeak_up.txt");
            StreamWriter twriter_lpeak = File.CreateText(txtfileName_lpeak);
            for (int i = 0; i < LPeak.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point p = LPeak[i];
                twriter_lpeak.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_lpeak.Close();

            string txtfileName_ltrack = datarootdirpath + string.Format(@"\ltrack_up.txt");
            StreamWriter twriter_ltrack = File.CreateText(txtfileName_ltrack);
            for (int i = 0; i < LTrack.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point3d p = LTrack[i];
                twriter_ltrack.WriteLine("{0} {1} {2} {3}", i, p.X, p.Y, p.Z);
            }
            twriter_ltrack.Close();

            string txtfileName_msdxdy = datarootdirpath + string.Format(@"\msdxdy.txt");
            StreamWriter twriter_msdxdy = File.CreateText(txtfileName_msdxdy);
            for (int i = 0; i < Msdxdy.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point2d p = Msdxdy[i];
                twriter_msdxdy.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_msdxdy.Close();


            //file write out low_gel
            string txtfileName_sh_low = datarootdirpath + string.Format(@"\Sh_low.txt");
            StreamWriter twriter_sh_low = File.CreateText(txtfileName_sh_low);
            twriter_sh_low.WriteLine("{0}", Sh_low);
            twriter_sh_low.Close();

            string txtfileName_t_info_low = datarootdirpath + string.Format(@"\location_low.txt");
            StreamWriter twriter_t_info_low = File.CreateText(txtfileName_t_info_low);
            for (int i = 0; i < LowTrackInfo.Count; i++) {
                for (int t = 0; t < LowTrackInfo[i].Count; t++) {
                    LowTrackInfo[i][t].img.ImWrite(datarootdirpath + string.Format(@"\img_t_info_low_{0}-{1}.png", i, t));
                    Vector3 p = LowTrackInfo[i][t].StageCoord;
                    twriter_t_info_low.WriteLine("{0} {1} {2} {3} {4}", i, t, p.X, p.Y, p.Z);

                }
            }
            twriter_t_info_low.Close();

            string txtfileName_lpeak_low = datarootdirpath + string.Format(@"\lpeak_low.txt");
            StreamWriter twriter_lpeak_low = File.CreateText(txtfileName_lpeak_low);
            for (int i = 0; i < LPeak_Low.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point p = LPeak_Low[i];
                twriter_lpeak_low.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_lpeak_low.Close();

            string txtfileName_ltrack_low = datarootdirpath + string.Format(@"\ltrack_low.txt");
            StreamWriter twriter_ltrack_low = File.CreateText(txtfileName_ltrack_low);
            for (int i = 0; i < LTrack_Low.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point3d p = LTrack_Low[i];
                twriter_ltrack_low.WriteLine("{0} {1} {2} {3}", i, p.X, p.Y, p.Z);
            }
            twriter_ltrack_low.Close();

            string txtfileName_msdxdy_low = datarootdirpath + string.Format(@"\msdxdy_low.txt");
            StreamWriter twriter_msdxdy_low = File.CreateText(txtfileName_msdxdy_low);
            for (int i = 0; i < Msdxdy_Low.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point2d p = Msdxdy_Low[i];
                twriter_msdxdy_low.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_msdxdy_low.Close();


            if (dubflag == false) {
                string TPC;
                int LL = LStage_Low.Count;
                if (LL == 0) {
                    TPC = "Nopass_topsurface";

                } else {
                    double LZ = Math.Abs(LStage_Low[LL - 1].Z);
                    int UU = LStage.Count;
                    double UZ = Math.Abs(LStage[UU - 1].Z);

                    if (LZ + 0.003 >= Math.Abs(surface.LowBottom)) {
                        TPC = "Pass";
                    } else {

                        TPC = "NoPass";
                    }

                }
                string[] sp1 = myTrack.IdString.Split('-');
                /*   string logtxt_ = string.Format(@"c:\test\bpm\{0}\{1}-{2}_TCK.txt", mod, mod, pl);
                   //string log_ = string.Format("{0} \n", sw.Elapsed);
                   string log_ = string.Format("{0} {1} {2} \n", sp1[0], sp1[1], TPC);
                   StreamWriter swr = new StreamWriter(logtxt_, true, Encoding.ASCII);
                   swr.Write(log_);
                   swr.Close();*/

                string logtxt = string.Format(@"C:\MKS_test\followingCheck\{0}\{1}-{2}_Trackingcheck_newversion.txt", mod, mod, pl);
                SimpleLogger SL1 = new SimpleLogger(logtxt, sp1[0], sp1[1]);
                SL1.Trackcheck(TPC);

            }
  }
    

        
        //........................................................................

        private void Full_Automatic_Tracking_button(object sender, RoutedEventArgs e) {

            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            TracksManager tm = parameterManager.TracksManager;
            Surface surface = Surface.GetInstance(parameterManager);
            int mod = parameterManager.ModuleNo;
            int pl = parameterManager.PlateNo;
            bool dubflag;
            Led led_ = Led.GetInstance();
           
            

            for (int i = 0; i < tm.NumOfTracks; i++) {
                Track myTrack = tm.GetTrack(i);
                Console.WriteLine(myTrack.IdString);
                string[] sp1 = myTrack.IdString.Split('-');

                string logtxt = string.Format(@"C:\MKS_test\WorkingTime\{0}\{1}-{2}_Trackingtime.txt", mod, mod, pl);
                SimpleLogger SL1 = new SimpleLogger(logtxt, sp1[0], sp1[1]);
                SL1.Info("Tracking Start");

                MessageBoxResult r;
                // Massage box to check tracking is ok or not, if OK is put, will go to track position.
                r = MessageBox.Show(
                  String.Format("Let's go to {0}; {1} {2}. OK->Go, Cancel->exit", myTrack.IdString, myTrack.MsX, myTrack.MsY),
                  Properties.Strings.LensTypeSetting,
                  MessageBoxButton.OKCancel);
                if (r == MessageBoxResult.Cancel) {
                    return;
                }


             // Go to the track position
                double dstx = myTrack.MsX;
                double dsty = myTrack.MsY;
                mc.MovePointXY(dstx, dsty, delegate {
                    stage.WriteLine(Properties.Strings.MovingComplete);
                });
                mc.Join();


             // Oil putting time
                Thread.Sleep(1000);
                led_.AdjustLight(parameterManager);


             // Go to near grid mark and get parameter for shrinkage in X,Y
                NearGridParameter();


             // Go to track position after correction with near gridmark
                gotrack(myTrack);
                mc.Join();
                Vector3 initialpos = mc.GetPoint();


                bool surfacerecogOK = false;

             // Detection of boundaries of emulsion layers
                for (int trial = 0; trial < 5; trial++ ) {
                    mc.MoveTo(initialpos);
                    mc.Join();

                    mc.MoveTo(new Vector3(initialpos.X, initialpos.Y, initialpos.Z + 0.02));
                    mc.Join();
                    led_.AdjustLight(parameterManager);
                    Thread.Sleep(500);
                    surfacerecog();
                    while (surface.IsActive) {
                        Thread.Sleep(500);
                    }
                    mc.Join();
                    Double Base = (surface.UpBottom - surface.LowTop) * 1000;
                    Double UpLayer = (surface.UpTop - surface.UpBottom) * 1000;
                    Double LowLayer = (surface.LowTop - surface.LowBottom) * 1000;

                    if (Base < 50.0 && UpLayer > 200.0 && LowLayer > 200.0) {
                        surfacerecogOK = true;
                        break;
                    }
                }

                if (surfacerecogOK == false) {
                    continue;//go to the next track
                }
                Thread.Sleep(100);
                GoTopUp();
                mc.Join();

             // Beampatternmatching
                BPMW(myTrack, mod, pl);
                mc.Join();
                Thread.Sleep(100);


                // Tracking in emulsion
                Tracking(myTrack, mod, pl, false);
              
                // #If the the followed track is stop or cause event, go to Upper surface and to the next track#


                // Taking Beam pattern
               // Detectbeam(myTrack, mod, pl);


                // Go to top of uppr layer after tracking is finished
                GoTopUp();
                mc.Join();
                Thread.Sleep(100);




                // Massage box to cheack tracking is ok or not, it Ok is put, it will go to next track.
                r = MessageBox.Show(
                     "OK->Next, Cancel->exit",
                     Properties.Strings.LensTypeSetting,
                     MessageBoxButton.OKCancel);
                if (r == MessageBoxResult.Cancel) {
                    return;
                }
                SL1.Info("Tracking End");

            }
        }


        static OpenCvSharp.CPlusPlus.Point TrackDetection_new2(List<Mat> mats, int px, int py, int shiftx = 2, int shifty = 2, int shiftpitch = 4, int windowsize = 40, int phthresh = 5, double dx = 0.12, double dy = 0.12, int mod = 5, int plt = 2, string sp0 = "", string sp1 = "", bool debugflag = true) {
            OpenCvSharp.CPlusPlus.Point center = new OpenCvSharp.CPlusPlus.Point(0, 0);
            int x0 = px - 256;
            int y0 = py - 220;

           // List<trackdata> rms = new List<trackdata>();


            double phi = Math.Atan2(dy, dx);
            double angle = phi * (180 / Math.PI);
            double tant = Math.Sqrt(dx * dx + dy * dy);
            double pix = 0.26;//pix_to_micron

            //Making a mask
            double depth = 180;
            int mask_x = (int)(90 + tant * depth);//if tantheta=0.38, mask_x=30
            int mask_y = 90;
            if (mask_x < mask_y) {
                mask_x = mask_y;
            }

            int counter = 0;
            // Mat imgTemplt = Mat.Zeros(512, 440, MatType.CV_8UC1);
            //using (Mat big = Mat.Zeros(521, 440, MatType.CV_8UC1))
            using (Mat big = Mat.Zeros(600, 600, MatType.CV_8UC1))
            using (Mat imgMask = Mat.Zeros(big.Height, big.Width, MatType.CV_8UC1)) {


                int ystart = big.Height / 2 - mask_y / 2;
                int yend = big.Height / 2 + mask_y / 2;
                int xstart = big.Width / 2 - mask_x / 2;
                int xend = big.Width / 2 + mask_x / 2;
                OpenCvSharp.CPlusPlus.Rect recMask = new OpenCvSharp.CPlusPlus.Rect(xstart, ystart, mask_x, mask_y);
                Cv2.Rectangle(imgMask, recMask, 255, -1);//brightness=1, fill
                Mat imgmask = imgMask.Clone();
                Mat affineMask = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(big.Width / 2, big.Height / 2), angle, 1);
                Cv2.WarpAffine(imgmask, imgMask, affineMask, imgMask.Size());



                // Cv2.Rectangle(imgMask, recMask, 255, -1);//brightness=1, fill

                for (int p = 0; p < mats.Count; p++) {
                    int startx = big.Width / 2 - mats[p].Width / 2;
                    int starty = big.Height / 2 - mats[p].Height / 2;
                    Cv2.Add(
                        big[starty, starty + mats[p].Height, startx, startx + mats[p].Width],
                        mats[p],
                        big[starty, starty + mats[p].Height, startx, startx + mats[p].Width]);
                }

                using (Mat big_c = big.Clone()) {
                    Cv2.BitwiseAnd(big, imgMask, big);
                    Mat big_ = big.Clone();
                    Cv2.Threshold(big, big, phthresh, 255, ThresholdType.ToZero);
                    //Cv2.BitwiseAnd(big, imgMask, big);
                    //Mat big_ = big.Clone();
                    Mat affineMask2 = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(big.Width / 2, big.Height / 2), angle, 1);
                    Cv2.WarpAffine(big, big, affineMask2, imgMask.Size());
                    Mat maskRoi = big.Clone();
                    
                    if (debugflag) {

                        //Cv2.ImShow("maskroi_", maskRoi * 30);
                        Cv2.ImShow("bigx30", big * 30);
                        Cv2.ImShow("imgmask", big_*30);
                        Cv2.ImShow("imgMask", imgMask);                      

                        Cv2.ImShow("bigcut", big_c * 30);
                        Cv2.WaitKey(0);
                    }

                    //  Cv2.BitwiseAnd(big, imgMask, big);

                }//using big_c

                //Making template
                double forcaldepth = 15;
                //int templt_x = (int)(tant * forcaldepth / pix);//if tantheta=0.38, templt_x=22
                int templt_x = 6;
                int templt_y = 6;
                if (templt_x < templt_y) {
                    templt_x = templt_y;
                }

                Mat imgTemplt = Mat.Zeros(200, 200, MatType.CV_8UC1);
                OpenCvSharp.CPlusPlus.Rect recTemplt = new OpenCvSharp.CPlusPlus.Rect(100, 100, templt_x, templt_y);
                Cv2.Rectangle(imgTemplt, recTemplt, 1, -1);//brightness=1, fill
                Mat affineTemplt = Cv2.GetRotationMatrix2D(new OpenCvSharp.CPlusPlus.Point(100, 100), 0, 1);
                Cv2.WarpAffine(imgTemplt, imgTemplt, affineTemplt, imgTemplt.Size(), Interpolation.Linear, BorderType.Constant, 0);
                Mat Template = imgTemplt.Clone();

                Mat imgTempltTrim = new Mat();
                using (CvMemStorage storage = new CvMemStorage())
                using (CvContourScanner scanner = new CvContourScanner(imgTemplt.ToIplImage(), storage, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxSimple)) {
                    foreach (CvSeq<CvPoint> c in scanner) {
                        CvRect brect = Cv.BoundingRect(c);
                        int rowstart = brect.Top;
                        int rowend = brect.Bottom;
                        int colstart = brect.Left;
                        int colend = brect.Right;
                        imgTempltTrim = imgTemplt[rowstart, rowend, colstart, colend];
                    }
                }

                Mat temp = Mat.Zeros(big.Width - imgTemplt.Width + 1, big.Height - imgTemplt.Height + 1, MatType.CV_32FC1);
                Cv2.MatchTemplate(big, imgTempltTrim, temp, MatchTemplateMethod.CCorr);

                double minval = 0;
                double maxval = 0;
                OpenCvSharp.CPlusPlus.Point min_loc = new OpenCvSharp.CPlusPlus.Point();
                OpenCvSharp.CPlusPlus.Point max_loc = new OpenCvSharp.CPlusPlus.Point();
                Cv2.MinMaxLoc(temp, out minval, out maxval, out min_loc, out max_loc);

                OpenCvSharp.CPlusPlus.Point top_left = max_loc;
                OpenCvSharp.CPlusPlus.Rect rectframe = new OpenCvSharp.CPlusPlus.Rect(top_left.X, top_left.Y, imgTempltTrim.Width, imgTempltTrim.Height);

                if (debugflag) {

                    //Cv2.ImShow("maskroi_", maskRoi * 30);
                    Cv2.ImShow("bigx30", big * 30);
                    Cv2.ImShow("imgmask", imgmask);
                    Cv2.ImShow("imgMask", imgMask);
                    Cv2.ImShow("temp", Template * 30);
                    Cv2.ImShow("temp", temp * 30);

                    
                    Cv2.WaitKey(0);
                }
                double W = top_left.X - ((big.Width / 2) - 256);
                double H = top_left.Y - ((big.Height / 2) - 220);

                if (maxval > templt_x * templt_y * 0.3) {
                    center = new OpenCvSharp.CPlusPlus.Point(W + imgTempltTrim.Width / 2, H + imgTempltTrim.Height / 2);
                } else {
                    center = new OpenCvSharp.CPlusPlus.Point(-1, -1);
                }

                big.Release();
                imgMask.Release();
                affineMask.Release();
                imgTemplt.Release();
                //affineTemplt.Release();
                imgTempltTrim.Release();
                temp.Release();
                //binorg.Release();
                //maskRoi.Release();
            }
            return center;
        }


// ##############################################################################################
                        //AUTOMATIC TRACKING FOR PL#2

        private void Full_Automatic_Tracking_PL2_button(object sender, RoutedEventArgs e) {

            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            TracksManager tm = parameterManager.TracksManager;
            Track myTrack = tm.GetTrack(tm.TrackingIndex);
            Surface surface = Surface.GetInstance(parameterManager);
            int mod = parameterManager.ModuleNo;
            int pl = parameterManager.PlateNo;
            bool dubflag;
            Led led_ = Led.GetInstance();
            string[] sp1 = myTrack.IdString.Split('-');

            string logtxt = string.Format(@"C:\MKS_test\WorkingTime\{0}\{1}-{2}_TTracking_newversion.txt", mod, mod, pl);
            SimpleLogger SL1 = new SimpleLogger(logtxt, sp1[0], sp1[1]);

            // Tracking in emulsion
            SL1.Info("Tracking Start");            
          //  Tracking(myTrack, mod, pl, false);

         //   GoToDown();
          //  mc.Join();
            // Taking Beam pattern            
          //  Detectbeam(myTrack, mod, pl);
            

            // Go to top of uppr layer after tracking is finished
            

            
           

           /* // Oil putting time
            Thread.Sleep(1000);
            led_.AdjustLight(parameterManager);*/

/*
            //Go to near grid mark and get parameter for shrinkage in X,Y
            NearGridParameter();


            // Go to track position after correction with near gridmark
            gotrack(myTrack);
            mc.Join();
            Vector3 initialpos = mc.GetPoint();*/


            bool surfacerecogOK = false;

            // Detection of boundaries of emulsion layers
        /*    for (int trial = 0; trial < 1; trial++) {
                mc.MoveTo(initialpos);
                mc.Join();

                mc.MoveTo(new Vector3(initialpos.X, initialpos.Y, initialpos.Z + 0.02));
                mc.Join();
                led_.AdjustLight(parameterManager);
                Thread.Sleep(500);
                surfacerecog();
                while (surface.IsActive) {
                    Thread.Sleep(500);
                }
                mc.Join();
                Double Base = (surface.UpBottom - surface.LowTop) * 1000;
                Double UpLayer = (surface.UpTop - surface.UpBottom) * 1000;
                Double LowLayer = (surface.LowTop - surface.LowBottom) * 1000;

                if (Base < 50.0 && UpLayer > 200.0 && LowLayer > 200.0) {
                    surfacerecogOK = true;
                    break;
                }
            }

           // if (surfacerecogOK == false) {

           //     continue;//go to the next track
          //  }
        */
          /*
            // Beampatternmatching
            BPMW(myTrack, mod, pl);
            mc.Join();
            Thread.Sleep(100);*/


            // Tracking in emulsion
            Tracking2(myTrack, mod, pl, false);

            // #If the the followed track is stop or cause event, go to Upper surface and to the next track#


            // Taking Beam pattern
            // Detectbeam(myTrack, mod, pl);


            // Go to top of uppr layer after tracking is finished
            GoTopUp();
            mc.Join();
            Thread.Sleep(100);


        }
         

        private void GoNearGrid_CorrecteTrackPosition_button(object sender, RoutedEventArgs e) {  
        TracksManager tm = parameterManager.TracksManager;
        Track myTrack = tm.GetTrack(tm.TrackingIndex);
        int mod = parameterManager.ModuleNo;
        int pl = parameterManager.PlateNo;
        string[] sp1 = myTrack.IdString.Split('-');
        MotorControler mc = MotorControler.GetInstance(parameterManager);
        string logtxt = string.Format(@"C:\MKS_test\WorkingTime\{0}\{1}-{2}_TNeargrid.txt", mod, mod, pl);
        SimpleLogger SL2 = new SimpleLogger(logtxt, sp1[0], sp1[1]);
        SL2.Info("Neargrid Start");

        NearGridParameter();
        gotrack(myTrack);
        mc.Join();
        Led led_ = Led.GetInstance();
        led_.AdjustLight(parameterManager);
        Thread.Sleep(200);
        surfacerecog();
        SL2.Info("Neargrid End");
        }



        //..................Start Buttton..............................

        private void Start_button(object sender, RoutedEventArgs e) {

            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            TracksManager tm = parameterManager.TracksManager;
            Track myTrack = tm.GetTrack(tm.TrackingIndex);
            Surface surface = Surface.GetInstance(parameterManager);
            Stopwatch sw = new Stopwatch();
            int mod = parameterManager.ModuleNo;
            int pl = parameterManager.PlateNo;

            sw.Start();
            //Surface surface = Surface.GetInstance(parameterManager);
            /*  try {
                  surface.Start(true);
              } catch (Exception ex) {
                  MessageBox.Show(ex.Message, Properties.Strings.Error);
              }
              mc.Join();*/
            //.................Go near grid mark from current point............./////

            try {
                //MotorControler mc = MotorControler.GetInstance(parameterManager);
                GridMark nearestMark = coordManager.GetTheNearestGridMark(mc.GetPoint());
                System.Diagnostics.Debug.WriteLine(String.Format("{0},  {1}", nearestMark.x, nearestMark.y));
                mc.MovePointXY(nearestMark.x, nearestMark.y);
                mc.Join();
            } catch (GridMarkNotFoundException ex) {
                System.Diagnostics.Debug.WriteLine(String.Format("{0}", ex.ToString()));
            }
            try {
                // MotorControler mc = MotorControler.GetInstance(parameterManager);
                IGridMarkRecognizer GridMarkRecognizer = coordManager;
                mc.SetSpiralCenterPoint();
                Led led = Led.GetInstance();
                Vector2 encoderPoint = new Vector2(-1, -1);
                encoderPoint.X = mc.GetPoint().X;
                encoderPoint.Y = mc.GetPoint().Y;//おこられたのでしかたなくこうする　吉田20150427
                Vector2 viewerPoint = new Vector2(-1, -1);

                bool continueFlag = true;
                while (continueFlag) {
                    led.AdjustLight(parameterManager);
                    viewerPoint = GridMarkRecognizer.SearchGridMarkx50();
                    if (viewerPoint.X < 0 || viewerPoint.Y < 0) {
                        System.Diagnostics.Debug.WriteLine(String.Format("grid mark not found"));
                        mc.MoveInSpiral(true);
                        mc.Join();
                        continueFlag = (mc.SpiralIndex < 30);
                    } else {
                        System.Diagnostics.Debug.WriteLine(String.Format("******** {0}  {1}", viewerPoint.X, viewerPoint.Y));
                        encoderPoint = coordManager.TransToEmulsionCoord(viewerPoint);
                        mc.MovePointXY(encoderPoint);
                        mc.Join();
                        continueFlag = false;
                    }
                } // while

                //重心検出と移動を2回繰り返して、グリッドマークを視野中心にもっていく
                mc.MovePointXY(encoderPoint);
                mc.Join();
                viewerPoint = GridMarkRecognizer.SearchGridMarkx50();
                encoderPoint = coordManager.TransToEmulsionCoord(viewerPoint);
                mc.MovePointXY(encoderPoint);
                mc.Join();

            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine("exception");
            }

            //.........Get value of HFDX and HFDY................//

            try {

                Vector3 CurrentCenterPoint = mc.GetPoint();
                GridMark nearestMark = coordManager.GetTheNearestGridMark(CurrentCenterPoint);
                System.Diagnostics.Debug.WriteLine(String.Format("{0},  {1}", CurrentCenterPoint.X, CurrentCenterPoint.Y));
                coordManager.HFDX = CurrentCenterPoint.X - nearestMark.x;
                coordManager.HFDY = CurrentCenterPoint.Y - nearestMark.y;
            } catch (EntryPointNotFoundException ex) {
                MessageBox.Show("エントリポイントが見当たりません。 " + ex.Message);
                System.Diagnostics.Debug.WriteLine("エントリポイントが見当たりません。 " + ex.Message);
            }

            //.......　Move to track place after revised with shift value.....//

            double dstx = myTrack.MsX + coordManager.HFDX;
            double dsty = myTrack.MsY + coordManager.HFDY;
            mc.MovePointXY(dstx, dsty, delegate {
                stage.WriteLine(Properties.Strings.MovingComplete);
            });

            mc.Join();

            Led led_ = Led.GetInstance();
            led_.AdjustLight(parameterManager);
            Thread.Sleep(500); //Wait for 5s

            //////////////////////////////Surfacerecog/////////////////////////////////////.................................../////////

            try {
                if (mc.IsMoving) {
                    MessageBoxResult r = MessageBox.Show(
                        Properties.Strings.SurfaceException01,
                        Properties.Strings.Abort + "?",
                        MessageBoxButton.YesNo);
                    if (r == MessageBoxResult.Yes) {
                        mc.AbortMoving();
                    } else {
                        return;
                    }
                }


                //Surface surface = Surface.GetInstance(parameterManager);
                if (surface.IsActive) {
                    MessageBoxResult r = MessageBox.Show(
                        Properties.Strings.SurfaceException02,
                        Properties.Strings.Abort + "?",
                        MessageBoxButton.YesNo);
                    if (r == MessageBoxResult.Yes) {
                        surface.Abort();
                    } else {
                        return;
                    }
                }

                try {
                    surface.Start(true);

                } catch (Exception ex) {
                    MessageBox.Show(ex.Message, Properties.Strings.Error);
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, Properties.Strings.Error);
            }
            mc.Join();

            //...............Beam Pattern Matching..................////////////////


            try {

                string datarootdirpath = string.Format(@"C:\test\bpm\{0}", mod);
                System.IO.DirectoryInfo mydir = System.IO.Directory.CreateDirectory(datarootdirpath);
                string[] sp = myTrack.IdString.Split('-');
                string uptxt = string.Format(@"c:\test\bpm\{0}\{1}-{2}-{3}-{4}_up.txt", mod, mod, pl, sp[0], sp[1]);
                string dwtxt = string.Format(@"c:\test\bpm\{0}\{1}-{2}-{3}-{4}_dw.txt", mod, mod, pl - 1, sp[0], sp[1]);
                BeamDetection(uptxt, true);

                BeamPatternMatch bpm = new BeamPatternMatch(8, 200);
                bpm.ReadTrackDataTxtFile(dwtxt, false);

                bpm.ReadTrackDataTxtFile(uptxt, true);
                bpm.DoPatternMatch();

                stage.WriteLine(String.Format("pattern match dx,dy = {0}, {1}", bpm.GetPeakX() * 0.2625 * 0.001, bpm.GetPeakY() * 0.2625 * 0.001));
                Vector3 BfPoint = mc.GetPoint();
                mc.MoveDistance(bpm.GetPeakX() * 0.2625 * 0.001, VectorId.X);
                mc.Join();
                mc.MoveDistance(-bpm.GetPeakY() * 0.2625 * 0.001, VectorId.Y);
                mc.Join();
                Led led = Led.GetInstance();
                led.AdjustLight(parameterManager);
                Vector3 AfPoint = mc.GetPoint();
                stage.WriteLine(String.Format("Move dx,dy = {0}, {1}", BfPoint.X - AfPoint.X, BfPoint.Y - AfPoint.Y));

            } catch (ArgumentOutOfRangeException) {
                MessageBox.Show("ID numver is not existed。 ");
            } catch (System.Exception) {
                MessageBox.Show("No beam battern。 ");
            }
            // }
            /////////////////////////////SurfaceRecog////////////////////////////////////////////////////////////////////////////
            /*    
                    try {
                        surface.Start(true);
                    } catch (Exception ex) {
                        MessageBox.Show(ex.Message, Properties.Strings.Error);
                    }
                    //mc1.Join();
                   // Led led = Led.GetInstance();
                    //led.AdjustLight(parameterManager);
                     
                } catch (ArgumentOutOfRangeException) {
                    MessageBox.Show("Surface recognitin is fail ");
                }
                 
   // }*/
            ////..............Start Step1.................//(Have to input Determination file)////////////////
            //  try {

            Vector3 initial = mc.GetPoint();///////initialpoint

            /*  double Sh = 0.5 / (surface.UpTop - surface.UpBottom);

              //ここから角度によって撮影間隔を変更するように書き換える。
              double tansi = Math.Sqrt(myTrack.MsDX * myTrack.MsDX + myTrack.MsDY * myTrack.MsDY);
              double theta = Math.Atan(tansi);
              //絶対値の大きさを入れる。dzはマイナスの値になるようにする。
              double dz_price = (40 * Math.Cos(theta) / Sh) / 1000;
              double dz = dz_price * (-1);

              //
              string datarootdirpathw = string.Format(@"C:\test\{0}", myTrack.IdString);
              System.IO.DirectoryInfo mydir_ = System.IO.Directory.CreateDirectory(datarootdirpathw);
              //

              //必要なlistをまとめる
              //List<ImageTaking> LiITUpTrack = new List<ImageTaking>();
              List<OpenCvSharp.CPlusPlus.Point2d> Msdxdy = new List<OpenCvSharp.CPlusPlus.Point2d>();
              Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(myTrack.MsDX, myTrack.MsDY));

              List<OpenCvSharp.CPlusPlus.Point3d> LStage = new List<OpenCvSharp.CPlusPlus.Point3d>();
              List<OpenCvSharp.CPlusPlus.Point> LPeak = new List<OpenCvSharp.CPlusPlus.Point>();//i番目の画像で実際に見つかったトラックの座標。pixel座標で視野中心からの差分。
              List<Point3d> LTrack = new List<Point3d>();//i番目の画像で実際に見つかったトラックの座標のステージ座標

              List<List<ImageTaking>> UpTrackInfo = new List<List<ImageTaking>>();


              //エラー防止のために下側乳剤層で使用するlist等の関数をここに持ってきた。
              double Sh_low;
              Sh_low = 0.5 / (surface.LowTop - surface.LowBottom);
              List<OpenCvSharp.CPlusPlus.Point2d> Msdxdy_Low = new List<OpenCvSharp.CPlusPlus.Point2d>();
              //List<ImageTaking> LiITLowMid = new List<ImageTaking>();
              List<OpenCvSharp.CPlusPlus.Point3d> LStage_Low = new List<OpenCvSharp.CPlusPlus.Point3d>();
              List<OpenCvSharp.CPlusPlus.Point> LPeak_Low = new List<OpenCvSharp.CPlusPlus.Point>();//i番目の画像で実際に見つかったトラックの座標。pixel座標で視野中心からの差分。
              List<Point3d> LTrack_Low = new List<Point3d>();//i番目の画像で実際に見つかったトラックの座標のステージ座標
              List<List<ImageTaking>> LowTrackInfo = new List<List<ImageTaking>>();

              int gotobase = 0;
              int not_detect = 0;

              for (int i = 0; gotobase < 1; i++) {
                  ///////移動して画像処理をしたときに、baseの中に入らないようにする。
                  Vector3 initialpos = mc.GetPoint();
                  double moverange = 7 * (-0.003) + dz;
                  double predpoint = moverange + initialpos.Z;

                  if (predpoint < surface.UpBottom)//もしもbaseに入りそうなら、8枚目の画像がちょうどbaseを撮影するようにdzを調整する。
                   {
                      gotobase = 1;

                      dz = surface.UpBottom - initialpos.Z + 7 * 0.003;
                  }
                  ////////

                  //i != 0のときは、移動して画像を撮影するようにする。
                  if (i != 0) {
                      Vector3 dstpoint = new Vector3(
                          LTrack[i - 1].X + Msdxdy[i].X * dz * Sh,
                          LTrack[i - 1].Y + Msdxdy[i].Y * dz * Sh,
                          LTrack[i - 1].Z + dz
                          );
                      mc.MovePoint(dstpoint);
                      mc.Join();
                  }

                  //今までのtrack追跡プログラムとは異なる角度等の使い方をする。
                  List<ImageTaking> LiITUpMid = TakeSequentialImage(
                      Msdxdy[i].X * Sh,
                      Msdxdy[i].Y * Sh,
                      -0.003,
                      8);

                  LStage.Add(new OpenCvSharp.CPlusPlus.Point3d(LiITUpMid[7].StageCoord.X, LiITUpMid[7].StageCoord.Y, LiITUpMid[7].StageCoord.Z));
                  LiITUpMid[7].img.ImWrite(datarootdirpathw + string.Format(@"\img_l_up_{0}.bmp", i));

                  UpTrackInfo.Add(LiITUpMid);//撮影した8枚の画像と、撮影した位置を記録する。

                  //撮影した画像をここで処理する。
                  List<Mat> binimages = new List<Mat>();
                  for (int t = 0; t <= 7; t++) {
                      Mat bin = (Mat)DogContrastBinalize(LiITUpMid[t].img);

                      double xx = myTrack.MsDX * myTrack.MsDX;
                      double yy = myTrack.MsDY * myTrack.MsDY;
                      if (Math.Sqrt(xx + yy) >= 0.4) {
                          Cv2.Dilate(bin, bin, new Mat());
                      }
                      Cv2.Dilate(bin, bin, new Mat());
                      binimages.Add(bin);
                  }
                  //trackを重ねる処理を入れる。
                  Point2d pixel_cen = TrackDetection(binimages, 256, 220, 3, 3, 4, 90, 3);//画像の8枚目におけるtrackのpixel座標を算出する。

                  //もし検出に失敗した場合はループを抜ける。
                  if (pixel_cen.X == -1 & pixel_cen.Y == -1) {
                      not_detect = 1;
                      goto not_detect_track;
                  }
                  //

                  //検出したpixel座標をstage座標に変換するなどlistに追加する。LPredはおそらくこの手法では必要ない。

                  LPeak.Add(new OpenCvSharp.CPlusPlus.Point(pixel_cen.X - 256, pixel_cen.Y - 220));

                  double firstx = LStage[i].X - LPeak[i].X * 0.000267;
                  double firsty = LStage[i].Y + LPeak[i].Y * 0.000267;
                  double firstz = LStage[i].Z;
                  LTrack.Add(new Point3d(firstx, firsty, firstz));
                  //

                  //ここからは、最小二乗法で角度を算出するプログラムである。
                  //上側乳剤層上面の1回目のtrack検出によって、次のtrackの位置を検出する角度を求める。
                  //その角度が、1回目のtrack検出の結果によって大きな角度にならないように調整をする。

                  if (i == 0) {
                      Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(myTrack.MsDX, myTrack.MsDY));

                  } else {
                      if (i == 1) {
                          List<Point3d> LTrack_ghost = new List<Point3d>();
                          double dzPrev = (LStage[0].Z - surface.UpTop) * Sh;
                          double Lghost_x = LTrack[0].X - Msdxdy[i].X * dzPrev;
                          double Lghost_y = LTrack[0].Y - Msdxdy[i].Y * dzPrev;
                          LTrack_ghost.Add(new Point3d(Lghost_x, Lghost_y, surface.UpTop));//上側乳剤層上面にtrackがあるならどの位置にあるかを算出する。

                          string txtfileName_ltrackghost = datarootdirpathw + string.Format(@"\LTrack_ghost.txt");
                          StreamWriter twriter_ltrackghost = File.CreateText(txtfileName_ltrackghost);
                          twriter_ltrackghost.WriteLine("{0} {1} {2}", LTrack_ghost[0].X, LTrack_ghost[0].Y, LTrack_ghost[0].Z);
                          twriter_ltrackghost.Close();

                          OpenCvSharp.CPlusPlus.Point2d Tangle = ApproximateStraight(Sh, LTrack_ghost[0], LTrack[0], LTrack[1]);
                          Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle.X, Tangle.Y));

                      } else {
                          OpenCvSharp.CPlusPlus.Point2d Tangle = ApproximateStraight(Sh, LTrack[i - 2], LTrack[i - 1], LTrack[i]);
                          Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle.X, Tangle.Y));
                      }
                  }

              }//for　i-loop


              //baseまたぎ
              int ltrack_counter = LTrack.Count();
              int msdxdy_counter = Msdxdy.Count();

              mc.MovePoint(
                  LTrack[ltrack_counter - 1].X + Msdxdy[msdxdy_counter - 1].X * (surface.LowTop - surface.UpBottom),
                  LTrack[ltrack_counter - 1].Y + Msdxdy[msdxdy_counter - 1].Y * (surface.LowTop - surface.UpBottom),
                  surface.LowTop
                  );
              mc.Join();

              //////ここから下gelの処理

              Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Msdxdy[msdxdy_counter - 1].X, Msdxdy[msdxdy_counter - 1].Y));

              //dzが、上側で行った追跡の最後のままであるので、ここでdzを正常の値に戻す。
              dz_price = (40 * Math.Cos(theta) / Sh) / 1000;
              dz = dz_price * (-1);

              int goto_dgel = 0;

              for (int i = 0; goto_dgel < 1; i++) {
                  ///////移動して画像処理をしたときに、下gelの下に入らないようにする。
                  Vector3 initialpos = mc.GetPoint();
                  double moverange = 7 * (-0.003) + dz;
                  double predpoint = moverange + initialpos.Z;

                  if (predpoint < surface.LowBottom)//もしもbaseに入りそうなら、8枚目の画像がちょうど下gelを撮影するようにdzを調整する。
                   {
                      goto_dgel = 1;

                      dz = surface.LowBottom - initialpos.Z + 7 * 0.003;
                  }
                  ////////

                  //i != 0のときは、移動して画像を撮影するようにする。
                  if (i != 0) {
                      Vector3 dstpoint = new Vector3(
                      LTrack_Low[i - 1].X + Msdxdy_Low[i].X * dz * Sh_low,
                      LTrack_Low[i - 1].Y + Msdxdy_Low[i].Y * dz * Sh_low,
                      LTrack_Low[i - 1].Z + dz
                      );
                      mc.MovePoint(dstpoint);
                      mc.Join();
                  }

                  //今までのtrack追跡プログラムとは異なる角度等の使い方をする。
                  List<ImageTaking> LiITLowMid = TakeSequentialImage(
                      Msdxdy[i].X * Sh_low,
                      Msdxdy[i].Y * Sh_low,
                      -0.003,
                      8);

                  LiITLowMid[7].img.ImWrite(datarootdirpathw + string.Format(@"\img_l_low_{0}.bmp", i));
                  LStage_Low.Add(new OpenCvSharp.CPlusPlus.Point3d(LiITLowMid[7].StageCoord.X, LiITLowMid[7].StageCoord.Y, LiITLowMid[7].StageCoord.Z));

                  LowTrackInfo.Add(LiITLowMid);//撮影した8枚の画像と、撮影した位置を記録する。

                  //撮影した画像をここで処理する。
                  List<Mat> binimages = new List<Mat>();
                  for (int t = 0; t <= 7; t++) {
                      Mat bin = (Mat)DogContrastBinalize(LiITLowMid[t].img);

                      double xx = myTrack.MsDX * myTrack.MsDX;
                      double yy = myTrack.MsDY * myTrack.MsDY;
                      if (Math.Sqrt(xx + yy) >= 0.4) {
                          Cv2.Dilate(bin, bin, new Mat());
                      }
                      Cv2.Dilate(bin, bin, new Mat());
                      binimages.Add(bin);
                  }
                  //trackを重ねる処理を入れる。
                  Point2d pixel_cen = TrackDetection(binimages, 256, 220, 3, 3, 4, 90, 3);//画像の8枚目におけるtrackのpixel座標を算出する。

                  //もし検出に失敗した場合はループを抜ける。
                  if (pixel_cen.X == -1 & pixel_cen.Y == -1) {
                      not_detect = 1;
                      goto not_detect_track;
                  }
                  //

                  //検出したpixel座標をstage座標に変換するなどlistに追加する。LPredはおそらくこの手法では必要ない。
                  LPeak_Low.Add(new OpenCvSharp.CPlusPlus.Point(pixel_cen.X - 256, pixel_cen.Y - 220));

                  double firstx = LStage_Low[i].X - LPeak_Low[i].X * 0.000267;
                  double firsty = LStage_Low[i].Y + LPeak_Low[i].Y * 0.000267;
                  double firstz = LStage_Low[i].Z;
                  LTrack_Low.Add(new Point3d(firstx, firsty, firstz));
                  //

                  //ここからは、最小二乗法で角度を算出するプログラムである。
                  //上側乳剤層上面の1回目のtrack検出によって、次のtrackの位置を検出する角度を求める。
                  //その角度が、1回目のtrack検出の結果によって大きな角度にならないように調整をする。
                  if (i == 0) {
                      OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraightBase(Sh, Sh_low, LTrack[ltrack_counter - 2], LTrack[ltrack_counter - 1], LTrack_Low[i], surface);
                      Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                  } else {
                      if (i == 1) {
                          OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraightBase(Sh, Sh_low, LTrack[ltrack_counter - 1], LTrack_Low[i - 1], LTrack_Low[i], surface);
                          Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                      } else {
                          OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraight(Sh_low, LTrack_Low[i - 2], LTrack_Low[i - 1], LTrack_Low[i]);
                          Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                      }
                  }

              }//i_loop

              //
              int ltrack_low_count = LTrack_Low.Count();
              mc.MovePointXY(LTrack_Low[ltrack_low_count - 1].X, LTrack_Low[ltrack_low_count - 1].Y);

              mc.Join();


               //検出に失敗した場合は、ループを抜けてここに来る。
          not_detect_track: ;//検出に失敗したと考えられる地点で画像を取得し、下ゲル下面まで移動する。(現在は下ゲル下面とするが、今後変更する可能性有。)

              if (not_detect != 0) {
                  //写真撮影
                  List<ImageTaking> NotDetect = TakeSequentialImage(
                          Msdxdy[0].X * Sh,
                          Msdxdy[0].Y * Sh,
                          0,
                          1);

                  string txtfileName_t_not_detect = datarootdirpathw + string.Format(@"\not_detect.txt");
                  StreamWriter twriter_t_not_detect = File.CreateText(txtfileName_t_not_detect);
                  for (int i = 0; i < NotDetect.Count; i++) {
                      NotDetect[i].img.ImWrite(datarootdirpathw + string.Format(@"\img_t_not_detect.bmp"));
                      Vector3 p = NotDetect[i].StageCoord;
                      twriter_t_not_detect.WriteLine("{0} {1} {2} {3}", i, p.X, p.Y, p.Z);
                  }
                  twriter_t_not_detect.Close();

                  mc.MovePointZ(surface.LowBottom);

                  mc.Join();
              }

              //file write out
              string txtfileName_sh_up = datarootdirpathw + string.Format(@"\Sh_up.txt");
              StreamWriter twriter_sh_up = File.CreateText(txtfileName_sh_up);
              twriter_sh_up.WriteLine("{0}", Sh);
              twriter_sh_up.Close();

              //file write out
              string txtfileName_t_info_up = datarootdirpathw + string.Format(@"\location_up.txt");
              StreamWriter twriter_t_info_up = File.CreateText(txtfileName_t_info_up);
              for (int i = 0; i < UpTrackInfo.Count; i++) {
                  for (int t = 0; t < UpTrackInfo[i].Count; t++) {
                      UpTrackInfo[i][t].img.ImWrite(datarootdirpathw + string.Format(@"\img_t_info_up_{0}-{1}.bmp", i, t));
                      Vector3 p = UpTrackInfo[i][t].StageCoord;
                      twriter_t_info_up.WriteLine("{0} {1} {2} {3} {4}", i, t, p.X, p.Y, p.Z);
                  }
              }
              twriter_t_info_up.Close();

              string txtfileName_lpeak = datarootdirpathw + string.Format(@"\lpeak_up.txt");
              StreamWriter twriter_lpeak = File.CreateText(txtfileName_lpeak);
              for (int i = 0; i < LPeak.Count(); i++) {
                  OpenCvSharp.CPlusPlus.Point p = LPeak[i];
                  twriter_lpeak.WriteLine("{0} {1} {2}", i, p.X, p.Y);
              }
              twriter_lpeak.Close();

              string txtfileName_ltrack = datarootdirpathw + string.Format(@"\ltrack_up.txt");
              StreamWriter twriter_ltrack = File.CreateText(txtfileName_ltrack);
              for (int i = 0; i < LTrack.Count(); i++) {
                  OpenCvSharp.CPlusPlus.Point3d p = LTrack[i];
                  twriter_ltrack.WriteLine("{0} {1} {2} {3}", i, p.X, p.Y, p.Z);
              }
              twriter_ltrack.Close();

              string txtfileName_msdxdy = datarootdirpathw + string.Format(@"\msdxdy.txt");
              StreamWriter twriter_msdxdy = File.CreateText(txtfileName_msdxdy);
              for (int i = 0; i < Msdxdy.Count(); i++) {
                  OpenCvSharp.CPlusPlus.Point2d p = Msdxdy[i];
                  twriter_msdxdy.WriteLine("{0} {1} {2}", i, p.X, p.Y);
              }
              twriter_msdxdy.Close();


              //file write out
              string txtfileName_sh_low = datarootdirpathw + string.Format(@"\Sh_low.txt");
              StreamWriter twriter_sh_low = File.CreateText(txtfileName_sh_low);
              twriter_sh_low.WriteLine("{0}", Sh_low);
              twriter_sh_low.Close();

              string txtfileName_t_info_low = datarootdirpathw + string.Format(@"\location_low.txt");
              StreamWriter twriter_t_info_low = File.CreateText(txtfileName_t_info_low);
              for (int i = 0; i < LowTrackInfo.Count; i++) {
                  for (int t = 0; t < LowTrackInfo[i].Count; t++) {
                      LowTrackInfo[i][t].img.ImWrite(datarootdirpathw + string.Format(@"\img_t_info_low_{0}-{1}.bmp", i, t));
                      Vector3 p = LowTrackInfo[i][t].StageCoord;
                      twriter_t_info_low.WriteLine("{0} {1} {2} {3} {4}", i, t, p.X, p.Y, p.Z);
                  }
              }
              twriter_t_info_low.Close();

              string txtfileName_lpeak_low = datarootdirpathw + string.Format(@"\lpeak_low.txt");
              StreamWriter twriter_lpeak_low = File.CreateText(txtfileName_lpeak_low);
              for (int i = 0; i < LPeak_Low.Count(); i++) {
                  OpenCvSharp.CPlusPlus.Point p = LPeak_Low[i];
                  twriter_lpeak_low.WriteLine("{0} {1} {2}", i, p.X, p.Y);
              }
              twriter_lpeak_low.Close();

              string txtfileName_ltrack_low = datarootdirpathw + string.Format(@"\ltrack_low.txt");
              StreamWriter twriter_ltrack_low = File.CreateText(txtfileName_ltrack_low);
              for (int i = 0; i < LTrack_Low.Count(); i++) {
                  OpenCvSharp.CPlusPlus.Point3d p = LTrack_Low[i];
                  twriter_ltrack_low.WriteLine("{0} {1} {2} {3}", i, p.X, p.Y, p.Z);
              }
              twriter_ltrack_low.Close();

              string txtfileName_msdxdy_low = datarootdirpathw + string.Format(@"\msdxdy_low.txt");
              StreamWriter twriter_msdxdy_low = File.CreateText(txtfileName_msdxdy_low);
              for (int i = 0; i < Msdxdy_Low.Count(); i++) {
                  OpenCvSharp.CPlusPlus.Point2d p = Msdxdy_Low[i];
                  twriter_msdxdy_low.WriteLine("{0} {1} {2}", i, p.X, p.Y);
              }
              twriter_msdxdy_low.Close();
      //   } catch (Exception ex) {
      //      MessageBox.Show("Cannot follow");*/
            //  * ....................................................
            // }*/
            double Sh = 0.5 / (surface.UpTop - surface.UpBottom);

            //ここから角度によって撮影間隔を変更するように書き換える。
            double tansi = Math.Sqrt(myTrack.MsDX * myTrack.MsDX + myTrack.MsDY * myTrack.MsDY);
            double theta = Math.Atan(tansi);
            //絶対値の大きさを入れる。dzはマイナスの値になるようにする。         
            double dz;

            double dz_price_img = (6 * Math.Cos(theta) / Sh) / 1000;
            double dz_img = dz_price_img * (-1);
            //
            string datarootdirpathw = string.Format(@"C:\test\{0}", myTrack.IdString);
            System.IO.DirectoryInfo mydir_ = System.IO.Directory.CreateDirectory(datarootdirpathw);
            //

            //必要なlistをまとめる
            //List<ImageTaking> LiITUpTrack = new List<ImageTaking>();

            List<OpenCvSharp.CPlusPlus.Point2d> Msdxdy = new List<OpenCvSharp.CPlusPlus.Point2d>();
            Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(myTrack.MsDX, myTrack.MsDY));

            List<OpenCvSharp.CPlusPlus.Point3d> LStage = new List<OpenCvSharp.CPlusPlus.Point3d>();
            List<OpenCvSharp.CPlusPlus.Point> LPeak = new List<OpenCvSharp.CPlusPlus.Point>();//i番目の画像で実際に見つかったトラックの座標。pixel座標で視野中心からの差分。
            List<Point3d> LTrack = new List<Point3d>();//i番目の画像で実際に見つかったトラックの座標のステージ座標

            List<List<ImageTaking>> UpTrackInfo = new List<List<ImageTaking>>();

            //エラー防止のために下ゲルの処理の際に必要なListをここに移動した。
            double Sh_low;
            Sh_low = 0.5 / (surface.LowTop - surface.LowBottom);

            List<OpenCvSharp.CPlusPlus.Point2d> Msdxdy_Low = new List<OpenCvSharp.CPlusPlus.Point2d>();

            //List<ImageTaking> LiITLowMid = new List<ImageTaking>();
            List<OpenCvSharp.CPlusPlus.Point3d> LStage_Low = new List<OpenCvSharp.CPlusPlus.Point3d>();
            List<OpenCvSharp.CPlusPlus.Point> LPeak_Low = new List<OpenCvSharp.CPlusPlus.Point>();//i番目の画像で実際に見つかったトラックの座標。pixel座標で視野中心からの差分。
            List<Point3d> LTrack_Low = new List<Point3d>();//i番目の画像で実際に見つかったトラックの座標のステージ座標
            List<List<ImageTaking>> LowTrackInfo = new List<List<ImageTaking>>();


            //今までのtrack追跡プログラムとは異なる角度等の使い方をする。
            dz_price_img = (6 * Math.Cos(theta) / Sh) / 1000;
            dz_img = dz_price_img * (-1);
            dz = dz_img;

            int gotobase = 0;
            int not_detect = 0;

            for (int i = 0; gotobase < 1; i++) {
                ///////移動して画像処理をしたときに、baseの中に入らないようにする。
                Vector3 initialpos = mc.GetPoint();
                double moverange = 7 * dz_img;
                double predpoint = moverange + initialpos.Z;

                if (predpoint < surface.UpBottom)//もしもbaseに入りそうなら、8枚目の画像がちょうどbaseを撮影するようにdzを調整する。
                {
                    gotobase = 1;

                    dz = surface.UpBottom - initialpos.Z + 7 * dz_price_img;
                }
                ////////

                //gotobase = 1のときは、移動して画像を撮影するようにする。
                if (i != 0) {
                    Vector3 dstpoint = new Vector3(
                        LTrack[i - 1].X + Msdxdy[i].X * dz * Sh,
                        LTrack[i - 1].Y + Msdxdy[i].Y * dz * Sh,
                        LTrack[i - 1].Z + dz
                        );
                    mc.MovePoint(dstpoint);
                    mc.Join();
                }

                List<ImageTaking> LiITUpMid = TakeSequentialImage(
                    Msdxdy[i].X * Sh,
                    Msdxdy[i].Y * Sh,
                    dz_img,
                    8);

                ////画像の保存、座標の保存。
                LStage.Add(new OpenCvSharp.CPlusPlus.Point3d(LiITUpMid[7].StageCoord.X, LiITUpMid[7].StageCoord.Y, LiITUpMid[7].StageCoord.Z));
                LiITUpMid[7].img.ImWrite(datarootdirpathw + string.Format(@"\img_l_up_{0}.bmp", i));

                UpTrackInfo.Add(LiITUpMid);//撮影した8枚の画像と、撮影した位置を記録する。

                //撮影した画像をここで処理する。
                List<Mat> binimages = new List<Mat>();
                for (int t = 0; t <= 7; t++) {
                    Mat bin = (Mat)DogContrastBinalize(LiITUpMid[t].img);

                    double xx = myTrack.MsDX * myTrack.MsDX;
                    double yy = myTrack.MsDY * myTrack.MsDY;
                    if (Math.Sqrt(xx + yy) >= 0.4) {
                        Cv2.Dilate(bin, bin, new Mat());
                    }
                    Cv2.Dilate(bin, bin, new Mat());
                    binimages.Add(bin);
                }

                //trackを重ねる処理を入れる。
                Point2d pixel_cen = TrackDetection(binimages, 256, 220, 3, 3, 4, 90, 3);//画像の8枚目におけるtrackのpixel座標を算出する。

                if (pixel_cen.X == -1 & pixel_cen.Y == -1) {
                    //追跡に失敗した時に最後に検出したtrack座標に移動してから、追跡に失敗した地点の画像を撮影するようにする。
                    //mc.MovePoint(LTrack[i - 1].X, LTrack[i - 1].Y, LTrack[i - 1].Z);
                    mc.Join();

                    not_detect = 1;
                    goto not_detect_track;
                }

                //検出したpixel座標をstage座標に変換するなどlistに追加する。

                LPeak.Add(new OpenCvSharp.CPlusPlus.Point(pixel_cen.X - 256, pixel_cen.Y - 220));

                double firstx = LStage[i].X - LPeak[i].X * 0.000267;
                double firsty = LStage[i].Y + LPeak[i].Y * 0.000267;
                double firstz = LStage[i].Z;
                LTrack.Add(new Point3d(firstx, firsty, firstz));
                //

                //上側乳剤層上面の1回目のtrack検出によって、次のtrackの位置を検出する角度を求める。
                //その角度が、1回目のtrack検出の結果によって大きな角度にならないように調整をする。

                if (i == 0) {
                    Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(myTrack.MsDX, myTrack.MsDY));

                } else if (i == 1) {
                    List<Point3d> LTrack_ghost = new List<Point3d>();
                    double dzPrev = (LStage[0].Z - surface.UpTop) * Sh;
                    double Lghost_x = LTrack[0].X - Msdxdy[i].X * dzPrev;
                    double Lghost_y = LTrack[0].Y - Msdxdy[i].Y * dzPrev;
                    LTrack_ghost.Add(new Point3d(Lghost_x, Lghost_y, surface.UpTop));//上側乳剤層上面にtrackがあるならどの位置にあるかを算出する。

                    string txtfileName_ltrackghost = datarootdirpathw + string.Format(@"\LTrack_ghost.txt");
                    StreamWriter twriter_ltrackghost = File.CreateText(txtfileName_ltrackghost);
                    twriter_ltrackghost.WriteLine("{0} {1} {2}", LTrack_ghost[0].X, LTrack_ghost[0].Y, LTrack_ghost[0].Z);
                    twriter_ltrackghost.Close();

                    OpenCvSharp.CPlusPlus.Point2d Tangle = ApproximateStraight(Sh, LTrack_ghost[0], LTrack[0], LTrack[1]);
                    Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle.X, Tangle.Y));

                } else {
                    OpenCvSharp.CPlusPlus.Point2d Tangle = ApproximateStraight(Sh, LTrack[i - 2], LTrack[i - 1], LTrack[i]);
                    Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle.X, Tangle.Y));
                }


            }//for　i-loop

            //baseまたぎ
            int ltrack_counter = LTrack.Count();
            int msdxdy_counter = Msdxdy.Count();

            mc.MovePoint(
                LTrack[ltrack_counter - 1].X + Msdxdy[msdxdy_counter - 1].X * (surface.LowTop - surface.UpBottom),
                LTrack[ltrack_counter - 1].Y + Msdxdy[msdxdy_counter - 1].Y * (surface.LowTop - surface.UpBottom),
                surface.LowTop
                );
            mc.Join();

            //////ここから下gelの処理
            Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Msdxdy[msdxdy_counter - 1].X, Msdxdy[msdxdy_counter - 1].Y));

            //今までのtrack追跡プログラムとは異なる角度等の使い方をする。
            dz_price_img = (6 * Math.Cos(theta) / Sh_low) / 1000;
            dz_img = dz_price_img * (-1);
            dz = dz_img;

            int goto_dgel = 0;

            for (int i = 0; goto_dgel < 1; i++) {
                ///////移動して画像処理をしたときに、下gelの下に入らないようにする。
                Vector3 initialpos = mc.GetPoint();
                double moverange = 7 * dz_img;
                double predpoint = moverange + initialpos.Z;

                if (predpoint < surface.LowBottom)//もしもbaseに入りそうなら、8枚目の画像がちょうど下gelを撮影するようにdzを調整する。
                {
                    goto_dgel = 1;

                    dz = surface.LowBottom - initialpos.Z + 7 * dz_price_img;
                }
                ////////

                //goto_dgel == 1のときは、移動して画像を撮影するようにする。
                if (i != 0) {
                    Vector3 dstpoint = new Vector3(
                    LTrack_Low[i - 1].X + Msdxdy_Low[i].X * dz * Sh_low,
                    LTrack_Low[i - 1].Y + Msdxdy_Low[i].Y * dz * Sh_low,
                    LTrack_Low[i - 1].Z + dz
                    );
                    mc.MovePoint(dstpoint);
                    mc.Join();
                }

                //今までのtrack追跡プログラムとは異なる角度等の使い方をする。
                List<ImageTaking> LiITLowMid = TakeSequentialImage(
                    Msdxdy[i].X * Sh_low,
                    Msdxdy[i].Y * Sh_low,
                    dz_img,
                    8);

                //画像・座標の記録
                LStage_Low.Add(new OpenCvSharp.CPlusPlus.Point3d(LiITLowMid[7].StageCoord.X, LiITLowMid[7].StageCoord.Y, LiITLowMid[7].StageCoord.Z));
                LiITLowMid[7].img.ImWrite(datarootdirpathw + string.Format(@"\img_l_low_{0}.bmp", i));

                LowTrackInfo.Add(LiITLowMid);//撮影した8枚の画像と、撮影した位置を記録する。

                //撮影した画像をここで処理する。
                List<Mat> binimages = new List<Mat>();
                for (int t = 0; t <= 7; t++) {
                    Mat bin = (Mat)DogContrastBinalize(LiITLowMid[t].img);

                    double xx = myTrack.MsDX * myTrack.MsDX;
                    double yy = myTrack.MsDY * myTrack.MsDY;
                    if (Math.Sqrt(xx + yy) >= 0.4) {
                        Cv2.Dilate(bin, bin, new Mat());
                    }
                    Cv2.Dilate(bin, bin, new Mat());
                    binimages.Add(bin);
                }
                //trackを重ねる処理を入れる。
                Point2d pixel_cen = TrackDetection(binimages, 256, 220, 3, 3, 4, 90, 3);//画像の8枚目におけるtrackのpixel座標を算出する。

                //もし検出に失敗した場合はループを抜ける。
                if (pixel_cen.X == -1 & pixel_cen.Y == -1) {
                    //mc.MovePoint(LTrack_Low[i - 1].X, LTrack_Low[i - 1].Y, LTrack_Low[i - 1].Z);
                    mc.Join();

                    not_detect = 1;
                    goto not_detect_track;
                }
                //

                //検出したpixel座標をstage座標に変換するなどlistに追加する。
                LPeak_Low.Add(new OpenCvSharp.CPlusPlus.Point(pixel_cen.X - 256, pixel_cen.Y - 220));

                double firstx = LStage_Low[i].X - LPeak_Low[i].X * 0.000267;
                double firsty = LStage_Low[i].Y + LPeak_Low[i].Y * 0.000267;
                double firstz = LStage_Low[i].Z;
                LTrack_Low.Add(new Point3d(firstx, firsty, firstz));
                //

                //ここからは、最小二乗法で角度を算出するプログラムである。
                //上側乳剤層上面の1回目のtrack検出によって、次のtrackの位置を検出する角度を求める。
                //その角度が、1回目のtrack検出の結果によって大きな角度にならないように調整をする。
                if (i == 0) {
                    OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraightBase(Sh, Sh_low, LTrack[ltrack_counter - 2], LTrack[ltrack_counter - 1], LTrack_Low[i], surface);
                    Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                } else if (i == 1) {
                    OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraightBase(Sh, Sh_low, LTrack[ltrack_counter - 1], LTrack_Low[i - 1], LTrack_Low[i], surface);
                    Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                } else {
                    OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraight(Sh_low, LTrack_Low[i - 2], LTrack_Low[i - 1], LTrack_Low[i]);
                    Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                }


            }//i_loop

            //
            int ltrack_low_count = LTrack_Low.Count();
            mc.MovePointXY(LTrack_Low[ltrack_low_count - 1].X, LTrack_Low[ltrack_low_count - 1].Y);

            mc.Join();

        //検出に失敗した場合は、ループを抜けてここに来る。
        not_detect_track: ;//検出に失敗したと考えられる地点で画像を取得し、下ゲル下面まで移動する。(現在は下ゲル下面とするが、今後変更する可能性有。)

            if (not_detect != 0) {
                //写真撮影
                List<ImageTaking> NotDetect = TakeSequentialImage(
                        Msdxdy[0].X * Sh,
                        Msdxdy[0].Y * Sh,
                        0,
                        1);

                string txtfileName_t_not_detect = datarootdirpathw + string.Format(@"\not_detect.txt");
                StreamWriter twriter_t_not_detect = File.CreateText(txtfileName_t_not_detect);
                for (int i = 0; i < NotDetect.Count; i++) {
                    NotDetect[i].img.ImWrite(datarootdirpathw + string.Format(@"\img_t_not_detect.bmp"));
                    Vector3 p = NotDetect[i].StageCoord;
                    twriter_t_not_detect.WriteLine("{0} {1} {2} {3}", i, p.X, p.Y, p.Z);
                }
                twriter_t_not_detect.Close();

                mc.MovePointZ(surface.LowBottom);

                mc.Join();
            }


            //file write out up_gel
            string txtfileName_sh_up = datarootdirpathw + string.Format(@"\Sh_up.txt");
            StreamWriter twriter_sh_up = File.CreateText(txtfileName_sh_up);
            twriter_sh_up.WriteLine("{0}", Sh);
            twriter_sh_up.Close();

            //file write out
            string txtfileName_t_info_up = datarootdirpathw + string.Format(@"\location_up.txt");
            StreamWriter twriter_t_info_up = File.CreateText(txtfileName_t_info_up);
            for (int i = 0; i < UpTrackInfo.Count; i++) {
                for (int t = 0; t < UpTrackInfo[i].Count; t++) {
                    UpTrackInfo[i][t].img.ImWrite(datarootdirpathw + string.Format(@"\img_t_info_up_{0}-{1}.bmp", i, t));
                    Vector3 p = UpTrackInfo[i][t].StageCoord;
                    twriter_t_info_up.WriteLine("{0} {1} {2} {3} {4}", i, t, p.X, p.Y, p.Z);
                }
            }
            twriter_t_info_up.Close();

            string txtfileName_lpeak = datarootdirpathw + string.Format(@"\lpeak_up.txt");
            StreamWriter twriter_lpeak = File.CreateText(txtfileName_lpeak);
            for (int i = 0; i < LPeak.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point p = LPeak[i];
                twriter_lpeak.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_lpeak.Close();

            string txtfileName_ltrack = datarootdirpathw + string.Format(@"\ltrack_up.txt");
            StreamWriter twriter_ltrack = File.CreateText(txtfileName_ltrack);
            for (int i = 0; i < LTrack.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point3d p = LTrack[i];
                twriter_ltrack.WriteLine("{0} {1} {2} {3}", i, p.X, p.Y, p.Z);
            }
            twriter_ltrack.Close();

            string txtfileName_msdxdy = datarootdirpathw + string.Format(@"\msdxdy.txt");
            StreamWriter twriter_msdxdy = File.CreateText(txtfileName_msdxdy);
            for (int i = 0; i < Msdxdy.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point2d p = Msdxdy[i];
                twriter_msdxdy.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_msdxdy.Close();


            //file write out low_gel
            string txtfileName_sh_low = datarootdirpathw + string.Format(@"\Sh_low.txt");
            StreamWriter twriter_sh_low = File.CreateText(txtfileName_sh_low);
            twriter_sh_low.WriteLine("{0}", Sh_low);
            twriter_sh_low.Close();

            string txtfileName_t_info_low = datarootdirpathw + string.Format(@"\location_low.txt");
            StreamWriter twriter_t_info_low = File.CreateText(txtfileName_t_info_low);
            for (int i = 0; i < LowTrackInfo.Count; i++) {
                for (int t = 0; t < LowTrackInfo[i].Count; t++) {
                    LowTrackInfo[i][t].img.ImWrite(datarootdirpathw + string.Format(@"\img_t_info_low_{0}-{1}.bmp", i, t));
                    Vector3 p = LowTrackInfo[i][t].StageCoord;
                    twriter_t_info_low.WriteLine("{0} {1} {2} {3} {4}", i, t, p.X, p.Y, p.Z);
                }
            }
            twriter_t_info_low.Close();

            string txtfileName_lpeak_low = datarootdirpathw + string.Format(@"\lpeak_low.txt");
            StreamWriter twriter_lpeak_low = File.CreateText(txtfileName_lpeak_low);
            for (int i = 0; i < LPeak_Low.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point p = LPeak_Low[i];
                twriter_lpeak_low.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_lpeak_low.Close();

            string txtfileName_ltrack_low = datarootdirpathw + string.Format(@"\ltrack_low.txt");
            StreamWriter twriter_ltrack_low = File.CreateText(txtfileName_ltrack_low);
            for (int i = 0; i < LTrack_Low.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point3d p = LTrack_Low[i];
                twriter_ltrack_low.WriteLine("{0} {1} {2} {3}", i, p.X, p.Y, p.Z);
            }
            twriter_ltrack_low.Close();

            string txtfileName_msdxdy_low = datarootdirpathw + string.Format(@"\msdxdy_low.txt");
            StreamWriter twriter_msdxdy_low = File.CreateText(txtfileName_msdxdy_low);
            for (int i = 0; i < Msdxdy_Low.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point2d p = Msdxdy_Low[i];
                twriter_msdxdy_low.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_msdxdy_low.Close();//*/

            //.................Taking Photo in buttom of down layer.......................//

            try {
                string[] sp = myTrack.IdString.Split('-');
                string dwtxt = string.Format(@"c:\test\bpm\{0}\{1}-{2}-{3}-{4}_dw.txt", mod, mod, pl, sp[0], sp[1]);
                string datarootdirpath = string.Format(@"C:\test\bpm\{0}", mod);
                System.IO.DirectoryInfo mydir = System.IO.Directory.CreateDirectory(datarootdirpath);
                System.IO.DirectoryInfo mydir2 = System.IO.Directory.CreateDirectory(datarootdirpath);
                BeamDetection(dwtxt, false);
            } catch (ArgumentOutOfRangeException) {
                MessageBox.Show("ID is not exist ");
            }


            //...............Move to top surfacr of upper layer........................//

            try {
                Vector3 cc = mc.GetPoint();
                double Zp = surface.UpTop;
                mc.MoveTo(new Vector3(cc.X, cc.Y, Zp));
                mc.Join();
            } catch (ArgumentOutOfRangeException) {
                MessageBox.Show("Cannot move to top surface of upperlayer ");
            }

            // .......................................................................//

            sw.Stop();
            string[] sp1 = myTrack.IdString.Split('-');
            string logtxt_ = string.Format(@"c:\test\bpm\{0}\{1}-{2}_log_.txt", mod, mod, pl);
            //string log_ = string.Format("{0} \n", sw.Elapsed);
            string log_ = string.Format("{0} {1} {2} \n", sp1[0], sp1[1], sw.Elapsed);
            StreamWriter swr = new StreamWriter(logtxt_, true, Encoding.ASCII);
            swr.Write(log_);
            swr.Close();

        }//startAutoFollowingStep1Button_Click*/


        // .....////////////////////////////////////////////////////......................



        private void surfRecB_Click(object sender, RoutedEventArgs e) {
            // モータが稼働中であれば停止するかどうかを尋ねる．
            int mod = parameterManager.ModuleNo;
            int pl = parameterManager.PlateNo;
            //MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            TracksManager tm = parameterManager.TracksManager;
            Track myTrack = tm.GetTrack(tm.TrackingIndex);
            Stopwatch swsurf = new Stopwatch();
            // Stopwatch Time = new Stopwatch();
            swsurf.Start();
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            if (mc.IsMoving) {
                MessageBoxResult r = MessageBox.Show(
                    Properties.Strings.SurfaceException01,
                    Properties.Strings.Abort + "?",
                    MessageBoxButton.YesNo);
                if (r == MessageBoxResult.Yes) {
                    mc.AbortMoving();
                } else {
                    return;
                }
            }

            // すでに表面認識が実行中であれば停止するかどうか尋ねる．
            Surface surface = Surface.GetInstance(parameterManager);
            if (surface.IsActive) {
                MessageBoxResult r = MessageBox.Show(
                    Properties.Strings.SurfaceException02,
                    Properties.Strings.Abort + "?",
                    MessageBoxButton.YesNo);
                if (r == MessageBoxResult.Yes) {
                    surface.Abort();
                } else {
                    return;
                }
            }

            try {
                surface.Start(true);

            } catch (Exception ex) {
                MessageBox.Show(ex.Message, Properties.Strings.Error);
            }
            double Time = surface.Time;
            mc.Join();
            swsurf.Stop();
            string[] spsurf = myTrack.IdString.Split('-');
            string logtxt_ = string.Format(@"c:\test\bpm\{0}\{1}-{2}_surftime.txt", mod, mod, pl);
            //string log_ = string.Format("{0} \n", sw.Elapsed);
            string surftime = string.Format("{0} {1} {2} {3} \n", spsurf[0], spsurf[1], swsurf.Elapsed, Time);
            StreamWriter swr = new StreamWriter(logtxt_, true, Encoding.ASCII);
            swr.Write(surftime);
            swr.Close();

        }


        private void startAutoFollowingNewStep1Button_Click(object sender, RoutedEventArgs e) {


            System.Diagnostics.Stopwatch worktime = new System.Diagnostics.Stopwatch();//Stopwatch
            worktime.Start();

            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            TracksManager tm = parameterManager.TracksManager;
            Track myTrack = tm.GetTrack(tm.TrackingIndex);
            Surface surface = Surface.GetInstance(parameterManager);


            //for up layer

            double Sh = 0.5 / (surface.UpTop - surface.UpBottom);
            double tansi = Math.Sqrt(myTrack.MsDX * myTrack.MsDX + myTrack.MsDY * myTrack.MsDY);
            double theta = Math.Atan(tansi);
 
            double dz;
            double dz_price_img = (6 * Math.Cos(theta) / Sh) / 1000;
            double dz_img = dz_price_img * (-1);

            string datarootdirpath = string.Format(@"C:\MKS_test\{0}", myTrack.IdString);//Open forder to store track information
            System.IO.DirectoryInfo mydir = System.IO.Directory.CreateDirectory(datarootdirpath);

            List<OpenCvSharp.CPlusPlus.Point2d> Msdxdy = new List<OpenCvSharp.CPlusPlus.Point2d>();
            Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(myTrack.MsDX, myTrack.MsDY));

            List<OpenCvSharp.CPlusPlus.Point3d> LStage = new List<OpenCvSharp.CPlusPlus.Point3d>();
            List<OpenCvSharp.CPlusPlus.Point> LPeak = new List<OpenCvSharp.CPlusPlus.Point>();
            List<Point3d> LTrack = new List<Point3d>();
            List<List<ImageTaking>> UpTrackInfo = new List<List<ImageTaking>>();

            //for down layer................................................................

            double Sh_low;
            Sh_low = 0.5 / (surface.LowTop - surface.LowBottom);

            List<OpenCvSharp.CPlusPlus.Point2d> Msdxdy_Low = new List<OpenCvSharp.CPlusPlus.Point2d>();
            List<OpenCvSharp.CPlusPlus.Point3d> LStage_Low = new List<OpenCvSharp.CPlusPlus.Point3d>();
            List<OpenCvSharp.CPlusPlus.Point> LPeak_Low = new List<OpenCvSharp.CPlusPlus.Point>();
            List<Point3d> LTrack_Low = new List<Point3d>();
            List<List<ImageTaking>> LowTrackInfo = new List<List<ImageTaking>>();
            dz_price_img = (6 * Math.Cos(theta) / Sh) / 1000;
            dz_img = dz_price_img * (-1);
            dz = dz_img;
            int gotobase = 0;
            int not_detect = 0;


            for (int i = 0; gotobase < 1; i++) {

                Vector3 initialpos = mc.GetPoint();
                double moverange = 9 * dz_img;
                double predpoint = moverange + initialpos.Z;

                if (predpoint < surface.UpBottom) {
                    gotobase = 1;

                    dz = surface.UpBottom - initialpos.Z + 9 * dz_price_img;
                }


                //gotobase = 1のときは、移動して画像を撮影するようにする。
                if (i != 0) {
                    Vector3 dstpoint = new Vector3(
                        LTrack[i - 1].X + Msdxdy[i].X * dz * Sh,
                        LTrack[i - 1].Y + Msdxdy[i].Y * dz * Sh,
                        LTrack[i - 1].Z + dz
                        );
                    mc.MovePoint(dstpoint);
                    mc.Join();
                }

                List<ImageTaking> LiITUpMid = TakeSequentialImage( //image taking
                    Msdxdy[i].X * Sh,//Dx
                    Msdxdy[i].Y * Sh,//Dy
                    dz_img,//Dz
                    10);//number of images


                LStage.Add(new OpenCvSharp.CPlusPlus.Point3d(LiITUpMid[9].StageCoord.X, LiITUpMid[9].StageCoord.Y, LiITUpMid[9].StageCoord.Z));
                LiITUpMid[9].img.ImWrite(datarootdirpath + string.Format(@"\img_l_up_{0}.bmp", i));
                UpTrackInfo.Add(LiITUpMid);


                List<Mat> binimages = new List<Mat>();
                for (int t = 0; t <= 9; t++) {
                    Mat bin = (Mat)DogContrastBinalize(LiITUpMid[t].img);

                    double xx = myTrack.MsDX * myTrack.MsDX;
                    double yy = myTrack.MsDY * myTrack.MsDY;
                    if (Math.Sqrt(xx + yy) >= 0.4) {//////////////////????????????????????
                        Cv2.Dilate(bin, bin, new Mat());
                    }
                    Cv2.Dilate(bin, bin, new Mat());
                    binimages.Add(bin);
                }

                //trackを重ねる処理を入れる。
                Point2d pixel_cen = TrackDetection(binimages, 256, 220, 3, 3, 4, 90, 6);//, true);
                //Point2d pixel_cen = TrackDetection(binimages, 256, 220, 3, 3, 4, 90, true);

                if (pixel_cen.X == -1 & pixel_cen.Y == -1) {
                    mc.Join();
                    not_detect = 1;
                    goto not_detect_track;

                }



                LPeak.Add(new OpenCvSharp.CPlusPlus.Point(pixel_cen.X - 256, pixel_cen.Y - 220));

                double firstx = LStage[i].X - LPeak[i].X * 0.000267;
                double firsty = LStage[i].Y + LPeak[i].Y * 0.000267;
                double firstz = LStage[i].Z;
                LTrack.Add(new Point3d(firstx, firsty, firstz));
                //

                //上側乳剤層上面の1回目のtrack検出によって、次のtrackの位置を検出する角度を求める。
                //その角度が、1回目のtrack検出の結果によって大きな角度にならないように調整をする。

                if (i == 0) {
                    Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(myTrack.MsDX, myTrack.MsDY));

                } else if (i == 1) {
                    List<Point3d> LTrack_ghost = new List<Point3d>();
                    double dzPrev = (LStage[0].Z - surface.UpTop) * Sh;
                    double Lghost_x = LTrack[0].X - Msdxdy[i].X * dzPrev;
                    double Lghost_y = LTrack[0].Y - Msdxdy[i].Y * dzPrev;
                    LTrack_ghost.Add(new Point3d(Lghost_x, Lghost_y, surface.UpTop));//上側乳剤層上面にtrackがあるならどの位置にあるかを算出する。

                    string txtfileName_ltrackghost = datarootdirpath + string.Format(@"\LTrack_ghost.txt");
                    StreamWriter twriter_ltrackghost = File.CreateText(txtfileName_ltrackghost);
                    twriter_ltrackghost.WriteLine("{0} {1} {2}", LTrack_ghost[0].X, LTrack_ghost[0].Y, LTrack_ghost[0].Z);
                    twriter_ltrackghost.Close();

                    OpenCvSharp.CPlusPlus.Point2d Tangle = ApproximateStraight(Sh, LTrack_ghost[0], LTrack[0], LTrack[1]);
                    Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle.X, Tangle.Y));

                } else {
                    OpenCvSharp.CPlusPlus.Point2d Tangle = ApproximateStraight(Sh, LTrack[i - 2], LTrack[i - 1], LTrack[i]);
                    Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle.X, Tangle.Y));
                }


            }//for　i-loop

            //baseまたぎ
            int ltrack_counter = LTrack.Count();
            int msdxdy_counter = Msdxdy.Count();

            mc.MovePoint(
                LTrack[ltrack_counter - 1].X + Msdxdy[msdxdy_counter - 1].X * (surface.LowTop - surface.UpBottom),
                LTrack[ltrack_counter - 1].Y + Msdxdy[msdxdy_counter - 1].Y * (surface.LowTop - surface.UpBottom),
                surface.LowTop
                );
            mc.Join();

            //////ここから下gelの処理
            Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Msdxdy[msdxdy_counter - 1].X, Msdxdy[msdxdy_counter - 1].Y));

            //今までのtrack追跡プログラムとは異なる角度等の使い方をする。
            dz_price_img = (6 * Math.Cos(theta) / Sh_low) / 1000;
            dz_img = dz_price_img * (-1);
            dz = dz_img;

            int goto_dgel = 0;

            for (int i = 0; goto_dgel < 1; i++) {
                ///////移動して画像処理をしたときに、下gelの下に入らないようにする。
                Vector3 initialpos = mc.GetPoint();
                double moverange = 9 * dz_img;
                double predpoint = moverange + initialpos.Z;

                if (predpoint < surface.LowBottom)//もしもbaseに入りそうなら、8枚目の画像がちょうど下gelを撮影するようにdzを調整する。
                {
                    goto_dgel = 1;

                    dz = surface.LowBottom - initialpos.Z + 9 * dz_price_img;
                }
                ////////

                //goto_dgel == 1のときは、移動して画像を撮影するようにする。
                if (i != 0) {
                    Vector3 dstpoint = new Vector3(
                    LTrack_Low[i - 1].X + Msdxdy_Low[i].X * dz * Sh_low,
                    LTrack_Low[i - 1].Y + Msdxdy_Low[i].Y * dz * Sh_low,
                    LTrack_Low[i - 1].Z + dz
                    );
                    mc.MovePoint(dstpoint);
                    mc.Join();
                }

                //今までのtrack追跡プログラムとは異なる角度等の使い方をする。
                List<ImageTaking> LiITLowMid = TakeSequentialImage(
                    Msdxdy[i].X * Sh_low,
                    Msdxdy[i].Y * Sh_low,
                    dz_img,
                    10);

                //画像・座標の記録
                LStage_Low.Add(new OpenCvSharp.CPlusPlus.Point3d(LiITLowMid[9].StageCoord.X, LiITLowMid[9].StageCoord.Y, LiITLowMid[9].StageCoord.Z));
                LiITLowMid[9].img.ImWrite(datarootdirpath + string.Format(@"\img_l_low_{0}.png", i));

                LowTrackInfo.Add(LiITLowMid);//撮影した8枚の画像と、撮影した位置を記録する。

                //撮影した画像をここで処理する。
                List<Mat> binimages = new List<Mat>();
                for (int t = 0; t <= 9; t++) {
                    Mat bin = (Mat)DogContrastBinalize(LiITLowMid[t].img);

                    double xx = myTrack.MsDX * myTrack.MsDX;
                    double yy = myTrack.MsDY * myTrack.MsDY;
                    if (Math.Sqrt(xx + yy) >= 0.4) {
                        Cv2.Dilate(bin, bin, new Mat());
                    }
                    Cv2.Dilate(bin, bin, new Mat());
                    binimages.Add(bin);
                }
                //trackを重ねる処理を入れる。
                Point2d pixel_cen = TrackDetection(binimages, 256, 220, 3, 3, 4, 90, 6);//, true);//画像の8枚目におけるtrackのpixel座標を算出する。

                //もし検出に失敗した場合はループを抜ける。
                if (pixel_cen.X == -1 & pixel_cen.Y == -1) {
                    //mc.MovePoint(LTrack_Low[i - 1].X, LTrack_Low[i - 1].Y, LTrack_Low[i - 1].Z);
                    mc.Join();

                    not_detect = 1;
                    goto not_detect_track;
                }
                //

                //検出したpixel座標をstage座標に変換するなどlistに追加する。
                LPeak_Low.Add(new OpenCvSharp.CPlusPlus.Point(pixel_cen.X - 256, pixel_cen.Y - 220));

                double firstx = LStage_Low[i].X - LPeak_Low[i].X * 0.000267;
                double firsty = LStage_Low[i].Y + LPeak_Low[i].Y * 0.000267;
                double firstz = LStage_Low[i].Z;
                LTrack_Low.Add(new Point3d(firstx, firsty, firstz));
                //

                //ここからは、最小二乗法で角度を算出するプログラムである。
                //上側乳剤層上面の1回目のtrack検出によって、次のtrackの位置を検出する角度を求める。
                //その角度が、1回目のtrack検出の結果によって大きな角度にならないように調整をする。
                if (i == 0) {
                    OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraightBase(Sh, Sh_low, LTrack[ltrack_counter - 2], LTrack[ltrack_counter - 1], LTrack_Low[i], surface);
                    Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                } else if (i == 1) {
                    OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraightBase(Sh, Sh_low, LTrack[ltrack_counter - 1], LTrack_Low[i - 1], LTrack_Low[i], surface);
                    Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                } else {
                    OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraight(Sh_low, LTrack_Low[i - 2], LTrack_Low[i - 1], LTrack_Low[i]);
                    Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                }


            }//i_loop

            //
            int ltrack_low_count = LTrack_Low.Count();
            mc.MovePointXY(LTrack_Low[ltrack_low_count - 1].X, LTrack_Low[ltrack_low_count - 1].Y);

            mc.Join();

              //検出に失敗した場合は、ループを抜けてここに来る。
        not_detect_track: ;//検出に失敗したと考えられる地点で画像を取得し、下ゲル下面まで移動する。(現在は下ゲル下面とするが、今後変更する可能性有。)

            if (not_detect != 0) {
                //写真撮影
                List<ImageTaking> NotDetect = TakeSequentialImage(
                        Msdxdy[0].X * Sh,
                        Msdxdy[0].Y * Sh,
                        0,
                        1);

                string txtfileName_t_not_detect = datarootdirpath + string.Format(@"\not_detect.txt");
                StreamWriter twriter_t_not_detect = File.CreateText(txtfileName_t_not_detect);
                for (int i = 0; i < NotDetect.Count; i++) {
                    NotDetect[i].img.ImWrite(datarootdirpath + string.Format(@"\img_t_not_detect.bmp"));
                    Vector3 p = NotDetect[i].StageCoord;
                    twriter_t_not_detect.WriteLine("{0} {1} {2} {3}", i, p.X, p.Y, p.Z);
                }
                twriter_t_not_detect.Close();

                mc.MovePointZ(surface.LowBottom);

                mc.Join();
            }

            //file write out up_gel
            string txtfileName_surface = datarootdirpath + string.Format(@"\surface.txt");
            StreamWriter twriter_surface = File.CreateText(txtfileName_surface);
            twriter_surface.WriteLine("{0} {1} {2} {3}", surface.UpTop, surface.UpBottom, surface.LowTop, surface.LowBottom);
            twriter_surface.Close();


            string txtfileName_sh_up = datarootdirpath + string.Format(@"\Sh_up.txt");
            StreamWriter twriter_sh_up = File.CreateText(txtfileName_sh_up);
            twriter_sh_up.WriteLine("{0}", Sh);
            twriter_sh_up.Close();

            //file write out
            string txtfileName_t_info_up = datarootdirpath + string.Format(@"\location_up.txt");
            StreamWriter twriter_t_info_up = File.CreateText(txtfileName_t_info_up);
            for (int i = 0; i < UpTrackInfo.Count; i++) {
                for (int t = 0; t < UpTrackInfo[i].Count; t++) {
                    UpTrackInfo[i][t].img.ImWrite(datarootdirpath + string.Format(@"\img_t_info_up_{0}-{1}.bmp", i, t));
                    Vector3 p = UpTrackInfo[i][t].StageCoord;
                    twriter_t_info_up.WriteLine("{0} {1} {2} {3} {4}", i, t, p.X, p.Y, p.Z);
                }
            }
            twriter_t_info_up.Close();

            string txtfileName_lpeak = datarootdirpath + string.Format(@"\lpeak_up.txt");
            StreamWriter twriter_lpeak = File.CreateText(txtfileName_lpeak);
            for (int i = 0; i < LPeak.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point p = LPeak[i];
                twriter_lpeak.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_lpeak.Close();

            string txtfileName_ltrack = datarootdirpath + string.Format(@"\ltrack_up.txt");
            StreamWriter twriter_ltrack = File.CreateText(txtfileName_ltrack);
            for (int i = 0; i < LTrack.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point3d p = LTrack[i];
                twriter_ltrack.WriteLine("{0} {1} {2} {3}", i, p.X, p.Y, p.Z);
            }
            twriter_ltrack.Close();

            string txtfileName_msdxdy = datarootdirpath + string.Format(@"\msdxdy.txt");
            StreamWriter twriter_msdxdy = File.CreateText(txtfileName_msdxdy);
            for (int i = 0; i < Msdxdy.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point2d p = Msdxdy[i];
                twriter_msdxdy.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_msdxdy.Close();


            //file write out low_gel
            string txtfileName_sh_low = datarootdirpath + string.Format(@"\Sh_low.txt");
            StreamWriter twriter_sh_low = File.CreateText(txtfileName_sh_low);
            twriter_sh_low.WriteLine("{0}", Sh_low);
            twriter_sh_low.Close();

            string txtfileName_t_info_low = datarootdirpath + string.Format(@"\location_low.txt");
            StreamWriter twriter_t_info_low = File.CreateText(txtfileName_t_info_low);
            for (int i = 0; i < LowTrackInfo.Count; i++) {
                for (int t = 0; t < LowTrackInfo[i].Count; t++) {
                    LowTrackInfo[i][t].img.ImWrite(datarootdirpath + string.Format(@"\img_t_info_low_{0}-{1}.bmp", i, t));
                    Vector3 p = LowTrackInfo[i][t].StageCoord;
                    twriter_t_info_low.WriteLine("{0} {1} {2} {3} {4}", i, t, p.X, p.Y, p.Z);
                }
            }
            twriter_t_info_low.Close();

            string txtfileName_lpeak_low = datarootdirpath + string.Format(@"\lpeak_low.txt");
            StreamWriter twriter_lpeak_low = File.CreateText(txtfileName_lpeak_low);
            for (int i = 0; i < LPeak_Low.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point p = LPeak_Low[i];
                twriter_lpeak_low.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_lpeak_low.Close();

            string txtfileName_ltrack_low = datarootdirpath + string.Format(@"\ltrack_low.txt");
            StreamWriter twriter_ltrack_low = File.CreateText(txtfileName_ltrack_low);
            for (int i = 0; i < LTrack_Low.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point3d p = LTrack_Low[i];
                twriter_ltrack_low.WriteLine("{0} {1} {2} {3}", i, p.X, p.Y, p.Z);
            }
            twriter_ltrack_low.Close();

            string txtfileName_msdxdy_low = datarootdirpath + string.Format(@"\msdxdy_low.txt");
            StreamWriter twriter_msdxdy_low = File.CreateText(txtfileName_msdxdy_low);
            for (int i = 0; i < Msdxdy_Low.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point2d p = Msdxdy_Low[i];
                twriter_msdxdy_low.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_msdxdy_low.Close();

            worktime.Stop();

            string txtfileName_time_log = datarootdirpath + string.Format(@"\time_log.txt");
            StreamWriter twriter_time_log = File.CreateText(txtfileName_time_log);
            twriter_time_log.WriteLine("{0} {1} {2}", myTrack.MsDX, myTrack.MsDY, worktime.Elapsed);
            twriter_time_log.Close();

            GoTopUp();
            mc.Join();

        }//startAutoFollowingStep1_not_jumpButton_Click

        //}


        public struct pozi {
            public Point3d img;//画像の撮影した地点の座標。
            public Point2d peak;//beamのpixel座標。
            public Point3d stage;//beamのstage座標。

        }

        static OpenCvSharp.CPlusPlus.Point TrackDetection_verold(List<Mat> mats, int px, int py, int shiftx = 2, int shifty = 2, int shiftpitch = 4, int windowsize = 40, int phthresh = 5, bool debugflag = false) {
            int x0 = px - 256;
            int y0 = py - 220;

            List<rawmicrotrack> rms = new List<rawmicrotrack>();

            // Point2d pixel_cen = TrackDetection(binimages, 256, 220, 3, 3, 4, 90, 3);


            int counter = 0;
            for (int ax = -shiftx; ax <= shiftx; ax++) {
                for (int ay = -shifty; ay <= shifty; ay++) {
                    using (Mat big = Mat.Zeros(600, 600, MatType.CV_8UC1))
                    using (Mat imgMask = Mat.Zeros(big.Height, big.Width, MatType.CV_8UC1)) {

                        //make the size of mask
                        int ystart = big.Height / 2 + y0 - windowsize / 2;
                        int yend = big.Height / 2 + y0 + windowsize / 2;
                        int xstart = big.Width / 2 + x0 - windowsize / 2;
                        int xend = big.Width / 2 + x0 + windowsize / 2;

                        //make mask as shape of rectangle. by use of opencv
                        OpenCvSharp.CPlusPlus.Rect recMask = new OpenCvSharp.CPlusPlus.Rect(xstart, ystart, windowsize, windowsize);
                        Cv2.Rectangle(imgMask, recMask, 255, -1);//brightness=1, fill

                        for (int p = 0; p < mats.Count; p++) {
                            int startx = big.Width / 2 - mats[p].Width / 2 + (int)(p * ax * shiftpitch / 8.0);
                            int starty = big.Height / 2 - mats[p].Height / 2 + (int)(p * ay * shiftpitch / 8.0);
                            Cv2.Add(
                                big[starty, starty + mats[p].Height, startx, startx + mats[p].Width],
                                mats[p],
                                big[starty, starty + mats[p].Height, startx, startx + mats[p].Width]);
                        }

                        using (Mat big_c = big.Clone()) {

                            Cv2.Threshold(big, big, phthresh, 255, ThresholdType.ToZero);
                            Cv2.BitwiseAnd(big, imgMask, big);

                            //Mat roi = big[ystart, yend , xstart, xend];//メモリ領域がシーケンシャルにならないから輪郭抽出のときに例外が出る。

                            if (debugflag == true) {//
                                //bigorg.ImWrite(String.Format(@"{0}_{1}_{2}.png",counter,ax,ay));
                                //Mat roiwrite = roi.Clone() * 30;
                                //roiwrite.ImWrite(String.Format(@"roi_{0}_{1}_{2}.png", counter, ax, ay));
                                Cv2.Rectangle(big_c, recMask, 255, 1);//brightness=1, fill
                                Cv2.ImShow("big_cx30", big_c * 30);
                                Cv2.ImShow("bigx30", big * 30);
                                //Cv2.ImShow("imgMask", imgMask);
                                //Cv2.ImShow("roi", roi * 30);
                                Cv2.WaitKey(0);
                            }
                        }//using big_c

                        using (CvMemStorage storage = new CvMemStorage())
                        using (CvContourScanner scanner = new CvContourScanner(big.ToIplImage(), storage, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxSimple)) {
                            foreach (CvSeq<CvPoint> c in scanner) {
                                CvMoments mom = new CvMoments(c, false);
                                if (c.ElemSize < 2) continue;
                                if (mom.M00 < 1.0) continue;
                                double mx = mom.M10 / mom.M00;
                                double my = mom.M01 / mom.M00;
                                rawmicrotrack rm = new rawmicrotrack();
                                rm.ax = ax;
                                rm.ay = ay;
                                rm.cx = (int)(mx - big.Width / 2);
                                rm.cy = (int)(my - big.Height / 2);
                                rm.pv = (int)(mom.M00);
                                rms.Add(rm);
                                //Console.WriteLine(string.Format("{0}   {1} {2}   {3} {4}", rm.pv, ax, ay, rm.cx, rm.cy ));
                            }
                        }//using contour

                        //big_c.Dispose();

                        counter++;


                    }//using Mat
                }//ay
            }//ax



            OpenCvSharp.CPlusPlus.Point trackpos = new OpenCvSharp.CPlusPlus.Point(0, 0);
            if (rms.Count > 0) {
                rawmicrotrack rm = new rawmicrotrack();
                double meancx = 0;
                double meancy = 0;
                double meanax = 0;
                double meanay = 0;
                double meanph = 0;
                double meanpv = 0;
                double sumpv = 0;

                for (int i = 0; i < rms.Count; i++) {
                    meanpv += rms[i].pv * rms[i].pv;
                    meancx += rms[i].cx * rms[i].pv;
                    meancy += rms[i].cy * rms[i].pv;
                    meanax += rms[i].ax * rms[i].pv;
                    meanay += rms[i].ay * rms[i].pv;
                    sumpv += rms[i].pv;
                }

                meancx /= sumpv;//重心と傾きを輝度値で重み付き平均
                meancy /= sumpv;
                meanax /= sumpv;
                meanay /= sumpv;
                meanpv /= sumpv;

                trackpos = new OpenCvSharp.CPlusPlus.Point(
                    (int)(meancx) + 256 - meanax * shiftpitch,
                    (int)(meancy) + 220 - meanay * shiftpitch
                    );

                double anglex = (meanax * shiftpitch * 0.267) / (3.0 * 7.0 * 2.2);
                double angley = (meanay * shiftpitch * 0.267) / (3.0 * 7.0 * 2.2);
                Console.WriteLine(string.Format("{0:f4} {1:f4}", anglex, angley));
            } else {
                trackpos = new OpenCvSharp.CPlusPlus.Point(-1, -1);
            }


            return trackpos;
        }//track detection


        private void BeamFollow(Track myTrack, int mod, int pl, bool dubflag) {

            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            Surface surface = Surface.GetInstance(parameterManager);
            Vector3 initial = mc.GetPoint();
            //            TimeLogger tl = new TimeLogger("savepath");


            //
            //string uptxt = string.Format(@"c:\GTR_test\bpm\{0}_{1}_up.txt", initial.X, initial.Y);
            string uptxt = string.Format(@"c:\GTR_test\bpm\{0}_{1}_up.txt", 10, 10);
            BeamDetection(uptxt, true);
            List<Point2d> BPM_pix = new List<Point2d>();//beamのピクセル座標を格納するListである。

            string line;

            System.IO.StreamReader file = new System.IO.StreamReader(uptxt);
            while ((line = file.ReadLine()) != null) {
                string[] data = line.Split(' ');
                double sx = double.Parse(data[0]);
                double sy = double.Parse(data[1]);
                BPM_pix.Add(new Point2d(sx, sy));
            }
            file.Close();

            //ここからは、画像の中心(256,220)に近いいくつかのbeamの座標を選択し、それを追跡するように修正する。
            List<Point2d> some_bpm = new List<Point2d>();
            List<Point3d> point = new List<Point3d>();
            List<Point2d> point_10 = new List<Point2d>();

            for (int i = 0; i < BPM_pix.Count(); i++) {
                List<Point3d> point2 = new List<Point3d>();
                for (int r = 0; r < BPM_pix.Count(); r++) {
                    Point3d p = new Point3d();
                    double dx = BPM_pix[i].X - BPM_pix[r].X;
                    double dy = BPM_pix[i].Y - BPM_pix[r].Y;
                    double dr = Math.Sqrt(dx * dx + dy * dy);

                    if (dr < 7) {//この7という数字は、windowsizeが一辺10ピクセルのため、全体で見た時に7ピクセル離れていれば良いだろうと判断し、このようにした。
                        p.X = 10;
                        p.Y = 10;
                        p.Z = 10;
                        point2.Add(p);
                    }
                }//for r

                if (point2.Count() == 1) {
                    Point2d bem = new Point2d();
                    bem.X = BPM_pix[i].X;
                    bem.Y = BPM_pix[i].Y;
                    point_10.Add(bem);
                }
            
            }//for i


            //この中の処理は、座標を検出するために使ったが、検出した座標が近すぎると追跡の際にバグの発生源となる恐れがあるので、コメントアウトしてある。

            //for (int i = 0; i < BPM_pix.Count(); i++) {
            //    Point3d p = new Point3d();
            //    double dx = 256 - BPM_pix[i].X;
            //    double dy = 220 - BPM_pix[i].X;
            //    double dr = Math.Sqrt(dx * dx + dy * dy);

            //    p.X = BPM_pix[i].X;
            //    p.Y = BPM_pix[i].Y;
            //    p.Z = dr;
            //    point.Add(p);
            //}

            //point.Sort((a, b) => a.Z.CompareTo(b.Z));

            //ここまで
            int bemcount = 0;
            if (point_10.Count() >= 5) {
                bemcount = 5;
            } else {
                bemcount = point_10.Count();
            }

            for (int i = 0; i < bemcount; i++)//ここで、領域における分け方も含めてbeamを選択できるようにする。
            {
                some_bpm.Add(new Point2d(point_10[i].X, point_10[i].Y));
            }
            //ただ、とりあえずこれで作動はするであろう形になった。



            //パターンマッチに関する内容を挿入する。
            //


            List<List<pozi>> LBeam = new List<List<pozi>>();
            List<List<pozi>> LBeam_Low = new List<List<pozi>>();

            List<pozi> c2 = new List<pozi>();
            List<Point2d> PM_result = some_bpm;//パターンマッチの結果から取得したbeamのpixel座標
            for (int a = 0; a < PM_result.Count(); a++) {
                pozi c3 = new pozi();
                c3.img.X = initial.X;
                c3.img.Y = initial.Y;
                c3.img.Z = initial.Z;

                c3.peak = PM_result[a];

                double firstx = c3.img.X - (c3.peak.X - 256) * 0.000267;
                double firsty = c3.img.Y + (c3.peak.Y - 220) * 0.000267;
                double firstz = c3.img.Z;

                c3.stage = new Point3d(firstx, firsty, firstz);

                c2.Add(c3);
            }
            LBeam.Add(c2);//第一層目でのbeamの情報をぶち込む。

            //for up layer

            int number_of_images = 10;
            int hits = 6;

            double Sh = 0.5 / (surface.UpTop - surface.UpBottom);
            double theta = Math.Atan(0);

            double dz;
            double dz_price_img = (6 * Math.Cos(theta) / Sh) / 1000;
            double dz_img = dz_price_img * (-1);

            string datarootdirpath = string.Format(@"C:\GTR_test\{0}", myTrack.IdString);//Open forder to store track information
            System.IO.DirectoryInfo mydir = System.IO.Directory.CreateDirectory(datarootdirpath);

            List<OpenCvSharp.CPlusPlus.Point2d> Msdxdy = new List<OpenCvSharp.CPlusPlus.Point2d>();//stage移動のための角度を入れるList。
            Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(0.0, 0.0));//最初はbeamが垂直に原子核乾板に照射されていると考えて（0.0,0.0）を入れた。

            List<OpenCvSharp.CPlusPlus.Point3d> LStage = new List<OpenCvSharp.CPlusPlus.Point3d>();//ImageTakeingで撮影した画像の最後の画像の座標を入れるList。
            List<OpenCvSharp.CPlusPlus.Point3d> LCenter = new List<OpenCvSharp.CPlusPlus.Point3d>();//検出したbemaのずれから算出した、本来の画像の中心点のstage座標。

            List<List<ImageTaking>> UpTrackInfo = new List<List<ImageTaking>>();

            //for down layer................................................................
            //            tl.Rec("down");
            double Sh_low;
            Sh_low = 0.5 / (surface.LowTop - surface.LowBottom);

            List<OpenCvSharp.CPlusPlus.Point2d> Msdxdy_Low = new List<OpenCvSharp.CPlusPlus.Point2d>();
            List<OpenCvSharp.CPlusPlus.Point3d> LStage_Low = new List<OpenCvSharp.CPlusPlus.Point3d>();

            List<OpenCvSharp.CPlusPlus.Point3d> LCenter_Low = new List<OpenCvSharp.CPlusPlus.Point3d>();//検出したbemaのずれから算出した、本来の画像の中心点のstage座標。

            List<List<ImageTaking>> LowTrackInfo = new List<List<ImageTaking>>();


            dz_price_img = (6 * Math.Cos(theta) / Sh) / 1000;
            dz_img = dz_price_img * (-1);
            dz = dz_img;
            int gotobase = 0;
            int not_detect = 0;

            for (int i = 0; gotobase < 1; i++) {
                Vector3 initialpos = mc.GetPoint();
                double moverange = (number_of_images - 1) * dz_img;
                double predpoint = moverange + initialpos.Z;

                if (predpoint < surface.UpBottom) {
                    gotobase = 1;

                    dz = surface.UpBottom - initialpos.Z + (number_of_images - 1) * dz_price_img;
                }

                //gotobase = 1のときは、移動して画像を撮影するようにする。
                if (i != 0)//このままでOK
                {
                    Vector3 dstpoint = new Vector3(
                        LCenter[i - 1].X + Msdxdy[i].X * dz * Sh,
                        LCenter[i - 1].Y + Msdxdy[i].Y * dz * Sh,
                        LCenter[i - 1].Z + dz
                        );
                    mc.MovePoint(dstpoint);
                    mc.Join();
                }

                List<ImageTaking> LiITUpMid = TakeSequentialImage( //image taking
                    Msdxdy[i].X * Sh,//Dx
                    Msdxdy[i].Y * Sh,//Dy
                    dz_img,//Dz
                    number_of_images);//number of images


                LStage.Add(new OpenCvSharp.CPlusPlus.Point3d(
                    LiITUpMid[number_of_images - 1].StageCoord.X,
                    LiITUpMid[number_of_images - 1].StageCoord.Y,
                    LiITUpMid[number_of_images - 1].StageCoord.Z
                    ));//撮影した画像の最後の画像の座標を LStage に代入する。

                LiITUpMid[number_of_images - 1].img.ImWrite(datarootdirpath + string.Format(@"\img_l_up_{0}.png", i));//最後の画像だけ別で保存。
                UpTrackInfo.Add(LiITUpMid);//撮影した画像すべてを UpTrackInfo に代入。

                List<Mat> binimages = new List<Mat>();//撮影した画像に対して画像処理をかける。
                for (int t = 0; t <= number_of_images - 1; t++) {
                    Mat bin = (Mat)DogContrastBinalize(LiITUpMid[t].img);
                    Cv2.Dilate(bin, bin, new Mat());
                    binimages.Add(bin);
                }

                List<pozi> beam_data = new List<pozi>();//各stepごとで検出したbeamそれぞれのデータを格納する。
                List<Point2d> MSDXDY_BEAM = new List<Point2d>();//それぞれのbeamから算出した角度を格納するList。
                for (int r = 0; r < LBeam[0].Count(); r++) //検出したbeamの数だけ処理を行うようにする。
                {
                    pozi beam_pozi = new pozi();
                    beam_pozi.img = LStage[i];//画像の撮影場所を格納する。

                    //trackを重ねる処理を入れる。
                    Point2d beam_peak = TrackDetection_verold(//シフトしないようにして、処理を行うようにしよう。
                        binimages,
                        (int)LBeam[i][r].peak.X,
                        (int)LBeam[i][r].peak.Y,
                        0,//shiftx もともと　3
                        0,//shifty もともと　3
                        4,
                        10,//windowsize もともと　90
                        hits);// true);

                    if (beam_peak.X == -1 & beam_peak.Y == -1) {//検出できなかった時にどのような処理を行うのかを考えたほうがいいだろうな。
                        mc.Join();
                        not_detect = 1;

                        //goto not_detect_track; とりあえずコメントアウトしておく
                    }

                    beam_pozi.peak.X = beam_peak.X;
                    beam_pozi.peak.Y = beam_peak.Y;

                    double firstx = beam_pozi.img.X - (beam_pozi.peak.X - 256) * 0.000267;
                    double firsty = beam_pozi.img.Y + (beam_pozi.peak.Y - 220) * 0.000267;
                    double firstz = beam_pozi.img.Z;

                    beam_pozi.stage = new Point3d(firstx, firsty, firstz);

                    beam_data.Add(beam_pozi);

                    if (i == 0) {
                        MSDXDY_BEAM.Add(new OpenCvSharp.CPlusPlus.Point2d(Msdxdy[i].X, Msdxdy[i].Y));
                    } else {
                        if (i == 1) {
                            Point3d LTrack_ghost = new Point3d();
                            double dzPrev = (LBeam[i][r].stage.Z - surface.UpTop) * Sh;
                            double Lghost_x = LBeam[i][r].stage.X - Msdxdy[i].X * dzPrev;
                            double Lghost_y = LBeam[i][r].stage.Y - Msdxdy[i].Y * dzPrev;
                            LTrack_ghost = new Point3d(Lghost_x, Lghost_y, surface.UpTop);//上側乳剤層上面にtrackがあるならどの位置にあるかを算出する。

                            OpenCvSharp.CPlusPlus.Point2d Tangle = ApproximateStraight(Sh, LTrack_ghost, LBeam[0][r].stage, LBeam[1][r].stage);//ここを2点で行うようにする。
                            MSDXDY_BEAM.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle.X, Tangle.Y));

                        } else {
                            OpenCvSharp.CPlusPlus.Point2d Tangle = ApproximateStraight(Sh, LBeam[i + 1][r].stage, LBeam[i][r].stage, LBeam[i - 1][r].stage);
                            MSDXDY_BEAM.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle.X, Tangle.Y));
                        }
                    }

                }//r_loop

                Point2d Ms_esti = new Point2d();
                double Ms_x = new double();
                double Ms_y = new double();

                int pix_dX = new int();
                int pix_dY = new int();

                for (int q = 0; q < MSDXDY_BEAM.Count(); q++) //ここで個々のbeamの角度を平均してstageの移動角度を算出する。
                {
                    Ms_x += MSDXDY_BEAM[q].X;
                    Ms_y += MSDXDY_BEAM[q].Y;

                    pix_dX += (int)LBeam[i][q - 1].peak.X - (int)LBeam[i][q].peak.X;//q - 1 がおかしい。ここで何をしたかったのかを思い出そう。
                    pix_dY += (int)LBeam[i][q - 1].peak.Y - (int)LBeam[i][q].peak.Y;//予想した地点とのピクセルのズレかな？
                }
                Ms_x = Ms_x / MSDXDY_BEAM.Count();
                Ms_y = Ms_y / MSDXDY_BEAM.Count();
                Ms_esti = new Point2d(Ms_x, Ms_y);
                Msdxdy.Add(Ms_esti);//算出した角度をぶち込む。
                LBeam.Add(beam_data);

                pix_dX = pix_dX / MSDXDY_BEAM.Count();//ずれたピクセル量
                pix_dY = pix_dY / MSDXDY_BEAM.Count();//ずれたピクセル量
                double cenX = LStage[i].X - pix_dX * 0.000267;
                double cenY = LStage[i].Y + pix_dY * 0.000267;
                double cenZ = LStage[i].Z;

                LCenter.Add(new Point3d(cenX, cenY, cenZ));//検出したそれぞれのbeamのズレから算出したパターンマッチの際の中心座標(stage)。

            }//for　i-loop


            //baseまたぎ
            int lcen_counter = LCenter.Count();
            int msdxdy_counter = Msdxdy.Count();

            mc.MovePoint(
                LCenter[lcen_counter - 1].X + Msdxdy[msdxdy_counter - 1].X * (surface.LowTop - surface.UpBottom),
                LCenter[lcen_counter - 1].Y + Msdxdy[msdxdy_counter - 1].Y * (surface.LowTop - surface.UpBottom),
                surface.LowTop
                );
            mc.Join();




            //////ここから下gelの処理
            Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Msdxdy[msdxdy_counter - 1].X, Msdxdy[msdxdy_counter - 1].Y));
            int lbeam_counter = LBeam.Count();
            LBeam_Low.Add(LBeam[lbeam_counter - 1]);

            //今までのtrack追跡プログラムとは異なる角度等の使い方をする。
            dz_price_img = (6 * Math.Cos(theta) / Sh_low) / 1000;
            dz_img = dz_price_img * (-1);
            dz = dz_img;

            int goto_dgel = 0;

            for (int i = 0; goto_dgel < 1; i++) {
                ///////移動して画像処理をしたときに、下gelの下に入らないようにする。
                Vector3 initialpos = mc.GetPoint();
                double moverange = (number_of_images - 1) * dz_img;
                double predpoint = moverange + initialpos.Z;

                if (predpoint < surface.LowBottom)//もしもbaseに入りそうなら、8枚目の画像がちょうど下gelを撮影するようにdzを調整する。
                {
                    goto_dgel = 1;

                    dz = surface.LowBottom - initialpos.Z + (number_of_images - 1) * dz_price_img;
                }
                ////////

                //goto_dgel == 1のときは、移動して画像を撮影するようにする。
                if (i != 0) {
                    Vector3 dstpoint = new Vector3(
                    LCenter_Low[i - 1].X + Msdxdy_Low[i].X * dz * Sh_low,
                    LCenter_Low[i - 1].Y + Msdxdy_Low[i].Y * dz * Sh_low,
                    LCenter_Low[i - 1].Z + dz
                    );
                    mc.MovePoint(dstpoint);
                    mc.Join();
                }

                //今までのtrack追跡プログラムとは異なる角度等の使い方をする。
                List<ImageTaking> LiITLowMid = TakeSequentialImage(
                    Msdxdy[i].X * Sh_low,
                    Msdxdy[i].Y * Sh_low,
                    dz_img,
                    number_of_images);

                //画像・座標の記録
                LStage_Low.Add(new OpenCvSharp.CPlusPlus.Point3d(
                    LiITLowMid[number_of_images - 1].StageCoord.X,
                    LiITLowMid[number_of_images - 1].StageCoord.Y,
                    LiITLowMid[number_of_images - 1].StageCoord.Z));

                LiITLowMid[number_of_images - 1].img.ImWrite(datarootdirpath + string.Format(@"\img_l_low_{0}.png", i));

                LowTrackInfo.Add(LiITLowMid);//撮影した8枚の画像と、撮影した位置を記録する。

                //撮影した画像をここで処理する。
                List<Mat> binimages = new List<Mat>();
                for (int t = 0; t <= number_of_images - 1; t++) {
                    Mat bin = (Mat)DogContrastBinalize(LiITLowMid[t].img);
                    Cv2.Dilate(bin, bin, new Mat());
                    binimages.Add(bin);
                }

                List<pozi> beam_data_low = new List<pozi>();//各stepごとで検出したbeamそれぞれのデータを格納する。
                List<Point2d> MSDXDY_BEAM_LOW = new List<Point2d>();//それぞれのbeamから算出した角度を格納するList。
                for (int r = 0; r < LBeam_Low[0].Count(); r++) {
                    pozi beam_pozi = new pozi();
                    beam_pozi.img = LStage_Low[i];//画像の撮影場所を格納する。

                    //trackを重ねる処理を入れる。
                    Point2d beam_peak = TrackDetection_verold(
                        binimages,
                        (int)LBeam_Low[i][r].peak.X,
                        (int)LBeam_Low[i][r].peak.Y,
                        0,
                        0,
                        4,
                        10,
                        hits);// true);

                    if (beam_peak.X == -1 & beam_peak.Y == -1) {
                        mc.Join();
                        not_detect = 1;

                        //goto not_detect_track; とりあえずコメントアウトしておく
                    }

                    beam_pozi.peak.X = beam_peak.X;
                    beam_pozi.peak.Y = beam_peak.Y;

                    double firstx = beam_pozi.img.X - (beam_pozi.peak.X - 256) * 0.000267;
                    double firsty = beam_pozi.img.Y + (beam_pozi.peak.Y - 220) * 0.000267;
                    double firstz = beam_pozi.img.Z;

                    beam_pozi.stage = new Point3d(firstx, firsty, firstz);

                    beam_data_low.Add(beam_pozi);

                    if (i == 0) {
                        OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraightBase(
                            Sh,
                            Sh_low,
                            LBeam[lbeam_counter - 2][r].stage,
                            LBeam[lbeam_counter - 1][r].stage,
                            LBeam_Low[i][r].stage,
                            surface);
                        MSDXDY_BEAM_LOW.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                    } else if (i == 1) {
                        OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraightBase(
                            Sh,
                            Sh_low,
                            LBeam_Low[lbeam_counter - 1][r].stage,
                            LBeam_Low[i - 1][r].stage,
                            LBeam_Low[i][r].stage,
                            surface);
                        MSDXDY_BEAM_LOW.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                    } else {
                        OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraight(
                            Sh_low,
                            LBeam_Low[i - 2][r].stage,
                            LBeam_Low[i - 1][r].stage,
                            LBeam_Low[i][r].stage);
                        MSDXDY_BEAM_LOW.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                    }

                }//r_loop

                Point2d Ms_esti = new Point2d();
                double Ms_x = new double();
                double Ms_y = new double();

                int pix_dX = new int();
                int pix_dY = new int();

                for (int q = 0; q < MSDXDY_BEAM_LOW.Count(); q++) //ここで個々のbeamの角度を平均してstageの移動角度を算出する。
                {
                    Ms_x += MSDXDY_BEAM_LOW[q].X;
                    Ms_y += MSDXDY_BEAM_LOW[q].Y;

                    pix_dX += (int)LBeam_Low[i][q - 1].peak.X - (int)LBeam_Low[i][q].peak.X;
                    pix_dY += (int)LBeam_Low[i][q - 1].peak.Y - (int)LBeam_Low[i][q].peak.Y;
                }

                Ms_x = Ms_x / MSDXDY_BEAM_LOW.Count();
                Ms_y = Ms_y / MSDXDY_BEAM_LOW.Count();
                Ms_esti = new Point2d(Ms_x, Ms_y);
                Msdxdy_Low.Add(Ms_esti);//算出した角度をぶち込む。
                LBeam_Low.Add(beam_data_low);

                pix_dX = pix_dX / MSDXDY_BEAM_LOW.Count();//ずれたピクセル量
                pix_dY = pix_dY / MSDXDY_BEAM_LOW.Count();//ずれたピクセル量
                double cenX = LStage_Low[i].X - pix_dX * 0.000267;
                double cenY = LStage_Low[i].Y + pix_dY * 0.000267;
                double cenZ = LStage_Low[i].Z;

                LCenter_Low.Add(new Point3d(cenX, cenY, cenZ));//検出したそれぞれのbeamのズレから算出したパターンマッチの際の中心座標(stage)。

            }//i_loop

            //
            int lcen_low_count = LCenter_Low.Count();
            mc.MovePointXY(LCenter_Low[lcen_low_count - 1].X, LCenter_Low[lcen_low_count - 1].Y);

            mc.Join();

            //検出に失敗した場合は、ループを抜けてここに来る。

            //file write out up_gel
            string txtfileName_sh_up = datarootdirpath + string.Format(@"\Sh_up.txt");
            StreamWriter twriter_sh_up = File.CreateText(txtfileName_sh_up);
            twriter_sh_up.WriteLine("{0}", Sh);
            twriter_sh_up.Close();

            //file write out
            string txtfileName_t_info_up = datarootdirpath + string.Format(@"\location_up.txt");
            StreamWriter twriter_t_info_up = File.CreateText(txtfileName_t_info_up);
            for (int i = 0; i < UpTrackInfo.Count; i++) {
                for (int t = 0; t < UpTrackInfo[i].Count; t++) {
                    UpTrackInfo[i][t].img.ImWrite(datarootdirpath + string.Format(@"\img_t_info_up_{0}-{1}.png", i, t));
                    Vector3 p = UpTrackInfo[i][t].StageCoord;
                    twriter_t_info_up.WriteLine("{0} {1} {2} {3} {4}", i, t, p.X, p.Y, p.Z);
                }
            }
            twriter_t_info_up.Close();

            string txtfileName_lbeam = datarootdirpath + string.Format(@"\lbeam_up.txt");
            StreamWriter twriter_lbeam = File.CreateText(txtfileName_lbeam);
            for (int i = 0; i < LBeam.Count(); i++) {
                for (int r = 0; r < LBeam[i].Count(); r++) {
                    twriter_lbeam.WriteLine("{0} {1} BeamPeak: {2} {3} LBeam(Stage): {4} {5} {6}",
                        i,
                        r,
                        LBeam[i][r].peak.X,
                        LBeam[i][r].peak.Y,
                        LBeam[i][r].stage.X,
                        LBeam[i][r].stage.Y,
                        LBeam[i][r].stage.Z);
                }

            }
            twriter_lbeam.Close();

            string txtfileName_LCenter = datarootdirpath + string.Format(@"\LCenter_up.txt");
            StreamWriter twriter_LCenter = File.CreateText(txtfileName_LCenter);
            for (int i = 0; i < LCenter.Count(); i++) {
                twriter_LCenter.WriteLine("{0} {1} {2}", LCenter[i].X, LCenter[i].Y, LCenter[i].Z);
            }
            twriter_LCenter.Close();

            string txtfileName_msdxdy = datarootdirpath + string.Format(@"\msdxdy.txt");
            StreamWriter twriter_msdxdy = File.CreateText(txtfileName_msdxdy);
            for (int i = 0; i < Msdxdy.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point2d p = Msdxdy[i];
                twriter_msdxdy.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_msdxdy.Close();

            //file write out low_gel
            string txtfileName_sh_low = datarootdirpath + string.Format(@"\Sh_low.txt");
            StreamWriter twriter_sh_low = File.CreateText(txtfileName_sh_low);
            twriter_sh_low.WriteLine("{0}", Sh_low);
            twriter_sh_low.Close();

            string txtfileName_t_info_low = datarootdirpath + string.Format(@"\location_low.txt");
            StreamWriter twriter_t_info_low = File.CreateText(txtfileName_t_info_low);
            for (int i = 0; i < LowTrackInfo.Count; i++) {
                for (int t = 0; t < LowTrackInfo[i].Count; t++) {
                    LowTrackInfo[i][t].img.ImWrite(datarootdirpath + string.Format(@"\img_t_info_low_{0}-{1}.png", i, t));
                    Vector3 p = LowTrackInfo[i][t].StageCoord;
                    twriter_t_info_low.WriteLine("{0} {1} {2} {3} {4}", i, t, p.X, p.Y, p.Z);
                }
            }
            twriter_t_info_low.Close();


            string txtfileName_lbeam_low = datarootdirpath + string.Format(@"\lbeam_low.txt");
            StreamWriter twriter_lbeam_low = File.CreateText(txtfileName_lbeam_low);
            for (int i = 0; i < LBeam_Low.Count(); i++) {
                for (int r = 0; r < LBeam_Low[i].Count(); r++) {
                    twriter_lbeam.WriteLine("{0} {1} BeamPeak: {2} {3} LBeam(Stage): {4} {5} {6}",
                        i,
                        r,
                        LBeam_Low[i][r].peak.X,
                        LBeam_Low[i][r].peak.Y,
                        LBeam_Low[i][r].stage.X,
                        LBeam_Low[i][r].stage.Y,
                        LBeam_Low[i][r].stage.Z);
                }
            }
            twriter_lbeam_low.Close();


            string txtfileName_LCenter_low = datarootdirpath + string.Format(@"\LCenter_low.txt");
            StreamWriter twriter_LCenter_low = File.CreateText(txtfileName_LCenter_low);
            for (int i = 0; i < LCenter_Low.Count(); i++) {
                twriter_LCenter_low.WriteLine("{0} {1} {2}", LCenter_Low[i].X, LCenter_Low[i].Y, LCenter_Low[i].Z);
            }
            twriter_LCenter_low.Close();

            string txtfileName_msdxdy_low = datarootdirpath + string.Format(@"\msdxdy_low.txt");
            StreamWriter twriter_msdxdy_low = File.CreateText(txtfileName_msdxdy_low);
            for (int i = 0; i < Msdxdy_Low.Count(); i++) {
                OpenCvSharp.CPlusPlus.Point2d p = Msdxdy_Low[i];
                twriter_msdxdy_low.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_msdxdy_low.Close();




        }

        private void BeamFollowButton_Click(object sender, RoutedEventArgs e) {

            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            TracksManager tm = parameterManager.TracksManager;
            Track myTrack = tm.GetTrack(tm.TrackingIndex);
            Surface surface = Surface.GetInstance(parameterManager);
            int mod = parameterManager.ModuleNo;
            int pl = parameterManager.PlateNo;
            bool dubflag;
            Led led_ = Led.GetInstance();

            //string[] sp1 = myTrack.IdString.Split('-');

            //string logtxt = string.Format(@"C:\GTR_test\WorkingTime\{0}\{1}-{2}_TTracking_newversion.txt", mod, mod, pl);
            //SimpleLogger SL1 = new SimpleLogger(logtxt, sp1[0], sp1[1]);

            //// Tracking in emulsion
            //SL1.Info("Tracking Start");
            
            BeamFollow(myTrack, mod, pl, false);

            
            GoTopUp();
            mc.Join();
            Thread.Sleep(100);


        }

    }

}