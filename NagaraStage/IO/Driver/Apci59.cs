using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace NagaraStage {
    namespace IO {
        namespace Driver {
            /// <summary>
            /// apci59.dllをインポートしラッピングを行うクラスです.
            /// apci59.dllがもつ関数を静的なメソッドとして提供します．
            /// <para>モータコントロールボードaPCIM-59にアクセスします．</para>
            /// </summary>
            /// <author email="hirokazu.online@gmail.com">Hirokazu Yokoyama</author>
            /// <author href="http://www.adtek.co.jp/seihin/apci/aPCI-M59.html">
            /// ADTEK SYSTEM SCIENCE CO.,Ltd.
            /// </author>
            /// <date>2012-07-05</date>
            class Apci59 {

                /**-----------------------------------------------------------------
                * Axis
                *-----------------------------------------------------------------*/
                public const short AXIS_1 = 0;
                public const short AXIS_2 = 1;
                public const short AXIS_3 = 2;
                public const short AXIS_4 = 3;

                /**-----------------------------------------------------------------
                ' command
                '-----------------------------------------------------------------*/
                public const byte RANGE_DATA_WRITE = 0x0;
                public const byte RANGE_DATA_READ = 0x1;
                public const byte START_STOP_SPEED_DATA_WRITE = 0x2;
                public const byte START_STOP_SPEED_DATA_READ = 0x3;
                public const byte OBJECT_SPEED_DATA_WRITE = 0x4;
                public const byte OBJECT_SPEED_DATA_READ = 0x5;
                public const byte RATE1_DATA_WRITE = 0x6;
                public const byte RATE1_DATA_READ = 0x7;
                public const byte RATE2_DATA_WRITE = 0x8;
                public const byte RATE2_DATA_READ = 0x9;
                public const byte RATE3_DATA_WRITE = 0xA;
                public const byte RATE3_DATA_READ = 0xB;
                public const byte RATE_CHANGE_PObyte_1_2_WRITE = 0xC;
                public const byte RATE_CHANGE_PObyte_1_2_READ = 0xD;
                public const byte RATE_CHANGE_PObyte_2_3_WRITE = 0xE;
                public const byte RATE_CHANGE_PObyte_2_3_READ = 0xF;
                public const byte SLOW_DOWN_REAR_PULSE_WRITE = 0x10;
                public const byte SLOW_DOWN_REAR_PULSE_READ = 0x11;
                public const byte NOW_SPEED_DATA_READ = 0x12;
                public const byte DRIVE_PULSE_COUNTER_READ = 0x13;
                public const byte PRESET_PULSE_DATA_OVERRIDE = 0x14;
                public const byte PRESET_PULSE_DATA_READ = 0x15;
                public const byte DEVIATION_DATA_READ = 0x16;
                public const byte INPOSITION_WAIT_MODE1_SET = 0x17;
                public const byte INPOSITION_WAIT_MODE2_SET = 0x18;
                public const byte INPOSITION_WAIT_MODE_RESET = 0x19;
                public const byte ALARM_STOP_ENABLE_MODE_SET = 0x1A;
                public const byte ALARM_STOP_ENABLE_MODE_RESET = 0x1B;
                public const byte byteERRUPT_OUT_ENABLE_MODE_SET = 0x1C;
                public const byte byteERRUPT_OUT_ENABLE_MODE_RESET = 0x1D;
                public const byte SLOW_DOWN_STOP = 0x1E;
                public const byte EMERGENCY_STOP = 0x1F;
                public const byte PLUS_PRESET_PULSE_DRIVE = 0x20;
                public const byte MINUS_PRESET_PULSE_DRIVE = 0x21;
                public const byte PLUS_CONTINUOUS_DRIVE = 0x22;
                public const byte MINUS_CONTINUOUS_DRIVE = 0x23;
                public const byte PLUS_SIGNAL_SEARCH1_DRIVE = 0x24;
                public const byte MINUS_SIGNAL_SEARCH1_DRIVE = 0x25;
                public const byte PLUS_SIGNAL_SEARCH2_DRIVE = 0x26;
                public const byte MINUS_SIGNAL_SEARCH2_DRIVE = 0x27;
                public const byte INTERNAL_COUNTER_WRITE = 0x28;
                public const byte INTERNAL_COUNTER_READ = 0x29;
                public const byte INTERNAL_COMPARATE_DATA_WRITE = 0x2A;
                public const byte INTERNAL_COMPARATE_DATA_READ = 0x2B;
                public const byte EXTERNAL_COUNTER_WRITE = 0x2C;
                public const byte EXTERNAL_COUNTER_READ = 0x2D;
                public const byte EXTERNAL_COMPARATE_DATA_WRITE = 0x2E;
                public const byte EXTERNAL_COMPARATE_DATA_READ = 0x2F;
                public const byte byteERNAL_PRE_SCALE_DATA_WRITE = 0x30;
                public const byte byteERNAL_PRE_SCALE_DATA_READ = 0x31;
                public const byte EXTERNAL_PRE_SCALE_DATA_WRITE = 0x32;
                public const byte EXTERNAL_PRE_SCALE_DATA_READ = 0x33;
                public const byte CLEAR_SIGNAL_SELECT = 0x34;
                public const byte ONE_TIME_CLEAR_REQUEST = 0x35;
                public const byte FULL_TIME_CLEAR_REQUEST = 0x36;
                public const byte CLEAR_REQUEST_RESET = 0x37;
                public const byte REVERSE_COUNT_MODE_SET = 0x38;
                public const byte REVERSE_COUNT_MODE_RESET = 0x39;
                public const byte NO_OPERATION = 0x3A;
                public const byte STRAIGHT_ACCELERATE_MODE_SET = 0x84;
                public const byte US_STRAIGHT_ACCELERATE_MODE_SET = 0x85;
                public const byte S_CURVE_ACCELERATE_MODE_SET = 0x86;
                public const byte US_S_CURVE_ACCELERATE_MODE_SET = 0x87;
                public const byte SW1_DATA_WRITE = 0x88;
                public const byte SW1_DATA_READ = 0x89;
                public const byte SW2_DATA_WRITE = 0x8A;
                public const byte SW2_DATA_READ = 0x8B;
                public const byte SLOW_DOWN_LIMIT_ENABLE_MODE_SET = 0x8C;
                public const byte SLOW_DOWN_LIMIT_ENABLE_MODE_RESET = 0x8D;
                public const byte EMERGENCY_LIMIT_ENABLE_MODE_SET = 0x8E;
                public const byte EMERGENCY_LIMIT_ENABLE_MODE_RESET = 0x8F;
                public const byte INITIAL_CLEAR = 0x90;

                /**-----------------------------------------------------------------
                 * IO MAP
                 * ----------------------------------------------------------------*/
                public const short PORT_DATA1 = 0x00;
                public const short PORT_DATA2 = 0x01;
                public const short PORT_DATA3 = 0x02;
                public const short PORT_DATA4 = 0x03;
                public const short PORT_COMMAND = 0x04;
                public const short PORT_MODE1 = 0x05;
                public const short PORT_MODE2 = 0x06;
                public const short PORT_UNIVERSAL_SIGNAL = 0x07;
                public const short PORT_DRIVE_STATUS = 0x04;
                public const short PORT_END_STATUS = 0x05;
                public const short PORT_MECHANICAL_SIGNAL = 0x06;



                /**-----------------------------------------------------------------
                ' Misc
                '-----------------------------------------------------------------*/
                public const short SLOT_AUTO = -1;
                public const short MAX_SLOTS = 16;

                /*-----------------------------------------------------------------
                ' Resoucer information
                '-----------------------------------------------------------------*/
                public const int MAX_MEM = 9;
                public const int MAX_IO = 20;
                public const int MAX_IRQ = 7;
                public const int MAX_DMA = 7;
                public const int MAX_RSV = 3;


                /// <summary>
                /// バージョン番号を取得します．
                /// </summary>
                /// <param name="pdwDllVer">DLLバージョンを格納する領域のポインタ</param>
                /// <param name="pdwDrvVer">DRVバージョンを格納する領域へのポインタ</param>
                /// <returns>True: 成功, False: 失敗</returns>
                [DllImport("apci59.dll", EntryPoint = "Apci59GetVersion")]
                public static extern Boolean GetVersion(ref int pdwDllVer, ref int pdwDrvVer);

                /// <summary>
                /// デバイスの使用を宣言します．
                /// </summary>
                /// <param name="pwLogSlot">デバイスを格納する領域へのポインタ</param>
                /// <returns>成功ならばTrueを返し，　失敗ならばFalseを返します．</returns>
                [DllImport("apci59.dll", EntryPoint = "Apci59Create")]
                public static extern Boolean Create(ref short pwLogSlot);

                /// <summary>
                /// デバイスを解放します．
                /// </summary>
                /// <param name="wLogSlot">デバイス</param>
                /// <returns>成功ならばTrueを返し，　失敗ならばFalseを返します．</returns>
                [DllImport("apci59.dll", EntryPoint = "Apci59Close")]
                public static extern Boolean Close(short wLogSlot);

                /// <summary>
                /// リソース情報を取得します．
                /// </summary>
                /// <param name="wLogSlot">デバイス</param>
                /// <param name="pres">リソース情報を格納する領域へのポインタ</param>
                /// <returns></returns>
                [DllImport("apci59.dll", EntryPoint = "Apci59GetResouce")]
                public static extern Boolean GetResouce(short wLogSlot, ref Apci59Resouce pres);

                /// <summary>
                /// 読み込みを実行します．
                /// </summary>
                /// <param name="wLogSlot">デバイス</param>
                /// <param name="wAxis">対象制御軸</param>
                /// <param name="wPort">対象ポート番号</param>
                /// <param name="pbData">読み込んだデータを書くのする領域へのポインタ</param>
                /// <returns>成功ならばTrueを返し，　失敗ならばFalseを返します．</returns>
                [DllImport("apci59.dll", EntryPoint = "Apci59InPort")]
                public static extern Boolean InPort(short wLogSlot, short wAxis, short wPort, ref byte pbData);

                /// <summary>
                /// 書き込みを実行します．
                /// </summary>
                /// <param name="wLogSlot">デバイス</param>
                /// <param name="wAxis">対象制御軸</param>
                /// <param name="wPort">対象ポート番号</param>
                /// <param name="bData">書き込む値</param>
                /// <returns>成功ならばTrueを返し，　失敗ならばFalseを返します．</returns>
                [DllImport("apci59.dll", EntryPoint = "Apci59OutPort")]
                public static extern Boolean OutPort(short wLogSlot, short wAxis, short wPort, byte bData);

                /// <summary>
                /// エラーコードを取得します．
                /// </summary>
                /// <param name="wLogSlot">デバイス</param>
                /// <returns>エラーコード</returns>
                [DllImport("apci59.dll")]
                private static extern int Apci59GetLastError(short wLogSlot);

                /// <summary>
                /// ドライバステータスを取得します．
                /// </summary>
                /// <param name="wLogSlot">デバイス</param>
                /// <param name="xAxis">対象制御軸</param>
                /// <param name="pbStatus">情報を格納する領域へのポインタ</param>
                /// <returns>成功ならばTrueを返し，　失敗ならばFalseを返します．</returns>
                [DllImport("apci59.dll", EntryPoint = "Apci59GetDriveStatus")]
                public static extern Boolean GetDriveStatus(short wLogSlot, short xAxis, ref byte pbStatus);

                /// <summary>
                /// エンドステータスを取得します．
                /// </summary>
                /// <param name="wLogSlot">デバイス</param>
                /// <param name="xAxis">対象制御軸</param>
                /// <param name="pbStatus">情報を格納する領域へのポインタ</param>
                /// <returns>成功ならばTrueを返し，　失敗ならばFalseを返します．</returns>
                [DllImport("apci59.dll", EntryPoint = "Apci59GetEndStatus")]
                public static extern Boolean GetEndStatus(short wLogSlot, short xAxis, ref byte pbStatus);

                /// <summary>
                /// メカニカルシグナルを取得します．
                /// </summary>
                /// <param name="wLogSlot">デイバイス</param>
                /// <param name="wAxis">対象制御軸</param>
                /// <param name="pbStatus">情報を格納する領域へのポインタ</param>
                /// <returns>成功ならばTrueを返し，　失敗ならばFalseを返します．</returns>
                [DllImport("apci59.dll", EntryPoint = "Apci59GetMechanicalSignal")]
                public static extern Boolean GetMechanicalSignal(short wLogSlot, short wAxis, ref byte pbStatus);

                /// <summary>
                /// ユニバーサルシグナル取得
                /// </summary>
                /// <param name="wLogSlot">デバイス</param>
                /// <param name="wAxis">対象制御軸</param>
                /// <param name="pbSignal">情報を格納する領域へのポインタ</param>
                /// <returns>成功ならばTrueを返し，　失敗ならばFalseを返します．</returns>
                [DllImport("apci59.dll", EntryPoint = "Apci59GetUniversalSignal")]
                public static extern Boolean GetUniversalSignal(short wLogSlot, short wAxis, ref byte pbSignal);

                /// <summary>
                /// モード１に書き込みます．
                /// </summary>
                /// <param name="wLogSlot">デバイス</param>
                /// <param name="xAxis">対象制御軸</param>
                /// <param name="bMode">書き込むデータ</param>
                /// <returns>成功ならばTrueを返し，　失敗ならばFalseを返します．</returns>
                [DllImport("apci59.dll", EntryPoint = "Apci59Mode1Write")]
                public static extern Boolean Mode1Write(short wLogSlot, short xAxis, byte bMode);

                /// <summary>
                /// モード２に書き込みます
                /// </summary>
                /// <param name="wLogSlot">デバイス</param>
                /// <param name="xAxis">対象制御軸</param>
                /// <param name="bMode">書き込むデータ</param>
                /// <returns>成功ならばTrueを返し，　失敗ならばFalseを返します．</returns>
                [DllImport("apci59.dll", EntryPoint = "Apci59Mode2Write")]
                public static extern Boolean Mode2Write(short wLogSlot, short xAxis, byte bMode);

                /// <summary>
                /// ユニバーサルシグナルを書き込みます．
                /// </summary>
                /// <param name="wLogSlot">デバイス</param>
                /// <param name="xAxis">対象制御軸</param>
                /// <param name="bSignal">書き込むデータ</param>
                /// <returns>成功ならばTrueを返し，　失敗ならばFalseを返します．</returns>
                [DllImport("apci59.dll", EntryPoint = "Apci59UniversalSignalWrite")]
                public static extern Boolean UniversalSignalWrite(short wLogSlot, short xAxis, byte bSignal);

                /// <summary>
                /// 2バイトデータを読み込みます．
                /// </summary>
                /// <param name="wLogSlot">デバイス</param>
                /// <param name="xAxis">対象制御軸</param>
                /// <param name="bCmd">読み込み時に実行するコマンド</param>
                /// <param name="pwData">読み込んだデータを書くのする領域へのポインタ</param>
                /// <returns>成功ならばTrueを返し，　失敗ならばFalseを返します．</returns>
                [DllImport("apci59.dll", EntryPoint = "Apci59DataHalfRead")]
                public static extern Boolean DataHalfRead(short wLogSlot, short xAxis, byte bCmd, ref short pwData);

                /// <summary>
                /// 4バイトデータを読み込みます．
                /// </summary>
                /// <param name="wLogSlot">デバイス</param>
                /// <param name="xAxis">対象制御軸</param>
                /// <param name="bCmd">読み込み時に実行するコマンド</param>
                /// <param name="pwData">読み込んだデータを書くのする領域へのポインタ</param>
                /// <returns>成功ならばTrueを返し，　失敗ならばFalseを返します．</returns>
                [DllImport("apci59.dll", EntryPoint = "Apci59DataFullRead")]
                public static extern Boolean DataFullRead(short wLogSlot, short xAxis, byte bCmd, ref int pwData);

                /// <summary>
                /// コマンドを書き込みます．
                /// </summary>
                /// <param name="wLogSlot">デバイス</param>
                /// <param name="wAxis">対象制御軸</param>
                /// <param name="bCmd">コマンド</param>
                /// <returns>成功ならばTrueを返し，　失敗ならばFalseを返します．</returns>
                [DllImport("apci59.dll", EntryPoint = "Apci59CommandWrite")]
                public static extern Boolean CommandWrite(short wLogSlot, short wAxis, byte bCmd);

                /// <summary>
                /// 2バイトデータを書き込みます．
                /// </summary>
                /// <param name="wLogSlot">デバイス</param>
                /// <param name="wAxis">対象制御軸</param>
                /// <param name="bCmd">書き込むときに実行するコマンド</param>
                /// <param name="pwData">書き込むデータ</param>
                /// <returns>成功ならばTrueを返し，　失敗ならばFalseを返します．</returns>
                [DllImport("apci59.dll", EntryPoint = "Apci59DataHalfWrite")]
                public static extern Boolean DataHalfWrite(short wLogSlot, short wAxis, byte bCmd, short pwData);

                /// <summary>
                /// 2バイトデータを書き込みます．
                /// </summary>
                /// <param name="wLogSlot">デバイス</param>
                /// <param name="wAxis">対象制御軸</param>
                /// <param name="bCmd">書き込むときに実行するコマンド</param>
                /// <param name="pdwData">書き込むデータ</param>
                /// <returns>成功ならばTrueを返し，　失敗ならばFalseを返します．</returns>
                [DllImport("apci59.dll", EntryPoint = "Apci59DataFullWrite")]
                public static extern Boolean DataFullWrite(short wLogSlot, short wAxis, byte bCmd, int pdwData);

                /// <summary>
                /// BSNスイッチの値を読みこみます
                /// </summary>
                /// <param name="wLogSlot">デバイス</param>
                /// <param name="pdwSwitchValue">スイッチの値を格納する領域へのポインタ</param>
                /// <returns>成功ならばTrueを返し，　失敗ならばFalseを返します．</returns>
                [DllImport("apci59.dll", EntryPoint = "Apci59GetSwitchValue")]
                public static extern Boolean GetSwitchValue(short wLogSlot, ref long pdwSwitchValue);

                /// <summary>
                /// エラーコードを取得します．
                /// </summary>
                /// <param name="wLogSlot">デバイス</param>
                /// <returns>エラーコード</returns>
                public static ApciErrorCode GetLastError(short wLogSlot) {
                    return (ApciErrorCode)Apci59GetLastError(wLogSlot);
                }
            }
        }
    }
}