/**
 * @author Hirokazu Yokoyama <o1007410@edu.gifu-u.ac.jp>
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using NagaraStage.Activities;

namespace NagaraStage {
    /// <summary>
    /// libgel.dllをラッピングするためのクラスです．
    /// </summary>
    /// <author>Hirokazu Yokoyama</author>
    public class Gel {
        [DllImport("libgel.dll", EntryPoint="isInGel")]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool isInGel(
            IntPtr imageData, 
            int width, int height, 
            int startRow, int endRow,
            int lenghtOfSide, 
            int threshold0, int threshold1,
            double powerOfDifference);

        [DllImport("libgel.dll", EntryPoint="isInGelBrightness")]        
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool isInGel(
            IntPtr imageData, 
            int width, int height, 
            int startRow, int endRow,
            ref int brightness,
            int lenghtOfSide, 
            int threshold0, int threshold1,
            double powerOfDifference);

        /// <summary>
        /// 与えられた画像データがゲルの内部であるかどうかを取得します．
        /// <para>ただし，8bitグレースケール画像に限ります．</para>
        /// </summary>
        /// <param name="imageData">画像データの配列</param>
        /// <param name="width">画像の横幅(=画像のストライド)</param>
        /// <param name="height">画像の縦幅</param>
        /// <param name="startRow">判定に用いる範囲の開始行</param>
        /// <param name="endRow">判定に用いる範囲の終了行</param>
        /// <param name="lengthOfSize">輝度値の平均を行う単位</param>
        /// <param name="threshold0">輝度値の合計に対してゲル内かどうかを判定するためのしきい値</param>
        /// <param name="threshold1">輝度値の微分に対して二値化を行うためのしきい値，
        /// 0以下を指定した場合二値化が行われなくなります．</param>
        /// <param name="powerOfDifference">累乗する値</param>
        /// <returns>True: ゲルの内部である, False: ゲルの外部である</returns>
        public static Boolean IsInGel(
            byte[] imageData,
            int width, int height,
            int startRow, int endRow,
            int lengthOfSize = Surface.NumOfSidePixcelDefault,
            int threshold0 = Surface.BrightnessThresholdUTDefault,
            int threshold1 = -1,
            double powerOfDifference = Surface.PowerOfDifferenceDefault) {
            // ガベージコレクションで管理されている画像データの領域を固定してアンマ
            // ネージコードに渡す
                GCHandle gcH = GCHandle.Alloc(imageData, GCHandleType.Pinned);
                Boolean retVal = isInGel(
                    gcH.AddrOfPinnedObject(),
                    width, height,
                    startRow, endRow,
                    lengthOfSize,
                    threshold0, threshold1,
                    powerOfDifference);

                gcH.Free();
                imageData = null;
                return retVal;
        }

        /// <summary>
        /// 与えられた画像データがゲルの内部であるかどうかを取得します．
        /// <para>ただし，8bitグレースケール画像に限ります．</para>
        /// </summary>
        /// <param name="imageData">画像データの配列</param>
        /// <param name="width">画像の横幅(=画像のストライド)</param>
        /// <param name="height">画像の縦幅</param>
        /// <param name="startRow">判定に用いる範囲の開始行</param>
        /// <param name="endRow">判定に用いる範囲の終了行</param>
        /// <param name="brightness">指標となる輝度値を取得するポインタ</param>
        /// <param name="lengthOfSize">輝度値の平均を行う単位</param>
        /// <param name="threshold0">輝度値の合計に対してゲル内かどうかを判定するためのしきい値</param>
        /// <param name="threshold1">輝度値の微分に対して二値化を行うためのしきい値，
        /// 0以下を指定した場合二値化が行われなくなります．</param>
        /// <param name="powerOfDifference">累乗する値</param>
        /// <returns>True: ゲルの内部である, False: ゲルの外部である</returns>
        public static Boolean IsInGel(
            byte[] imageData,
            int width, int height,
            int startRow, int endRow,
            ref int brightness,
            int lengthOfSize = Surface.NumOfSidePixcelDefault,
            int threshold0 = Surface.BrightnessThresholdUTDefault,
            int threshold1 = -1,
            double powerOfDifference = Surface.PowerOfDifferenceDefault) {
            // ガベージコレクションで管理されている画像データの領域を固定してアンマ
            // ネージコードに渡す
                GCHandle gcH = GCHandle.Alloc(imageData, GCHandleType.Pinned);
            
                Boolean retVal = isInGel(
                    gcH.AddrOfPinnedObject(),
                    width, height,
                    startRow, endRow,
                    ref brightness,
                    lengthOfSize,
                    threshold0,
                    threshold1,
                    powerOfDifference);
            
                gcH.Free();
                imageData = null;
                return retVal;
        }
    }
}
