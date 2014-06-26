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
using System.Globalization;
using Microsoft.Windows.Controls.Ribbon;
using System.Windows.Media.Animation;

using NagaraStage;
using NagaraStage.IO;
using NagaraStage.IO.Driver;
using NagaraStage.Activities;
using NagaraStage.Parameter;
using NagaraStage.ImageEnhancement;

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
                    case "fullScanTab":
                        fullScanTab.IsSelected = true;
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
                fullScanTab.IsEnabled = value;
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
            surfaceRecognizeAbortButton.IsEnabled = false;
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

        private void surfaceRecognizeButton_Click(object sender, RoutedEventArgs e) {
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
                surface.Start();
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
                surfaceRecognizeAbortButton.IsEnabled = true;
            }), null);            
        }
        
        private void surface_Exited(object sender, ActivityEventArgs e) {
            Dispatcher.BeginInvoke(new Action(delegate() {
                // 制限していたUIを元に戻す
                IsTabsEnabled = true;
                controlGroup.IsEnabled = true;
                shootingGrop.IsEnabled = true;
                surfaceRecognizeAbortButton.IsEnabled = false;
            }), null);            
        }

        private void surfaceRecognizeConfigButton_Click(object sender, RoutedEventArgs e) {
            SurfaceConfig surfaceConfig = new SurfaceConfig(this);
            surfaceConfig.NextControl = SurfaceConfig.PresentControl;
            SetElementOnWorkspace(surfaceConfig);
        }

        private void surfaceVerifButton_Click(object sender, RoutedEventArgs e) {
            SurfaceVerif verif = new SurfaceVerif(this);
            verif.NextControl = Workspace.PresentControl;          
            SetElementOnWorkspace(verif);
        }

        private void fastSurfaceRecognizeButton_Click(object sender, RoutedEventArgs e) {
            Surface surface = Surface.GetInstance(parameterManager);
            try {
                surface.Start(true);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, Properties.Strings.Error);
            }
        }        

        private void surfaceRecognizeAbortButton_Click(object sender, RoutedEventArgs e) {
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
            mc.MovePointXY(p);
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
        /// どでユーザーに確認をとります．</para>
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
    }

}