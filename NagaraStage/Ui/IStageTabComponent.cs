using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Ui {
        public interface IStageTabComponent {
            /// <summary>
            /// タブページがアクティブになった時のイベントハンドラです．
            /// </summary>
            void EnteredHandler(object sender, EventArgs e);
        }
    }
}