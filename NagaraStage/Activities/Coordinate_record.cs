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
    class Coordinate_record : Activity, IActivity {
        private static Coordinate_record instance;

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

        public Coordinate_record(ParameterManager _paramaterManager)
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
        public static Coordinate_record GetInstance(
            ParameterManager parameterManger = null) {
            if (instance == null) {
                instance = new Coordinate_record(parameterManger);
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
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Vector3 CenterPoint = mc.GetPoint();

            string fileName = string.Format(@"c:\test\coordinate.txt");
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fileName, true, System.Text.Encoding.GetEncoding("shift_jis"));
            string coordinate = string.Format("{0} {1} {2}\n", CenterPoint.X, CenterPoint.Y, CenterPoint.Z);
            sw.Write(coordinate);
            sw.Close();
        }

        private bool isValidate() {
            return true;
        }
    }
}
