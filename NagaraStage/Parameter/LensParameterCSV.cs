using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NagaraStage.Parameter {
    /// <summary>
    /// レンズのパラメータを旧来使われていたCSV形式から読み込んで値を管理するクラスです．
    /// </summary>
    class LensParameterCSV : LensParameter, ILensParameter {
        private double xm, xp, ym, yp;

        public LensParameterCSV() {
            ConfigureFile = "Configure/ccdreso.ini";
        }

        public double ScanAreaXm {
            get { return xm; }
        }

        public double ScanAreaXp {
            get { return xp; }
        }

        public double ScanAreaYm {
            get { return ym; }
        }

        public double ScanAreaYp {
            get { return yp; }
        }

        public new void Load() {
            if (!File.Exists(ConfigureFile)) {
                throw new FileNotFoundException();
            }
            parseConfigureFile();
            Magnification = 10;
        }

        public new void Load(string path) {
            if (!File.Exists(path)) {
                throw new FileNotFoundException();
            }
            ConfigureFile = path;
            Load();
        }

        private void parseConfigureFile() {
            if (!File.Exists(ConfigureFile)) {
                throw new FileNotFoundException(ConfigureFile + " is not found. Loading CCD Profile is failed.");
            }

            LensList = new List<LensStatus>();

            StreamReader streamReader = File.OpenText(ConfigureFile);
            streamReader.ReadLine();
            streamReader.ReadLine();
            
            string line;
            string[] elements;
            bool continueFlag = true;
            char[] delimiterChars = { ' ', '\t' };
            while(continueFlag) {
                line = streamReader.ReadLine();
                elements = line.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                if (elements.Length > 0) {
                    LensStatus lens = new LensStatus();
                    lens.Magnification = int.Parse(elements[0]);
                    lens.CcdResolution.X = double.Parse(elements[1]);
                    lens.CcdResolution.Y = double.Parse(elements[2]);
                    lens.LensOffset.X = double.Parse(elements[3]);
                    lens.LensOffset.Y = double.Parse(elements[4]);
                    lens.SpiralShift.X = double.Parse(elements[5]);
                    lens.SpiralShift.Y = double.Parse(elements[6]);
                    lens.ZStep = double.Parse(elements[7]);
                    lens.ImageLength.X = lens.CcdResolution.X * ParameterManager.ImageResolution.Width;
                    lens.ImageLength.Y = lens.CcdResolution.Y * ParameterManager.ImageResolution.Height;
                    switch (lens.Magnification) { 
                        case 10:
                            lens.GridMarkSize = 50;
                            lens.LedParameter = 40;
                            break;
                        case 20:
                            lens.GridMarkSize = 110;
                            lens.LedParameter = 40;
                            break;
                        case 50:
                            lens.GridMarkSize = 250;
                            lens.LedParameter = 80;
                            break;
                        default:
                            lens.GridMarkSize = 0;
                            break;
                    }
                    LensList.Add(lens);
                } else {
                    continueFlag = false;
                }
            }           

            streamReader.ReadLine();
            streamReader.ReadLine();

            // スキャン範囲の読み込み
            line = streamReader.ReadLine();
            elements = line.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);            
            xm = double.Parse(elements[0]);
            xp = double.Parse(elements[1]);
            ym = double.Parse(elements[2]);
            yp = double.Parse(elements[3]);
            streamReader.Close();
        }
    }
}
