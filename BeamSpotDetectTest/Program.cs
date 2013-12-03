using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BeamSpotDetectTest {
    class Program {
        static void Main(string[] args) {
            //String url = "e:\\home\\hirokazu\\Pictures\\image03.png";
            //String url = "e:\\home\\hirokazu\\Dropbox\\hirokazu\\batsu01.bmp";
            //String url = "e:\\image01.png";
            String url = "c:\\Users\\hirokazu\\Dropbox\\hirokazu\\batsu04.png";
            //String url = "c:\\Users\\hirokazu\\Pictures\\image2.png";
            try {
                if (!System.IO.File.Exists(url)) {
                    throw new ArgumentException(url + "is not exist.");
                }

                BitmapImage image = new BitmapImage(new Uri(url));
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(url);

                CrossMarkDetector d = new CrossMarkDetector();
                d.SetImage(url, ImreadMode.Conservation);
                d.Binarize(30, ThresholdMode.Binary);
                d.AreaThreshold = 10000;
                d.Test();
                //System.Windows.Point p = d.GetCenter();
                //System.Console.WriteLine(string.Format("x,y={0},{1}", p.X, p.Y));
                //d.ShowWindow();

#if false
                Labeling l = new Labeling();
                l.SetImage(url, ImreadMode.Force3Color);
                l.Label(30, ThresholdMode.Binary);
                List<CircumBox> list = l.GetObjectListSortedByArea(0,5);
                for (int i = 0; i < list.Count; ++i) {
                    System.Console.WriteLine(
                        string.Format("ID:{0}, Area:{1}, (sx, sy) = ({2}, {3}), (ex, ey) = ({4}, {5})",
                        list[i].Id, list[i].Area, list[i].X0, list[i].Y0, list[i].X1, list[i].Y1));
                }
#endif
            } catch (ArgumentException ex) {
                System.Console.WriteLine(ex.Message);                
            }
            System.Console.ReadLine();
        }
    }
}