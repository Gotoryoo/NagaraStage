using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    /// <summary>
    /// メッセージを蓄えるためにList&lt;string&gt;クラスを継承したクラスです．
    /// Addメソッドでコールバックメソッドを実行できるようにしました．
    /// </summary>
    /// <author>Hirokazu Yokoyama</author>
    public class MessageList : List<string> {
        /// <summary>
        /// Addメソッドを行ったあとのコールバックメソッドを設定，または取得します．
        /// <para>引数には最も後に追加された要素が与えられます．</para>
        /// </summary>
        public Action<string> CallbackOfAdd;

        /// <summary>
        /// メッセージ文字列を追加します．追加後，CallbackOfAddが設定されていれば実行します．
        /// </summary>
        /// <param name="message"></param>
        public new void Add(string message) {
            base.Add(message);

            if (CallbackOfAdd != null) {
                CallbackOfAdd(message);
            }
        }
    }
}
