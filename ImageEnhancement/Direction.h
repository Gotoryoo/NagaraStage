#pragma once

namespace NagaraStage{ 
    namespace ImageEnhancement {
        /// <summary>
        /// 方向を定義した列挙体です．
        /// </summary>
        public enum class Direction {            
            Null = 0,
            Over = 1,
            Right = 2,
            Under = 4,
            Left = 8,
            RightOver = Over + Right,
            RightUnder = Under + Right,
            LeftUnder = Under + Left,
            LeftOver = Over + Left
        };
    }
}