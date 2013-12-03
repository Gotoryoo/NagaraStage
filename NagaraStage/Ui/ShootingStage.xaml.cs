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

using NagaraStage.Activities;
using NagaraStage.Parameter;
using NagaraStage.IO;

namespace NagaraStage.Ui {
    /// <summary>
    /// 連続撮像を行うための準備をするユーザーインターフェイスを提供するクラスです．
    /// <para>ShootingStage.xaml の相互作用ロジック</para>
    /// </summary>
    public partial class ShootingStage : KeyHandleWorkspace {
        private IMainWindow window;
        private ParameterManager parameterManager;
        private CoordManager coord;
        private ShootingMode mode;
        private DispatcherTimer viewTimer;
        private string destinationDir;
        private string fileName;
        private string extention = "png";

        /// <summary>
        /// 保存する画像の拡張子を取得，または設定します．
        /// </summary>
        /// <exception cref="System.ArgumentException">拡張子がPNG, JPEG, BMP形式以外のものであった場合</exception>
        public string Extention {
            get { return extention; }
            set {
                if (value == "jpeg" || value == "jpg" || value == "png" || value == "bmp") {
                    extention = value.Replace(".", "");
                } else {
                    throw new ArgumentException();
                }
            }
        }

        public ShootingStage(IMainWindow _window, ShootingMode shootingMode)
            : base(_window) {
            InitializeComponent();
            this.window = _window;
            this.parameterManager = window.ParameterManager;
            this.coord = window.CoordManager;
            this.mode = shootingMode;
            this.viewTimer = new DispatcherTimer(DispatcherPriority.Normal);
            this.viewTimer.Interval = new TimeSpan(ParameterManager.ParamtersIntervalMilliSec * 1000);
            this.viewTimer.Tick += viewTimer_Ticked;
            Loaded += new RoutedEventHandler(ShootingStage_Loaded);
            Unloaded += new RoutedEventHandler(ShootingStage_Unloaded);
        }

        void ShootingStage_Loaded(object sender, RoutedEventArgs e) {
            emulsionCanvas.Start();
        }

        void ShootingStage_Unloaded(object sender, RoutedEventArgs e) {
            AccumImage accumImage = AccumImage.GetInstance(parameterManager);
            accumImage.Exited -= accumImage_Exited;
            accumImage.OnShort -= accumImage_OnShot;
        }

        private void viewTimer_Ticked(object sender, EventArgs e) {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            Vector3 p = mc.GetPoint();
            coordinateLabel.Content = string.Format("X:{0:0.0000}, Y:{1:0.0000}, Z:{2:0.0000}",
                p.X, p.Y, p.Z);            
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            AccumImage accumImage = AccumImage.GetInstance(parameterManager);
            accumImage.Exited += accumImage_Exited;
            accumImage.OnShort += accumImage_OnShot;
            viewTimer.Start();

            Led led = Led.GetInstance();
            ledSlider.Value = led.LastLightVoltage;
            ledTextBox.Text = led.LastLightVoltage.ToString();

            switch (mode) {
                case ShootingMode.Single:
                    modeLabel.Content = Properties.Strings.TakeSingle;
                    break;
                case ShootingMode.Plurality:
                    modeLabel.Content = Properties.Strings.TakePlurality;
                    break;
                case ShootingMode.Accumlative:
                    modeLabel.Content = Properties.Strings.TakeAccumlative;
                    break;
            }
            abortButton.IsEnabled = false;
            progressPanel.Visibility = Visibility.Hidden;

            distanceRangeLabel.Content =
                "(" + Properties.Strings.Max + AccumImage.MaxDistance.ToString() + "mm)";
            distanceTextBox.Text = string.Format("{0:0.00}", Properties.Settings.Default.ShootDistance);
            intervalTextBox.Text = string.Format("{0:0.0}", Properties.Settings.Default.ShootInterval);

        }

        private void accumImage_Exited(object sender, ActivityEventArgs e) {            
            switch (mode) {
                case ShootingMode.Plurality:
                    Dispatcher.BeginInvoke(new Action(saveParaImages), null);
                    break;
                case ShootingMode.Accumlative:
                    Dispatcher.BeginInvoke(new Action(saveProjectedImages), null);
                    break;
            }
            abortButton.IsEnabled = false;
            ledGroupBox.IsEnabled = true;
            MessageBox.Show("Complete");
        }

        private void saveParaImages() {
            AccumImage accumImage = AccumImage.GetInstance(parameterManager);
            for (int i = 0; i < accumImage.NumOfShots; ++i) {
                string path = destinationDir + "/" + fileName + "-" + (i + 1).ToString() + "." + Extention;
                ImageUtility.Save(accumImage.GetShotImage(i), path);
            }            
        }

        private void saveProjectedImages() {
            AccumImage accumImage = AccumImage.GetInstance(parameterManager);
            BitmapSource dst = ImageEnhancement.LibCv.Add(accumImage.ShootParaImages);
            ImageUtility.Save(dst, fileName + "." + Extention);
        }

        private void ledSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            ledTextBox.Text = e.NewValue.ToString();
        }

        private void adjustLedButton_Click(object sender, RoutedEventArgs e) {
            Led led = Led.GetInstance();
            try {
                int val = led.AdjustLight(parameterManager);
                ledTextBox.Text = val.ToString();
                ledSlider.Value = (double)val;
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, Properties.Strings.Error);
            }
        }

        private void ledTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                int val = int.Parse(ledTextBox.Text);
                if (val < 0 && val > 255) {
                    throw new ArgumentOutOfRangeException();
                }

                Led led = Led.GetInstance();
                led.DAout(val, parameterManager);
                ledTextBox.Background = Brushes.White;
            } catch (Exception) {
                ledTextBox.Background = Brushes.Pink;
            }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e) {            
            Finish();
        }

        private void startButton_Click(object sender, RoutedEventArgs e) {
            if (!System.IO.Directory.Exists(destinationDir)) {
                MessageBox.Show(Properties.Strings.DirNotFound);
                return;
            }
            if (fileNameTextBox.Background == Brushes.Pink) {
                MessageBox.Show(Properties.Strings.FileNameInvalid);
                return;
            }            

            AccumImage accumImage = AccumImage.GetInstance(parameterManager);
            if (accumImage.IsActive) {
                MessageBoxResult r = MessageBox.Show(
                    Properties.Strings.ShootException01, Properties.Strings.Abort + "?",
                    MessageBoxButton.YesNo);
                if (r == MessageBoxResult.Yes) {
                    accumImage.Abort();
                } else {
                    return;
                }
            }

            try {
                double distance = double.Parse(distanceTextBox.Text);
                MotorControler mc = MotorControler.GetInstance(parameterManager);
                accumImage.IntervalUm = double.Parse(intervalTextBox.Text) / 1000;
                accumImage.StartPoint = mc.GetPoint().Z;
                accumImage.EndPoint = accumImage.StartPoint
                                        + ((bool)minusRadioButton.IsChecked ? -distance : distance);

                progressPanel.Visibility = Visibility.Visible;
                progressBar.Minimum = 0;
                progressBar.Maximum = distance / accumImage.IntervalUm;

                accumImage.Start();
                abortButton.IsEnabled = true;
                ledGroupBox.IsEnabled = false;
                Properties.Settings.Default.ShootDistance = double.Parse(distanceTextBox.Text);
                Properties.Settings.Default.ShootInterval = accumImage.IntervalUm;                
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void emulsionView_MouseMove(object sender, MouseEventArgs e) {
            int x = (int)e.GetPosition(emulsionCanvas).X;
            int y = (int)e.GetPosition(emulsionCanvas).Y;
            Vector2 point = coord.TransToEmulsionCoord(x, y);
            xCoordLabel.Content = string.Format("X:{0:000.000}mm", point.X);
            yCoordLabel.Content = string.Format("Y:{0:000.000}mm", point.Y);
        }

        private void emulsionView_MouseLeave(object sender, MouseEventArgs e) {
            xCoordLabel.Content = "X:---.---mm";
            yCoordLabel.Content = "Y:---.---mm";
        }

        private void distanceTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                double val = double.Parse(distanceTextBox.Text);
                if (val <= 0) {
                    throw new ArgumentOutOfRangeException(Properties.Strings.ValMustPositive);
                }
                distanceTextBox.Background = Brushes.White;
            } catch (Exception) {
                distanceTextBox.Background = Brushes.Pink;
            }
        }

        private void intervalTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                double.Parse(intervalTextBox.Text);
                intervalTextBox.Background = Brushes.White;
            } catch (Exception) {
                distanceTextBox.Background = Brushes.Pink;
            }
        }

        /// <summary>
        /// 撮影処理が行われたときのイベントハンドラ
        /// </summary>
        private void accumImage_OnShot(object sender, ActivityEventArgs e) {
            Dispatcher.BeginInvoke(new Action(delegate() {
                AccumImage accumImage = AccumImage.GetInstance(parameterManager);
                progressBar.Value = accumImage.CompletePercent;
                shootNumLabel.Content = accumImage.NumOfShots.ToString();
            }), null);            
        }

        private void fileNameTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            // 入力された文字列にファイル名に使えない文字("<>|:*?\/)が含んでいないかを確認し，
            // 含んでいなければファイル名として設定する．            
            string enteredStr = fileNameTextBox.Text;
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();            
            if (isContainInvalidChars(enteredStr, invalidChars)) {
                fileName = enteredStr;
                fileNameTextBox.Background = Brushes.White;
            } else {
                fileNameTextBox.Background = Brushes.Pink;
            }
        }

        private void destinationTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            // 入力された文字列にファイル名に使えない文字("<>|:*?\/)が含んでいないかを確認し，
            // 含んでいなければファイル名として設定する．            
            string enteredStr = destinationTextBox.Text;
            char[] invalidChars = System.IO.Path.GetInvalidPathChars();
            if (!isContainInvalidChars(enteredStr, invalidChars)) {
                destinationTextBox.Background = Brushes.Pink;
                return;                
            }

            if (System.IO.Directory.Exists(enteredStr)) {
                destinationDir = enteredStr;
                destinationTextBox.Background = Brushes.White;
            } else {
                destinationTextBox.Background = Brushes.Pink;
            }
        }

        /// <summary>
        /// 文字列に不適切な文字が含まれていないかを取得します．
        /// </summary>
        /// <param name="text">確認対象の文字列</param>
        /// <param name="invalidChars">不適切な文字のリスト</param>
        /// <returns>true: 含まれていない， false: 含まれている</returns>
        private bool isContainInvalidChars(string text, char[] invalidChars) {
            bool validFlag = true;
            foreach (char invalidChar in invalidChars) {
                validFlag &= (text.IndexOf(invalidChar) == -1);
            }
            return validFlag;
        }

        private void refButton_Click(object sender, RoutedEventArgs e) {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                destinationTextBox.Text = dialog.SelectedPath;
            }
        }

        private void abortButton_Click(object sender, RoutedEventArgs e) {
            AccumImage accumImage = AccumImage.GetInstance(parameterManager);
            accumImage.Abort();
        }
    }
}