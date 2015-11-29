/**
 @author Hirokazu Yokoyama <o1007410@edu.gifu-u.ac.jp>
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using NagaraStage;
using NagaraStage.Parameter;
using NagaraStage.IO;

namespace NagaraStage {
    namespace Activities {

        /// <summary>
        /// 自動処理を行うアクティビティを管理するクラスです．
        /// <para>
        /// 全面スキャンなどでは，トラック追跡，表面認識を始めあらゆる工程を自動的に行います．
        /// 自動的に行う1工程を「アクティビティ」と呼ぶことにします．
        /// アクティビティは，ユーザーインターフェイススレッドとは別のスレッドで行います．
        /// 別のスレッドで行った方が，開始，中止などの制御を行いやすいからです．
        /// </para>
        /// <para>
        /// ただし，それぞれのアクティビティが独自のスレッドを用いて実行することは危険です．
        /// たとえば，トラック検出を行うアクティビティとグリッドマーク検出を行うアクティビティが
        /// 同時に実行されてしまい，一方がモータを右方向に動かし，もう一方がモータを左方向に
        /// 動かしてしまっては大問題です．
        /// </para>
        /// <para>
        /// そこで，アクティビティを行うスレッドは全アクティビティ間で共有して使います．
        /// 同時に1つのアクティビティしか実行できなくしてしまうわけです．
        /// そして，そのアクティビティを行うスレッドを管理するのがこのクラスです．
        /// </para>
        /// <para>
        /// <strong>各アクティビティを行うクラスはこのクラスを継承し，さらにIActivityインターフェイスを実装してください．</strong>
        /// </para>
        /// </summary>
        /// <author mail="o1007410@edu.gifu-u.ac.jp">Hirokazu Yokoyama</author>
        public class Activity {
            /// <summary>
            /// アクティビティを実行するスレッド
            /// </summary>
            private static Thread activityThread = null;

            /// <summary>
            /// アクティビティのタスクのためのデリゲート
            /// </summary>
            public delegate void ActivityTask();

            private ParameterManager parameterManager;

            /// <summary>
            /// アクティビティが実行中であるかどうかを取得します．
            /// <para>true: 実行中, false: 実行中でない</para>
            /// </summary>
            public bool IsActive {
                get {
                    bool flag = false;
                    if (activityThread != null) {
                        flag = activityThread.IsAlive;
                    }
                    return flag;
                }
            }

            /// <summary>
            /// アクティビティを実行するスレッドを作成します．
            /// <para>アクティビティが別の処理を実行中場合，InActionExceptionが投げられます．</para>
            /// </summary>
            /// <param name="start">アクティビティのタスク</param>
            /// <returns>アクティビティスレッド</returns>
            /// <exception cref="InActionException"></exception>
            protected Thread Create(ThreadStart start) {
                if (IsActive) {
                    throw new InActionException();
                }
                
                activityThread = new Thread(start);
                activityThread.IsBackground = true;
                return activityThread;
            }

            /// <param name="task">アクティビティのタスク</param>
            /// <param name="handler">タスク終了時のイベントハンドラ</param>
            /// <returns>アクティビティスレッド</returns>
            protected Thread Create(ActivityTask task, ActivityEventHandler handler)
            {
                ThreadStart start = new ThreadStart(delegate
                {
                    ActivityEventArgs args = new ActivityEventArgs();
                    try
                    {
                        task();
                        args.IsCompleted = true;
                    }
                    catch (ThreadAbortException ex)
                    {
                        args.IsAborted = true;
                        args.Exception = ex;
                    }
                    catch (Exception ex)
                    {
                        args.Exception = ex;
                    }
                    finally
                    {
                        MotorControler mc = MotorControler.GetInstance(parameterManager);
                        mc.AbortMoving();
                        mc.SlowDownStopAll();

                        if (handler != null)
                        {
                            handler(this, args);
                        }
                    }
                });
                return Create(start);
            }

            public Activity(ParameterManager _parameterManger) {
                this.parameterManager = _parameterManger;
            }

            /// <summary>
            /// 実行中のアクティビティを中止します．
            /// </summary>
            public void Abort() {
                if (IsActive) {
                    activityThread.Abort();
                    activityThread.Join();
                }
            }
        }
    }
}
