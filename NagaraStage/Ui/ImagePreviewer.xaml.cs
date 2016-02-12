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
using System.Windows.Shapes;

using NagaraStage;
//using NagaraStage.ImageEnhancement;

namespace NagaraStage.Ui {
    /// <summary>ImagePreviewer.xaml の相互作用ロジック</summary>
    public partial class ImagePreviewer : Window {
        private IMainWindow window;
        private BitmapSource image;

        public ImagePreviewer() {
            IO.Camera camera = IO.Camera.GetInstance();
            image = camera.IsRunning ? camera.Image : new BitmapImage(new Uri("images/miku01.png", UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// 表示する画像を設定，または取得します．
        /// </summary>
        public BitmapSource ImageSource {
            get { return image; }
            set {
                image = value;
                view.Source = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImagePreviewer" /> class.
        /// </summary>
        /// <param name="_window">The _window.</param>
        public ImagePreviewer(IMainWindow _window) {
            InitializeComponent();
            this.window = _window;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e) {
            /*
            try {
                ImageUtility.Save(view, refferenceTextBox.Text);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }*/

            DialogResult = true;
        }

        private void refferenceButton_Click(object sender, RoutedEventArgs e) {
            string path = refferenceTextBox.Text;
            Microsoft.Win32.SaveFileDialog dialog = createDialog();
            if ((bool)dialog.ShowDialog(this)) {
                path = dialog.FileName;
            }
            refferenceTextBox.Text = path;
        }

        private Microsoft.Win32.SaveFileDialog createDialog() {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "Jpeg Image(*.jpg)|*.jpg|" + "Png Image(*.png)|*.png|" + "Bitmap(*.bmp)|*.bmp";
            dialog.FilterIndex = 2;
            dialog.AddExtension = true;
            return dialog;
        }
    }
}
