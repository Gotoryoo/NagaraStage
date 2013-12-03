#include "CircumBox.h"

using namespace System::Collections::Generic;

namespace NagaraStage {
    namespace ImageEnhancement {
        /// <author mail="hirokazu.online@gmail.com">Hirokazu Yokoyama</author>
        public ref class Sort {

        public:
            /// <summary>
            /// 昇順に並べる通常のソートを行います．
            /// </summary>
            /// <param name="source">ソートする配列</param>
            /// <param name="start">ソートの開始地点</param>
            /// <param name="end">ソートの終了地点</param>
            static void SortNomal(array<int>^ source, int start, int end);

            /// <summary>
            /// 降順に並べる通常のソートを行います．
            /// </summary>
            /// <param name="source">ソートする配列</param>
            /// <param name="start">ソートの開始地点</param>
            /// <param name="end">ソートの終了地点</param>
            static void SortDescread(array<int>^ source, int start, int end);

            /// <summary>
            /// CircumBoxのリストを面積で昇順にソートします．
            /// </summary>
            /// <param name="boxes">ソートするCircumBoxのリスト</param>
            /// <param name="start">ソートの開始地点</param>
            /// <param name="end">ソートの終了地点</param>
            static void Sort::SortCircumBoxByArea(List<CircumBox^>^ boxes, int start, int end);

            /// <summary>
            /// CircumBoxのリストを面積で降順にソートします．
            /// </summary>
            /// <param name="boxes">ソートするCircumBoxのリスト</param>
            /// <param name="start">ソートの開始地点</param>
            /// <param name="end">ソートの終了地点</param>
            static void Sort::SortDesvreadCircumBoxByArea(List<CircumBox^>^ boxes, int start, int end);
        };
    }
}

