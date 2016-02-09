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
 * <p>ようこそ，NagaraStageへ</p>
 * <h2>Nagara Stageとは？</h2>
 * <p>かつて行われたE373実験では，エマルション(Emulsion)上に残る粒子の軌跡であるトラック(Track)を分析するためにAutoStageが開発されました．
 * 当時開発されたAutoStageを用いてE373実験のデータ分析を行いました．そしてE373実験の規模を拡大させた<strong>E07実験</strong>が行われようとしています．
 * E07実験はE373実験と比べ実験規模が多きいため，トラックなどの分析量が膨大になることが予想されています．
 * 膨大な量のデータを分析するにはコンピュータによる支援は不可欠です．NagaraStageはその先駆けとして開発を行っています．
 * </p>
 * <h2>仕様・指針</h2>
 * <p>E373実験は1997年に行われました．当時と比べコンピュータの事情は大きく変化しました．当時のAutoStageはVisual Basic6で書かれていました．
 * しかし，2012年現在においてVisual Basic6は古きものとなってしまいました．今後の将来を考えても新しいプラットフォームに移行する必要があると思います．
 * そこで，新AutoStageであるこのNagaraStageは.Net Framework4.0というプラットフォームを使っています．言語はC\#です．
 * </p>
 * <p>基本的には.Net Framework4.0，C\#で開発を行いますが，画像処理など高い演算能力を必要とする部分はネイティブ言語であるC++を使います．
 * また，.Net Frameworkの機能にもアクセスする必要もあるため，C++を拡張したC++/CLIも使っています．
 * </p>
 * @author Hirokazu Yokoyama <o1007410@edu.gifu-u.ac.jp>
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
            SplashScreen splashScreen = new SplashScreen("Images/splash01.jpg");
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
            // IP5000 カメラの初期化
            try {
                int retCode = new int();
                retCode = Ipt.Initialize(0, ref retCode);
                retCode = Ipt.ReadSocketIniFile();
                Ipt.SetCameraType((int)Profile.CameraType);
                if (Profile.CameraType == CameraType.SONY_XC_HR3000_2I_59MHz) {
                    NagaraStage.IO.Driver.VP910.InitializeLUT(Ipt.GetDeviceId());
                }
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

#if !_NoHardWare
            // モータを原点に戻す.
            MotorControler mc = MotorControler.GetInstance(ParameterManager);
            try {
                //mc.MovePointXY(0, 0);
                //mc.Join();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
#endif
            NagaraStage.Properties.Settings.Default.Save();
            App.logger.Info("exit");
        }
    }
}
