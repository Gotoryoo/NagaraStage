using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Collections;

namespace NagaraStage {
    class TigerPatternMatch {
        private static bool PixToCell(double px, double py, ref int ix, ref int iy) {
            ix = (int)(px / 4.0 + 25.0);
            iy = (int)(py / 4.0 + 25.0);
            return true;
        }

        private static bool CellToPix(double x, double y, ref double px, ref double py) {
            px = (x - 25.0) * 4.0;
            py = (y - 25.0) * 4.0;
            return true;
        }

        public static bool PatternMatch(ref Vector2 vshift) {
            //http://air-snowly.cocolog-nifty.com/rakkyo/2008/02/c2_223f.html

            //downside/////////////////////////////////
            StreamReader objReaderU = new StreamReader(@"c:\img\d.txt");
            ArrayList arrTextU = new ArrayList();
            string sLineU = "";

            while (sLineU != null) {
                sLineU = objReaderU.ReadLine();
                if (sLineU != null)
                    arrTextU.Add(sLineU);
            }
            objReaderU.Close();

            int line_countU = arrTextU.Count;
            double[,] InDataU = new double[line_countU, 2];

            int a = 0;
            foreach (string sOutput in arrTextU) {
                string[] temp_line = sOutput.Split(' ');
                InDataU[a, 0] = Convert.ToDouble(temp_line[0]);
                InDataU[a, 1] = Convert.ToDouble(temp_line[1]);
                a++;
            }

            //upside/////////////////////////////////
            StreamReader objReaderD = new StreamReader(@"c:\img\u.txt");
            ArrayList arrTextD = new ArrayList();
            string sLineD = "";

            while (sLineD != null) {
                sLineD = objReaderD.ReadLine();
                if (sLineD != null)
                    arrTextD.Add(sLineD);
            }
            objReaderD.Close();

            int line_countD = arrTextD.Count;
            double[,] InDataD = new double[line_countD, 2];

            a = 0;
            foreach (string sOutput in arrTextD) {
                string[] temp_line = sOutput.Split(' ');
                InDataD[a, 0] = Convert.ToDouble(temp_line[0]);
                InDataD[a, 1] = Convert.ToDouble(temp_line[1]);
                a++;
            }

            //vote/////////////////////////////////
            int[,] dxdymap = new int[50, 50];
            int sumbackground = 0;
            for (int i = 0; i < InDataU.GetLength(0); i++) {
                for (int j = 0; j < InDataD.GetLength(0); j++) {
                    //Console.WriteLine(string.Format("{0} {1}  {2} {3}", InDataU[i, 0], InDataU[i, 1], InDataD[j, 0], InDataD[j, 1]));
                    int ix = 0;
                    int iy = 0;
                    TigerPatternMatch.PixToCell(InDataD[j, 0] - InDataU[i, 0], InDataD[j, 1] - InDataU[i, 1], ref ix, ref iy);
                    if (ix < 0 || ix >= 50 || iy < 0 || iy >= 50) continue;
                    dxdymap[ix, iy]++;
                    sumbackground++;
                }
            }

            //peakdetecrion/////////////////////////////////
            int maxbin = -1;
            int max_i = -1;
            int max_j = -1;

            for (int i = 0; i < 50; i++) {
                for (int j = 0; j < 50; j++) {
                    if (maxbin < dxdymap[i, j]) {
                        maxbin = dxdymap[i, j];
                        max_i = i;
                        max_j = j;
                    }
                }
            }
            Console.WriteLine(string.Format("{0} {1}   {2}", max_i, max_j, maxbin));

            if (maxbin > 50) {

                //mass center/////////////////////////////////
                int peakvol = 0;
                double centerx = 0;
                double centery = 0;
                if (max_i > 1 && max_i < 49 && max_j > 1 && max_j < 49) {
                    for (int i = max_i - 1; i <= max_i + 1; i++) {
                        for (int j = max_j - 1; j <= max_j + 1; j++) {
                            peakvol += dxdymap[i, j];
                            centerx += i * dxdymap[i, j];
                            centery += j * dxdymap[i, j];
                        }
                    }
                    centerx /= peakvol;
                    centery /= peakvol;
                    Console.WriteLine(string.Format("{0} {1}", centerx, centery));

                    sumbackground -= peakvol;
                    Console.WriteLine(string.Format("sumbackground:{0}", sumbackground));
                }

                double peakx = 0;
                double peaky = 0;
                TigerPatternMatch.CellToPix(centerx, centery, ref peakx, ref peaky);
                Console.WriteLine(string.Format("peak:{0} {1}", peakx, peaky));

                vshift.X = peakx;
                vshift.Y = peaky;
                return true;
            }else{
                return false;
            }

        }
    }
}
