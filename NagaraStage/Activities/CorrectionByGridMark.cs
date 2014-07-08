using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using NagaraStage.IO;
using NagaraStage.Parameter;

namespace NagaraStage.Activities {
    /// <summary>
    /// 近くのグリッドマークを見つけて、座標系の補正を行うアクティビティクラスです
    /// </summary>
    class CorrectionByGridMark: IActivity {
        private CoordManager coordManager;
        private ParameterManager parameterManager;

        CorrectionByGridMark(CoordManager _coordManager, ParameterManager _parameterManager) {
            this.coordManager = _coordManager;
            this.parameterManager = _parameterManager;
            isValidate();
        }

        private GridMark getNearestGirdMark() {
            MotorControler mc = MotorControler.GetInstance();
            return coordManager.GetNearestGridMark(mc.GetPoint());            
        }

        private Boolean isValidate() { 
            if(coordManager == null) {
                throw new NullReferenceException();
            }
            if (parameterManager == null) {
                throw new NullReferenceException();
            }
            return true;
        }

        public bool IsActive {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler<EventArgs> Started;

        public event ActivityEventHandler Exited;

        public void Start() {
            throw new NotImplementedException();
        }

        public void Abort() {
            throw new NotImplementedException();
        }

        public Thread CreateTaskThread() {
            return new Thread(new ThreadStart(task));
        }

        /// <summary>
        /// アクティビティの動作スレッド用のメソッドです。
        /// 直接呼び出さないでください。
        /// </summary>
        private void task() {
            GridMark nearestMark = getNearestGirdMark();
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            mc.MovePointXY(nearestMark.x, nearestMark.y);
            mc.Join();

            // 新たにグリッドーマークの座標を取得したら、
            // Ipt.setHyperFineXYにその座標と既知の座標(予測点)のズレ(エンコーダ座標系)を渡す
        }
    }
}
