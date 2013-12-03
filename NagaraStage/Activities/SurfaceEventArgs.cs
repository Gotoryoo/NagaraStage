using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage.Activities {
    /// <summary>
    /// 表面認識のイベントデータを格納するクラスです．
    /// </summary>
    public class SurfaceEventArgs : ActivityEventArgs {
        /// <summary>
        /// イベントのID番号を取得，または設定します．
        /// </summary>
        [System.Xml.Serialization.XmlAttribute]
        public int Id;
        /// <summary>
        /// 表面認識の判定基準となっている輝度値を取得，または設定します．
        /// </summary>
        [System.Xml.Serialization.XmlAttribute]
        public int Brightness;
        /// <summary>
        /// ゲル内かどうかの判定結果を取得，または設定します．
        /// </summary>
        [System.Xml.Serialization.XmlAttribute]
        public bool IsInGel;
        /// <summary>
        /// 開始地点からの距離を取得，または設定します．
        /// </summary>
        [System.Xml.Serialization.XmlAttribute]
        public double Distance;
        /// <summary>
        /// ゲルの境界面であるかどうかを表します．
        /// </summary>
        [System.Xml.Serialization.XmlAttribute]
        public bool IsBoundary;

        /// <summary>
        /// 最下点からの経過秒数を取得，または設定します．
        /// </summary>
        [System.Xml.Serialization.XmlAnyAttribute]
        public double Second;

        /// <summary>
        /// メモをします．
        /// </summary>
        [System.Xml.Serialization.XmlAttribute]
        public string Note;

        public SurfaceEventArgs() {
            Note = "";
        }
    }
}
