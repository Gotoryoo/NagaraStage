using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    /// <summary>
    /// Local Grid Coordinate Calibration with 3x3 grid mark
    /// </summary>
    /// <author>Hirokazu Yokoyama</author>
    public class GridMark {
        public double x = 0;
        public double y = 0;
        public Boolean Existed = false;
        public byte[] Image;
    }
}
