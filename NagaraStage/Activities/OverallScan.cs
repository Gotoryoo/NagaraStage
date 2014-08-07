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
            Surface sur = Surface.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            Led led = Led.GetInstance();

            Vector3 InitPoint = mc.GetPoint();
            Vector3 p = new Vector3();
            int viewcounter = 0;
            int rowcounter = 0;
            int colcounter = 0;

            while (rowcounter < 6) {
                while (colcounter < 6) {

                    string stlog = "";
                    int nshot = (int)((sur.UpTop - sur.LowBottom) / 0.004);
                    byte[] bb = new byte[440 * 512 * nshot];

                    double startZ = 0.0;
                    PlusMinus plusminus;
                    if (colcounter%2==0) {
                        camera.Start();
                        led.AdjustLight(parameterManager);
                        camera.Stop();
                        startZ = sur.UpTop + 0.01;
                        plusminus = PlusMinus.Minus;
                    } else {
                        startZ = sur.LowBottom-0.014;
                        plusminus = PlusMinus.Plus;
                    }


                    mc.MovePoint(
                        InitPoint.X + (parameterManager.ImageLengthX - 0.01) * colcounter,
                        InitPoint.Y + (parameterManager.ImageLengthY - 0.01) * rowcounter,
                        startZ);
                    mc.Join();
                    
                    p = mc.GetPoint();
                    string datfileName = string.Format(@"c:\img\{0}_{1}_{2}.dat",
                        System.DateTime.Now.ToString("yyyyMMdd_HHmmss_ffff"),
                        (int)(p.X * 1000),
                        (int)(p.Y * 1000));
                    BinaryWriter writer = new BinaryWriter(File.Open(datfileName, FileMode.Create));

                    mc.Inch(plusminus, 0.20, VectorId.Z);

                    while (viewcounter < nshot + 6) {
                        byte[] b = Ipt.CaptureMain();
                        p = mc.GetPoint();
                        stlog += String.Format("{0} {1} {2} {3} {4} captured {3}\n",
                            System.DateTime.Now.ToString("HHmmss_ffff"),
                            (int)(p.X * 1000),
                            (int)(p.Y * 1000),
                            (int)(p.Z * 10000),
                            viewcounter);

                        if (viewcounter >= 6) {
                            b.CopyTo(bb, 440 * 512 * (viewcounter - 6));
                        }
                        viewcounter++;
                    }

                    mc.StopInching(MechaAxisAddress.ZAddress);
                                        
                    viewcounter = 0;
                    colcounter++;
                    Console.Write(stlog);
                    writer.Write(bb);
                    writer.Flush();
                    writer.Close();
                }
                colcounter = 0;
                rowcounter++;
            }
            camera.Start();
        }

        private bool isValidate() {
            if (x < -100 || x > 100) {
                throw new ArgumentException("X must be in range from -100 to 100.");
            }
            return true;
        }
    }
}
