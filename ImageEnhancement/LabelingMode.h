#pragma once

namespace ImageEnhancement {
    /// <summary>
    /// ラベリングのモードを定義した列挙体です．
    /// </summary>
    public enum LabelingMode {
        /// <summary>
        /// 縦横の4連結
        /// </summary>
        Attachment4 = 0,

        /// <summary>
        /// 縦横斜めの8連結
        /// </summary>
        Attachment8 = 1
    };
}