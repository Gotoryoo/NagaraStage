using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Parameter {
        /// <summary>
        /// 
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public class ScanModeUtility {
            /// <summary>
            /// 指定されたスキャンモードの名称を取得します．
            /// </summary>
            /// <param name="scanMode">名称を取得したいスキャンモード</param>
            /// <returns>スキャンモードの名称</returns>
            public static string ToString(ScanMode scanMode) {
                string modeString = "";
                switch (scanMode) {
                    case ScanMode.Automatic:
                        modeString = Properties.Strings.Automatic;
                        break;
                    case ScanMode.SemiAuto:
                        modeString = Properties.Strings.SemiAuto;
                        break;
                    default:
                        modeString = scanMode.ToString();
                        break;
                }
                return modeString;
            }
        }
    }
}