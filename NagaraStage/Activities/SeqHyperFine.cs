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
    class SeqHyperFine : Activity, IActivity {
        private static SeqHyperFine instance;

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

        public SeqHyperFine(ParameterManager _paramaterManager)
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
        public static SeqHyperFine GetInstance(
            ParameterManager parameterManger = null) {
            if (instance == null) {
                instance = new SeqHyperFine(parameterManger);
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
            // レンズが50倍に設定されていない場合は例外を返すようにしたいがやり方が分からん(20140724)
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

        private bool isValidate() {
            return true;
        }
    }
}
