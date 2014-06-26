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

namespace NagaraStage {
    namespace Ui {
        /// <summary>
        /// Trackの方角を描画するためのCanvasです．
        /// <para>TrackInfoCanvas.xaml の相互作用ロジック</para>
        /// </summary>
        public partial class TrackInfoCanvas : Canvas {
            private Track track;
            private Line trackLine = new Line();
            private Ellipse externalCircle;
            private Ellipse internalCircle;
            private ParameterManager parameterManager;

            public ParameterManager ParameterManager {
                set { this.parameterManager = value; }
            }

            /// <summary>
            /// 描画するTrackを取得，または設定します．
            /// <para>このプロパティに値を設定すると，Trackの描画処理を行います．</para>
            /// <para>このプロパティを設定する前にParameterManagerプロパティの値を設定する必要があります．</para>
            /// </summary>
            public Track Track {
                get { return track; }
                set {
                    if (parameterManager == null) {
                        throw new ArgumentException("ParameterManager is null.");
                    }

                    track = value;
                    trackLine.Stroke = Brushes.Black;
                    trackLine.StrokeThickness = 2;
                    trackLine.X1 = Width / 2;
                    trackLine.Y1 = Height / 2;
                    trackLine.X2 = (Width / 2) + ((track.MsDX * 0.07) / parameterManager.CameraMainResolution);
                    trackLine.Y2 = (Height / 2) - ((track.MsDY * 0.07) / parameterManager.CameraSubResolution);
                }
            }

            public new double Width {
                get { return base.Width; }
                set {
                    base.Width = value;
                    initialize();
                }
            }

            public new double Height {
                get { return base.Height; }
                set {
                    base.Height = value;
                    initialize();
                }
            }

            public TrackInfoCanvas() {
                InitializeComponent();
                Children.Add(trackLine);
            }

            /// <summary>
            /// Canvasに描画する２つの円を描画します．
            /// </summary>
            private void initialize() {

                // 外円を描画
                if (externalCircle != null) {
                    Children.Remove(externalCircle);
                }
                // 縦幅，横幅のうち，短い方の長さを採用する．
                double exLength = (Width < Height ? Width - 4 : Height - 4);
                externalCircle = new Ellipse();
                externalCircle.Fill = Brushes.Transparent;
                externalCircle.Stroke = Brushes.Yellow;
                externalCircle.StrokeThickness = 3;
                externalCircle.Width = exLength;
                externalCircle.Height = exLength;
                Canvas.SetLeft(externalCircle, (Width / 2) - (externalCircle.Width / 2));
                Canvas.SetTop(externalCircle, Height / 2 - externalCircle.Height / 2);
                Children.Add(externalCircle);

                // 内円を描画
                if (internalCircle != null) {
                    Children.Remove(internalCircle);
                }
                double inLength = exLength / 10;
                internalCircle = new Ellipse();
                internalCircle.Fill = Brushes.Transparent;
                internalCircle.Stroke = Brushes.Black;
                internalCircle.StrokeThickness = 2;
                internalCircle.Width = inLength;
                internalCircle.Height = inLength;
                Canvas.SetLeft(internalCircle, (Width / 2) - (internalCircle.Width / 2));
                Canvas.SetTop(internalCircle, Height / 2 - internalCircle.Height / 2);
                Children.Add(internalCircle);
            }

            private void Canvas_Loaded(object sender, RoutedEventArgs e) {
                initialize();
            }
        }
    }
}