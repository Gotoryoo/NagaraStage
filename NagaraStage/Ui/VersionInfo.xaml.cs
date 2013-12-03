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

namespace NagaraStage {
    namespace Ui {
        /// <summary>
        /// バージョン情報を表示するためのコントロールです．
        /// VersionInfo.xaml の相互作用ロジック
        /// </summary>
        public partial class VersionInfo : Workspace, IDialogWorkspace {
            private IMainWindow window;

            /// <summary>ダイアログボックスの結果を取得します．．</summary>
            public MessageBoxResult Result {
                get { return MessageBoxResult.OK; }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="VersionInfo" /> class.
            /// </summary>
            /// <param name="_window">このコントロールを配置する親ウィンドウ</param>
            public VersionInfo(IMainWindow _window) : base(_window) {
                InitializeComponent();
                this.window = _window;
            }

            private void backButton_Click(object sender, RoutedEventArgs e) {
                Finish();
            }
          
        }
    }
}