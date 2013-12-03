using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NagaraStage {
    namespace IO {
        namespace Driver {
            static class ApciIp54 {
                public const string Param54File = "p54.prm";
                private static Boolean enabled = false;
                private static int slotNo = 0;

                private static Boolean existanceP54 = false;

                public static Boolean Enabled {
                    get { return enabled; }
                }

                public static int SlotNo {
                    get { return slotNo; }
                    set { slotNo = value; }
                }

                public static Boolean ExistanceP54 {
                    get { return existanceP54; }
                }

                public static Boolean Initialize() {
                    Boolean status;
                    long sts;

                    Read54Parameter();
                    status = (!existanceP54 ? true : false);

                    // スロット番号を自動検索
                    slotNo = Apci54.SlotAuto;

                    try {
                        sts = Apci54.Create(slotNo);
                        status = (sts != 0 ? true : false);

                    } catch (Exception ex) {
                        status = false;
                        throw ex;
                    }

                    return status;
                }

                public static void Terminate() {
                    if (enabled) {
                        Apci54.Close(slotNo);
                        enabled = false;
                    }
                }

                /// <summary>
                /// パラメータをファイルから読み込みます．
                /// <para>読み込んだ値はExistanceP54プロパティから取得できます．</para>
                /// </summary>
                public static void Read54Parameter() {
                    StreamReader sr = File.OpenText(Param54File);
                    int id = int.Parse(sr.ReadLine());
                    existanceP54 = (id == 0 ? false : true);
                }
            }
        }
    }
}