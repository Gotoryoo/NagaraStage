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

using NagaraStage;
using NagaraStage.Parameter;
using NagaraStage.Activities;

namespace NagaraStage.Ui {
    /// <summary>
    /// SurfaceConfig.xaml の相互作用ロジック
    /// </summary>
    public partial class SurfaceConfig : Workspace, IDialogWorkspace {
        private IMainWindow window;
        private ParameterManager parameterManager;
        private int numOfSideBuffer;
        private int brightnessThresholdBuffer;
        private int binarizeThresholdBuffer;
        private int startRowBuffer;
        private int endRowBuffer;
        private double distanceOutBuffer;
        private double powerOfDifferenceBuffer;
        private double motorSpeedBuffer;
        private int timeDelayBuffer;
        private double loweringThin;
        private double loweringThick;
        private int numOfVoting;
        private MessageBoxResult dialogResult;

        /// <summary>ダイアログボックスの結果を取得します．．</summary>
        public MessageBoxResult Result {
            get { return dialogResult; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurfaceConfig" /> class.
        /// </summary>
        /// <param name="_window">The _window.</param>
        public SurfaceConfig(IMainWindow _window) : base(_window) {
            InitializeComponent();
            this.window = _window;
            this.parameterManager = window.ParameterManager;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e) {
            dialogResult = MessageBoxResult.Cancel;
            Finish();
        }

        private void applyButton_Click(object sender, RoutedEventArgs e) {
            if (!isEnteredValueAvairable()) {
                MessageBox.Show(Properties.Strings.InputInvalidValue, Properties.Strings.Error);
                return;
            }

            if (startRowBuffer >= endRowBuffer) {
                string message = string.Format(
                    Properties.Strings.ValMustMore,
                    Properties.Strings.EndRow,
                    Properties.Strings.StartRow);
                MessageBox.Show(message, Properties.Strings.Error);
                return;
            }

            // 入力された値を適用する            
            Surface surface = Surface.GetInstance(parameterManager);
            surface.NumOfSidePixcel = numOfSideBuffer;
            surface.BrightnessThreshold = brightnessThresholdBuffer;
            surface.BinarizeThreshold = binarizeThresholdBuffer;
            surface.StartRow = startRowBuffer;
            surface.EndRow = endRowBuffer;
            surface.DistanceOut = distanceOutBuffer;
            surface.PowerOfDiffrence = powerOfDifferenceBuffer;
            surface.DelayTime = timeDelayBuffer;
            surface.MotorSpeed = motorSpeedBuffer;
            surface.LoweringDistanceThick = loweringThick;
            surface.LoweringDistanceThin = loweringThin;
            surface.NumOfVoting = numOfVoting;

            dialogResult = MessageBoxResult.OK;
            Finish();
        }

        private Boolean isEnteredValueAvairable() {
            return (numOfSideTextBox.Background == Brushes.White
                && brightnessThresholdTextBox.Background == Brushes.White
                && startRowTextBox.Background == Brushes.White
                && endRowTextBox.Background == Brushes.White
                && distanceOutTextBox.Background == Brushes.White
                && powerDifferenceTextBox.Background == Brushes.White
                && delayTimeTextBox.Background == Brushes.White);
        }

        private void surfaceConfig_Loaded(object sender, RoutedEventArgs e) {
            Surface surface = Surface.GetInstance(parameterManager);
            numOfSideTextBox.Text = surface.NumOfSidePixcel.ToString();
            brightnessThresholdTextBox.Text = surface.BrightnessThreshold.ToString();
            binThresholdTextBox.Text = surface.BinarizeThreshold.ToString();
            endRowTextBox.Text = surface.EndRow.ToString();
            startRowTextBox.Text = surface.StartRow.ToString();
            distanceOutTextBox.Text = surface.DistanceOut.ToString();
            powerDifferenceTextBox.Text = surface.PowerOfDiffrence.ToString();
            delayTimeTextBox.Text = surface.DelayTime.ToString();
            motorSpeedTextbox.Text = surface.MotorSpeed.ToString("0.00");
            loweringThickTextBox.Text = Properties.Settings.Default.SurfaceLoweringThick.ToString();
            loweringThinTextBox.Text = Properties.Settings.Default.SurfaceLoweringThin.ToString();
            numVotingTextBox.Text = Properties.Settings.Default.SurfaceVoting.ToString();

            if (surface.IsActive) {
                MessageBoxResult r = MessageBox.Show(
                    Properties.Strings.SurfaceException02,
                    Properties.Strings.Attention,
                    MessageBoxButton.YesNo);
                if (r == MessageBoxResult.Yes) {
                    surface.Abort();
                } else {
                    cancelButton_Click(new object(), new RoutedEventArgs());
                }
            }
        }

        private void defaultSideButton_Click(object sender, RoutedEventArgs e) {
            numOfSideTextBox.Text = Surface.NumOfSidePixcelDefault.ToString();
        }

        private void defaultBrightnessButton_Click(object sender, RoutedEventArgs e) {
            brightnessThresholdTextBox.Text = Surface.BrightnessThresholdUTDefault.ToString();
        }

        private void defaultStartRowButton_Click(object sender, RoutedEventArgs e) {
            startRowTextBox.Text = Surface.StartRowDefault.ToString();
        }

        private void defaultDistanceOutButton_Click(object sender, RoutedEventArgs e) {
            distanceOutTextBox.Text = Surface.DistanceOutDefault.ToString();
        }

        private void defaultEndRowButton_Click(object sender, RoutedEventArgs e) {
            endRowTextBox.Text = Surface.EndRowDefault.ToString();
        }

        private void numOfSideTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                numOfSideBuffer = int.Parse(numOfSideTextBox.Text);

                // 入力値を確認して，不適切な場合は例外を発生させる．
                if (numOfSideBuffer < 2) {
                    string message = string.Format(Properties.Strings.ValMustOrMore, Properties.Strings.NumOfSidePixcel, 2);
                    throw new ArgumentException(message);
                }
                // 例外が発生することなく，値が適切であった場合はテキストボックスを白色にする．
                numOfSideTextBox.Background = Brushes.White;
                numOfSideLabel.Content = null;
            } catch (Exception ex) {
                if (ex.Message != null) {
                    numOfSideLabel.Content = ex.Message;
                }
                numOfSideTextBox.Background = Brushes.Pink;
            }
        }

        private void brightnessThresholdTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                brightnessThresholdBuffer = int.Parse(brightnessThresholdTextBox.Text);
                if (brightnessThresholdBuffer < 0) {
                    string message = string.Format(Properties.Strings.ValMustPositive, Properties.Strings.BrightnessThreshold);
                    throw new ArgumentException(message);
                }

                brightnessThresholdTextBox.Background = Brushes.White;
                brightnessLabel.Content = null;
            } catch (Exception ex) {
                if (ex.Message != null) {
                    brightnessLabel.Content = ex.Message;
                }
                brightnessThresholdTextBox.Background = Brushes.Pink;
            }
        }

        private void startRowTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            // StartRowの値を確認
            try {
                startRowBuffer = int.Parse(startRowTextBox.Text);
                if (startRowBuffer < 0) {
                    string message = string.Format(
                        Properties.Strings.ValMustPositive, Properties.Strings.StartRow);
                    throw new ArgumentException(message);
                } else if (startRowBuffer < numOfSideBuffer / 2) {
                    string message = string.Format(
                        Properties.Strings.ValTooSmall, Properties.Strings.StartRow);
                    throw new ArgumentException(message);
                } 
            } catch (Exception ex) {
                rowLabel.Content = ex.Message;
                startRowTextBox.Background = Brushes.Pink;
                return;
            }

            // StartRowとEndRowの値の関係を確認
            try {
                checkRowItems();
                startRowTextBox.Background = Brushes.White;
                rowLabel.Content = null;
            } catch (Exception ex) {
                rowLabel.Content = ex.Message;                
                startRowTextBox.Background = Brushes.Pink;
            }
        }

        private void endRowTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            // EndRowの値を確認
            try {
                endRowBuffer = int.Parse(endRowTextBox.Text);
                if (endRowBuffer + (numOfSideBuffer / 2)                 
                        > ParameterManager.ImageResolution.Height) {
                    string message = string.Format(
                        Properties.Strings.ValTooLarge, Properties.Strings.EndRow);
                    throw new ArgumentException(message);
                }
            } catch(Exception ex) {
                rowLabel.Content = ex.Message;
                endRowTextBox.Background = Brushes.Pink;
                return;
            }

            // StartRowとEndRowの値の関係を確認
            try {
                checkRowItems();
                endRowTextBox.Background = Brushes.White;
                rowLabel.Content = null;
            } catch (Exception ex) {
                if (ex.Message != null) {
                    rowLabel.Content = ex.Message;
                }
                endRowTextBox.Background = Brushes.Pink;
            }
        }

        /// <summary>
        /// 開始行，終了行の入力値の関係が適切かを確認します．
        /// <para>値が不正であった場合，例外が投げられます．</para>
        /// </summary>
        private void checkRowItems() {
            if (startRowBuffer < 0 && endRowBuffer < 0) {
                string rows = Properties.Strings.StartRow + "," + Properties.Strings.EndRow;
                string message = string.Format(Properties.Strings.ValMustPositive, rows);
                throw new ArgumentException(message);
            } else if (startRowBuffer >= endRowBuffer) {
                string message = string.Format(Properties.Strings.ValMustMore, Properties.Strings.EndRow, Properties.Strings.StartRow);
                throw new ArgumentException(message);
            } 
        }

        private void distanceOutTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                distanceOutBuffer = int.Parse(distanceOutTextBox.Text);

                if (distanceOutBuffer < 0) {
                    string message = string.Format(Properties.Strings.ValMustPositive, Properties.Strings.DistanceOut);
                    throw new ArgumentException(message);
                }

                distanceOutTextBox.Background = Brushes.White;
                distanceLabel.Content = null;
            } catch (Exception ex) {
                if (ex.Message != null) {
                    distanceLabel.Content = ex.Message;
                }
                distanceOutTextBox.Background = Brushes.Pink;
            }
        }

        private void powerDifferenceTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                powerOfDifferenceBuffer = double.Parse(powerDifferenceTextBox.Text);
                powerDifferenceTextBox.Background = Brushes.White;
                powerLabel.Content = null;
            } catch (Exception ex) {
                if (ex.Message != null) {
                    powerLabel.Content = ex.Message;
                }
                powerDifferenceTextBox.Background = Brushes.Pink;
            }
        }

        private void delayTimeTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                timeDelayBuffer = int.Parse(delayTimeTextBox.Text);
                delayTimeTextBox.Background = Brushes.White;
                delayTimeLabel.Content = null;
            } catch (Exception ex) {
                delayTimeLabel.Content = ex.Message;
                delayTimeTextBox.Background = Brushes.Pink;
            }
        }

        private void delayTimeDefaultButton_Click(object sender, RoutedEventArgs e) {
            delayTimeTextBox.Text = Surface.DelayTimeDefault.ToString();
        }

        private void motorSpeedTextbox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                motorSpeedBuffer = double.Parse(motorSpeedTextbox.Text);
                motorSpeedTextbox.Background = Brushes.White;
                motorSpeedLabel.Content = null;
            } catch (Exception ex) {
                motorSpeedLabel.Content = ex.Message;
                motorSpeedTextbox.Background = Brushes.Pink;
            }
        }

        private void motorSpeedDefaultButton_Click_1(object sender, RoutedEventArgs e) {
            motorSpeedTextbox.Text = Surface.DefaultSpeed.ToString();
        }        

        private void loweringThinTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                loweringThin = double.Parse(loweringThinTextBox.Text);

                if (loweringThin <= 0) {
                    throw new ArgumentException(
                        string.Format(Properties.Strings.ValMustPositiveNotZero),
                        Properties.Strings.SurfaceLoweringDistance);
                }

                loweringThinTextBox.Background = Brushes.White;
                lowringLabel.Content = null;
            } catch (Exception ex) {
                if (ex.Message != null) {
                    lowringLabel.Content = ex.Message;
                }
                loweringThinTextBox.Background = Brushes.Pink;
            }
        }

        private void loweringThickTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                loweringThick = double.Parse(loweringThickTextBox.Text);

                if (loweringThick <= 0) {
                    throw new ArgumentException(
                        string.Format(Properties.Strings.ValMustPositiveNotZero),
                        Properties.Strings.SurfaceLoweringDistance);
                }

                loweringThickTextBox.Background = Brushes.White;
                lowringLabel.Content = null;
            } catch (Exception ex) {
                if (ex.Message != null) {
                    lowringLabel.Content = ex.Message;
                }
                loweringThickTextBox.Background = Brushes.Pink;
            }
        }

        private void lowringThinButton_Click(object sender, RoutedEventArgs e) {
            loweringThinTextBox.Text = Surface.LoweringDistanceThinDefault.ToString();
        }

        private void lowringThickButton_Click(object sender, RoutedEventArgs e) {
            loweringThickTextBox.Text = Surface.LoweringDistanceThickDefault.ToString();
        }

        private void numVotingTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                numOfVoting = int.Parse(numVotingTextBox.Text);
                if (numOfVoting <= 0) {
                    throw new ArgumentException(Properties.Strings.ValMustPositive);
                } else if (numOfVoting % 2 == 0) {
                    throw new ArgumentException(Properties.Strings.ValMustOdd);
                }
                numVotingLabel.Content = null;
                numVotingTextBox.Background = Brushes.White;
            } catch (Exception ex) {
                if (ex.Message != null) {
                    numVotingLabel.Content = ex.Message;
                }
                numVotingTextBox.Background = Brushes.Pink;
            }
        }

        private void numVotingDefaultButton_Click(object sender, RoutedEventArgs e) {
            numOfSideTextBox.Text = Properties.Settings.Default.SurfaceVoting.ToString();
        }

        private void defaultBinThresholdButton_Click(object sender, RoutedEventArgs e) {
            binThresholdTextBox.Text = Surface.BinarizeThresholdDefault.ToString();
        }

        private void binThresholdTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                binarizeThresholdBuffer = int.Parse(binThresholdTextBox.Text);

                binThresholdLabel.Content = null;
                binThresholdTextBox.Background = Brushes.White;
                if (binarizeThresholdBuffer <= 0) {
                    binThresholdLabel.Content = Properties.Strings.SurfaceBinThresholdMessage01;
                }
            } catch (Exception ex) { 
                if(ex.Message != null) {
                    binThresholdLabel.Content = ex.Message;
                }
                binThresholdTextBox.Background = Brushes.Pink;
            }
        }        
    }

}