using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Ui {
        /// <summary>
        /// 個別に値を設定した，ファイルを開くダイアログを提供します．
        /// <para>詳細なリファレンスについてはMicrosoft.Win32.OpenFileDialogと
        /// あわせて御覧ください．</para>
        /// </summary>
        /// <author email="o1007410@edu.gifu-u.ac.jp">Hirokazu Yokoyama</author>
        public class OpenFileDialog {

            private Microsoft.Win32.OpenFileDialog dialog;
            private OpenFileDialogMode mode;
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

            public Boolean Multiselect {
                get { return dialog.Multiselect; }
                set { dialog.Multiselect = value; }
            }

            public Boolean RestoreDirectory {
                get { return dialog.RestoreDirectory; }
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="_mode">ダイアログのモード</param>
            public OpenFileDialog(OpenFileDialogMode _mode) {
                this.mode = _mode;
                this.dialog = new Microsoft.Win32.OpenFileDialog();

                // modeによりOpenFileDialogのプロパティなどを設定
                switch (this.mode) {
                    case OpenFileDialogMode.GridOriginalDataFile:
                        dialog.Title = "Read Grid Original Data";
                        dialog.Filter = "Grid Original Data FIle (*.gorg)|*.gorg";
                        dialog.DefaultExt = ".gorg";
                        dialog.RestoreDirectory = true;
                        break;
                    case OpenFileDialogMode.GridFineParameterFile:
                        dialog.FileName = "";
                        dialog.Filter = "Grid Fine Parameter File(*.gofs)|*.gofs";
                        dialog.Title = "Read Grid Fine Parameter";
                        dialog.DefaultExt = ".gofs";
                        break;
                    case OpenFileDialogMode.GridTrackOutputDataFile:
                        dialog.FileName = "";
                        dialog.Filter = "Grid Track output data file(*.gkt)|*.gtk";
                        dialog.DefaultExt = "*.gtk";
                        dialog.Title = "Open track data file";
                        break;
                    case OpenFileDialogMode.GridDataFile:
                        dialog.FileName = "";
                        dialog.Filter = "Grid Data File(*.grid)|*.grid";
                        dialog.DefaultExt = ".grid";
                        dialog.Title = "Read Grid Data";
                        break;
                    case OpenFileDialogMode.PredictionFile:
                        dialog.FileName = "";
                        dialog.Filter = "Prediction File(*.pred)|*.pred|(*.err)|*.err";
                        dialog.DefaultExt = ".pred";
                        break;
                    case OpenFileDialogMode.ScanDataFile:
                        dialog.FileName = "";
                        dialog.Filter = "Input Scan File(*.scan0)|*.scan0|(*.scan1)|*.scan1";
                        dialog.DefaultExt = "scan0";
                        break;
                    case OpenFileDialogMode.VertexDataFile:
                        dialog.FileName = "";
                        dialog.Filter = "Input Vertex File(*.vtx)|*.vtx|";
                        dialog.DefaultExt = ".vtx";
                        break;
                    case OpenFileDialogMode.IniFile:
                        dialog.FileName = "";
                        dialog.Filter = "INI File(*.ini)|*.ini|All Files(*.*)|*.*";
                        dialog.DefaultExt = "*.ini";
                        break;
                    case OpenFileDialogMode.CsvFile:
                        dialog.FileName = "";
                        dialog.Filter = "CSV File(*.csv)|*.csv|All Files(*.*)|*.*";
                        dialog.DefaultExt = "*.csv";
                        break;
                    case OpenFileDialogMode.XmlFile:
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