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
    class GridMeasure : Activity, IActivity {
        private static GridMeasure instance;

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

        public GridMeasure(ParameterManager _paramaterManager)
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
        public static GridMeasure GetInstance(
            ParameterManager parameterManger = null) {
            if (instance == null) {
                instance = new GridMeasure(parameterManger);
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

            List<Vector2> list_grid_pred = new List<Vector2>();
            List<Vector2> list_grid_meas = new List<Vector2>();
            List<Vector2> list_grid_part = new List<Vector2>();
            List<Vector2> list_grid_meas2 = new List<Vector2>();


            /*
            list_grid_pred.Add(new Vector2( 0.0, 0.0));
            list_grid_pred.Add(new Vector2(30.0, 0.0));
            list_grid_pred.Add(new Vector2( 0.0, 30.0));
            list_grid_pred.Add(new Vector2(30.0, 30.0));

            list_grid_pred.Add(new Vector2(60.0, 0.0));
            list_grid_pred.Add(new Vector2(0.0, 60.0));
            list_grid_pred.Add(new Vector2(60.0, 60.0));




            //list_grid_pred.Add(new Vector2(0.0, 10.0));
            //list_grid_pred.Add(new Vector2(10.0, 0.0));
            //list_grid_pred.Add(new Vector2(10.0, 10.0));

            //list_grid_pred.Add(new Vector2(20.0, 0.0));
            //list_grid_pred.Add(new Vector2(0.0, 20.0));
            //list_grid_pred.Add(new Vector2(20.0, 20.0));

            //list_grid_pred.Add(new Vector2(20.0, 10.0));
            //list_grid_pred.Add(new Vector2(30.0, 0.0));
            //list_grid_pred.Add(new Vector2(30.0, 10.0));
            //list_grid_pred.Add(new Vector2(10.0, 20.0));
            //list_grid_pred.Add(new Vector2(30.0, 20.0));
            //list_grid_pred.Add(new Vector2(10.0, 30.0));
            //list_grid_pred.Add(new Vector2(20.0, 30.0));

            camera.Start();

            Affine ap = new Affine(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);
            for (int i = 0; i < list_grid_pred.Count; i++) {
                Vector2 predpoint = ap.Trance(list_grid_pred[i]);
                //mc.MovePoint(predpoint.X, predpoint.Y, InitPoint.Z + 0.030);
                mc.MoveTo(new Vector3(predpoint.X, predpoint.Y, InitPoint.Z + 0.030), new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                mc.Join();

                led.AdjustLight(parameterManager);


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

                    if (brightness > 5000 ) flag = false;
                }

                Thread.Sleep(100);

                mc.SetSpiralCenterPoint();
                flag = true;
                int counter = 0;
                while (flag) {
                    if(counter!=0){
                       mc.MoveInSpiral(true);
                       mc.Join();
                    }
                    counter++;

                    Camera c = Camera.GetInstance();
                    byte[] b = c.ArrayImage;
                    Mat mat = new Mat(440, 512, MatType.CV_8U, b);
                    Cv2.GaussianBlur(mat, mat, Cv.Size(7, 7), -1);

                    Cv2.Threshold(mat, mat, 60, 255, ThresholdType.BinaryInv);

                    Moments mom = new Moments(mat);
                    if (mom.M00 < 1000 * 255) continue;

                    double cx = mom.M10 / mom.M00;
                    double cy = mom.M01 / mom.M00;
                    Mat innercir = Mat.Zeros(440, 512, MatType.CV_8UC1);
                    Cv2.Circle(innercir, new Point(cx, cy), 10, new Scalar(255, 255, 255), 3);
                    int innerpath = Cv2.CountNonZero(innercir);
                    Cv2.BitwiseAnd(innercir, mat, innercir);
                    int innersum = Cv2.CountNonZero(innercir);

                    Mat outercir = Mat.Zeros(440, 512, MatType.CV_8UC1);
                    Cv2.Circle(outercir, new Point(cx, cy), 100, new Scalar(255, 255, 255), 3);
                    int outerpath = Cv2.CountNonZero(outercir);
                    Cv2.BitwiseAnd(outercir, mat, outercir);
                    int outersum = Cv2.CountNonZero(outercir);

                    double innerratio = innersum * 1.0 / innerpath * 1.0;
                    double outerratio = outersum * 1.0 / outerpath * 1.0;

                    if (innerratio < 0.8) continue;
                    if (outerratio > 0.2) continue;

                    flag = false;


                    //
                    Vector2 grid_meas = cm.TransToEmulsionCoord((int)(cx), (int)(cy));

                    Vector3 to = new Vector3(grid_meas.X, grid_meas.Y, mc.GetPoint().Z);
                    mc.MoveTo(to, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                    mc.Join();

                    b = c.ArrayImage;
                    mat = new Mat(440, 512, MatType.CV_8U, b);
                    Cv2.GaussianBlur(mat, mat, Cv.Size(7, 7), -1);

                    Cv2.Threshold(mat, mat, 60, 255, ThresholdType.BinaryInv);

                    mom = new Moments(mat);
                    if (mom.M00 < 1000 * 255) continue;

                    cx = mom.M10 / mom.M00;
                    cy = mom.M01 / mom.M00;
                    innercir = Mat.Zeros(440, 512, MatType.CV_8UC1);
                    Cv2.Circle(innercir, new Point(cx, cy), 10, new Scalar(255, 255, 255), 3);
                    innerpath = Cv2.CountNonZero(innercir);
                    Cv2.BitwiseAnd(innercir, mat, innercir);
                    innersum = Cv2.CountNonZero(innercir);

                    outercir = Mat.Zeros(440, 512, MatType.CV_8UC1);
                    Cv2.Circle(outercir, new Point(cx, cy), 100, new Scalar(255, 255, 255), 3);
                    outerpath = Cv2.CountNonZero(outercir);
                    Cv2.BitwiseAnd(outercir, mat, outercir);
                    outersum = Cv2.CountNonZero(outercir);

                    innerratio = innersum * 1.0 / innerpath * 1.0;
                    outerratio = outersum * 1.0 / outerpath * 1.0;

                    if (innerratio < 0.8) continue;
                    if (outerratio > 0.2) continue;

                    grid_meas = cm.TransToEmulsionCoord((int)(cx), (int)(cy));
                    System.Diagnostics.Debug.WriteLine(string.Format("gridmark {0} {1}", grid_meas.X, grid_meas.Y));


                    list_grid_meas.Add(grid_meas);
                    list_grid_part.Add(list_grid_pred[i]);
                    if (i >= 3) {
                        ap = Affine.CreateAffineBy(list_grid_part, list_grid_meas);
                    }
                }


            }//for(int i = 0; i < list_grid_pred.Count; i++)
            */

            Affine ap = new Affine(1.00055550, -0.00152264, 0.00174182, 1.00096792, 12.7498, -62.5798);

            string txtfileName = string.Format(@"c:\img\grid_g.txt");
            StreamWriter twriter = File.CreateText(txtfileName);

            for (int x = 0; x < 6; x++) {
                for (int y = 0; y < 6; y++) {
                    //if (x == 0 && y == 5) continue;
                    //if (x == 1 && y == 5) continue;


                    Vector2 predpoint = ap.Trance(new Vector2(x * 10, y * 10));
                    mc.MoveTo(new Vector3(predpoint.X, predpoint.Y, InitPoint.Z + 0.030), new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                    mc.Join();

                    led.AdjustLight(parameterManager);

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

                        if (brightness > 5000) flag = false;
                    }

                    Thread.Sleep(100);

                    byte[] b2 = camera.ArrayImage;
                    Mat src2 = new Mat(440, 512, MatType.CV_8U, b2);
                    Mat mat2 = src2.Clone();
                    string FileName = string.Format(@"C:\img\x{0}_y{1}.bmp", x, y);
                    Cv2.ImWrite(FileName, mat2);

                    
                    Cv2.GaussianBlur(mat2, mat2, Cv.Size(7, 7), -1);

                    Cv2.Threshold(mat2, mat2, 60, 255, ThresholdType.BinaryInv);

                    Moments mom2 = new Moments(mat2);
                   
                    double cx2 = mom2.M10 / mom2.M00;
                    double cy2 = mom2.M01 / mom2.M00;
                    
           /*       Mat innercir = Mat.Zeros(440, 512, MatType.CV_8UC1);
                    Cv2.Circle(innercir, new Point(cx2, cy2), 10, new Scalar(255, 255, 255), 3);
                    int innerpath = Cv2.CountNonZero(innercir);
                    Cv2.BitwiseAnd(innercir, mat2, innercir);
                    int innersum = Cv2.CountNonZero(innercir);

                    Mat outercir = Mat.Zeros(440, 512, MatType.CV_8UC1);
                    Cv2.Circle(outercir, new Point(cx2, cy2), 100, new Scalar(255, 255, 255), 3);
                    int outerpath = Cv2.CountNonZero(outercir);
                    Cv2.BitwiseAnd(outercir, mat2, outercir);
                    int outersum = Cv2.CountNonZero(outercir);

                    double innerratio = innersum * 1.0 / innerpath * 1.0;
                    double outerratio = outersum * 1.0 / outerpath * 1.0;
                    */
                   

                    Vector2 grid_meas2 = cm.TransToEmulsionCoord((int)(cx2), (int)(cy2));



                    FileName = string.Format(@"C:\img\after_x{0}_y{1}.bmp", x, y);
                    Cv2.ImWrite(FileName, mat2 );
                    
                    string stlog = "";
                    stlog += String.Format("{0} {1} {2:f4} {3:f4} {4:f4} {5:f4}\n",
                            x*10,
                            y*10,
                            predpoint.X,
                            predpoint.Y,
                            grid_meas2.X,
                            grid_meas2.Y);
                    twriter.Write(stlog);
                    
                }//for y
            }//for x

            twriter.Close();
        }




        private bool isValidate() {
            return true;
        }
    }
}
