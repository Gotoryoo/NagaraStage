using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace NagaraStage {
    namespace IO {
        namespace Driver {
            /// <summary>
            /// aPCI-59のResouce構造体のラッピングです．
            /// </summary>
            /// <author>Hirokazu Yokoyama</author>
            [StructLayout(LayoutKind.Sequential)]
            public struct Apci59Resouce {
                /// <summary>
                /// メモリウィンドウ数
                /// </summary>
                public int dwNumMemWindows;

                /// <summary>
                /// メモリウィンドウ開始アドレス
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = Apci59.MAX_MEM)]
                public int[] dwMemBase;

                /// <summary>
                /// メモリウィンドウ長
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = Apci59.MAX_MEM)]
                public int[] dwMemLength;

                /// <summary>
                /// メモリウィンドウ属性
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = Apci59.MAX_MEM)]
                public int[] dwMemAttrib;

                /// <summary>
                /// I/Oポート数
                /// </summary>
                public int dwNumIOPorts;

                /// <summary>
                /// I/Oポート開始アドレス
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = Apci59.MAX_IO)]
                public int[] dwIOPortBase;

                /// <summary>
                /// I/Oポート長
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = Apci59.MAX_IO)]
                public int[] dwIOPortLength;

                /// <summary>
                /// IRQ情報数
                /// </summary>
                public int dwNumIRQs;

                /// <summary>
                /// IRQ番号
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = Apci59.MAX_IRQ)]
                public int[] dwIRQRegisters;

                /// <summary>
                /// IRQ属性
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = Apci59.MAX_IRQ)]
                public int[] dwIRQAttrib;

                /// <summary>
                /// DMAチャネル数
                /// </summary>
                public int dwNumDMAs;

                /// <summary>
                /// DMAチャネル
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = Apci59.MAX_DMA)]
                public int[] dwDMALast;

                /// <summary>
                /// DMAチャネル属性
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = Apci59.MAX_DMA)]
                public int[] dwDMAAttrib;

                /// <summary>
                /// 予約
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = Apci59.MAX_RSV)]
                public int[] dwReserved1;

            }
        }
    }
}