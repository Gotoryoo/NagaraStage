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
using Microsoft.Windows.Controls.Ribbon;

using NagaraStage;
using NagaraStage.IO;
using NagaraStage.Parameter;

namespace simple_stage_control
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        private ParameterManager parameterManager;
        
        public MainWindow()
        {
            InitializeComponent();

            // Insert code required on object creation below this point.
        }
       
      

        private void But_SnapShot_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void But_Up_Click(object sender, RoutedEventArgs e)
        {

        }

        private void But_Left_Click(object sender, RoutedEventArgs e)
        {

        }

        private void But_Right_Click(object sender, RoutedEventArgs e)
        {

        }

        private void But_Down_Click(object sender, RoutedEventArgs e)
        {

        }

        private void But_Z_Up_Click(object sender, RoutedEventArgs e)
        {

        }

        private void But_Z_Down_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Sli_LEDBrightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
            Lab_LEDBrightness.Content = e.NewValue.ToString("#,0");
            NagaraStage.IO.Led led = NagaraStage.IO.Led.GetInstance();
            led.DAout((int)Sli_LEDBrightness.Value,parameterManager);

          
        }


    }
}
