using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NagaraStage.Parameter {
    /// <summary>
    /// エマルション，プレート及びレンズに関するパラメータ,及びモータ，カメラその他に関するパラメータを管理するクラスです．
    /// </summary>
    /// <author>
    /// Hirokazu Yokoyama
    /// </author>
    public class ParameterManager : IStageParameter, ILensParameter {

        private GridParameter gridParameter;
        private EmulsionParameter emulsionParameter;
        private TracksManager tracksManager;
        private ILensParameter lensParameter;

        private string userName;
        private string mailAddress;

        private DateTime startTime;

        private EmulsionIndex emulsionIndex = new EmulsionIndex();
        private EmulsionType emulsionType;

        private int plateNo = 0;
        private int moduleNo = 0;
        private Vector3[] indexes = new Vector3[4];
        private OpenInFileMode openedInFileMode = OpenInFileMode.None;

        /// <summary>エマルション画像のサイズ(pixcel)を取得します.</summary>
        public static readonly Size ImageResolution = new Size(512, 440);

        /// <summary>撮影している映像のフレーム更新間隔(msec)を取得します．</summary>
        public const int FrameIntervalMilliSec = 30;

        /// <summary>パラメータをユーザーインターフェイスに反映させる際の更新間隔(msec)を取得します</summary>
        public const int ParamtersIntervalMilliSec = 30;

        /// <summary>
        /// グリッドマーク探索・認識処理を行うときのレンズの倍率
        /// </summary>
        public const int LensMagnificationOfGridMarkSearch = 10;

        /// <summary>PlateNoが変更されたときのイベント</summary>
        public event EventHandler<EventArgs> PlateChanged;
        /// <summary>ModuleNoTypeが変更されたときのイベント</summary>
        public event EventHandler<EventArgs> ModuleChanged;
        /// <summary>LensTypeが変更されたときのイベント</summary>
        public event EventHandler<LensEventArgs> LensTypeChanged;
        /// <summary>EmulsionIndexUpが変更されたときのイベント</summary>
        public event EventHandler<EventArgs> EmulsionIndexUpChanged;
        /// <summary>EmulsionIndexDownが変更されたときのイベント</summary>
        public event EventHandler<EventArgs> EmulsionIndexDownChanged;
        /// <summary>EmulsionTypeが変更されたときのイベント</summary>
        public event EventHandler<EmulsionEventArgs> EmulsionTypeChanged;

        /// <summary>
        /// TracksManagerを取得，または設定します．
        /// </summary>
        public TracksManager TracksManager {
            get { return tracksManager; }
            set { tracksManager = value; }
        }

        /// <summary>
        /// EmulsionParameterを取得，または設定します．
        /// </summary>
        public EmulsionParameter EmulsionParameter {
            get { return emulsionParameter; }
            set { emulsionParameter = value; }
        }

        /// <summary>
        /// GridParameterを取得，または設定します．
        /// </summary>
        public GridParameter GridParameter {
            get { return gridParameter; }
            set { gridParameter = value; }
        }

        /// <summary>
        /// 作業者の氏名を取得，または設定します．
        /// </summary>
        public string UserName {
            get { return userName; }
            set { userName = value; }
        }

        /// <summary>
        /// 作業者の連絡先メールアドレスを取得，または設定します．
        /// </summary>
        public string MailAddress {
            get { return mailAddress; }
            set { mailAddress = value; }
        }

        /// <summary>
        /// ステージの開始時間を取得，または設定します．
        /// </summary>
        public DateTime StartTime {
            get { return startTime; }
            set { startTime = value; }
        }

        /// <summary>
        /// 観測するエマルションのプレート番号を取得または設定します．
        /// </summary>
        public int PlateNo {
            get { return plateNo; }
            set {
                if (value < 1) {
                    throw new ArgumentException(Properties.Strings.PlateNoInavilityException);
                }
                plateNo = value;
                Ipt.InitializeCoodination(1, plateNo);
                if (PlateChanged != null) {
                    PlateChanged(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// 現在使用しているモジュール番号を取得，または設定します．
        /// </summary>
        public int ModuleNo {
            get { return moduleNo; }
            set {
                if (!isModuleNoCollect(value)) {
                    throw new ArgumentException(Properties.Strings.ModuleNoOutOfRangeException);
                }
                moduleNo = value;
                if (ModuleChanged != null) {
                    ModuleChanged(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// エマルション上ゲルのインデックス値を取得,または設定します．
        /// </summary>
        /// <exception cref="ArgumentException">値が1より大きい場合</exception>
        public double EmulsionIndexUp {
            get { return emulsionIndex.Up; }
            set {
                if (value < 1.0) {
                    throw new ArgumentException(Properties.Strings.IndexTooSmallException);
                }
                emulsionIndex.Up = value;
                Properties.Settings.Default.IndexUp = value;
                Ipt.SetEmulsionIndexUp(value);
#if !_NoHardWare
                Ipt.InitializeIchiLib(
                (int)EmulsionType,
                CoordManager.NumStep,
                EmulsionIndexUp, EmulsionIndexDown,
                0, 0, 1
                );
#endif
                if (EmulsionIndexUpChanged != null) {
                    EmulsionIndexUpChanged(this, new EventArgs());
                }

            }
        }

        /// <summary>
        /// エマルション下ゲルのインデックス値を取得,または設定します．
        /// </summary>
        /// <exception cref="ArgumentException">値が1より大きい場合</exception>
        public double EmulsionIndexDown {
            get { return emulsionIndex.Down; }
            set {
                if (value < 1.0) {
                    throw new ArgumentException(Properties.Strings.IndexTooSmallException);
                }
                emulsionIndex.Down = value;
                Properties.Settings.Default.IndexDown = value;
                Ipt.SetEmulsionIndexDown(value);
#if !_NoHardWare
                Ipt.InitializeIchiLib(
                (int)EmulsionType,
                CoordManager.NumStep,
                EmulsionIndexUp, EmulsionIndexDown,
                0, 0, 1
                );
#endif
                if (EmulsionIndexDownChanged != null) {
                    EmulsionIndexDownChanged(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// エマルションのタイプを取得または設定します．
        /// </summary>
        public EmulsionType EmulsionType {
            get { return emulsionType; }
            set {
                EmulsionParameter ep = emulsionParameter;
                if (value == EmulsionType.ThinType) {
                    Ipt.SetEmulsionType(
                        (int)value, -ep.AutoScanAngleRange, ep.AutoScanAngleRange + 0.01, EmulsionParameter.AngleStepMin);
                } else if (value == EmulsionType.ThickType) {
                    Ipt.SetEmulsionType(
                        (int)value,
                        -EmulsionParameter.AngleRangeOfThick,
                        EmulsionParameter.AngleRangeOfThick + 0.01,
                        EmulsionParameter.AngleStepMin
                        );
                    IO.Driver.VP910.InitializeCamera(value);
                    Ipt.InitializeIchiLib(
                        (int)value,
                        CoordManager.NumStep,
                        EmulsionIndexUp,
                        EmulsionIndexDown,
                        0, 0, 1
                        );
                }
                emulsionType = value;
                if (EmulsionTypeChanged != null) {
                    EmulsionTypeChanged(this, new EmulsionEventArgs(value));
                }
            }
        }

        /// <summary>
        /// 読み込み済みのtrackファイルのモードを取得します．
        /// </summary>
        public OpenInFileMode OpenedInFileMode {
            get { return openedInFileMode; }
        }

        /// <summary>主走査解像度を取得します．</summary>
        public double CameraMainResolution {
            get { return lensParameter.CcdResolutionX; }
        }

        /// <summary>副走査解像度を取得します．</summary>
        public double CameraSubResolution {
            get { return lensParameter.CcdResolutionY; }
        }

        /// <summary>
        /// スキャンする範囲(mm)を取得，または設定します．
        /// </summary>
        public double ScanAreaXm {
            get { return Properties.Settings.Default.ScanAreaXm; }
            set { Properties.Settings.Default.ScanAreaXm = value; }
        }

        /// <summary>
        /// スキャンする範囲(mm)を取得，または設定します．
        /// </summary>
        public double ScanAreaXp {
            get { return Properties.Settings.Default.ScanAreaXp; }
            set { Properties.Settings.Default.ScanAreaXp = value; }
        }

        /// <summary>
        /// スキャンする範囲(mm)を取得，または設定します．
        /// </summary>
        public double ScanAreaYm {
            get { return Properties.Settings.Default.ScanAreaYm; }
            set { Properties.Settings.Default.ScanAreaYm = value; }
        }

        /// <summary>
        /// スキャンする範囲(mm)を取得，または設定します．
        /// </summary>
        public double ScanAreaYp {
            get { return Properties.Settings.Default.ScanAreaYp; }
            set { Properties.Settings.Default.ScanAreaYp = value; }
        }
         
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ParameterManager() {
            emulsionParameter = new EmulsionParameter(this);
            tracksManager = new TracksManager(this);
            gridParameter = new GridParameter(this);
        }        

        /// <summary>
        /// 各パラメータを読み込みます．
        /// </summary>
        public void Initialize() {           
            /* レンズパラメータの読み込み */
            // はじめにXMLの設定ファイルの読み込みを試みるが，XMLファイルの読み込み
            // ・解析に失敗した場合，従来のCSVファイルからの読み込みを試みる．
            try {
                LensParameter lensParamXml = new LensParameter();
                lensParamXml.Load();
                lensParameter = lensParamXml;
            } catch(Exception) {
                LensParameterCSV lensParamCsv = new LensParameterCSV();
                lensParamCsv.Load();
                ScanAreaXm = lensParamCsv.ScanAreaXm;
                ScanAreaXp = lensParamCsv.ScanAreaXp;
                ScanAreaYm = lensParamCsv.ScanAreaYm;
                ScanAreaYp = lensParamCsv.ScanAreaYp;
                lensParameter = lensParamCsv;                
            }
        }

        /// <summary>
        /// インスタンスをシャロウ(浅い)コピーを行います．
        /// </summary>
        /// <returns>インスタンスのクローン</returns>
        public ParameterManager Clone() {
            return MemberwiseClone() as ParameterManager;
        }


        /// <summary>
        /// Prediction, ScanData, Vertexを記録するファイルを開きます．
        /// 更に，出力先ファイルを設定します．
        /// </summary>
        /// <param name="mode">ファイルの種類</param>
        /// <param name="inputFilePath">入力元ファイルへのパス</param>
        /// <param name="outputFilePath">出力先ファイルへのパス(エラーの出力先)</param>
        /// <param name="append">
        /// 出力ファイルが存在していた場合にアペンドするかどうか，true=アペンド, false=上書き
        /// </param>
        public void OpenInFile(OpenInFileMode mode, string inputFilePath, string outputFilePath, Boolean append = true) {
            int returnCode = Ipt.OpenFile((int)mode, inputFilePath, outputFilePath, (append ? "a" : "w"));
            if (returnCode == -1) {
                throw new Exception(
                    string.Format(
                    "ipt.dll OpenFile is failed. input is {0}, output is {1}, mode is {2}",
                    inputFilePath,
                    outputFilePath,
                    mode.ToString()
                    ));
            }
            openedInFileMode = mode;
            tracksManager.Initialize();
        }



        /// <summary>
        /// モジュール番号が適正かどうかを判定します．
        /// </summary>
        /// <param name="no">モジュール番号</param>
        /// <returns>True: 適正である． False: 適正ではない．</returns>
        private Boolean isModuleNoCollect(int no) {
            Boolean flag = true;
            flag = (no < 1 ? false : flag);
            flag = (no > 100 ? false : flag);
            flag = (no >= 46 & no <= 50 ? false : flag);

            return flag;
        }

        /// <summary>
        /// ユーザーインターフェイスの言語を設定，または取得します．
        /// </summary>
        public string Language {
            get { return Properties.Settings.Default.Language; }
            set { }
        }

        /// <summary>
        /// エンコーダの分解能を取得，または設定します．
        /// </summary>
        public Vector3 EncoderResolution {
            get { 
                return new Vector3(
                    Properties.Settings.Default.EncoderResolutionX,
                    Properties.Settings.Default.EncoderResolutionY,
                    Properties.Settings.Default.EncoderResolutionZ);
            }
            set {
                Properties.Settings.Default.EncoderResolutionX = value.X;
                Properties.Settings.Default.EncoderResolutionY = value.Y;
                Properties.Settings.Default.EncoderResolutionZ = value.Z;
            }
        }

        /// <summary>
        /// モータの分解能を取得，または設定します．
        /// </summary>
        public Vector3 MotorResolution {
            get {
                return new Vector3(
                    Properties.Settings.Default.MotorResolutionX,
                    Properties.Settings.Default.MotorResolutionY,
                    Properties.Settings.Default.MotorResolutionZ
                    );
            }
            set { 
                Properties.Settings.Default.MotorResolutionX = value.X;
                Properties.Settings.Default.MotorResolutionY = value.Y;
                Properties.Settings.Default.MotorResolutionZ = value.Z;
            }
        }

        /// <summary>
        /// モータの初速度を取得，または設定します．
        /// </summary>
        public Vector3 MotorInitialVelocity {
            get {
                return new Vector3(
                    Properties.Settings.Default.InitialVelocityX,
                    Properties.Settings.Default.InitialVelocityY,
                    Properties.Settings.Default.InitialVelocityZ);
            }
            set {
                Properties.Settings.Default.InitialVelocityX = value.X;
                Properties.Settings.Default.InitialVelocityY = value.Y;
                Properties.Settings.Default.InitialVelocityZ = value.Z;
            }
        }

        /// <summary>
        /// モータの加速時間を取得，または設定します．
        /// </summary>
        public Vector3 MotorAccelTime {
            get {
                return new Vector3(
                  Properties.Settings.Default.AccelTimeX,
                  Properties.Settings.Default.AccelTimeY,
                  Properties.Settings.Default.AccelTimeZ);
            }
            set {
                Properties.Settings.Default.AccelTimeX = value.X;
                Properties.Settings.Default.AccelTimeY = value.Y;
                Properties.Settings.Default.AccelTimeZ = value.Z;
            }
        }

        /// <summary>
        /// モータ速度1の値を取得，また設定します．
        /// </summary>
        public Vector3 MotorSpeed1 {
            get { return new Vector3(
                    Properties.Settings.Default.Speed1X,
                    Properties.Settings.Default.Speed1Y,
                    Properties.Settings.Default.Speed1Z); 
            }
            set {
                Properties.Settings.Default.Speed1X = value.X;
                Properties.Settings.Default.Speed1Y = value.Y;
                Properties.Settings.Default.Speed1Z = value.Z;
            }
        }

        /// <summary>
        /// モータ速度2の値を取得，また設定します．
        /// </summary>
        public Vector3 MotorSpeed2 {
            get {
                return new Vector3(
                  Properties.Settings.Default.Speed2X,
                  Properties.Settings.Default.Speed2Y,
                  Properties.Settings.Default.Speed2Z);
            }
            set {
                Properties.Settings.Default.Speed2X = value.X;
                Properties.Settings.Default.Speed2Y = value.Y;
                Properties.Settings.Default.Speed2Z = value.Z;
            }
        }

        /// <summary>
        /// モータ速度3の値を取得，また設定します．
        /// </summary>
        public Vector3 MotorSpeed3 {
            get {
                return new Vector3(
                  Properties.Settings.Default.Speed3X,
                  Properties.Settings.Default.Speed3Y,
                  Properties.Settings.Default.Speed3Z);
            }
            set {
                Properties.Settings.Default.Speed3X = value.X;
                Properties.Settings.Default.Speed3Y = value.Y;
                Properties.Settings.Default.Speed3Z = value.Z;
            }
        }

        /// <summary>
        /// モータ速度4の値を取得，また設定します．
        /// </summary>
        public Vector3 MotorSpeed4 {
            get {
                return new Vector3(
                  Properties.Settings.Default.Speed4X,
                  Properties.Settings.Default.Speed4Y,
                  Properties.Settings.Default.Speed4Z);
            }
            set {
                Properties.Settings.Default.Speed4X = value.X;
                Properties.Settings.Default.Speed4Y = value.Y;
                Properties.Settings.Default.Speed4Z = value.Z;
            }
        }
        
        public double LimitPol {
            get { return Properties.Settings.Default.LimitPol; }
            set { Properties.Settings.Default.LimitPol = value; }
        }

        /// <summary>
        /// 現在使用中のレンズの倍率を取得，または設定します．
        /// ここで倍率を設定した場合，レンズに関わる他のプロパティも更新されます．
        /// </summary>
        public double Magnification {
            get { return lensParameter.Magnification; }
            set { 
                lensParameter.Magnification = value;
                if (LensTypeChanged != null) {
                    LensTypeChanged(this, new LensEventArgs(value));
                }
            }
        }

        /// <summary>
        /// X軸方向の画面解像度を取得します．
        /// </summary>
        public double CcdResolutionX {
            get { return lensParameter.CcdResolutionX; }
        }

        /// <summary>
        /// Y方向の解像度を取得します．
        /// </summary>
        public double CcdResolutionY {
            get { return lensParameter.CcdResolutionY; }
        }

        /// <summary>
        /// X方向の1ピクセルあたりの長さ(mm)を取得します．
        /// </summary>
        public double ImageLengthX {
            get { return lensParameter.ImageLengthX; }
        }

        /// <summary>
        /// Y方向の１ピクセルあたりの長さ(mm)を取得します．
        /// </summary>
        public double ImageLengthY {
            get { return lensParameter.ImageLengthY; }
        }

        /// <summary>
        /// X方向にらせん移動するときの移動単位を取得します．
        /// </summary>
        public double SpiralShiftX {
            get { return lensParameter.SpiralShiftX; }
        }

        /// <summary>
        /// Y方向にらせん移動するときの移動単位を取得します．
        /// </summary>
        public double SpiralShiftY {
            get { return lensParameter.SpiralShiftY; }
        }

        /// <summary>
        /// </summary>
        public double LensZStep {
            get { return lensParameter.LensZStep; }
        }

        /// <summary>
        /// グリッドマークの大きさを取得します．
        /// </summary>
        public double GridMarkSize {
            get { return lensParameter.GridMarkSize; }
        }

        /// <summary>
        /// X方向のオフセット値(mm)を取得します．
        /// </summary>
        public double LensOffsetX {
            get { return lensParameter.LensOffsetX; }
        }

        /// <summary>
        /// Y方向のオフセット値(mm)を取得します．
        /// </summary>
        public double LensOffsetY {
            get { return lensParameter.LensOffsetX; }
        }

        /// <summary>
        /// インスタンスが管理しているレンズのプロファイルの数を取得します．
        /// </summary>
        public int NumOfLens {
            get { return lensParameter.NumOfLens; }
        }

        /// <summary>
        /// レンズ毎の設定されたLEDの明るさ調整のための値を取得します．
        /// </summary>
        public int LedParameter {
            get { return lensParameter.LedParameter; }
        }

        /// <summary>
        /// 指定された倍率のレンズのX方向における解像度を取得します．
        /// </summary>
        /// <param name="lensType">レンズの倍率</param>
        /// <returns>
        /// X方向の解像度
        /// </returns>
        public double GetCcdResoulutionX(double lensType) {
            return lensParameter.GetCcdResoulutionX(lensType);
        }

        /// <summary>
        /// 指定された倍率のレンズのY方向における解像度を取得します．
        /// </summary>
        /// <param name="lensType">レンズの倍率</param>
        /// <returns>
        /// Y方向の解像度
        /// </returns>
        public double GetCcdResoulutionY(double lensType) {
            return lensParameter.GetCcdResoulutionY(lensType);
        }

        /// <summary>
        /// 指定された倍率のレンズのX方向における1ピクセルあたりの長さ(mm)を取得します．
        /// </summary>
        /// <param name="lensType">レンズの倍率</param>
        /// <returns>
        /// 1ピクセルあたり長さ(mm)
        /// </returns>
        public double GetImageLengthX(double lensType) {
            return lensParameter.GetImageLengthX(lensType);
        }

        /// <summary>
        /// 指定された倍率のレンズのY方向における1ピクセルあたりの長さ(mm)を取得します．
        /// </summary>
        /// <param name="lensType">レンズの倍率</param>
        /// <returns>
        /// 1ピクセルあたり長さ(mm)
        /// </returns>
        public double GetImageLengthY(double lensType) {
            return lensParameter.GetImageLengthY(lensType);
        }

        /// <summary>
        /// 指定された倍率のレンズのX方向におけるオフセット値を取得します．
        /// </summary>
        /// <param name="lensType">レンズの倍率</param>
        /// <returns>
        /// オフセット値
        /// </returns>
        public double GetLensOffsetX(double lensType) {
            return lensParameter.GetLensOffsetX(lensType);
        }

        /// <summary>
        /// 指定された倍率のレンズのY方向におけるオフセット値を取得します．
        /// </summary>
        /// <param name="lensType">レンズの倍率</param>
        /// <returns>
        /// オフセット値
        /// </returns>
        public double GetLensOffsetY(double lensType) {
            return lensParameter.GetLensOffsetY(lensType);
        }

        /// <summary>
        /// 指定された倍率のレンズのX方向におけるらせん移動の移動単位を取得します．
        /// </summary>
        /// <param name="lensType">レンズの倍率</param>
        /// <returns>
        /// 移動単位
        /// </returns>
        public double GetSpiralShiftX(double lensType) {
            return lensParameter.GetSpiralShiftX(lensType);
        }

        /// <summary>
        /// 指定された倍率のレンズのY方向におけるらせん移動の移動単位を取得します．
        /// </summary>
        /// <param name="lensType">レンズの倍率</param>
        /// <returns>
        /// 移動単位
        /// </returns>
        public double GetSpiralShiftY(double lensType) {
            return lensParameter.GetSpiralShiftY(lensType);
        }

        /// <summary>
        /// Gets the lens Z step.
        /// </summary>
        /// <param name="lensType">Type of the lens.</param>
        /// <returns></returns>
        public double GetLensZStep(double lensType) {
            return lensParameter.GetLensZStep(lensType);
        }

        /// <summary>
        /// 指定された倍率のレンズのグリッドマークの大きさを取得します．
        /// </summary>
        /// <param name="lensType">レンズの倍率.</param>
        /// <returns>
        /// グリッドマークの大きさ
        /// </returns>
        public double GetGridMarkSize(double lensType) {
            return lensParameter.GetGridMarkSize(lensType);
        }

        /// <summary>
        /// 指定された倍率のレンズのLEDの明るさ調整のための値を取得します．
        /// </summary>
        /// <param name="lensType">レンズの倍率</param>
        /// <returns>
        /// LEDのための値
        /// </returns>
        public int GetLedParameter(double lensType) {
            return lensParameter.GetLedParameter(lensType);
        }

        /// <summary>
        /// インスタンスが管理しているレンズの倍率一覧を取得します．
        /// </summary>
        /// <returns>
        /// レンズの倍率一覧
        /// </returns>
        public double[] GetLensMagList() {
            return lensParameter.GetLensMagList();
        }
    }

}