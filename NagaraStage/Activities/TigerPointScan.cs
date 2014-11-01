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
    class TigerPointScan : Activity, IActivity {
        private static TigerPointScan instance;

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

        public TigerPointScan(ParameterManager _paramaterManager)
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
        public static TigerPointScan GetInstance(
            ParameterManager parameterManger = null) {
            if (instance == null) {
                instance = new TigerPointScan(parameterManger);
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
            return taskList;
        }

        private void task() {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Surface sur = Surface.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            Led led = Led.GetInstance();

            Vector3 InitPoint = mc.GetPoint();
            Vector3 p = new Vector3();
            int viewcounter = 0;


            string txtfileName = string.Format(@"{0}\{1}.txt",
                direcotryPath, System.DateTime.Now.ToString("yyyyMMdd_HHmmss_ffff"));
            StreamWriter twriter = File.CreateText(txtfileName);

            List<Vector3> PointList = new List<Vector3>();
            for (int xx = 0; xx < 10; xx++ ) {
                PointList.Add(new Vector3(InitPoint.X + xx * 0.1, InitPoint.Y, InitPoint.Z));
            }

            camera.Stop();

            for(int pp=0; pp<PointList.Count(); pp++){
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
        }

        private bool isValidate() {
            return true;
        }
    }
}
