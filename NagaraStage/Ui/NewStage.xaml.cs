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

using NagaraStage.Parameter;

namespace NagaraStage.Ui {

    /// <summary>
    /// NewStagePage.xaml の相互作用ロジック
    /// </summary>
    public partial class NewStage : Workspace, IDialogWorkspace {
        private IMainWindow window;
        private ParameterManager parameterManager;
        private ParameterManager parameterBuffer;
        private MessageBoxResult dialogResult;
        private DispatcherTimer dateTimer;

        /// <summary>
        /// ユーザーがダイアログボックスのボタンのうち，どれを選択したのかを取得します．
        /// </summary>
        public MessageBoxResult Result {
            get { return dialogResult; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mainWindow">呼び出し元IMaionWindowインスタンス</param>
        public NewStage(IMainWindow mainWindow)
            : base(mainWindow) {
            InitializeComponent();
            this.window = mainWindow;
            this.parameterManager = window.ParameterManager;
            this.parameterBuffer = parameterManager.Clone();
            this.overIndexTextBox.Text = String.Format("{0:0.000}", Properties.Settings.Default.IndexUp);
            this.underIndexTextBox.Text = String.Format("{0:0.000}", Properties.Settings.Default.IndexDown);
            this.overIndexTextBox.GotKeyboardFocus += overIndexTextBox_GotKeyboardFocus;
            this.overIndexTextBox.PreviewMouseLeftButtonDown += overIndexTextBox_MouseLeftButtonDown;
            this.underIndexTextBox.GotKeyboardFocus += underIndexTextBox_GotKeyboardFocus;
            this.underIndexTextBox.PreviewMouseLeftButtonDown += underIndexTextBox_MouseLeftButtonDown;
            this.dateTimer = new DispatcherTimer(DispatcherPriority.Normal);
            this.dateTimer.Interval = new TimeSpan(0, 0, 1);
            this.dateTimer.Tick += new EventHandler(dateTimer_Tick);
            this.dateTimer.Start();
            this.nameTextBox.Text = parameterManager.UserName;
            this.mailTextBox.Text = parameterManager.MailAddress;
        }

        void dateTimer_Tick(object sender, EventArgs e) {
            if (!(bool)dateChangeCheckBox.IsChecked) {
                string dateTime = DateTime.Now.ToLongDateString() + DateTime.Now.ToLongTimeString();
                dateTextBox.Text = dateTime;
            }
        }

        private void plateNoTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                parameterBuffer.PlateNo = int.Parse(plateNoTextBox.Text);
                plateNoTextBox.Background = Brushes.White;
            } catch {
                plateNoTextBox.Background = Brushes.Pink;
            }
        }

        private void moduleNoTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                int moduleNo = int.Parse(moduleNoTextBox.Text);
                parameterBuffer.ModuleNo = moduleNo;

                if (parameterBuffer.ModuleNo == 1) {
                    thinRadioButton.IsChecked = true;
                } else {
                    thickRadioButton.IsChecked = true;
                }
                moduleNoTextBox.Background = Brushes.White;
            } catch (Exception) {
                moduleNoTextBox.Background = Brushes.Pink;
            }
        }

        private void thickRadioButton_Checked(object sender, RoutedEventArgs e) {
            parameterBuffer.EmulsionType = EmulsionType.ThickType;
        }

        private void thinRadioButton_Checked(object sender, RoutedEventArgs e) {
            parameterBuffer.EmulsionType = EmulsionType.ThinType;
        }

        private void overIndexTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                parameterBuffer.EmulsionIndexUp = double.Parse(overIndexTextBox.Text);
                overIndexTextBox.Background = Brushes.White;
            } catch {
                overIndexTextBox.Background = Brushes.Pink;
            }
        }

        private void underIndexTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                parameterBuffer.EmulsionIndexDown = double.Parse(underIndexTextBox.Text);
                underIndexTextBox.Background = Brushes.White;
            } catch {
                underIndexTextBox.Background = Brushes.Pink;
            }
        }

        private void overIndexTextBox_GotKeyboardFocus(object sender, EventArgs e) {
            overIndexTextBox.SelectAll();
        }

        private void underIndexTextBox_GotKeyboardFocus(object sender, EventArgs e) {
            underIndexTextBox.SelectAll();
        }

        private void overIndexTextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            overIndexTextBox.SelectAll();
            overIndexTextBox.Focus();
            e.Handled = true;
        }

        private void underIndexTextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            underIndexTextBox.SelectAll();
            underIndexTextBox.Focus();
            e.Handled = true;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e) {
            dialogResult = MessageBoxResult.Cancel;
            Finish();
        }

        private void okButton_Click(object sender, RoutedEventArgs e) {
            // Index値が2より大きければ注意ダイアログを表示し，
            // ユーザーが「はい」をクリックした時のみ続行する．
            if (isIndexValueCorrect()) {
                MessageBoxResult r = MessageBox.Show(
                    Properties.Strings.IndexFarExceptionWarning,
                    Properties.Strings.Attention,
                    MessageBoxButton.YesNo);
                if (r != MessageBoxResult.Yes) {
                    return;
                }
            }
            try {
                // 設定中の値を格納していたバッファから元ののparameterManagerにコピーする
                parameterManager.EmulsionType = parameterBuffer.EmulsionType;
                parameterManager.PlateNo = parameterBuffer.PlateNo;
                parameterManager.ModuleNo = parameterBuffer.ModuleNo;
                parameterManager.EmulsionIndexUp = parameterBuffer.EmulsionIndexUp;
                parameterManager.EmulsionIndexDown = parameterBuffer.EmulsionIndexDown;
                parameterManager.UserName = parameterBuffer.UserName;
                parameterManager.MailAddress = parameterBuffer.MailAddress;
                parameterManager.EmulsionParameter.Load(emConfPathTextBox.Text);

                // このコントロールをウィンドウから削除
                dialogResult = MessageBoxResult.OK;
                Finish();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private Boolean isIndexValueCorrect() {
            return (Math.Abs(parameterBuffer.EmulsionIndexUp) - 2 > 0.5
                || Math.Abs(parameterBuffer.EmulsionIndexDown) - 2 > 0.5
                ? false : true);
        }

        private void skipButton_Click(object sender, RoutedEventArgs e) {
            parameterManager.EmulsionType = EmulsionType.ThickType;

            // このコントロールをウィンドウから削除
            dialogResult = MessageBoxResult.Yes;
            Finish();
        }

        private void dateTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            if ((bool)dateChangeCheckBox.IsChecked) {
                try {
                    DateTime.Parse(dateTextBox.Text);
                    dateTextBox.Background = Brushes.White;
                } catch (Exception) {
                    dateTextBox.Background = Brushes.Pink;
                }
            }
        }

        private void dateChangeCheckBox_Checked(object sender, RoutedEventArgs e) {
            dateTextBox.IsEnabled = true;
        }

        private void dateChangeCheckBox_Unchecked(object sender, RoutedEventArgs e) {
            dateTextBox.IsEnabled = false;
        }

        private void emConfReferenceButton_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog dialog = new OpenFileDialog(OpenFileDialogMode.IniFile);
            if (dialog.ShowDialog()) {
                emConfPathTextBox.Text = dialog.FileName;
            }
        }

        private void nameTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            parameterBuffer.UserName = nameTextBox.Text;
            nameTextBox.Background = (nameTextBox.Text.Length == 0 ? Brushes.Pink : Brushes.White);
        }

        private void mailTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            parameterBuffer.MailAddress = mailTextBox.Text;
            mailTextBox.Background = (mailTextBox.Text.Length == 0 ? Brushes.Pink : Brushes.White);
        }


    }

}