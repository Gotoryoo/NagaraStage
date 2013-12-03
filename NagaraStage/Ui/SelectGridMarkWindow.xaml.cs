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

namespace NagaraStage {
    namespace Ui {
        /// <summary>
        /// SelectGridMarkWindow.xaml の相互作用ロジック
        /// </summary>
        public partial class SelectGridMarkWindow : Window {
            private GridMarkPoint selectedPoint;

            /// <summary>
            /// 選択されたグリッドマークを取得，または設定します．
            /// </summary>
            public GridMarkPoint GridMarkPoint {
                get { return selectedPoint; }
                set {
                    switch (value) {
                        case GridMarkPoint.LeftTop:
                            leftTopRadioButton.IsChecked = true;
                            break;
                        case GridMarkPoint.LeftMiddle:
                            leftMiddleRadioButton.IsChecked = true;
                            break;
                        case GridMarkPoint.LeftBottom:
                            leftBottomRadioButton.IsChecked = true;
                            break;
                        case GridMarkPoint.CenterTop:
                            centerTopRadioButton.IsChecked = true;
                            break;
                        case GridMarkPoint.CenterMiddle:
                            centralRadioButton.IsChecked = true;
                            break;
                        case GridMarkPoint.CenterBottom:
                            centerBottomRadioButton.IsChecked = true;
                            break;
                        case GridMarkPoint.RightTop:
                            rightTopRadioButton.IsChecked = true;
                            break;
                        case GridMarkPoint.RightMiddle:
                            rightMiddleRadioButton.IsChecked = true;
                            break;
                        case GridMarkPoint.RightBottom:
                            rightBottomRadioButton.IsChecked = true;
                            break;
                    }
                }
            }

            public SelectGridMarkWindow() {
                InitializeComponent();
            }

            private void okButton_Click(object sender, RoutedEventArgs e) {
                DialogResult = true;
            }

            private void leftTopRadioButton_Checked(object sender, RoutedEventArgs e) {
                selectedPoint = GridMarkPoint.LeftTop;
            }

            private void leftMiddleRadioButton_Checked(object sender, RoutedEventArgs e) {
                selectedPoint = GridMarkPoint.LeftMiddle;
            }

            private void leftBottomRadioButton_Checked(object sender, RoutedEventArgs e) {
                selectedPoint = GridMarkPoint.LeftBottom;
            }

            private void centerTopRadioButton_Checked(object sender, RoutedEventArgs e) {
                selectedPoint = GridMarkPoint.CenterTop;
            }

            private void centralRadioButton_Checked(object sender, RoutedEventArgs e) {
                selectedPoint = GridMarkPoint.CenterMiddle;
            }

            private void centerBottomRadioButton_Checked(object sender, RoutedEventArgs e) {
                selectedPoint = GridMarkPoint.CenterBottom;
            }

            private void rightTopRadioButton_Checked(object sender, RoutedEventArgs e) {
                selectedPoint = GridMarkPoint.RightTop;
            }

            private void rightMiddleRadioButton_Checked(object sender, RoutedEventArgs e) {
                selectedPoint = GridMarkPoint.RightMiddle;
            }

            private void rightBottomRadioButton_Checked(object sender, RoutedEventArgs e) {
                selectedPoint = GridMarkPoint.RightBottom;
            }
        }
    }
}