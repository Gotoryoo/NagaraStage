using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using NagaraStage;

namespace NagaraStage.Parameter {
    /// <summary>
    /// レンズのパラメータの読み込み及び管理をするクラスです．
    /// </summary>
    class LensParameter : ILensParameter, IFileParser {
        /// <summary>
        /// 設定ファイルパス
        /// </summary>
        protected string ConfigureFile = "Configure/ccdreso.xml";

        private XmlDocument xmlDocument = new XmlDocument();
        protected List<LensStatus> LensList;
        private LensStatus presentLens;

        public LensParameter() {
        }

        /// <summary>
        /// 設定ファイルを読み込み，パラメータを(再)定義します．
        /// </summary>
        /// <exception cref="System.IO.FileNotFoundException">設定ファイルが見つからなかった場合</exception>
        public void Load() {
            xmlDocument.Load(ConfigureFile);
            parseCcdXml();
            Magnification = 10;
            Ipt.SetCcdResolution(CcdResolutionX, CcdResolutionY);
        }

        /// <summary>
        /// 設定ファイルを読み込み，パラメータを(再)定義します．
        /// </summary>
        /// <param name="path">設定ファイルへのパス</param>
        /// <exception cref="System.IO.FileNotFoundException">設定ファイルが見つからなかった場合</exception>
        public void Load(string path) {
            if (!System.IO.File.Exists(path)) {
                throw new System.IO.FileNotFoundException();
            }
            ConfigureFile = path;
            Load();
        }

        /// <summary>
        /// レンズ情報を定義したXMLドキュメントを解析し，フィールド変数に値を設定します．
        /// </summary>
        private void parseCcdXml() {
            XmlNodeList nodeList = xmlDocument.GetElementsByTagName("lens");
            LensList = new List<LensStatus>(nodeList.Count);
            for (int i = 0; i < nodeList.Count; ++i) {
                LensStatus lens = new LensStatus();
                lens.Magnification
                    = int.Parse(nodeList[i].Attributes["magnification"].Value);
                lens.CcdResolution = new Vector2();
                lens.LensOffset = new Vector2();
                lens.SpiralShift = new Vector2();
                bool[] compFlag = new bool[8];
                foreach (XmlElement element in nodeList[i].ChildNodes) {                    
                    double val;
                    switch (element.Name) {
                        case "resolutionX":
                            val = double.Parse(element.InnerText);
                            lens.CcdResolution.X = val;
                            lens.ImageLength.X = val * ParameterManager.ImageResolution.Width;
                            compFlag[0] = true;
                            break;
                        case "resolutionY":
                            val = double.Parse(element.InnerText);
                            lens.CcdResolution.Y = val;
                            lens.ImageLength.Y = val * ParameterManager.ImageResolution.Height;
                            compFlag[1] = true;
                            break;
                        case "offsetDX":
                            lens.LensOffset.X = double.Parse(element.InnerText);
                            compFlag[2] = true;
                            break;
                        case "offsetDY":
                            lens.LensOffset.Y = double.Parse(element.InnerText);
                            compFlag[3] = true;
                            break;
                        case "spiralDX":
                            lens.SpiralShift.X = double.Parse(element.InnerText);
                            compFlag[4] = true;
                            break;
                        case "spiralDY":
                            lens.SpiralShift.Y = double.Parse(element.InnerText);
                            compFlag[5] = true;
                            break;
                        case "zStep":
                            lens.ZStep = double.Parse(element.InnerText);
                            compFlag[6] = true;
                            break;
                        case "gridMarkSize" :
                            lens.GridMarkSize = double.Parse(element.InnerText);
                            compFlag[7] = true;
                            break;
                        case "ledParam" :
                            lens.LedParameter = int.Parse(element.InnerText);
                            break;
                        default:
                            break;
                    }
                }
                // 上記switchのすべての項目が読み込まれていなければ例外を投げる．
                if (!isCompLensItems(compFlag)) {
                    string message =
                        string.Format(Properties.Strings.LensNodeParseException01, ConfigureFile);
                    throw new XmlException(message);
                }
                LensList.Add(lens);
            }
        }

        private bool isCompLensItems(bool[] flags) {
            bool retVal = true;
            for (int i = 0; i < flags.Length; ++i) { 
                retVal &= flags[i];
            }
            return retVal;
        }

        /// <summary>
        /// 現在使用しているレンズタイプ(レンズの倍率)を取得，または設定します．
        /// </summary>
        /// <exception cref="System.ArgumentException">対象外の倍率を指定した場合</exception>
        public double Magnification {
            get {
                return presentLens.Magnification;
            }

            set {
                bool flag = false;
                for (int i = 0; i < LensList.Count; ++i) { 
                    if(value == LensList[i].Magnification) {
                        presentLens = LensList[i];
                        flag = true;
                    }
                }
                Ipt.SetCcdResolution(CcdResolutionX, CcdResolutionY);
                if (!flag) {
                    throw new ArgumentException(Properties.Strings.LensTypeException01);
                }
            }
        }

        public double CcdResolutionX {
            get { return presentLens.CcdResolution.X; }
        }

        public double CcdResolutionY {
            get { return presentLens.CcdResolution.Y; }
        }

        public double ImageLengthX {
            get { return presentLens.ImageLength.X; }
        }

        public double ImageLengthY {
            get { return presentLens.ImageLength.Y; }
        }

        public double LensOffsetX {
            get { return presentLens.LensOffset.X; }
        }

        public double LensOffsetY {
            get { return presentLens.LensOffset.Y; }
        }

        public double SpiralShiftX {
            get { return presentLens.SpiralShift.X; }
        }

        public double SpiralShiftY {
            get { return presentLens.SpiralShift.Y; }
        }

        public double LensZStep {
            get { return presentLens.ZStep; }
        }

        public int NumOfLens {
            get { return LensList.Count; }
        }

        public double GridMarkSize {
            get { return presentLens.GridMarkSize; }
        }

        public int LedParameter {
            get { return presentLens.LedParameter; }
        }

        public double GetCcdResoulutionX(double lensType) {
            double retVal = -1;
            for (int i = 0; i < LensList.Count; ++i) { 
                retVal = (lensType == LensList[i].Magnification 
                    ? LensList[i].CcdResolution.X : retVal);
            }
            if (retVal == -1) {
                throw new ArgumentException(Properties.Strings.LensTypeException01);
            }
            return retVal;
        }

        public double GetCcdResoulutionY(double lensType) {
            double retVal = -1;
            for (int i = 0; i < LensList.Count; ++i) {
                retVal = (lensType == LensList[i].Magnification
                    ? LensList[i].CcdResolution.Y : retVal);
            }
            if (retVal == -1) {
                throw new ArgumentException(Properties.Strings.LensTypeException01);
            }
            return retVal;
        }

        public double GetImageLengthX(double lensType) {
            double retVal = -1;
            for (int i = 0; i < LensList.Count; ++i) {
                retVal = (lensType == LensList[i].Magnification
                    ? LensList[i].ImageLength.X : retVal);
            }
            if (retVal == -1) {
                throw new ArgumentException(Properties.Strings.LensTypeException01);
            }
            return retVal;            
        }

        public double GetImageLengthY(double lensType) {
            double retVal = -1;
            for (int i = 0; i < LensList.Count; ++i) {
                retVal = (lensType == LensList[i].Magnification
                    ? LensList[i].ImageLength.Y : retVal);
            }
            if (retVal == -1) {
                throw new ArgumentException(Properties.Strings.LensTypeException01);
            }
            return retVal;            
        }

        public double GetLensOffsetX(double lensType) {
            double retVal = -9999;
            for (int i = 0; i < LensList.Count; ++i) {
                retVal = (lensType == LensList[i].Magnification
                    ? LensList[i].LensOffset.X : retVal);
            }
            if (retVal == -9999) {
                throw new ArgumentException(Properties.Strings.LensTypeException01);
            }
            return retVal;            
        }

        public double GetLensOffsetY(double lensType) {
            double retVal = -9999;
            for (int i = 0; i < LensList.Count; ++i) {
                retVal = (lensType == LensList[i].Magnification
                    ? LensList[i].LensOffset.Y : retVal);
            }
            if (retVal == -9999) {
                throw new ArgumentException(Properties.Strings.LensTypeException01);
            }
            return retVal;            
        }

        public double GetSpiralShiftX(double lensType) {
            double retVal = -1;
            for (int i = 0; i < LensList.Count; ++i) {
                retVal = (lensType == LensList[i].Magnification
                    ? LensList[i].SpiralShift.X : retVal);
            }
            if (retVal == -1) {
                throw new ArgumentException(Properties.Strings.LensTypeException01);
            }
            return retVal;            
        }

        public double GetSpiralShiftY(double lensType) {
            double retVal = -1;
            for (int i = 0; i < LensList.Count; ++i) {
                retVal = (lensType == LensList[i].Magnification
                    ? LensList[i].SpiralShift.Y : retVal);
            }
            if (retVal == -1) {
                throw new ArgumentException(Properties.Strings.LensTypeException01);
            }
            return retVal;            
        }

        public double GetLensZStep(double lensType) {
            double retVal = -1;
            for (int i = 0; i < LensList.Count; ++i) {
                retVal = (lensType == LensList[i].Magnification
                    ? LensList[i].ZStep : retVal);
            }
            if (retVal == -1) {
                throw new ArgumentException(Properties.Strings.LensTypeException01);
            }
            return retVal;            
        }

        public double GetGridMarkSize(double lensType) {
            double retVal = -1;
            for (int i = 0; i < LensList.Count; ++i) {
                retVal = (lensType == LensList[i].Magnification
                    ? LensList[i].GridMarkSize : retVal);
            }
            if (retVal == -1) {
                throw new ArgumentException(Properties.Strings.LensTypeException01);
            }
            return retVal;            
        }

        public int GetLedParameter(double lensType) {
            int retVal = -1;
            for (int i = 0; i < LensList.Count; ++i) {
                retVal = (lensType == LensList[i].Magnification
                    ? LensList[i].LedParameter : retVal);
            }
            if (retVal == -1) {
                throw new ArgumentException(Properties.Strings.LensTypeException01);
            }
            return retVal;            
        }


        public double[] GetLensMagList() {
            double[] mags = new double[NumOfLens];
            for (int i = 0; i < mags.Length; ++i) {
                mags[i] = LensList[i].Magnification;
            }
            return mags;
        }

        /// <summary>
        /// 現状の設定をファイルに出力します．
        /// </summary>
        /// <param name="path">保存先</param>
        public void Save(string path) {
            try {
                xmlDocument.Save(path);
            } catch (XmlException ex) {
                throw ex;
            }
        }
    }
}
