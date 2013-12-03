using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using NagaraStage.IO;
using NagaraStage.Parameter;

namespace NagaraStage.Ui {
    /// <summary>
    /// カメラからの入力画像を表示するためのキャンバスクラスです．
    /// <para>CameraViewer.xaml の相互作用ロジック</para>
    /// </summary>
    public partial class CameraViewer : Canvas {
        private DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal);
        Camera camera = Camera.GetInstance();

        public CameraViewer() {
            InitializeComponent();
            Unloaded += CameraViewer_Unloaded;
            timer.Tick += timer_Tick;
            timer.Interval = new TimeSpan(ParameterManager.FrameIntervalMilliSec * 1000);
        }

        /// <summary>
        /// カメラからの入力と映像の表示を開始したときに発生するイベント
        /// </summary>
        public event EventHandler Started;
        /// <summary>
        /// カメラからの入力と映像の表示を停止したときに発生するイベント
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// カメラからの入力と映像の表示を開始します．
        /// </summary>
        public void Start() {
            camera.Start();
            timer.Start();
            if (Started != null) {
                Started(this, new EventArgs());
            }
        }

        /// <summary>
        /// カメラからの入力と映像の表示を停止します．
        /// </summary>
        public void Stop() {
            timer.Stop();
            camera.Stop();
            cameraLayer.Source = new BitmapImage(new Uri("Images/miku01.jpg", UriKind.Relative));
            if (Stopped != null) {
                Stopped(this, new EventArgs());
            }
        }

        /// <summary>
        /// カメラが実行中であるかを取得します．
        /// </summary>
        public bool IsRunning {
            get { return camera.IsRunning; }
        }

        void CameraViewer_Unloaded(object sender, RoutedEventArgs e) {
            timer.Stop();
            camera.Stop();
        }

        void timer_Tick(object sender, EventArgs e) {
            try {
                cameraLayer.Source = camera.Image;
            } catch(Exception ex){
                MessageBox.Show(ex.Message, Properties.Strings.Error);
            }
        }
    }
}
