using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NagaraStage {
    namespace Parameter {
        /// <summary>
        /// 設定ファイル stage.ini の読み込み，書き出し，分析など設定ファイル及び
        /// 設定ファイルの値を管理するクラスです．
        /// </summary>
        /// <author email="hirokazu.online@gmail.com">Hirokazu Yokoyama</author>
        /// <date>2012-06-28</date>
        public class StageIniManager : IniFileHandler {

            private static string stageIniPath = null;
            private string path;
            private IniType initData;

            /// <summary>
            /// 設定ファイルから読み込んだ値を取得または設定します．
            /// <para>
            /// なお，このプロパティで値を設定しても設定ファイルは更新されません．
            /// 設定ファイルを更新するにはWriteStageIniメソッドを実行してください．
            /// </para>
            /// </summary>
            public IniType InitD {
                get { return initData; }
                set { initData = value; }
            }

            /// <summary>
            /// デフォルトの設定ファイルへのパス 
            /// </summary>
            public static string StageIniDefaultPath {
                get {
                    if (stageIniPath == null) {
                        string exeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        stageIniPath = Path.GetDirectoryName(exeFilePath) + "\\stage.ini";
                    }
                    return stageIniPath;
                }
            }

            /// <summary>
            /// エンコーダの分解能を取得します．
            /// </summary>
            public Vector3 EncoderResolution {
                get { return initData.EncoderResolution; }
            }

            /// <summary>
            /// モータの分解能を取得します．
            /// </summary>
            public Vector3 MotorResolution {
                get { return initData.MotorResolution; }
            }

            /// <summary>
            /// 初速度を取得します．
            /// </summary>
            public Vector3 MotorInitialVelocity {
                get { return initData.MotorInitialVelocity; }
            }

            /// <summary>
            /// モータの加速時間を取得します．
            /// </summary>
            public Vector3 MotorAccelTime {
                get { return initData.MotorAccelTime; }
            }

            /// <summary>
            /// 設定ファイルが持つMotorSpeed1を取得します．
            /// </summary>
            public Vector3 MotorSpeed1 {
                get { return initData.MotorSpeed1; }
            }

            /// <summary>
            /// 設定ファイルが持つMotorSpeed2を取得します．
            /// </summary>
            public Vector3 MotorSpeed2 {
                get { return initData.MotorSpeed2; }
            }

            /// <summary>
            /// 設定ファイルが持つMotorSpeed3を取得します．
            /// </summary>
            public Vector3 MotorSpeed3 {
                get { return initData.MotorSpeed3; }
            }

            /// <summary>
            /// 設定ファイルが持つMotorSpeed4を取得します．
            /// </summary>
            public Vector3 MotorSpeed4 {
                get { return initData.MotorSpeed4; }
            }

            /// <summary>
            /// 設定ファイルへのパスを取得します．
            /// </summary>
            public string StageIniPath {
                get { return path; }
            }

            /// <summary>コンストラクタ</summary>
            /// <param name="_path">設定ファイルへのパス</param>
            public StageIniManager(string _path) {
                this.path = _path;
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public StageIniManager() : this(StageIniDefaultPath) { }

            /// <summary>
            /// 設定ファイルを読み込み，その値を反映します．
            /// </summary>
            public void ReadStageIni() {
                StringBuilder sb = new StringBuilder(256);
                string defaultData;
                uint listSize;

                try {
                    // エンコーダ分解能の取得
                    defaultData = "0.001";
                    listSize = GetPrivateProfileString("ENCODER", "ENCX", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.EncoderResolution.X = checkIniData(listSize, sb, defaultData);

                    defaultData = "0.001";
                    listSize = GetPrivateProfileString("ENCODER", "ENCY", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.EncoderResolution.Y = checkIniData(listSize, sb, defaultData);

                    defaultData = "0.001";
                    listSize = GetPrivateProfileString("ENCODER", "ENCZ", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.EncoderResolution.Z = checkIniData(listSize, sb, defaultData);

                    // リミットスイッチ極性の取得
                    defaultData = "0xFC";
                    listSize = GetPrivateProfileString("LIMIT", "POL", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.LimitPol = (long)checkIniHexData(listSize, sb, defaultData);

                    // モータ分解能の取得
                    defaultData = "0.001";
                    listSize = GetPrivateProfileString("MOTOR", "RESOLVX", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.MotorResolution.X = checkIniData(listSize, sb, defaultData);

                    defaultData = "0.001";
                    listSize = GetPrivateProfileString("MOTOR", "RESOLVY", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.MotorResolution.Y = checkIniData(listSize, sb, defaultData);

                    defaultData = "0.001";
                    listSize = GetPrivateProfileString("MOTOR", "RESOLVZ", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.MotorResolution.Z = checkIniData(listSize, sb, defaultData);

                    defaultData = "1";
                    listSize = GetPrivateProfileString("MOTOR", "V0X", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.MotorInitialVelocity.X = checkIniData(listSize, sb, defaultData);

                    defaultData = "1";
                    listSize = GetPrivateProfileString("MOTOR", "V0Y", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.MotorInitialVelocity.Y = checkIniData(listSize, sb, defaultData);

                    defaultData = "1";
                    listSize = GetPrivateProfileString("MOTOR", "V0Z", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.MotorInitialVelocity.Z = checkIniData(listSize, sb, defaultData);

                    defaultData = "0.5";
                    listSize = GetPrivateProfileString("MOTOR", "ATX", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.MotorAccelTime.X = checkIniData(listSize, sb, defaultData);

                    defaultData = "0.5";
                    listSize = GetPrivateProfileString("MOTOR", "ATY", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.MotorAccelTime.Y = checkIniData(listSize, sb, defaultData);

                    defaultData = "0.5";
                    listSize = GetPrivateProfileString("MOTOR", "ATZ", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.MotorAccelTime.Z = checkIniData(listSize, sb, defaultData);

                    defaultData = "10";
                    listSize = GetPrivateProfileString("MOTOR", "SPEED1X", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.MotorSpeed1.X = checkIniData(listSize, sb, defaultData);

                    defaultData = "10";
                    listSize = GetPrivateProfileString("MOTOR", "SPEED1Y", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.MotorSpeed1.Y = checkIniData(listSize, sb, defaultData);

                    defaultData = "10";
                    listSize = GetPrivateProfileString("MOTOR", "SPEED1Z", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.MotorSpeed1.Z = checkIniData(listSize, sb, defaultData);

                    defaultData = "10";
                    listSize = GetPrivateProfileString("MOTOR", "SPEED2X", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.MotorSpeed2.X = checkIniData(listSize, sb, defaultData);

                    defaultData = "10";
                    listSize = GetPrivateProfileString("MOTOR", "SPEED2Y", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.MotorSpeed2.Y = checkIniData(listSize, sb, defaultData);

                    defaultData = "10";
                    listSize = GetPrivateProfileString("MOTOR", "SPEED2Z", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.MotorSpeed2.Z = checkIniData(listSize, sb, defaultData);

                    defaultData = "10";
                    listSize = GetPrivateProfileString("MOTOR", "SPEED3X", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.MotorSpeed3.X = checkIniData(listSize, sb, defaultData);

                    defaultData = "10";
                    listSize = GetPrivateProfileString("MOTOR", "SPEED3Y", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.MotorSpeed3.Y = checkIniData(listSize, sb, defaultData);

                    defaultData = "10";
                    listSize = GetPrivateProfileString("MOTOR", "SPEED3Z", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.MotorSpeed3.Z = checkIniData(listSize, sb, defaultData);

                    defaultData = "10";
                    listSize = GetPrivateProfileString("MOTOR", "SPEED4X", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.MotorSpeed4.X = checkIniData(listSize, sb, defaultData);

                    defaultData = "10";
                    listSize = GetPrivateProfileString("MOTOR", "SPEED4Y", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.MotorSpeed4.Y = checkIniData(listSize, sb, defaultData);

                    defaultData = "25";
                    listSize = GetPrivateProfileString("MOTOR", "SPEED4Z", defaultData, sb, (uint)sb.Capacity, @path);
                    initData.MotorSpeed4.Z = checkIniData(listSize, sb, defaultData);


                } catch (FileLoadException ex) {
                    System.Console.WriteLine("Error: " + path + "の読み込みに失敗しました.");
                    System.Console.WriteLine("Error: Failed loading " + path);
                    throw ex;
                } catch (FileNotFoundException ex) {
                    System.Console.WriteLine("Error: " + path + "が見つかりません.");
                    System.Console.WriteLine("Error: " + path + " is not found, orz...");
                    throw ex;
                } catch (Exception ex) {
                    System.Console.WriteLine("Error: " + path + "の読み込みに失敗しました.");
                    System.Console.WriteLine("Error: Failed loading " + path);
                    throw ex;
                }
            }

            /// <summary>設定ファイルを更新します．</summary>
            public void WriteStageIni() {
                // 現在の値を用いて設定ファイルを更新する．
                WriteStageIni(initData);
            }

            /// <summary>Writes the stage ini.</summary>
            /// <param name="_initData">設定ファイルに書き込む値</param>
            /// <see cref="WriteStageIni" />
            public void WriteStageIni(IniType _initData) {
                string val;

                try {
                    // エンコーダの分解能の書き込み
                    val = String.Format(initData.EncoderResolution.X.ToString(), "0.0000");
                    WritePrivateProfileString("ENCODER", "ENCX", val, @path);

                    val = String.Format(initData.EncoderResolution.Y.ToString(), "0.0000");
                    WritePrivateProfileString("ENCODER", "ENCY", val, @path);

                    val = String.Format(initData.EncoderResolution.Z.ToString(), "0.0000");
                    WritePrivateProfileString("ENCODER", "ENCZ", val, @path);

                    // リミットスイッチの極性
                    val = Convert.ToString(initData.LimitPol, 16);
                    WritePrivateProfileString("LIMIT", "POL", val, @path);

                    // モータ分解能の書き込み
                    val = String.Format(initData.MotorResolution.X.ToString(), "0.000000");
                    WritePrivateProfileString("MOTOR", "RESOLVX", val, @path);

                    val = String.Format(initData.MotorResolution.Y.ToString(), "0.000000");
                    WritePrivateProfileString("MOTOR", "RESOLVY", val, @path);

                    val = String.Format(initData.MotorResolution.Z.ToString(), "0.000000");
                    WritePrivateProfileString("MOTOR", "RESOLVZ", val, @path);

                    val = String.Format(initData.MotorInitialVelocity.X.ToString(), "0.0000");
                    WritePrivateProfileString("MOTOR", "V0X", val, @path);

                    val = String.Format(initData.MotorInitialVelocity.Y.ToString(), "0.0000");
                    WritePrivateProfileString("MOTOR", "V0Y", val, @path);

                    val = String.Format(initData.MotorInitialVelocity.Z.ToString(), "0.0000");
                    WritePrivateProfileString("MOTOR", "V0Z", val, @path);

                    val = String.Format(initData.MotorAccelTime.X.ToString(), "0.0000");
                    WritePrivateProfileString("MOTOR", "ATX", val, @path);

                    val = String.Format(initData.MotorAccelTime.Y.ToString(), "0.0000");
                    WritePrivateProfileString("MOTOR", "ATY", val, @path);

                    val = String.Format(initData.MotorAccelTime.Z.ToString(), "0.0000");
                    WritePrivateProfileString("MOTOR", "ATZ", val, @path);

                    val = String.Format(initData.MotorSpeed1.X.ToString(), "0.0000");
                    WritePrivateProfileString("MOTOR", "SPEED1X", val, @path);

                    val = String.Format(initData.MotorSpeed1.Y.ToString(), "0.0000");
                    WritePrivateProfileString("MOTOR", "SPEED1Y", val, @path);

                    val = String.Format(initData.MotorSpeed1.Z.ToString(), "0.0000");
                    WritePrivateProfileString("MOTOR", "SPEED1Z", val, @path);

                    val = String.Format(initData.MotorSpeed2.X.ToString(), "0.0000");
                    WritePrivateProfileString("MOTOR", "SPEED2X", val, @path);

                    val = String.Format(initData.MotorSpeed2.Y.ToString(), "0.0000");
                    WritePrivateProfileString("MOTOR", "SPEED2Y", val, @path);

                    val = String.Format(initData.MotorSpeed2.Z.ToString(), "0.0000");
                    WritePrivateProfileString("MOTOR", "SPEED2Z", val, @path);

                    val = String.Format(initData.MotorSpeed3.X.ToString(), "0.0000");
                    WritePrivateProfileString("MOTOR", "SPEED3X", val, @path);

                    val = String.Format(initData.MotorSpeed3.Y.ToString(), "0.0000");
                    WritePrivateProfileString("MOTOR", "SPEED3Y", val, @path);

                    val = String.Format(initData.MotorSpeed3.Z.ToString(), "0.0000");
                    WritePrivateProfileString("MOTOR", "SPEED3Z", val, @path);

                    val = String.Format(initData.MotorSpeed4.X.ToString(), "0.0000");
                    WritePrivateProfileString("MOTOR", "SPEED4X", val, @path);

                    val = String.Format(initData.MotorSpeed4.Y.ToString(), "0.0000");
                    WritePrivateProfileString("MOTOR", "SPEED4Y", val, @path);

                    val = String.Format(initData.MotorSpeed4.Z.ToString(), "0.0000");
                    WritePrivateProfileString("MOTOR", "SPEED4Z", val, @path);
                } catch (Exception) {
                    System.Console.WriteLine("Error: 設定ファイルの書き込みに失敗しました．");
                    System.Console.WriteLine("Error: Failed writing Configure File.");
                }
            }

            /// <summary>
            /// 設定ファイルの値が適切かどうかをチェックし, 適切でない場合は例外を返します．
            /// </summary>
            /// <exception cref="System.Exception"></exception>
            public void CheckStageIni() {

                if (initData.EncoderResolution.X == 0) {
                    throw new Exception("ENCX value is not correct.");
                }
                if (initData.EncoderResolution.Y == 0) {
                    throw new Exception("ENCY value is not correct");
                }
                if (initData.EncoderResolution.Z == 0) {
                    throw new Exception("ENCZ value is not correct");
                }

                if (initData.MotorResolution.X == 0) {
                    throw new Exception("RESOLVX value is not correct");
                }
                if (initData.MotorResolution.Y == 0) {
                    throw new Exception("RESOLVY value is not correct");
                }
                if (initData.MotorResolution.Z == 0) {
                    throw new Exception("RESOLVZ value is not correct");
                }

                if (initData.MotorAccelTime.X == 0) {
                    throw new Exception("ATX value is not correct");
                }
                if (initData.MotorAccelTime.Y == 0) {
                    throw new Exception("ATY value is not correct");
                }
                if (initData.MotorAccelTime.Z == 0) {
                    throw new Exception("ATZ value is not correct");
                }

                if (!isMotorSpeedCollect(initData.MotorSpeed1.X, initData.MotorResolution.X)) {
                    throw new Exception("SPEED1X value is not correct");
                }
                if (!isMotorSpeedCollect(initData.MotorSpeed1.Y, initData.MotorResolution.Y)) {
                    throw new Exception("SPEED1Y value is not correct");
                }
                if (!isMotorSpeedCollect(initData.MotorSpeed1.Z, initData.MotorResolution.Z)) {
                    throw new Exception("SPEED1Z value is not correct");
                }

                if (!isMotorSpeedCollect(initData.MotorSpeed2.X, initData.MotorResolution.X)) {
                    throw new Exception("SPEED2X value is not correct");
                }
                if (!isMotorSpeedCollect(initData.MotorSpeed2.Y, initData.MotorResolution.Y)) {
                    throw new Exception("SPEED2Y value is not correct");
                }
                if (!isMotorSpeedCollect(initData.MotorSpeed2.Z, initData.MotorResolution.Z)) {
                    throw new Exception("SPEED2Z value is not correct");
                }

                if (!isMotorSpeedCollect(initData.MotorSpeed3.X, initData.MotorResolution.X)) {
                    throw new Exception("SPEED3X value is not correct");
                }
                if (!isMotorSpeedCollect(initData.MotorSpeed3.Y, initData.MotorResolution.Y)) {
                    throw new Exception("SPEED3Y value is not correct");
                }
                if (!isMotorSpeedCollect(initData.MotorSpeed3.Z, initData.MotorResolution.Z)) {
                    throw new Exception("SPEED3Z value is not correct");
                }

                if (!isMotorSpeedCollect(initData.MotorSpeed4.X, initData.MotorResolution.X)) {
                    throw new Exception("SPEED4X value is not correct");
                }
                if (!isMotorSpeedCollect(initData.MotorSpeed4.Y, initData.MotorResolution.Y)) {
                    throw new Exception("SPEED4Y value is not correct");
                }
                if (!isMotorSpeedCollect(initData.MotorSpeed4.Z, initData.MotorResolution.Z)) {
                    throw new Exception("SPEED4Z value is not correct");
                }
            }

            /// <summary>
            /// モータの速度(MotorSpeed)が適切かどうかを判定し，適切な場合はtrue,
            /// 不適切な場合はfalseを返します．
            /// </summary>
            /// <param name="speed">モータの速度</param>
            /// <param name="motorResolution">モータの分解能</param>
            /// <returns>True: 適切, False: 不適切</returns>
            private Boolean isMotorSpeedCollect(double speed, double motorResolution) {
                return (speed < motorResolution || speed > 100 ? false : true);
            }

            /// <summary>
            /// INIファイルのキーに該当する値が適切な場合はINIファイルに設定されている
            /// 値を返し，そうでない場合は初期値を返します．
            /// </summary>
            /// <param name="listSize">INIファイルから取得した値の文字数</param>
            /// <param name="iniValue">INIファイルから取得した値が格納されているバッファ</param>
            /// <param name="defaultValue">初期値</param>
            /// <returns>INIファイルのキーに該当する値が適切な場合はINIファイルに設定されている
            /// 値を返し，そうでない場合は初期値</returns>
            private double checkIniData(uint listSize, StringBuilder iniValue, string defaultValue) {
                return (listSize <= 0 ? double.Parse(defaultValue) : double.Parse(iniValue.ToString()));
            }

            private int checkIniHexData(uint listSize, StringBuilder iniValue, string defaultValue) {
                int value = 0;
                string strValue = (listSize <= 0 ? defaultValue : iniValue.ToString());

                try {
                    value = Convert.ToInt32(strValue, 16);
                } catch (FormatException) {
                    strValue = strValue.Replace("&H", "0x");
                    value = Convert.ToInt32(strValue, 16);
                }

                return value;
            }
        }
    }
}