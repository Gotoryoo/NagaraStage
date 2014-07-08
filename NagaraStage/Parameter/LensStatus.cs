using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NagaraStage;

namespace NagaraStage {
    namespace Parameter {
        /// <summary>
        /// レンズの状態を示す型です．
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public class LensStatus {
            public int Magnification = 0;
            public Vector2 CcdResolution = new Vector2();
            public Vector2 ImageLength = new Vector2();
            public Vector2 LensOffset = new Vector2();
            public Vector2 SpiralShift = new Vector2();
            public double ZStep = 0;
            public double GridMarkSize = 0;
            public int LedParameter = 0;            
        }
    }
}