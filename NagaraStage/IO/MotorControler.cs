using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NagaraStage;
using NagaraStage.Parameter;
using NagaraStage.IO.Driver;

namespace NagaraStage {
    namespace IO {
        /// <summary>
        /// aPCI-59モータを制御するクラスです．このクラスはモータ動作における一般的な動作を行うメソッドを提供し，
        /// それらのメソッドが下位のAPIにアクセスします．
        /// <para>
        /// このクラスはモータドライバを制御するためのAPI群を組み合わせて，
        /// モータ駆動を「指定した座標まで移動させる」などといった実用的な状態で使えるようにするための
        /// メソッドを持ちます．ただし，モータを移動させるだけのメソッドを持つだけで，移動させる座標値を算出するといった
        /// 機能は持ちませんし，今後とも持たせていはいけません．それはもっと上位なクラスが行うべきです．
        /// </para>
        /// </summary>
        /// <author email="o1007410@edu.gifu-u.ac.jp">Hirokazu Yokoyama</author>
        public class MotorControler : Driver.Apci59Constants {

            /// <summary>ThreadAbortExceptionが発生した時に行うメッソドのためのデリゲートです． </summary>        
            public delegate void AbortedCallback(ThreadAbortException ex);

            private static MotorControler instance;
            private ParameterManager parameterManager;
            private Vector3Int nowSpeed;
            private int limitPol0;
            private Vector3 tolerance = new Vector3();
            private Thread movingThread;
            //private Thread isMovingPointContinuemovingThread;
            /// <summary>らせん移動の位置を保持するカウンタ</summary>            
            private Vector2Int spiralCounter;
            /// <summary>らせん移動の中心座標</summary>
            private Vector3 spiralCentralPosition;
            /// <summary>
            /// らせん移動において，次の行き先を示すインデックスを取得，または設定します．
            /// <para>この値はらせん移動を行うと自動更新されます．通常はこの値を設定する必要ありません．
            /// ただし，らせん移動を中心からやり直す場合は0を設定してください．</para>
            /// </summary>
            public int SpiralIndex = 0;
            /// <summary>らせん移動をしたときのイベント</summary>
            public event EventHandler<SpiralEventArgs> SpiralMoved;
            /// <summary>目標座標値を保持するために一時的に用いられるメンバ変数</summary>
            private Vector3 purposePoint;

            // <summary>許容誤差を取得します．</summary>
            public Vector3 Tolerance {
                get { return tolerance; }
            }

            /// <summary>
            /// らせん移動した量を取得します．
            /// </summary>
            public Vector2Int SpiralCounter {
                get { return spiralCounter; }
            }

            /// <summary>
            /// 現在のモータ速度を取得します．
            /// モータの速度を設定するにはSetMotorSpeedメソッドを実行してください．
            /// </summary>
            public Vector3Int NowSpeed {
                get { return nowSpeed; }
            }

            /// <summary>
            /// 現在，モータが移動動作中かどうかを取得します．
            /// <para>true:移動中, false 停止中</para>
            /// <para>移動動作を強制停止するにはAbortMovingメソッドを実行してください．</para>
            /// </summary>
            public Boolean IsMoving {
                get { return (movingThread != null ? movingThread.IsAlive : false); }
            }

            /// <summary>
            /// コンストラクタ，可視性はプライベートです．
            /// <para>GetInsatnceメソッドを通して生成され，インスタンスが返されます．</para>
            /// </summary>
            /// <param name="_parameterManager">ソフトウェア全体を通して用いるParameterManagerのインスタンス</param>
            private MotorControler(ParameterManager _parameterManager) {
                this.nowSpeed = new Vector3Int(sp1, sp1, sp1);
                this.parameterManager = _parameterManager;
                setTolerance(parameterManager.EncoderResolution, parameterManager.MotorResolution);
            }

            /// <summary>
            /// インタンスを取得します，
            /// <para>MotorControlerクラスはコンストラクタを直接呼び出すことはできません．
            /// このメソッドを用いてインタンスを取得します．
            /// </para>
            /// <para>ただし，インスタンスはどれも同一のものとなり，インスタンスは複数個生成されません．</para>
            /// </summary>
            /// <returns>MotorControlerのインスタンス</returns>
            public static MotorControler GetInstance(ParameterManager _parameterManager) {
                if (instance == null) {
                    instance = new MotorControler(_parameterManager);
                }

                return instance;
            }

            /// <summary>
            /// インスタンスを取得します．
            /// このメソッドを初めて実行する場合は引数にIniTypeを与えてオーバーロードされたメソッドを実行してください．
            /// </summary>
            /// <returns>MotorControlerのインスタンス</returns>
            public static MotorControler GetInstance() {
                // このメソッドは途中でreturnします．

                if (instance == null) {
                    // インスタンスがない場合は例外を返す
                    throw new Exception("Instance of MotorControler is null.");
                } else {
                    return instance;
                }
            }

            /// <summary>
            /// I/Oドライバを初期化します
            /// </summary>
            /// <exception cref="System.Exception">モーター制御ボードの初期化に失敗した場合</exception>
            public void Initialize() {
#if !_NoHardWare
                if (!ApciM59.Initialize()) {
                    throw new Exception("Initializing aPCI-59 Motor Control Board is failed.");
                }
#endif
            }

            /// <summary>
            /// モータコントロールボードを初期化します．
            /// </summary>
            /// <param name="axisAddress">初期化する軸</param>
            public void InitializeMotorControlBoard(MechaAxisAddress axisAddress) {
                VectorId axis = convertToAxis(axisAddress);
                int rangeData;
                int speedData;
                int objectSpeedData;
                int rate1Data;
                Boolean status;
                limitPol0 = (int)(parameterManager.LimitPol != 0 ? parameterManager.LimitPol : limitPol0);

                // パルス出力方式(1パルス)，これでいいはず
                if (axisAddress == MechaAxisAddress.ZAddress) {
                    status = Converter.IODRV1_Outpb((int)axisAddress + MODE1_WRITE, 0x20);
                } else {
                    // X, Y軸は動きが逆
                    status = Converter.IODRV1_Outpb((int)axisAddress + MODE1_WRITE, 0x30);
                }

                // エンコーダ入力仕様(4てい倍)，DEAD,DERROR,リミット極性指定，これでいいはず
                status = Converter.IODRV1_Outpb((int)axisAddress + MODE2_WRITE, (byte)limitPol0);
                status = Converter.IODRV1_Outpb((int)axisAddress + COMMAND_WRITE, INPOSITION_WAIT_MODE_RESET);
                status = Converter.IODRV1_Outpb((int)axisAddress + COMMAND_WRITE, ALARM_STOP_ENABLE_MODE_RESET);
                status = Converter.IODRV1_Outpb((int)axisAddress + COMMAND_WRITE, INTERRUPT_OUT_ENABLE_MODE_RESET);

                // Range dataの設定
                rangeData = getRangeData(axis);
                status = Converter.IODRV1_Outpb((long)axisAddress + DATA3_WRITE, (byte)((rangeData / 256) & 0xFF));
                status = Converter.IODRV1_Outpb((long)axisAddress + DATA4_WRITE, (byte)(rangeData & 0xFF));
                status = Converter.IODRV1_Outpb((long)axisAddress + COMMAND_WRITE, RANGE_WRITE);

                // Start stop speed dataの作成
                speedData = getStartStopSpeedData(axis);
                status = Converter.IODRV1_Outpb((long)axisAddress + DATA3_WRITE, (byte)((speedData / 256) & 0xFF));
                status = Converter.IODRV1_Outpb((long)axisAddress + DATA4_WRITE, (byte)(speedData & 0xFF));
                status = Converter.IODRV1_Outpb((long)axisAddress + COMMAND_WRITE, START_STOP_SPEED_DATA_WRITE);

                // Object speed data の作成
                objectSpeedData = getObjectSpeedData(axis);
                status = Converter.IODRV1_Outpb((long)axisAddress + DATA3_WRITE, (byte)((objectSpeedData / 256) & 0xFF));
                status = Converter.IODRV1_Outpb((long)axisAddress + DATA4_WRITE, (byte)(objectSpeedData & 0xFF));
                status = Converter.IODRV1_Outpb((long)axisAddress + COMMAND_WRITE, OBJECT_SPEED_DATA_WRITE);

                // Rate1 data の作成
                rate1Data = getRate1Data(axis);
                status = Converter.IODRV1_Outpb((long)axisAddress + DATA3_WRITE, (byte)((rate1Data / 256) & 0xFF));
                status = Converter.IODRV1_Outpb((long)axisAddress + DATA4_WRITE, (byte)(rate1Data & 0xFF));
                status = Converter.IODRV1_Outpb((long)axisAddress + COMMAND_WRITE, RATE1_DATA_WRITE);

                // Rate2 data の作成
                status = Converter.IODRV1_Outpb((long)axisAddress + DATA3_WRITE, 0x1F);
                status = Converter.IODRV1_Outpb((long)axisAddress + DATA4_WRITE, 0xFF);
                status = Converter.IODRV1_Outpb((long)axisAddress + COMMAND_WRITE, RATE2_DATA_WRITE);

                // Rate3 data の作成
                status = Converter.IODRV1_Outpb((long)axisAddress + DATA3_WRITE, 0x1F);
                status = Converter.IODRV1_Outpb((long)axisAddress + DATA4_WRITE, 0xFF);
                status = Converter.IODRV1_Outpb((long)axisAddress + COMMAND_WRITE, RATE3_DATA_WRITE);

                status = Converter.IODRV1_Outpb((long)axisAddress + DATA3_WRITE, 0x1F);
                status = Converter.IODRV1_Outpb((long)axisAddress + DATA4_WRITE, 0xFF);
                status = Converter.IODRV1_Outpb((long)axisAddress + COMMAND_WRITE, RATE_CHANGE_POINT_1_2_WRITE);

                status = Converter.IODRV1_Outpb((long)axisAddress + DATA1_WRITE, 0x0);
                status = Converter.IODRV1_Outpb((long)axisAddress + DATA2_WRITE, 0x0);
                status = Converter.IODRV1_Outpb((long)axisAddress + DATA3_WRITE, 0x0);
                status = Converter.IODRV1_Outpb((long)axisAddress + DATA4_WRITE, 0x0);
                status = Converter.IODRV1_Outpb((long)axisAddress + COMMAND_WRITE, INTERNAL_COUNTER_WRITE);

                status = Converter.IODRV1_Outpb((long)axisAddress + DATA1_WRITE, 0x0);
                status = Converter.IODRV1_Outpb((long)axisAddress + DATA2_WRITE, 0x0);
                status = Converter.IODRV1_Outpb((long)axisAddress + DATA3_WRITE, 0x0);
                status = Converter.IODRV1_Outpb((long)axisAddress + DATA4_WRITE, 0x0);
                status = Converter.IODRV1_Outpb((long)axisAddress + COMMAND_WRITE, EXTERNAL_COUNTER_WRITE);

            }


            /// <summary>
            /// 現在位置を取得します．
            /// </summary>
            /// <returns>現在の座標値</returns>
            public Vector3 GetPoint() {
                Vector3Int c = new Vector3Int();
                string szBuf;
                long counter;
                Vector3 location = new Vector3();
                Vector3 encoderResolution = parameterManager.EncoderResolution;

#if !NoHardware
                // モータドライバから座標を取得する
                Apci59.DataFullRead(ApciM59.SlotNo, (short)VectorId.X, Apci59.EXTERNAL_COUNTER_READ, ref c.X);
                Apci59.DataFullRead(ApciM59.SlotNo, (short)VectorId.Y, Apci59.EXTERNAL_COUNTER_READ, ref c.Y);
                Apci59.DataFullRead(ApciM59.SlotNo, (short)VectorId.Z, Apci59.EXTERNAL_COUNTER_READ, ref c.Z);
#endif
                // X,Y,Z軸それぞれで符号拡張を行う．
                // 27bit目に符合情報が含まれているため27bit目が1のとき
                // 27bitより上位のビットをFに変換した値を用いる．
                // ただし，uint型とint型のキャストの関係上，
                // 一旦16bitの文字列型にキャストした後，再度intにキャストする．
                // この部分はSDKのサンプルコードを参考にした．
                if ((c.X & 0x8000000) != 0) {
                    counter = c.X;
                    counter = counter | 0xF0000000;
                    szBuf = String.Format("{0:x8}", counter);
                    c.X = Convert.ToInt32(szBuf, 16);
                }

                if ((c.Y & 0x8000000) != 0) {
                    counter = c.Y;
                    counter = counter | 0xF0000000;
                    szBuf = String.Format("{0:x8}", counter);
                    c.Y = Convert.ToInt32(szBuf, 16);
                }

                if ((c.Z & 0x8000000) != 0) {
                    counter = c.Z;
                    counter = counter | 0xF0000000;
                    szBuf = String.Format("{0:x8}", counter);
                    c.Z = Convert.ToInt32(szBuf, 16);
                }

                // モータドライバから取得した座標値にエンコーダ分解能を乗算して距離に変換する
                location.X = c.X * encoderResolution.X;
                location.Y = c.Y * encoderResolution.Y;
                location.Z = c.Z * encoderResolution.Z;

                return location;
            }

            /// <summary>
            /// モータの異常状態を検出します．
            /// </summary>
            /// <param name="axis">検出する軸</param>
            /// <param name="isConfCheck">設定値の異常もチェックするかどうか</param>
            /// <returns>異常状態</returns>
            public MotorAbnomalState GetAbnomalState(MechaAxisAddress axis, Boolean isConfCheck = false) {
                MotorAbnomalState motorState = MotorAbnomalState.NoProblem;
                byte returnValue = new byte();
                Apci59.GetMechanicalSignal(ApciM59.SlotNo, (short)convertToAxis(axis), ref returnValue);
                //byte returnValue = Converter.IODRV1_INpb((short)(axis + END_STATUS_READ));

                // 設定値の異常
                if (isConfCheck) {
                    if ((returnValue & (byte)MotorAbnomalState.ConfiguredValueNotCorrect) != 0x0) {
                        motorState |= MotorAbnomalState.ConfiguredValueNotCorrect;
                    }
                }

                // オーバーヒート
                if ((returnValue & (byte)MotorAbnomalState.OverHeat) != 0x0) {
                    motorState |= MotorAbnomalState.OverHeat;
                }

                // マイナス方向に軸の限界
                if ((returnValue & (byte)MotorAbnomalState.AxisLimitMinus) != 0x0) {
                    motorState |= MotorAbnomalState.AxisLimitMinus;
                }

                // プラス方向に軸の限界
                if ((returnValue & (byte)MotorAbnomalState.AxisLimitPlus) != 0x0) {
                    motorState |= MotorAbnomalState.AxisLimitPlus;
                }

                return motorState;
            }

            /// <summary>
            /// 現在のモータの速度を設定します．
            /// </summary>
            /// <param name="speed">モータの速度</param>
            public void SetMotorSpeed(MotorSpeed speed) {
                nowSpeed.X = (int)speed;
                nowSpeed.Y = (int)speed;
                nowSpeed.Z = (int)speed;
            }

            /// <summary>
            /// 現在のモータの速度を設定します．
            /// </summary>
            /// <param name="speedX">X軸方向のスピード</param>
            /// <param name="speedY">Y軸方向のスピード</param>
            /// <param name="speedZ">Z軸方向のスピード</param>
            public void SetMotorSpeed(MotorSpeed speedX, MotorSpeed speedY, MotorSpeed speedZ) {
                nowSpeed = new Vector3Int((int)speedX, (int)speedY, (int)speedZ);
            }

            /// <summary>
            /// 現在のモータ速度を設定します．
            /// <para>速度値を直接指定できますが，値には十分に注意する必要があります．</para>
            /// </summary>
            /// <param name="speed">速度</param>
            /// <param name="axis">軸</param>
            public void Inch(PlusMinus direction, double speed, VectorId axis) {
                Boolean status = true;

                int rangeData = getRangeData(axis, speed);
                status = ApciM59.SetRange(axis, (short)rangeData);
                if (!status) {
                    throw new Exception(string.Format("range data is not correct．range data = {0}", rangeData));
                }

                int startStopSpeedData = getStartStopSpeedData(axis, rangeData);
                status = ApciM59.SetStartStopSpeed(axis, (short)startStopSpeedData);
                if (!status) {
                    throw new Exception(string.Format("start stop data is not correct．start stop data = {0}",
                        startStopSpeedData));
                }

                int objectSpeedData = getObjectSpeedData(axis, speed, rangeData);
                status = ApciM59.SetObjectSpeed(axis, (short)objectSpeedData);
                if (!status) {
                    throw new Exception(string.Format("object speed data is not correct．object speed data = {0}",
                        objectSpeedData));
                }

                int rate1Data = getRate1Data(axis, speed, objectSpeedData, startStopSpeedData);
                status = ApciM59.SetRate1(axis, (short)rate1Data);
                if (!status) {
                    throw new Exception(string.Format("rate1 data is not correct．rate1 data = {0}",
                        rate1Data));
                } 
                byte command = (byte)(direction == PlusMinus.Plus ? PLUS_CONTINUOUS_DRIVE : MINUS_CONTINUOUS_DRIVE);
                status = ApciM59.WriteCommand(axis, command);
                if (!status) {
                    throw new Exception("コマンドの発信に失敗しました． command output failed.");
                }
            }

            /// <summary>
            /// インチング(寸動)します．
            /// </summary>
            /// <param name="axisAddress">移動させる軸</param>
            /// <param name="direction">方向</param>
            /// <exception cref="NagaraStage.IO.MotorOverHeatException"></exception>
            /// <exception cref="NagaraStage.IO.MotorAxisException"></exception>
            public void Inch(MechaAxisAddress axisAddress, PlusMinus direction) {
                VectorId axis = convertToAxis(axisAddress);
                Boolean status = true;
                MotorAbnomalState motorState = GetAbnomalState(axisAddress);

                if (motorState == MotorAbnomalState.OverHeat) {
                    throw new MotorOverHeatException(Properties.Strings.MotorOverHeat);
                } else if (motorState == MotorAbnomalState.AxisLimitPlus & direction == PlusMinus.Plus) {
                    throw new MotorAxisException(Properties.Strings.MotorAxisLimitPlus);
                } else if (motorState == MotorAbnomalState.AxisLimitMinus & direction == PlusMinus.Minus) {
                    throw new MotorAxisException(Properties.Strings.MotorAxisLimitMinus);
                }

                int rangeData = getRangeData(axis);
                status = ApciM59.SetRange(axis, (short)rangeData);
                if (!status) {
                    throw new Exception(string.Format("range data is not correct．range data = {0}", rangeData));
                }

                int startStopSpeedData = getStartStopSpeedData(axis);
                status = ApciM59.SetStartStopSpeed(axis, (short)startStopSpeedData);
                if (!status) {
                    throw new Exception(string.Format("start stop data is not correct．start stop data = {0}",
                        startStopSpeedData));
                }

                int objectSpeedData = getObjectSpeedData(axis);
                status = ApciM59.SetObjectSpeed(axis, (short)objectSpeedData);
                if (!status) {
                    throw new Exception(string.Format("object speed data is not correct．object speed data = {0}",
                        objectSpeedData));
                }

                int rate1Data = getRate1Data(axis);
                status = ApciM59.SetRate1(axis, (short)rate1Data);
                if (!status) {
                    throw new Exception(string.Format("rate1 data is not correct．rate1 data = {0}",
                        rate1Data));
                }

                byte command = (byte)(direction == PlusMinus.Plus ? PLUS_CONTINUOUS_DRIVE : MINUS_CONTINUOUS_DRIVE);
                status = ApciM59.WriteCommand(axis, command);
                if (!status) {
                    throw new Exception("コマンドの発信に失敗しました． command output failed.");
                }
            }

            public void Inch(VectorId axis, PlusMinus direction) {
                Inch(convertToMechaAxisAddress(axis), direction);
            }

            /// <summary>
            /// インチング(寸動)を停止します．
            /// </summary>
            /// <param name="axisAddress">停止する軸</param>
            public void StopInching(MechaAxisAddress axisAddress) {
                int status;

                Converter.IODRV1_Outpb((long)axisAddress + COMMAND_WRITE, SLOW_DOWN_STOP);

                for (int i = 0; i <= 32000; ++i) {
                    status = Converter.IODRV1_INpb((short)(axisAddress + END_STATUS_READ));
                    if ((status & 0x10) != 0x0) {
                        break;
                    }
                }

            }

            /// <summary>
            /// 指定距離を指定速度で移動するようにモータドライバに命令を発信します．
            /// <para>このメソッドは命令を出すのみであり，実際に移動したことなどの確認は行いません．
            /// 移動を完了したかは別途GetPointメソッドなどとあわせて確認を行う必要があります．
            /// </para>
            /// </summary>
            /// <param name="axisAddress">移動させる軸</param>
            /// <param name="distance">移動距離</param>
            /// <param name="oSpddt">移動速度</param>
            /// <returns>モータの異常検知結果</returns>
            private MotorAbnomalState InchUnit(MechaAxisAddress axisAddress, double distance, double oSpddt) {
                // このメソッドは途中でreturnをする場合があります．

                int x, h, m, l, tmpx;
                double resolution;
                MotorAbnomalState motorStatus = MotorAbnomalState.NoProblem;
                Boolean statusFlag = true;
                VectorId axis = convertToAxis(axisAddress);

                motorStatus = GetAbnomalState(axisAddress);
                if (motorStatus != MotorAbnomalState.NoProblem) {
                    // モータに異常を検知したら異常ステータスを返して，このメソッドを終了します．
                    return motorStatus;
                }

                int rangeData = getRangeData(axis, oSpddt);
                ApciM59.SetRange(axis, (short)rangeData);

                int startStopSpeedData = getStartStopSpeedData(axis, rangeData);
                ApciM59.SetStartStopSpeed(axis, (short)startStopSpeedData);

                int objectSpeedData = getObjectSpeedData(axis, oSpddt, rangeData);
                ApciM59.SetObjectSpeed(axis, (short)objectSpeedData);

                int rate1Data = getRate1Data(axis, oSpddt);
                ApciM59.SetRate1(axis, (short)rate1Data);

                // 移動量をパルス数に変換する
                resolution = parameterManager.MotorResolution.Index(axis);
                x = (int)(Math.Abs(distance) / resolution);
                statusFlag = (x > 1677215 ? false : statusFlag);
                h = (int)(x / 65536) & 0xFF;
                m = (int)(x / 256) & 0xFF;
                l = x & 0xFF;
                if (axisAddress == MechaAxisAddress.XAddress) {
                    tmpx = (int)((h * 65536 + m * 256 + l) * resolution);
                }

                if (statusFlag) {
                    // 移動方向
                    if (distance >= 0) {
                        // プラス
                        Converter.IODRV1_Outpb((long)(axisAddress + DATA2_WRITE), (byte)h);
                        Converter.IODRV1_Outpb((long)(axisAddress + DATA3_WRITE), (byte)m);
                        Converter.IODRV1_Outpb((long)(axisAddress + DATA4_WRITE), (byte)l);
                        Converter.IODRV1_Outpb((long)(axisAddress + COMMAND_WRITE), PLUS_PRESET_PULSE_DRIVE);
                    } else {
                        // マイナス
                        Converter.IODRV1_Outpb((long)(axisAddress + DATA2_WRITE), (byte)h);
                        Converter.IODRV1_Outpb((long)(axisAddress + DATA3_WRITE), (byte)m);
                        Converter.IODRV1_Outpb((long)(axisAddress + DATA4_WRITE), (byte)l);
                        Converter.IODRV1_Outpb((long)(axisAddress + COMMAND_WRITE), MINUS_PRESET_PULSE_DRIVE);
                    }
                }

                return motorStatus;
            }


            /// <summary>
            /// 指定距離だけ移動するようにモータドライバに命令を出します．
            /// 移動速度は予め設定されている速度を用います．
            /// <para>移動速度を設定したい場合はオーバロードされたメソッドを用いてください．</para>
            /// <para>このメソッドはモータドライバに命令を出すのみであり，移動完了などは行いません．
            /// 移動完了などは別途GetPointメソッドなどとあわせて行う必要があります．
            /// </para>
            /// </summary>
            /// <param name="axisAddress">移動させる軸</param>
            /// <param name="distance">移動距離</param>
            private MotorAbnomalState InchUnit(MechaAxisAddress axisAddress, double distance) {
                VectorId axis = convertToAxis(axisAddress);
                double objectSpeedData;

                if (axis == VectorId.X | axis == VectorId.Y) {
                    if (Math.Abs(distance) <= Marginxyp) {
                        objectSpeedData = 0.1;
                    } else if (Math.Abs(distance) < 0.3) {
                        objectSpeedData = parameterManager.MotorSpeed2.Index(axis);
                    } else if (Math.Abs(distance) < 50) {
                        objectSpeedData = parameterManager.MotorSpeed3.Index(axis);
                    } else {
                        objectSpeedData = parameterManager.MotorSpeed4.Index(axis);
                    }
                } else {
                    // Z軸の時は例外的にMotorSpeed2を設定する
                    objectSpeedData = parameterManager.MotorSpeed2.Z;
                }

                return InchUnit(axisAddress, distance, objectSpeedData);
            }

            /// <summary>
            /// 指定座標に移動します．
            /// </summary>
            /// <param name="x">移動先座標(X)</param>
            /// <param name="y">移動先座標(Y)</param>
            /// <param name="z">移動先座標(Z)</param>
            /// <param name="callback">移動処理後のコールバックメソッド</param>
            /// <param name="abortedCallback">移動処理が中止された場合のコールバックメソッド</param>
            public void MovePoint(double x, double y, double z,
              Action callback = null, AbortedCallback abortedCallback = null) {
                purposePoint = new Vector3(x, y, z);
                if (movingThread != null) {
                    if (movingThread.IsAlive) {
                        AbortMoving();
                    }
                }
                movingThread = new Thread(new ParameterizedThreadStart(movePointThreadFunction));
                movingThread.IsBackground = true;
                movingThread.Start();

                // 移動処理を監視する
                // 移動が完了したらコールバックメソッドを実行する
                if (callback != null) {
                    Thread thread = new Thread(new ThreadStart(delegate {
                        while (IsMoving) {
                            Thread.Sleep(10);
                        }
                        callback();
                    }));
                    thread.IsBackground = true;
                    thread.Start();
                }
            }

            public void MovePoint(Vector3 point, Action callback = null, AbortedCallback abortedCallback = null) {
                MovePoint(point.X, point.Y, point.Z, callback, abortedCallback);
            }

            /// <summary>
            /// 指定座標に移動します．
            /// <para>Z座標は現在地を維持します．</para>
            /// </summary>
            /// <param name="x">移動先座標(X)</param>
            /// <param name="y">移動先座標(Y)</param>
            /// <param name="callback">移動処理後のコールバックメソッド</param>
            /// <param name="abortedCallback">移動処理が中止された場合のコールバックメソッド</param>
            public void MovePointXY(double x, double y, Action callback = null, AbortedCallback abortedCallback = null) {
                MovePoint(x, y, GetPoint().Z, callback, abortedCallback);
            }

            public void MovePointXY(Vector2 p, Action callback = null, AbortedCallback abortedCallback = null) {
                MovePointXY(p.X, p.Y, callback, abortedCallback);
            }

            /// <summary>
            /// X軸を指定座標に移動します．
            /// </summary>
            /// <param name="x">移動先座標</param>
            /// <param name="callback">移動処理後のコールバックメソッド</param>
            /// <param name="abortedCallback">移動処理が中止された場合のコールバックメソッド</param>
            public void MovePointX(double x, Action callback = null, AbortedCallback abortedCallback = null) {
                Vector3 p = GetPoint();
                MovePoint(x, p.Y, p.Z, callback, abortedCallback);
            }

            /// <summary>
            /// Y軸を指定座標に移動します
            /// </summary>
            /// <param name="y">指定座標</param>
            /// <param name="callback">移動処理後のコールバックメソッド</param>
            /// <param name="abortedCallback">移動処理が中止された場合のコールバックメソッド</param>
            public void MovePointY(double y, Action callback = null, AbortedCallback abortedCallback = null) {
                Vector3 p = GetPoint();
                MovePoint(p.X, y, p.Z, callback, abortedCallback);
            }

            /// <summary>
            /// Z軸を指定座標に移動します．
            /// </summary>
            /// <param name="z">指定座標</param>
            /// <param name="callback">移動処理後のコールバックメソッド</param>
            /// <param name="abortedCallback">移動処理が中止された場合のコールバックメソッド</param>
            public void MovePointZ(double z, Action callback = null, AbortedCallback abortedCallback = null) {
                Vector3 p = GetPoint();
                MovePoint(p.X, p.Y, z, callback, abortedCallback);
            }

            /// <summary>
            /// 指定距離だけ移動させます
            /// </summary>
            /// <param name="distance">移動距離</param>
            /// <param name="axis">移動させる軸</param>
            /// <returns>移動後の位置座標(推定値)</returns>
            /// <param name="callback">移動完了後のコールバックメソッド</param>
            /// <param name="abortedCallback">移動処理が中止された場合のコールバックメソッド</param>
            public Vector3 MoveDistance(double distance, VectorId axis, Action callback = null, AbortedCallback abortedCallback = null) {
                Vector3 movedPoint = GetPoint();
                double presentPoint = GetPoint().Index(axis);
                double purpose = presentPoint + distance;

                switch (axis) {
                    case VectorId.X:
                        movedPoint.X = purpose;
                        MovePointX(purpose, callback, abortedCallback);
                        break;
                    case VectorId.Y:
                        movedPoint.Y = purpose;
                        MovePointY(purpose, callback, abortedCallback);
                        break;
                    case VectorId.Z:
                        movedPoint.Z = purpose;
                        MovePointZ(purpose, callback, abortedCallback);
                        break;
                }

                return movedPoint;
            }

            /// <summary>
            /// らせん移動を行います．
            /// <para>らせん移動とは，現在の座標を中心に1視野ずつらせんを描くように周回することです．
            /// このメソッドを1回実行すると1視野分だけ横または縦に移動します．周回するにはこのメソッドを繰り返し実行してください．</para>
            /// </summary>
            /// <param name="wait">
            /// 移動処理が完了するまで待機するまではtrue,待機しない場合はfalseを指定します．
            /// デフォルト値はfalseです．
            /// <para>trueに為た場合，移動が完了するまでこのメソッドを実行したスレッドを占有します．</para>
            /// </param>
            /// <exception cref="MotorActiveException"></exception>
            public void MoveInSpiral(bool wait = false) {
                if (IsMoving) {
                    throw new MotorActiveException();
                }

                if(SpiralIndex == 0){
                    spiralCentralPosition = GetPoint();
                }

                ++SpiralIndex;
                Vector2Int i = getSpiralPosition(SpiralIndex);
                double mx = spiralCentralPosition.X + i.X * parameterManager.SpiralShiftX;
                double my = spiralCentralPosition.Y + i.Y * parameterManager.SpiralShiftY;
                MovePointXY(mx, my, delegate {
                    if (SpiralMoved != null) {
                        SpiralMoved(this, new SpiralEventArgs(i.X, i.Y));
                    }
                });
                spiralCounter = i;
                if (wait) {
                    movingThread.Join();
                }
            }

            /// <summary>
            /// らせん移動の中心地に戻ります．
            /// </summary>
            /// <param name="wait">
            /// 移動処理の完了まで待機する場合はtrue，待機しない場合はfalse
            /// <para>trueにした場合，呼び出し側スレッドが移動完了まで停止することを留意する必要があります．</para>
            /// </param>
            public void BackToSpiralCenter(bool wait = false) {
                Vector2Int i = getSpiralPosition(0);
                MovePointXY(spiralCentralPosition.X, spiralCentralPosition.Y, delegate
                {
                    if (SpiralMoved != null)
                    {
                        SpiralMoved(this, new SpiralEventArgs(i.X, i.Y));
                    }
                });
                if (wait) {
                    movingThread.Join();
                }
                SpiralIndex = 0;
                spiralCounter = i;
            }

            /// <summary>
            /// 現在値をらせん移動の中心地に設定します．
            /// </summary>
            public void SetSpiralCenterPoint() {
                SpiralMoved(this, new SpiralEventArgs(0, 0));
                Vector2Int i = getSpiralPosition(0);
                SpiralIndex = 0;
                spiralCounter = i;
            }

            /// <summary>
            /// らせん移動を一視野もどします．
            /// <para></para>
            /// </summary>
            /// <param name="wait">
            /// 移動処理が完了するまで待機するまではtrue,待機しない場合はfalseを指定します．
            /// デフォルト値はfalseです．
            /// <para>trueに為た場合，移動が完了するまでこのメソッドを実行したスレッドを占有します．</para>
            /// </param>
            /// <exception cref="MotorActiveException"></exception>
            public void SpiralBack(bool wait = false)
            {
                if (IsMoving)
                {
                    throw new MotorActiveException();
                }
                //「すでにスパイラル原点です」と例外メッセージを出すようにしたい
                if (SpiralIndex > 0)
                {
                    SpiralIndex--;
                    Vector2Int i = getSpiralPosition(SpiralIndex);
                    double mx = spiralCentralPosition.X + i.X * parameterManager.SpiralShiftX;
                    double my = spiralCentralPosition.Y + i.Y * parameterManager.SpiralShiftY;
                    MovePointXY(mx, my, delegate
                    {
                        if (SpiralMoved != null)
                        {
                            SpiralMoved(this, new SpiralEventArgs(i.X, i.Y));
                        }
                    });
                    spiralCounter = i;
                    if (wait)
                    {
                        movingThread.Join();
                    }
                }
            }



            /// <summary>
            /// らせん移動における次の移動先を算出します．
            /// </summary>
            /// <param name="n"></param>
            /// <returns></returns>
            private Vector2Int getSpiralPosition(int n) {
                Vector2Int _spiral;

                int n1 = (int)((3 + Math.Sqrt(4 * n + 1)) / 4);
                int n2 = (int)((2 + Math.Sqrt(4 * n)) / 4);
                int n3 = (int)((1 + Math.Sqrt(4 * n + 1)) / 4);
                int n4 = (int)(Math.Sqrt(n / 4));

                if (n <= 0) {
                    _spiral = new Vector2Int(0, 0);
                } else if (n1 > n2) {
                    int nA = (n1 - 1) * (-2) + 4 * n1 * (n1 - 1);
                    int ix = -n1 + 1;
                    int iy = ix + n - nA;
                    _spiral = new Vector2Int(ix, iy);
                } else if (n2 > n3) {
                    int nB = 1 + 4 * n2 * (n2 - 1);
                    _spiral = new Vector2Int(-n1 + 1 + n - nB, n1);
                } else if (n3 > n4) {
                    int nC = 2 + (n3 - 1) * 2 + 4 * n3 * (n3 - 1);
                    _spiral = new Vector2Int(n1, n1 - n + nC);
                } else {
                    int nD = 4 + (n4 - 1) * 4 + 4 * n4 * (n4 - 1);
                    _spiral = new Vector2Int(n1 - n + nD, -n1);
                }
                return _spiral;
            } 

            /// <summary>
            /// 現在の座標移動処理を停止します．
            /// </summary>
            public void AbortMoving() {
                if (movingThread != null) {
                    if (movingThread.IsAlive) {
                        movingThread.Abort();
                        movingThread.Join();
                    }
                }
#if !_NoHardWare
                StopInching(MechaAxisAddress.XAddress);
                StopInching(MechaAxisAddress.YAddress);
                StopInching(MechaAxisAddress.ZAddress);
#endif
            }

            /// <summary>
            /// モータが移動中の場合，移動完了までスレッドを待機させます．
            /// <para>呼び出し元スレッドが移動完了まで停止させるため注意してください．</para>
            /// </summary>
            /// <param name="interval">移動確認の周期(ミリ秒), デフォルト値5msec</param>
            public void Join(int interval = 5) {
                while (IsMoving) {
                    Thread.Sleep(interval);
                    MotorAbnomalState status = GetAbnomalState(MechaAxisAddress.ZAddress);
                    switch (status) {
                        case MotorAbnomalState.AxisLimitPlus:
                        case MotorAbnomalState.AxisLimitMinus:
                            throw new MotorAxisException();
                        case MotorAbnomalState.OverHeat:
                            throw new MotorOverHeatException();
                    }

                }
            }

            /// <summary>
            /// 指定座標までの移動を行うループ処理
            /// <para>このメソッドはMovePointメソッドを介して呼び出されます．
            /// 直接呼び出さないでください．
            /// </para>
            /// </summary>                
            /// <param name="o">ThreadAbortException発生時のコールバックメソッド</param>
            private void movePointThreadFunction(object o) {
                double x = purposePoint.X;
                double y = purposePoint.Y;
                double z = purposePoint.Z;
                Vector3 delta0 = new Vector3();
                Vector3 delta = new Vector3(1, 1, 1);
                Vector3 point = new Vector3();
                MotorAbnomalState motorState = MotorAbnomalState.NoProblem;
                try {
                    bool continueFlag = true;
                    do {
#if !_NoHardWare
                        point = GetPoint();

                        delta0.X = x - point.X;
                        if (isAcceptableRangeOfError(delta0.X, VectorId.X)) {
                            delta.X = 0;
                        } else if (delta0.X < 0) {
                            delta.X = delta0.X; //- Marginxy;
                        } else {
                            delta.X = delta0.X;
                        }

                        delta0.Y = y - point.Y;
                        if (isAcceptableRangeOfError(delta0.Y, VectorId.Y)) {
                            delta.Y = 0;
                        } else if (delta0.Y < 0) {
                            delta.Y = delta0.Y;// - Marginxy;
                        } else {
                            delta.Y = delta0.Y;
                        }

                        delta0.Z = z - point.Z;
                        delta.Z = (isAcceptableRangeOfError(delta0.Z, VectorId.Z) ? 0 : delta0.Z);

                        System.Console.WriteLine(string.Format("dx,dy,dx={0},{1},{2}", delta.X, delta.Y, delta.Z));
                        continueFlag = isMovingPointContinue(delta.X, delta.Y, delta.Z, motorState);
                        motorState = moveBasic(delta.X, delta.Y, delta.Z);
#endif

                    } while (continueFlag);

                    System.Console.WriteLine("Move complete.");
                } catch (ThreadAbortException ex) {
                    if (o != null) {
                        AbortedCallback callback = (AbortedCallback)o;
                        callback(ex);
                    }
                }
            }

            /// <summary>
            /// MovoingPointThreadのループ処理を続行すべきかどうかを取得します．
            /// </summary>
            /// <param name="deltaX">移動距離(X)</param>
            /// <param name="deltaY">移動距離(Y)</param>
            /// <param name="deltaZ">移動距離(Z)</param>
            /// <param name="state">モータの異常状態</param>
            /// <returns>true: 続行してください, false: 中止してください</returns>
            private Boolean isMovingPointContinue(double deltaX, double deltaY, double deltaZ, MotorAbnomalState state) {
                Boolean flag = false;
                flag = ((deltaX != 0 | deltaY != 0 | deltaZ != 0) ? true : false);
                flag = ((state == MotorAbnomalState.NoProblem) ? flag : false);
                return flag;
            }

            /// <summary>
            /// 指定された量だけ移動させます．移動中に異常をきたした場合は移動を停止し，異常状態を返します．
            /// </summary>
            /// <param name="deltaX">移動速度(X)</param>
            /// <param name="deltaY">移動速度(Y)</param>
            /// <param name="deltaZ">移動速度(Z)</param>
            /// <returns>モータの異常状態</returns>
            private MotorAbnomalState moveBasic(double deltaX, double deltaY, double deltaZ) {
                Boolean stopFlagX = true;
                Boolean stopFlagY = true;
                Boolean stopFlagZ = true;
                MotorAbnomalState status = MotorAbnomalState.NoProblem;
                byte deviceStatus = new byte();

                // 移動距離が"0"でないことを確認し，移動を開始する．
                if (deltaX != 0) {
                    stopFlagX = false;
                    InchUnit(MechaAxisAddress.XAddress, deltaX);
                }
                if (deltaY != 0) {
                    stopFlagY = false;
                    InchUnit(MechaAxisAddress.YAddress, deltaY);
                }
                if (deltaZ != 0) {
                    stopFlagZ = false;
                    InchUnit(MechaAxisAddress.ZAddress, deltaZ);
                }

                // X軸 Y軸 Z軸の移動終了チェックする
                do {
                    if (!stopFlagX) {
                        Apci59.GetDriveStatus(ApciM59.SlotNo, (short)VectorId.X, ref deviceStatus);
                        stopFlagX = ((deviceStatus & 0x01) == 0x00 ? true : false);
                    }

                    if (!stopFlagY) {
                        Apci59.GetDriveStatus(ApciM59.SlotNo, (short)VectorId.Y, ref deviceStatus);
                        stopFlagY = ((deviceStatus & 0x01) == 0x00 ? true : false);
                    }

                    if (!stopFlagZ) {
                        Apci59.GetDriveStatus(ApciM59.SlotNo, (short)VectorId.Z, ref deviceStatus);
                        stopFlagZ = ((deviceStatus & 0x01) == 0x00 ? true : false);
                    }

                    // 異常状態を検出
                    if (!stopFlagX) {
                        status = GetAbnomalState(MechaAxisAddress.XAddress);
                    }
                    if (!stopFlagY) {
                        status = GetAbnomalState(MechaAxisAddress.YAddress);
                    }
                    if (!stopFlagZ) {
                        status = GetAbnomalState(MechaAxisAddress.ZAddress);
                    }

                } while ((!stopFlagX | !stopFlagY | !stopFlagZ) & status == MotorAbnomalState.NoProblem);

                return status;
            }

            /// <summary>
            /// Rate1 Dataを取得します
            /// </summary>
            /// <param name="axis"></param>
            /// <returns></returns>
            private int getRate1Data(VectorId axis, double originalDirectionSpeed) {
                double objectSpeedData = getObjectSpeedData(axis);
                double startStopSpeed = getStartStopSpeedData(axis);
                return getRate1Data(axis, originalDirectionSpeed, objectSpeedData, startStopSpeed);
            }

            private int getRate1Data(VectorId axis, double originalDirectionSpeed, double objectSpeedData, double startStopSpeed) {
                double initialSpeed = parameterManager.MotorInitialVelocity.Index(axis);
                double motorAccelTime = parameterManager.MotorAccelTime.Index(axis);
                double accelerationTime;
                int dt = 0;

                if (objectSpeedData - startStopSpeed > 1) {
                    accelerationTime = (originalDirectionSpeed - initialSpeed) / motorAccelTime;
                    dt = (int)((2048000 / (objectSpeedData - startStopSpeed)) * accelerationTime);
                    dt = (dt > 8191 ? 8191 : dt);
                } else {
                    dt = 1;
                }

                return dt;
            }

            public int getRate1Data(VectorId axis) {
                double originalDirectionSpeed = getOrginalSpeedDirectionData(axis);
                return getRate1Data(axis, originalDirectionSpeed);
            }

            /// <summary>
            /// 目的地との距離が誤差の許容範囲であるかどうかを返します．
            /// </summary>
            /// <param name="diestance">目的地までの距離</param>
            /// <param name="axis">軸</param>
            /// <returns>許容範囲であればtrue, そうでなければfalse</returns>
            private Boolean isAcceptableRangeOfError(double diestance, VectorId axis) {
                return (Math.Abs(diestance) < tolerance.Index(axis));
            }

            /// <summary>
            /// Object Speed Dataを取得します．
            /// </summary>
            /// <param name="axis">取得する軸</param>
            /// <param name="oSpddt"></param>
            /// <returns>Object Speed Data</returns>
            public int getObjectSpeedData(VectorId axis, double oSpddt) {
                double resolution = parameterManager.MotorResolution.Index(axis);
                int rangeData = getRangeData(axis);
                return (int)(((oSpddt / resolution) / (500.0 / rangeData)) + 0.5) & 0x1FFF;
            }

            public int getObjectSpeedData(VectorId axis, double oSpddt, double rangeData) {
                double resolution = parameterManager.MotorResolution.Index(axis);
                return (int)(((oSpddt / resolution) / (500.0 / rangeData)) + 0.5) & 0x1FFF;
            }

            /// <summary>
            /// Object Speed Dataを取得します．
            /// </summary>
            /// <param name="axis">取得する軸</param>
            /// <returns>Object Speed Data</returns>
            private int getObjectSpeedData(VectorId axis) {
                double oSpddt = getOrginalSpeedDirectionData(axis);
                return getObjectSpeedData(axis, oSpddt);
            }

            /// <summary>
            /// Start stop speed dataを取得します．
            /// </summary>
            /// <param name="axis">取得する軸</param>
            /// <returns>StartStopSpeedData</returns>
            private int getStartStopSpeedData(VectorId axis) {
                double resolution = parameterManager.MotorResolution.Index(axis);
                double startSpeedData = parameterManager.MotorInitialVelocity.Index(axis);
                int rangeData = getRangeData(axis);
                return ((int)(((startSpeedData / resolution) / (500.0 / rangeData)) + 0.5)) & 0x1FFF;
            }

            /// <summary>
            /// Start stop speed dataを取得します．
            /// </summary>
            /// <param name="axis">取得する軸</param>
            /// <returns>StartStopSpeedData</returns>
            private int getStartStopSpeedData(VectorId axis, double rangeData) {
                double resolution = parameterManager.MotorResolution.Index(axis);
                double startSpeedData = parameterManager.MotorInitialVelocity.Index(axis);
                return ((int)(((startSpeedData / resolution) / (500.0 / rangeData)) + 0.5)) & 0x1FFF;
            }

            /// <summary>
            /// Range Dataを取得します．
            /// </summary>
            /// <param name="axis">取得する軸</param>
            /// <returns>RangeData</returns>
            private int getRangeData(VectorId axis, double originalDirectionSpeed) {
                double f = originalDirectionSpeed / parameterManager.MotorResolution.Index(axis);
                double range = (4000000 / f);// +0.5;
                range = (range > 8191 ? 8191 : range);

                return ((int)range < 1 ? 1 : (int)range);
            }

            public int getRangeData(VectorId axis) {
                double originalDirectionSpeed = getOrginalSpeedDirectionData(axis);
                return getRangeData(axis, originalDirectionSpeed);
            }

            /// <summary>
            /// 軸の本来設定されている速度を取得します
            /// </summary>
            /// <param name="axis">取得する軸</param>
            /// <returns>本来の速度</returns>
            private double getOrginalSpeedDirectionData(VectorId axis) {
                double speed = 0;

                switch (nowSpeed.Index(axis)) {
                    case sp1:
                        speed = parameterManager.MotorSpeed1.Index(axis);
                        break;
                    case sp2:
                        speed = parameterManager.MotorSpeed2.Index(axis);
                        break;
                    case sp3:
                        speed = parameterManager.MotorSpeed3.Index(axis);
                        break;
                    case sp4:
                        speed = parameterManager.MotorSpeed4.Index(axis);
                        break;
                }

                return speed;
            }

            /// <summary>
            /// MechaAxisAddress型をVectorId型の値に変換します．
            /// </summary>
            /// <param name="axisAddress">変換する値</param>
            /// <returns>変換した値</returns>
            private VectorId convertToAxis(MechaAxisAddress axisAddress) {
                VectorId vector = new VectorId();

                switch (axisAddress) {
                    case MechaAxisAddress.XAddress:
                        vector = VectorId.X;
                        break;
                    case MechaAxisAddress.YAddress:
                        vector = VectorId.Y;
                        break;
                    case MechaAxisAddress.ZAddress:
                        vector = VectorId.Z;
                        break;
                }

                return vector;
            }

            private MechaAxisAddress convertToMechaAxisAddress(VectorId axis) {
                MechaAxisAddress address = new MechaAxisAddress();

                switch (axis) {
                    case VectorId.X:
                        address = MechaAxisAddress.XAddress;
                        break;
                    case VectorId.Y:
                        address = MechaAxisAddress.YAddress;
                        break;
                    case VectorId.Z:
                        address = MechaAxisAddress.ZAddress;
                        break;
                }

                return address;
            }

            /// <summary>
            /// 許容誤差を設定します．
            /// </summary>
            /// <param name="encoderResolution">エンコーダの分解能</param>
            /// <param name="motorResolution">モータの分解能</param>
            private void setTolerance(Vector3 encoderResolution, Vector3 motorResolution) {
                tolerance.X = (encoderResolution.X > motorResolution.X ? 1.2 * encoderResolution.X : 1.2 * motorResolution.X);
                tolerance.Y = (encoderResolution.Y > motorResolution.Y ? 1.2 * encoderResolution.Y : 1.2 * motorResolution.Y);
                tolerance.Z = (encoderResolution.Z > motorResolution.Z ? 1.2 * encoderResolution.Z : 1.2 * motorResolution.Z);
            }
        }
    }
}