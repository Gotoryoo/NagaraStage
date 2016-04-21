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
    class GridMeasure07 : Activity, IActivity {
        private static GridMeasure07 instance;

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

        public GridMeasure07(ParameterManager _paramaterManager)
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
        public static GridMeasure07 GetInstance(
            ParameterManager parameterManger = null) {
            if (instance == null) {
                instance = new GridMeasure07(parameterManger);
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
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Surface sur = Surface.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            Led led = Led.GetInstance();
            CoordManager cm = new CoordManager(parameterManager);


            Vector3 InitPoint = mc.GetPoint();

            List<Vector2> src = new List<Vector2>();
            List<Vector2> dst = new List<Vector2>();

            src.Add(new Vector2(0, 0));
            dst.Add(new Vector2(0, 0));

            src.Add(new Vector2(-140.0, 150.0));
            dst.Add(new Vector2(-138.770, 151.485));

            src.Add(new Vector2(+140.0, 160.0));
            dst.Add(new Vector2(+141.581, 158.903));

            src.Add(new Vector2(+150.0, -140.0));
            dst.Add(new Vector2(+148.880, -141.600));

            src.Add(new Vector2(-150.0, -140.0));
            dst.Add(new Vector2(-151.297, -138.670));


            Affine ap = Affine.CreateAffineBy(src, dst);


            string txtfileName = string.Format(@"c:\img\_grid_g.txt");
            StreamWriter twriter = File.CreateText(txtfileName);

            for (int x = -14; x < 14; x += 2) {
                for (int y = -14; y < 14; y += 2) {

                    Vector2 predpoint = ap.Trance(new Vector2(x * 10, y * 10));
                    mc.MoveTo(new Vector3(predpoint.X, predpoint.Y, InitPoint.Z));
                    mc.Join();

                    led.AdjustLight(parameterManager);

                    byte[] b2 = camera.ArrayImage;
                    Mat src2 = new Mat(440, 512, MatType.CV_8U, b2);
                    Mat mat2 = src2.Clone();

                    string FileName = string.Format(@"C:\img\grid_x{0}_y{1}.png", x, y);
                    Cv2.ImWrite(FileName, mat2);


                    string stlog = "";
                    stlog += String.Format("{0} {1} {2:f4} {3:f4}\n",
                            x * 10,
                            y * 10,
                            predpoint.X,
                            predpoint.Y);
                    twriter.Write(stlog);

                }//for y
            }//for x

            twriter.Close();
        }//task()



        private bool isValidate() {
            return true;
        }
    }
}
