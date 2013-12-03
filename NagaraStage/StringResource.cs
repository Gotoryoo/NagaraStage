using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace NagaraStage {
    /// <summary>
    /// メッセージのテキストを取得するためのクラスです．
    /// </summary>
    public sealed class StringResource {
        private XmlDocument resouce = new XmlDocument();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="filepath">言語ファイルXMLへのパス</param>
        public StringResource(string filepath) {
            resouce.Load(filepath);
        }

        /// <summary>
        /// メッセージのテキストを取得します．
        /// </summary>
        /// <param name="hashTag">メッセージタグ</param>
        /// <returns>メッセージテキスト</returns>
        public string Get(string hashTag) {
            string str = "";
            XmlElement root = resouce.DocumentElement;
            XmlNodeList nodes = root.GetElementsByTagName("string");
            for (int i = 0; i < nodes.Count; ++i) {
                XmlElement node = (XmlElement)nodes.Item(i);
                str = (node.GetAttribute("name") == hashTag ? node.InnerText : str);
            }
            return str;
        }

        public StringResource() : this("lang/default.xml") {
        }
    }
}
