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

/// <summary>
///　2015年10月にJ-PARCにてpbarを当てた乾板のdistortion解析用に作ったもの。
///　dat形式というバイナリファイルに書き出す。
/// </summary>


namespace NagaraStage.Activities {
    class DATFile : Activity, IActivity {
        private static DATFile instance;

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

        public DATFile(ParameterManager _paramaterManager)
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
        public static DATFile GetInstance(
            ParameterManager parameterManger = null) {
            if (instance == null) {
                instance = new DATFile(parameterManger);
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
            try {
                MotorControler mc = MotorControler.GetInstance(parameterManager);
                Surface sur = Surface.GetInstance(parameterManager);
                Camera camera = Camera.GetInstance();
                Led led = Led.GetInstance();

                Vector3 initialpoint = mc.GetPoint();

                camera.Stop();


                string txtfileName = string.Format(@"C:\img\{0}_{1}.txt",
                    initialpoint.X,
                    initialpoint.Y);
                StreamWriter twriter = File.CreateText(txtfileName);



                double thickness = sur.UpTop - initialpoint.Z;
                int npict = (int)(thickness / 0.002) + 3;

                byte[] bb = new byte[440 * 512 * npict];
                string datfileName = string.Format(@"C:\img\{0}_{1}.dat",
                                    (int)(initialpoint.X),
                                    (int)(initialpoint.Y)
                                    );
                BinaryWriter writer = new BinaryWriter(File.Open(datfileName, FileMode.Create));



                string stlog = "";
                int viewcounter = 0;

                Vector3 p = new Vector3();

                while (viewcounter < npict) {

                    mc.MoveDistance(0.002, VectorId.Z);
                    mc.Join();
                    byte[] b = Ipt.CaptureMain();
                    p = mc.GetPoint();

                    stlog += String.Format("{0}  {1} {2} {3}  {4}\n",
                        System.DateTime.Now.ToString("HHmmss\\.fff"),
                        (p.X * 1000).ToString("0.0"),
                        (p.Y * 1000).ToString("0.0"),
                        (p.Z * 1000).ToString("0.0"),
                        viewcounter);
                    b.CopyTo(bb, 440 * 512 * viewcounter);
                    viewcounter++;
                }


                twriter.Write(stlog);
                twriter.Close();

                writer.Write(bb);
                writer.Flush();
                writer.Close();

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                camera.Start();

            } catch (SystemException) {
            }


        }//task



    }

}

