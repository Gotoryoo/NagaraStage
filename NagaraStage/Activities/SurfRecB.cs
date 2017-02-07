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

using OpenCvSharp;
using OpenCvSharp.CPlusPlus;

namespace NagaraStage.Activities {
    class SurfRecB : Activity, IActivity {
        private static SurfRecB instance;

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

        public SurfRecB(ParameterManager _paramaterManager)
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
        public static SurfRecB GetInstance(
            ParameterManager parameterManger = null) {
            if (instance == null) {
                instance = new SurfRecB(parameterManger);
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
            // モータが稼働中であれば停止するかどうかを尋ねる．
            int mod = parameterManager.ModuleNo;
            int pl = parameterManager.PlateNo;
            //MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            TracksManager tm = parameterManager.TracksManager;
            Track myTrack = tm.GetTrack(tm.TrackingIndex);
            Stopwatch swsurf = new Stopwatch();
            // Stopwatch Time = new Stopwatch();
            swsurf.Start();
            MotorControler mc = MotorControler.GetInstance(parameterManager);
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
            Surface surface = Surface.GetInstance(parameterManager);
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
            double Time = surface.Time;
            mc.Join();
            swsurf.Stop();
            string[] spsurf = myTrack.IdString.Split('-');
            string logtxt_ = string.Format(@"c:\test\bpm\{0}\{1}-{2}_surftime.txt", mod, mod, pl);
            //string log_ = string.Format("{0} \n", sw.Elapsed);
            string surftime = string.Format("{0} {1} {2} {3} \n", spsurf[0], spsurf[1], swsurf.Elapsed, Time);
            StreamWriter swr = new StreamWriter(logtxt_, true, Encoding.ASCII);
            swr.Write(surftime);
            swr.Close();

        }

        private bool isValidate() {
            return true;
        }
    }
}
