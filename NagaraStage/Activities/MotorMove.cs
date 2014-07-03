using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using NagaraStage.IO;
using NagaraStage.Parameter;

namespace NagaraStage.Activities {
    [Obsolete("アクティビティキューを試験するためのクラスです。そのうち廃止します。")]
    class MotorMove: Activity, IActivity {
        public bool IsActive {
            get { throw new NotImplementedException(); }
        }

        public double X {
            get { return x; }
            set {
                x = value;
                isValidate();
            }
        }

        public double Y {
            get { return y; }
            set {
                y = value;
                isValidate();
            }
        }

        public double Z {
            get { return z; }
            set {
                z = value;
                isValidate();
            }
        }

        public event EventHandler<EventArgs> Started;
        public event ActivityEventHandler Exited;
        private ParameterManager parameterManager;
        private double x, y, z;

        public MotorMove(ParameterManager _paramaterManager):base(_paramaterManager) {
            this.parameterManager = _paramaterManager;
        }

        public void Start() {
            throw new NotImplementedException();
        }

        public void Abort() {
            throw new NotImplementedException();
        }


        public Thread CreateTaskThread() {
            return Create(new ThreadStart(task));
        }

        private void task() {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            mc.MovePoint(X, Y, Z);
            mc.Join();
        }

        private bool isValidate() { 
            if(x < -100 || x > 100) {
                throw new ArgumentException("X must be in range from -100 to 100.");
            }
            if (y < -100 || y > 100) {
                throw new ArgumentException("Y must be in range from -100 to 100.");
            }
            if (z < -100 || z > 100) {
                throw new ArgumentException("Z must be in range from -100 to 100.");
            }
            return true;
        }
    }
}
