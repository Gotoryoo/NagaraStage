using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using NagaraStage;
using NagaraStage.IO;
using NagaraStage.Parameter;
using System.Windows.Media.Imaging;

namespace NagaraStage.Activities {
    class SequentialImage: Activity, IActivity {


        public event EventHandler<EventArgs> Started;
        public event ActivityEventHandler Exited;

        public List<Vector3> PhotographySpotList {
            get { return photographySpotList; }
            set { 
                if(IsActive) {
                    throw new InActionException("このPhotographySpotListはアクティブです.");
                }
                photographySpotList = value;
            }
        }

        public List<BitmapImage> ShootParaImages {
            get {
                List<BitmapImage> retval = new List<BitmapImage>();
                for (int i = 0; i < PhotographySpotList.Count; ++i) {
                    retval.Add(new BitmapImage(new Uri(imagesUri[i], UriKind.RelativeOrAbsolute)));
                }
                return retval;
            }
        }

        public List<string> ShootUriList {
            get { return imagesUri; }
        }

        public bool IsActive {
            get { return isActive; }
        }

        private ParameterManager parameterManager = null;
        private List<Vector3> photographySpotList = new List<Vector3>();
        private bool isActive = false;
        private List<string> imagesUri = new List<string>();

        public SequentialImage(ParameterManager _parameterManager): base(_parameterManager) {
            this.parameterManager = _parameterManager;
        }

        public void SetPhotographySpotsBy(Vector3 startPoint, Vector3 endPoint, double intervalUm) {
        }

        public void SetPhotographySoptBy(double startZ, double endZ, double intervalUm, Vector3 angle) {

        }

        [Obsolete("")]
        public void Start() {
            throw new NotImplementedException();
        }

        public List<Thread> CreateTask() {
            List<Thread> taskList = new List<Thread>();
            taskList.Add(new Thread(new ThreadStart(task)));
            return taskList;
        }

        private void task() {
            isActive = true;
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Camera camera = Camera.GetInstance();
            imagesUri = new List<string>();
            for (int i = 0; i < photographySpotList.Count; ++i ) {
                mc.MovePoint(photographySpotList[i]);
                mc.Join();
                camera.Stop();
                BitmapSource image = camera.Image;
                saveTemp(image);
                camera.Start();
            }
            isActive = false;
        }

        private void saveTemp(BitmapSource image) {
            string name = System.IO.Path.GetTempFileName() + ".bmp";
            ImageUtility.Save(image, name);
            imagesUri.Add(name);
        }
    }
}
