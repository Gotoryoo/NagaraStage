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
using System.IO;

namespace NagaraStage.Ui {
    /// <summary>
    /// OverAllScanConfigureDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class OverAllScanConfigureDialog : Window {
        private string directoryPath, _direcotryPath;
        private int nxView, _nxView;
        private int nyView, _nyView;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public OverAllScanConfigureDialog() {
            InitializeComponent();

            _nxView = Properties.Settings.Default.NumOfViewX;
            _nyView = Properties.Settings.Default.NumOfViewY;
            _direcotryPath = (Directory.Exists(Properties.Settings.Default.OverAllScanDirectory) ?
                Properties.Settings.Default.OverAllScanDirectory :
                System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
            innerValueToUi();
        }

        /// <summary>
        /// 保存先ディレクトリパス
        /// </summary>
        public string DirectoryPath {
            get { return directoryPath; }
            set { directoryPath = value; } 
        }

        /// <summary>
        /// 撮影する視野数X
        /// </summary>
        public int NumOfViewX {
            get { return nxView; }
            set {
                nxView = value;
                isValidate();
            }
        }

        /// <summary>
        /// 撮影する視野数Y
        /// </summary>
        public int NumOfViewY {
            get { return nyView; }
            set {
                nyView = value;
                isValidate();
            }
        }

        private void innerValueToUi() {
            xViewTextBox.Text = _nxView.ToString();
            yViewTextBox.Text = _nyView.ToString();
            directoryPathTextBox.Text = _direcotryPath;
        }

        private Boolean isValidate() {
            if(nxView < 0) {
                throw new ArgumentException("NumOfViewX(nxView) must be a positive number.");
            }
            if(nyView < 0) {
                throw new ArgumentException("NumOfViewY(nxView) must be a positive number.");
            }
            if(!Directory.Exists(directoryPath)) {
                throw new ArgumentException("Specified directory does not exist.");
            }
            return true;
        }

        private Boolean isEnteredValueCorrect() {
            return (xViewTextBox.Background == Brushes.White &&
                    yViewTextBox.Background == Brushes.White &&
                    directoryPathTextBox.Background == Brushes.White);
        }

        private void okButton_Click(object sender, RoutedEventArgs e) {
            try {
                if (isEnteredValueCorrect()) {
                    nxView = _nxView;
                    nyView = _nyView;
                    directoryPath = _direcotryPath;
                    isValidate();
                    Properties.Settings.Default.NumOfViewX = NumOfViewX;
                    Properties.Settings.Default.NumOfViewY = NumOfViewY;
                    Properties.Settings.Default.OverAllScanDirectory = DirectoryPath;
                    DialogResult = true;
                } else {
                    MessageBox.Show("Incorrect value(s) exist. Check entered value(s).");
                }
            } catch (ArgumentException ex) {
                MessageBox.Show(ex.Message);               
            }
        }

        private void xViewTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                _nxView = int.Parse(xViewTextBox.Text);
                if(_nxView < 0) {
                    throw new ArgumentException();
                }
                xViewTextBox.Background = Brushes.White;            
            } catch(Exception)  {
                xViewTextBox.Background = Brushes.Pink;
            }
        }

        private void yViewTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                _nyView = int.Parse(yViewTextBox.Text);
                if(_nyView < 0) {
                    throw new ArgumentException();
                }
                yViewTextBox.Background = Brushes.White;
            } catch (Exception) {
                yViewTextBox.Background = Brushes.Pink;
            }
        }

        private void directoryPathTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                _direcotryPath = directoryPathTextBox.Text;
                if(!Directory.Exists(_direcotryPath)) {
                    throw new ArgumentException();
                }
                directoryPathTextBox.Background = Brushes.White;
            } catch(Exception) {
                directoryPathTextBox.Background = Brushes.Pink;
            }
        }

        private void directoryRefButton_Click(object sender, RoutedEventArgs e) {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                directoryPathTextBox.Text = dialog.SelectedPath;
            }
        }
    }
}
