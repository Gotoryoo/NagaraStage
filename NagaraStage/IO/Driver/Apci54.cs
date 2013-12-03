using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace NagaraStage {
    namespace IO {
        namespace Driver {
            /// <summary>
            /// apci54.dllをインポートしラッピングを行うクラスです.
            /// apci54.dllがもつ関数を静的なメソッドとして提供します．
            /// </summary>
            /// <author email="hirokazu.online@gmail.com">Hirokazu Yokoyama</author>
            /// <date>2012-06-28</date>
            static class Apci54 {
                /**-----------------------------------------------------------------
                * Error code
                *-----------------------------------------------------------------*/
                /// <summary>異状なし（正常終了）</summary>
                public const int SUCCESS = 0;

                /// <summary>システムエラー</summary>
                public const int ERR_SYSTEM = 1;

                /// <summary>使用可能なデバイスがありません</summary>
                public const int ERR_NO_DEVICE = 2;

                /// <summary>指定のデバイスは使用中です</summary>
                public const int ERR_IN_USE = 3;

                /// <summary>無効な論理スロットです</summary>
                public const int ERR_INVALID_SLOT = 4;

                /// <summary>リソースエラー</summary>
                public const int ERR_RESOURCE = 5;

                /// <summary>不正なポートを要求した</summary>
                public const int ERR_INVALID_PORT = 6;

                /// <summary>不正な引数を要求しました</summary>
                public const int ERR_INVALID_ARGUMENT = 7;

                /// <summary>無効な制御軸を要求した</summary>
                public const int ERR_INVALID_AXIS = 50;

                /**-----------------------------------------------------------------
                ' 方向
                '-----------------------------------------------------------------*/
                public const int DIR_A_OUTPUT = 0x0;
                public const int DIR_A_INPUT = 0x1;
                public const int DIR_B_OUTPUT = 0x0;
                public const int DIR_B_INPUT = 0x2;
                public const int DIR_C_OUTPUT = 0x0;
                public const int DIR_C_INPUT = 0x4;
                public const int DIR_D_OUTPUT = 0x0;
                public const int DIR_D_INPUT = 0x8;
                public const int DIR_E_OUTPUT = 0x0;
                public const int DIR_E_INPUT = 0x10;
                public const int DIR_F_OUTPUT = 0x0;
                public const int DIR_F_INPUT = 0x20;

                /**-----------------------------------------------------------------
                ' 論理ポート
                '-----------------------------------------------------------------*/
                public const int PortA = 0;
                public const int PortB = 1;
                public const int PortC = 2;
                public const int PortD = 3;
                public const int PortE = 4;
                public const int PortF = 5;

                /*-----------------------------------------------------------------
                ' その他定数
                '-----------------------------------------------------------------*/
                /// <summary>サポートするボード枚数</summary>
                public const int MaxSlots = 16;

                /// <summary>アクセス可能なポート数</summary>
                public const int MaxPorts = 6;

                public const int SlotAuto = 0xFFFF;

                /*-----------------------------------------------------------------
                ' Resoucer information
                '-----------------------------------------------------------------*/
                public const int MAX_MEM = 9;
                public const int MAX_IO = 20;
                public const int MAX_IRQ = 7;
                public const int MAX_DMA = 7;



                [DllImport("apci54.dll", EntryPoint = "Apci54GetVersion")]
                public static extern long GetVersion(ref int pdwDllVer, ref int pdwDrvVer);

                [DllImport("aPCI54.dll", EntryPoint = "Apci54Create")]
                public static extern long Create(int pwLogSlot);

                [DllImport("apci54.dll", EntryPoint = "Apci54Close")]
                public static extern long Close(int wLogSlot);

                [DllImport("apci54.dll", EntryPoint = "Apci54GetResouce")]
                public static extern long GetResouce(int wLogSlot, ref Apci54Resouce pres);

                [DllImport("apci54.dll", EntryPoint = "Apci54GetSwitchValue")]
                public static extern long GetSwitchValue(int wLogSlot, ref long pdwSwitchValue);

                [DllImport("apci54.dll", EntryPoint = "ApciGetDirections")]
                public static extern long GetDirections(int wLogSlot, long pdwDirections);

                [DllImport("apci54.dll", EntryPoint = "ApciSetDirections")]
                public static extern long SetDirections(int wLogSlot, long pdwDirections);

                [DllImport("apci54.dll", EntryPoint = "Apci54InPort")]
                public static extern long InPort(int wLogSlot, long wPort, byte pbData);

                [DllImport("apci54.dll", EntryPoint = "Apci54OutPort")]
                public static extern long OutPort(int wLogSlot, long swLogPort, byte bData);

                [DllImport("apci54.dll", EntryPoint = "Apci54GetLastError")]
                public static extern long GetLastError(long wLogSlot);
            }
        }
    }
}