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

        public event EventHandler<EventArgs> Started;
        public event ActivityEventHandler Exited;
        private ParameterManager parameterManager;
        private int nxView, nyView;
        private string direcotryPath;

        public int NumOfViewX {
            get { return nxView; }
            set {
                nxView = value;
                isValidate();
            }
        }

        public int NumOfViewY {
            get { return nyView; }
            set {
                nyView = value;
                isValidate();
            }
        }

        public string DirectoryPath {
            get { return direcotryPath; }
            set {
                direcotryPath = value;
                isValidate();
            }
        }

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
            return taskList;
        }

        private void task() {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Surface sur = Surface.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            Led led = Led.GetInstance();

            Vector3 InitPoint = mc.GetPoint();
            Vector3 p = new Vector3();

            double emthickness = sur.UpTop - sur.UpBottom;
            int nshot = (int)(emthickness / 0.003);
            

            int blockXCounter = 0;
            int blockYCounter = 0;
            while(blockYCounter < nyView){
                while(blockXCounter < nxView){

                    string txtfileName = string.Format(@"{0}\{1}.txt"
                        , direcotryPath
                        , System.DateTime.Now.ToString("yyyyMMdd_HHmmss_ffff")
                        );
                    StreamWriter twriter = File.CreateText(txtfileName);

                    //Vector3 InitPointofThisBlock = new Vector3(
                    //    InitPoint.X + blockXCounter * 4.350,
                    //    InitPoint.Y + blockYCounter * 4.390,
                    //    InitPoint.Z
                    //    );

                    //Vector3 SurfPointofThisBlock = new Vector3(
                    //    InitPointofThisBlock.X + 2.200,
                    //    InitPointofThisBlock.Y + 2.200,
                    //    InitPoint.Z
                    //    );
                    
                    Vector3 InitPointofThisBlock = new Vector3(
                        InitPoint.X + (double)(blockXCounter) * ((0.210 - 0.01) * 10 - 0.030),//if x40 -> 2.150,
                        InitPoint.Y - (double)(blockYCounter) * ((0.180 - 0.01) * 10 - 0.030),//if x40 -> 2.170,
                        InitPoint.Z
                        );

                    Vector3 SurfPointofThisBlock = new Vector3(
                        InitPointofThisBlock.X + 1.000,
                        InitPointofThisBlock.Y - 1.000,
                        InitPoint.Z
                        );


                    //go to surface measurement 
                    mc.MovePoint(SurfPointofThisBlock.X, SurfPointofThisBlock.Y, sur.UpTop + 0.050);//above 50micron
                    mc.Join();

                    //surface landing
                    bool flag = true;
                    int layercounter = 0;
                    camera.Start();
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
                        layercounter++;

                        if (brightness > 10000 || layercounter > 30) flag = false;
                    }
                    led.AdjustLight(parameterManager);
                    camera.Stop();

                    //surface
                    double surfacetopz = mc.GetPoint().Z;
                    double surfacebottomz = surfacetopz - emthickness;


                    //data taking
                    int rowcounter = 0;
                    int colcounter = 0;
                    //while (rowcounter < 24) {
                    //    while (colcounter < 20) {
                    while (rowcounter < 12) {
                        while (colcounter < 10) {

                            string stlog = "";
                            byte[] bb = new byte[440 * 512 * nshot];
                            double startZ = 0.0;
                            PlusMinus plusminus;

                            if (colcounter%2==0) {
                                //camera.Start();
                                //led.AdjustLight(parameterManager);
                                //camera.Stop();
                                startZ = surfacetopz + 0.012;
                                plusminus = PlusMinus.Minus;
                            } else {
                                startZ = surfacebottomz - 0.009;
                                plusminus = PlusMinus.Plus;
                            }

                            double prev_z = startZ;

                            mc.MovePoint(
                                InitPointofThisBlock.X + (0.210 - 0.01) * colcounter, //x40, 0.230-0.01 //parameterManager.SpiralShiftX
                                InitPointofThisBlock.Y - (0.180 - 0.01) * rowcounter, //x40, 0.195-0.01 //parameterManager.SpiralShiftY
                                startZ);
                            mc.Join();
                    
                            p = mc.GetPoint();
                            DateTime starttime = System.DateTime.Now;
                            string datfileName = string.Format(@"{0}\{1}_x{2}_y{3}_xi{4}_yi{5}.dat",
                                direcotryPath,
                                starttime.ToString("yyyyMMdd_HHmmss"),
                                (int)(p.X * 1000),
                                (int)(p.Y * 1000),
                                colcounter,
                                rowcounter
                                );
                            BinaryWriter writer = new BinaryWriter(File.Open(datfileName, FileMode.Create));

                            mc.Inch(plusminus, 0.15, VectorId.Z);

                            int viewcounter = 0;
                            while (viewcounter < nshot + 3) {
                                byte[] b = Ipt.CaptureMain();
                                p = mc.GetPoint();
                                TimeSpan ts = System.DateTime.Now - starttime; 
                                stlog += String.Format("{0} {1} {2} {3} {4} {5} {6} {7}\n",
                                    colcounter%2,
                                    System.DateTime.Now.ToString("HHmmss\\.fff"),
                                    ts.ToString("s\\.fff"),
                                    (p.X * 1000).ToString("0.0"),
                                    (p.Y * 1000).ToString("0.0"),
                                    (p.Z * 1000).ToString("0.0"),
                                    (prev_z*1000 - p.Z*1000).ToString("0.0"),
                                    viewcounter);
                                prev_z = p.Z;

                                if (viewcounter >= 3) {
                                    b.CopyTo(bb, 440 * 512 * (viewcounter - 3));
                                }
                                viewcounter++;
                            }//view
                            viewcounter = 0;

                            mc.SlowDownStop(VectorId.Z);
                                                                
                            twriter.Write(stlog);
                            writer.Write(bb);
                            writer.Flush();
                            writer.Close();
                            colcounter++;
                        }//col
                        colcounter = 0;
                        rowcounter++;
                    }//row
                    rowcounter = 0;
                    twriter.Close();
                    blockXCounter++;
                }//blockX
                blockXCounter = 0;
                blockYCounter++;
            }//blockY
            blockYCounter = 0;
            camera.Start();

        }//end of task()

        private bool isValidate() {
            return true;
        }
    }
}
