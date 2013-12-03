using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Ui {
        /// <summary>
        /// 個別に値を設定shチア，ファイルを保存するダイアログを提供するクラスです．
        /// <para>詳細なリファレンスについてはSystem.Windows.Forms.OpenFileDialogと
        /// あわせて御覧ください．</para>
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public class SaveFileDialog {
            private Microsoft.Win32.SaveFileDialog dialog;
            private SaveFileDialogMode mode;

            public Boolean AddExtension {
                get { return dialog.AddExtension; }
                set { dialog.AddExtension = value; }
            }

            public string Title {
                get { return dialog.Title; }
            }

            public string FileName {
                get { return dialog.FileName; }
                set { dialog.FileName = value; }
            }

            public string[] FileNames {
                get { return dialog.FileNames; }
            }

            public string Filter {
                get { return dialog.Filter; }
            }

            public int FilterIndex {
                get { return dialog.FilterIndex; }
                set { dialog.FilterIndex = value; }
            }

            public string InitialDirectory {
                get { return dialog.InitialDirectory; }
                set { dialog.InitialDirectory = value; }
            }

            public Boolean RestoreDirectory {
                get { return dialog.RestoreDirectory; }
            }

            public SaveFileDialog(SaveFileDialogMode _mode) {
                this.mode = _mode;
                this.dialog = new Microsoft.Win32.SaveFileDialog();

                switch (mode) {
                    case SaveFileDialogMode.GridTrackOutputDataFile:
                        dialog.FileName = "";
                        dialog.Filter = "Grid Track output data file(*.gkt)|*.gtk";
                        dialog.DefaultExt = "*.gtk";
                        dialog.Title = "Open track data file";
                        break;
                    case SaveFileDialogMode.ScanDataFile:
                        dialog.FileName = "";
                        dialog.Filter = "Output Scan File(*scan0)|*.scan0|(*.scan1)|*.scan1";
                        dialog.DefaultExt = "scan0";
                        break;
                    case SaveFileDialogMode.VertexDataFile:
                        dialog.FileName = "";
                        dialog.Filter = "Input Vertex File(*.vtx)|*.vtx";
                        dialog.DefaultExt = ".vtx";
                        break;
                    case SaveFileDialogMode.CsvFile:
                        dialog.FileName = "";
                        dialog.Filter = "CSV File(*.csv)|*.csv|All Files(*.*)|*.*";
                        dialog.DefaultExt = "*.csv";
                        break;
                    case SaveFileDialogMode.XmlFile:
                        dialog.FileName = "";
                        dialog.Filter = "XML File(*.xml)|*.xml|All Files(*.*)|*.*";
                        dialog.DefaultExt = "*.xml";
                        break;
                }
            }

            /// <summary>
            /// ダイアログを表示します．
            /// </summary>
            /// <returns>ユーザーがダイアログ ボックスの [OK] をクリックした場合は
            /// DialogResult.OK。それ以外の場合は DialogResult.Cancel</returns>
            public Boolean ShowDialog() {
                return (Boolean)dialog.ShowDialog();
            }

            /// <summary>
            /// ダイアログを表示します．
            /// </summary>
            /// <param name="owner">親ウィンドウ</param>
            /// <returns>ユーザーがダイアログ ボックスの [OK] をクリックした場合は 
            /// DialogResult.OK。それ以外の場合は DialogResult.Cancel</returns>
            public Boolean ShowDialog(
                 System.Windows.Window owner) {
                return (Boolean)dialog.ShowDialog(owner);
            }

            /// <summary>
            /// ユーザーが指定したファイルを読み込み専用で開きます．
            /// <para>開くファイルはFileNameプロパティで指定します．</para>
            /// </summary>
            /// <returns>ユーザーが選択した読み取り専用ファイルを指定する Stream</returns>
            public System.IO.Stream OpenFile() {
                return dialog.OpenFile();
            }

            /// <summary>
            /// すべての値を初期化します．
            /// </summary>
            public void Reset() {
                dialog.Reset();
            }
        }
    }
}