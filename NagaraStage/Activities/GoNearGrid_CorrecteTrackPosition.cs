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

using System.Diagnostics;

using OpenCvSharp;
using OpenCvSharp.CPlusPlus;

namespace NagaraStage.Activities {
    class GoNearGrid_CorrecteTrackPosition : Activity, IActivity {
        private static GoNearGrid_CorrecteTrackPosition instance;

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

        public GoNearGrid_CorrecteTrackPosition(ParameterManager _paramaterManager)
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
        public static GoNearGrid_CorrecteTrackPosition GetInstance(
            ParameterManager parameterManger = null) {
            if (instance == null) {
                instance = new GoNearGrid_CorrecteTrackPosition(parameterManger);
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

        private void NearGridParameter()
        {

            try
            {
                MotorControler mc = MotorControler.GetInstance(parameterManager);
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
                MotorControler mc = MotorControler.GetInstance(parameterManager);
                IGridMarkRecognizer GridMarkRecognizer = coordManager;
                mc.SetSpiralCenterPoint();
                Led led = Led.GetInstance();
                Vector2 encoderPoint = new Vector2(-1, -1);
                encoderPoint.X = mc.GetPoint().X;
                encoderPoint.Y = mc.GetPoint().Y;
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
            try
            {
                MotorControler mc = MotorControler.GetInstance(parameterManager);
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
        }
        
        private void gotrack(Track myTrack)
        {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            double dstx = myTrack.MsX + coordManager.HFDX;
            double dsty = myTrack.MsY + coordManager.HFDY;
            mc.MovePointXY(dstx, dsty, delegate {
                stage.WriteLine(Properties.Strings.MovingComplete);
            });

        }

        private void surfacerecog()
        {
            Surface surface = Surface.GetInstance(parameterManager);
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            /*
                        string[] sp = myTrack.IdString.Split('-');

                        //string datfileName = string.Format("{0}.dat", System.DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                        string datfileName = string.Format(@"c:\test\bpm\{0}\{1}-{2}-{3}-{4}-{5}.dat", mod, mod, pl, sp[0], sp[1], System.DateTime.Now.ToString("ddHHmmss"));
                        BinaryWriter writer = new BinaryWriter(File.Open(datfileName, FileMode.Create));
                        */

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
                mc.Join();

                //Surface surface = Surface.GetInstance(parameterManager);

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

        }

        private void task() {
            TracksManager tm = parameterManager.TracksManager;
            Track myTrack = tm.GetTrack(tm.TrackingIndex);
            int mod = parameterManager.ModuleNo;
            int pl = parameterManager.PlateNo;
            string[] sp1 = myTrack.IdString.Split('-');
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            string logtxt = string.Format(@"C:\MKS_test\WorkingTime\{0}\{1}-{2}_TNeargrid.txt", mod, mod, pl);
            SimpleLogger SL2 = new SimpleLogger(logtxt, sp1[0], sp1[1]);
            SL2.Info("Neargrid Start");

            NearGridParameter();
            gotrack(myTrack);
            mc.Join();
            Led led_ = Led.GetInstance();
            led_.AdjustLight(parameterManager);
            Thread.Sleep(200);
            surfacerecog();
            SL2.Info("Neargrid End");
        }

        private bool isValidate() {
            return true;
        }
    }
}
