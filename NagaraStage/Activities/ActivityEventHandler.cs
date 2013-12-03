using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Activities {
        [Serializable]
        public delegate void ActivityEventHandler(object sender, ActivityEventArgs e);
    }
}
