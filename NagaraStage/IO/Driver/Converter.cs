using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NagaraStage;
using NagaraStage.Parameter;

namespace NagaraStage.IO.Driver {

    /// <summary>
    /// モータドライバAPIへのアクセスを行うメソッドを持つ静的クラスです．
    /// </summary>
    /// <author>Hirokazu Yokoyama</author>
    static class Converter {
        /*
        /// <summary>
        /// APCI-P54 から読み出します．
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static byte IODRV0_Inpb(long address) {
            byte data = new byte();
            Apci54.InPort(ApciIp54.SlotNo, (address & 0xFFFF), data);

            return data;
        }


        /// <summary>
        /// APCI-P54 に書き出します．
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        public static void IODRV0_Outpb(long address, byte data) {
            Boolean status = ApciIp54.ExistanceP54;

            if (status) {
                switch (address) {
                    case 6:
                        Apci54.SetDirections(ApciIp54.SlotNo, data);
                        break;
                    default:
                        long val = Apci54.OutPort(ApciIp54.SlotNo, (address & 0xFFFF), data);
                        status = (val == 0 ? false : true);
                        break;
                } // switch end
            } // if end
        }
        */
        /// <summary>
        /// APCI-P59 から読み出します．
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static byte IODRV1_INpb(short address) {
            byte data = new byte();
            short axis = (short)(address / 8);

            Apci59.InPort(ApciM59.SlotNo, axis, (short)(address & 0x07), ref data);

            return data;
        }

        /// <summary>
        /// APCI-59 に書き込みます．
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        public static Boolean IODRV1_Outpb(long address, byte data) {
            short axis = (short)Math.Floor(address / 8.0);
            return Apci59.OutPort(ApciM59.SlotNo, axis, (short)(address & 0x07), data);
        }

        /// <summary>
        /// モータの速度を設定します．
        /// </summary>
        /// <param name="speed">速度</param>
        /// <param name="axis">方向</param>
        /// <param name="param">ソフトウェア全体を通して用いるParameterManagerインスタンス</param>
        public static void SetMotorSpeed(double speed, VectorId axis, ParameterManager param) {
            double oSpddt = speed;
            double sSpddt;
            short spdt;
            short oSdt;
            double at;
            double dt;
            double resolution = 0;
            double f = 0;
            double range = 0;
            short rangeDifferenceTime = 0;

            // Range Dataの設定
            resolution = param.MotorResolution.Index(axis);
            f = oSpddt / resolution;
            range = (4000000 / f) + 0.5;
            if (range > 8191) {
                rangeDifferenceTime = (short)range;
            } else if (range < 1) {
                rangeDifferenceTime = 1;
            }
            ApciM59.SetRange(axis, rangeDifferenceTime);

            // START_STOP_SPEED_DATAの作成
            sSpddt = param.MotorInitialVelocity.Index(axis);
            spdt = (short)((int)(((sSpddt / resolution) / (500 / rangeDifferenceTime)) + 0.5) & 0x1FFF);
            ApciM59.SetStartStopSpeed(axis, spdt);

            // 速度を設定(ObjectSpeed)
            oSdt = (short)((int)(((oSpddt / resolution) / (500 / rangeDifferenceTime)) + 0.5) & 0x1FFF);
            ApciM59.SetObjectSpeed(axis, oSdt);

            // RATE1_DATAの作成
            if ((oSdt - spdt) > 1) {
                at = (oSpddt - sSpddt) / param.MotorAccelTime.Index(axis);
                dt = (2048000 / (oSdt - spdt)) * at;
                dt = (dt > 8191 ? 8191 : dt);
            } else {
                dt = 8191;
            }

            ApciM59.SetRate1(axis, (short)dt);
        }

        public static double GetNowLocation(VectorId axis, ParameterManager param) {
            int counter = new int();

            Apci59.DataFullRead(ApciM59.SlotNo, (short)axis, Apci59.EXTERNAL_COUNTER_READ, ref counter);
            counter = ((counter & 0x8000000) > 0 ? counter | 0xF000000 : counter);
            return counter * param.EncoderResolution.Index(axis);
        }
    }

}