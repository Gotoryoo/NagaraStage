using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Ui {
        /// <summary>
        /// SaveFileDialogの動作モードを定義した列挙対です．
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public enum SaveFileDialogMode {
            /// <summary>
            /// モードを指定しません．通所のファイルを開くダイアログを取得します．
            /// </summary>
            None = 0,

            /// <summary>
            /// Grid Track Output Data File(.gtk)ファイルを保存するためのダイアログを取得します．
            /// </summary>
            GridTrackOutputDataFile = 1,

            /// <summary>
            /// XMLファイルを保存するためのダイアログを取得します．
            /// </summary>
            XmlFile = 2,

            /// <summary>
            /// CSVファイルを保存するためのダイアログを取得します．
            /// </summary>
            CsvFile = 3,

            /// <summary>
            /// スキャンモード(*.scan0, *.scan1)ファイルを開くためのダイアログを取得します．
            /// </summary>
            ScanDataFile = 6,

            /// <summary>
            /// Vertex Data File(*.vtx)ファイルを開くためのダイアログを取得します．
            /// </summary>
            VertexDataFile = 7
        }
    }
}