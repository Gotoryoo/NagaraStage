using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NagaraStage {
    namespace Parameter {
        /// <summary>
        /// エマルションのパラメータを管理するクラスです．
        /// </summary>
        /// <author email="o1007410@edu.gifu-u.ac.jp">Hirokazu Yokoyama</author>
        public class EmulsionParameter : IFileParser {


            protected string _IniFile;
            protected double[] _BaseThick = new double[2];
            protected double[] _GelThick = new double[2];
            protected int[] _Brightness = new int[2];
            protected double _DistanceBundleEm;
            protected double _AutoScanAreaX, _AutoScanAreaY, _AutoScanAngleRange;
            protected int _ThresholdBrightnessHit;
            protected int _ThresholdBrightnessDust;
            protected int[] _ThresholdDust = new int[2];
            protected int[] _BaseThresholdTrack = new int[2];
            protected int[] _ThresholdSurface = new int[2];
            protected double _ThresholdGridBrightness;
            protected double _GridOriginalX0, _GridOriginalY0;
            protected double _GridOriginalSita, _GridOrigianlMag;
            protected double[,] _GridOriginalX = new double[4, 4];
            protected double[,] _GridOriginalY = new double[4, 4];


            private ParameterManager parameterManager;
#if false
            private EmParamType emulsion1To45 = new EmParamType();
            private EmParamType emulsion51To63 = new EmParamType();
            private EmParamType emulsion64To100 = new EmParamType();

            /// <summary>
            /// モジュール番号が1から45までで用いるパラメータが記載されたファイル
            /// </summary>
            public const string FileNameEmulsion1To45 = "emulsion_001-045.ini";

            /// <summary>
            /// モジュール番号が51から63までで用いるパラメータが記載されたファイル
            /// </summary>
            public const string FileNameEmulsion51To63 = "emulsion_051-063.ini";

            /// <summary>
            /// モジュール番号が64から100までで持ちるパラメータが記載されたファイル
            /// </summary>
            public const string FileNameEmulsion64To100 = "emulsion_064-100.ini";
#endif
            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="_parameterManger"></param>
            public EmulsionParameter(ParameterManager _parameterManger) {
                this.parameterManager = _parameterManger;
            }

            /// <summary>
            /// 最小値の角度(旧angle_step)
            /// </summary>
            public const double AngleStepMin = 0.03;

            /// <summary>
            /// 厚型エマルションにおける角度(旧angle_range2)
            /// </summary>
            public const double AngleRangeOfThick = 0.02;

            /// <summary>
            /// 現在使用しているエマルションのモジュール番号に適したGridOrgXの値を取得します．
            /// </summary>
            public double[,] GridOrgX {
                get { return _GridOriginalX; }
            }

            /// <summary>
            /// 現在使用しているエマルションのモジュール番号に適したGridOrgYの値を取得します．
            /// </summary>
            public double[,] GridOrgY {
                get { return _GridOriginalY; }
            }

            /// <summary>
            /// 現在使用しているエマルションのモジュール番号及びエマルションタイプに応じた明るさを取得します．
            /// </summary>
            public int BasicBrightness {
                get {
                    return (parameterManager.EmulsionType == EmulsionType.ThinType
                        ? _Brightness[0] : _Brightness[1]);
                }
            }

            /// <summary>
            /// 現在使用しているエマルションのベースの厚さを取得します．
            /// </summary>
            public double BaseThick {
                get {
                    return (parameterManager.EmulsionType == EmulsionType.ThinType
                        ? _BaseThick[0] : _BaseThick[1]);
                }
            }

            public double GelThick {
                get {
                    return (parameterManager.EmulsionType == EmulsionType.ThickType
                        ? _GelThick[0] : _GelThick[1]);
                }
            }

            public double DistanceBundle {
                get {
                    return _DistanceBundleEm;
                }
            }

            public double AutoScanAreaX {
                get {
                    return _AutoScanAreaX;
                }
            }

            public double AutoScanAreY {
                get {
                    return _AutoScanAreaY;
                }
            }

            public double AutoScanAngleRange {
                get {
                    return _AutoScanAngleRange;
                }
            }

            public int ThresholdBrightnessHit {
                get {
                    return _ThresholdBrightnessHit;
                }
            }

            public int ThresholdBrightnessDust {
                get {
                    return _ThresholdBrightnessDust;
                }
            }

            public int ThresholdDust {
                get {
                    return (parameterManager.EmulsionType == EmulsionType.ThickType
                        ? _ThresholdDust[0] : _ThresholdDust[1]);
                }
            }

            public int BaseThresholdTrack {
                get {
                    return (parameterManager.EmulsionType == EmulsionType.ThickType
                        ? _BaseThresholdTrack[0] : _BaseThresholdTrack[1]);
                }
            }

            public int ThresholdSurface {
                get {
                    return (parameterManager.EmulsionType == EmulsionType.ThickType
                        ? _ThresholdSurface[0] : _ThresholdSurface[1]);
                }
            }

            public double ThresholdGridBrightness {
                get {
                    return _ThresholdGridBrightness;
                }
            }

            public double GridOriginalX0 {
                get {
                    return _GridOriginalX0;
                }
            }

            public double GridOriginalY0 {
                get {
                    return _GridOriginalY0;
                }
            }

            public double GridOriginalSita {
                get {
                    return _GridOriginalSita;
                }
            }

            public double GridOriginalMag {
                get {
                    return _GridOrigianlMag;
                }
            }

#if false
            /// <summary>
            /// モジュール番号に応じたEmParamTypeのインスタンスを取得します．．
            /// </summary>
            /// <param name="moduleNo">モジュール番号</param>
            /// <returns>モジュール番号に応じたパラメータ</returns>
            private EmParamType getEmParam(int moduleNo) {
                EmParamType param = new EmParamType();
                if (moduleNo >= 1 & moduleNo <= 45) {
                    param = emulsion1To45;
                } else if (moduleNo >= 51 & moduleNo <= 63) {
                    param = emulsion51To63;
                } else if (moduleNo >= 64 & moduleNo <= 100) {
                    param = emulsion64To100;
                }
                return param;
            }
#endif
            /// <summary>
            /// ファイルから読み込んでエマルションのパラメータを初期化します．
            /// </summary>
            /// <param name="path">INIファイルへのパス</param>
            /// <exception cref="System.IO.FileNotFoundException"></exception>
            /// <exception cref="System.IO.FileLoadException"></exception>
            /// <exception cref="System.Exception"></exception>
            public void Load(string path) {
                string line;
                string[] args;

                try {
                    StreamReader sr = File.OpenText(path);
                    char[] delimiterChars = { ' ', '\t' };
                    _IniFile = path;
                    sr.ReadLine();
                    sr.ReadLine();
                    for (int i = 0; i < 2; ++i) {
                        line = sr.ReadLine();
                        args = line.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                        _BaseThick[i] = double.Parse(args[1]);
                        _GelThick[i] = double.Parse(args[2]);
                        _Brightness[i] = int.Parse(args[3]);
                    }
                    sr.ReadLine();
                    _DistanceBundleEm = double.Parse(sr.ReadLine());
                    sr.ReadLine();
                    line = sr.ReadLine();
                    args = line.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                    _AutoScanAreaX = double.Parse(args[0]);
                    _AutoScanAreaY = double.Parse(args[1]);
                    _AutoScanAngleRange = double.Parse(args[2]);
                    sr.ReadLine();
                    sr.ReadLine();
                    line = sr.ReadLine();
                    args = line.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                    _ThresholdBrightnessHit = int.Parse(args[0]);
                    _ThresholdBrightnessDust = int.Parse(args[1]);
                    _ThresholdDust[0] = int.Parse(args[2]);
                    _ThresholdDust[1] = int.Parse(args[3]);
                    _ThresholdSurface[0] = int.Parse(args[4]);
                    _ThresholdSurface[1] = int.Parse(args[5]);
                    _ThresholdGridBrightness = double.Parse(args[6]);
                    _BaseThresholdTrack[0] = int.Parse(args[7]);
                    _BaseThresholdTrack[1] = int.Parse(args[8]);
                    sr.ReadLine();
                    sr.ReadLine();
                    line = sr.ReadLine();
                    args = line.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                    _GridOriginalX0 = double.Parse(args[0]);
                    _GridOriginalY0 = double.Parse(args[1]);
                    Ipt.SetThreshold(_ThresholdBrightnessHit, _ThresholdBrightnessDust,
                        _ThresholdDust[0], _ThresholdDust[1]);

                    // グリッドマーク関係のパラメータを設定
                    double[] markx = new double[9];
                    double[] marky = new double[9];
                    double distance = 0;
                    _GridOriginalSita = 0;

                    for (int i = 1; i < 9; ++i) {
                        line = sr.ReadLine();
                        args = line.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                        markx[i] = double.Parse(args[0]) - _GridOriginalX0;
                        marky[i] = double.Parse(args[1]) - _GridOriginalY0;
                        distance += Math.Sqrt(markx[i] * markx[i] + marky[i] * marky[i]);
                        _GridOriginalSita += (Math.Abs(markx[i]) > 5 ?
                            Math.Atan(marky[i] / markx[i]) : -Math.Atan(markx[i] / marky[i]));
                    }

                    // Grid mark間の距離を200とする
                    _GridOrigianlMag = distance / (4 * 100 * (1 + 1.41421356));
                    _GridOriginalSita /= 8;

                    _GridOriginalX[1, 1] = 0;
                    _GridOriginalY[1, 1] = 0;
                    double cosGridOrgSita = Math.Cos(_GridOriginalSita);
                    double sinGridOrgSita = Math.Sign(_GridOriginalSita);
                    for (int i = 1; i < 9; ++i) {
                        double x = (markx[i] * cosGridOrgSita + marky[i] * sinGridOrgSita) / _GridOrigianlMag;
                        double y = (-markx[i] * sinGridOrgSita + marky[i] * cosGridOrgSita) / _GridOrigianlMag;
                        int ix = (int)((x + 100) / 100 + 0.5);
                        int iy = (int)((y + 100) / 100 + 0.5);
                        _GridOriginalX[ix, iy] = x;
                        _GridOriginalY[ix, iy] = y;
                    }
                    sr.Close();
                    _IniFile = path;
                } catch (Exception ex) {
                    throw ex;
                }
            }
#if false
            /// <summary>
            /// 全てのエマルションのパラメータをファイルに書き込みます．
            /// </summary>
            public void WriteEmulsionCondition() {
                try {
                    writeEmulsionCondition(FileNameEmulsion1To45, emulsion1To45);
                    writeEmulsionCondition(FileNameEmulsion51To63, emulsion51To63);
                    writeEmulsionCondition(FileNameEmulsion64To100, emulsion64To100);
                } catch (Exception ex) {
                    throw ex;
                }
            }
#endif
            /// <summary>
            /// エマルションのパラメータを実際にファイルに書き込みます．
            /// </summary>
            /// <param name="path">書き込むファイルへのパス</param>
            public void Save(string path) {
                StreamWriter sr = new StreamWriter(path);
                sr.WriteLine("# Emulsion Condition");
                sr.WriteLine("# type base-thick(mm) gel-thick(mm) brightness EMindexUpperGel EMindexLowerGel");
                for (int i = 0; i < 2; ++i) {
                    string line = string.Format("    0", i + 1)
                        + string.Format("    0.000", _BaseThick[i])
                        + string.Format("           0.000", _GelThick[i])
                        + string.Format("        ###", _Brightness[i])
                        + string.Format("       0.000", parameterManager.EmulsionIndexUp)
                        + string.Format("       0.000", parameterManager.EmulsionIndexDown);
                    sr.WriteLine(line);
                }
                sr.WriteLine("# Distance from Bundle to Emulsion");
                sr.WriteLine(_DistanceBundleEm.ToString("  0.00"));
                sr.WriteLine("# AutoScan are (x, y), angle range");
                sr.WriteLine(_AutoScanAreaX.ToString("  0.000")
                    + _AutoScanAreaY.ToString("  0.000")
                    + _AutoScanAngleRange.ToString("  0.000"));
                sr.WriteLine("# Threshold");
                sr.WriteLine("#  Br-hit#  Br-dust  Dust1  Dust2  Surface1  Surface2  Br-Grid  Track1  Track2");
                sr.WriteLine(_ThresholdBrightnessHit.ToString("     ###")
                    + _ThresholdBrightnessDust.ToString("     ###")
                    + _ThresholdDust[0].ToString("    ####")
                    + _ThresholdDust[1].ToString("    ####")
                    + _ThresholdSurface[0].ToString("    ####")
                    + _ThresholdSurface[1].ToString("    ####") + _ThresholdGridBrightness.ToString("       ##"));
                sr.Close();
            }
        }
    }
}