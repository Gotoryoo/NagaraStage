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
    class StartAutoFollowing : Activity, IActivity {
        private static StartAutoFollowing instance;

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

        public StartAutoFollowing(ParameterManager _paramaterManager)
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
        public static StartAutoFollowing GetInstance(
            ParameterManager parameterManger = null) {
            if (instance == null) {
                instance = new StartAutoFollowing(parameterManger);
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

        static int number_of_follow;

        public struct mm
        {
            public double white_x;
            public double white_y;
            public double white_kido;
            public double white_dx;
            public double white_dy;
        }


        static double common_dx;
        static double common_dy;

        static double over_dx;
        static double over_dy;

        private void Follow()
        {
            TracksManager tm = parameterManager.TracksManager;
            Track myTrack = tm.GetTrack(tm.TrackingIndex);
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            List<Mat> image_set = new List<Mat>();
            List<Mat> image_set_reverse = new List<Mat>();

            double now_x = mc.GetPoint().X;
            double now_y = mc.GetPoint().Y;
            double now_z = mc.GetPoint().Z;

            number_of_follow++;

            if (number_of_follow == 1)
            {
                common_dx = myTrack.MsDX;
                common_dy = myTrack.MsDY;
            }
            else if (number_of_follow == 2)
            {
                common_dx = myTrack.MsDX + ((0.265625 * over_dx * 3) / (0.024 * 2.2 * 1000));
                common_dy = myTrack.MsDY - ((0.265625 * over_dy * 3) / (0.024 * 2.2 * 1000));
            }
            else
            {
                common_dx = common_dx + ((0.265625 * over_dx * 3) / (0.024 * 2.2 * 1000));
                common_dy = common_dy - ((0.265625 * over_dy * 3) / (0.024 * 2.2 * 1000));
            }


            for (int i = 0; i < 8; i++)
            {//myTrack.MsD○はdz1mmあたりのd○の変位mm
                double next_x = now_x - i * common_dx * 0.003 * 2.2;//3μm間隔で撮影
                double next_y = now_y - i * common_dy * 0.003 * 2.2;//Shrinkage Factor は2.2で計算(仮)
                mc.MovePoint(next_x, next_y, now_z - 0.003 * i);
                mc.Join();

                byte[] b = camera.ArrayImage;
                Mat image = new Mat(440, 512, MatType.CV_8U, b);
                Mat imagec = image.Clone();
                image_set.Add(imagec);
            }

            for (int i = 7; i >= 0; i--)
            {
                image_set_reverse.Add(image_set[i]);
            }



            int n = image_set.Count();//１回分の取得画像の枚数

            Mat cont = new Mat(440, 512, MatType.CV_8U);
            Mat gau_1 = new Mat(440, 512, MatType.CV_8U);
            Mat gau_2 = new Mat(440, 512, MatType.CV_8U);
            Mat sub = new Mat(440, 512, MatType.CV_8U);
            Mat bin = new Mat(440, 512, MatType.CV_8U);

            double Max_kido;
            double Min_kido;

            OpenCvSharp.CPlusPlus.Point maxloc;
            OpenCvSharp.CPlusPlus.Point minloc;

            List<Mat> two_set = new List<Mat>();
            List<Mat> Part_img = new List<Mat>();

            for (int i = 0; i < image_set.Count(); i++)
            {
                Cv2.GaussianBlur((Mat)image_set_reverse[i], gau_1, Cv.Size(3, 3), -1);//パラメータ見ないといけない。
                Cv2.GaussianBlur(gau_1, gau_2, Cv.Size(51, 51), -1);//パラメータ見ないといけない。
                Cv2.Subtract(gau_2, gau_1, sub);
                Cv2.MinMaxLoc(sub, out Min_kido, out Max_kido, out minloc, out maxloc);
                cont = (sub - Min_kido) * 255 / (Max_kido - Min_kido);
                cont.ImWrite(string.Format(@"C:\set\cont_{0}.bmp", i));
                Cv2.Threshold(cont, bin, 115, 1, ThresholdType.Binary);//パラメータ見ないといけない。
                two_set.Add(bin);
            }

            List<mm> white_area = new List<mm>();
            int x0 = 256;
            int y0 = 220;//視野の中心


            for (int delta_xx = -1; delta_xx <= 1; delta_xx++)//一番下の画像よりどれだけずらすか
                for (int delta_yy = -1; delta_yy <= 1; delta_yy++)
                {
                    {
                        //積層写真の型作り（行列の中身は０行列）
                        Mat superimposed = Mat.Zeros(440 + 3 * Math.Abs(delta_yy), 512 + 3 * Math.Abs(delta_xx), MatType.CV_8UC1);


                        //各写真の型作り
                        for (int i = 0; i < two_set.Count; i++)
                        {
                            Mat Part = Mat.Zeros(440 + 3 * Math.Abs(delta_yy), 512 + 3 * Math.Abs(delta_xx), MatType.CV_8UC1);
                            Part_img.Add(Part);
                        }//2枚を１セットにしてずらす場合

                        if (delta_xx >= 0 && delta_yy >= 0)//画像の右下への移動
                        {
                            for (int i = 0; i < two_set.Count; i++)
                            {
                                if (i == 0 || i == 1)
                                {
                                    Part_img[i][
                                             0
                                         , 440
                                         , 0
                                         , 512
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                                else if (i == 2 || i == 3)
                                {
                                    Part_img[i][
                                             0 + Math.Abs(delta_yy)  //yの値のスタート地点
                                         , 440 + Math.Abs(delta_yy)  //yの値のゴール地点
                                         , 0 + Math.Abs(delta_xx)    //xの値のスタート地点
                                         , 512 + Math.Abs(delta_xx)  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                                else if (i == 4 || i == 5)
                                {
                                    Part_img[i][
                                             0 + 2 * Math.Abs(delta_yy)  //yの値のスタート地点
                                         , 440 + 2 * Math.Abs(delta_yy) //yの値のゴール地点
                                         , 0 + 2 * Math.Abs(delta_xx)    //xの値のスタート地点
                                         , 512 + 2 * Math.Abs(delta_xx)  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                                else if (i == 6 || i == 7)
                                {
                                    Part_img[i][
                                                 0 + 3 * Math.Abs(delta_yy)  //yの値のスタート地点
                                             , 440 + 3 * Math.Abs(delta_yy)  //yの値のゴール地点
                                             , 0 + 3 * Math.Abs(delta_xx)    //xの値のスタート地点
                                             , 512 + 3 * Math.Abs(delta_xx)  //xの値のゴール地点
                                             ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                            }
                            for (int i = 0; i < Part_img.Count(); i++)
                            {
                                superimposed += Part_img[i];
                            }

                            Cv2.Threshold(superimposed, superimposed, 5, 255, ThresholdType.ToZero);//パラメータ見ないといけない。

                            superimposed.SubMat(0
                                                    , 440
                                                    , 0
                                                    , 512).CopyTo(superimposed);//１枚目の画像の大きさ、場所で切り取る
                        }



                        if (delta_xx >= 0 && delta_yy < 0)//画像の右上への移動
                        {
                            for (int i = 0; i < two_set.Count; i++)
                            {
                                if (i == 0 || i == 1)
                                {
                                    Part_img[i][
                                             0 + 3
                                         , 440 + 3
                                         , 0
                                         , 512
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                                else if (i == 2 || i == 3)
                                {
                                    Part_img[i][
                                             0 + 3 - 1  //yの値のスタート地点
                                         , 440 + 3 - 1  //yの値のゴール地点
                                         , 0 + Math.Abs(delta_xx)    //xの値のスタート地点
                                         , 512 + Math.Abs(delta_xx)  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                                else if (i == 4 || i == 5)
                                {
                                    Part_img[i][
                                             0 + 3 - 2  //yの値のスタート地点
                                         , 440 + 3 - 2  //yの値のゴール地点
                                         , 0 + 2 * Math.Abs(delta_xx)    //xの値のスタート地点
                                         , 512 + 2 * Math.Abs(delta_xx)  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                                else if (i == 6 || i == 7)
                                {
                                    Part_img[i][
                                                 0 + 3 - 3  //yの値のスタート地点
                                             , 440 + 3 - 3  //yの値のゴール地点
                                             , 0 + 3 * Math.Abs(delta_xx)    //xの値のスタート地点
                                             , 512 + 3 * Math.Abs(delta_xx)  //xの値のゴール地点
                                             ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                            }
                            for (int i = 0; i < Part_img.Count(); i++)
                            {
                                superimposed += Part_img[i];
                            }

                            Cv2.Threshold(superimposed, superimposed, 5, 255, ThresholdType.ToZero);//パラメータ見ないといけない。

                            superimposed.SubMat(0 + 3
                                                    , 440 + 3
                                                    , 0
                                                    , 512).CopyTo(superimposed);//１枚目の画像の大きさ、場所で切り取る
                        }



                        if (delta_xx < 0 && delta_yy < 0)//画像の左上への移動 
                        {
                            for (int i = 0; i < two_set.Count; i++)
                            {
                                if (i == 0 || i == 1)
                                {
                                    Part_img[i][
                                             0 + 3
                                         , 440 + 3
                                         , 0 + 3
                                         , 512 + 3
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                                else if (i == 2 || i == 3)
                                {
                                    Part_img[i][
                                             0 + 3 - 1  //yの値のスタート地点
                                         , 440 + 3 - 1  //yの値のゴール地点
                                         , 0 + 3 - 1    //xの値のスタート地点
                                         , 512 + 3 - 1  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                                else if (i == 4 || i == 5)
                                {
                                    Part_img[i][
                                             0 + 3 - 2  //yの値のスタート地点
                                         , 440 + 3 - 2  //yの値のゴール地点
                                         , 0 + 3 - 2    //xの値のスタート地点
                                         , 512 + 3 - 2  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                                else if (i == 6 || i == 7)
                                {
                                    Part_img[i][
                                                 0 + 3 - 3  //yの値のスタート地点
                                             , 440 + 3 - 3  //yの値のゴール地点
                                             , 0 + 3 - 3    //xの値のスタート地点
                                             , 512 + 3 - 3  //xの値のゴール地点
                                             ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                            }
                            for (int i = 0; i < Part_img.Count(); i++)
                            {
                                superimposed += Part_img[i];
                            }

                            Cv2.Threshold(superimposed, superimposed, 5, 255, ThresholdType.ToZero);//パラメータ見ないといけない。

                            superimposed.SubMat(0 + 3
                                                    , 440 + 3
                                                    , 0 + 3
                                                    , 512 + 3).CopyTo(superimposed);//１枚目の画像の大きさ、場所で切り取る
                        }


                        if (delta_xx < 0 && delta_yy >= 0)//画像の左下への移動
                        {
                            for (int i = 0; i < two_set.Count; i++)
                            {
                                if (i == 0 || i == 1)
                                {
                                    Part_img[i][
                                             0
                                         , 440
                                         , 0 + 3
                                         , 512 + 3
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                                else if (i == 2 || i == 3)
                                {
                                    Part_img[i][
                                             0 + Math.Abs(delta_yy)  //yの値のスタート地点
                                         , 440 + Math.Abs(delta_yy)  //yの値のゴール地点
                                         , 0 + 3 - 1    //xの値のスタート地点
                                         , 512 + 3 - 1  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                                else if (i == 4 || i == 5)
                                {
                                    Part_img[i][
                                             0 + 2 * Math.Abs(delta_yy)  //yの値のスタート地点
                                         , 440 + 2 * Math.Abs(delta_yy)  //yの値のゴール地点
                                         , 0 + 3 - 2    //xの値のスタート地点
                                         , 512 + 3 - 2  //xの値のゴール地点
                                         ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                                else if (i == 6 || i == 7)
                                {
                                    Part_img[i][
                                                 0 + 3 * Math.Abs(delta_yy)  //yの値のスタート地点
                                             , 440 + 3 * Math.Abs(delta_yy)  //yの値のゴール地点
                                             , 0 + 3 - 3    //xの値のスタート地点
                                             , 512 + 3 - 3 //xの値のゴール地点
                                             ] = two_set[i];//処理済み画像をPartの対応する部分に入れていく
                                }
                            }
                            for (int i = 0; i < Part_img.Count(); i++)
                            {
                                superimposed += Part_img[i];
                            }

                            Cv2.Threshold(superimposed, superimposed, 5, 255, ThresholdType.ToZero);//パラメータ見ないといけない。

                            superimposed.SubMat(0
                                                    , 440
                                                    , 0 + 3
                                                    , 512 + 3).CopyTo(superimposed);//１枚目の画像の大きさ、場所で切り取る
                        }

                        Mat one1 = Mat.Ones(y0 - 20, 512, MatType.CV_8UC1);//視野の中心からどれだけの窓を開けるか
                        Mat one2 = Mat.Ones(41, x0 - 20, MatType.CV_8UC1);
                        Mat one3 = Mat.Ones(41, 491 - x0, MatType.CV_8UC1);
                        Mat one4 = Mat.Ones(419 - y0, 512, MatType.CV_8UC1);

                        superimposed[0, y0 - 20, 0, 512] = one1 * 0;
                        superimposed[y0 - 20, y0 + 21, 0, x0 - 20] = one2 * 0;
                        superimposed[y0 - 20, y0 + 21, x0 + 21, 512] = one3 * 0;
                        superimposed[y0 + 21, 440, 0, 512] = one4 * 0;//中心から○μｍの正方形以外は黒くする。

                        superimposed.ImWrite("C:\\set\\superimposed25_1.bmp");

                        using (CvMemStorage storage = new CvMemStorage())
                        {
                            using (CvContourScanner scanner = new CvContourScanner(superimposed.ToIplImage(), storage, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxSimple))
                            {
                                foreach (CvSeq<CvPoint> c in scanner)
                                {
                                    CvMoments mom = new CvMoments(c, false);
                                    if (c.ElemSize < 2) continue;
                                    if (mom.M00 == 0.0) continue;
                                    double mx = mom.M10 / mom.M00;
                                    double my = mom.M01 / mom.M00;
                                    mm koko = new mm();
                                    koko.white_x = mx;
                                    koko.white_y = my;
                                    koko.white_kido = mom.M00;
                                    koko.white_dx = delta_xx;
                                    koko.white_dy = delta_yy;
                                    white_area.Add(koko);
                                    stage.WriteLine(String.Format("mx={0:f2} , my={1:f2} , dx={2:f2} , dy={3:f2} , M={4:f2}", mx, my, delta_xx, delta_yy, mom.M00));
                                }
                            }
                        }
                        Part_img.Clear();

                    }//pixel移動x
                }//pixel移動y

            if (white_area.Count > 0)
            {
                double center_x = 0;
                double center_y = 0;
                double center_dx = 0;
                double center_dy = 0;
                double kido_sum = 0;
                for (int i = 0; i < white_area.Count; i++)
                {
                    kido_sum += white_area[i].white_kido;
                    center_x += white_area[i].white_x * white_area[i].white_kido;
                    center_y += white_area[i].white_y * white_area[i].white_kido;
                    center_dx += white_area[i].white_dx * white_area[i].white_kido;
                    center_dy += white_area[i].white_dy * white_area[i].white_kido;
                }
                center_x = center_x / kido_sum;
                center_y = center_y / kido_sum;
                center_dx = center_dx / kido_sum;
                center_dy = center_dy / kido_sum;

                int c_o_g_x;
                int c_o_g_y;
                if (center_x >= 0)
                {
                    c_o_g_x = (int)(center_x + 0.5);
                }
                else
                {
                    c_o_g_x = (int)(center_x - 0.5);
                }

                if (center_x >= 0)
                {
                    c_o_g_y = (int)(center_y + 0.5);
                }
                else
                {
                    c_o_g_y = (int)(center_y - 0.5);
                }

                int dx_pixel = c_o_g_x - x0;
                int dy_pixel = c_o_g_y - y0;

                double dx_micron = dx_pixel * 0.265625 / 1000;
                double dy_micron = dy_pixel * 0.265625 / 1000;

                double now_x2 = mc.GetPoint().X;
                double now_y2 = mc.GetPoint().Y;
                mc.MovePointXY(now_x2 - dx_micron, now_y2 + dy_micron);//pixelの軸とstageの軸の関係から
                mc.Join();

                over_dx = center_dx;
                over_dy = center_dy;
            }

        }

        private void task() {
            TracksManager tm = parameterManager.TracksManager;
            Track myTrack = tm.GetTrack(tm.TrackingIndex);
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();


            int nshot = 70;
            byte[] bb = new byte[440 * 512 * nshot];

            Vector3 now_p = mc.GetPoint();         //スライス画像を取るための残し

            DateTime starttime = System.DateTime.Now;
            string datfileName = string.Format(@"C:\Documents and Settings\stage1-user\デスクトップ\window_model\7601_135\d.dat");
            BinaryWriter writer = new BinaryWriter(File.Open(datfileName, FileMode.Create));

            string txtfileName = string.Format(@"C:\Documents and Settings\stage1-user\デスクトップ\window_model\7601_135\d.txt");
            StreamWriter twriter = File.CreateText(txtfileName);
            string stlog = "";



            int kk = 0;
            while (kk < nshot)
            {
                byte[] b = camera.ArrayImage;//画像を取得
                //Mat image = new Mat(440, 512, MatType.CV_8U, b);
                //Mat image_clone = image.Clone();
                b.CopyTo(bb, 440 * 512 * kk);

                //image_clone.ImWrite(string.Format(@"E:\20141015\5201_136\{0}.bmp", kk));
                stlog += String.Format("{0} {1} {2} {3} {4}\n",
                    System.DateTime.Now.ToString("HHmmss\\.fff"),
                    (now_p.X * 1000).ToString("0.0"),
                    (now_p.Y * 1000).ToString("0.0"),
                    (now_p.Z * 1000).ToString("0.0"),
                    kk);

                kk++;
                //mc.MovePointZ(now_p.Z - 0.0020);
                mc.MoveDistance(-0.0020, VectorId.Z);
                mc.Join();
            }

            twriter.Write(stlog);
            writer.Write(bb);
            writer.Flush();
            writer.Close();

            return;


            Surface surface = Surface.GetInstance(parameterManager);//表面認識から境界値を取得
            double uptop = surface.UpTop;
            double upbottom = surface.UpBottom;
            double lowtop = surface.LowTop;
            double lowbottom = surface.LowBottom;

            while (mc.GetPoint().Z >= upbottom + 0.030)//上ゲル内での連続移動
            {
                Follow();
            }

            if (mc.GetPoint().Z >= upbottom + 0.024)
            {
                double now_x = mc.GetPoint().X;
                double now_y = mc.GetPoint().Y;
                double now_z = mc.GetPoint().Z;
                mc.MovePoint(now_x - common_dx * (now_z - (upbottom + 0.024)) * 2.2, now_y - common_dy * (now_z - (upbottom + 0.024)) * 2.2, upbottom + 0.024);
            }
            else
            {
                double now_x = mc.GetPoint().X;
                double now_y = mc.GetPoint().Y;
                double now_z = mc.GetPoint().Z;
                mc.MovePoint(now_x - common_dx * ((upbottom + 0.024) - now_z) * 2.2, now_y - common_dy * ((upbottom + 0.024) - now_z) * 2.2, upbottom + 0.024);
            }

            while (mc.GetPoint().Z >= upbottom)//上ゲル内での連続移動
            {
                Follow();
            }

            mc.MovePoint(mc.GetPoint().X - common_dx * (upbottom - lowtop)
                , mc.GetPoint().Y - common_dy * (upbottom - lowtop)
                , lowtop);//Base下側への移動
        }

        private bool isValidate() {
            return true;
        }
    }
}
