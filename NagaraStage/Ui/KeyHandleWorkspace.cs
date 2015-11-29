using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

using NagaraStage.IO;
using NagaraStage.Activities;

namespace NagaraStage.Ui {
    /// <summary>
    /// モータ制御などのキーボードイベントのイベントハンドラを備えたコントロールのクラスです．
    /// </summary>
    public class KeyHandleWorkspace : Workspace {

        /// <summary>
        /// モータの手動制御を許可するかどうかを取得，または設定します．
        /// </summary>
        public bool IsControllable = true;        

        private IMainWindow window;
        private Parameter.ParameterManager parameterManager;

        public KeyHandleWorkspace(IMainWindow _window)
            : base(_window) {
            this.window = _window;
            this.parameterManager = window.ParameterManager;
            KeyDown += KeyHandleWorkspace_KeyDown;
            KeyUp += KeyHandleWorkspace_KeyUp;
            Unloaded += MotorKeyHandler_Unloaded;
        }

        public virtual void WriteLine(string message) {                            
            MessageBox.Show(message);
        }

        void KeyHandleWorkspace_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            // モータの移動処理を停止する．
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            if (e.Key == Key.A || e.Key == Key.D) {
                mc.SlowDownStop(VectorId.X);
            }
            if (e.Key == Key.W || e.Key == Key.X) {
                mc.SlowDownStop(VectorId.Y);
            }
            if (e.Key == Key.Q || e.Key == Key.E) {
                mc.SlowDownStop(VectorId.Z);
            }
        }

        protected virtual void KeyHandleWorkspace_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (!IsControllable) {
                WriteLine(Properties.Strings.NoControllable);
                return;
            }

            // アクティビティが実行中であれば中止する.
            Activity activity = new Activity(parameterManager);
            activity.Abort();

            // モータが移動中であれば移動を中止する．
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            if (mc.IsMoving) {
                mc.AbortMoving();
                WriteLine(Properties.Strings.AbortMotor);
            }
            try {
#if !_NoHardWare
                if (e.Key == Key.A) {
                    mc.ContinuousDrive(VectorId.X, PlusMinus.Minus, mc.Speed.X);
                } else if (e.Key == Key.D) {
                    mc.ContinuousDrive(VectorId.X, PlusMinus.Plus, mc.Speed.X);
                } else if (e.Key == Key.X) {
                    mc.ContinuousDrive(VectorId.Y, PlusMinus.Minus, mc.Speed.Y);
                } else if (e.Key == Key.W) {
                    mc.ContinuousDrive(VectorId.Y, PlusMinus.Plus, mc.Speed.Y);
                } else if (e.Key == Key.Q) {
                    mc.ContinuousDrive(VectorId.Z, PlusMinus.Minus, mc.Speed.Z);
                } else if (e.Key == Key.E) {
                    mc.ContinuousDrive(VectorId.Z, PlusMinus.Plus, mc.Speed.Z);
                } else if (e.Key == Key.N) {
                    mc.MoveInSpiral();
                } else if (e.Key == Key.O) {                    
                    mc.SetSpiralCenterPoint();
                } else if (e.Key == Key.I) {
                    mc.BackToSpiralCenter();
                } else if (e.Key == Key.B) {
                    mc.SpiralBack();
                }
#endif
            } catch (MotorAxisException ex) {
                WriteLine(ex.Message);
            }
        }

        void MotorKeyHandler_Unloaded(object sender, System.Windows.RoutedEventArgs e) {
#if !_NoHardWare
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            mc.AbortMoving();
            mc.SlowDownStopAll();
#endif
        }
    }
}
