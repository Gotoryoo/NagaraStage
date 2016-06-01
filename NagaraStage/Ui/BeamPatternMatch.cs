using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Collections;


namespace NagaraStage {
	class BeamPatternMatch {

		private int binsize;
		private int binrange;
		private double fbinsize;
		private double fbinrange;
		private int nbin;

		private int peak;
		private int peakvol;
		private double centerx;
		private double centery;
		private double peakx;
		private double peaky;
		private int entries;
		private int backgroundEntries;//総エントリ数からピーク周りの3*3binのエントリ数を引いた数
		private int effectiveNumberCells;//総bin数からピーク周りの9を引いた数

		private double backgroundProbability;//peakがバックグラウンドである確率
		private double backgroundAverage;//1binあたりのバックグラウンドのエントリ数の平均；backgroundEntries / effectiveNumberCells

		private double[,] InDataU;// = new double[line_countU, 2];
		private double[,] InDataD;// = new double[line_countU, 2];

		public int GetPeak() { return peak; }
		public int GetPeakVol() { return peakvol; }
		public int GetEntries() { return entries; }
		public int GetBackgroundEntries() { return backgroundEntries; }
		public int GetEffectiveNumberCells() { return effectiveNumberCells; }
		public double GetPeakX() { return peakx; }
		public double GetPeakY() { return peaky; }
		public double GetBackgroundProbability() { return backgroundProbability; }


		public BeamPatternMatch(int mybinsize, int mybinrange) {
			Initialize(mybinsize, mybinrange);
		}


		public void Initialize(int mybinsize, int mybinrange) {
			binsize = mybinsize;
			binrange = mybinrange;
			fbinsize = binsize * 1.00;
			fbinrange = binrange * 1.00;
			nbin = binrange * 2 / binsize;
			peak = 0;
			peakvol = 0;
			centerx = 0;
			centery = 0;
			peakx = 0;
			peaky = 0;
			entries = 0;
			backgroundEntries = 0;
			backgroundProbability = 0;
			effectiveNumberCells = nbin * nbin - 9;
		}


		public bool PixToCell(double px, double py, ref int ix, ref int iy) {
			//どこのbinなのか
			// binsize=8, fbinrange=200のときに
			//[-8, 0) = bin24
			//[0, 8) = bin25
			//[8, 16) = bin26

			double ox = (px + fbinrange) / fbinsize;
			double oy = (py + fbinrange) / fbinsize;
			ix = (int)(Math.Floor(ox));
			iy = (int)(Math.Floor(oy));
			return true;
		}

		public bool CellToPix(double x, double y, ref double px, ref double py) {
			//どこの位置なのか
			px = x * fbinsize - (fbinrange - fbinsize / 2.00);
			py = y * fbinsize - (fbinrange - fbinsize / 2.00);
			return true;
		}


		public double Poisson(double x, double par) {
			//cern rootlib 
			if (x < 0)
				return 0;
			else if (x == 0.00)
				return 1.00 / Math.Exp(par);
			else {
				double lnpoisson = x * Math.Log(par) - par - LogGamma(x + 1.0);
				return Math.Exp(lnpoisson);
			}
		}

		//http://www.johndcook.com/blog/csharp_gamma/
		public static double Gamma(double x)// We require x > 0
		{
			if (x <= 0.0) {
				string msg = string.Format("Invalid input argument {0}. Argument must be positive.", x);
				throw new ArgumentOutOfRangeException(msg);
			}

			// Split the function domain into three intervals:
			// (0, 0.001), [0.001, 12), and (12, infinity)

			///////////////////////////////////////////////////////////////////////////
			// First interval: (0, 0.001)
			//
			// For small x, 1/Gamma(x) has power series x + gamma x^2  - ...
			// So in this range, 1/Gamma(x) = x + gamma x^2 with error on the order of x^3.
			// The relative error over this interval is less than 6e-7.

			const double gamma = 0.577215664901532860606512090; // Euler's gamma constant

			if (x < 0.001)
				return 1.0 / (x * (1.0 + gamma * x));

			///////////////////////////////////////////////////////////////////////////
			// Second interval: [0.001, 12)

			if (x < 12.0) {
				// The algorithm directly approximates gamma over (1,2) and uses
				// reduction identities to reduce other arguments to this interval.

				double y = x;
				int n = 0;
				bool arg_was_less_than_one = (y < 1.0);

				// Add or subtract integers as necessary to bring y into (1,2)
				// Will correct for this below
				if (arg_was_less_than_one) {
					y += 1.0;
				} else {
					n = (int)(Math.Floor(y)) - 1;  // will use n later
					y -= n;
				}

				// numerator coefficients for approximation over the interval (1,2)
				double[] p =
				{
					-1.71618513886549492533811E+0,
					 2.47656508055759199108314E+1,
					-3.79804256470945635097577E+2,
					 6.29331155312818442661052E+2,
					 8.66966202790413211295064E+2,
					-3.14512729688483675254357E+4,
					-3.61444134186911729807069E+4,
					 6.64561438202405440627855E+4
				};

				// denominator coefficients for approximation over the interval (1,2)
				double[] q =
				{
					-3.08402300119738975254353E+1,
					 3.15350626979604161529144E+2,
					-1.01515636749021914166146E+3,
					-3.10777167157231109440444E+3,
					 2.25381184209801510330112E+4,
					 4.75584627752788110767815E+3,
					-1.34659959864969306392456E+5,
					-1.15132259675553483497211E+5
				};

				double num = 0.0;
				double den = 1.0;
				int i;

				double z = y - 1;
				for (i = 0; i < 8; i++) {
					num = (num + p[i]) * z;
					den = den * z + q[i];
				}
				double result = num / den + 1.0;

				// Apply correction if argument was not initially in (1,2)
				if (arg_was_less_than_one) {
					// Use identity gamma(z) = gamma(z+1)/z
					// The variable "result" now holds gamma of the original y + 1
					// Thus we use y-1 to get back the orginal y.
					result /= (y - 1.0);
				} else {
					// Use the identity gamma(z+n) = z*(z+1)* ... *(z+n-1)*gamma(z)
					for (i = 0; i < n; i++)
						result *= y++;
				}

				return result;
			}

			///////////////////////////////////////////////////////////////////////////
			// Third interval: [12, infinity)

			if (x > 171.624) {
				// Correct answer too large to display. 
				return double.PositiveInfinity;
			}

			return Math.Exp(LogGamma(x));
		}


		//http://www.johndcook.com/blog/csharp_gamma/		
		public static double LogGamma(double x) // x must be positive
		{
			if (x <= 0.0) {
				string msg = string.Format("Invalid input argument {0}. Argument must be positive.", x);
				throw new ArgumentOutOfRangeException(msg);
			}

			if (x < 12.0) {
				return Math.Log(Math.Abs(Gamma(x)));
			}

			// Abramowitz and Stegun 6.1.41
			// Asymptotic series should be good to at least 11 or 12 figures
			// For error analysis, see Whittiker and Watson
			// A Course in Modern Analysis (1927), page 252

			double[] c =
			{
				 1.0/12.0,
				-1.0/360.0,
				1.0/1260.0,
				-1.0/1680.0,
				1.0/1188.0,
				-691.0/360360.0,
				1.0/156.0,
				-3617.0/122400.0
			};
			double z = 1.0 / (x * x);
			double sum = c[7];
			for (int i = 6; i >= 0; i--) {
				sum *= z;
				sum += c[i];
			}
			double series = sum / x;

			double halfLogTwoPi = 0.91893853320467274178032973640562;
			double logGamma = (x - 0.5) * Math.Log(x) - x + halfLogTwoPi + series;
			return logGamma;
		}



		public bool ReadTrackDataTxtFile(string inputfilename, bool isup) {
			//reading csv file in C#
			//http://air-snowly.cocolog-nifty.com/rakkyo/2008/02/c2_223f.html
			StreamReader objReader = new StreamReader(inputfilename);
			ArrayList arrText = new ArrayList();
			string sLine = "";

			while (sLine != null) {
				sLine = objReader.ReadLine();
				if (sLine != null)
					arrText.Add(sLine);
			}
			objReader.Close();

			int line_count = arrText.Count;
			double[,] InDataTmp = new double[line_count, 2];

			int a = 0;
			foreach (string sOutput in arrText) {
				string[] temp_line = sOutput.Split(' ');
				InDataTmp[a, 0] = Convert.ToDouble(temp_line[0]);
				InDataTmp[a, 1] = Convert.ToDouble(temp_line[1]);
				a++;
			}

			if (isup) {
				InDataU = InDataTmp;
			} else {
				InDataD = InDataTmp;
			}

			return true;
		}


		public bool IsReady() {
			if (InDataU == null) return false;
			if (InDataD == null) return false;
			return true;
		}


		public void DoPatternMatch() {


			double thebestresult = 9999999.99;//バックグラウンドで説明できる確率がより低い結果に置き換えていく


			for (int k = 0; k < 2; k++)//k=0 noshift,  k=1 shift
			{
				for (int l = 0; l < 2; l++)//l=0 noshift,  l=1 shift
				{
					//vote/////////////////////////////////
					int[,] dxdymap = new int[nbin, nbin];
					int tmp_peak;
					int tmp_peakvol = 0;
					double tmp_ntpeak = 0.00;
					double tmp_centerx = 0.00;
					double tmp_centery = 0.00;
					double tmp_peakx = 0.00;
					double tmp_peaky = 0.00;
					int tmp_entries = 0;
					int tmp_backgroundEntries = 0;
					double tmp_backgroundAverage = 0.00;

					for (int i = 0; i < InDataU.GetLength(0); i++) {
						for (int j = 0; j < InDataD.GetLength(0); j++) {
							//Console.WriteLine(string.Format("{0} {1}  {2} {3}", InDataU[i, 0], InDataU[i, 1], InDataD[j, 0], InDataD[j, 1]));
							int ix = 0;
							int iy = 0;
							double dx = InDataD[j, 0] - InDataU[i, 0];
							double dy = InDataD[j, 1] - InDataU[i, 1];
							this.PixToCell(dx + k * binsize / 2.00, dy + l * binsize / 2.00, ref ix, ref iy);  //root bpmと同じ向きにしてみた
							if (ix < 0 || ix > nbin - 1) continue;
							if (iy < 0 || iy > nbin - 1) continue;
							dxdymap[ix, iy]++;
							tmp_entries++;
						}
					}

					//peakdetecrion/////////////////////////////////
					int maxbin = -1;
					int max_i = -1;
					int max_j = -1;

					for (int i = 0; i < nbin; i++) {
						for (int j = 0; j < nbin; j++) {
							if (maxbin < dxdymap[i, j]) {
								maxbin = dxdymap[i, j];
								max_i = i;
								max_j = j;
							}
						}
					}
					//Console.WriteLine(string.Format("maxbinlocate = ({0}, {1})   maxbin = {2}", max_i, max_j, maxbin));
					tmp_peak = maxbin;


					int binmax = 2 * binrange / binsize;//それぞれの軸のビンの数の最大

					//mass center/////////////////////////////////

					if (max_i > 1 && max_i < binmax - 1 && max_j > 1 && max_j < binmax - 1) {
						for (int i = max_i - 1; i <= max_i + 1; i++) {
							for (int j = max_j - 1; j <= max_j + 1; j++) {
								tmp_peak += dxdymap[i, j];

							}
						}

						tmp_backgroundEntries = tmp_entries - tmp_peakvol;//バックグラウンドの平均を引きピークの純粋な部分だけにする
						tmp_backgroundAverage = tmp_backgroundEntries * 1.00 / effectiveNumberCells * 1.00;


						for (int i = max_i - 1; i <= max_i + 1; i++) {
							for (int j = max_j - 1; j <= max_j + 1; j++) {

								tmp_ntpeak += dxdymap[i, j] - tmp_backgroundAverage;
								tmp_centerx += i * (dxdymap[i, j] - tmp_backgroundAverage);
								tmp_centery += j * (dxdymap[i, j] - tmp_backgroundAverage);

							}
						}
						tmp_centerx /= tmp_ntpeak;
						tmp_centery /= tmp_ntpeak;
						//tmp_backgroundEntries = tmp_entries - tmp_peakvol;
					} else {
						tmp_centerx = max_i;
						tmp_centery = max_j;
					}

					this.CellToPix(tmp_centerx, tmp_centery, ref tmp_peakx, ref tmp_peaky);
					tmp_peakx -= k * binsize / 2.00;
					tmp_peaky -= l * binsize / 2.00;


					double TemporaryProbability;
					TemporaryProbability = Poisson(maxbin, tmp_backgroundAverage) * effectiveNumberCells;

					//Console.WriteLine(string.Format("entries:{0}", entries));
					//Console.WriteLine(string.Format("effectiveEntries:{0}", effectiveEntries));
					//Console.WriteLine(string.Format("peak bin:{0} {1}", centerx, centery));
					//Console.WriteLine(string.Format("peak micron:{0} {1}", peakx, peaky));

					if (TemporaryProbability < thebestresult && tmp_backgroundAverage != 0) {

						thebestresult = TemporaryProbability;
						peak = tmp_peak;
						peakvol = tmp_peakvol;
						centerx = tmp_centerx;
						centery = tmp_centery;
						peakx = tmp_peakx;
						peaky = tmp_peaky;
						entries = tmp_entries;
						backgroundEntries = tmp_backgroundEntries;
						backgroundAverage = tmp_backgroundAverage;
						backgroundProbability = TemporaryProbability;
					}




				}//forloop l

			}//forloop k

			if (thebestresult > 0.00001) {

				peakvol = 0;
				centerx = 0;
				centery = 0;
				peakx = 0;
				peaky = 0;
				entries = 0;
				backgroundEntries = 0;
				backgroundAverage = 0;
				backgroundProbability = 0;

			}

		}


	}//BeamPatternMatch


}
