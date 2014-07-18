using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NagaraStage {
    namespace Activities {
        /// <summary>
        /// 自動スキャン，自動表面認識，自動グリッドマーク検出など「自動処理」を行うActivityの
        /// インターフェイスです．
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public interface IActivity {

            /// <summary>
            /// 現在，動作中であるかどうかを取得します．
            /// </summary>
            bool IsActive {
                get;
            }

            /// <summary>
            /// アクティビティが開始したときのイベントハンドラ
            /// <para>このイベントハンドラはアクティビティのスレッドで実行されます．
            /// ユーザーインターフェイススレッドで実行されないことに注意してください．
            /// </para>
            /// </summary>
            event EventHandler<EventArgs> Started;

            /// <summary>
            /// アクティビティが終了したときのイベントハンドラ
            /// <para>このイベントハンドラはアクティビティのスレッドで実行されます．
            /// ユーザーインターフェイススレッドで実行されないことに注意してください．
            /// </para>
            /// </summary>
            event ActivityEventHandler Exited;

            /// <summary>
            /// アクティビティを開始します．
            /// </summary>
            void Start();

            /// <summary>
            /// 実行中のアクティビティを中止します．
            /// </summary>
            [Obsolete("アクティビティキューに移行するため廃止予定です。")]
            void Abort();

            /// <summary>
            /// アクティビティを実行するスレッドを作成します。
            /// </summary>
            /// <returns>実行スレッド</returns>
            List<Thread> CreateTask();
        }
    }
}