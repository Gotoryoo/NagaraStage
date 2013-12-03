using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Parameter {
        /// <summary>
        /// Trackのプロファイル示すクラスです．
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public class Track {

            /// <summary>
            /// プレートの枚数
            /// </summary>
            public const int NumOfPlate = 12;

            private int index;
            private short[] id = new short[NumOfPlate];
            private short direction = 0;
            private double x = 0;
            private double y = 0;
            private double msx = 0;
            private double msy = 0;
            private double msdx = 0;
            private double msdy = 0;
            private double msTan = 0;
            private string comment = "";
            private Boolean scaned = false;

            /// <summary>
            /// このトラックがエマルション中の何番目のトラックであるかを取得します．
            /// </summary>
            public int Index {
                get { return index; }
            }

            /// <summary>
            /// トラックのIDを取得します．
            /// </summary>
            public short[] Id {
                get { return id; }
                set { id = value; }
            }

            /// <summary>
            /// トラックのIDを文字列型にして取得します．
            /// </summary>
            public string IdString {
                get {
                    string str = "";
                    for (int i = 0; i < id.Length; ++i) {
                        str += id[i].ToString() + (i != id.Length - 1 ? "-" : null);
                    }
                    return str;
                }
            }

            public short Direction {
                get { return direction; }
                set { direction = value; }
            }

            /// <summary>
            /// トラックがスキャン済みであるかどうかを取得または設定します．
            /// </summary>
            public Boolean Scaned {
                get { return scaned; }
                set { scaned = value; }
            }

            /// <summary>
            /// 旧gx0
            /// </summary>
            public double X {
                get { return x; }
                set { x = value; }
            }

            /// <summary>
            /// 旧gy0
            /// </summary>
            public double Y {
                get { return y; }
                set { y = value; }
            }

            public double MsX {
                get { return msx; }
                set { msx = value; }
            }

            public double MsY {
                get { return msy; }
                set { msy = value; }
            }

            public double MsDX {
                get { return msdx; }
                set { msdx = value; }
            }

            public double MsDY {
                get { return msdy; }
                set { msdy = value; }
            }

            public double MsTan {
                get { return msTan; }
                set { msTan = value; }
            }

            public string Comment {
                get { return comment; }
                set { comment = value; }
            }

            public Track(int _index, short[] _id, short _direction, double _x, double _y, double dx, double dy, string _comment = null) {
                this.index = _index;
                this.id = _id;
                this.direction = _direction;
                this.x = _x;
                this.y = _y;
                this.comment = _comment;
                Ipt.GToM("p", x, y, ref this.msx, ref this.msy);
                Ipt.GToM("a", dx, dy, ref this.msdx, ref this.msdy);
                this.msTan = Math.Sqrt(msdx * msdx + msdy * msdy);
            }
        }
    }
}