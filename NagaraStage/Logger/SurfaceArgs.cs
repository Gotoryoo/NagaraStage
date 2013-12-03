/**
 * @author Hirokazu Yokoyama
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using NagaraStage.Activities;

namespace NagaraStage.Logger {
    /// <summary>
    /// 表面認識の実行結果を保持するためのクラスです．
    /// </summary>
    public class SurfaceArgs {
        public int ModuleNo;
        public int PlateNo;
        public int BinarizeThreshold;
        public int BrightnessThreshold;
        public double MotorSpeed;
        public double Time;
        public List<Obj> EventList;

        public SurfaceArgs() {
            EventList = new List<Obj>();
        }

        public void SetEventList(List<SurfaceEventArgs> list) {
            foreach (SurfaceEventArgs arg in list) {
                Obj obj = new Obj();
                obj.Id = arg.Id;
                obj.Brightness = arg.Brightness;
                obj.IsInGel = arg.IsInGel;
                obj.Distance = arg.Distance;
                obj.IsBoundary = arg.IsBoundary;
                obj.Note = arg.Note;
                obj.Second = arg.Second;
                obj.ExceptionMessage = (arg.Exception == null ? "" : arg.Exception.Message);
                EventList.Add(obj);
            }
        }


        public class Obj : EventArgs {
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
            /// メモをします．
            /// </summary>
            [System.Xml.Serialization.XmlAttribute]
            public string Note;

            /// <summary>
            /// 最下点からの経過秒数を取得，または設定します．
            /// </summary>
            [System.Xml.Serialization.XmlAttribute]
            public double Second;

            [System.Xml.Serialization.XmlAttribute]
            public string ExceptionMessage;

            public Obj() {
                Note = "";
            }
        }
    }
}
