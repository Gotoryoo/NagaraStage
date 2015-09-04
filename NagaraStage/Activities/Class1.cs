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
    class Class1 : Activity, IActivity {
        private static Class1 instance;

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

        public Class1(ParameterManager _paramaterManager)
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
        public static Class1 GetInstance(
            ParameterManager parameterManger = null) {
            if (instance == null) {
                instance = new Class1(parameterManger);
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


            List<Mat> image_set = new List<Mat>();
            Vector3 ggg = mc.GetPoint();

            string txtfileName = string.Format(@"c:\img\bbbbbb.txt");
            StreamWriter twriter = File.CreateText(txtfileName);
           // twriter.Write("line95\n");

            Vector3 initialpoint = mc.GetPoint();

            List<Vector2> list_lateralmark43 = new List<Vector2>();//in PL#43
            list_lateralmark43.Add(new Vector2(81.706, 89.124));
            list_lateralmark43.Add(new Vector2(-89.715, 83.350));
            list_lateralmark43.Add(new Vector2(-86.969, -87.964));
            list_lateralmark43.Add(new Vector2(83.521, -86.169));
            
            List<Vector2> list_scanregion43 = new List<Vector2>();
            list_scanregion43.Add(list_lateralmark43[0] - new Vector2(-5, -5));
            list_scanregion43.Add(list_lateralmark43[1] - new Vector2(+5, -5));
            list_scanregion43.Add(list_lateralmark43[2] - new Vector2(+5, +5));
            list_scanregion43.Add(list_lateralmark43[3] - new Vector2(-5, +5));

            
            List<Vector2> list_lateralmark42 = new List<Vector2>();//in PL#42
            list_lateralmark42.Add(new Vector2(81.750, 89.099));
            list_lateralmark42.Add(new Vector2(-89.526, 83.417));
            list_lateralmark42.Add(new Vector2(-86.754, -87.727));
            list_lateralmark42.Add(new Vector2(83.612, -85.831));

            List<Vector2> list_scanregion42 = new List<Vector2>();
            list_scanregion42.Add(list_lateralmark42[0] - new Vector2(-5, -5));
            list_scanregion42.Add(list_lateralmark42[1] - new Vector2(+5, -5));
            list_scanregion42.Add(list_lateralmark42[2] - new Vector2(+5, +5));
            list_scanregion42.Add(list_lateralmark42[3] - new Vector2(-5, +5));

            Affine ap_s43_s42 = Affine.CreateAffineBy(list_lateralmark43, list_lateralmark42);

            List<Vector2> list_scanpoint_g = new List<Vector2>();
            for (int x = 0; x < 11; x++) {
                for (int y = 0; y < 11; y++) {
                    list_scanpoint_g.Add(new Vector2(x * 1.0, y * 1.0));
                }
            }
            

            List<Vector2> list_scancorner_g = new List<Vector2>();
            list_scancorner_g.Add(new Vector2(10.0, 10.0));
            list_scancorner_g.Add(new Vector2(0.0, 10.0));
            list_scancorner_g.Add(new Vector2(0.0, 0.0));
            list_scancorner_g.Add(new Vector2(10.0, 0.0));
            
           
            Affine ap_g43_s43 = Affine.CreateAffineBy(list_scancorner_g, list_scanregion43);





            camera.Start();



            for (int d = 0; d < list_scanpoint_g.Count; d++) {

//                Vector2 predpoint41 = ap_g41_s41.Trance(list_scanpoint_g[d]);

                Vector2 predpoint43 = ap_g43_s43.Trance(list_scanpoint_g[d]);
                Vector2 predpoint42 = ap_s43_s42.Trance(predpoint43);


                mc.MoveTo(new Vector3(predpoint42.X, predpoint42.Y, initialpoint.Z - 0.100), new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                mc.Join();
                Thread.Sleep(4000);
                int ledbrightness = led.AdjustLight(parameterManager);

                int viewcounter = 0;
                bool flag = true;
                while (flag) {
                    mc.MoveDistance(+0.003, VectorId.Z);
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

                    if (brightness > 2000 || viewcounter > 100) flag = false;
                }//while-loop surface detection

                Vector3 surfacepoint = mc.GetPoint();

                for (int vy = 0; vy < 2; vy++) {
                    for (int vx = 0; vx < 2; vx++) {


                        mc.MoveTo
                            (new Vector3(
                                predpoint42.X -0.125/2.0 + 0.125 * vx,
                                predpoint42.Y -0.105/2.0 + 0.105 * vy,
                                surfacepoint.Z),
                            new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        mc.Join();


                        for (int q = 0; q < 8; q++) {
                            mc.MovePointZ(surfacepoint.Z + 0.003 + (0.003 * q));
                            mc.Join();

                            Vector3 currentpoint = mc.GetPoint();

                            byte[] b = camera.ArrayImage;
                            Mat image = new Mat(440, 512, MatType.CV_8U, b);
                            Mat clone_image = image.Clone();

                            //clone_image.ImWrite(String.Format(@"c:\img\{0}_{1:00}.bmp", d, q));
                            
                            clone_image.ImWrite(String.Format(@"c:\img\{0}_{1}_{2}_{3:00}.bmp",d, vx,vy, q));

                            string stlog = "";
                            stlog += String.Format("{0}   {1}  {2}  {3}\n",
                                    ledbrightness,
                                    currentpoint.X,
                                    currentpoint.Y,
                                    currentpoint.Z);
                            twriter.Write(stlog);
                           // twriter.Write("line198\n");
                            twriter.Flush();
                        }//for-loop q pictureID
               

                    }//for-loop vx view0,1
                }//for-loop vy view0,1

            }//for-loop d-th scanning point

            camera.Stop();
            twriter.Close();


        }

        private bool isValidate() {
            return true;
        }
    }
}
