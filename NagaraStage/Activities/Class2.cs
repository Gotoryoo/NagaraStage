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
    class Class2 : Activity, IActivity {
        private static Class2 instance;

        public bool IsActive {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler<EventArgs> Started;
        public event ActivityEventHandler Exited;
        private ParameterManager parameterManager;
        private string direcotryPath;

      /*   public string DirectoryPath {
             get { return direcotryPath; }
             set {
                 direcotryPath = value;
                 isValidate();
             }
         }*/

        public Class2(ParameterManager _paramaterManager)
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
        public static Class2 GetInstance(
            ParameterManager parameterManger = null) {
            if (instance == null) {
                instance = new Class2(parameterManger);
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
                    mc.StopInching(MechaAxisAddress.XAddress);
                    mc.StopInching(MechaAxisAddress.YAddress);
                    mc.StopInching(MechaAxisAddress.ZAddress);
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
            Vector3 p = new Vector3();


            List<Mat> image_set = new List<Mat>();
            Vector3 ggg = mc.GetPoint();

            string txtfileName = string.Format(@"c:\img\spiral.txt");
            StreamWriter twriter = File.CreateText(txtfileName);

            Vector3 initialpoint = mc.GetPoint();

            //    List<Vector2> list_grid_pred = new List<Vector2>();
            //list_grid_pred.Add(new Vector2(2.0, 30.0));
            //list_grid_pred.Add(new Vector2(-2.0, 30.0));
            //list_grid_pred.Add(new Vector2(30.0, 2.0));
            //list_grid_pred.Add(new Vector2(30.0, -2.0));
            //    list_grid_pred.Add(new Vector2(38.4, -14.0));
            //   list_grid_pred.Add(new Vector2(38.5, -14.1));

            camera.Start();
            mc.SetSpiralCenterPoint();

            int ledbrightness = led.AdjustLight(parameterManager);

            for (int t = 0; t < 400; t++) {
                
                mc.MovePointZ(initialpoint.Z+0.020);
                mc.Join();

                int viewcounter = 0;
                bool flag = true;
                while (flag) {

                    mc.MoveDistance(-0.003, VectorId.Z);
                    mc.Join();
                    byte[] b = camera.ArrayImage;
                    Mat src = new Mat(440, 512, MatType.CV_8U, b);
                    Mat mat = src.Clone();

                    Cv2.GaussianBlur(mat, mat, Cv.Size(3, 3), -1);
                    Mat gau = mat.Clone();
                    Cv2.GaussianBlur(gau, gau, Cv.Size(7, 7), -1);
                    Cv2.Subtract(gau, mat, mat);
                    Cv2.Threshold(mat, mat, 4, 1, ThresholdType.Binary);
                    int brightness = Cv2.CountNonZero(mat);

                    viewcounter++;

                    if (brightness > 5000 || viewcounter > 100) flag = false;
                }
                
                //ledbrightness = led.AdjustLight(parameterManager);

                Vector3 surfacepoint = mc.GetPoint();

                for (int q = 0; q < 23; q++) {

                        mc.MovePointZ(surfacepoint.Z - 0.06 + (0.003 * q));
                        mc.Join();


                        Vector3 nowpoint = mc.GetPoint();

                        byte[] b = camera.ArrayImage;
                        Mat image = new Mat(440, 512, MatType.CV_8U, b);
                        Mat clone_image = image.Clone();

                        //image_set.Add(clone_image);
                        //char[] filename = new char[64];
                        //String.Format(filename,"gtrd%d.png" , i );

                        clone_image.ImWrite(String.Format(@"c:\img\{0}_{1}_{2:00}.bmp",
                            (int)(nowpoint.X * 1000),
                            (int)(nowpoint.Y * 1000),
                            q)
                            );

                        string stlog = "";
                        stlog += String.Format("{0}   {1}  {2}  {3}\n",
                                ledbrightness,
                                nowpoint.X,
                                nowpoint.Y,
                                nowpoint.Z);
                        twriter.Write(stlog);

                    }//for q-th layer

                    mc.MoveInSpiral(true);
                    mc.Join();
                }//for t-th view


                camera.Stop();
                twriter.Close();
            }

            /*
            int viewcounter = 0;


            string txtfileName = string.Format(@"{0}\{1}.txt",
                direcotryPath, System.DateTime.Now.ToString("yyyyMMdd_HHmmss_ffff"));
            StreamWriter twriter = File.CreateText(txtfileName);

            List<Vector3> PointList = new List<Vector3>();
            for (int xx = 0; xx < 5; xx++) {
                for (int yy = 0; yy < 5; yy++) {
                    GridMark nearestMark = cm.GetTheNearestGridMark(new Vector3(InitPoint.X + xx * 10, InitPoint.Y + yy * 10, InitPoint.Z));
                    PointList.Add(new Vector3(nearestMark.x, nearestMark.y, InitPoint.Z));
                }
            }

            camera.Stop();

            for (int pp = 0; pp < PointList.Count(); pp++) {
                string stlog = "";
                int nshot = 20;
                byte[] bb = new byte[440 * 512 * nshot];

                mc.MovePoint(PointList[pp]);
                mc.Join();

                p = mc.GetPoint();
                double prev_z = p.Z;
                DateTime starttime = System.DateTime.Now;
                string datfileName = string.Format(@"{0}\{1}_x{2}_y{3}.dat",
                    direcotryPath,
                    starttime.ToString("yyyyMMdd_HHmmss_fff"),
                    (int)(p.X * 1000),
                    (int)(p.Y * 1000));
                BinaryWriter writer = new BinaryWriter(File.Open(datfileName, FileMode.Create));



                while (viewcounter < nshot) {
                    mc.MoveDistance(-0.001, VectorId.Z);
                    mc.Join();
                    byte[] b = Ipt.CaptureMain();
                    p = mc.GetPoint();
                    TimeSpan ts = System.DateTime.Now - starttime;
                    stlog += String.Format("{0} {1} {2} {3} {4} {5} {6} {7}\n",
                        pp,
                        System.DateTime.Now.ToString("HHmmss\\.fff"),
                        ts.ToString("s\\.fff"),
                        (p.X * 1000).ToString("0.0"),
                        (p.Y * 1000).ToString("0.0"),
                        (p.Z * 1000).ToString("0.0"),
                        (prev_z * 1000 - p.Z * 1000).ToString("0.0"),
                        viewcounter);
                    b.CopyTo(bb, 440 * 512 * viewcounter);
                    viewcounter++;
                }

                viewcounter = 0;
                twriter.Write(stlog);
                writer.Write(bb);
                writer.Flush();
                writer.Close();

            }

            twriter.Close();
            camera.Start();
             */

        }

    }

    