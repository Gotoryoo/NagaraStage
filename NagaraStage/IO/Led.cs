using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.IO.Ports;

using NagaraStage;
using NagaraStage.Parameter;

namespace NagaraStage.IO {

    /// <summary>
    /// LEDコントロールのためのメソッドを提供するクラスです．
    /// </summary>
    /// <author>Hirokazu Yokoyama</author>
    [System.Runtime.InteropServices.GuidAttribute("BF152505-84E7-40DF-B00D-38ED8C93B4AB")]
    public class Led {
        private static Led instance;

        private Timer ledTimer;
        private SerialPort port;
        private int portNo;
        private double lastLightVoltage;

        private Boolean enabled = false;
        private Boolean timeOut = false;

        private double volBase;
        private int lastCCDmagni;

        /// <summary>ポート番号を記録したファイルへのパス</summary>
        public static readonly string LedParamFilePath = "led.prm";

        /// <summary>シリアル通信のボーレート</summary>
        public const int BaudRate = 9600;

        private readonly byte[] BrCode;

        public double LastLightVoltage {
            get { return lastLightVoltage; }
        }

        public string PortName {
            get { return port.PortName; }
        }

        /// <summary>
        /// LEDが有効であるかどうかを取得します．
        /// <para>True: 有効である, False: 有効ではない</para>
        /// </summary>
        public Boolean Enabled {
            get { return enabled; }
        }

        public Boolean TimeOut {
            get { return timeOut; }
            set { timeOut = value; }
        }

        /// <summary>
        /// コンストラクタ，ただし可視性はprivateです．
        /// </summary>        
        private Led() {
        }

        /// <summary>
        /// LedTimerの別スレッドで動くメソッドです
        /// </summary>        
        void ledTimer_Elapsed(object state) {
            timeOut = true;
            ledTimer.Dispose();
        }

        public static Led GetInstance() {
            if (instance == null) {
                instance = new Led();
            }

            return instance;
        }


        /// <summary>
        /// LEDポートを初期化します．
        /// </summary>
        public void Initiazlie() {
            enabled = false;
            // ポートのオープン
            port = new SerialPort();
            port.PortName = Properties.Settings.Default.LedParam;
            port.BaudRate = BaudRate;
            port.Parity = Parity.None;
            port.DataBits = 8;
            port.StopBits = StopBits.One;
            port.DtrEnable = true;
            port.RtsEnable = true;
            port.ReadBufferSize = 1024;
            port.WriteBufferSize = 512;
            port.Open();
            enabled = true;
        }
        

        public void Flush() {
            try {
                char[] dummy = new char[port.BytesToRead];
                port.Read(dummy, 0, port.BytesToRead);
                System.Console.WriteLine("dm:" + new string(dummy));
            } catch (InvalidOperationException) {
                port.Open();
            }            
        }

        public Boolean Command(string command) {
            Flush();
            System.Console.WriteLine("output:" + command + (char)13);
            port.Write(command + (char)13);

            return true;
        }

        public string Receive() {
            // このメソッドには途中でreturnする場合があります。

            TimerCallback callback = new TimerCallback(ledTimer_Elapsed);
            ledTimer = new Timer(callback, null, 1000, System.Threading.Timeout.Infinite);

            char[] replyBuffer = new char[1];
            string reply = "";
            timeOut = false;
            //port.ReadTimeout = 1000;

            do {
                //#if false
                do {
                    if (timeOut == true) {
                        Flush();
                        reply = "";
                        System.Console.WriteLine("led recieve timeout..");
                        return reply;
                    }
                } while (port.BytesToRead < 1);
                //#endif

                port.Read(replyBuffer, 0, 1);

                reply += new string(replyBuffer); ;

                if (reply.Substring(reply.Length - 1).ToCharArray()[0] == (char)13) {
                    break;
                }
            } while (true);
            System.Console.WriteLine("LED Recieve:" + reply);
            return reply;
        }

        public void SetTimer(int tick) {
            string command = "U " + String.Format(tick.ToString(), "####0");
            Command(command);
            Receive();
        }

        public Boolean PulsePower(int t, int l) {
            Boolean status = false;
            string reply = null;
            string command = null;

            command = "P 3 ";
            command = command + String.Format(t.ToString(), "##0") + " 0 1 ";
            command = command + String.Format(l.ToString(), "##0");
            Command(command);

            reply = Receive();
            status = (reply != "" ? true : false);

            return status;
        }

        public void OnPulse() {
            Command("E ON");
            Receive();
        }

        public void OffPulse() {
            Command("E OFF");
            Receive();
        }

        public Boolean SetDcPower(int level) {
            string command;
            string reply;

            command = "D " + string.Format(level.ToString(), "##0");
            Command(command);

            reply = Receive();

            return (reply == "" ? true : false);
        }

        /// <summary>
        /// LEDの明るさを自動調整します
        /// </summary>
        /// <param name="parameterManager">ParameterManagerのインスタンス</param>
        /// <returns>調整後の明るさ</returns>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.Exception"></exception>
        public int AdjustLight(ParameterManager parameterManager) {
            double xGrid = new double(), yGrid = new double();
            int brightness = 0;
            int brightness0 = parameterManager.EmulsionParameter.BasicBrightness;
            int lightStlength = 0, goodLightStlength = 0, goodDiffrence = 255;
            Camera camera = Camera.GetInstance();
            camera.Stop();
            try {

                for (int i = 7; i >= 0; --i) {
                    lightStlength += (brightness < brightness0 ? (int)Math.Pow(2, i) : -(int)Math.Pow(2, i));
                    DAout(lightStlength, parameterManager);
                    brightness = Ipt.GetAver();
                    if (Math.Abs(brightness - brightness0) < goodDiffrence) {
                        goodLightStlength = lightStlength;
                        goodDiffrence = Math.Abs(brightness - brightness0);
                    }
                }

                if (goodDiffrence > 5) {
                    MotorControler mc = MotorControler.GetInstance();
                    Vector3 stage = mc.GetPoint();
                    Ipt.MtoG(1, stage.X, stage.Y, ref xGrid, ref yGrid);
                    string error = string.Format("Failed to LightAdjust {0}, {1}", xGrid, yGrid);
                    ErrorLog.OutputError(error);
                }

                DAout(goodLightStlength, parameterManager);

            } catch (Exception ex) {
                throw ex;
            }
            camera.Action = null;
            camera.Start();
            return goodLightStlength;
        }


        /// <summary>
        /// 渡された値が適切かどうかを判定し，LEDの明るさをその値にします．
        /// <para>成功した場合はTrueを返し，そうでない場合はFalseを返します．</para>
        /// </summary>
        /// <param name="lightStrength">明るさ</param>
        /// <param name="parameterManager">ParameterManagerのインスタンス</param>
        /// <exception cref="System.ArgumentException">要求された明るさが不正な場合</exception>
        public void DAout(int lightStrength, ParameterManager parameterManager) {
            EmulsionType emulsionType = parameterManager.EmulsionType;
            int t = 0;

            if (!isLightStrengthCorrect(lightStrength)) {
                // 光の強さが不適切な値の場合は例外を返す
                throw new ArgumentException("Requested led light strength is not correct.");
            }
            // 光の強さ適切な場合
            if (emulsionType == EmulsionType.ThinType) {
                t = 3;
            } else if (emulsionType == EmulsionType.ThickType) {
                t = parameterManager.LedParameter;
            }

            PulsePower(t, lightStrength);
            lastLightVoltage = lightStrength;
        }

        /// <summary>
        /// LEDの光の強さが適切な値をとっているかを判定します．
        /// <para>True: 適切な値をとってる．　False: 適切な値をとっていない．</para>
        /// </summary>
        /// <param name="lightStrength">光の強さを示す値</param>
        /// <returns>True: 適切な値をとってる．　False: 適切な値をとっていない．</returns>
        private Boolean isLightStrengthCorrect(int lightStrength) {
            return (lightStrength < 0 || lightStrength > 255 ? false : true);
        }

        /// <summary>
        /// LEDのポート番号をファイルから読み込みます．
        /// <para>値はフィールド変数ledInfo.PortNoに代入されます．</para>
        /// </summary>
        /// <exception cref="System.Exception">読み込み失敗時</exception>
        ///  <returns>LEDで用いるポート名</returns>
        private string loadParam() {
            string fileName = LedParamFilePath;
            string portName;

            try {
                StreamReader streamReader = File.OpenText(fileName);
                string str = streamReader.ReadLine();
                portName = "COM" + str;
                portNo = int.Parse(str);
            } catch (FileLoadException) {
                throw new FileLoadException(fileName + "loading is failed.");
            } catch (FileNotFoundException) {
                throw new FileNotFoundException(fileName + "is not found.");
            } catch (Exception ex) {
                throw ex;
            }

            return portName;
        }

#if false
        /// <summary>
        /// LEDのポート番号をファイルに書き込みます．
        /// </summary>
        private void saveParam() {
            string fileName = LedParamFilePath;
            try {
                File.WriteAllText(fileName, portNo.ToString());
            } catch (Exception ex) {
                throw ex;
            }
        }
#endif
    }

}