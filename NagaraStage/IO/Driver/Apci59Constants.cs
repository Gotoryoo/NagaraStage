/**
 * @author Hirokazu Yokoyama <o1007410@edu.gifu-u.ac.jp>
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage.IO.Driver {
    /// <summary>
    /// Apci59とのインターフェイスに用いる値，MotorControlerのための定数などを定義したクラスです．
    /// </summary>
    public class Apci59Constants {
        /*-----------------------------------------
         * I / O アドレス 
        -----------------------------------------*/

        public const int DATA1_WRITE = 0x0;
        public const int DATA2_WRITE = 0x1;
        public const int DATA3_WRITE = 0x2;
        public const int DATA4_WRITE = 0x3;
        public const int COMMAND_WRITE = 0x4;
        public const int MODE1_WRITE = 0x5;
        public const int MODE2_WRITE = 0x6;
        public const int UNIVERSAL_SIGNAL_WRITE = 0x7;

        public const int DATA1_READ = 0x0;
        public const int DATA2_READ = 0x1;
        public const int DATA3_READ = 0x2;
        public const int DATA4_READ = 0x3;
        public const int DRIVE_STATUS_READ = 0x4;
        public const int END_STATUS_READ = 0x5;
        public const int MECHANICAL_SIGNAL_READ = 0x6;
        public const int UNIVERSAL_SIGNAL_READ = 0x7;

        /*-----------------------------------
         * コマンド
         * ---------------------------------*/
        public const int RANGE_WRITE = 0x0;
        public const int RANGE_READ = 0x1;
        public const int START_STOP_SPEED_DATA_WRITE = 0x2;
        public const int START_STOP_SPEED_DATA_READ = 0x3;
        public const int OBJECT_SPEED_DATA_WRITE = 0x4;
        public const int OBJECT_SPEED_DATA_READ = 0x5;
        public const int RATE1_DATA_WRITE = 0x6;
        public const int RATE1_DATA_READ = 0x7;
        public const int RATE2_DATA_WRITE = 0x8;
        public const int RATE2_DATA_READ = 0x9;
        public const int RATE3_DATA_WRITE = 0xA;
        public const int RATE3_DATA_READ = 0xB;
        public const int RATE_CHANGE_POINT_1_2_WRITE = 0xC;
        public const int RATE_CHANGE_POINT_1_2_READ = 0xD;
        public const int RATE_CHANGE_POINT_2_3_WRITE = 0xE;
        public const int RATE_CHANGE_POINT_2_3_READ = 0xF;
        public const int SLOW_DOWN_REAR_PULSE_WRITE = 0x10;
        public const int SLOW_DOWN_REAR_PULSE_READ = 0x11;
        public const int NOW_SPEED_DATA_READ = 0x12;
        public const int DRIVE_PULSE_COUNTER_READ = 0x13;
        public const int PRESET_PULSE_DATA_OVERRIDE = 0x14;
        public const int PRESET_PULSE_DATA_READ = 0x15;
        public const int DEVIATION_DATA_READ = 0x16;
        public const int NO_OPRATION = 0x17;
        public const int INPOSITION_WAIT_MODE_SET = 0x18;
        public const int INPOSITION_WAIT_MODE_RESET = 0x19;
        public const int ALARM_STOP_ENABLE_MODE_SET = 0x1A;
        public const int ALARM_STOP_ENABLE_MODE_RESET = 0x1B;
        public const int INTERRUPT_OUT_ENABLE_MODE_SET = 0x1C;
        public const int INTERRUPT_OUT_ENABLE_MODE_RESET = 0x1D;
        public const int SLOW_DOWN_STOP = 0x1E;
        public const int EMERGENCY_STOP = 0x1F;
        public const int PLUS_PRESET_PULSE_DRIVE = 0x20;
        public const int MINUS_PRESET_PULSE_DRIVE = 0x21;
        public const int PLUS_CONTINUOUS_DRIVE = 0x22;
        public const int MINUS_CONTINUOUS_DRIVE = 0x23;
        public const int PLUS_SIGNAL_SEARCH1_DRIVE = 0x24;
        public const int MINUS_SIGNAL_SEARCH1_DRIVE = 0x25;
        public const int PLUS_SIGNAL_SEARCH2_DRIVE = 0x26;
        public const int MINUS_SIGNAL_SEARCH2_DRIVE = 0x27;
        public const int INTERNAL_COUNTER_WRITE = 0x28;
        public const int INTERNAL_COUNTER_READ = 0x29;
        public const int INTERNAL_COMPARATE_DATA_WRITE = 0x2A;
        public const int INTERNAL_COMPARATE_DATA_READ = 0x2B;
        public const int EXTERNAL_COUNTER_WRITE = 0x2C;
        public const int EXTERNAL_COUNTER_READ = 0x2D;
        public const int EXTERNAL_COMPARATE_DATA_WRITE = 0x2E;
        public const int EXTERNAL_COMPARATE_DATA_READ = 0x2F;

        /// <summary>DIDOボードトップアドレス</summary>
        public const int PIO_280 = 0x0;

        /// <summary>速度表示ｱﾄﾞﾚｽ</summary>
        public const int PIO_SPLED_Disp = 0x0;

        /// <summary>ﾄﾞﾗｲﾊﾞﾁｯﾌﾟｾﾚｸﾄ(DIPSW選択)ｱﾄﾞﾚｽ</summary>
        public const int PIO_DRV_CS = 0x1;

        /// <summary>速度軸ｽｲｯﾁｱﾄﾞﾚｽ</summary>
        public const int PIO_SP_AXIS = 0x2;

        /// <summary>運転指定ｽｲｯﾁｱﾄﾞﾚｽ</summary>
        public const int PIO_DRV_SW = 0x3;

        /// <summary>ﾎﾟｰﾄﾃﾞｨﾚｸｼｮﾝ指定ｱﾄﾞﾚｽ</summary>
        public const int PIO_DIR = 0x6;

        /// <summary>速度１</summary>
        public const int sp1 = 0x1;

        /// <summary>速度２</summary>
        public const int sp2 = 0x2;

        /// <summary>速度３</summary>
        public const int sp3 = 0x4;

        /// <summary>速度４</summary>
        public const int sp4 = 0x8;

        /// <summary>スイッチ状態ステータスフラグ</summary>
        public const int idle_st = 0;

        /// <summary>スイッチ状態ステータスフラグ</summary>
        //public const int move_st = 0x80;

        /// <summary>スイッチ状態ステータスフラグ</summary>
        public const int move_st = 0x2;

        /// <summary>スイッチ状態ステータスフラグ</summary>
        public const int hold_st = 0xFF;

        /// <summary>30ミクロン</summary>
        public const double Marginxy = 0.03;

        /// <summary>35ミクロン</summary>
        public const double Marginxyp = 0.035;
    }
}
