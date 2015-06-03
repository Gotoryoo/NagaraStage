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

            
            mc.SetSpiralCenterPoint();

            int ledbrightness = led.AdjustLight(parameterManager);
            

            for (int vy = 0; vy < 16; vy++) {

                Vector3 cp = mc.GetPoint();
                mc.MoveTo(new Vector3(cp.X, cp.Y, initialpoint.Z + 0.020), new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                mc.Join();
                Thread.Sleep(100);

                camera.Start();
                bool flag = true;
                while (flag) {

                    mc.MoveDistance(-0.003, VectorId.Z);
                    mc.Join();
                    Thread.Sleep(100);

                    byte[] b = camera.ArrayImage;
                    Mat src = new Mat(440, 512, MatType.CV_8U, b);
                    Mat mat = src.Clone();

                    Cv2.GaussianBlur(mat, mat, Cv.Size(3, 3), -1);
                    Mat gau = mat.Clone();
                    Cv2.GaussianBlur(gau, gau, Cv.Size(7, 7), -1);
                    Cv2.Subtract(gau, mat, mat);
                    Cv2.Threshold(mat, mat, 4, 1, ThresholdType.Binary);
                    int brightness = Cv2.CountNonZero(mat);

                    if (brightness > 5000) flag = false;
                }
                camera.Stop();
                Vector3 surfpoint = mc.GetPoint();

                for (int vx = 0; vx < 16; vx++) {

                    byte[] bb = new byte[440 * 512 * 17];
                    /*
                    string datfileName = string.Format(@"c:\img\{0}_{1}_{2}_{3}.dat",
                        (int)(p.X * 1000),
                        (int)(p.Y * 1000),
                        vx,
                        vy
                        );
                    BinaryWriter writer = new BinaryWriter(File.Open(datfileName, FileMode.Create));
                    */

                    double vcenterx = initialpoint.X + vx * parameterManager.SpiralShiftX;
                    double vcentery = initialpoint.Y + vy * parameterManager.SpiralShiftY;
                    double vcenterz = surfpoint.Z - 0.053;
                    mc.MoveTo(new Vector3(vcenterx, vcentery, vcenterz), new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                    mc.Join();
                    Thread.Sleep(100);

                    Vector3 ip = mc.GetPoint();

                    mc.Inch(PlusMinus.Plus, 0.15, VectorId.Z);

                    int viewcounter = 0;
                    while (viewcounter < 16 + 1 + 3) {
                        byte[] b = Ipt.CaptureMain();
                        p = mc.GetPoint();

                        if (viewcounter >= 3) {
                            //b.CopyTo(bb, 440 * 512 * (viewcounter - 3));
                            Mat image = new Mat(440, 512, MatType.CV_8U, b);
                            Mat clone_image = image.Clone();

                        clone_image.ImWrite(string.Format(@"c:\img\{0}_{1}_{2}_{3}_{4}.bmp",
                         (int)(p.X * 1000),
                         (int)(p.Y * 1000),
                         vx,
                         vy,
                         viewcounter - 3));
                            string stlog = "";
                            stlog += String.Format("{0}   {1}  {2}  {3}\n",
                                    ledbrightness,
                                    ip.X,
                                    ip.Y,
                                    p.Z);
                            twriter.Write(stlog);
                        }
                        viewcounter++;
                    }//view
                    viewcounter = 0;

                    mc.StopInching(MechaAxisAddress.ZAddress);
                    mc.Join();
                    Thread.Sleep(100);
                    //writer.Write(bb);
                    //writer.Flush();
                    //writer.Close();
                }//vx
            }//vy


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

