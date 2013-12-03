using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage.Activities {
    interface ISurface : IActivity{

        /// <summary>
        /// 下ゲル下部の表面認識をしたときのイベント
        /// </summary>
        event EventHandler<ActivityEventArgs> LowBottomRecognized;
        /// <summary>
        /// 下ゲル上部の表面認識をしたときのイベント
        /// </summary>
        event EventHandler<ActivityEventArgs> LowTopRecognized;
        /// <summary>
        /// 上ゲル下部の表面認識をしたときのイベント
        /// </summary>
        event EventHandler<ActivityEventArgs> UpBottomRecognized;
        /// <summary>
        /// 下ゲル上部の表面認識をしたときのイベント
        /// </summary>
        event EventHandler<ActivityEventArgs> UpTopRecognized;
        /// <summary>
        /// 表面認識処理が終了したときのイベント
        /// <para>正常に完了した場合，中止された場合，エラーが発生した場合のすべてを含みます．</para>
        /// </summary>
        new event ActivityEventHandler Exited;

        /// <summary>
        /// 表面認識の実行時間を取得します．
        /// </summary>
        DateTime DateTime {
            get;
        }

        /// <summary>上ゲル上面の座標を取得します．</summary>
        double UpTop {
            get;
        }

        /// <summary>上ゲル下面の座標を取得します．</summary>
        double UpBottom {
            get;
        }

        /// <summary>下ゲル上面の座標を取得します．</summary>
        double LowTop {
            get;
        }

        /// <summary>下ゲル下面の座標を取得します．</summary>
        double LowBottom {
            get;
        }

        /// <summary>現在撮影中の位置がゲルの中であるかどうかを取得します．</summary>
        /// <returns><c>true</c> ゲルの中似る場合; そうでなければ, <c>false</c>.</returns>
        bool IsInGel();
    }
}
