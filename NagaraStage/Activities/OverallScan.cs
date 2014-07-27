using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

using NagaraStage.IO;
using NagaraStage.Parameter;

using OpenCvSharp;
using OpenCvSharp.CPlusPlus;

namespace NagaraStage.Activities {
    class OverallScan : Activity, IActivity {
        private static OverallScan instance;

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

        public event EventHandler<EventArgs> Started;
        public event ActivityEventHandler Exited;
        private ParameterManager parameterManager;
        private double x;

        public OverallScan(ParameterManager _paramaterManager)
            : base(_paramaterManager) {
            this.parameterManager = _paramaterManager;
        }

        public void Start() {
            throw new NotImplementedException();
        }

        public void Abort() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// インスタンスを取得します．
        /// </summary>
        /// <returns></returns>
        public static OverallScan GetInstance(
            ParameterManager parameterManger = null) {
            if (instance == null) {
                instance = new OverallScan(parameterManger);
            }
            return instance;
        }

        public List<Thread> CreateTask() {
            List<Thread> taskList = new List<Thread>();
            taskList.Add(Create(new ThreadStart(task)));
            return taskList;
        }

        private void task() {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            Vector3 InitPoint = mc.GetPoint();
            Vector3 p = new Vector3();
            int viewcounter = 0;
            int linecounter = 0;

            while (linecounter < 5) {
                Vector3 ViewPoint = mc.GetPoint();
                string datfileName = string.Format(@"c:\img\{0}.dat", System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"));
                BinaryWriter writer = new BinaryWriter(File.Open(datfileName, FileMode.Create));
                while (viewcounter < 30) {
                    byte[] b = camera.ArrayImage;
                    writer.Write(b);
                    mc.MoveDistance(-0.004, VectorId.Z);
                    mc.Join();
                    viewcounter++;
                }
                viewcounter = 0;
                mc.MovePoint(ViewPoint.X + parameterManager.ImageLengthX - 0.01, ViewPoint.Y, ViewPoint.Z);
                mc.Join();
                linecounter++;
            }
        }

        private bool isValidate() {
            if (x < -100 || x > 100) {
                throw new ArgumentException("X must be in range from -100 to 100.");
            }
            return true;
        }
    }
}
