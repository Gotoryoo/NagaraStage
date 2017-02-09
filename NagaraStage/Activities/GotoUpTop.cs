using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

using System.Windows;

using NagaraStage.Ui;
using NagaraStage.IO;
using NagaraStage.Parameter;



using OpenCvSharp;
using OpenCvSharp.CPlusPlus;

namespace NagaraStage.Activities {
    class GotoUpTop : Activity, IActivity {
        private static GotoUpTop instance;

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

        public GotoUpTop(ParameterManager _paramaterManager)
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
        public static GotoUpTop GetInstance(
            ParameterManager parameterManger = null) {
            if (instance == null) {
                instance = new GotoUpTop(parameterManger);
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
        
        private void Detectbeam(Track myTrack, int mod, int pl)
        {
            try
            {
                string[] sp = myTrack.IdString.Split('-');
                string dwtxt = string.Format(@"c:\MKS_test\bpm\{0}-{1}\{2}-{3}-{4}-{5}_dw.txt", mod, pl, mod, pl, sp[0], sp[1]);
                string datarootdirpath = string.Format(@"C:\MKS_test\bpm\{0}-{1}", mod, pl);
                System.IO.DirectoryInfo mydir = System.IO.Directory.CreateDirectory(datarootdirpath);
                System.IO.DirectoryInfo mydir2 = System.IO.Directory.CreateDirectory(datarootdirpath);
                BeamDetection(dwtxt, false);
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("ID is not exist ");
            }
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
            int mod = parameterManager.ModuleNo;
            int pl = parameterManager.PlateNo;
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

            // すでに表面認識が実行中であれば停止するかどうか尋ねる．
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

            mc.Join();

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
        }

        private bool isValidate() {
            return true;
        }
    }
}
