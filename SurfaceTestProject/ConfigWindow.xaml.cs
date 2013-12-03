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

using NagaraStage.Activities;

namespace SurfaceTestProject {
    /// <summary>
    /// ConfigWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ConfigWindow : Window {

        public int NumOfSidePixcel = Surface.NumOfSidePixcelDefault;
        public int BrightnessThreshold = Surface.BrightnessThresholdUTDefault;
        public int BinarizeThreshold = Surface.BinarizeThresholdDefault;
        public int StartRow = Surface.StartRowDefault;
        public int EndRow = Surface.EndRowDefault;
        public double PowerOfDiffrence = Surface.PowerOfDifferenceDefault;

        public ConfigWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            numSideTextBox.Text = NumOfSidePixcel.ToString();
            thresholdTextBox.Text = BrightnessThreshold.ToString();
            binThresholdTextBox.Text = BinarizeThreshold.ToString();
            startRowTextBox.Text = StartRow.ToString();
            endRowTextBox.Text = EndRow.ToString();
            powerTextBox.Text = PowerOfDiffrence.ToString();
        }

        private void numSideDefaultButton_Click(object sender, RoutedEventArgs e) {
            numSideTextBox.Text = Surface.NumOfSidePixcelDefault.ToString();
        }

        private void thresholdDefaultButton_Click(object sender, RoutedEventArgs e) {
            thresholdTextBox.Text = Surface.BrightnessThresholdUTDefault.ToString();
        }

        private void startRowDefaultButton_Click(object sender, RoutedEventArgs e) {
            startRowTextBox.Text = Surface.StartRowDefault.ToString();
        }

        private void powerDefaultButton_Click(object sender, RoutedEventArgs e) {
            powerTextBox.Text = Surface.PowerOfDifferenceDefault.ToString();
        }

        private void endRowDefaultButton_Click(object sender, RoutedEventArgs e) {
            endRowTextBox.Text = Surface.EndRowDefault.ToString();
        }

        private void okButton_Click(object sender, RoutedEventArgs e) {
            try {
                NumOfSidePixcel = int.Parse(numSideTextBox.Text);
                BrightnessThreshold = int.Parse(thresholdTextBox.Text);
                BinarizeThreshold = int.Parse(binThresholdTextBox.Text);
                StartRow = int.Parse(startRowTextBox.Text);
                EndRow = int.Parse(endRowTextBox.Text);
                PowerOfDiffrence = double.Parse(powerTextBox.Text);
                DialogResult = true;
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void binThresholdDefaultButton_Click(object sender, RoutedEventArgs e) {
            binThresholdTextBox.Text = Surface.BinarizeThresholdDefault.ToString();
        }
    }
}
