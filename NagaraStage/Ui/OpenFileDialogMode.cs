using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Ui {
        /// <summary>
        /// OpenFileDialogの動作モードを定義する列挙体です．
        /// </summary>
        /// <author email="o1007410@edu.gifu-u.ac.jp">Hirokazu Yokoyama</author>
        public enum OpenFileDialogMode {
            /// <summary>
            /// モードを指定しません．通所のファイルを開くダイアログを取得します．
            /// </summary>
            None = 0,

            /// <summary>
            /// Grid Original Data File(*.gorg)ファイルを開くためのダイアログを取得します．
            /// </summary>
            GridOriginalDataFile = 1,

            /// <summary>
            /// Grid Fine Parameter File(*.gofs)ファイルを開くためのダイアログを取得します．
            /// </summary>
            GridFineParameterFile = 2,

            /// <summary>
            /// Grid Track output Data FIle(*.gkt)ファイルを開くためのダイアログを取得します．
            /// </summary>
            GridTrackOutputDataFile = 3,

            /// <summary>
            /// Grid Data File(*.grid)ファイルを開くためのダイアログを取得します．
            /// </summary>
            GridDataFile = 4,

            /// <summary>
            /// Prediction File(*.pred, *.err)ファイルを開くためのダイアログを取得します．
            /// </summary>
            PredictionFile = 5,

            /// <summary>
            /// スキャンモード(*.scan0, *.scan1)ファイルを開くためのダイアログを取得します．
            /// </summary>
            ScanDataFile = 6,

            /// <summary>
            /// Vertex Data File(*.vtx)ファイルを開くためのダイアログを取得します．
            /// </summary>
            VertexDataFile = 7,

            /// <summary>
            /// CSVファイルを開くためのダイアログを取得します．
            /// </summary>
            CsvFile = 8,

            /// <summary>
            /// XMLファイルを開くためのダイアログを取得します．
            /// </summary>
            XmlFile = 9,

            /// <summary>
            /// INIファイルを開くためのダイアログを取得します.
            /// </summary>
            IniFile = 10
        }
    }
}