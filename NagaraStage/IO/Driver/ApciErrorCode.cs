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
            public enum ApciErrorCode {
                /// <summary>異状なし（正常終了）</summary>
                SUCCESS = 0,

                /// <summary>システムエラー</summary>
                ERR_SYSTEM = 1,

                /// <summary>使用可能なデバイスがありません</summary>
                ERR_NO_DEVICE = 2,

                /// <summary>指定のデバイスは使用中です</summary>
                ERR_IN_USE = 3,

                /// <summary>無効な論理スロットです</summary>
                ERR_INVALID_SLOT = 4,

                /// <summary>リソースエラー</summary>
                ERR_RESOURCE = 5,

                /// <summary>不正なポートを要求した</summary>
                ERR_INVALID_PORT = 6,

                /// <summary>不正な引数を要求しました</summary>
                ERR_INVALID_ARGUMENT = 7,

                /// <summary>無効な制御軸を要求した</summary>
                ERR_INVALID_AXIS = 50
            }
        }
    }
}