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

namespace NagaraStage.Ui {
    /// <summary>
    /// SystemConfControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SystemConfControl : Workspace, IDialogWorkspace {
        private IMainWindow window;
        private MessageBoxResult result;

        public SystemConfControl(IMainWindow _window) : base(_window) {
            InitializeComponent();
            this.window = _window;
        }

        /// <summary>ダイアログボックスの結果を取得します．</summary>
        public MessageBoxResult Result {
            get { return result; }
        }

        private void okButton_Click(object sender, RoutedEventArgs e) {
            if (!isCorrectValues) {
                MessageBox.Show(Properties.Strings.InputInvalidValue, Properties.Strings.Error);
                return;
            }
            if (MessageBox.Show(Properties.Strings.AskReallySetting, "", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                Properties.Settings.Default.AccelTimeX = double.Parse(accelTimeXTextBox.Text);
                Properties.Settings.Default.AccelTimeY = double.Parse(accelTimeYTextBox.Text);
                Properties.Settings.Default.AccelTimeZ = double.Parse(accelTimeZTextBox.Text);
                Properties.Settings.Default.EncoderResolutionX = double.Parse(encoderResolutionXTextBox.Text);
                Properties.Settings.Default.EncoderResolutionY = double.Parse(encoderResolutionYTextBox.Text);
                Properties.Settings.Default.EncoderResolutionZ = double.Parse(encoderResolutionZTextBox.Text);
                Properties.Settings.Default.MotorResolutionX = double.Parse(motorResolutionXTextBox.Text);
                Properties.Settings.Default.MotorResolutionY = double.Parse(motorResolutionYTextBox.Text);
                Properties.Settings.Default.MotorResolutionZ = double.Parse(motorResolutionZTextBox.Text);
                Properties.Settings.Default.InitialVelocityX = double.Parse(initialVelocityXTextBox.Text);
                Properties.Settings.Default.InitialVelocityY = double.Parse(initialVelocityYTextBox.Text);
                Properties.Settings.Default.InitialVelocityZ = double.Parse(initialVelocityZTextBox.Text);
                Properties.Settings.Default.Speed1X = double.Parse(speed1XTextBox.Text);
                Properties.Settings.Default.Speed1Y = double.Parse(speed1YTextBox.Text);
                Properties.Settings.Default.Speed1Z = double.Parse(speed1ZTextBox.Text);
                Properties.Settings.Default.Speed2X = double.Parse(speed2XTextBox.Text);
                Properties.Settings.Default.Speed2Y = double.Parse(speed2YTextBox.Text);
                Properties.Settings.Default.Speed2Z = double.Parse(speed2ZTextBox.Text);
                Properties.Settings.Default.Speed3X = double.Parse(speed3XTextBox.Text);
                Properties.Settings.Default.Speed3Y = double.Parse(speed3YTextBox.Text);
                Properties.Settings.Default.Speed3Z = double.Parse(speed3ZTextBox.Text);
                Properties.Settings.Default.Speed4X = double.Parse(speed4XTextBox.Text);
                Properties.Settings.Default.Speed4Y = double.Parse(speed4YTextBox.Text);
                Properties.Settings.Default.Speed4Z = double.Parse(speed4ZTextBox.Text);
                Properties.Settings.Default.LedParam = ledPortTextBox.Text;
                Properties.Settings.Default.LimitPol = double.Parse(limitPolTextBox.Text);
                Properties.Settings.Default.ScanAreaXm = double.Parse(scanAreaXmTextBox.Text);
                Properties.Settings.Default.ScanAreaXp = double.Parse(scanAreaXpTextBox.Text);
                Properties.Settings.Default.ScanAreaYm = double.Parse(scanAreaYmTextBox.Text);
                Properties.Settings.Default.ScanAreaYp = double.Parse(scanAreaYpTextBox.Text);
                result = MessageBoxResult.OK;
                Finish();
                App.Restart();
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e) {
            result = MessageBoxResult.Cancel;
            Finish();
        }

        /// <summary>
        /// 入力された値が正しいかどうかを取得します．
        /// </summary>
        private bool isCorrectValues {
            get { 
                return(
                    accelTimeXTextBox.Background == Brushes.White
                    && accelTimeYTextBox.Background == Brushes.White
                    && accelTimeZTextBox.Background == Brushes.White
                    && encoderResolutionXTextBox.Background == Brushes.White
                    && encoderResolutionYTextBox.Background == Brushes.White
                    && encoderResolutionZTextBox.Background == Brushes.White
                    && motorResolutionXTextBox.Background == Brushes.White
                    && motorResolutionYTextBox.Background == Brushes.White
                    && motorResolutionZTextBox.Background == Brushes.White
                    && initialVelocityXTextBox.Background == Brushes.White
                    && initialVelocityYTextBox.Background == Brushes.White
                    && initialVelocityZTextBox.Background == Brushes.White
                    && speed1XTextBox.Background == Brushes.White
                    && speed1YTextBox.Background == Brushes.White
                    && speed1ZTextBox.Background == Brushes.White
                    && speed2XTextBox.Background == Brushes.White
                    && speed2YTextBox.Background == Brushes.White
                    && speed2ZTextBox.Background == Brushes.White
                    && speed3XTextBox.Background == Brushes.White
                    && speed3YTextBox.Background == Brushes.White
                    && speed3ZTextBox.Background == Brushes.White
                    && speed4XTextBox.Background == Brushes.White
                    && speed4YTextBox.Background == Brushes.White
                    && speed4ZTextBox.Background == Brushes.White
                    && scanAreaXmTextBox.Background == Brushes.White
                    && scanAreaXpTextBox.Background == Brushes.White
                    && scanAreaYmTextBox.Background == Brushes.White
                    && scanAreaYpTextBox.Background == Brushes.White
                    //&& ledPortTextBox.Background == Brushes.White
                    && limitPolTextBox.Background == Brushes.White
                    );
            }
        }

        /// <summary>
        /// 0より大きい値が入力されるべきテストボックスのイベントハンドラです．
        /// </summary>
        private void positiveNotZero_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox textBox = sender as TextBox;
            try {                
                double val = double.Parse(textBox.Text);
                if (val <= 0) {
                    throw new ArgumentException(Properties.Strings.ValMustPositiveNotZero);
                }
                textBox.Background = Brushes.White;
            } catch(Exception) {
                textBox.Background = Brushes.Pink;
            }
        }

        private void scanArea_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox textBox = sender as TextBox;
            try {
                int val = int.Parse(textBox.Text);                
                textBox.Background = Brushes.White;
            } catch (Exception) {
                textBox.Background = Brushes.Pink;
            }
        }

        private void Workspace_Loaded_1(object sender, RoutedEventArgs e) {
            accelTimeXTextBox.Text = Properties.Settings.Default.AccelTimeX.ToString();
            accelTimeYTextBox.Text = Properties.Settings.Default.AccelTimeY.ToString();
            accelTimeZTextBox.Text = Properties.Settings.Default.AccelTimeZ.ToString();
            encoderResolutionXTextBox.Text = Properties.Settings.Default.EncoderResolutionX.ToString();
            encoderResolutionYTextBox.Text = Properties.Settings.Default.EncoderResolutionY.ToString();
            encoderResolutionZTextBox.Text = Properties.Settings.Default.EncoderResolutionZ.ToString();
            motorResolutionXTextBox.Text = Properties.Settings.Default.MotorResolutionX.ToString();
            motorResolutionYTextBox.Text = Properties.Settings.Default.MotorResolutionY.ToString();
            motorResolutionZTextBox.Text = Properties.Settings.Default.MotorResolutionZ.ToString();
            initialVelocityXTextBox.Text = Properties.Settings.Default.InitialVelocityX.ToString();
            initialVelocityYTextBox.Text = Properties.Settings.Default.InitialVelocityY.ToString();
            initialVelocityZTextBox.Text = Properties.Settings.Default.InitialVelocityZ.ToString();
            scanAreaXmTextBox.Text = Properties.Settings.Default.ScanAreaXm.ToString();
            scanAreaXpTextBox.Text = Properties.Settings.Default.ScanAreaXp.ToString();
            scanAreaYmTextBox.Text = Properties.Settings.Default.ScanAreaYm.ToString();
            scanAreaYpTextBox.Text = Properties.Settings.Default.ScanAreaYp.ToString();
            speed1XTextBox.Text = Properties.Settings.Default.Speed1X.ToString();
            speed1YTextBox.Text = Properties.Settings.Default.Speed1Y.ToString();
            speed1ZTextBox.Text = Properties.Settings.Default.Speed1Z.ToString();
            speed2XTextBox.Text = Properties.Settings.Default.Speed2X.ToString();
            speed2YTextBox.Text = Properties.Settings.Default.Speed2Y.ToString();
            speed2ZTextBox.Text = Properties.Settings.Default.Speed2Z.ToString();
            speed3XTextBox.Text = Properties.Settings.Default.Speed3X.ToString();
            speed3YTextBox.Text = Properties.Settings.Default.Speed3Y.ToString();
            speed3ZTextBox.Text = Properties.Settings.Default.Speed3Z.ToString();
            speed4XTextBox.Text = Properties.Settings.Default.Speed4X.ToString();
            speed4YTextBox.Text = Properties.Settings.Default.Speed4Y.ToString();
            speed4ZTextBox.Text = Properties.Settings.Default.Speed4Z.ToString();
            ledPortTextBox.Text = Properties.Settings.Default.LedParam;
            limitPolTextBox.Text = Properties.Settings.Default.LimitPol.ToString();
        }
    }
}
