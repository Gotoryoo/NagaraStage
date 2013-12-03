using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace IO {
        namespace Driver {
            /// <summary>
            ///  
            /// </summary>
            /// <author>Hirokazu Yokoyama</author>
            class Apci54Resouce {
                /// <summary>
                /// メモリウィンドウ数
                /// </summary>
                public long dwNumMemWindows = 0;

                /// <summary>
                /// メモリウィンドウ開始アドレス
                /// </summary>
                public long[] dwMemBase = new long[Apci54.MAX_MEM];

                /// <summary>
                /// メモリウィンドウ長
                /// </summary>
                public long[] dwMemLength = new long[Apci54.MAX_MEM];

                /// <summary>
                /// メモリウィンドウ属性
                /// </summary>
                public long[] dwMemAttrib = new long[Apci54.MAX_MEM];

                /// <summary>
                /// I/Oポート数
                /// </summary>
                public long dwNumIOPorts = 0;

                /// <summary>
                /// I/Oポート開始アドレス
                /// </summary>
                public long[] dwIOPortBase = new long[Apci54.MAX_IO];

                /// <summary>
                /// I/Oポート長
                /// </summary>
                public long[] dwIOPortLength = new long[Apci54.MAX_IO];

                /// <summary>
                /// IRQ情報数
                /// </summary>
                public long dwNumIRQs = 0;

                /// <summary>
                /// IRQ番号
                /// </summary>
                public long[] dwIRQRegisters = new long[Apci54.MAX_IRQ];

                /// <summary>
                /// IRQ属性
                /// </summary>
                public long[] dwIRQAttrib = new long[Apci54.MAX_IRQ];

                /// <summary>
                /// DMAチャネル数
                /// </summary>
                public long dwNumDMAs = 0;

                /// <summary>
                /// DMAチャネル
                /// </summary>
                public long[] dwDMALast = new long[Apci54.MAX_DMA];

                /// <summary>
                /// DMAチャネル属性
                /// </summary>
                public long[] dwDMAAttrib = new long[Apci54.MAX_DMA];

                /// <summary>
                /// 予約
                /// </summary>
                public long[] dwReserved1 = new long[3];

            }
        }
    }
}