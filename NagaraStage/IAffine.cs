using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    public interface IAffine {
        double A { get; set; }
        double B { get; set; }
        double C { get; set; }
        double D { get; set; }
        double P { get; set; }
        double Q { get; set; }
        Vector2 Trance(Vector2 src);
        Affine Inverse { get; }
    }
}
