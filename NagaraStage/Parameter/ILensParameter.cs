using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage.Parameter {
    /// <summary>
    /// レンズのパラメータを管理するクラスが実装すべきインターフェイスです．
    /// </summary>
    public interface ILensParameter {
        /// <summary>
        /// 使用するレンズの倍率を取得，または設定します．
        /// <para>倍率を設定した場合，その倍率レンズのプロファイルに切り替わります．</para>
        /// </summary>
        double Magnification {
            get;
            set;
        }

        /// <summary>
        /// X方向の解像度を取得します．
        /// </summary>
        double CcdResolutionX {
            get;
        }

        /// <summary>
        /// Y方向の解像度を取得します．
        /// </summary>
        double CcdResolutionY {
            get;
        }

        /// <summary>
        /// X方向の1ピクセルあたりの長さ(mm)を取得します．
        /// </summary>
        double ImageLengthX {
            get;
        }

        /// <summary>
        /// Y方向の１ピクセルあたりの長さ(mm)を取得します．
        /// </summary>
        double ImageLengthY {
            get;
        }

        /// <summary>
        /// X方向のオフセット値(mm)を取得します．
        /// </summary>
        double LensOffsetX {
            get;
        }

        /// <summary>
        /// Y方向のオフセット値(mm)を取得します．
        /// </summary>        
        double LensOffsetY {
            get;
        }

        /// <summary>
        /// X方向にらせん移動するときの移動単位を取得します．
        /// </summary>
        double SpiralShiftX {
            get;
        }

        /// <summary>
        /// Y方向にらせん移動するときの移動単位を取得します．
        /// </summary>
        double SpiralShiftY {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        double LensZStep {
            get;
        }

        /// <summary>
        /// グリッドマークの大きさを取得します．
        /// </summary>
        double GridMarkSize {
            get;
        }

        /// <summary>
        /// レンズ毎の設定されたLEDの明るさ調整のための値を取得します．
        /// </summary>
        int LedParameter {
            get;
        }

        /// <summary>
        /// インスタンスが管理しているレンズのプロファイルの数を取得します．
        /// </summary>
        int NumOfLens {
            get;
        }

        /// <summary>
        /// 指定された倍率のレンズのX方向における解像度を取得します．
        /// </summary>
        /// <param name="lensType">レンズの倍率</param>
        /// <returns>X方向の解像度</returns>
        double GetCcdResoulutionX(double lensType);

        /// <summary>
        /// 指定された倍率のレンズのY方向における解像度を取得します．
        /// </summary>
        /// <param name="lensType">レンズの倍率</param>
        /// <returns>Y方向の解像度</returns>
        double GetCcdResoulutionY(double lensType);

        /// <summary>
        /// 指定された倍率のレンズのX方向における1ピクセルあたりの長さ(mm)を取得します．
        /// </summary>
        /// <param name="lensType">レンズの倍率</param>
        /// <returns>1ピクセルあたり長さ(mm)</returns>
        double GetImageLengthX(double lensType);

        /// <summary>
        /// 指定された倍率のレンズのY方向における1ピクセルあたりの長さ(mm)を取得します．
        /// </summary>
        /// <param name="lensType">レンズの倍率</param>
        /// <returns>1ピクセルあたり長さ(mm)</returns>
        double GetImageLengthY(double lensType);

        /// <summary>
        /// 指定された倍率のレンズのX方向におけるオフセット値を取得します．
        /// </summary>
        /// <param name="lensType">レンズの倍率</param>
        /// <returns>オフセット値</returns>
        double GetLensOffsetX(double lensType);

        /// <summary>
        /// 指定された倍率のレンズのY方向におけるオフセット値を取得します．
        /// </summary>
        /// <param name="lensType">レンズの倍率</param>
        /// <returns>オフセット値</returns>
        double GetLensOffsetY(double lensType);

        /// <summary>
        /// 指定された倍率のレンズのX方向におけるらせん移動の移動単位を取得します．
        /// </summary>
        /// <param name="lensType">レンズの倍率</param>
        /// <returns>移動単位</returns>
        double GetSpiralShiftX(double lensType);

        /// <summary>
        /// 指定された倍率のレンズのY方向におけるらせん移動の移動単位を取得します．
        /// </summary>
        /// <param name="lensType">レンズの倍率</param>
        /// <returns>移動単位</returns>
        double GetSpiralShiftY(double lensType);

        /// <summary>
        /// Gets the lens Z step.
        /// </summary>
        /// <param name="lensType">Type of the lens.</param>
        /// <returns></returns>
        double GetLensZStep(double lensType);

        /// <summary>
        /// 指定された倍率のレンズのグリッドマークの大きさを取得します．
        /// </summary>
        /// <param name="lensType">レンズの倍率.</param>
        /// <returns>グリッドマークの大きさ</returns>
        double GetGridMarkSize(double lensType);

        /// <summary>
        /// 指定された倍率のレンズのLEDの明るさ調整のための値を取得します．
        /// </summary>
        /// <param name="lensType">レンズの倍率</param>
        /// <returns>LEDのための値</returns>
        int GetLedParameter(double lensType);

        /// <summary>
        /// インスタンスが管理しているレンズの倍率一覧を取得します．
        /// </summary>
        /// <returns>レンズの倍率一覧</returns>
        double[] GetLensMagList();
    }
}
