#pragma once

namespace NagaraStage{
    namespace ImageEnhancement {
        public enum class ImreadMode {
            /// <summary>
            /// 画像は，強制的に3チャンネルカラー画像として読み込まれます．
            /// </summary>
            Force3Color = 1,

            /// <summary>
            /// 画像は，強制的にグレースケール画像として読み込まれます．
            /// </summary>
            ForceGrayScale = 0,

            /// <summary>
            /// 画像は，そのままの画像として読み込まれます
            /// <para>
            /// (現在の実装では，アルファチャンネルがもしあったとしても，
            /// 出力画像からは取り除かれることに注意してください．
            /// 例えば，  の場合でも，4チャンネルRGBA画像はRGB画像として読み込まれます）．
            /// </para>
            /// </summary>
            Conservation = -1
        };
    }
}