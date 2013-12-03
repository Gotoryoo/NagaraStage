using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace NagaraStage {

    /// <summary>
    /// エマルションの画像処理や補助を行うメソッドを持つクラスです．
    /// </summary>
    /// <author>Hirokazu Yokoyama</author>
    public class ImageUtility {
        /// <summary>
        /// 画像データを保存します．
        /// </summary>
        /// <param name="image">保存する画像ソース</param>
        /// <param name="fileName">保存先</param>
        /// <exception cref="System.ArgumentException">ファイルの拡張子が対応していない場合</exception>
        public static void Save(Image image, string fileName) {
            var bitmap = new RenderTargetBitmap(
                (int)image.Width, (int)image.Height,
                96, 96,
                PixelFormats.Default);
            bitmap.Render(image);

            string extension = Path.GetExtension(fileName);
            BitmapEncoder encorder = null;
            switch (extension) {
                case ".jpeg":
                case ".jpg":
                    encorder = new JpegBitmapEncoder();
                    break;
                case ".png":
                    encorder = new PngBitmapEncoder();
                    break;
                case "bmp":
                    encorder = new BmpBitmapEncoder();
                    break;
                default:
                    throw new ArgumentException(Properties.Strings.FileExtensionNotAvairable);
            }
            encorder.Frames.Add(BitmapFrame.Create(bitmap));
            System.IO.FileStream stream = new FileStream(fileName, FileMode.Create);
            encorder.Save(stream);
            stream.Close();
        }

        /// <param name="image">保存する画像ソース</param>
        /// <param name="fileName">保存先</param>        
        public static void Save(BitmapSource image, string fileName) {
            string extension = Path.GetExtension(fileName);
            BitmapEncoder encorder = null;
            switch (extension) {
                case ".jpeg":
                case ".jpg":
                    encorder = new JpegBitmapEncoder();
                    break;
                case ".png":
                    encorder = new PngBitmapEncoder();
                    break;
                case "bmp":
                    encorder = new BmpBitmapEncoder();
                    break;
                default:
                    throw new ArgumentException(Properties.Strings.FileExtensionNotAvairable);
            }
            encorder.Frames.Add(BitmapFrame.Create(image));
            System.IO.FileStream stream = new FileStream(fileName, FileMode.Create);
            encorder.Save(stream);
            stream.Close();
        }
        /// <summary>
        /// 与えられたBitmapSourceを8bit, 256色グレースケール画像に変換します．
        /// </summary>
        /// <param name="source">画像ソース</param>
        /// <returns>変換後の画像</returns>
        public static BitmapSource ToGrayScale(BitmapSource source) {
            FormatConvertedBitmap bitmap = new FormatConvertedBitmap();
            bitmap.BeginInit();
            bitmap.Source = source;
            bitmap.DestinationFormat = PixelFormats.Gray8;
            bitmap.DestinationPalette = BitmapPalettes.Gray256;
            bitmap.EndInit();
            return bitmap;
        }

        /// <summary>
        /// BitmapSourceからbyte配列の画像データに変換します. 
        /// </summary>
        /// <param name="source">画像ソース</param>
        /// <returns>byte配列の画像データ</returns>
        /// <exception cref="System.ArgumentException">PixcelFormatがGray8でない場合</exception>
        public static byte[] ToArrayImage(BitmapSource source) {
            if (source.Format != PixelFormats.Gray8) {
                throw new ArgumentException(Properties.Strings.PixcelFormatException);
            }

            byte[] array = new byte[source.PixelWidth * source.PixelHeight];
            source.CopyPixels(array, source.PixelWidth, 0);
            return array;
        }

        /// <summary>
        /// System.Drawing.BitmapをBitmapSourceに変換します．
        /// </summary>
        /// <param name="source">ソース画像</param>
        /// <returns>Bitmapsource</returns>
        public static BitmapSource ToBitmapSource(System.Drawing.Bitmap source) {
            IntPtr ptr = source.GetHbitmap();
            BitmapSizeOptions sizeOptions = BitmapSizeOptions.FromEmptyOptions();
            BitmapSource bms = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                ptr,
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                sizeOptions);
            bms.Freeze();
            return bms;
        }
    }

}