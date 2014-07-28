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
            int rowcounter = 0;
            int colcounter = 0;

            while (rowcounter < 4) {
                while (colcounter < 4) {
                    mc.MovePoint(
                        InitPoint.X + (parameterManager.ImageLengthX - 0.01) * colcounter,
                        InitPoint.Y + (parameterManager.ImageLengthY - 0.01) * rowcounter,
                        InitPoint.Z);
                    mc.Join();

                    p = mc.GetPoint();
                    string datfileName = string.Format(@"c:\img\{0}_{1}_{2}.bmp",
                        System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"),
                        (int)(p.X * 1000),
                        (int)(p.Y * 1000));
//                    BinaryWriter writer = new BinaryWriter(File.Open(datfileName, FileMode.Create));
//                    while (viewcounter < 1) {
                        byte[] b = camera.ArrayImage;
                        Mat mat = new Mat(440, 512, MatType.CV_8U, b);
                        //writer.Write(b);
                        Mat forsavemat = mat.Clone();
                        forsavemat.ImWrite(datfileName);
                        Thread.Sleep(200);
                        mc.MoveDistance(-0.004, VectorId.Z);
                        mc.Join();
                        viewcounter++;
//                    }
                    viewcounter = 0;
                    colcounter++;
                }
                colcounter = 0;
                rowcounter++;
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
