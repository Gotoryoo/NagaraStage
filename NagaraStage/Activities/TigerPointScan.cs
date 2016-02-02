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

    class pointscan {
        public int id;
        public Vector3 stagecoord;
    }


    class TigerPointScan : Activity, IActivity {
        private static TigerPointScan instance;

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

        public TigerPointScan(ParameterManager _paramaterManager)
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
        public static TigerPointScan GetInstance(
            ParameterManager parameterManger = null) {
            if (instance == null) {
                instance = new TigerPointScan(parameterManger);
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
            try {
                MotorControler mc = MotorControler.GetInstance(parameterManager);
                Surface sur = Surface.GetInstance(parameterManager);
                Camera camera = Camera.GetInstance();
                Led led = Led.GetInstance();
                CoordManager cm = new CoordManager(parameterManager);

                Vector3 InitPoint = mc.GetPoint();
                Vector3 SurfPoint = mc.GetPoint();
                Vector3 p = new Vector3();
                int viewcounter = 0;


                //string txtfileName = string.Format(@"{0}\{1}.txt",
                //    direcotryPath, System.DateTime.Now.ToString("yyyyMMdd_HHmmss_ffff"));
                //StreamWriter twriter = File.CreateText(txtfileName);

                List<pointscan> PSList = new List<pointscan>();

                var reader = new StreamReader(File.OpenRead(@"C:\test\list.txt"));
                bool headerflag = true;


                while (!reader.EndOfStream) {
                    var line = reader.ReadLine();
                    string[] delimiter = { " " };
                    var values = line.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

                    pointscan ps = new pointscan();
                    ps.id = int.Parse(values[1]);
                    ps.stagecoord = new Vector3(
                        double.Parse(values[12]),
                        double.Parse(values[13]),
                        double.Parse(values[14])
                        );
                    PSList.Add(ps);
                }


                int nshot = 26;
                byte[] bb = new byte[440 * 512 * nshot];
                camera.Stop();

                for (int pp = 0; pp < PSList.Count(); pp++) {

                    viewcounter = 0;
                    if (pp % 1 == 0) {
                        Vector3 surfrecogpoint = PSList[pp].stagecoord;
                        surfrecogpoint.Z = InitPoint.Z + 0.06;
                        mc.MoveTo(surfrecogpoint);
                        mc.Join();

                        bool flag = true;
                        while (flag) {
                            mc.MoveDistance(-0.003, VectorId.Z);
                            mc.Join();
                            byte[] b = Ipt.CaptureMain();
                            int brightness;

                            using (Mat src = new Mat(440, 512, MatType.CV_8U, b))
                            using (Mat mat = src.Clone()) {
                                Cv2.GaussianBlur(mat, mat, Cv.Size(3, 3), -1);
                                using (Mat gau = mat.Clone()) {
                                    Cv2.GaussianBlur(gau, gau, Cv.Size(31, 31), -1);
                                    Cv2.Subtract(gau, mat, mat);
                                    Cv2.Threshold(mat, mat, 10, 1, ThresholdType.Binary);
                                    brightness = Cv2.CountNonZero(mat);
                                }//using gau
                            }//using src and mat

                            viewcounter++;

                            if (brightness > 5000 || viewcounter > 40) flag = false;
                        }

                        SurfPoint = mc.GetPoint();
                    }

                    Vector3 CandPoint = PSList[pp].stagecoord;
                    CandPoint.Z = SurfPoint.Z + CandPoint.Z - 0.044;

                    mc.MoveTo(CandPoint);
                    mc.Join();

                    camera.Start();
                    led.AdjustLight(parameterManager);
                    camera.Stop();

                    p = mc.GetPoint();
                    double prev_z = p.Z;
                    DateTime starttime = System.DateTime.Now;
                    string datfileName = string.Format(@"{0}\{1:00000}_x{2}_y{3}.dat",
                        direcotryPath,
                        PSList[pp].id,
                        (int)(p.X * 1000),
                        (int)(p.Y * 1000));
                    BinaryWriter writer = new BinaryWriter(File.Open(datfileName, FileMode.Create));

                    string stlog = "";
                    viewcounter = 0;
                    while (viewcounter < nshot) {
                        mc.MoveDistance(0.003, VectorId.Z);
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

                    //twriter.Write(stlog);
                    writer.Write(bb);
                    writer.Flush();
                    writer.Close();

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                }

                camera.Start();
                //twriter.Close();

            } catch (Exception ex) {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private bool isValidate() {
            return true;
        }
    }
}
