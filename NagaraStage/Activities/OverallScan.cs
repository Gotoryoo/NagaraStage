﻿using System;
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

            while (rowcounter < 6) {
                while (colcounter < 5) {

                    camera.Stop();
                    double startZ = 0.0;
                    PlusMinus plusminus;
                    if (colcounter%2==0) {
                        startZ = InitPoint.Z;
                        plusminus = PlusMinus.Minus;
                    } else {
                        startZ = InitPoint.Z - 0.50;//thickness 0.5mm
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

                    string stlog = "";

                    while (viewcounter < 130) {

                        byte[] b = Ipt.CaptureMain();

                        p = mc.GetPoint();
                        stlog += String.Format("{0} {1} {2} {3} {4} captured {3}\n",
                            System.DateTime.Now.ToString("HHmmss_ffff"),
                            (int)(p.X * 1000),
                            (int)(p.Y * 1000),
                            (int)(p.Z * 10000),
                            viewcounter);

                        writer.Write(b);

                        //Mat mat = new Mat(440, 512, MatType.CV_8U, b);
                        //Mat forsavemat = mat.Clone();
                        //forsavemat.ImWrite(datfileName);

                        p = mc.GetPoint();
                        if (p.Z<-0.45) {
                            viewcounter+=200;
                        }
                        viewcounter++;

                    }

                    p = mc.GetPoint();
                    stlog += String.Format("{0} {1} {2} {3} {4} stop\n",
                        System.DateTime.Now.ToString("HHmmss_ffff"),
                        (int)(p.X * 1000),
                        (int)(p.Y * 1000),
                        (int)(p.Z * 10000),
                        viewcounter);

                    mc.StopInching(MechaAxisAddress.ZAddress);
                    
                    for(int i=0; i<20; i++){
                        Thread.Sleep(10);
                        p = mc.GetPoint();
                        stlog += String.Format("{0} {1} {2} {3} {4} dec\n",
                            System.DateTime.Now.ToString("HHmmss_ffff"),
                            (int)(p.X * 1000),
                            (int)(p.Y * 1000),
                            (int)(p.Z * 10000),
                            viewcounter);
                    }
                    
                    viewcounter = 0;
                    colcounter++;
                    Console.Write(stlog);
                    writer.Flush();
                    writer.Close();
                    camera.Start();
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
