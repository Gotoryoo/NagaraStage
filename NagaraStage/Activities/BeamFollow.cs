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
    class BeamFollow : Activity, IActivity {
        private static BeamFollow instance;

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

        public BeamFollow(ParameterManager _paramaterManager)
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
        public static BeamFollow GetInstance(
            ParameterManager parameterManger = null) {
            if (instance == null) {
                instance = new BeamFollow(parameterManger);
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



        public struct pozi
        {
            public Point3d img;//画像の撮影した地点の座標。
            public Point2d peak;//beamのpixel座標。
            public Point3d stage;//beamのstage座標。

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

        static OpenCvSharp.CPlusPlus.Point TrackDetection_verold(List<Mat> mats, int px, int py, int shiftx = 2, int shifty = 2, int shiftpitch = 4, int windowsize = 40, int phthresh = 5, bool debugflag = false)
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

        private void BeamFollower(Track myTrack, int mod, int pl, bool dubflag)
        {

            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            Surface surface = Surface.GetInstance(parameterManager);
            Vector3 initial = mc.GetPoint();

            string uptxt = string.Format(@"c:\GTR_test\bpm\{0}_{1}_up.txt", 10, 10);
            BeamDetection(uptxt, true);
            List<Point2d> BPM_pix = new List<Point2d>();//beamのピクセル座標を格納するListである。

            string line;

            System.IO.StreamReader file = new System.IO.StreamReader(uptxt);
            while ((line = file.ReadLine()) != null)
            {
                string[] data = line.Split(' ');
                double sx = double.Parse(data[0]);
                double sy = double.Parse(data[1]);
                BPM_pix.Add(new Point2d(sx, sy));
            }
            file.Close();

            //ここからは、画像の中心(256,220)に近いいくつかのbeamの座標を選択し、それを追跡するように修正する。
            List<Point2d> some_bpm = new List<Point2d>();
            List<Point3d> point = new List<Point3d>();
            List<Point2d> point_10 = new List<Point2d>();

            for (int i = 0; i < BPM_pix.Count(); i++)
            {
                List<Point3d> point2 = new List<Point3d>();
                for (int r = 0; r < BPM_pix.Count(); r++)
                {
                    Point3d p = new Point3d();
                    double dx = BPM_pix[i].X - BPM_pix[r].X;
                    double dy = BPM_pix[i].Y - BPM_pix[r].Y;
                    double dr = Math.Sqrt(dx * dx + dy * dy);

                    if (dr < 7)
                    {//この7という数字は、windowsizeが一辺10ピクセルのため、全体で見た時に7ピクセル離れていれば良いだろうと判断し、このようにした。
                        p.X = 10;
                        p.Y = 10;
                        p.Z = 10;
                        point2.Add(p);
                    }
                }//for r

                if (point2.Count() == 1)
                {
                    Point2d bem = new Point2d();
                    bem.X = BPM_pix[i].X;
                    bem.Y = BPM_pix[i].Y;
                    point_10.Add(bem);
                }

            }//for i

            //ここまで
            int bemcount = 0;
            if (point_10.Count() >= 5)
            {
                bemcount = 5;
            }
            else
            {
                bemcount = point_10.Count();
            }

            for (int i = 0; i < bemcount; i++)//ここで、領域における分け方も含めてbeamを選択できるようにする。
            {
                some_bpm.Add(new Point2d(point_10[i].X, point_10[i].Y));
            }
            //ただ、とりあえずこれで作動はするであろう形になった。

            List<List<pozi>> LBeam = new List<List<pozi>>();
            List<List<pozi>> LBeam_Low = new List<List<pozi>>();

            List<pozi> c2 = new List<pozi>();
            List<Point2d> PM_result = some_bpm;//パターンマッチの結果から取得したbeamのpixel座標
            for (int a = 0; a < PM_result.Count(); a++)
            {
                pozi c3 = new pozi();
                c3.img.X = initial.X;
                c3.img.Y = initial.Y;
                c3.img.Z = initial.Z;

                c3.peak = PM_result[a];

                double firstx = c3.img.X - (c3.peak.X - 256) * 0.000267;
                double firsty = c3.img.Y + (c3.peak.Y - 220) * 0.000267;
                double firstz = c3.img.Z;

                c3.stage = new Point3d(firstx, firsty, firstz);

                c2.Add(c3);
            }
            LBeam.Add(c2);//第一層目でのbeamの情報をぶち込む。

            //for up layer

            int number_of_images = 7;
            int hits = 4;

            double Sh = 0.5 / (surface.UpTop - surface.UpBottom);
            double theta = Math.Atan(0);

            double dz;
            double dz_price_img = (6 * Math.Cos(theta) / Sh) / 1000;
            double dz_img = dz_price_img * (-1);

            string datarootdirpath = string.Format(@"C:\GTR_test\{0}", myTrack.IdString);//Open forder to store track information
            System.IO.DirectoryInfo mydir = System.IO.Directory.CreateDirectory(datarootdirpath);

            List<OpenCvSharp.CPlusPlus.Point2d> Msdxdy = new List<OpenCvSharp.CPlusPlus.Point2d>();//stage移動のための角度を入れるList。
            Msdxdy.Add(new OpenCvSharp.CPlusPlus.Point2d(0.0, 0.0));//最初はbeamが垂直に原子核乾板に照射されていると考えて（0.0,0.0）を入れた。

            List<OpenCvSharp.CPlusPlus.Point3d> LStage = new List<OpenCvSharp.CPlusPlus.Point3d>();//ImageTakeingで撮影した画像の最後の画像の座標を入れるList。
            List<OpenCvSharp.CPlusPlus.Point3d> LCenter = new List<OpenCvSharp.CPlusPlus.Point3d>();//検出したbemaのずれから算出した、本来の画像の中心点のstage座標。

            List<List<ImageTaking>> UpTrackInfo = new List<List<ImageTaking>>();

            //for down layer................................................................
            //            tl.Rec("down");
            double Sh_low;
            Sh_low = 0.5 / (surface.LowTop - surface.LowBottom);

            List<OpenCvSharp.CPlusPlus.Point2d> Msdxdy_Low = new List<OpenCvSharp.CPlusPlus.Point2d>();
            List<OpenCvSharp.CPlusPlus.Point3d> LStage_Low = new List<OpenCvSharp.CPlusPlus.Point3d>();

            List<OpenCvSharp.CPlusPlus.Point3d> LCenter_Low = new List<OpenCvSharp.CPlusPlus.Point3d>();//検出したbemaのずれから算出した、本来の画像の中心点のstage座標。

            List<List<ImageTaking>> LowTrackInfo = new List<List<ImageTaking>>();


            dz_price_img = (6 * Math.Cos(theta) / Sh) / 1000;
            dz_img = dz_price_img * (-1);
            dz = dz_img;
            int gotobase = 0;
            int not_detect = 0;

            for (int i = 0; gotobase < 1; i++)
            {
                Vector3 initialpos = mc.GetPoint();
                double moverange = (number_of_images - 1) * dz_img;
                double predpoint = moverange + initialpos.Z;

                if (predpoint < surface.UpBottom)
                {
                    gotobase = 1;

                    dz = surface.UpBottom - initialpos.Z + (number_of_images - 1) * dz_price_img;
                }

                //gotobase = 1のときは、移動して画像を撮影するようにする。
                if (i != 0)//このままでOK
                {
                    Vector3 dstpoint = new Vector3(
                        LCenter[i - 1].X + Msdxdy[i].X * dz * Sh,
                        LCenter[i - 1].Y + Msdxdy[i].Y * dz * Sh,
                        LCenter[i - 1].Z + dz
                        );
                    mc.MovePoint(dstpoint);
                    mc.Join();
                }

                List<ImageTaking> LiITUpMid = TakeSequentialImage( //image taking
                    Msdxdy[i].X * Sh,//Dx
                    Msdxdy[i].Y * Sh,//Dy
                    dz_img,//Dz
                    number_of_images);//number of images


                LStage.Add(new OpenCvSharp.CPlusPlus.Point3d(
                    LiITUpMid[number_of_images - 1].StageCoord.X,
                    LiITUpMid[number_of_images - 1].StageCoord.Y,
                    LiITUpMid[number_of_images - 1].StageCoord.Z
                    ));//撮影した画像の最後の画像の座標を LStage に代入する。

                LiITUpMid[number_of_images - 1].img.ImWrite(datarootdirpath + string.Format(@"\img_l_up_{0}.png", i));//最後の画像だけ別で保存。
                UpTrackInfo.Add(LiITUpMid);//撮影した画像すべてを UpTrackInfo に代入。

                List<Mat> binimages = new List<Mat>();//撮影した画像に対して画像処理をかける。
                for (int t = 0; t <= number_of_images - 1; t++)
                {
                    Mat bin = (Mat)DogContrastBinalize(LiITUpMid[t].img, 31, 60);
                    Cv2.Dilate(bin, bin, new Mat());
                    binimages.Add(bin);
                }

                List<pozi> beam_data = new List<pozi>();//各stepごとで検出したbeamそれぞれのデータを格納する。
                List<Point2d> MSDXDY_BEAM = new List<Point2d>();//それぞれのbeamから算出した角度を格納するList。
                for (int r = 0; r < LBeam[0].Count(); r++) //検出したbeamの数だけ処理を行うようにする。
                {
                    pozi beam_pozi = new pozi();
                    beam_pozi.img = LStage[i];//画像の撮影場所を格納する。

                    //trackを重ねる処理を入れる。
                    Point2d beam_peak = TrackDetection_verold(//シフトしないようにして、処理を行うようにしよう。
                        binimages,
                        (int)LBeam[i][r].peak.X,
                        (int)LBeam[i][r].peak.Y,
                        0,//shiftx もともと　3
                        0,//shifty もともと　3
                        4,
                        10,//windowsize もともと　90
                        hits, true);// true);

                    if (beam_peak.X == -1 & beam_peak.Y == -1)
                    {//検出できなかった時にどのような処理を行うのかを考えたほうがいいだろうな。
                        mc.Join();
                        not_detect = 1;

                        //goto not_detect_track; とりあえずコメントアウトしておく
                    }

                    beam_pozi.peak.X = beam_peak.X;
                    beam_pozi.peak.Y = beam_peak.Y;

                    double firstx = beam_pozi.img.X - (beam_pozi.peak.X - 256) * 0.000267;
                    double firsty = beam_pozi.img.Y + (beam_pozi.peak.Y - 220) * 0.000267;
                    double firstz = beam_pozi.img.Z;

                    beam_pozi.stage = new Point3d(firstx, firsty, firstz);

                    beam_data.Add(beam_pozi);

                }//r_loop

                Point2d Ms_esti = new Point2d();
                double Ms_x = new double();
                double Ms_y = new double();

                int pix_dX = new int();
                int pix_dY = new int();

                LBeam.Add(beam_data);

                for (int k = 0; k < LBeam[i].Count(); k++)
                {
                    if (i == 0)
                    {
                        MSDXDY_BEAM.Add(new OpenCvSharp.CPlusPlus.Point2d(Msdxdy[i].X, Msdxdy[i].Y));
                    }
                    else
                    {
                        if (i == 1)
                        {
                            Point3d LTrack_ghost = new Point3d();
                            double dzPrev = (LBeam[i][k].stage.Z - surface.UpTop) * Sh;
                            double Lghost_x = LBeam[i][k].stage.X - Msdxdy[i].X * dzPrev;
                            double Lghost_y = LBeam[i][k].stage.Y - Msdxdy[i].Y * dzPrev;
                            LTrack_ghost = new Point3d(Lghost_x, Lghost_y, surface.UpTop);//上側乳剤層上面にtrackがあるならどの位置にあるかを算出する。

                            OpenCvSharp.CPlusPlus.Point2d Tangle = ApproximateStraight(Sh, LTrack_ghost, LBeam[i][k].stage, LBeam[i + 1][k].stage);//ここを2点で行うようにする。
                            MSDXDY_BEAM.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle.X, Tangle.Y));
                        }
                        else
                        {
                            OpenCvSharp.CPlusPlus.Point2d Tangle = ApproximateStraight(Sh, LBeam[i - 1][k].stage, LBeam[i][k].stage, LBeam[i + 1][k].stage);
                            MSDXDY_BEAM.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle.X, Tangle.Y));
                        }
                    }
                }

                for (int q = 0; q < MSDXDY_BEAM.Count(); q++) //ここで個々のbeamの角度を平均してstageの移動角度を算出する。
                {
                    Ms_x += MSDXDY_BEAM[q].X;
                    Ms_y += MSDXDY_BEAM[q].Y;

                    //pix_dX += (int)LBeam[i][q - 1].peak.X - (int)LBeam[i][q].peak.X;//q - 1 がおかしい。ここで何をしたかったのかを思い出そう。
                    //pix_dY += (int)LBeam[i][q - 1].peak.Y - (int)LBeam[i][q].peak.Y;//予想した地点とのピクセルのズレかな？

                    pix_dX += (int)LBeam[i + 1][q].peak.X - (int)LBeam[i][q].peak.X;
                    pix_dY += (int)LBeam[i + 1][q].peak.Y - (int)LBeam[i][q].peak.Y;
                }
                Ms_x = Ms_x / MSDXDY_BEAM.Count();
                Ms_y = Ms_y / MSDXDY_BEAM.Count();
                Ms_esti = new Point2d(Ms_x, Ms_y);
                Msdxdy.Add(Ms_esti);//算出した角度をぶち込む。
                //LBeam.Add(beam_data);

                pix_dX = pix_dX / MSDXDY_BEAM.Count();//ずれたピクセル量
                pix_dY = pix_dY / MSDXDY_BEAM.Count();//ずれたピクセル量
                double cenX = LStage[i].X - pix_dX * 0.000267;
                double cenY = LStage[i].Y + pix_dY * 0.000267;
                double cenZ = LStage[i].Z;

                LCenter.Add(new Point3d(cenX, cenY, cenZ));//検出したそれぞれのbeamのズレから算出したパターンマッチの際の中心座標(stage)。

            }//for　i-loop

            //baseまたぎ
            int lcen_counter = LCenter.Count();
            int msdxdy_counter = Msdxdy.Count();

            mc.MovePoint(
                LCenter[lcen_counter - 1].X + Msdxdy[msdxdy_counter - 1].X * (surface.LowTop - surface.UpBottom),
                LCenter[lcen_counter - 1].Y + Msdxdy[msdxdy_counter - 1].Y * (surface.LowTop - surface.UpBottom),
                surface.LowTop
                );
            mc.Join();

            //////ここから下gelの処理
            Msdxdy_Low.Add(new OpenCvSharp.CPlusPlus.Point2d(Msdxdy[msdxdy_counter - 1].X, Msdxdy[msdxdy_counter - 1].Y));
            int lbeam_counter = LBeam.Count();
            LBeam_Low.Add(LBeam[lbeam_counter - 1]);

            //今までのtrack追跡プログラムとは異なる角度等の使い方をする。
            dz_price_img = (6 * Math.Cos(theta) / Sh_low) / 1000;
            dz_img = dz_price_img * (-1);
            dz = dz_img;

            int goto_dgel = 0;

            for (int i = 0; goto_dgel < 1; i++)
            {
                ///////移動して画像処理をしたときに、下gelの下に入らないようにする。
                Vector3 initialpos = mc.GetPoint();
                double moverange = (number_of_images - 1) * dz_img;
                double predpoint = moverange + initialpos.Z;

                if (predpoint < surface.LowBottom)//もしもbaseに入りそうなら、8枚目の画像がちょうど下gelを撮影するようにdzを調整する。
                {
                    goto_dgel = 1;

                    dz = surface.LowBottom - initialpos.Z + (number_of_images - 1) * dz_price_img;
                }
                ////////

                //goto_dgel == 1のときは、移動して画像を撮影するようにする。
                if (i != 0)
                {
                    Vector3 dstpoint = new Vector3(
                    LCenter_Low[i - 1].X + Msdxdy_Low[i].X * dz * Sh_low,
                    LCenter_Low[i - 1].Y + Msdxdy_Low[i].Y * dz * Sh_low,
                    LCenter_Low[i - 1].Z + dz
                    );
                    mc.MovePoint(dstpoint);
                    mc.Join();
                }

                //今までのtrack追跡プログラムとは異なる角度等の使い方をする。
                List<ImageTaking> LiITLowMid = TakeSequentialImage(
                    Msdxdy_Low[i].X * Sh_low,
                    Msdxdy_Low[i].Y * Sh_low,
                    dz_img,
                    number_of_images);

                //画像・座標の記録
                LStage_Low.Add(new OpenCvSharp.CPlusPlus.Point3d(
                    LiITLowMid[number_of_images - 1].StageCoord.X,
                    LiITLowMid[number_of_images - 1].StageCoord.Y,
                    LiITLowMid[number_of_images - 1].StageCoord.Z));

                LiITLowMid[number_of_images - 1].img.ImWrite(datarootdirpath + string.Format(@"\img_l_low_{0}.png", i));

                LowTrackInfo.Add(LiITLowMid);//撮影した8枚の画像と、撮影した位置を記録する。

                //撮影した画像をここで処理する。
                List<Mat> binimages = new List<Mat>();
                for (int t = 0; t <= number_of_images - 1; t++)
                {
                    Mat bin = (Mat)DogContrastBinalize(LiITLowMid[t].img, 31, 60);
                    Cv2.Dilate(bin, bin, new Mat());
                    binimages.Add(bin);
                }

                List<pozi> beam_data_low = new List<pozi>();//各stepごとで検出したbeamそれぞれのデータを格納する。
                List<Point2d> MSDXDY_BEAM_LOW = new List<Point2d>();//それぞれのbeamから算出した角度を格納するList。
                for (int r = 0; r < LBeam_Low[0].Count(); r++)
                {
                    pozi beam_pozi = new pozi();
                    beam_pozi.img = LStage_Low[i];//画像の撮影場所を格納する。

                    //trackを重ねる処理を入れる。
                    Point2d beam_peak = TrackDetection_verold(
                        binimages,
                        (int)LBeam_Low[i][r].peak.X,
                        (int)LBeam_Low[i][r].peak.Y,
                        0,
                        0,
                        4,
                        10,
                        hits);// true);

                    if (beam_peak.X == -1 & beam_peak.Y == -1)
                    {
                        mc.Join();
                        not_detect = 1;

                        //goto not_detect_track; とりあえずコメントアウトしておく
                    }

                    beam_pozi.peak.X = beam_peak.X;
                    beam_pozi.peak.Y = beam_peak.Y;

                    double firstx = beam_pozi.img.X - (beam_pozi.peak.X - 256) * 0.000267;
                    double firsty = beam_pozi.img.Y + (beam_pozi.peak.Y - 220) * 0.000267;
                    double firstz = beam_pozi.img.Z;

                    beam_pozi.stage = new Point3d(firstx, firsty, firstz);

                    beam_data_low.Add(beam_pozi);

                }//r_loop

                Point2d Ms_esti = new Point2d();
                double Ms_x = new double();
                double Ms_y = new double();

                int pix_dX = new int();
                int pix_dY = new int();

                LBeam_Low.Add(beam_data_low);
                for (int k = 0; k < LBeam_Low[i].Count(); k++)
                {
                    if (i == 0)
                    {
                        OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraightBase(
                            Sh,
                            Sh_low,
                            LBeam[lbeam_counter - 2][k].stage,
                            LBeam[lbeam_counter - 1][k].stage,
                            LBeam_Low[i][k].stage,
                            surface);
                        MSDXDY_BEAM_LOW.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                    }
                    else if (i == 1)
                    {
                        OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraightBase(
                            Sh,
                            Sh_low,
                            LBeam[lbeam_counter - 1][k].stage,
                            LBeam_Low[i - 1][k].stage,
                            LBeam_Low[i][k].stage,
                            surface);
                        MSDXDY_BEAM_LOW.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                    }
                    else
                    {
                        OpenCvSharp.CPlusPlus.Point2d Tangle_l = ApproximateStraight(
                            Sh_low,
                            LBeam_Low[i - 2][k].stage,
                            LBeam_Low[i - 1][k].stage,
                            LBeam_Low[i][k].stage);
                        MSDXDY_BEAM_LOW.Add(new OpenCvSharp.CPlusPlus.Point2d(Tangle_l.X, Tangle_l.Y));
                    }

                }//roop_k

                for (int q = 0; q < MSDXDY_BEAM_LOW.Count(); q++) //ここで個々のbeamの角度を平均してstageの移動角度を算出する。
                {
                    Ms_x += MSDXDY_BEAM_LOW[q].X;
                    Ms_y += MSDXDY_BEAM_LOW[q].Y;

                    pix_dX += (int)LBeam_Low[i + 1][q].peak.X - (int)LBeam_Low[i][q].peak.X;
                    pix_dY += (int)LBeam_Low[i + 1][q].peak.Y - (int)LBeam_Low[i][q].peak.Y;
                }

                Ms_x = Ms_x / MSDXDY_BEAM_LOW.Count();
                Ms_y = Ms_y / MSDXDY_BEAM_LOW.Count();
                Ms_esti = new Point2d(Ms_x, Ms_y);
                Msdxdy_Low.Add(Ms_esti);//算出した角度をぶち込む。


                pix_dX = pix_dX / MSDXDY_BEAM_LOW.Count();//ずれたピクセル量
                pix_dY = pix_dY / MSDXDY_BEAM_LOW.Count();//ずれたピクセル量
                double cenX = LStage_Low[i].X - pix_dX * 0.000267;
                double cenY = LStage_Low[i].Y + pix_dY * 0.000267;
                double cenZ = LStage_Low[i].Z;

                LCenter_Low.Add(new Point3d(cenX, cenY, cenZ));//検出したそれぞれのbeamのズレから算出したパターンマッチの際の中心座標(stage)。

            }//i_loop

            //
            int lcen_low_count = LCenter_Low.Count();
            mc.MovePointXY(LCenter_Low[lcen_low_count - 1].X, LCenter_Low[lcen_low_count - 1].Y);

            mc.Join();

            //検出に失敗した場合は、ループを抜けてここに来る。

            //file write out up_gel
            string txtfileName_sh_up = datarootdirpath + string.Format(@"\Sh_up.txt");
            StreamWriter twriter_sh_up = File.CreateText(txtfileName_sh_up);
            twriter_sh_up.WriteLine("{0}", Sh);
            twriter_sh_up.Close();

            //file write out
            string txtfileName_t_info_up = datarootdirpath + string.Format(@"\location_up.txt");
            StreamWriter twriter_t_info_up = File.CreateText(txtfileName_t_info_up);
            for (int i = 0; i < UpTrackInfo.Count; i++)
            {
                for (int t = 0; t < UpTrackInfo[i].Count; t++)
                {
                    UpTrackInfo[i][t].img.ImWrite(datarootdirpath + string.Format(@"\img_t_info_up_{0}-{1}.png", i, t));
                    Vector3 p = UpTrackInfo[i][t].StageCoord;
                    twriter_t_info_up.WriteLine("{0} {1} {2} {3} {4}", i, t, p.X, p.Y, p.Z);
                }
            }
            twriter_t_info_up.Close();

            string txtfileName_lbeam = datarootdirpath + string.Format(@"\lbeam_up.txt");
            StreamWriter twriter_lbeam = File.CreateText(txtfileName_lbeam);
            for (int i = 0; i < LBeam.Count(); i++)
            {
                for (int r = 0; r < LBeam[i].Count(); r++)
                {
                    twriter_lbeam.WriteLine("{0} {1} BeamPeak: {2} {3} LBeam(Stage): {4} {5} {6}",
                        i,
                        r,
                        LBeam[i][r].peak.X,
                        LBeam[i][r].peak.Y,
                        LBeam[i][r].stage.X,
                        LBeam[i][r].stage.Y,
                        LBeam[i][r].stage.Z);
                }

            }
            twriter_lbeam.Close();

            string txtfileName_LCenter = datarootdirpath + string.Format(@"\LCenter_up.txt");
            StreamWriter twriter_LCenter = File.CreateText(txtfileName_LCenter);
            for (int i = 0; i < LCenter.Count(); i++)
            {
                twriter_LCenter.WriteLine("{0} {1} {2}", LCenter[i].X, LCenter[i].Y, LCenter[i].Z);
            }
            twriter_LCenter.Close();

            string txtfileName_msdxdy = datarootdirpath + string.Format(@"\msdxdy.txt");
            StreamWriter twriter_msdxdy = File.CreateText(txtfileName_msdxdy);
            for (int i = 0; i < Msdxdy.Count(); i++)
            {
                OpenCvSharp.CPlusPlus.Point2d p = Msdxdy[i];
                twriter_msdxdy.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_msdxdy.Close();

            //file write out low_gel
            string txtfileName_sh_low = datarootdirpath + string.Format(@"\Sh_low.txt");
            StreamWriter twriter_sh_low = File.CreateText(txtfileName_sh_low);
            twriter_sh_low.WriteLine("{0}", Sh_low);
            twriter_sh_low.Close();

            string txtfileName_t_info_low = datarootdirpath + string.Format(@"\location_low.txt");
            StreamWriter twriter_t_info_low = File.CreateText(txtfileName_t_info_low);
            for (int i = 0; i < LowTrackInfo.Count; i++)
            {
                for (int t = 0; t < LowTrackInfo[i].Count; t++)
                {
                    LowTrackInfo[i][t].img.ImWrite(datarootdirpath + string.Format(@"\img_t_info_low_{0}-{1}.png", i, t));
                    Vector3 p = LowTrackInfo[i][t].StageCoord;
                    twriter_t_info_low.WriteLine("{0} {1} {2} {3} {4}", i, t, p.X, p.Y, p.Z);
                }
            }
            twriter_t_info_low.Close();


            string txtfileName_lbeam_low = datarootdirpath + string.Format(@"\lbeam_low.txt");
            StreamWriter twriter_lbeam_low = File.CreateText(txtfileName_lbeam_low);
            for (int i = 0; i < LBeam_Low.Count(); i++)
            {
                for (int r = 0; r < LBeam_Low[i].Count(); r++)
                {
                    twriter_lbeam_low.WriteLine("{0} {1} BeamPeak: {2} {3} LBeam(Stage): {4} {5} {6}",
                        i,
                        r,
                        LBeam_Low[i][r].peak.X,
                        LBeam_Low[i][r].peak.Y,
                        LBeam_Low[i][r].stage.X,
                        LBeam_Low[i][r].stage.Y,
                        LBeam_Low[i][r].stage.Z);
                }
            }
            twriter_lbeam_low.Close();


            string txtfileName_LCenter_low = datarootdirpath + string.Format(@"\LCenter_low.txt");
            StreamWriter twriter_LCenter_low = File.CreateText(txtfileName_LCenter_low);
            for (int i = 0; i < LCenter_Low.Count(); i++)
            {
                twriter_LCenter_low.WriteLine("{0} {1} {2}", LCenter_Low[i].X, LCenter_Low[i].Y, LCenter_Low[i].Z);
            }
            twriter_LCenter_low.Close();

            string txtfileName_msdxdy_low = datarootdirpath + string.Format(@"\msdxdy_low.txt");
            StreamWriter twriter_msdxdy_low = File.CreateText(txtfileName_msdxdy_low);
            for (int i = 0; i < Msdxdy_Low.Count(); i++)
            {
                OpenCvSharp.CPlusPlus.Point2d p = Msdxdy_Low[i];
                twriter_msdxdy_low.WriteLine("{0} {1} {2}", i, p.X, p.Y);
            }
            twriter_msdxdy_low.Close();

        }

        private void GoTopUp()
        {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Surface surface = Surface.GetInstance(parameterManager);
            try
            {
                Vector3 cc = mc.GetPoint();
                double Zp = surface.UpTop + 0.015;
                mc.MoveTo(new Vector3(cc.X, cc.Y, Zp));
                mc.Join();
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Cannot move to top surface of upperlayer ");
            }
        }

        private void task() {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            TracksManager tm = parameterManager.TracksManager;
            Track myTrack = tm.GetTrack(tm.TrackingIndex);
            Surface surface = Surface.GetInstance(parameterManager);
            int mod = parameterManager.ModuleNo;
            int pl = parameterManager.PlateNo;
            bool dubflag;
            Led led_ = Led.GetInstance();

            BeamFollower(myTrack, mod, pl, false);


            GoTopUp();
            mc.Join();
            Thread.Sleep(100);

        }

        private bool isValidate() {
            return true;
        }
    }
}
