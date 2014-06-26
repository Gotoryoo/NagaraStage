using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NagaraStage {
    namespace Parameter {
        /// <summary>
        /// グリッドマーク，グリッド関連のファイル(*.gofs, *.grid, *.gorg etc)の読み書き
        /// 及びそれらの値の管理を行うクラスです．
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public class GridParameter {

            public const int NFineGridX = 10;
            public const int NFineGridY = 10;
            public const int NFineGridXY2 = (NFineGridX + 1) + (NFineGridY + 1) + 1;
            public const int NFineGridNum = (NFineGridX + 1) * (NFineGridY + 1);
            public const int FineGridArea = 100;
            public const int FineGridArea2 = 110;
            public const int FineGridStep = 20;
            public const int FineGridStep2 = 10;

            private ParameterManager parameterManager;
            private Boolean definedBeamCo = false;
            private Boolean loadedGridOriginalFine = false;
            private Boolean definedGridFine = false;
            private Boolean definedGGDat = false;
            private Boolean loadedGrid = false;
            private double[,] gridOriginalFineX = new double[NFineGridXY2, NFineGridXY2];
            private double[,] gridOriginalFineY = new double[NFineGridXY2, NFineGridXY2];
            private double[,] gridOriginalPositionX = new double[NFineGridXY2, NFineGridXY2];
            private double[,] gridOriginalPositionY = new double[NFineGridXY2, NFineGridXY2];
            private double thisGelThickness = 0;

            public double[,] GridOriginalFineX {
                get {
                    if (!LoadedGridOriginalFine) {
                        throw new ArgumentNullException("*.gorg file did not still load;");
                    }
                    return gridOriginalFineX;
                }
            }

            public double[,] GridOriginalFineY {
                get {
                    if (!LoadedGridOriginalFine) {
                        throw new ArgumentNullException("*.gorg file did not still load;");
                    }
                    return gridOriginalFineY;
                }
            }

            public double[,] GridOriginalPositionX {
                get {
                    if (!LoadedGridOriginalFine) {
                        throw new ArgumentNullException("*.gorg file did not still load;");
                    }
                    return gridOriginalPositionX;
                }
            }

            public double[,] GridOriginalPositionY {
                get {
                    if (!LoadedGridOriginalFine) {
                        throw new ArgumentNullException("*.gorg file did not still load;");
                    }
                    return gridOriginalPositionY;
                }
            }

            /// <summary>
            /// (*.gofs)ファイルに記載されていたゲルの厚さを取得します．
            /// </summary>
            /// <exception cref="ArgumentNullException">*.gofsファイルが読み込まれていない場合</exception>
            public double ThisGelThickness {
                get {
                    if (!DefinedGridFine) {
                        throw new ArgumentNullException("*.gofs file did not still load.");
                    }
                    return thisGelThickness;
                }
            }

            /// <summary>
            /// GridOriginalFine(*.gorg)が読み込み済みであるかどうかを取得します．
            /// </summary>
            public Boolean LoadedGridOriginalFine {
                get { return loadedGridOriginalFine; }
            }

            /// <summary>
            /// Grid Fine Parameter(*.gofs)が読み込み済みであるかを取得します．
            /// </summary>
            public Boolean DefinedGridFine {
                get { return definedGridFine; }
            }

            public Boolean DefinedBeamCo {
                get { return definedBeamCo; }
            }

            public Boolean DefinedGGDat {
                get { return definedGGDat; }
            }

            public Boolean LoadedGrid {
                get { return loadedGrid; }
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public GridParameter(ParameterManager _parameterManager) {
                this.parameterManager = _parameterManager;
            }

            /// <summary>
            /// (*.gorg)ファイルを読み込みます．
            /// </summary>
            /// <param name="mode">The mode.</param>
            /// <param name="filePath">読み込むファイルへのパス</param>
            /// <exception cref="FileNotFoundException"></exception>
            /// <exception cref="FileLoadException"></exception>
            /// <exception cref="IOException"></exception>
            /// <exception cref="ArgumentException"></exception>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="FormatException"></exception>
            public void ReadGridOriginalFine(int mode, string filePath) {
                char[] delimiterChars = { ' ', '\t' };
                EmulsionParameter ep = parameterManager.EmulsionParameter;
                StreamReader sr = new StreamReader(filePath);
                sr.ReadLine();
                while (!sr.EndOfStream) {
                    string line = sr.ReadLine();
                    string[] args = line.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                    double x = double.Parse(args[0]) - ep.GridOriginalX0;
                    double y = double.Parse(args[1]) - ep.GridOriginalY0;
                    double xp = (x * Math.Cos(ep.GridOriginalSita) + y * Math.Sin(ep.GridOriginalSita)) / ep.GridOriginalMag;
                    double yp = (-x * Math.Sin(ep.GridOriginalSita) + y * Math.Cos(ep.GridOriginalSita)) / ep.GridOriginalMag;
                    switch (mode) {
                        case 0:
                        case 1:
                            int ix = (int)((x + FineGridArea) / (FineGridStep / 2) + 0.5);
                            int iy = (int)((y + FineGridArea) / (FineGridStep / 2) + 0.5);
                            if (ix % 2 == mode && iy % 2 == mode) {
                                ix = (ix - mode) / 2;
                                iy = (iy - mode) / 2;
                                if (ix >= 0 && ix <= NFineGridX && iy >= 0 && iy <= NFineGridY) {
                                    gridOriginalFineX[ix, iy] = xp;
                                    gridOriginalFineY[ix, iy] = yp;
                                    gridOriginalPositionX[ix, iy] = double.Parse(args[0]);
                                    gridOriginalPositionY[ix, iy] = double.Parse(args[1]);
                                }
                            }
                            break;
                        case 2:
                            ix = (int)((x + FineGridArea2) / FineGridStep2);
                            iy = (int)((y + FineGridArea2) / FineGridStep2);
                            if (ix >= 0 && ix <= NFineGridXY2 && iy >= 0 && iy <= NFineGridXY2) {
                                gridOriginalFineX[ix, iy] = xp;
                                gridOriginalFineY[ix, iy] = yp;
                                gridOriginalPositionX[ix, iy] = double.Parse(args[0]);
                                gridOriginalPositionY[ix, iy] = double.Parse(args[1]);
                            }
                            break;
                    }
                    loadedGridOriginalFine = true;
                }
                sr.Close();
            }

            /// <summary>
            /// (*.grid)ファイルを読み込みます
            /// </summary>
            /// <param name="filePath"></param>
            public void ReadGrid(string filePath) {
                if (!File.Exists(filePath)) {
                    throw new FileNotFoundException(string.Format("{0} is not found.", filePath));
                }

                int ret = Ipt.ReadWriteGridData("r", filePath);
                if (ret != 0) {
                    throw new FormatException(string.Format("Format of {0} is not correct", filePath));
                }

                loadedGrid = true;
                definedBeamCo = true;
                definedGGDat = true;
            }

            /// <summary>
            /// (*.gofs)ファイルを読み込みます．
            /// </summary>
            /// <param name="filePath">読み込むファイルへのパス</param>
            /// <param name="readAnyTime"></param>
            /// <exception cref="FormatException">読み込みに失敗した場合</exception>
            /// <exception cref="System.IO.FileNotFoundException">ファイルが存在しなかった場合</exception>
            public void ReadGridFineParameter(string filePath, Boolean readAnyTime) {
                if (!File.Exists(filePath)) {
                    throw new FileNotFoundException(filePath);
                }

                EmulsionParameter emParam = parameterManager.EmulsionParameter;
                double emulsionThickness = new double();
                int ret = Ipt.ReadWriteGridFine("r", filePath, ref emulsionThickness);
                if (ret == -1) {
                    throw new FormatException(string.Format("ThisGelThickness can not input from {0}", filePath));
                }

                thisGelThickness = (emulsionThickness - emParam.BaseThick) * 0.5;
                definedGridFine = true;
            }

            /// <summary>
            /// (*.gtk)ファイルを読み込みます．
            /// </summary>
            /// <param name="filePath">ファイルパス</param>
            /// <param name="plateNo">使用中のプレートの番号</param>
            public void ReadGridTrack(string filePath, int plateNo) {
                StreamReader sr = File.OpenText(filePath);
                char[] delimiterChars = { ' ', '\t' };
                int plateNp;

                string line = sr.ReadLine();
                string[] args = line.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                plateNp = int.Parse(args[1]);

                line = sr.ReadLine();
                args = line.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                int iGrid = int.Parse(args[1]);

                sr.Close();

                if (plateNp != plateNo - 1) {
                    throw new InvalidOperationException("Wrong plate data.");
                }

                if (iGrid != 1) {
                    throw new FormatException();
                }
            }

            /// <summary>
            /// (*.gtk)ファイルを書き込みます．
            /// </summary>
            /// <param name="filePath">ファイルパス</param>
            /// <param name="plateNo">使用中のプレートの番号</param>
            public void WriteGridTrack(string filePath, int plateNo) {
                string outputString = (char)34 + "Plate" + (char)34 + " "
                    + string.Format(plateNo.ToString(), "0");

                StreamWriter writer = new StreamWriter(filePath);
                writer.Write(outputString);
                writer.Close();

            }
        }
    }
}