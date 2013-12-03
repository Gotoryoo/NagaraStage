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
using NagaraStage.IO;
using NagaraStage.Parameter;

namespace NagaraStage {
    namespace Ui {
        /// <summary>
        /// TrackStageControl.xaml の相互作用ロジック
        /// </summary>
        public partial class TrackStageControl : UserControl {
            private IMainWindow window;
            private ParameterManager parameterManager;
            private Stage stage;
            private ComboBoxItem[] tracksItem;

            /// <summary>
            /// Initializes a new instance of the <see cref="TrackStageControl" /> class.
            /// </summary>
            /// <param name="_window">このコントロールを配置する親ウィンドウ</param>
            public TrackStageControl(IMainWindow _window) {
                InitializeComponent();
                this.window = _window;
                this.stage = window.WorkspaceContent as Stage;
                this.parameterManager = window.ParameterManager;
                this.trackInfoCanvas.ParameterManager = parameterManager;
            }

            /// <summary>
            /// allTracksButtonのドロップダウンメニューにトラックの一覧を追加して，初期化します．
            /// </summary>
            public void InitializeTracksItemMenu() {
                TracksManager tracksManager = parameterManager.TracksManager;

                // トラックデータが読み込まれていないなど，TracksManagerが初期化されていない場合は終了する．
                if (!tracksManager.IsInitialized) {
                    nextTrackButton.IsEnabled = false;
                    allTracksButton.IsEnabled = false;
                    return;
                }

                // トラックが存在しない場合はnextTrackButton及びallTracksButtonを無効にする．
                // トラックが一つのみの場合はallTracksButtonのみを無効にする．            
                if (tracksManager.NumOfTracks == 0) {
                    nextTrackButton.IsEnabled = false;
                }
                if (tracksManager.NumOfTracks <= 1) {
                    allTracksButton.IsEnabled = false;
                }

                // allTracksButtonのドロップダウンメニューの中身を初期化する
                tracksItem = new ComboBoxItem[tracksManager.NumOfTracks];
                for (int i = 0; i < tracksItem.Length; ++i) {
                    Track myTrack = tracksManager.GetTrack(i);
                    tracksItem[i] = new ComboBoxItem();
                    tracksItem[i].Content = myTrack.IdString;
                    tracksItem[i].HorizontalAlignment = HorizontalAlignment.Left;
                    tracksItem[i].Selected += delegate(object o, RoutedEventArgs e2) {
                        /* ComboBoxItemのClickイベントハンドラを追加 */
                        // 該当トラックの座標までモータ移動させる．
                        // ただし，モータが移動中の場合はその移動を中止させるかをユーザーに尋ねる．
                        MotorControler mc = MotorControler.GetInstance(parameterManager);
                        if (mc.IsMoving) {
                            if (askAbortMotorMoving()) {
                                mc.AbortMoving();
                                if (stage != null) {
                                    stage.WriteLine(Properties.Strings.MotorStop01);
                                }
                            } else {
                                // ユーザーがモータ動作を中止しない場合は終了
                                return;
                            }
                        }

                        // モータの移動動作
                        stage.WriteLine(Properties.Strings.Moving);
#if !NoHardware
                        mc.MovePointXY(myTrack.MsX, myTrack.MsY, delegate {
                            stage.WriteLine(Properties.Strings.MovingComplete);
                        });
#endif
                        // 追跡中のトラック番号を更新
                        tracksManager.TrackingIndex = myTrack.Index;

                        // 移動済みのメニュー項目は太字にするようにセット
                        ComboBoxItem thisItem = o as ComboBoxItem;
                        thisItem.FontWeight = FontWeights.Bold;
                        UpdateTrackInfo();
                    };
                    allTracksButton.Items.Add(tracksItem[i]);
                }
            }

            /// <summary>
            /// 現在，追跡中のTrackを描画，情報の更新を行います．
            /// </summary>
            public void UpdateTrackInfo() {
                TracksManager tracksManager = parameterManager.TracksManager;
                Track presentTrack = tracksManager.Track;
                trackInfoCanvas.Track = presentTrack;
                trackIdLabel.Content = presentTrack.IdString;
                trackCommentTextBox.Text = presentTrack.Comment;
            }

            private void nextTrackButton_Click(object sender, RoutedEventArgs e) {
                if (!parameterManager.TracksManager.IsInitialized) {
                    MessageBox.Show(Properties.Strings.TracksManagerException01);
                    return;
                }

                TracksManager tracksManager = parameterManager.TracksManager;
                if (tracksManager.TrackingIndex + 1 >= tracksManager.NumOfTracks) {
                    MessageBox.Show(Properties.Strings.TrackFoundComplete);
                    return;
                }
                tracksManager.UpdateTrack();
                UpdateTrackInfo();

                MotorControler mc = MotorControler.GetInstance();
                if (mc.IsMoving) {
                    if (askAbortMotorMoving()) {
                        mc.AbortMoving();
                        stage.WriteLine(Properties.Strings.AbortMotor);
                    } else {
                        return;
                    }
                }
                Track track = tracksManager.Track;
                stage.WriteLine(Properties.Strings.Moving);
#if !NoHardware
                mc.MovePointXY(track.MsX, track.MsX, delegate {
                    stage.WriteLine(Properties.Strings.MovingComplete);
                });
#endif
                tracksItem[tracksManager.TrackingIndex].FontWeight = FontWeights.Bold;
            }

            private void trackStageControl_Loaded(object sender, RoutedEventArgs e) {
                InitializeTracksItemMenu();
            }

            /// <summary>
            /// モータの移動処理を中止するかどうかをユーザーに尋ねます．
            /// </summary>
            /// <returns>中止させる場合はtrue, そうでない場合はfalse</returns>
            private Boolean askAbortMotorMoving() {
                MessageBoxResult r = MessageBox.Show(
                                Properties.Strings.MotorActive01,
                                Properties.Strings.Abort + "?",
                                MessageBoxButton.YesNo);
                return (r == MessageBoxResult.Yes);
            }
        }
    }
}