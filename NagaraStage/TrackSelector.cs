using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenCvSharp;
using OpenCvSharp.CPlusPlus;


namespace NagaraStage
{

    public class microtrack
    {
        public double ph;
        public double pv;
        public double ax;
        public double ay;
        public double cx;
        public double cy;
        public int n;
        public bool flag;
    }


    public class tsparams
    {
        public int ax;
        public int ay;
        public int pitchx;
        public int pitchy;
        public int phthre;
        public int clpix;//cluster pix
        public int clang;//cluster angle

        public tsparams()
        {
            //default: tentative parameter
            ax = 5;
            ay = 5;
            pitchx = 4;
            pitchy = 4;
            phthre = 10;
            clpix = 3;
            clang = 2;
        }
    }


    public class TrackSelector
    {


        public TrackSelector()
        {

        }


        static public List<microtrack> Select(List<ImageTaking> ITs, tsparams param)
        {
            List<microtrack> rms = new List<microtrack>();

            for (int ax = -param.ax; ax <= param.ax; ax++)
            {
                for (int ay = -param.ay; ay <= param.ay; ay++)
                {
                    //TODO: 角度におうじてMatのサイズを決定するようにしたい
                    using (Mat big = Mat.Zeros(800, 800, MatType.CV_8UC1))
                    {
                        for (int p = 0; p < ITs.Count; p++)
                        {
                            int startx = big.Width / 2 - ITs[p].img.Width / 2 + (int)(p * ax * param.pitchx / 16.0);
                            int starty = big.Height / 2 - ITs[p].img.Height / 2 + (int)(p * ay * param.pitchy / 16.0);
                            Cv2.Add(
                                big[starty, starty + ITs[p].img.Height, startx, startx + ITs[p].img.Width],
                                ITs[p].img,
                                big[starty, starty + ITs[p].img.Height, startx, startx + ITs[p].img.Width]);
                        }

                        //Cv2.ImWrite(string.Format("{0}_{1}.png", ax, ay), big * 15);
                        //Cv2.ImShow("big", big * 20);
                        //Cv2.WaitKey(0);

                        Cv2.Threshold(big, big, param.phthre, 255, ThresholdType.ToZero);

                        //Cv2.ImWrite(string.Format("{0}_{1}.png", ax, ay), big * 15);
                        //Cv2.ImShow("big_thre", big*20);
                        //Cv2.WaitKey(0);

                        
                        using (IplImage big_copy = big.Clone().ToIplImage())
                        using (CvMemStorage storage = new CvMemStorage())
                        using (CvContourScanner scanner = new CvContourScanner(big_copy, storage, CvContour.SizeOf, ContourRetrieval.External, ContourChain.ApproxNone))
                        {
                            CvScalar white = Cv.RGB(255, 255, 255);
                            foreach (CvSeq<CvPoint> c in scanner)
                            {
                                IplImage mask = new IplImage(big.Size(), BitDepth.U8, 1);
                                Cv.DrawContours(mask, c, white, white, 0, - 1);
                                Cv.And(big.ToIplImage(), mask, mask);
                                double minval;
                                double maxval;
                                CvPoint minLoc;
                                CvPoint maxLoc;
                                Cv.MinMaxLoc(mask, out minval, out maxval, out minLoc, out maxLoc);
 
                                microtrack rm = new microtrack();
                                rm.ph = maxval;
                                rm.ax = ax;
                                rm.ay = ay;
                                rm.cx = maxLoc.X - (big.Width / 2 - ITs[0].img.Width / 2);
                                rm.cy = maxLoc.Y - (big.Height / 2 - ITs[0].img.Height / 2);
                                rm.pv = mask.Sum();
                                rms.Add(rm);
                                //Console.WriteLine(string.Format("{0} {1}   {2} {3}  {4} {5}", rm.ph, rm.pv, rm.ax, rm.ay, rm.cx, rm.cy));
                                //Cv.ShowImage("a", mask);
                                //Cv2.WaitKey(0);
                            }
                        }//using contour
                        

                    }//using Mat



                }//for[ay:
            }//for[ax]


            rms.Sort((x, y) => y.pv.CompareTo(x.pv));

            //Console.WriteLine("-----");
            //foreach (microtrack rm in rms)
            //{
            //    Console.WriteLine(string.Format("{0} {1}   {2} {3}  {4} {5}", rm.ph, rm.pv, rm.ax, rm.ay, rm.cx, rm.cy));
            //}



            List<microtrack> ms = new List<microtrack>();

            for (int i = 0; i < rms.Count; i++)
            {

                microtrack a = rms[i];
                if (a.flag == true)
                {
                    continue;//already clusterd
                }

                rms[i].flag = true;

                microtrack mym = new microtrack();
                mym.ph = a.ph * a.pv;
                mym.pv = a.pv * a.pv;
                mym.ax = a.ax * a.pv;
                mym.ay = a.ay * a.pv;
                mym.cx = a.cx * a.pv;
                mym.cy = a.cy * a.pv;
                double pvsum = a.pv;


                for (int j = i + 1; j < rms.Count; j++)
                {

                    microtrack b = rms[j];
                    if (b.flag == true)
                    {
                        continue;//already clusterd
                    }

                    if (Math.Abs(a.cx - b.cx) > param.clpix || Math.Abs(a.cy - b.cy) > param.clpix)
                    {
                        continue;

                    }
                    if (Math.Abs(a.ax - b.ax) > param.clang || Math.Abs(a.ay - b.ay) > param.clang)
                    {
                        continue;
                    }


                    mym.ph += b.ph * b.pv;
                    mym.pv += b.pv * b.pv;
                    mym.ax += b.ax * b.pv;
                    mym.ay += b.ay * b.pv;
                    mym.cx += b.cx * b.pv;
                    mym.cy += b.cy * b.pv;
                    mym.n += 1;
                    pvsum += b.pv;
                    rms[j].flag = true;
                }

                mym.ph /= pvsum;
                mym.pv /= pvsum;
                mym.ax /= pvsum;
                mym.ay /= pvsum;
                mym.cx /= pvsum;
                mym.cy /= pvsum;
                ms.Add(mym);
            }
          
            return ms;
        }



    }
}
