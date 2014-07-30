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
    class SurfaceLanding : Activity, IActivity {
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

        public SurfaceLanding(ParameterManager _paramaterManager)
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
            int viewcounter = 0;

            bool flag =true;
            while (flag) {
                mc.MoveDistance(-0.003, VectorId.Z);
                mc.Join();
                byte[] b = camera.ArrayImage;
                Mat src = new Mat(440, 512, MatType.CV_8U, b);
                Mat mat = src.Clone();

                Cv2.GaussianBlur(mat, mat, Cv.Size(3, 3), -1);
                Mat gau = mat.Clone();
                Cv2.GaussianBlur(gau, gau, Cv.Size(31, 31), -1);
                Cv2.Subtract(gau, mat, mat);
                Cv2.Threshold(mat, mat, 10, 1, ThresholdType.Binary);
                int brightness = Cv2.CountNonZero(mat);

                viewcounter++;

                if(brightness > 3000 || viewcounter > 30) flag = false;

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
