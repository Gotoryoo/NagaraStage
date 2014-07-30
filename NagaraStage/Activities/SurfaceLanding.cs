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
    class SurfaceLanding : Activity, IActivity {
        private static SurfaceLanding instance;

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
        public static SurfaceLanding GetInstance(
            ParameterManager parameterManger = null) {
            if (instance == null) {
                instance = new SurfaceLanding(parameterManger);
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

            while (rowcounter < 45) {
                while (colcounter < 40) {
                    mc.MovePoint(
                        InitPoint.X + (parameterManager.ImageLengthX - 0.01) * colcounter,
                        InitPoint.Y + (parameterManager.ImageLengthY - 0.01) * rowcounter,
                        InitPoint.Z + 0.01);
                    mc.Join();

                    p = mc.GetPoint();

                    bool flag = true;
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
                        if (brightness > 10000 || viewcounter > 30) flag = false;
                    }

                    p = mc.GetPoint();
                    byte[] bb = camera.ArrayImage;
                    Mat mat2 = new Mat(440, 512, MatType.CV_8U, bb);
                    mat2.ImWrite(String.Format(@"c:\img\{0}_{1}_{2}_{3}.bmp", System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"),
                        (int)(p.X * 1000),
                        (int)(p.Y * 1000),
                        (int)(p.Z * 1000)));

                    viewcounter = 0;
                    colcounter++;
                }
                colcounter = 0;
                rowcounter++;
            }
            /*
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

                if(brightness > 10000 || viewcounter > 30) flag = false;
            }
            byte[] bb = camera.ArrayImage;
            Mat mat2 = new Mat(440, 512, MatType.CV_8U, bb);
            Vector3 p = new Vector3();
            mat2.ImWrite(String.Format(@"c:\img\{0}_{1}_{2}_{3}.bmp",  System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"),
                (int)(p.X * 1000),
                (int)(p.Y * 1000),
                (int)(p.Z * 1000)));
            */
        }

        private bool isValidate() {
            if (x < -100 || x > 100) {
                throw new ArgumentException("X must be in range from -100 to 100.");
            }
            return true;
        }

    }
}
