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

using NagaraStage.Activities;
using NagaraStage.Parameter;

namespace NagaraStage.Ui {
    /// <summary>
    /// CoordControl.xaml の相互作用ロジック
    /// </summary>
    public partial class CoordControl : UserControl {
        private IMainWindow window;
        private ParameterManager parameterManager;

        public CoordControl(IMainWindow _window) {
            InitializeComponent();

            this.window = _window;
            this.parameterManager = window.ParameterManager;
        }
    }
}
