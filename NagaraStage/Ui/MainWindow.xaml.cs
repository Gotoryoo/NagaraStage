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
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            GridMarksSpiralSearch gmss = GridMarksSpiralSearch.GetInstance(parameterManager);//後でbeamdetection消した後になんとかする。
            manager.Enqueue(gmss);
            manager.Start();
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
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            SeqHyperFine shf = SeqHyperFine.GetInstance(parameterManager);//後でbeamdetection消した後になんとかする。
            manager.Enqueue(shf);
            manager.Start();
        }

        private void BeamDetectionButton_Click(object sender, RoutedEventArgs e) {
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            BeamDetention bd = BeamDetention.GetInstance(parameterManager);//後でbeamdetection消した後になんとかする。
            manager.Enqueue(bd);
            manager.Start();
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
        
        private void start_auto_following_Click(object sender, RoutedEventArgs e) {
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            StartAutoFollowing saf = StartAutoFollowing.GetInstance(parameterManager);
            manager.Enqueue(saf);
            manager.Start();
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
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            Start_Following sf = Start_Following.GetInstance(parameterManager);
            manager.Enqueue(sf);
            manager.Start();
        }

        private void OverallAbortButton_Click(object sender, RoutedEventArgs e) {
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            OverallAbort oaa = OverallAbort.GetInstance(parameterManager);
            manager.Enqueue(oaa);
            manager.Start();
        }

        private void GotoUpTop_button(object sender, RoutedEventArgs e) {
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            GotoUpTop gtut = GotoUpTop.GetInstance(parameterManager);
            manager.Enqueue(gtut);
            manager.Start();
        }

        private void coordinate_record(object sender, RoutedEventArgs e) {
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            Coordinate_record cr = Coordinate_record.GetInstance(parameterManager);
            manager.Enqueue(cr);
            manager.Start();
        }

        private void coordinate_init(object sender, RoutedEventArgs e) {
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            Coordinate_init ci = Coordinate_init.GetInstance(parameterManager);
            manager.Enqueue(ci);
            manager.Start();
        }
               
        private void Full_Automatic_Tracking_button(object sender, RoutedEventArgs e) {
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            Full_Automatic_Tracking fat = Full_Automatic_Tracking.GetInstance(parameterManager);
            manager.Enqueue(fat);
            manager.Start();
        }

// ##############################################################################################
                        //AUTOMATIC TRACKING FOR PL#2

        private void Full_Automatic_Tracking_PL2_button(object sender, RoutedEventArgs e) {
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            Full_Automatic_Tracking_PL2 fatpl2 = Full_Automatic_Tracking_PL2.GetInstance(parameterManager);
            manager.Enqueue(fatpl2);
            manager.Start();
        }
         
        private void GoNearGrid_CorrecteTrackPosition_button(object sender, RoutedEventArgs e) {
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            GoNearGrid_CorrecteTrackPosition ggctp = GoNearGrid_CorrecteTrackPosition.GetInstance(parameterManager);
            manager.Enqueue(ggctp);
            manager.Start();
        }

        private void Start_button(object sender, RoutedEventArgs e) {
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            StartButton sb = StartButton.GetInstance(parameterManager);
            manager.Enqueue(sb);
            manager.Start();
        }//Start_button

        private void surfRecB_Click(object sender, RoutedEventArgs e) {
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            SurfRecB srb = SurfRecB.GetInstance(parameterManager);
            manager.Enqueue(srb);
            manager.Start();
        }//surfRecB_Click

        private void startAutoFollowingNewStep1Button_Click(object sender, RoutedEventArgs e) {
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            StartAutoFollowingNewStep1 step1 = StartAutoFollowingNewStep1.GetInstance(parameterManager);
            manager.Enqueue(step1);
            manager.Start();
        }//startAutoFollowingStep1_not_jumpButton_Click

        private void BeamFollowButton_Click(object sender, RoutedEventArgs e)
        {
            ActivityManager manager = ActivityManager.GetInstance(parameterManager);
            BeamFollow bf = BeamFollow.GetInstance(parameterManager);
            manager.Enqueue(bf);
            manager.Start();
        }

    }

}