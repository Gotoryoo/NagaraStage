using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace IO {
        /// <summary>
        /// カメラからの映像に対して分析などを行うipt.dllの関数への媒介となるメソッドを
        /// 提供する持つクラスです.
        /// <para>
        /// 旧来の映像分析ライブラリであるipt.dllはカメラのAPIに直接アクセスして画像を
        /// 取得して、分析を行なっています.したがってCameraクラスのスレッドと同期しなけ
        /// れば１つのカメラに複数のスレッドからアクセスする現象が発生してしまいかねま
        /// せん.このクラスはCameraクラスと同期を取りつつ、ipt.dllの関数にアクセスします.
        /// </para>
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public class CameraUtil {

            /// <summary>
            /// 引数sizeの円を撮影中の映像から検出して、その座標を取得します.
            /// <para>X10レンズにてグリッドマークの検出に用いいます.</para>
            /// </summary>
            /// <param name="x">X座標を格納する変数</param>
            /// <param name="y">Y座標を格納する変数</param>
            /// <param name="size">検出する円のサイズ</param>
            /// <returns>Ipt.MarkCenterの戻り値に準拠</returns>
            public int MarkCenter(ref double x, ref double y, int size) {
                Camera camera = Camera.GetInstance();
                bool isStart = camera.IsRunning;
                camera.Stop();
                int retval = Ipt.MarkCenter(ref x, ref y, size);
                if (isStart) {
                    camera.Start();
                }
                return retval;
            }
        }
    }
}