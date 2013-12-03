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

namespace GridMarkSearchTest {
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window {
        private ParameterManager parameterManager;
        private CoordManager subCoord;
        private CoordManager coord;
        private Ellipse[] ellipses = new Ellipse[CoordManager.AllGridMarksNum];

        public MainWindow() {
            InitializeComponent();
            parameterManager = new ParameterManager();
            subCoord = new CoordManager(parameterManager);
            coord = new CoordManager(parameterManager);
            for (int i = 0; i < ellipses.Length; ++i) {
                ellipses[i] = new Ellipse();
            }
        }

        public void SimurateGridMark(double x, double y, GridMarkPoint mark) {
            switch (mark) {
                case GridMarkPoint.CenterTop:
                    centerTopXSlider.Value = x;
                    centerTopYSlider.Value = y;
                    break;
                case GridMarkPoint.CenterMiddle:
                    centerMiddleXSlider.Value = x;
                    centerMiddleYSlider.Value = y;
                    break;
                case GridMarkPoint.CenterBottom:
                    centerBottomXSlider.Value = x;
                    centerBottomYSlider.Value = y;
                    break;
                case GridMarkPoint.LeftTop:
                    leftTopXSlider.Value = x;
                    leftTopYSlider.Value = y;
                    break;
                case GridMarkPoint.LeftMiddle:
                    leftMiddleXSlider.Value = x;
                    leftMiddleYSlider.Value = y;
                    break;
                case GridMarkPoint.LeftBottom:
                    leftBottomXSlider.Value = x;
                    leftBottomYSlider.Value = y;
                    break;
                case GridMarkPoint.RightTop:
                    rightTopXSlider.Value = x;
                    rightTopYSlider.Value = y;
                    break;
                case GridMarkPoint.RightMiddle:
                    rightMiddleXSlider.Value = x;
                    rightMiddleYSlider.Value = y;
                    break;
                case GridMarkPoint.RightBottom:
                    rightBottomXSlider.Value = x;
                    rightBottomYSlider.Value = y;
                    break;
            }
        }

        public void PredCoords() {
            GridMarkDefinitionUtil util = new GridMarkDefinitionUtil(coord);
            for (int i = 0; i < CoordManager.AllGridMarksNum; ++i) {
                GridMarkPoint mark = (GridMarkPoint)i;
                Vector2 position = util.GetGridMarkCoord(mark);
                if (coord.GetGridMark(mark).Existed) {                    
                    DrawCircle(ellipses[i], position.x, position.y, 4, Brushes.Blue);
                } else if(coord.DefinedGridMarkNum >= 2) {
                    SimurateGridMark(position.x, position.y, mark);
                }
            }
        }

        public Ellipse DrawCircle(Ellipse ellipse, double x, double y, double radius, Brush brush) {
            if (radius <= 0) { throw new ArgumentOutOfRangeException(); } 
            ellipse = (ellipse == null ? new Ellipse() : ellipse); 
            x *= 1.5; 
            y *= 1.5; 
            ellipse.Width = radius * 2; 
            ellipse.Height = radius * 2; 
            ellipse.Fill = brush; 
            double x0 = canvas1.ActualWidth / 2 + x; 
            double y0 = canvas1.ActualHeight / 2 - y; 
            Canvas.SetLeft(ellipse, x0 - radius); 
            Canvas.SetTop(ellipse, y0 - radius);
            if (!canvas1.Children.Contains(ellipse)) {
                canvas1.Children.Add(ellipse);
            }
            return ellipse;
        }

        public Ellipse DrawCircle(double x, double y, double radius, Brush brush) {
            return DrawCircle(new Ellipse(), x, y, radius, brush);
        }

        public void RemoveCanvasElement(Shape shape) {
            if (shape != null) {
                canvas1.Children.Remove(shape);
            }
        }

        public void Initialize() {
            leftTopGroup.IsEnabled = true;
            leftMiddleGroup.IsEnabled = true;
            leftBottomGroup.IsEnabled = true;
            centerTopGroup.IsEnabled = true;
            centerMiddleGroup.IsEnabled = true;
            centerBottomGroup.IsEnabled = true;
            rightTopGroup.IsEnabled = true;
            rightMiddleGroup.IsEnabled = true;
            rightBottomGroup.IsEnabled = true;
            leftTopXSlider.Value = -100;
            leftTopYSlider.Value = 100;
            leftMiddleXSlider.Value = -100;
            leftMiddleYSlider.Value = 0;
            leftBottomXSlider.Value = -100;
            leftBottomYSlider.Value = -100;
            centerTopXSlider.Value = 0;
            centerTopYSlider.Value = 100;
            centerMiddleXSlider.Value = 0;
            centerMiddleYSlider.Value = 0;
            centerBottomXSlider.Value = 0;
            centerBottomYSlider.Value = -100;
            rightTopXSlider.Value = 100;
            rightTopYSlider.Value = 100;
            rightMiddleXSlider.Value = 100;
            rightMiddleYSlider.Value = 0;
            rightBottomXSlider.Value = 100;
            rightBottomYSlider.Value = -100;
            coord.InitializeGridMarks();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            xAxis.X1 = 0;
            xAxis.X2 = canvas1.ActualWidth;
            xAxis.Y1 = canvas1.ActualHeight / 2;
            xAxis.Y2 = canvas1.ActualHeight / 2;
            yAxis.X1 = canvas1.ActualWidth / 2;
            yAxis.X2 = canvas1.ActualWidth / 2;
            yAxis.Y1 = 0;
            yAxis.Y2 = canvas1.ActualHeight;
            Initialize();
        }

        private void leftTopXSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            try {
                double x = leftTopXSlider.Value;
                double y = leftTopYSlider.Value;                
                leftTopXTextBox.Text = x.ToString();
                subCoord.SetGridMark(x, y, GridMarkPoint.LeftTop);
                DrawCircle(ellipses[(int)GridMarkPoint.LeftTop], x, y, 4, Brushes.Black);
                leftTopXTextBox.Background = Brushes.White;
            } catch(Exception){
            }
        }

        private void leftTopYSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            try {
                double x = leftTopXSlider.Value;
                double y = leftTopYSlider.Value;                
                leftTopYTextBox.Text = y.ToString();
                subCoord.SetGridMark(x, y, GridMarkPoint.LeftTop);
                DrawCircle(ellipses[(int)GridMarkPoint.LeftTop], x, y, 4, Brushes.Black);
                leftTopXTextBox.Background = Brushes.White;
            } catch (Exception) { 
            }
        }

        private void leftTopXTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                leftTopXSlider.Value = double.Parse(leftTopXTextBox.Text);
                leftTopXTextBox.Background = Brushes.White;
            } catch (Exception) {
                leftTopXTextBox.Background = Brushes.Pink;
            }
        }

        private void leftTopYTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                leftTopYSlider.Value = double.Parse(leftTopYTextBox.Text);
                leftTopYTextBox.Background = Brushes.White;
            } catch (Exception) {
                leftTopYTextBox.Background = Brushes.Pink;
            }
        }

        private void leftMiddleXSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            try {
                double x = leftMiddleXSlider.Value;
                double y = leftMiddleYSlider.Value;
                leftMiddleXTextBox.Text = x.ToString();
                subCoord.SetGridMark(x, y, GridMarkPoint.LeftMiddle);
                DrawCircle(ellipses[(int)GridMarkPoint.LeftMiddle], x, y, 4, Brushes.Black);
                leftMiddleXTextBox.Background = Brushes.White;
            } catch (Exception) {
            }
        }

        private void leftMiddleYSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            try {
                double x = leftMiddleXSlider.Value;
                double y = leftMiddleYSlider.Value;                
                leftMiddleYTextBox.Text = y.ToString();
                subCoord.SetGridMark(x, y, GridMarkPoint.LeftMiddle);
                DrawCircle(ellipses[(int)GridMarkPoint.LeftMiddle], x, y, 4, Brushes.Black);
                leftMiddleYTextBox.Background = Brushes.White;
            } catch (Exception) {
            }
        }

        private void leftMiddleXTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                leftMiddleXSlider.Value = double.Parse(leftMiddleXTextBox.Text);
                leftMiddleXTextBox.Background = Brushes.White;
            } catch (Exception) {
                leftMiddleXTextBox.Background = Brushes.Pink;
            }
        }

        private void leftMiddleYTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                leftMiddleYSlider.Value = double.Parse(leftMiddleYTextBox.Text);
                leftMiddleYTextBox.Background = Brushes.White;
            } catch (Exception) {
                leftMiddleYTextBox.Background = Brushes.Pink;
            }
        }

        private void leftBotomXSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            try {
                double x = leftBottomXSlider.Value;
                double y = leftBottomYSlider.Value;                
                leftBottomXTextBox.Text = x.ToString();
                subCoord.SetGridMark(x, y, GridMarkPoint.LeftBottom);
                DrawCircle(ellipses[(int)GridMarkPoint.LeftBottom], x, y, 4, Brushes.Black);
                leftBottomXTextBox.Background = Brushes.White;
            } catch (Exception) {
            }
        }

        private void leftBottomXTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                leftBottomXSlider.Value = double.Parse(leftBottomXTextBox.Text);
                leftBottomXTextBox.Background = Brushes.White;
            } catch (Exception) {
                leftBottomXTextBox.Background = Brushes.Pink;
            }
        }

        private void leftBottomYSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            try {
                double x = leftBottomXSlider.Value;
                double y = leftBottomYSlider.Value;                
                leftBottomYTextBox.Text = y.ToString();
                subCoord.SetGridMark(x, y, GridMarkPoint.LeftBottom);
                DrawCircle(ellipses[(int)GridMarkPoint.LeftBottom], x, y, 4, Brushes.Black);
                leftBottomYTextBox.Background = Brushes.White;
            } catch (Exception) {
            }
        }

        private void leftBottomYTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                leftBottomYSlider.Value = double.Parse(leftBottomYTextBox.Text);
                leftBottomYTextBox.Background = Brushes.White;
            } catch (Exception) {
                leftBottomYTextBox.Background = Brushes.Pink;
            }
        }

        private void centerTopXSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            try {
                double x = centerTopXSlider.Value;
                double y = centerTopYSlider.Value;
                centerTopXTextBox.Text = x.ToString();
                subCoord.SetGridMark(x, y, GridMarkPoint.CenterTop);
                DrawCircle(ellipses[(int)GridMarkPoint.CenterTop], x, y, 4, Brushes.Black);
                centerTopXTextBox.Background = Brushes.White;
            } catch (Exception) { 
            }
        }

        private void centerTopYSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            try {
                double x = centerTopXSlider.Value;
                double y = centerTopYSlider.Value;
                centerTopYTextBox.Text = y.ToString();
                subCoord.SetGridMark(x, y, GridMarkPoint.CenterTop);
                DrawCircle(ellipses[(int)GridMarkPoint.CenterTop], x, y, 4, Brushes.Black);
                centerTopXTextBox.Background = Brushes.White;
            } catch (Exception) { 
            }
        }

        private void centerTopXTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                centerTopXSlider.Value = double.Parse(centerTopXTextBox.Text);
                centerTopXTextBox.Background = Brushes.White;
            } catch (Exception) { 
                centerTopXTextBox.Background = Brushes.Pink;
            }
        }

        private void centerTopYTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                centerTopYSlider.Value = double.Parse(centerTopYTextBox.Text);
                centerTopYTextBox.Background = Brushes.White;
            } catch (Exception) { 
            }
        }

        private void centerMiddleXSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            try {
                double x = centerMiddleXSlider.Value;
                double y = centerMiddleYSlider.Value;
                centerMiddleXTextBox.Text = x.ToString();
                subCoord.SetGridMark(x, y, GridMarkPoint.CenterMiddle);
                DrawCircle(ellipses[(int)GridMarkPoint.CenterMiddle], x, y, 4, Brushes.Black);
                centerMiddleXTextBox.Background = Brushes.White;
            } catch (Exception) { 
            }
        }

        private void centerMiddleYSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            try {
                double x = centerMiddleXSlider.Value;
                double y = centerMiddleYSlider.Value;
                centerMiddleYTextBox.Text = y.ToString();
                subCoord.SetGridMark(x, y, GridMarkPoint.CenterMiddle);
                DrawCircle(ellipses[(int)GridMarkPoint.CenterMiddle], x, y, 4, Brushes.Black);
                centerMiddleXTextBox.Background = Brushes.White;           
            } catch (Exception) { 
            }
        }

        private void centerMiddleXTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                centerMiddleXSlider.Value = double.Parse(centerMiddleXTextBox.Text);
                centerMiddleXTextBox.Background = Brushes.White;
            } catch (Exception) { 
                centerMiddleXTextBox.Background = Brushes.Pink;
            }
        }

        private void centerMiddleYTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                centerMiddleYSlider.Value = double.Parse(centerMiddleYTextBox.Text);
                centerMiddleYTextBox.Background = Brushes.White;
            } catch (Exception) { 
                centerMiddleYTextBox.Background = Brushes.Pink;
            }
        }

        private void centerBottomXSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            try {
                double x = centerBottomXSlider.Value;
                double y = centerBottomYSlider.Value;
                centerBottomXTextBox.Text = x.ToString();
                subCoord.SetGridMark(x, y, GridMarkPoint.CenterBottom);
                DrawCircle(ellipses[(int)GridMarkPoint.CenterBottom], x, y, 4, Brushes.Black);
                centerBottomXTextBox.Background = Brushes.White;
            } catch (Exception) { 
            }
        }

        private void centerBottomYSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            try {
                double x = centerBottomXSlider.Value;
                double y = centerBottomYSlider.Value;
                centerBottomYTextBox.Text = y.ToString();
                subCoord.SetGridMark(x, y, GridMarkPoint.CenterBottom);
                DrawCircle(ellipses[(int)GridMarkPoint.CenterBottom], x, y, 4, Brushes.Black);
                centerBottomYTextBox.Background = Brushes.White;
            } catch (Exception) { 
            }
        }

        private void centerBottomXTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                centerBottomXSlider.Value = double.Parse(centerBottomXTextBox.Text);
                centerBottomXTextBox.Background = Brushes.White;
            } catch (Exception) { 
                centerBottomXTextBox.Background = Brushes.Pink;
            }
        }

        private void centerBottomYTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                centerBottomYSlider.Value = double.Parse(centerBottomYTextBox.Text);
                centerBottomYTextBox.Background = Brushes.White;
            } catch (Exception) { 
                centerBottomYTextBox.Background = Brushes.Pink;
            }
        }

        private void rightTopXSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            try {
                double x = rightTopXSlider.Value;
                double y = rightTopYSlider.Value;
                rightTopXTextBox.Text = x.ToString();
                subCoord.SetGridMark(x, y, GridMarkPoint.RightTop);
                DrawCircle(ellipses[(int)GridMarkPoint.RightTop], x, y, 4, Brushes.Black);
                rightTopXTextBox.Background = Brushes.White;
            } catch (Exception) { 
            }
        }

        private void rightTopYSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            try {
                double x = rightTopXSlider.Value;
                double y = rightTopYSlider.Value;
                rightTopYTextBox.Text = y.ToString();
                subCoord.SetGridMark(x, y, GridMarkPoint.RightTop);
                DrawCircle(ellipses[(int)GridMarkPoint.RightTop], x, y, 4, Brushes.Black);
                rightTopXTextBox.Background = Brushes.White;
            } catch (Exception) { 
            }
        }

        private void rightTopXTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                rightTopXSlider.Value = double.Parse(rightTopXTextBox.Text);
                rightTopXTextBox.Background = Brushes.White;
            } catch (Exception) { 
                rightTopXTextBox.Background = Brushes.Pink;
            }
        }

        private void rightTopYTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                rightTopYSlider.Value = double.Parse(rightTopYTextBox.Text);
                rightTopYTextBox.Background = Brushes.White;
            } catch (Exception) { 
                rightTopYTextBox.Background = Brushes.Pink;
            }
        }

        private void rightMiddleXSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            try {
                double x = rightMiddleXSlider.Value;
                double y = rightMiddleYSlider.Value;
                rightMiddleXTextBox.Text = x.ToString();
                subCoord.SetGridMark(x, y, GridMarkPoint.RightMiddle);
                DrawCircle(ellipses[(int)GridMarkPoint.RightMiddle], x, y, 4, Brushes.Black);
                rightMiddleXTextBox.Background = Brushes.White;
            } catch (Exception) { 
            }
        }

        private void rightMiddleYSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            try {
                double x = rightMiddleXSlider.Value;
                double y = rightMiddleYSlider.Value;
                rightMiddleYTextBox.Text = y.ToString();
                subCoord.SetGridMark(x, y, GridMarkPoint.RightMiddle);
                DrawCircle(ellipses[(int)GridMarkPoint.RightMiddle], x, y, 4, Brushes.Black);
                rightMiddleYTextBox.Background = Brushes.White;
            } catch (Exception) { 
            }
        }

        private void rightMiddleXTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                rightMiddleXSlider.Value = double.Parse(rightMiddleXTextBox.Text);
                rightMiddleXTextBox.Background = Brushes.White;
            } catch (Exception) { 
                rightMiddleXTextBox.Background = Brushes.Pink;
            }
        }

        private void rightMiddleYTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                rightMiddleYSlider.Value = double.Parse(rightMiddleYTextBox.Text);
                rightMiddleYTextBox.Background = Brushes.White;
            } catch (Exception) { 
                rightMiddleYTextBox.Background = Brushes.Pink;
            }
        }

        private void rightBottomXSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            try {
                double x = rightBottomXSlider.Value;
                double y = rightBottomYSlider.Value;
                rightBottomXTextBox.Text = x.ToString();
                subCoord.SetGridMark(x, y, GridMarkPoint.RightBottom);
                DrawCircle(ellipses[(int)GridMarkPoint.RightBottom], x, y, 4, Brushes.Black);
                rightBottomXTextBox.Background = Brushes.White;
            } catch (Exception) { 
            }
        }

        private void rightBottomYSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            try {
                double x = rightBottomXSlider.Value;
                double y = rightBottomYSlider.Value;
                rightBottomYTextBox.Text = y.ToString();
                subCoord.SetGridMark(x, y, GridMarkPoint.RightBottom);
                DrawCircle(ellipses[(int)GridMarkPoint.RightBottom], x, y, 4, Brushes.Black);
                rightBottomYTextBox.Background = Brushes.White;
            } catch (Exception) { 
            }
        }

        private void rightBottomXTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                rightBottomXSlider.Value = double.Parse(rightBottomXTextBox.Text);
                rightBottomXTextBox.Background = Brushes.White;
            } catch (Exception) { 
                rightBottomXTextBox.Background = Brushes.Pink;
            }
        }

        private void rightBottomYTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                rightBottomYSlider.Value = double.Parse(rightBottomYTextBox.Text);
                rightBottomYTextBox.Background = Brushes.White;
            } catch (Exception) { 
                rightBottomYTextBox.Background = Brushes.Pink;
            }
        }

        private void leftTopDecideButton_Click(object sender, RoutedEventArgs e) {
            double x = leftTopXSlider.Value;
            double y = leftTopYSlider.Value;
            coord.SetGridMark(x, y, GridMarkPoint.LeftTop);
            PredCoords();
            leftTopGroup.IsEnabled = false;
        }

        private void leftMiddleDecideButton_Click(object sender, RoutedEventArgs e) {
            double x = leftMiddleXSlider.Value;
            double y = leftMiddleYSlider.Value;
            coord.SetGridMark(x, y, GridMarkPoint.LeftMiddle);
            PredCoords();
            leftMiddleGroup.IsEnabled = false;
        }

        private void leftBottomDecideButton_Click(object sender, RoutedEventArgs e) {
            double x = leftBottomXSlider.Value;
            double y = leftBottomYSlider.Value;
            coord.SetGridMark(x, y, GridMarkPoint.LeftBottom);
            PredCoords();
            leftBottomGroup.IsEnabled = false;
        }

        private void centerTopDecideButton_Click(object sender, RoutedEventArgs e) {
            double x = centerTopXSlider.Value;
            double y = centerTopYSlider.Value;
            coord.SetGridMark(x, y, GridMarkPoint.CenterTop);
            PredCoords();
            centerTopGroup.IsEnabled = false;
        }

        private void centerMiddleDecideButton_Click(object sender, RoutedEventArgs e) {
            double x = centerMiddleXSlider.Value;
            double y = centerMiddleYSlider.Value;
            coord.SetGridMark(x, y, GridMarkPoint.CenterMiddle);
            PredCoords();
            centerMiddleGroup.IsEnabled = false;
        }

        private void centerBottomDecideButton_Click(object sender, RoutedEventArgs e) {
            double x = centerBottomXSlider.Value;
            double y = centerBottomYSlider.Value;
            coord.SetGridMark(x,y,GridMarkPoint.CenterBottom);
            PredCoords();
            centerBottomGroup.IsEnabled = false;
        }

        private void rightTopTextBox_Click(object sender, RoutedEventArgs e) {
            double x = rightTopXSlider.Value;
            double y = rightTopYSlider.Value;
            coord.SetGridMark(x, y, GridMarkPoint.RightTop);
            PredCoords();
            rightTopGroup.IsEnabled = false;
        }

        private void rightMiddleDecideButton_Click(object sender, RoutedEventArgs e) {
            double x = rightMiddleXSlider.Value;
            double y = rightMiddleYSlider.Value;
            coord.SetGridMark(x, y, GridMarkPoint.RightMiddle);
            PredCoords();
            rightMiddleGroup.IsEnabled = false;
        }

        private void rightBottomDecideButton_Click(object sender, RoutedEventArgs e) {
            double x = rightBottomXSlider.Value;
            double y = rightBottomYSlider.Value;
            coord.SetGridMark(x, y, GridMarkPoint.RightBottom);
            PredCoords();
            rightBottomGroup.IsEnabled = false;
        }

        private void resetButton_Click(object sender, RoutedEventArgs e) {
            Initialize();
        }





        
    }
}