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

/// <summary>
///　2015年5月にJ-PARCにてpbarを当てた7cm*7cmの乾板解析用に作ったもの。
///　乾板をスキャンし、dat形式というバイナリファイルに書き出す。
/// x方向に16視野、y方向に20視野からなる2mm*2mmの領域を1ブロックとし、これをbx*by回繰り返す。
/// </summary>


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
                    mc.SlowDownStopAll();
                }
            })));
            //SurfaceLanding sl = new SurfaceLanding(parameterManager);
            //taskList.AddRange(sl.CreateTask());
            return taskList;
        }


        private void surfrecog(int thre, double deltaz){

            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();

            bool flag = true;
            while (flag) {
                mc.MoveDistance(deltaz, VectorId.Z);
                mc.Join();
                Thread.Sleep(100);

                byte[] b = camera.ArrayImage;
                Mat src = new Mat(440, 512, MatType.CV_8U, b);
                Mat mat = src.Clone();

                Cv2.GaussianBlur(mat, mat, Cv.Size(3, 3), -1);
                Mat gau = mat.Clone();
                Cv2.GaussianBlur(gau, gau, Cv.Size(17, 17), -1);
                Cv2.Subtract(gau, mat, mat);
                Cv2.Threshold(mat, mat, 10, 1, ThresholdType.Binary);
                int brightness = Cv2.CountNonZero(mat);

                if (brightness > thre) flag = false;
            }
        }



        private void task() {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Surface sur = Surface.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            Led led = Led.GetInstance();
            CoordManager cm = new CoordManager(parameterManager);


            Vector3 initialpoint = mc.GetPoint();

            int pixthre = 500;


            for (int bx = -1; bx <= 1 ; bx++) {
                for (int by = -1; by <= 1 ; by++) {



                    string txtfileName = string.Format(@"E:\img\{0}_{1}.txt",bx,by);
                    StreamWriter twriter = File.CreateText(txtfileName);

                    Vector3 blockstartpoint = new Vector3();
                    blockstartpoint.X = initialpoint.X + bx * 1.0;
                    blockstartpoint.Y = initialpoint.Y + by * 1.0;
                    blockstartpoint.Z = initialpoint.Z;

                    mc.MoveTo(new Vector3(blockstartpoint.X + 0.5, blockstartpoint.Y + 0.5, initialpoint.Z));
                    mc.Join();

                    camera.Start();
                    int ledbrightness = led.AdjustLight(parameterManager);
                    camera.Stop();


                    //初期位置からはじめてZ方向正の向きにスキャン


                    for (int vy = 0; vy < 10; vy++) {
                        
                        Vector3 linestartpoint = mc.GetPoint();
                        linestartpoint.X = blockstartpoint.X;
                        linestartpoint.Y = blockstartpoint.Y + vy * parameterManager.SpiralShiftY;
                        linestartpoint.Z = initialpoint.Z;
                        
                        for (int vx = 0; vx < 8; ) {
                            
                            if (vx == 0) {
                                Vector3 approachingpoint = mc.GetPoint();
                                approachingpoint.X = blockstartpoint.X + vx * parameterManager.SpiralShiftX - 0.05;
                                approachingpoint.Y = blockstartpoint.Y + vy * parameterManager.SpiralShiftY - 0.05;
                                approachingpoint.Z = linestartpoint.Z;
                                mc.MoveTo(approachingpoint);
                                mc.Join();
                            }
                                                                                  
                            Vector3 viewstartpoint = mc.GetPoint();
                            viewstartpoint.X = blockstartpoint.X + vx * parameterManager.SpiralShiftX;
                            viewstartpoint.Y = blockstartpoint.Y + vy * parameterManager.SpiralShiftY;
                            viewstartpoint.Z = linestartpoint.Z;

                            mc.MoveTo(viewstartpoint);
                            mc.Join();
                            Thread.Sleep(100);

                            Vector3 viewpoint = mc.GetPoint();

                            int npict = 300;
                            byte[] bb = new byte[440 * 512 * npict];
                            string datfileName = string.Format(@"E:\img\u_{0}_{1}_{2}_{3}.dat",
                                (int)(viewpoint.X * 1000),
                                (int)(viewpoint.Y * 1000),
                                vx,
                                vy
                                );
                            BinaryWriter writer = new BinaryWriter(File.Open(datfileName, FileMode.Create));

                            mc.Inch(PlusMinus.Plus, 0.15/12, VectorId.Z);

                            int viewcounter = 0;
                            while (viewcounter < npict + 3) {
                                byte[] b = Ipt.CaptureMain();
                                Vector3 p = mc.GetPoint();

                                if (viewcounter >= 3) {
                                    b.CopyTo(bb, 440 * 512 * (viewcounter - 3));

                                    string stlog = "";
                                    stlog += String.Format("{0}   {1}  {2}  {3}\n",
                                            ledbrightness,
                                            p.X,
                                            p.Y,
                                            p.Z);
                                    twriter.Write(stlog);
                                }
                                viewcounter++;
                            }//view
                            viewcounter = 0;
                            double endz = mc.GetPoint().Z;

                            mc.SlowDownStop(VectorId.Z);
                            mc.Join();
                            Thread.Sleep(100);

                            if (endz - viewstartpoint.Z < 0.15) {
                                vx++;
                                writer.Write(bb);
                                writer.Flush();
                                writer.Close();
                            }
                        }//vx
                    }//vy











                    camera.Stop();
                    twriter.Close();

                }//blocky
            }//blockx
            
        }//task

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

