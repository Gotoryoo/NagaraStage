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
using System.IO;
using System.Threading;

using NagaraStage;

namespace SurfaceTestProject {
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window {
        private string[] fileNames;
        private TextBlock[] textBlocks;
        private int[] brightnessValues;
        private BitmapSource[] bitmaps;
        private ConfigWindow configWindow = new ConfigWindow();

        public MainWindow() {
            InitializeComponent();
        }

        private void refButton_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Multiselect = true;
            if ((bool)dialog.ShowDialog(this)) {
                fileNames = dialog.FileNames;
                filesTextBox.Text = "";
                for (int i = 0; i < fileNames.Length; ++i) { 
                    filesTextBox.Text += string.Format(i == fileNames.Length -1 ? fileNames[i] : fileNames[i] + ",");
                }
            }
        }

        private void openButton_Click(object sender, RoutedEventArgs e) {
            InfoStackPanel.Children.RemoveRange(0, InfoStackPanel.Children.Count);            
            char[] delimiter = { ',' };
            string[] fileNames = filesTextBox.Text.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            if (fileNames.Length <= 0) {
                return;
            }
            openButton.IsEnabled = false;
            progressBar.Value = 0;
            progressBar.Minimum=0;
            progressBar.Maximum = fileNames.Length;
            progressPanel.Visibility = Visibility.Visible;
            textBlocks = new TextBlock[fileNames.Length];
            brightnessValues = new int[fileNames.Length];
            bitmaps = new BitmapSource[fileNames.Length];

            Thread gelThread = new Thread(new ThreadStart(delegate {
                for (int i = 0; i < fileNames.Length; ++i) {
                    BitmapFrame frame = BitmapFrame.Create(new Uri(fileNames[i]));
                    bitmaps[i] = ImageUtility.ToGrayScale(frame);
                    Gel.IsInGel(ImageUtility.ToArrayImage(bitmaps[i]), 
                                (int)frame.PixelWidth, (int)frame.PixelHeight,
                                configWindow.StartRow, configWindow.EndRow,
                                ref brightnessValues[i],
                                configWindow.NumOfSidePixcel, 
                                configWindow.BrightnessThreshold,
                                configWindow.BinarizeThreshold,
                                configWindow.PowerOfDiffrence);                   
                    Dispatcher.BeginInvoke(new Action(delegate {
                        progressBar.Value = i;
                        progressLabel.Content = string.Format(
                            "{0}/{1}", i.ToString(), fileNames.Length);
                    }), null);
                }
                Dispatcher.BeginInvoke(new Action(gelThreadCallback), null);
            }));
            gelThread.IsBackground = true;
            gelThread.Start();
        }

        private void gelThreadCallback() {
            for (int i = 0; i < brightnessValues.Length; ++i) {
                BitmapSource myBitmap = new BitmapImage(new Uri(fileNames[i]));
                string str = string.Format(
                    "{0} {1}: {2}",
                    i.ToString(),
                    (brightnessValues[i] < configWindow.BrightnessThreshold ? "Out": "In"), 
                    brightnessValues[i].ToString());
                textBlocks[i] = new TextBlock();
                textBlocks[i].Text = str;
                textBlocks[i].MouseDown += delegate(object o, MouseButtonEventArgs e2) {
                    emulsionImage.Source = myBitmap;
                    for (int n = 0; n < textBlocks.Length; ++n) {
                        if (textBlocks[n] != null) {
                            textBlocks[n].Background = Brushes.White;
                        }
                    }
                    TextBlock myTextBlock = o as TextBlock;
                    myTextBlock.Background = Brushes.BlueViolet;
                };
                InfoStackPanel.Children.Add(textBlocks[i]);
                progressPanel.Visibility = Visibility.Hidden;
                openButton.IsEnabled = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            progressPanel.Visibility = Visibility.Hidden;
        }

        private void confButton_Click(object sender, RoutedEventArgs e) {
            configWindow = new ConfigWindow();
            configWindow.ShowDialog();
        }

        private void outputButton_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            if ((bool)dialog.ShowDialog()) {
                StreamWriter sw = new StreamWriter(dialog.FileName);
                for (int i = 0; i < textBlocks.Length; ++i) {
                    string line = textBlocks[i].Text;
                    line = line.Replace(" ", ",");
                    line = line.Replace(":", ",");
                    sw.WriteLine(line);
                }
                sw.Close();
            }
        }
    }
}
