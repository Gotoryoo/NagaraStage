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
using Microsoft.Windows.Controls.Ribbon;
using System.Windows.Media.Animation;

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
            mc.InitializeMotorControlBoard(MechaAxisAddress.XAddress);
            mc.InitializeMotorControlBoard(MechaAxisAddress.YAddress);
            mc.InitializeMotorControlBoard(MechaAxisAddress.ZAddress);
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

        private void takeOneImageButton_Click(object sender, RoutedEventArgs e) {
            Camera camera = Camera.GetInstance();
            if (!camera.IsRunning) {
                MessageBox.Show(Properties.Strings.CameraNotWork, Properties.Strings.Error);
                return;
            }
            if (workspace.Content as Stage == null) {
                return;
            }

            ImagePreviewer previewer = new ImagePreviewer(this);
            previewer.ImageSource = camera.Image;
            previewer.ShowDialog();
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

        private void surfaceVerifButton_Click(object sender, RoutedEventArgs e) {
            SurfaceVerif verif = new SurfaceVerif(this);
            verif.NextControl = Workspace.PresentControl;
            SetElementOnWorkspace(verif);
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
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            mc.MovePoint(0, 0, 0);
#endif
            Properties.Settings.Default.Save();
        }

        private void takePluralityButton_Click(object sender, RoutedEventArgs e) {
            ShootingStage shootingStage = new ShootingStage(this, ShootingMode.Plurality);
            shootingStage.NextControl = Workspace.PresentControl;
            SetElementOnWorkspace(shootingStage);
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

            if (!gridmarkSearch.IsGridMarkedToStart) {
                MessageBox.Show(
                    Properties.Strings.GridMarkNotDefinedMin,
                    Properties.Strings.Error);
                continuFlag = false;
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
            // レンズが50倍に設定されていない場合は例外を返すようにしたいがやり方が分からん(20140724)

            //現在地からスパイラルサーチ30視野でグリッドマークを検出する
            //検出したら視野の真ん中に持ってくる
            try {
                MotorControler mc = MotorControler.GetInstance(parameterManager);
                IGridMarkRecognizer GridMarkRecognizer = coordManager;
                mc.SetSpiralCenterPoint();
                Led led = Led.GetInstance();
                Vector2 encoderPoint = mc.GetPoint();
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
                Vector2 encoderPoint = mc.GetPoint();
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

            string datfileName = string.Format(@"c:\img\{0}.dat",System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"));
            BinaryWriter writer = new BinaryWriter(File.Open(datfileName, FileMode.Create));

            for (int i = 0; i < 10; i++ ){
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


            Vector2 encoderPoint = mc.GetPoint();
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

        private void start_auto_following_Click(object sender, RoutedEventArgs e)
        {
            TracksManager tm = parameterManager.TracksManager;
            Track myTrack = tm.GetTrack(tm.TrackingIndex);
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();


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
        
            while(mc.GetPoint().Z >= upbottom + 0.030)//上ゲル内での連続移動
            {
                Follow();
            }
            
            if(mc.GetPoint().Z >= upbottom+0.024)
            {
            double now_x = mc.GetPoint().X;
            double now_y = mc.GetPoint().Y;
            double now_z = mc.GetPoint().Z;
            mc.MovePoint(now_x - common_dx * (now_z - (upbottom + 0.024)) * 2.2, now_y - common_dy * (now_z - (upbottom + 0.024)) * 2.2, upbottom + 0.024);
            }
            else
            {
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
                ,lowtop);//Base下側への移動











        }

        static int number_of_follow;

        private void Follow()
        {
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

            if (number_of_follow == 1) 
            {
                common_dx = myTrack.MsDX;
                common_dy = myTrack.MsDY;
            } 
            else if (number_of_follow == 2) 
            {
                common_dx = myTrack.MsDX + ((0.265625 * over_dx * 3) / (0.024 * 2.2 * 1000));
                common_dy = myTrack.MsDY - ((0.265625 * over_dy * 3) / (0.024 * 2.2 * 1000));
            } 
            else 
            {
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
            if(parameterManager.Magnification != 40) {
                MessageBox.Show(String.Format(Properties.Strings.LensTypeException02, 40));
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
                for (int delta_yy = -1; delta_yy <= 1; delta_yy++) 
                {
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
                           for(int i = 0; i < two_set.Count; i++)
                           {
                               if(i==0||i==1)
                               {
                               Part_img[i][
                                        0
                                    , 440
                                    , 0
                                    , 512
                                    ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                               }
                               else if(i==2||i==3)
                               {
                               Part_img[i][
                                        0 + Math.Abs(delta_yy)  //yの値のスタート地点
                                    , 440 + Math.Abs(delta_yy)  //yの値のゴール地点
                                    , 0 + Math.Abs(delta_xx)    //xの値のスタート地点
                                    , 512 + Math.Abs(delta_xx)  //xの値のゴール地点
                                    ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                               }
                               else if(i==4||i==5)
                               {
                                   Part_img[i][
                                            0 + 2 * Math.Abs(delta_yy)  //yの値のスタート地点
                                        , 440 + 2 * Math.Abs(delta_yy) //yの値のゴール地点
                                        , 0 + 2 * Math.Abs(delta_xx)    //xの値のスタート地点
                                        , 512 + 2 * Math.Abs(delta_xx)  //xの値のゴール地点
                                        ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                               }
                               else if(i==6||i==7)
                               {
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
                            for (int i = 0; i < two_set.Count; i++) 
                            {
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
                            for (int i = 0; i < two_set.Count; i++) 
                            {
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
                                    stage.WriteLine(String.Format("mx={0:f2} , my={1:f2} , dx={2:f2} , dy={3:f2} , M={4:f2}", mx, my,delta_xx ,delta_yy,mom.M00));
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
    }

}