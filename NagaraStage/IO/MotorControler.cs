using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

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

            private static Boolean enabled = false;
            public static Boolean Enabled {
                get { return enabled; }
            }

            private static short slotNo = 0;
            public static short SlotNo {
                get { return slotNo; }
            }


            /// <summary>ThreadAbortExceptionが発生した時に行うメッソドのためのデリゲートです． </summary>        
            public delegate void AbortedCallback(ThreadAbortException ex);

            private static MotorControler instance;
            private ParameterManager parameterManager;
            private Vector3 speed;
            private Vector3 tolerance = new Vector3();
            private Thread movingThread;
            //private Thread isMovingPointContinuemovingThread;

            /// <summary>らせん移動の相対的な視野位置(viewx,viewy)を保持するカウンタ</summary>            
            private Vector2Int spiralCounter;

            /// <summary>らせん移動の中心座標</summary>
            private Vector3 spiralCentralPosition;

            /// <summary>
            /// らせん移動において，次の行き先を示すインデックスを取得，または設定します．
            /// <para>この値はらせん移動を行うと自動更新されます．通常はこの値を設定する必要ありません．
            /// ただし，らせん移動を中心からやり直す場合は0を設定してください．</para>
            /// </summary>
            private int spiralIndex = 0;
            public int SpiralIndex {
                get { return spiralIndex; }
                set { spiralIndex = value; }
            }

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
            public Vector3 Speed {
                get { return speed; }
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
                this.speed = _parameterManager.MotorSpeed1;
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






            public bool setVelocityParams(VectorId axis, double objSpeed) {
            
                Boolean status;
                double resolution = parameterManager.MotorResolution.Index(axis);

                // Range dataの設定
                double f = objSpeed / resolution;
                double rangeData = (4000000 / f) + 0.5;

                int iRangeData = (int)rangeData;
                iRangeData = (iRangeData > 8191 ? 8191 : iRangeData);
                iRangeData = (iRangeData < 1 ? 1 : iRangeData);
                status = Apci59.DataHalfWrite(SlotNo, (short)axis, RANGE_WRITE, (short)iRangeData);
                if (!status) {
                    throw new Exception(string.Format("range data is not correct．range data = {0}", iRangeData));
                }


                // Start stop speed dataの作成
                double ssSpeed = parameterManager.MotorInitialVelocity.Index(axis);
                double funit = 500.0 / (iRangeData * 1.0);

                if (ssSpeed > objSpeed) {
                    ssSpeed = objSpeed;
                }
                double ssSpeedData = (ssSpeed / resolution) / funit + 0.5;

                int iSsSpeedData = (int)ssSpeedData;
                iSsSpeedData = (iSsSpeedData > 8191 ? 8191 : iSsSpeedData);
                iSsSpeedData = (iSsSpeedData < 1 ? 1 : iSsSpeedData);
                status = Apci59.DataHalfWrite(SlotNo, (short)axis, START_STOP_SPEED_DATA_WRITE, (short)iSsSpeedData);
                if (!status) {
                    throw new Exception(string.Format("start stop data is not correct．start stop data = {0}", iSsSpeedData));
                }

                // Object speed data の作成
                double objSpeedData = (objSpeed / resolution) / funit + 0.5;

                int iObjSpeedData = (int)objSpeedData;
                iObjSpeedData = (iObjSpeedData > 8191 ? 8191 : iObjSpeedData);
                iObjSpeedData = (iObjSpeedData < 1 ? 1 : iObjSpeedData);
                status = Apci59.DataHalfWrite(SlotNo, (short)axis, OBJECT_SPEED_DATA_WRITE, (short)iObjSpeedData);
                if (!status) {
                    throw new Exception(string.Format("object speed data is not correct．object speed data = {0}", iObjSpeedData));
                }

                // Rate1 data の作成
                double motorAccel = parameterManager.MotorAccelTime.Index(axis);

                double AccelTime = (objSpeed - ssSpeed) / motorAccel;
                double rate1Data = 2048000.0 / (iObjSpeedData - iSsSpeedData) * AccelTime;

                int iRate1Data = (int)rate1Data;
                iRate1Data = (iRate1Data > 8191 ? 8191 : iRate1Data);
                iRate1Data = (iRate1Data < 1 ? 1 : iRate1Data);
                status = Apci59.DataFullWrite(SlotNo, (short)axis, RATE1_DATA_WRITE, (short)iRate1Data);
                if (!status) {
                    throw new Exception(string.Format("rate1 data is not correct．rate1 data = {0}", iRate1Data));
                }

                // Rate2, 3, rate_change_point_1_2
                status = Apci59.DataFullWrite(SlotNo, (short)axis, RATE2_DATA_WRITE, 0x1FFF);
                status = Apci59.DataFullWrite(SlotNo, (short)axis, RATE3_DATA_WRITE, 0x1FFF);
                status = Apci59.DataFullWrite(SlotNo, (short)axis, RATE_CHANGE_POINT_1_2_WRITE, 0x1FFF);

                return true;
            }





            /// <summary>
            /// I/Oドライバを初期化します
            /// </summary>
            /// <exception cref="System.Exception">モーター制御ボードの初期化に失敗した場合</exception>
            public void Initialize() {
#if !_NoHardWare
                Boolean status;
                try {
                    slotNo = Apci59.SLOT_AUTO;
                    status = Apci59.Create(ref slotNo);

                    InitializeMotorControlParams(VectorId.X);
                    InitializeMotorControlParams(VectorId.Y);
                    InitializeMotorControlParams(VectorId.Z);

                } catch (Exception) {
                    status = false;
                    throw new Exception("Initializing aPCI-59 Motor Control Board is failed.");
                }
#endif
            }

      
            
            /// <summary>
            /// モータコントロールボードのパラメータを初期化します．
            /// </summary>
            /// <param name="axisAddress">初期化する軸</param>
            public void InitializeMotorControlParams(VectorId axis) {
                Boolean status;

                // パルス出力方式(1パルス)，これでいいはず
                if (axis == VectorId.Z) {
                    status = Apci59.Mode1Write(SlotNo, (short)axis, 0x20);
                } else {
                    // X, Y軸は動きが逆
                    status = Apci59.Mode1Write(SlotNo, (short)axis, 0x30);
                }

                // エンコーダ入力仕様(4てい倍)
                int limitPol0 = 252;//default val
                limitPol0 = (int)(parameterManager.LimitPol != 0 ? parameterManager.LimitPol : limitPol0);
                status = Apci59.Mode2Write(SlotNo, (short)axis, (byte)limitPol0);

                //DEAD,DERROR,リミット極性指定
                status = Apci59.CommandWrite(SlotNo, (short)axis, INPOSITION_WAIT_MODE_RESET);
                status = Apci59.CommandWrite(SlotNo, (short)axis, ALARM_STOP_ENABLE_MODE_RESET);
                status = Apci59.CommandWrite(SlotNo, (short)axis, INTERRUPT_OUT_ENABLE_MODE_RESET);

                // speedの設定
                double tmpSpeed = getPresetSpeed(axis);
                status = setVelocityParams(axis, tmpSpeed);
            }


            /// <summary>
            /// モータコントロールボードのカウンタ（位置情報）を初期化します．カウンタはPCとステージの再起動をしない限り保持されます。
            /// </summary>
            /// <param name="axisAddress">初期化する軸</param>
            public void InitializeMotorControlCounter(VectorId axis) {
                Boolean status;
                status = Apci59.DataFullWrite(SlotNo, (short)axis, INTERNAL_COUNTER_WRITE, 0);
                status = Apci59.DataFullWrite(SlotNo, (short)axis, EXTERNAL_COUNTER_WRITE, 0);
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
                Apci59.DataFullRead(SlotNo, (short)VectorId.X, Apci59.EXTERNAL_COUNTER_READ, ref c.X);
                Apci59.DataFullRead(SlotNo, (short)VectorId.Y, Apci59.EXTERNAL_COUNTER_READ, ref c.Y);
                Apci59.DataFullRead(SlotNo, (short)VectorId.Z, Apci59.EXTERNAL_COUNTER_READ, ref c.Z);
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
            /// 現在のモータの速度を設定します．
            /// </summary>
            /// <param name="speed">モータの速度</param>
            public void SetMotorSpeed(Vector3 _speed) {
                speed = _speed;
            }

            /// <summary>
            /// 現在のモータの速度を設定します．
            /// </summary>
            /// <param name="speedX">X軸方向のスピード</param>
            /// <param name="speedY">Y軸方向のスピード</param>
            /// <param name="speedZ">Z軸方向のスピード</param>
            public void SetMotorSpeed(double speedX, double speedY, double speedZ) {
                speed = new Vector3(speedX, speedY, speedZ);
            }

            /// <summary>
            /// 現在のモータの速度を設定します。
            /// </summary>
            /// <param name="speed">スピード</param>
            /// <param name="axis">変更する軸</param>
            public void SetMotorSpeed(double _speed, VectorId axis) {
                switch (axis) { 
                    case VectorId.X:
                        speed.X = _speed;
                        break;
                    case VectorId.Y:
                        speed.Y = _speed;
                        break;
                    case VectorId.Z:
                        speed.Z = _speed;
                        break;
                }
            }

            /// <summary>
            /// 現在のモータ速度を設定します．
            /// <para>速度値を直接指定できますが，値には十分に注意する必要があります．</para>
            /// </summary>
            /// <param name="speed">速度</param>
            /// <param name="axis">軸</param>
            public void Inch(PlusMinus direction, double objSpeed, VectorId axis) {
                Boolean status = true;

                status = setVelocityParams(axis, objSpeed);

                byte command = (byte)(direction == PlusMinus.Plus ? PLUS_CONTINUOUS_DRIVE : MINUS_CONTINUOUS_DRIVE);
                status = Apci59.CommandWrite(SlotNo, (short)axis, command);
                if (!status) {
                    throw new Exception("コマンドの発信に失敗しました． command output failed.");
                }
            }

            /// <summary>
            /// 一定速度でモーターをドライブします．
            /// </summary>
            /// <param name="axisAddress">移動させる軸</param>
            /// <param name="direction">方向</param>
            /// <exception cref="NagaraStage.IO.MotorOverHeatException"></exception>
            /// <exception cref="NagaraStage.IO.MotorAxisException"></exception>
            public void ContinuousDrive(VectorId axis, PlusMinus direction, double objSpeed) {
                MotorState motorState = GetMotorState(axis);

                if (motorState == MotorState.OverHeat) {
                    throw new MotorException("MotorOverHeat");
                } else if (motorState == MotorState.AxisLimitPlus & direction == PlusMinus.Plus) {
                    throw new MotorException("MotorAxisLimitPlus");
                } else if (motorState == MotorState.AxisLimitMinus & direction == PlusMinus.Minus) {
                    throw new MotorException("MotorAxisLimitMinus");
                }

                Boolean status = true;
                status = setVelocityParams(axis, objSpeed);


                byte command = (byte)(direction == PlusMinus.Plus ? PLUS_CONTINUOUS_DRIVE : MINUS_CONTINUOUS_DRIVE);
                status = Apci59.CommandWrite(SlotNo, (short)axis, command);
                if (!status) {
                    throw new Exception("コマンドの発信に失敗しました． command output failed.");
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
            private MotorState InchUnit(VectorId axis, double distance, double objSpeed) {
                // このメソッドは途中でreturnをする場合があります．

                MotorState motorStatus = MotorState.NoProblem;

                motorStatus = GetMotorState(axis);
                if (motorStatus != MotorState.NoProblem) {
                    // モータに異常を検知したら異常ステータスを返して，このメソッドを終了します．
                    return motorStatus;
                }

                bool status;
                status = setVelocityParams(axis, objSpeed);

                // 移動量をパルス数に変換する
                double resolution = parameterManager.MotorResolution.Index(axis);
                int x = (int)(Math.Abs(distance) / resolution);
                if (x > 1677215) return motorStatus;
                
                byte bdirection = (byte)(distance > 0 ? Apci59.PLUS_PRESET_PULSE_DRIVE : Apci59.MINUS_PRESET_PULSE_DRIVE);
                if (false == Apci59.DataFullWrite(SlotNo, (short)axis, bdirection, x)) {
                    return motorStatus;
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
            private MotorState InchUnit(VectorId axis, double distance) {
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

                return InchUnit(axis, distance, objectSpeedData);
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
                SlowDownStopAll();

#endif
            }

            public void AbortMoving(VectorId axis) {
                if (movingThread != null) {
                    if (movingThread.IsAlive) {
                        movingThread.Abort();
                        movingThread.Join();
                    }
                }
                SlowDownStop(axis);
            }

            /// <summary>
            /// モータが移動中の場合，移動完了までスレッドを待機させます．
            /// <para>呼び出し元スレッドが移動完了まで停止させるため注意してください．</para>
            /// </summary>
            /// <param name="interval">移動確認の周期(ミリ秒), デフォルト値5msec</param>
            public void Join(int interval = 5) {
                while (IsMoving) {
                    Thread.Sleep(interval);
                    MotorState status = GetMotorState(VectorId.Z);//なんでZのみ?
                    switch (status) {
                        case MotorState.AxisLimitPlus:
                        case MotorState.AxisLimitMinus:
                            throw new MotorException("axis limit");
                        case MotorState.OverHeat:
                            throw new MotorException("overheat");
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
                MotorState motorState = MotorState.NoProblem;
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

                        System.Console.WriteLine(string.Format("dx,dy,dz={0},{1},{2}", delta.X, delta.Y, delta.Z));
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
            /// 目的地との距離が誤差の許容範囲であるかどうかを返します．
            /// </summary>
            /// <param name="diestance">目的地までの距離</param>
            /// <param name="axis">軸</param>
            /// <returns>許容範囲であればtrue, そうでなければfalse</returns>
            private Boolean isAcceptableRangeOfError(double diestance, VectorId axis) {
                return (Math.Abs(diestance) < tolerance.Index(axis));
            }


            /// <summary>
            /// MovoingPointThreadのループ処理を続行すべきかどうかを取得します．
            /// </summary>
            /// <param name="deltaX">移動距離(X)</param>
            /// <param name="deltaY">移動距離(Y)</param>
            /// <param name="deltaZ">移動距離(Z)</param>
            /// <param name="state">モータの異常状態</param>
            /// <returns>true: 続行してください, false: 中止してください</returns>
            private Boolean isMovingPointContinue(double deltaX, double deltaY, double deltaZ, MotorState state) {
                Boolean flag = false;
                flag = ((deltaX != 0 | deltaY != 0 | deltaZ != 0) ? true : false);
                flag = ((state == MotorState.NoProblem) ? flag : false);
                return flag;
            }
            
            private bool isMotorStateOk(params MotorState[] states) {
                bool retval = true;
                foreach (var state in states) {
                    retval &= state == MotorState.NoProblem;
                }
                return retval;
            }

            /// <summary>
            /// 指定された量だけ移動させます．移動中に異常をきたした場合は移動を停止し，異常状態を返します．
            /// </summary>
            /// <param name="deltaX">移動速度(X)</param>
            /// <param name="deltaY">移動速度(Y)</param>
            /// <param name="deltaZ">移動速度(Z)</param>
            /// <returns>モータの異常状態</returns>
            private MotorState moveBasic(double deltaX, double deltaY, double deltaZ) {
                Boolean stopFlagX = true;
                Boolean stopFlagY = true;
                Boolean stopFlagZ = true;
                MotorState status = MotorState.NoProblem;
                byte deviceStatus = new byte();

                // 移動距離が"0"でないことを確認し，移動を開始する．
                if (deltaX != 0) {
                    stopFlagX = false;
                    InchUnit(VectorId.X, deltaX);
                }
                if (deltaY != 0) {
                    stopFlagY = false;
                    InchUnit(VectorId.Y, deltaY);
                }
                if (deltaZ != 0) {
                    stopFlagZ = false;
                    InchUnit(VectorId.Z, deltaZ);
                }

                // X軸 Y軸 Z軸の移動終了チェックする
                do {
                    if (!stopFlagX) {
                        Apci59.GetDriveStatus(SlotNo, (short)VectorId.X, ref deviceStatus);
                        stopFlagX = ((deviceStatus & 0x01) == 0x00 ? true : false);
                    }

                    if (!stopFlagY) {
                        Apci59.GetDriveStatus(SlotNo, (short)VectorId.Y, ref deviceStatus);
                        stopFlagY = ((deviceStatus & 0x01) == 0x00 ? true : false);
                    }

                    if (!stopFlagZ) {
                        Apci59.GetDriveStatus(SlotNo, (short)VectorId.Z, ref deviceStatus);
                        stopFlagZ = ((deviceStatus & 0x01) == 0x00 ? true : false);
                    }

                    // 異常状態を検出
                    if (!stopFlagX) {
                        status = GetMotorState(VectorId.X);
                    }
                    if (!stopFlagY) {
                        status = GetMotorState(VectorId.Y);
                    }
                    if (!stopFlagZ) {
                        status = GetMotorState(VectorId.Z);
                    }

                } while ((!stopFlagX | !stopFlagY | !stopFlagZ) & status == MotorState.NoProblem);

                return status;
            }




            /// <summary>
            /// スピード設定に関するパラメータ類を表示
            /// </summary>
            public void DisplayStat() {
                for (short ax = 0; ax < 3; ax++) {
                    int range = 0;
                    Apci59.DataFullRead(SlotNo, ax, Apci59.RANGE_DATA_READ, ref range);
                    int ssspeed = 0;
                    Apci59.DataFullRead(SlotNo, ax, Apci59.START_STOP_SPEED_DATA_READ, ref ssspeed);
                    int ospeed = 0;
                    Apci59.DataFullRead(SlotNo, ax, Apci59.OBJECT_SPEED_DATA_READ, ref ospeed);
                    int rate1 = 0;
                    Apci59.DataFullRead(SlotNo, ax, Apci59.RATE1_DATA_READ, ref rate1);
                    System.Console.WriteLine(string.Format("ax, range, ssspeed, ospeed, rate1 = {0} {1} {2} {3} {4}", ax, range, ssspeed, ospeed, rate1));
                }
            }


            /// <summary>
            /// 設定されているドライブ速度を取得します
            /// </summary>
            /// <param name="axis">取得する軸</param>
            /// <returns>本来の速度</returns>
            private double getPresetSpeed(VectorId axis) {
                double retspeed = 0;

                switch (axis) {
                    case VectorId.X:
                        retspeed = speed.X;
                        break;
                    case VectorId.Y:
                        retspeed = speed.Y;
                        break;
                    case VectorId.Z:
                        retspeed = speed.Z;
                        break;
                }
                return retspeed;
            }



            /// <summary>
            /// 許容誤差を設定します．
            /// </summary>
            /// <param name="encoderResolution">エンコーダの分解能</param>
            /// <param name="motorResolution">モータの分解能</param>
            private void setTolerance(Vector3 encoderResolution, Vector3 motorResolution) {
                tolerance.X = (encoderResolution.X > motorResolution.X ? 0.95 * encoderResolution.X : 1.2 * motorResolution.X);
                tolerance.Y = (encoderResolution.Y > motorResolution.Y ? 0.95 * encoderResolution.Y : 1.2 * motorResolution.Y);
                tolerance.Z = (encoderResolution.Z > motorResolution.Z ? 0.95 * encoderResolution.Z : 1.2 * motorResolution.Z);
            }




            /// <summary>
            /// 指定座標に移動します．ただし，おおよその位置です．
            /// <para>移指定座標に移動するようにモータドライバに命令を送信しますが，
            /// 通常のMovePointとは異なり，移動後，座標確認をしながら細かい位置補整を行いません．
            /// </para>
            /// </summary>
            /// <param name="to"></param>
            /// <param name="speed">移動速度</param>
            public void MovePointApproximate(Vector3 to, Vector3 _speed) {
                if (movingThread != null) {
                    if (movingThread.IsAlive) {
                        AbortMoving();
                    }
                }
                movingThread = new Thread(new ThreadStart(new Action(delegate {
                    try {
                        Vector3 amountOfMovement = to - GetPoint();
                        MotorState stateX = MotorState.NoProblem,
                            stateY = MotorState.NoProblem,
                            stateZ = MotorState.NoProblem;
                        Vector3 absAmount = amountOfMovement.ToAbs();
                        bool activeFlagX, activeFlagY, activeFlagZ;
                        if (activeFlagX = (absAmount.X > Tolerance.X)) {
                            stateX = InchUnit(VectorId.X, amountOfMovement.X);
                        }
                        if (activeFlagY = (absAmount.Y > Tolerance.Y)) {
                            stateY = InchUnit(VectorId.Y, amountOfMovement.Y);
                        }
                        if (activeFlagZ = (absAmount.Z > Tolerance.Z)) {
                            stateZ = InchUnit(VectorId.Z, amountOfMovement.Z);
                        }

                        while (activeFlagX || activeFlagY || activeFlagZ) {
                            Thread.Sleep(1);
                            byte deviceStatus = new byte();
                            if (activeFlagX) {
                                Apci59.GetDriveStatus(SlotNo, (short)VectorId.X, ref deviceStatus);
                                activeFlagX = !((deviceStatus & 0x01) == 0x00);
                            }
                            if (activeFlagY) {
                                Apci59.GetDriveStatus(SlotNo, (short)VectorId.Y, ref deviceStatus);
                                activeFlagY = !((deviceStatus & 0x01) == 0x00);
                            }
                            if (activeFlagZ) {
                                Apci59.GetDriveStatus(SlotNo, (short)VectorId.Z, ref deviceStatus);
                                activeFlagZ = !((deviceStatus & 0x01) == 0x00);
                            }
                            if (!isMotorStateOk(stateX, stateY, stateZ)) {
                                throw new MotorActiveException();
                            }
                        }
                    } catch (MotorActiveException) {
                    } catch (ThreadAbortException) {
                    } finally {
                        SlowDownStopAll();

                    }
                })));
                movingThread.IsBackground = true;
                movingThread.Start();
            }


            ////////////////////////////////////////////////////////////////////////////////////////////////////////


            /// <summary>
            /// モータの異常状態を検出します．
            /// </summary>
            /// <param name="axis">検出する軸</param>
            /// <param name="isConfCheck">設定値の異常もチェックするかどうか</param>
            /// <returns>異常状態</returns>
            public MotorState GetMotorState(VectorId axis, Boolean isConfCheck = false) {
                MotorState motorState = MotorState.NoProblem;
                byte returnValue = new byte();
                Apci59.GetMechanicalSignal(SlotNo, (short)(axis), ref returnValue);

                // 設定値の異常
                if (isConfCheck) {
                    if ((returnValue & (byte)MotorState.ConfiguredValueNotCorrect) != 0x0) {
                        motorState |= MotorState.ConfiguredValueNotCorrect;
                    }
                }

                // オーバーヒート
                if ((returnValue & (byte)MotorState.OverHeat) != 0x0) {
                    motorState |= MotorState.OverHeat;
                }

                // マイナス方向に軸の限界
                if ((returnValue & (byte)MotorState.AxisLimitMinus) != 0x0) {
                    motorState |= MotorState.AxisLimitMinus;
                }

                // プラス方向に軸の限界
                if ((returnValue & (byte)MotorState.AxisLimitPlus) != 0x0) {
                    motorState |= MotorState.AxisLimitPlus;
                }

                return motorState;
            }



            /// <summary>
            /// 指定された軸を指定された距離動かします。Move、MoveToではこの関数が実行されます。
            /// <para>
            /// 移動後，座標確認をしながら細かい位置補整を行いません．
            /// </para>
            /// </summary>
            /// <param name="axis">軸番号</param>
            /// <param name="distance">移動距離[mm]</param>
            /// <param name="speed">移動速度</param>
            private MotorState PresetPulseDrive(VectorId axis, double distance, double objSpeed) {
                MotorState motorStatus = MotorState.NoProblem;
                motorStatus = GetMotorState(axis);
                if (motorStatus != MotorState.NoProblem) {
                    return motorStatus;
                }
                Boolean status;
                status = setVelocityParams(axis, objSpeed);


                // 移動量をパルス数に変換する
                double resolution = parameterManager.MotorResolution.Index(axis);
                int x = (int)(Math.Abs(distance) / resolution);

                if (x > 1677215) return motorStatus;


                byte bdirection = (byte)(distance > 0 ? Apci59.PLUS_PRESET_PULSE_DRIVE : Apci59.MINUS_PRESET_PULSE_DRIVE);
                status = Apci59.DataFullWrite(SlotNo, (short)axis, bdirection, x);
                if (!status) {
                    return motorStatus;
                }

                return motorStatus;
            }


            /// <summary>
            /// Test
            /// </summary>
            public void AAAAAA() {

                Vector3 currentpoint = new Vector3();
                for (int i = 0; i < 10; i++) {
                    Vector3 distance = new Vector3(2, 3, 0.01);
                    Move(distance);
                    Join();
                    currentpoint = GetPoint();
                    System.Console.WriteLine(string.Format("{0},{1},{2}", currentpoint.X, currentpoint.Y, currentpoint.Z));
                }

            }


            /// <summary>
            /// 指定された3軸の距離分動かします。
            /// <para>
            /// 
            /// </para>
            /// </summary>
            /// <param name="distance">移動距離[mm]</param>
            /// <param name="_tolerance">目的地と到着地点誤差の許容値、いまはつかっていない</param>
            public void Move(Vector3 distance) {

                //トレランスがゼロなら、ハードウェアの分解能によって規定されたトレランスの最小値を設定するようにしたい

                if (movingThread != null) {
                    if (movingThread.IsAlive) {
                        AbortMoving();
                    }
                }

                movingThread = new Thread(new ThreadStart(new Action(delegate {
                    try {

                        MotorState 
                            stateX = MotorState.NoProblem,
                            stateY = MotorState.NoProblem,
                            stateZ = MotorState.NoProblem;

                        //移動量がToleranceよりも大きな値かを確認。移動量が微少だったら動かさない。
                        //移動する軸に対しactiveFlagを用意して、移動するのであればTrueにする
                        //移動速度はspeed=4
                        bool activeFlagX, activeFlagY, activeFlagZ;
                        Vector3 absdistance = distance.ToAbs();

                        if (activeFlagX = (absdistance.X > Tolerance.X)) {
                            stateX = PresetPulseDrive(VectorId.X, distance.X, parameterManager.MotorSpeed4.X);
                        }
                        if (activeFlagY = (absdistance.Y > Tolerance.Y)) {
                            stateY = PresetPulseDrive(VectorId.Y, distance.Y, parameterManager.MotorSpeed4.Y);
                        }
                        if (activeFlagZ = (absdistance.Z > Tolerance.Z)) {
                            stateZ = PresetPulseDrive(VectorId.Z, distance.Z, parameterManager.MotorSpeed4.Z);
                        }

                        //移動が完了したら各軸のactiveFlagをFalseにする
                        //GetDriveStatus関数によって移動が完了したかを監視する
                        while (activeFlagX || activeFlagY || activeFlagZ) {
                            Thread.Sleep(1);
                            byte deviceStatus = new byte();
                            if (activeFlagX) {
                                Apci59.GetDriveStatus(SlotNo, (short)VectorId.X, ref deviceStatus);
                                activeFlagX = !((deviceStatus & 0x01) == 0x00);
                            }
                            if (activeFlagY) {
                                Apci59.GetDriveStatus(SlotNo, (short)VectorId.Y, ref deviceStatus);
                                activeFlagY = !((deviceStatus & 0x01) == 0x00);
                            }
                            if (activeFlagZ) {
                                Apci59.GetDriveStatus(SlotNo, (short)VectorId.Z, ref deviceStatus);
                                activeFlagZ = !((deviceStatus & 0x01) == 0x00);
                            }
                            if (!isMotorStateOk(stateX, stateY, stateZ)) {
                                throw new MotorActiveException();
                            }
                        }
                    } catch (MotorActiveException) {
                    } catch (ThreadAbortException) {
                    } finally {
                        SlowDownStopAll();
                    }
                })));
                movingThread.IsBackground = true;
                movingThread.Start();
            }



            /// <summary>
            /// 指定された3軸の位置へ動かします。
            /// <para>
            /// 
            /// </para>
            /// </summary>
            /// <param name="to">目的地点の座標[mm]</param>
            /// <param name="speed">移動速度、いまはつかってない</param>
            /// <param name="_tolerance">目的地と到着地点誤差の許容値、いまはつかっていない</param>
            public void MoveTo(Vector3 to) { 
                Vector3 distance = to - GetPoint();
                Move(distance); 
            }



            /// <summary>
            /// 減速停止します．
            /// </summary>
            /// <param name="axis">停止する軸</param>
            public void SlowDownStop(VectorId axis) {
                if (false == Apci59.CommandWrite(SlotNo, (short)axis, Apci59.SLOW_DOWN_STOP)) return;
                bool status;
                byte pbstat = 0x0;
                while (true) {
                    status = Apci59.GetEndStatus(SlotNo, (short)axis, ref pbstat);
                    //System.Diagnostics.Debug.WriteLine(String.Format("pbstat {0}", pbstat));
                    if ((pbstat & 0x01) == 0x0) break;
                }
            }

            
            /// <summary>
            /// 全軸減速停止します
            /// </summary>
            public void SlowDownStopAll() {
                SlowDownStop(VectorId.X);
                SlowDownStop(VectorId.Y);
                SlowDownStop(VectorId.Z);
            }


            /// <summary>
            /// 閉じます
            /// </summary>
            public static void Terminate() {
                if (enabled) {
                    Apci59.Close(slotNo);
                    enabled = false;
                }
            }



/////spiralmove//////////////////////////////////////////

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

                if (spiralIndex == 0) {
                    spiralCentralPosition = GetPoint();
                }

                ++spiralIndex;
                Vector2Int i = getSpiralPosition(spiralIndex);
                double mx = spiralCentralPosition.X + i.X * parameterManager.SpiralShiftX;
                double my = spiralCentralPosition.Y + i.Y * parameterManager.SpiralShiftY;
                MoveTo(new Vector3(mx, my, GetPoint().Z));
                Join();

                spiralCounter = i;
                SpiralMoved(this, new SpiralEventArgs(i.X, i.Y));
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
                MovePointXY(spiralCentralPosition.X, spiralCentralPosition.Y, delegate {
                    if (SpiralMoved != null) {
                        SpiralMoved(this, new SpiralEventArgs(i.X, i.Y));
                    }
                });
                if (wait) {
                    movingThread.Join();
                }
                spiralIndex = 0;
                spiralCounter = i;
            }

            /// <summary>
            /// 現在値をらせん移動の中心地に設定します．
            /// </summary>
            public void SetSpiralCenterPoint() {
                SpiralMoved(this, new SpiralEventArgs(0, 0));
                Vector2Int i = getSpiralPosition(0);
                spiralIndex = 0;
                spiralCounter = i;
                spiralCentralPosition = GetPoint();
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
            public void SpiralBack(bool wait = false) {
                if (IsMoving) {
                    throw new MotorActiveException();
                }
                //「すでにスパイラル原点です」と例外メッセージを出すようにしたい
                if (spiralIndex > 0) {
                    spiralIndex--;
                    Vector2Int i = getSpiralPosition(spiralIndex);
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



        }
    }
}