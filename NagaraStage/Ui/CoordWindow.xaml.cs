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

namespace NagaraStage.Ui {
    /// <summary>
    /// CoordWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class CoordWindow : Window {
        public CoordWindow() {
            InitializeComponent();
        }        
        
        /// <summary>
        /// X値のテキストボックスでTextChangedイベントが発生したときに実行されるメソッドを設定，または取得します．
        /// <para>引数に入力された文字列が与えられるので，一般にその文字列が適切かどうかをtrueもしくはfalseで返すといった使い方をします．</para>
        /// </summary>
        public Predicate<string> X_TextChanged;

        /// <summary>
        /// Y値のテキストボックスでTextChangedイベントが発生したときに実行されるメソッドを設定，または取得します．
        /// <para>引数に入力された文字列が与えられるので，一般にその文字列が適切かどうかをtrueもしくはfalseで返すといった使い方をします．</para>
        /// </summary>
        public Predicate<string> Y_TextChanged;

        /// <summary>
        /// OKボタンがクリックされて，入力値が不正だった場合に表示するメッセージを設定，または取得します．
        /// </summary>
        public string ErrorMessage = Properties.Strings.InputInvalidValue;

        private readonly Brush errorColor = Brushes.Pink;

        /// <summary>
        /// X値のテキストボックスに入力されているテキストを取得，または設定します．
        /// </summary>
        public string X {
            get { return xTextBox.Text; }
            set { xTextBox.Text = value; }
        }

        /// <summary>
        /// Y値のテキストボックスに入力されているテキストを取得，または設定します．
        /// </summary>
        public string Y {
            get { return yTextBox.Text; }
            set { yTextBox.Text = value; }
        }

        /// <summary>
        /// 単位名を設定，または取得します．
        /// </summary>
        public string UnitName {
            get { return yLabel.Content.ToString(); }
            set {
                xLabel.Content = value;
                yLabel.Content = value;
            }
        }

        private void okButton_Click(object sender, RoutedEventArgs e) {
            bool flag = true;
            flag &= (xTextBox.Background == Brushes.White);
            flag &= (yTextBox.Background == Brushes.White);
            if (!flag) {
                MessageBox.Show(ErrorMessage, Properties.Strings.Error);
                return;
            }

            DialogResult = true;
        }


        private void xTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (X_TextChanged != null) {
                xTextBox.Background = (X_TextChanged(xTextBox.Text) ? Brushes.White : errorColor);
            }
        }

        private void yTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (Y_TextChanged != null) {
                yTextBox.Background = (Y_TextChanged(yTextBox.Text) ? Brushes.White : errorColor);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e) {
            xTextBox.Focus();
        }
    }
}
