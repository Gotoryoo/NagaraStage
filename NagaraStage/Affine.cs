using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace NagaraStage {
    /// <summary>
    /// アフィン変換行列を定義します．
    /// a, b, p
    /// c, d, q
    /// </summary>
    public struct Affine {
        private double a, b, c, d, p, q;
        public double A { get { return a; } set { a = value; } }
        public double B { get { return b; } set { b = value; } }
        public double C { get { return c; } set { c = value; } }
        public double D { get { return d; } set { d = value; } }
        public double P { get { return p; } set { p = value; } }
        public double Q { get { return q; } set { q = value; } }

        public Affine(double _a = 1, double _b = 0, double _c = 0, double _d = 1, double _p = 0, double _q = 0) {            
            this.a = _a;
            this.b = _b;
            this.c = _c;
            this.d = _d;
            this.p = _p;
            this.q = _q;
        }

        public Affine(Affine src) {
            a = src.A;
            b = src.B;
            c = src.C;
            d = src.D;
            p = src.P;
            q = src.Q;
        }

        public static bool operator== (Affine y ,Affine z) {
            return (y.a == z.a && y.b == z.b && y.c == z.c && y.d == z.d && y.p == z.p && y.q == z.q);
        }

        public static bool operator !=(Affine y, Affine z) {
            return (y.a != z.a || y.b != z.b || y.c != z.c || y.d != z.d || y.p != z.p || y.q != z.q);
        }

        /// <summary>
        /// 与えられた座標点をアフィンパラメータに基づいて変換します．
        /// </summary>
        /// <param name="src">元の座標点</param>
        /// <returns>変換後の座標点</returns>
        public Vector2 Trance(Vector2 src) {
            Vector2 dst = new Vector2();
            dst.X = A * src.X + B * src.Y + P;
            dst.Y = C * src.X + D * src.Y + Q;
            return dst;
        }

        public Vector2 Trance(double x, double y) { 
            return Trance(new Vector2(x, y));
        }

        /// <summary>
        /// 逆アフィン変換パラメータを取得します．
        /// </summary>
        public Affine Inverse {
            get {
                Affine inv = new Affine();
                double det = A * D - B * C;
                if (det == 0.0) {
                    return inv;
                } else {
                    inv.A = D / det;
                    inv.B = -B / det;
                    inv.C = -C / det;
                    inv.D = A / det;
                    inv.P = (B * Q - P * D) / det;
                    inv.Q = -(A * Q - P * C) / det;
                    return inv;
                }
            }
        }

        /// <summary>
        /// 元の座標群(src)から変換後の座標群(dst)へ変換するためのアフィンパラメータを生成します．
        /// </summary>
        /// <param name="src">変換前の座標群</param>
        /// <param name="dst">変換後の座標群</param>
        /// <returns>変換パラメータ</returns>
        public static Affine CreateAffineBy(IList<Vector2> src, IList<Vector2> dst) {
            Affine ap = new Affine();
            List<double> lorgx = new List<double>(from o in src select o.X);
            List<double> lorgy = new List<double>(from o in src select o.Y);
            List<double> ldstx = new List<double>(from d in dst select d.X);
            List<double> ldsty = new List<double>(from d in dst select d.Y);
            double[] abp = solve(lorgx, lorgy, ldstx);
            ap.A = abp[0];
            ap.B = abp[1];
            ap.P = abp[2];

            double[] cdq = solve(lorgx, lorgy, ldsty);
            ap.C = cdq[0];
            ap.D = cdq[1];
            ap.Q = cdq[2];
            return ap;
        }

        private static double[] solve(IList<double> ox, IList<double> oy, IList<double> v) {
            var m = Matrix<double>.Build.Dense(3, 3, 0.0);
            var b = Vector<double>.Build.Dense(3, 0.0);

            for (int i = 0; i < ox.Count; i++) {
                m[0, 0] += ox[i] * ox[i];
                m[0, 1] += ox[i] * oy[i];
                m[0, 2] += ox[i];
                m[1, 0] += ox[i] * oy[i];
                m[1, 1] += oy[i] * oy[i];
                m[1, 2] += oy[i];
                m[2, 0] += ox[i];
                m[2, 1] += oy[i];
                m[2, 2] += 1.0;
                b[0] += v[i] * ox[i];
                b[1] += v[i] * oy[i];
                b[2] += v[i];
            }
            var x = m.Solve(b);

            double[] p = new double[3];
            p[0] = x[0];
            p[1] = x[1];
            p[2] = x[2];

            return p;
        }
    }
}
