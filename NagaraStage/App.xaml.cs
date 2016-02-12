using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Globalization;

using NagaraStage.Parameter;
using NagaraStage.IO;

using log4net;

/**
 * @mainpage
 * <h1>Nagara Stage</h1>
 * 
 * <p>Nagara Stageは、E07実験の解析のために開発した顕微鏡制御のプログラムです．
 * <p>E373実験の解析に使ったプログラム：AutoStageはVisual Basic6で書かれていました．このNagaraStageは, .Net Framework4.0というプラットフォームを使っています．言語はC\#です．
 * @author Hirokazu Yokoyama <o1007410@edu.gifu-u.ac.jp>
 * @author Junya Yoshida <jyoshida@gifu-u.ac.jp>
 */

namespace NagaraStage {
    /// <summary>
    /// アプリケーションのエントリーポイントクラスです．Main関数を持っており，
    /// このクラスがアプリケーションを開始させます．
    /// <para>Interaction logic for App.xaml</para>
    /// </summary>
    public partial class App : Application {
        private static System.Threading.Mutex mutex;
        public ParameterManager ParameterManager;
        public static ILog logger;

        /// <summary>
        /// アプリケーションのエントリーポイント(メイン関数)です．ここから開始されます．
        /// </summary>
        [STAThread()]
        public static void Main(string[] args) {                        
            App app = new App();

            App.logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            App.logger.Info("start");

            app.InitializeComponent();

            // スプラッシュ画像の表示
            SplashScreen splashScreen = new SplashScreen("Images/splash01.png");
            splashScreen.Show(true);


            /* ユーザーインターフェイスの言語選択を行う */
            // 設定ファイルから使用する言語を取得するが，失敗した場合はシステムと同じ言語にする．
            // ただし，言語がない場合は標準(英語, En-US)にする．
            CultureInfo culture;
            try {
                culture = CultureInfo.GetCultureInfo(NagaraStage.Properties.Settings.Default.Language);
            } catch (Exception) {
                culture = CultureInfo.CurrentCulture;
                MessageBox.Show(NagaraStage.Properties.Strings.LangSelectException01);
            }
            CultureResources.ChangeCulture(culture);

            /* 二重起動を防止 */
            // 二重起動していないかを確認し，二重起動していた場合はエラーメッセージを表示して終了する．
            mutex = new System.Threading.Mutex(false, "AutoStage");
            if (!mutex.WaitOne(0, false)) {
                MessageBox.Show(NagaraStage.Properties.Strings.SoftwareAlreadyBoot);
                mutex.Close();
                mutex = null;
                app.Shutdown();
            }
            
            app.InitializeParameters();
#if !_NoHardWare           
            app.InitializedHardWare();
#endif
            app.Run(new Ui.MainWindow(app.ParameterManager));
        }

        /// <summary>
        /// ソフトウェア全体を通して用いるParameterManagerを初期化します．
        /// <para>失敗した場合，エラーメッセージを表示してアプリケーションを終了します．</para>
        /// </summary>
        public void InitializeParameters() {
            try {
                ParameterManager = new ParameterManager();
                ParameterManager.Initialize();

            } catch (Exception ex) {
                MessageBox.Show(ex.Message + NagaraStage.Properties.Strings.InitParamException01);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// モータコントローラ，LED，カメラといったハードウェアとのインターフェイスを初期化します．
        /// <para>失敗した場合，エラーメッセージを表示してアプリケーションを終了します．</para>
        /// </summary>
        public void InitializedHardWare() {
            /* ハードウェアを初期化する */
            // LED照明の初期化
            try {
                Led led = Led.GetInstance();
                led.Initiazlie();
                led.SetTimer(1600);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message + NagaraStage.Properties.Strings.InitLedException01);
                Environment.Exit(1);
            }
            // モータコントローラボードの初期化
            try {
                MotorControler mc = MotorControler.GetInstance(ParameterManager);
                mc.Initialize();
                //mc.InitializeMotorControlBoard(MechaAxisAddress.XAddress);
                //mc.InitializeMotorControlBoard(MechaAxisAddress.YAddress);
                //mc.InitializeMotorControlBoard(MechaAxisAddress.ZAddress);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message + NagaraStage.Properties.Strings.InitMotorException01);
                Environment.Exit(1);
            }
            //カメラの初期化
            try {
                Camera cam = Camera.GetInstance();
                cam.Initialize();

            } catch (Exception ex) {
                MessageBox.Show(ex.Message + NagaraStage.Properties.Strings.InitCameraExcetion01);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// アプリケーションを再起動します．
        /// </summary>
        public static void Restart() {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        /// <summary>
        /// アプリケーションが終了するときのイベントハンドラです．
        /// </summary>
        private void Application_Exit(object sender, ExitEventArgs e) {
            if (mutex != null) {
                /* Mutexを解放 */
                mutex.ReleaseMutex();

                /* Mutexを破棄 */
                mutex.Close();
            }

            NagaraStage.Properties.Settings.Default.Save();
            App.logger.Info("exit");
        }
    }
}
