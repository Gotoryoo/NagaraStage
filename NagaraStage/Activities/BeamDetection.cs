using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

using System.Windows;

using NagaraStage.IO;
using NagaraStage.Parameter;

using OpenCvSharp;
using OpenCvSharp.CPlusPlus;

namespace NagaraStage.Activities {
    class BeamDetention : Activity, IActivity {
        private static BeamDetention instance;

        public bool IsActive {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler<EventArgs> Started;
        public event ActivityEventHandler Exited;
        private ParameterManager parameterManager;
        private string direcotryPath;

        public string DirectoryPath {
            get { return direcotryPath; }
            set {
                direcotryPath = value;
                isValidate();
            }
        }

        public BeamDetention(ParameterManager _paramaterManager)
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
        public static BeamDetention GetInstance(
            ParameterManager parameterManger = null) {
            if (instance == null) {
                instance = new BeamDetention(parameterManger);
            }
            return instance;
        }

        public List<Thread> CreateTask() {
            List<Thread> taskList = new List<Thread>();
            taskList.Add(Create(new ThreadStart(delegate {
                try {
                    task();
                } catch (ThreadAbortException ex) {

                } finally {
                    MotorControler mc = MotorControler.GetInstance();
                    mc.AbortMoving();
                    mc.SlowDownStopAll();

                }
            })));
            //SurfaceLanding sl = new SurfaceLanding(parameterManager);
            //taskList.AddRange(sl.CreateTask());
            return taskList;
        }
 
        private void task() {
            Camera camera = Camera.GetInstance();
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Vector3 CurrentPoint = mc.GetPoint();
            Vector3 p = new Vector3();
            int BinarizeThreshold = 10;
            int BrightnessThreshold = 7;
            Mat sum = Mat.Zeros(440, 512, MatType.CV_8UC1);

            string datfileName = string.Format(@"c:\img\{0}.dat", System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"));
            BinaryWriter writer = new BinaryWriter(File.Open(datfileName, FileMode.Create));

            for (int i = 0; i < 10; i++)
            {
                byte[] b = camera.ArrayImage;
                writer.Write(b);
                p = mc.GetPoint();
                Mat mat = new Mat(440, 512, MatType.CV_8U, b);
                mat.ImWrite(String.Format(@"c:\img\{0}_{1}_{2}_{3}.bmp",
                        System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"),
                        (int)(p.X * 1000),
                        (int)(p.Y * 1000),
                        (int)(p.Z * 1000)));
                Cv2.GaussianBlur(mat, mat, Cv.Size(3, 3), -1);
                Mat gau = mat.Clone();
                Cv2.GaussianBlur(gau, gau, Cv.Size(31, 31), -1);
                Cv2.Subtract(gau, mat, mat);
                Cv2.Threshold(mat, mat, BinarizeThreshold, 1, ThresholdType.Binary);
                Cv2.Add(sum, mat, sum);
                mc.MoveDistance(-0.003, VectorId.Z);
                mc.Join();
            }

            Cv2.Threshold(sum, sum, BrightnessThreshold, 1, ThresholdType.Binary);

            //Cv2.FindContoursをつかうとAccessViolationExceptionになる(Release/Debug両方)ので、C-API風に書く
            using (CvMemStorage storage = new CvMemStorage())
            {
                using (CvContourScanner scanner = new CvContourScanner(sum.ToIplImage(), storage, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxSimple))
                {
                    //string fileName = string.Format(@"c:\img\{0}.txt",
                    //        System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"));
                    string fileName = string.Format(@"c:\img\u.txt");

                    foreach (CvSeq<CvPoint> c in scanner)
                    {
                        CvMoments mom = new CvMoments(c, false);
                        if (c.ElemSize < 2) continue;
                        if (mom.M00 == 0.0) continue;
                        double mx = mom.M10 / mom.M00;
                        double my = mom.M01 / mom.M00;
                        File.AppendAllText(fileName, string.Format("{0:F} {1:F}\n", mx, my));
                    }
                }
            }

            sum *= 255;
            sum.ImWrite(String.Format(@"c:\img\{0}_{1}_{2}.bmp",
                System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"),
                (int)(p.X * 1000),
                (int)(p.Y * 1000)));


            Vector2 encoderPoint = new Vector2(-1, -1);
            encoderPoint.X = mc.GetPoint().X;
            encoderPoint.Y = mc.GetPoint().Y;//おこられたのでしかたなくこうする　吉田20150427
            Vector2 viewerPoint = new Vector2(-1, -1);

            if (TigerPatternMatch.PatternMatch(ref viewerPoint))
            {
                encoderPoint = coordManager.TransToEmulsionCoord(viewerPoint);
                mc.MovePointXY(encoderPoint);
                mc.Join();
            }
        }

        private bool isValidate() {
            return true;
        }
    }
}
