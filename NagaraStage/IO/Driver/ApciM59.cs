using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace IO {
        namespace Driver {
            static class ApciM59 {

                private static Boolean enabled = false;
                private static short slotNo = 0;

                public static Boolean Enabled {
                    get { return enabled; }
                }

                public static short SlotNo {
                    get { return slotNo; }
                }

                public static Boolean Initialize() {
                    Boolean status;

                    try {
                        slotNo = Apci59.SLOT_AUTO;
                        status = Apci59.Create(ref slotNo);
                    } catch (Exception ex) {
                        status = false;
                        throw ex;
                    }

                    return status;
                }

                public static void Terminate() {
                    if (enabled) {
                        Apci59.Close(slotNo);
                        enabled = false;
                    }
                }

                public static Boolean SetMode1(VectorId axisNo, byte value) {
                    return Apci59.Mode1Write(slotNo, (short)axisNo, value);
                }

                public static Boolean SetMode2(VectorId axisNo, byte value) {
                    return Apci59.Mode2Write(slotNo, (short)axisNo, value);
                }

                public static Boolean SetCounter(VectorId axisNo, int counter) {
                    return Apci59.DataFullRead(slotNo, (short)axisNo, Apci59.INTERNAL_COUNTER_READ, ref counter);
                }

                public static Boolean GoOrigin(VectorId axisNo) {
                    Boolean status = false;
                    byte endStatus = new byte();

                    status = Apci59.DataFullWrite(slotNo, (short)axisNo, Apci59.MINUS_SIGNAL_SEARCH1_DRIVE, 0);
                    Apci59.GetEndStatus(slotNo, (short)axisNo, ref endStatus);

                    return status;
                }

                public static Boolean WriteCommand(VectorId axisNo, byte command) {
                    return Apci59.CommandWrite(slotNo, (short)axisNo, command);
                }

                public static long GetObjectSpeed(VectorId axisNo) {
                    short speed = new short();
                    Apci59.DataHalfRead(slotNo, (short)axisNo, Apci59.OBJECT_SPEED_DATA_READ, ref speed);
                    return speed;
                }

                public static Boolean SetObjectSpeed(VectorId axisNo, short speed) {
                    return Apci59.DataHalfWrite(slotNo, (short)axisNo, Apci59.OBJECT_SPEED_DATA_WRITE, speed);
                }

                public static Boolean SetRange(VectorId axis, short range) {
                    Boolean status;
                    if (range >= 1 & range <= 8192) {
                        status = Apci59.DataHalfWrite(slotNo, (short)axis, Apci59.RANGE_DATA_WRITE, range);
                    } else {
                        status = false;
                    }

                    return status;
                }

                public static Boolean SetStartStopSpeed(VectorId axisNo, short speed) {
                    Boolean status;
                    if (speed >= 1 & speed <= 8192) {
                        status = Apci59.DataHalfWrite(slotNo, (short)axisNo, Apci59.START_STOP_SPEED_DATA_WRITE, speed);
                    } else {
                        status = false;
                    }

                    return status;
                }

                public static Boolean SetRate1(VectorId axisNo, short rate) {
                    Boolean status;
                    if (rate >= 1 & rate <= 8192) {
                        status = Apci59.DataHalfWrite(slotNo, (short)axisNo, Apci59.RATE1_DATA_WRITE, rate);
                    } else {
                        status = false;
                    }

                    return status;
                }

                public static Boolean PresetPulseDrive(VectorId axisNo, int pulse) {
                    Boolean status;
                    if (pulse >= 0 & pulse <= 16777215) {
                        status = Apci59.DataFullWrite(slotNo, (short)axisNo, Apci59.PLUS_PRESET_PULSE_DRIVE, pulse);
                    } else {
                        status = false;
                    }

                    return status;
                }

                public static Boolean MinusPresetPulseDrive(VectorId axisNo, int pulse) {
                    Boolean status;
                    if (pulse >= 0 & pulse <= 16777215) {
                        status = Apci59.DataFullWrite(slotNo, (short)axisNo, Apci59.MINUS_PRESET_PULSE_DRIVE, pulse);
                    } else {
                        status = false;
                    }

                    return status;
                }
            }
        }
    }
}