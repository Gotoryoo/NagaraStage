using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Parameter {
        /// <summary>
        /// レンズの状態を示す型です．
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public struct LensStatus {
            public int Magnification;
            public Vector2 CcdResolution;
            public Vector2 ImageLength;
            public Vector2 LensOffset;
            public Vector2 SpiralShift;
            public double ZStep;
            public double GridMarkSize;
            public int LedParameter;
        }
    }
}