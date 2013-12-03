using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage.Parameter {
    /// <summary>
    /// ステージ全体に関わるパラメータのためのインターフェイスです．
    /// </summary>
    public interface IStageParameter {
        Vector3 EncoderResolution {
            get;
        }

        Vector3 MotorResolution {
            get;
        }

        Vector3 MotorInitialVelocity {
            get;
        }

        Vector3 MotorAccelTime {
            get;
        }

        Vector3 MotorSpeed1 {
            get;
        }

        Vector3 MotorSpeed2 {
            get;
        }

        Vector3 MotorSpeed3 {
            get;
        }

        Vector3 MotorSpeed4 {
            get;
        }

        double LimitPol {
            get;
        }
    }
}
