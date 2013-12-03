using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

using NagaraStage.Activities;

namespace NagaraStage.Logger {
    /// <summary>
    /// 表面認識のログを管理・保存するクラスです．
    /// </summary>
    public class SurfaceLogger {
        /// <summary>
        /// 出力するファイルパス，ファイル名を設定または取得します．
        /// </summary>
        public string FileName = "";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SurfaceLogger() { 
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="fileName">出力ファイルへのパス</param>
        public SurfaceLogger(string fileName) {
            this.FileName = fileName;
        }

        public void Save(SurfaceArgs args){
            XmlSerializer serializer = new XmlSerializer(typeof(SurfaceArgs));
            FileStream fs = new FileStream(FileName, FileMode.Create);
            serializer.Serialize(fs, args);
            fs.Close();
        }

#if _NoHardWare
        public static List<SurfaceEventArgs> CreateSurfEventArgs(int num = 1200) {
            if (num < 5) {
                throw new ArgumentException("Parameter: num must be more than 5.");
            }

            Random random = new Random();
            List<SurfaceEventArgs> list = new List<SurfaceEventArgs>();
            for (int m = 0; m < 5; ++m) {
                for (int i = 0; i < (int)(num / 5); ++i) {
                    SurfaceEventArgs arg = new SurfaceEventArgs();
                    int minVal = (m % 2 == 0 ? 0 : 200);
                    int maxVal = (m % 2 == 0 ? 10 : 800);
                    arg.Brightness = random.Next(minVal, maxVal);
                    arg.Distance = 0.0015 * (double)((num / 5) * m + i);
                    arg.Id = ((num / 5) * m + i);
                    arg.IsBoundary = (i == 0 && m > 0);
                    arg.IsInGel = (m % 2 == 0);
                    arg.ZValue = arg.Distance + 10;
                    arg.Second = (num / 5) * m + i;
                    list.Add(arg);
                }
            }
            return list;
        }
#endif

#if false
        /// <summary>
        /// 表面認識の結果をCSVファイルで保存します．
        /// </summary>
        /// <param name="list">出力するデータリスト</param>
        /// <param name="path">保存先</param>
        /// <param name="append">出力先のファイルが既存の場合に追書きする場合はtrue，上書きする場合はfalse</param>
        /// <param name="note">メモ</param>
        [Obsolete("XMLに移行予定")]
        public static void Save(List<SurfaceEventArgs> list, string path, bool append = false, string note =null) {            
            bool isNew = (!append || File.Exists(path));
            StreamWriter sw = new StreamWriter(path, append, Encoding.UTF8);
            if (note != null) {
                sw.WriteLine("# " + note);
            }
            if(isNew) {                
                sw.WriteLine(
                    string.Format("#{0}, {1}, {2}, {3}", 
                    Properties.Strings.SurfaceDistance,
                    Properties.Strings.Brightness,
                    Properties.Strings.Judge,
                    Properties.Strings.Boundary));
            }
            foreach(SurfaceEventArgs e in list ) {
                sw.WriteLine("{0}, {1}, {2}, {3}, {4}", e.Distance, e.Brightness, e.IsInGel, e.IsBoundary, e.Note);
            }

            // 目視した境界面と，プログラムが算出した境界面との差を記録する．
            double[] boundaryCaled = new double[4];
            double[] boundaryHumanEye = new double[4];
            int index = 0;
            foreach (SurfaceEventArgs surfEvent in list) {
                if (surfEvent.IsBoundary) {
                    boundaryCaled[index] = surfEvent.ZValue;
                    ++index;
                }
                if (surfEvent.Note.IndexOf(GetBoundaryNoteString(Properties.Strings.UpperGelTop)) >= 0) {
                    boundaryHumanEye[3] = surfEvent.ZValue;
                }

                if (surfEvent.Note.IndexOf(GetBoundaryNoteString(Properties.Strings.UpperGelBottom)) >= 0) {
                    boundaryHumanEye[2] = surfEvent.ZValue;
                }

                if (surfEvent.Note.IndexOf(GetBoundaryNoteString(Properties.Strings.LowerGelTop)) >= 0) {
                    boundaryHumanEye[1] = surfEvent.ZValue;
                }

                if (surfEvent.Note.IndexOf(GetBoundaryNoteString(Properties.Strings.LowerGelBottom)) >= 0) {
                    boundaryHumanEye[0] = surfEvent.ZValue;
                }
            }
            double diffUpTop = boundaryCaled[3] - boundaryHumanEye[3];
            double diffUpBottom = boundaryHumanEye[2] - boundaryCaled[2];
            double diffLowTop = boundaryCaled[1] - boundaryHumanEye[1];
            double diffLowBottom = boundaryHumanEye[0] - boundaryCaled[0];
            sw.WriteLine(string.Format("{0}, {1}", Properties.Strings.UpperGelTop, diffUpTop.ToString()));
            sw.WriteLine(string.Format("{0}, {1}", Properties.Strings.UpperGelBottom, diffUpBottom.ToString()));
            sw.WriteLine(string.Format("{0}, {1}", Properties.Strings.LowerGelTop, diffLowTop.ToString()));
            sw.WriteLine(string.Format("{0}, {1}", Properties.Strings.LowerGelBottom, diffLowBottom.ToString()));
            sw.Close();
        }
#endif
        public static string GetBoundaryNoteString(string typeStr) {
            return string.Format(" {0}-{1} ", Properties.Strings.Boundary, typeStr);
        }
    }
}
