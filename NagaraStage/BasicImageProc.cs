using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using OpenCvSharp;
using OpenCvSharp.CPlusPlus;



namespace NagaraStage
{

    public class ImageTaking
    {
        public Vector3 StageCoord;
        public byte[] b;//こうするべきかなあ。
        public Mat img;

        public ImageTaking(Vector3 _c, byte[] _b)
        {
            StageCoord = _c;
            b = _b;
            img = new Mat(440, 512, MatType.CV_8U, b);
        }

    }


    public class BasicImageProc
    {


        //@param input kernel: kernelsize. it should be odd number
        static public Mat DifferenceOfGaussian(Mat image, int kernel = 51)
        {
            Mat gau = new Mat(440, 512, MatType.CV_8U);
            Mat dst = new Mat(440, 512, MatType.CV_8U);

            Cv2.GaussianBlur(image, image, Cv.Size(3, 3), -1);//
            Cv2.GaussianBlur(image, gau, Cv.Size(kernel, kernel), -1);
            Cv2.Subtract(gau, image, dst);

            gau.Dispose();
            return dst;
        }


        static public Mat DogContrastBinalize(Mat image, int kernel = 51, int threshold = 100, ThresholdType thtype = ThresholdType.Binary)
        {
            Mat img = DifferenceOfGaussian(image, kernel);

            double Max_kido;
            double Min_kido;
            OpenCvSharp.CPlusPlus.Point maxloc;
            OpenCvSharp.CPlusPlus.Point minloc;
            Cv2.MinMaxLoc(img, out Min_kido, out Max_kido, out minloc, out maxloc);

            Cv2.ConvertScaleAbs(img, img, 255 / (Max_kido - Min_kido), -255 * Min_kido / (Max_kido - Min_kido));
            Cv2.Threshold(img, img, threshold, 1, thtype);

            return img;
        }



        static public int HitPixCount(byte[] b, int width, int height, int kernel, int thre = 10)
        {
            Mat mat0 = new Mat(height, width, MatType.CV_8U, b);
            Mat mat = mat0.Clone();
            Cv2.Threshold(mat, mat, thre, 1, ThresholdType.Binary);
            int brightness = Cv2.CountNonZero(mat);
            return brightness;
        }


        static public bool ImageWrite(byte[] b, int width, int height, string filename)
        {
            Mat mat0 = new Mat(440, 512, MatType.CV_8U, b);
            Mat mat = mat0.Clone();
            return true;
        }

        static public bool DatfileWrite(byte[] b, int width, int height, int npic, string filename)
        {
            return true;
        }


        static public List<byte[]> DatfileRead(int width, int height, string filename)
        {
            List<byte[]> lb = new List<byte[]>();

            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);

            int fileSize = (int)fs.Length;
            int remain = fileSize;
            
            byte[] buf = new byte[width * height];


            while (remain > 0)
            {
                int readSize = fs.Read(buf, 0, width * height);
                remain -= readSize;
                lb.Add((byte[])buf.Clone());
            }

            fs.Dispose();

            return lb;
        }



        //meanbrightness =  clone_image.Mean();
    }
}
