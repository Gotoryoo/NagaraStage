using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

using System.Windows;

using NagaraStage.IO;
using NagaraStage.Parameter;

using System.Diagnostics;

using NagaraStage.Ui;

using OpenCvSharp;
using OpenCvSharp.CPlusPlus;

using NagaraStage.Parameter;

namespace NagaraStage.Activities {
    class StartButton : Activity, IActivity {
        private static StartButton instance;

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

        public StartButton(ParameterManager _paramaterManager)
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
        public static StartButton GetInstance(
            ParameterManager parameterManger = null) {
            if (instance == null) {
                instance = new StartButton(parameterManger);
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



        public struct rawmicrotrack
        {//Raw microtrack

            public int ph;
            public int pv;
            public int ax;
            public int ay;
            public int cx;
            public int cy;
        }

        static OpenCvSharp.CPlusPlus.Point TrackDetection(List<Mat> mats, int px, int py, int shiftx = 2, int shifty = 2, int shiftpitch = 4, int windowsize = 40, int phthresh = 5, bool debugflag = false)
        {
            int x0 = px - 256;
            int y0 = py - 220;

            List<rawmicrotrack> rms = new List<rawmicrotrack>();

            // Point2d pixel_cen = TrackDetection(binimages, 256, 220, 3, 3, 4, 90, 3);


            int counter = 0;
            for (int ax = -shiftx; ax <= shiftx; ax++)
            {
                for (int ay = -shifty; ay <= shifty; ay++)
                {
                    using (Mat big = Mat.Zeros(600, 600, MatType.CV_8UC1))
                    using (Mat imgMask = Mat.Zeros(big.Height, big.Width, MatType.CV_8UC1))
                    {

                        //make the size of mask
                        int ystart = big.Height / 2 + y0 - windowsize / 2;
                        int yend = big.Height / 2 + y0 + windowsize / 2;
                        int xstart = big.Width / 2 + x0 - windowsize / 2;
                        int xend = big.Width / 2 + x0 + windowsize / 2;

                        //make mask as shape of rectangle. by use of opencv
                        OpenCvSharp.CPlusPlus.Rect recMask = new OpenCvSharp.CPlusPlus.Rect(xstart, ystart, windowsize, windowsize);
                        Cv2.Rectangle(imgMask, recMask, 255, -1);//brightness=1, fill

                        for (int p = 0; p < mats.Count; p++)
                        {
                            int startx = big.Width / 2 - mats[p].Width / 2 + (int)(p * ax * shiftpitch / 8.0);
                            int starty = big.Height / 2 - mats[p].Height / 2 + (int)(p * ay * shiftpitch / 8.0);
                            Cv2.Add(
                                big[starty, starty + mats[p].Height, startx, startx + mats[p].Width],
                                mats[p],
                                big[starty, starty + mats[p].Height, startx, startx + mats[p].Width]);
                        }

                        using (Mat big_c = big.Clone())
                        {

                            Cv2.Threshold(big, big, phthresh, 255, ThresholdType.ToZero);
                            Cv2.BitwiseAnd(big, imgMask, big);

                            //Mat roi = big[ystart, yend , xstart, xend];//メモリ領域がシーケンシャルにならないから輪郭抽出のときに例外が出る。

                            if (debugflag == true)
                            {//
                                //bigorg.ImWrite(String.Format(@"{0}_{1}_{2}.png",counter,ax,ay));
                                //Mat roiwrite = roi.Clone() * 30;
                                //roiwrite.ImWrite(String.Format(@"roi_{0}_{1}_{2}.png", counter, ax, ay));
                                Cv2.Rectangle(big_c, recMask, 255, 1);//brightness=1, fill
                                Cv2.ImShow("big_cx30", big_c * 30);
                                Cv2.ImShow("bigx30", big * 30);
                                //Cv2.ImShow("imgMask", imgMask);
                                //Cv2.ImShow("roi", roi * 30);
                                Cv2.WaitKey(0);
                            }
                        }//using big_c

                        using (CvMemStorage storage = new CvMemStorage())
                        using (CvContourScanner scanner = new CvContourScanner(big.ToIplImage(), storage, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxSimple))
                        {
                            foreach (CvSeq<CvPoint> c in scanner)
                            {
                                CvMoments mom = new CvMoments(c, false);
                                if (c.ElemSize < 2) continue;
                                if (mom.M00 < 1.0) continue;
                                double mx = mom.M10 / mom.M00;
                                double my = mom.M01 / mom.M00;
                                rawmicrotrack rm = new rawmicrotrack();
                                rm.ax = ax;
                                rm.ay = ay;
                                rm.cx = (int)(mx - big.Width / 2);
                                rm.cy = (int)(my - big.Height / 2);
                                rm.pv = (int)(mom.M00);
                                rms.Add(rm);
                                //Console.WriteLine(string.Format("{0}   {1} {2}   {3} {4}", rm.pv, ax, ay, rm.cx, rm.cy ));
                            }
                        }//using contour

                        //big_c.Dispose();

                        counter++;


                    }//using Mat
                }//ay
            }//ax



            OpenCvSharp.CPlusPlus.Point trackpos = new OpenCvSharp.CPlusPlus.Point(0, 0);
            if (rms.Count > 0)
            {
                rawmicrotrack rm = new rawmicrotrack();
                double meancx = 0;
                double meancy = 0;
                double meanax = 0;
                double meanay = 0;
                double meanph = 0;
                double meanpv = 0;
                double sumpv = 0;

                for (int i = 0; i < rms.Count; i++)
                {
                    meanpv += rms[i].pv * rms[i].pv;
                    meancx += rms[i].cx * rms[i].pv;
                    meancy += rms[i].cy * rms[i].pv;
                    meanax += rms[i].ax * rms[i].pv;
                    meanay += rms[i].ay * rms[i].pv;
                    sumpv += rms[i].pv;
                }

                meancx /= sumpv;//重心と傾きを輝度値で重み付き平均
                meancy /= sumpv;
                meanax /= sumpv;
                meanay /= sumpv;
                meanpv /= sumpv;

                trackpos = new OpenCvSharp.CPlusPlus.Point(
                    (int)(meancx) + 256 - meanax * shiftpitch,
                    (int)(meancy) + 220 - meanay * shiftpitch
                    );

                double anglex = (meanax * shiftpitch * 0.267) / (3.0 * 7.0 * 2.2);
                double angley = (meanay * shiftpitch * 0.267) / (3.0 * 7.0 * 2.2);
                Console.WriteLine(string.Format("{0:f4} {1:f4}", anglex, angley));
            }
            else
            {
                trackpos = new OpenCvSharp.CPlusPlus.Point(-1, -1);
            }


            return trackpos;
        }//track detection

        public struct ImageTaking
        {
            public Vector3 StageCoord;
            public Mat img;
        }

        List<ImageTaking> TakeSequentialImage(double ax, double ay, double dz, int nimage)
        {

            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();

            Vector3 initialpos = mc.GetPoint();
            List<ImageTaking> lit = new List<ImageTaking>();

            for (int i = 0; i < nimage; i++)
            {
                Vector3 dstpoint = new Vector3(
                    initialpos.X + ax * dz * i,
                    initialpos.Y + ay * dz * i,
                    initialpos.Z + dz * i
                );
                mc.MovePoint(dstpoint);
                mc.Join();

                byte[] b = camera.ArrayImage;
                Mat image = new Mat(440, 512, MatType.CV_8U, b);
                ImageTaking it = new ImageTaking();
                it.img = image.Clone();
                it.StageCoord = mc.GetPoint();
                lit.Add(it);

                //image.Release();
                //imagec.Release();
            }

            return lit;
        }

        static Mat DifferenceOfGaussian(Mat image, int kernel = 51)
        {
            Mat gau = new Mat(440, 512, MatType.CV_8U);
            Mat dst = new Mat(440, 512, MatType.CV_8U);

            Cv2.GaussianBlur(image, image, Cv.Size(3, 3), -1);//
            Cv2.GaussianBlur(image, gau, Cv.Size(kernel, kernel), -1);//パラメータ見ないといけない。
            Cv2.Subtract(gau, image, dst);

            gau.Dispose();
            return dst;
        }

        static Mat DogContrastBinalize(Mat image, int kernel = 51, int threshold = 100, ThresholdType thtype = ThresholdType.Binary)
        {
            Mat img = DifferenceOfGaussian(image, kernel);

            double Max_kido;
            double Min_kido;
            OpenCvSharp.CPlusPlus.Point maxloc;
            OpenCvSharp.CPlusPlus.Point minloc;
            Cv2.MinMaxLoc(img, out Min_kido, out Max_kido, out minloc, out maxloc);

            Cv2.ConvertScaleAbs(img, img, 255 / (Max_kido - Min_kido), -255 * Min_kido / (Max_kido - Min_kido));
            Cv2.Threshold(img, img, threshold, 1, thtype);

            return img;
        }

        static Point2d ApproximateStraight(double sh, Point3d ltrack, Point3d ltrack2, Point3d ltrack3)
        {
            Point2d p = new Point2d();

            int n = 3;

            double ltrack2_sh = (ltrack2.Z - ltrack.Z) * sh + ltrack.Z;
            double ltrack3_sh = (ltrack3.Z - ltrack2.Z) * sh + (ltrack2.Z - ltrack.Z) * sh + ltrack.Z;

            double sum_X = ltrack.X + ltrack2.X + ltrack3.X;
            double sum_Y = ltrack.Y + ltrack2.Y + ltrack3.Y;
            double sum_Z = ltrack.Z + ltrack2_sh + ltrack3_sh;
            double sum_ZZ = ltrack.Z * ltrack.Z + ltrack2_sh * ltrack2_sh + ltrack3_sh * ltrack3_sh;
            double sum_XZ = ltrack.Z * ltrack.X + ltrack2_sh * ltrack2.X + ltrack3_sh * ltrack3.X;

            double sum_YZ = ltrack.Z * ltrack.Y + ltrack2_sh * ltrack2.Y + ltrack3_sh * ltrack3.Y;

            double angle_x = (n * sum_XZ - sum_X * sum_Z) / (n * sum_ZZ - sum_Z * sum_Z);
            double angle_y = (n * sum_YZ - sum_Y * sum_Z) / (n * sum_ZZ - sum_Z * sum_Z);


            p = new Point2d(angle_x, angle_y);

            return p;
        }

        static Point2d ApproximateStraightBase(double sh, double sh_low, Point3d ltrack, Point3d ltrack2, Point3d ltrack3, Surface surface)
        {
            Point2d p = new Point2d();

            int n = 3;
            double ltrack2_sh;
            double ltrack3_sh;

            //ltrack2の座標とbase上面、下ゲル上面の距離の差を求める。二乗してあるのは負の値にならないようにするためである。
            double lt2_upbottom_dis = ltrack2.Z - surface.UpBottom;
            double lt2_upb_pow = Math.Pow(lt2_upbottom_dis, 2);

            double lt2_lowtop_dis = ltrack2.Z - surface.LowTop;
            double lt2_lowt_pow = Math.Pow(lt2_lowtop_dis, 2);


            if (lt2_upb_pow < lt2_lowt_pow)//ltrack2の座標とbase上面の距離が、ltrack2の座標と下ゲル上面の距離より小さい場合、ltrack2はbase上にあることになる。
            {
                ltrack2_sh = (ltrack2.Z - ltrack.Z) * sh + ltrack.Z;//ltrack2とltrack3の間にbaseがある場合。
                ltrack3_sh = (ltrack3.Z - surface.LowTop) * sh_low + (surface.LowTop - ltrack2.Z) + (ltrack2.Z - ltrack.Z) * sh + ltrack.Z;
            }
            else//ltrack2の座標とbase上面の距離が、ltrack2の座標と下ゲル上面の距離より大きい場合、ltrack2は下ゲル上にあることになる。
            {
                ltrack2_sh = (ltrack2.Z - surface.LowTop) * sh_low + (surface.LowTop - ltrack.Z) + ltrack.Z;//ltrackとltrack2の間にbaseがある場合。
                ltrack3_sh = (ltrack3.Z - ltrack2.Z) * sh_low + (ltrack2.Z - surface.LowTop) * sh_low + (surface.LowTop - ltrack.Z) + ltrack.Z;
            }

            double sum_X = ltrack.X + ltrack2.X + ltrack3.X;
            double sum_Y = ltrack.Y + ltrack2.Y + ltrack3.Y;
            double sum_Z = ltrack.Z + ltrack2_sh + ltrack3_sh;
            double sum_ZZ = ltrack.Z * ltrack.Z + ltrack2_sh * ltrack2_sh + ltrack3_sh * ltrack3_sh;
            double sum_XZ = ltrack.Z * ltrack.X + ltrack2_sh * ltrack2.X + ltrack3_sh * ltrack3.X;

            double sum_YZ = ltrack.Z * ltrack.Y + ltrack2_sh * ltrack2.Y + ltrack3_sh * ltrack3.Y;

            double angle_x = (n * sum_XZ - sum_X * sum_Z) / (n * sum_ZZ - sum_Z * sum_Z);
            double angle_y = (n * sum_YZ - sum_Y * sum_Z) / (n * sum_ZZ - sum_Z * sum_Z);


            p = new Point2d(angle_x, angle_y);

            return p;
        }

        private void BeamDetection(string outputfilename, bool isup)
        {// beam Detection

            int BinarizeThreshold = 60;
            int BrightnessThreshold = 4;
            int nop = 7;

            double dz = 0;
            if (isup == true)
            {
                dz = -0.003;
            }
            else
            {
                dz = 0.003;
            }

            Camera camera = Camera.GetInstance();
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Vector3 InitPoint = mc.GetPoint();
            Vector3 p = new Vector3();
            TracksManager tm = parameterManager.TracksManager;
            int mod = parameterManager.ModuleNo;
            int pl = parameterManager.PlateNo;
            Track myTrack = tm.GetTrack(tm.TrackingIndex);
            string[] sp = myTrack.IdString.Split('-');

            //string datfileName = string.Format("{0}.dat", System.DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            string datfileName = string.Format(@"c:\test\bpm\{0}\{1}-{2}-{3}-{4}-{5}.dat", mod, mod, pl, sp[0], sp[1], System.DateTime.Now.ToString("ddHHmmss"));
            BinaryWriter writer = new BinaryWriter(File.Open(datfileName, FileMode.Create));
            byte[] bb = new byte[440 * 512 * nop];

            string fileName = string.Format("{0}", outputfilename);
            StreamWriter twriter = File.CreateText(fileName);
            string stlog = "";


            List<ImageTaking> LiIT = TakeSequentialImage(0.0, 0.0, dz, nop);

            Mat sum = Mat.Zeros(440, 512, MatType.CV_8UC1);
            for (int i = 0; i < LiIT.Count; i++)
            {
                Mat bin = (Mat)DogContrastBinalize(LiIT[i].img, 31, BinarizeThreshold);
                Cv2.Add(sum, bin, sum);
                //byte[] b = LiIT[i].img.ToBytes();//format is .png
                MatOfByte mob = new MatOfByte(LiIT[i].img);
                byte[] b = mob.ToArray();
                b.CopyTo(bb, 440 * 512 * i);
            }

            mc.MovePointZ(InitPoint.Z);
            mc.Join();


            Cv2.Threshold(sum, sum, BrightnessThreshold, 1, ThresholdType.Binary);


            //Cv2.FindContoursをつかうとAccessViolationExceptionになる(Release/Debug両方)ので、C-API風に書く
            using (CvMemStorage storage = new CvMemStorage())
            {
                using (CvContourScanner scanner = new CvContourScanner(sum.ToIplImage(), storage, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxSimple))
                {
                    //string fileName = string.Format(@"c:\img\{0}.txt",
                    //        System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"));

                    foreach (CvSeq<CvPoint> c in scanner)
                    {
                        CvMoments mom = new CvMoments(c, false);
                        if (c.ElemSize < 2) continue;
                        if (mom.M00 == 0.0) continue;
                        double mx = mom.M10 / mom.M00;
                        double my = mom.M01 / mom.M00;
                        stlog += string.Format("{0:F} {1:F}\n", mx, my);
                    }
                }
            }

            twriter.Write(stlog);
            twriter.Close();

            writer.Write(bb);
            writer.Flush();
            writer.Close();

            sum *= 255;
            sum.ImWrite(String.Format(@"c:\img\{0}_{1}_{2}.bmp",
                System.DateTime.Now.ToString("yyyyMMdd_HHmmss"),
                (int)(p.X * 1000),
                (int)(p.Y * 1000)));

        }//BeamDetection

        private void task() {

            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            TracksManager tm = parameterManager.TracksManager;
            Track myTrack = tm.GetTrack(tm.TrackingIndex);
            Surface surface = Surface.GetInstance(parameterManager);
            Stopwatch sw = new Stopwatch();
            //CoordManager coordManager;
            int mod = parameterManager.ModuleNo;
            int pl = parameterManager.PlateNo;

            sw.Start();

            try
            {
                //MotorControler mc = MotorControler.GetInstance(parameterManager);
                GridMark nearestMark = coordManager.GetTheNearestGridMark(mc.GetPoint());
                System.Diagnostics.Debug.WriteLine(String.Format("{0},  {1}", nearestMark.x, nearestMark.y));
                mc.MovePointXY(nearestMark.x, nearestMark.y);
                mc.Join();
            }
            catch (GridMarkNotFoundException ex)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("{0}", ex.ToString()));
            }
            try
            {
                // MotorControler mc = MotorControler.GetInstance(parameterManager);
                IGridMarkRecognizer GridMarkRecognizer = coordManager;
                mc.SetSpiralCenterPoint();
                Led led = Led.GetInstance();
                Vector2 encoderPoint = new Vector2(-1, -1);
                encoderPoint.X = mc.GetPoint().X;
                encoderPoint.Y = mc.GetPoint().Y;//おこられたのでしかたなくこうする　吉田20150427
                Vector2 viewerPoint = new Vector2(-1, -1);

                bool continueFlag = true;
                while (continueFlag)
                {
                    led.AdjustLight(parameterManager);
                    viewerPoint = GridMarkRecognizer.SearchGridMarkx50();
                    if (viewerPoint.X < 0 || viewerPoint.Y < 0)
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("grid mark not found"));
                        mc.MoveInSpiral(true);
                        mc.Join();
                        continueFlag = (mc.SpiralIndex < 30);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("******** {0}  {1}", viewerPoint.X, viewerPoint.Y));
                        encoderPoint = coordManager.TransToEmulsionCoord(viewerPoint);
                        mc.MovePointXY(encoderPoint);
                        mc.Join();
                        continueFlag = false;
                    }
                } // while

                //重心検出と移動を2回繰り返して、グリッドマークを視野中心にもっていく
                mc.MovePointXY(encoderPoint);
                mc.Join();
                viewerPoint = GridMarkRecognizer.SearchGridMarkx50();
                encoderPoint = coordManager.TransToEmulsionCoord(viewerPoint);
                mc.MovePointXY(encoderPoint);
                mc.Join();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("exception");
            }

            //.........Get value of HFDX and HFDY................//

            try
            {

                Vector3 CurrentCenterPoint = mc.GetPoint();
                GridMark nearestMark = coordManager.GetTheNearestGridMark(CurrentCenterPoint);
                System.Diagnostics.Debug.WriteLine(String.Format("{0},  {1}", CurrentCenterPoint.X, CurrentCenterPoint.Y));
                coordManager.HFDX = CurrentCenterPoint.X - nearestMark.x;
                coordManager.HFDY = CurrentCenterPoint.Y - nearestMark.y;
            }
            catch (EntryPointNotFoundException ex)
            {
                MessageBox.Show("エントリポイントが見当たりません。 " + ex.Message);
                System.Diagnostics.Debug.WriteLine("エントリポイントが見当たりません。 " + ex.Message);
            }

            //.......　Move to track place after revised with shift value.....//

            double dstx = myTrack.MsX + coordManager.HFDX;
            double dsty = myTrack.MsY + coordManager.HFDY;
            mc.MovePointXY(dstx, dsty, delegate {
                stage.WriteLine(Properties.Strings.MovingComplete);
            });

            mc.Join();

            Led led_ = Led.GetInstance();
            led_.AdjustLight(parameterManager);
            Thread.Sleep(500); //Wait for 5s

            //////////////////////////////Surfacerecog/////////////////////////////////////.................................../////////

            try
            {
                if (mc.IsMoving)
                {
                    MessageBoxResult r = MessageBox.Show(
                        Properties.Strings.SurfaceException01,
                        Properties.Strings.Abort + "?",
                        MessageBoxButton.YesNo);
                    if (r == MessageBoxResult.Yes)
                    {
                        mc.AbortMoving();
                    }
                    else
                    {
                        return;
                    }
                }

                //Surface surface = Surface.GetInstance(parameterManager);
                if (surface.IsActive)
                {
                    MessageBoxResult r = MessageBox.Show(
                        Properties.Strings.SurfaceException02,
                        Properties.Strings.Abort + "?",
                        MessageBoxButton.YesNo);
                    if (r == MessageBoxResult.Yes)
                    {
                        surface.Abort();
                    }
                    else
                    {
                        return;
                    }
                }

                try
                {
                    surface.Start(true);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Properties.Strings.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Properties.Strings.Error);
            }
            mc.Join();

            //...............Beam Pattern Matching..................////////////////

            try
            {

                string datarootdirpath = string.Format(@"C:\test\bpm\{0}", mod);
                System.IO.DirectoryInfo mydir = System.IO.Directory.CreateDirectory(datarootdirpath);
                string[] sp = myTrack.IdString.Split('-');
                string uptxt = string.Format(@"c:\test\bpm\{0}\{1}-{2}-{3}-{4}_up.txt", mod, mod, pl, sp[0], sp[1]);
                string dwtxt = string.Format(@"c:\test\bpm\{0}\{1}-{2}-{3}-{4}_dw.txt", mod, mod, pl - 1, sp[0], sp[1]);
                BeamDetection(uptxt, true);

                BeamPatternMatch bpm = new BeamPatternMatch(8, 200);
                bpm.ReadTrackDataTxtFile(dwtxt, false);

                bpm.ReadTrackDataTxtFile(uptxt, true);
                bpm.DoPatternMatch();

                stage.WriteLine(String.Format("pattern match dx,dy = {0}, {1}", bpm.GetPeakX() * 0.2625 * 0.001, bpm.GetPeakY() * 0.2625 * 0.001));
                Vector3 BfPoint = mc.GetPoint();
                mc.MoveDistance(bpm.GetPeakX() * 0.2625 * 0.001, VectorId.X);
                mc.Join();
                mc.MoveDistance(-bpm.GetPeakY() * 0.2625 * 0.001, VectorId.Y);
                mc.Join();
                Led led = Led.GetInstance();
                led.AdjustLight(parameterManager);
                Vector3 AfPoint = mc.GetPoint();
                stage.WriteLine(String.Format("Move dx,dy = {0}, {1}", BfPoint.X - AfPoint.X, BfPoint.Y - AfPoint.Y));

            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("ID numver is not existed。 ");
            }
            catch (System.Exception)
            {
                MessageBox.Show("No beam battern。 ");
            }
            // }
            /////////////////////////////SurfaceRecog////////////////////////////////////////////////////////////////////////////

            Vector3 initial = mc.GetPoint();///////initialpoint

            double Sh = 0.5 / (surface.UpTop - surface.UpBottom);

            //ここから角度によって撮影間隔を変更するように書き換える。
            double tansi = Math.Sqrt(myTrack.MsDX * myTrack.MsDX + myTrack.MsDY * myTrack.MsDY);
            double theta = Math.Atan(tansi);
            //絶対値の大きさを入れる。dzはマイナスの値になるようにする。         
            double dz;

            double dz_price_img = (6 * Math.Cos(theta) / Sh) / 1000;
            double dz_img = dz_price_img * (-1);
            //
            string datarootdirpathw = string.Format(@"C:\test\{0}", myTrack.IdString);
            System.IO.DirectoryInfo mydir_ = System.IO.Directory.CreateDirectory(datarootdirpathw);
            //

            //必要なlistをまとめる
            //List<ImageTaking> LiITUpTrack = new List<ImageTaking>();

            List<OpenCvSharp.CPlusPlus.Point2d> Msdxdy = new List<OpenCvSharp.CPlusPlus.Point2d>();
            Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(myTrack.MsDX, myTrack.MsDY));

            List<OpenCvSharp.CPlusPlus.Point3d> LStage = new List<OpenCvSharp.CPlusPlus.Point3d>();
            List<OpenCvSharp.CPlusPlus.Point> LPeak = new List<OpenCvSharp.CPlusPlus.Point>();//i番目の画像で実際に見つかったトラックの座標。pixel座標で視野中心からの差分。
            List<Point3d> LTrack = new List<Point3d>();//i番目の画像で実際に見つかったトラックの座標のステージ座標

            List<List<ImageTaking>> UpTrackInfo = new List<List<ImageTaking>>();

            //エラー防止のために下ゲルの処理の際に必要なListをここに移動した。
            double Sh_low;
            Sh_low = 0.5 / (surface.LowTop - surface.LowBottom);

            List<OpenCvSharp.CPlusPlus.Point2d> Msdxdy_Low = new List<OpenCvSharp.CPlusPlus.Point2d>();

            //List<ImageTaking> LiITLowMid = new List<ImageTaking>();
            List<OpenCvSharp.CPlusPlus.Point3d> LStage_Low = new List<OpenCvSharp.CPlusPlus.Point3d>();
            List<OpenCvSharp.CPlusPlus.Point> LPeak_Low = new List<OpenCvSharp.CPlusPlus.Point>();//i番目の画像で実際に見つかったトラックの座標。pixel座標で視野中心からの差分。
            List<Point3d> LTrack_Low = new List<Point3d>();//i番目の画像で実際に見つかったトラックの座標のステージ座標
            List<List<ImageTaking>> LowTrackInfo = new List<List<ImageTaking>>();


            //今までのtrack追跡プログラムとは異なる角度等の使い方をする。
            dz_price_img = (6 * Math.Cos(theta) / Sh) / 1000;
            dz_img = dz_price_img * (-1);
            dz = dz_img;

            int gotobase = 0;
            int not_detect = 0;

            for (int i = 0; gotobase < 1; i++)
            {
                ///////移動して画像処理をしたときに、baseの中に入らないようにする。
                Vector3 initialpos = mc.GetPoint();
                double moverange = 7 * dz_img;
                double predpoint = moverange + initialpos.Z;

                if (predpoint < surface.UpBottom)//もしもbaseに入りそうなら、8枚目の画像がちょうどbaseを撮影するようにdzを調整する。
                {
                    gotobase = 1;

                    dz = surface.UpBottom - initialpos.Z + 7 * dz_price_img;
                }
                ////////

                //gotobase = 1のときは、移動して画像を撮影するようにする。
                if (i != 0)
                {
                    Vector3 dstpoint = new Vector3(
                        LTrack[i - 1].X + Msdxdy[i].X * dz * Sh,
                        LTrack[i - 1].Y + Msdxdy[i].Y * dz * Sh,
                        LTrack[i - 1].Z + dz
                        );
                    mc.MovePoint(dstpoint);
                    mc.Join();
                }

                List<ImageTaking> LiITUpMid = TakeSequentialImage(
                    Msdxdy[i].X * Sh,
                    Msdxdy[i].Y * Sh,
                    dz_img,
                    8);

                ////画像の保存、座標の保存。
                LStage.Add(new OpenCvSharp.CPlusPlus.Point3d(LiITUpMid[7].StageCoord.X, LiITUpMid[7].StageCoord.Y, LiITUpMid[7].StageCoord.Z));
                LiITUpMid[7].img.ImWrite(datarootdirpathw + string.Format(@"\img_l_up_{0}.bmp", i));

                UpTrackInfo.Add(LiITUpMid);//撮影した8枚の画像と、撮影した位置を記録する。

                //撮影した画像をここで処理する。
                List<Mat> binimages = new List<Mat>();
                for (int t = 0; t <= 7; t++)
                {
                    Mat bin = (Mat)DogContrastBinalize(LiITUpMid[t].img);

                    double xx = myTrack.MsDX * myTrack.MsDX;
                    double yy = myTrack.MsDY * myTrack.MsDY;
                    if (Math.Sqrt(xx + yy) >= 0.4)
                    {
                        Cv2.Dilate(bin, bin, new Mat());
                    }
                    Cv2.Dilate(bin, bin, new Mat());
                    binimages.Add(bin);
                }

                //trackを重ねる処理を入れる。
                Point2d pixel_cen = TrackDetection(binimages, 256, 220, 3, 3, 4, 90, 3);//画像の8枚目におけるtrackのpixel座標を算出する。

                if (pixel_cen.X == -1 & pixel_cen.Y == -1)
                {
                    //追跡に失敗した時に最後に検出したtrack座標に移動してから、追跡に失敗した地点の画像を撮影するようにする。
                    //mc.MovePoint(LTrack[i - 1].X, LTrack[i - 1].Y, LTrack[i - 1].Z);
                    mc.Join();

                    not_detect = 1;
                    goto not_detect_track;
                }

                //検出したpixel座標をstage座標に変換するなどlistに追加する。

                LPeak.Add(new OpenCvSharp.CPlusPlus.Point(pixel_cen.X - 256, pixel_cen.Y - 220));

                double firstx = LStage[i].X - LPeak[i].X * 0.000267;
                double firsty = LStage[i].Y + LPeak[i].Y * 0.000267;
                double firstz = LStage[i].Z;
                LTrack.Add(new Point3d(firstx, firsty, firstz));
                //

                //上側乳剤層上面の1回目のtrack検出によって、次のtrackの位置を検出する角度を求める。
                //その角度が、1回目のtrack検出の結果によって大きな角度にならないように調整をする。

                if (i == 0)
                {
                    Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(myTrack.MsDX, myTrack.MsDY));

                }
                else if (i == 1)
                {
                    List<Point3d> LTrack_ghost = new List<Point3d>();
                    double dzPrev = (LStage[0].Z - surface.UpTop) * Sh;
                    double Lghost_x = LTrack[0].X - Msdxdy[i].X * dzPrev;
                    double Lghost_y = LTrack[0].Y - Msdxdy[i].Y * dzPrev;
                    LTrack_ghost.Add(new Point3d(Lghost_x, Lghost_y, surface.UpTop));//上側乳剤層上面にtrackがあるならどの位置にあるかを算出する。

                    string txtfileName_ltrackghost = datarootdirpathw + string.Format(@"\LTrack_ghost.txt");
                    StreamWriter twriter_ltrackghost = File.CreateText(txtfileName_ltrackghost);
                    twriter_ltrackghost.WriteLine("{0} {1} {2}", LTrack_ghost[0].X, LTrack_ghost[0].Y, LTrack_ghost[0].Z);
                    twriter_ltrackghost.Close();

                    OpenCvSharp.CPlusPlus.Point2d Tangle = ApproximateStraight(Sh, LTrack_ghost[0], LTrack[0], LTrack[1]);
                    Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle.X, Tangle.Y));

                }
                else
                {
                    OpenCvSharp.CPlusPlus.Point2d Tangle = ApproximateStraight(Sh, LTrack[i - 2], LTrack[i - 1], LTrack[i]);
                    Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle.X, Tangle.Y));
                }


            }//for　i-loop

            //baseまたぎ
            int ltrack_counter = LTrack.Count();
            int msdxdy_counter = Msdxdy.Count();

            mc.MovePoint(
                LTrack[ltrack_counter - 1].X + Msdxdy[msdxdy_counter - 1].X * (surface.LowTop - surface.UpBottom),
                LTrack[ltrack_counter - 1].Y + Msdxdy[msdxdy_counter - 1].Y * (surface.LowTop - surface.UpBottom),
                surface.LowTop
                );
            mc.Join();

            //////ここから下gelの処理
            Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Msdxdy[msdxdy_counter - 1].X, Msdxdy[msdxdy_counter - 1].Y));

            //今までのtrack追跡プログラムとは異なる角度等の使い方をする。
            dz_price_img = (6 * Math.Cos(theta) / Sh_low) / 1000;
            dz_img = dz_price_img * (-1);
            dz = dz_img;

            int goto_dgel = 0;

            for (int i = 0; goto_dgel < 1; i++)
            {
                ///////移動して画像処理をしたときに、下gelの下に入らないようにする。
                Vector3 initialpos = mc.GetPoint();
                double moverange = 7 * dz_img;
                double predpoint = moverange + initialpos.Z;

                if (predpoint < surface.LowBottom)//もしもbaseに入りそうなら、8枚目の画像がちょうど下gelを撮影するようにdzを調整する。
                {
                    goto_dgel = 1;

                    dz = surface.LowBottom - initialpos.Z + 7 * dz_price_img;
                }
                ////////

                //goto_dgel == 1のときは、移動して画像を撮影するようにする。
                if (i != 0)
                {
                    Vector3 dstpoint = new Vector3(
                    LTrack_Low[i - 1].X + Msdxdy_Low[i].X * dz * Sh_low,
                    LTrack_Low[i - 1].Y + Msdxdy_Low[i].Y * dz * Sh_low,
                    LTrack_Low[i - 1].Z + dz
                    );
                    mc.MovePoint(dstpoint);
                    mc.Join();
                }

                //今までのtrack追跡プログラムとは異なる角度等の使い方をする。
                List<ImageTaking> LiITLowMid = TakeSequentialImage(
                    Msdxdy[i].X * Sh_low,
                    Msdxdy[i].Y * Sh_low,
                    dz_img,
                    8);

                //画像・座標の記録
                LStage_Low.Add(new OpenCvSharp.CPlusPlus.Point3d(LiITLowMid[7].StageCoord.X, LiITLowMid[7].StageCoord.Y, LiITLowMid[7].StageCoord.Z));
                LiITLowMid[7].img.ImWrite(datarootdirpathw + string.Format(@"\img_l_low_{0}.bmp", i));

                LowTrackInfo.Add(LiITLowMid);//撮影した8枚の画像と、撮影した位置を記録する。

                //撮影した画像をここで処理する。
                List<Mat> binimages = new List<Mat>();
                for (int t = 0; t <= 7; t++)
                {
                    Mat bin = (Mat)DogContrastBinalize(LiITLowMid[t].img);

                    double xx = myTrack.MsDX * myTrack.MsDX;
                    double yy = myTrack.MsDY * myTrack.MsDY;
                    if (Math.Sqrt(xx + yy) >= 0.4)
                    {
                        Cv2.Dilate(bin, bin, new Mat());
                    }
                    Cv2.Dilate(bin, bin, new Mat());
                    binimages.Add(bin);
                }
                //trackを重ねる処理を入れる。
                Point2d pixel_cen = TrackDetection(binimages, 256, 220, 3, 3, 4, 90, 3);//画像の8枚目におけるtrackのpixel座標を算出する。

                //もし検出に失敗した場合はループを抜ける。
                if (pixel_cen.X == -1 & pixel_cen.Y == -1)
                {
                    //mc.MovePoint(LTrack_Low[i - 1].X, LTrack_Low[i - 1].Y, LTrack_Low[i - 1].Z);
                    mc.Join();

                    not_detect = 1;
                    goto not_detect_track;
                }
                //

                //検出したpixel座標をstage座標に変換するなどlistに追加する。
                LPeak_Low.Add(new OpenCvSharp.CPlusPlus.Point(pixel_cen.X - 256, pixel_cen.Y - 220));

                double firstx = LStage_Low[i].X - LPeak_Low[i].X * 0.000267;
                double firsty = LStage_Low[i].Y + LPeak_Low[i].Y * 0.000267;
                double firstz = LStage_Low[i].Z;
                LTrack_Low.Add(new Point3d(firstx, firsty, firstz));
                //

                //ここからは、最小二乗法で角度を算出するプログラムである。
                //上側乳剤層上面の1回目のtrack検出によって、次のtrackの位置を検出する角度を求める。
                //その角度が、1回目のtrack検出の結果によって大きな角度にならないように調整をする。
                if (i == 0)
                {
                    OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraightBase(Sh, Sh_low, LTrack[ltrack_counter - 2], LTrack[ltrack_counter - 1], LTrack_Low[i], surface);
                    Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                }
                else if (i == 1)
                {
                    OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraightBase(Sh, Sh_low, LTrack[ltrack_counter - 1], LTrack_Low[i - 1], LTrack_Low[i], surface);
                    Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                }
                else
                {
                    OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraight(Sh_low, LTrack_Low[i - 2], LTrack_Low[i - 1], LTrack_Low[i]);
                    Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                }


            }//i_loop

            //
            int ltrack_low_count = LTrack_Low.Count();
            mc.MovePointXY(LTrack_Low[ltrack_low_count - 1].X, LTrack_Low[ltrack_low_count - 1].Y);

            mc.Join();

            //検出に失敗した場合は、ループを抜けてここに来る。
            not_detect_track:;//検出に失敗したと考えられる地点で画像を取得し、下ゲル下面まで移動する。(現在は下ゲル下面とするが、今後変更する可能性有。)

            if (not_detect != 0)
            {
                //写真撮影
                List<ImageTaking> NotDetect = TakeSequentialImage(
                        Msdxdy[0].X * Sh,
                        Msdxdy[0].Y * Sh,
                        0,
                        1);

                string txtfileName_t_not_detect = datarootdirpathw + string.Format(@"\not_detect.txt");
                StreamWriter twriter_t_not_detect = File.CreateText(txtfileName_t_not_detect);
                for (int i = 0; i < NotDetect.Count; i++)
                {
                    NotDetect[i].img.ImWrite(datarootdirpathw + string.Format(@"\img_t_not_detect.bmp"));
                    Vector3 p = NotDetect[i].StageCoord;
                    twriter_t_not_detect.WriteLine("{0} {1} {2} {3}", i, p.X, p.Y, p.Z);
                }
                twriter_t_not_detect.Close();

                mc.MovePointZ(surface.LowBottom);

                mc.Join();
            }


            //file write out up_gel
            string txtfileName_sh_up = datarootdirpathw + string.Format(@"\Sh_up.txt");
            StreamWriter twriter_sh_up = File.CreateText(txtfileName_sh_up);
            twriter_sh_up.WriteLine("{0}", Sh);
            twriter_sh_up.Close();

            //file write out
            string txtfileName_t_info_up = datarootdirpathw + string.Format(@"\location_up.txt");
            StreamWriter twriter_t_info_up = File.CreateText(txtfileName_t_info_up);
            for (int i = 0; i < UpTrackInfo.Count; i++)
            {
                for (int t = 0; t < UpTrackInfo[i].Count; t++)
                {
                    UpTrackInfo[i][t].img.ImWrite(datarootdirpathw + string.Format(@"\img_t_info_up_{0}-{1}.bmp", i, t));
                    Vector3 p = UpTrackInfo[i][t].StageCoord;
                    twriter_t_info_up.WriteLine("{0} {1} {2} {3} {4}", i, t, p.X, p.Y, p.Z);
                }
            }
            twriter_t_info_up.Close();

            string txtfileName_lpeak = datarootdirpathw + string.Format(@"\lpeak_up.txt");
            StreamWriter twriter_lpeak = File.CreateText(txtfileName_lpeak);
            for (int i = 0; i < LPeak.Count(); i++)
            {
                OpenCvSharp.CPlusPlus.Point p = LPeak[i];
                twriter_lpeak.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_lpeak.Close();

            string txtfileName_ltrack = datarootdirpathw + string.Format(@"\ltrack_up.txt");
            StreamWriter twriter_ltrack = File.CreateText(txtfileName_ltrack);
            for (int i = 0; i < LTrack.Count(); i++)
            {
                OpenCvSharp.CPlusPlus.Point3d p = LTrack[i];
                twriter_ltrack.WriteLine("{0} {1} {2} {3}", i, p.X, p.Y, p.Z);
            }
            twriter_ltrack.Close();

            string txtfileName_msdxdy = datarootdirpathw + string.Format(@"\msdxdy.txt");
            StreamWriter twriter_msdxdy = File.CreateText(txtfileName_msdxdy);
            for (int i = 0; i < Msdxdy.Count(); i++)
            {
                OpenCvSharp.CPlusPlus.Point2d p = Msdxdy[i];
                twriter_msdxdy.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_msdxdy.Close();


            //file write out low_gel
            string txtfileName_sh_low = datarootdirpathw + string.Format(@"\Sh_low.txt");
            StreamWriter twriter_sh_low = File.CreateText(txtfileName_sh_low);
            twriter_sh_low.WriteLine("{0}", Sh_low);
            twriter_sh_low.Close();

            string txtfileName_t_info_low = datarootdirpathw + string.Format(@"\location_low.txt");
            StreamWriter twriter_t_info_low = File.CreateText(txtfileName_t_info_low);
            for (int i = 0; i < LowTrackInfo.Count; i++)
            {
                for (int t = 0; t < LowTrackInfo[i].Count; t++)
                {
                    LowTrackInfo[i][t].img.ImWrite(datarootdirpathw + string.Format(@"\img_t_info_low_{0}-{1}.bmp", i, t));
                    Vector3 p = LowTrackInfo[i][t].StageCoord;
                    twriter_t_info_low.WriteLine("{0} {1} {2} {3} {4}", i, t, p.X, p.Y, p.Z);
                }
            }
            twriter_t_info_low.Close();

            string txtfileName_lpeak_low = datarootdirpathw + string.Format(@"\lpeak_low.txt");
            StreamWriter twriter_lpeak_low = File.CreateText(txtfileName_lpeak_low);
            for (int i = 0; i < LPeak_Low.Count(); i++)
            {
                OpenCvSharp.CPlusPlus.Point p = LPeak_Low[i];
                twriter_lpeak_low.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_lpeak_low.Close();

            string txtfileName_ltrack_low = datarootdirpathw + string.Format(@"\ltrack_low.txt");
            StreamWriter twriter_ltrack_low = File.CreateText(txtfileName_ltrack_low);
            for (int i = 0; i < LTrack_Low.Count(); i++)
            {
                OpenCvSharp.CPlusPlus.Point3d p = LTrack_Low[i];
                twriter_ltrack_low.WriteLine("{0} {1} {2} {3}", i, p.X, p.Y, p.Z);
            }
            twriter_ltrack_low.Close();

            string txtfileName_msdxdy_low = datarootdirpathw + string.Format(@"\msdxdy_low.txt");
            StreamWriter twriter_msdxdy_low = File.CreateText(txtfileName_msdxdy_low);
            for (int i = 0; i < Msdxdy_Low.Count(); i++)
            {
                OpenCvSharp.CPlusPlus.Point2d p = Msdxdy_Low[i];
                twriter_msdxdy_low.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_msdxdy_low.Close();//*/

            //.................Taking Photo in buttom of down layer.......................//

            try
            {
                string[] sp = myTrack.IdString.Split('-');
                string dwtxt = string.Format(@"c:\test\bpm\{0}\{1}-{2}-{3}-{4}_dw.txt", mod, mod, pl, sp[0], sp[1]);
                string datarootdirpath = string.Format(@"C:\test\bpm\{0}", mod);
                System.IO.DirectoryInfo mydir = System.IO.Directory.CreateDirectory(datarootdirpath);
                System.IO.DirectoryInfo mydir2 = System.IO.Directory.CreateDirectory(datarootdirpath);
                BeamDetection(dwtxt, false);
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("ID is not exist ");
            }

            //...............Move to top surfacr of upper layer........................//
            try
            {
                Vector3 cc = mc.GetPoint();
                double Zp = surface.UpTop;
                mc.MoveTo(new Vector3(cc.X, cc.Y, Zp));
                mc.Join();
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Cannot move to top surface of upperlayer ");
            }

            // .......................................................................//

            sw.Stop();
            string[] sp1 = myTrack.IdString.Split('-');
            string logtxt_ = string.Format(@"c:\test\bpm\{0}\{1}-{2}_log_.txt", mod, mod, pl);
            //string log_ = string.Format("{0} \n", sw.Elapsed);
            string log_ = string.Format("{0} {1} {2} \n", sp1[0], sp1[1], sw.Elapsed);
            StreamWriter swr = new StreamWriter(logtxt_, true, Encoding.ASCII);
            swr.Write(log_);
            swr.Close();

        }

        private bool isValidate() {
            return true;
        }
    }
}
