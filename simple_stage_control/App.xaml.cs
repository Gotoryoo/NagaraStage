using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

using NagaraStage.Parameter;
using NagaraStage.IO;








namespace simple_stage_control
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private static System.Threading.Mutex mutex;
        public ParameterManager ParameterManager;








        /// <summary>
        /// ソフトウェア全体を通して用いるParameterManagerを初期化します．
        /// <para>失敗した場合，エラーメッセージを表示してアプリケーションを終了します．</para>
        /// </summary>
        public void InitializeParameters()
        {
            try
            {
                ParameterManager = new ParameterManager();
                ParameterManager.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + NagaraStage.Properties.Strings.InitParamException01);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// モータコントローラ，LED，カメラといったハードウェアとのインターフェイスを初期化します．
        /// <para>失敗した場合，エラーメッセージを表示してアプリケーションを終了します．</para>
        /// </summary>
        public void InitializedHardWare()
        {
            /* ハードウェアを初期化する */
            // LED照明の初期化
            try
            {
                Led led = Led.GetInstance();
                led.Initiazlie();
                led.SetTimer(1600);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + NagaraStage.Properties.Strings.InitLedException01);
                Environment.Exit(1);
            }
            // モータコントローラボードの初期化
            try
            {
                MotorControler mc = MotorControler.GetInstance(ParameterManager);
                mc.Initialize();
                mc.InitializeMotorControlBoard(MechaAxisAddress.XAddress);
                mc.InitializeMotorControlBoard(MechaAxisAddress.YAddress);
                mc.InitializeMotorControlBoard(MechaAxisAddress.ZAddress);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + NagaraStage.Properties.Strings.InitMotorException01);
                Environment.Exit(1);
            }
            // IP5000 カメラの初期化
     //       try
     //       {
     //           int retCode = new int();
     //           retCode = Ipt.Initialize(0, ref retCode);
     //           retCode = Ipt.ReadSocketIniFile();
     //           Ipt.SetCameraType((int)Profile.CameraType);
     //           if (Profile.CameraType == CameraType.SONY_XC_HR3000_2I_59MHz)
     //           {
     //               NagaraStage.IO.Driver.VP910.InitializeLUT(Ipt.GetDeviceId());
     //           }
     //       }
     //       catch (Exception ex)
     //       {
     //           MessageBox.Show(ex.Message + NagaraStage.Properties.Strings.InitCameraExcetion01);
     //           Environment.Exit(1);
     //       }
        }
    }
}
