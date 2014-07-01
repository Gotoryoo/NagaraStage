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
        /// 自動処理を行うアクティビティを管理するクラスです。
        /// </summary>
        public class Activity {
            private static Activity instance = null;

            /// <summary>
            /// アクティビティのタスクのためのデリゲート
            /// </summary>
            public delegate void ActivityTask();

            /// <summary>
            /// アクティビティのキュー。
            /// <para>各タスクをキューイングすることで、順に実行します。</para>
            /// </summary>
            private ActivityQueue queue = new ActivityQueue();

            private ParameterManager parameterManager = null;

            /// <summary>
            /// キューイングを行うスレッドです
            /// </summary>
            private Thread queuingThread = null;

            /// <summary>
            /// アクティビティが実行中かどうかを取得します。
            /// <para>true: 実行中, false: 停止中</para>
            /// </summary>
            public bool IsActive {
                get { 
                    bool retval = false;
                    if(queuingThread != null) {
                        retval = (queuingThread.IsAlive);
                    }
                    return retval;
                }
            }

            public static Activity GetInstance(ParameterManager paramgerManger = null) {
                if(instance == null) {
                    instance = new Activity(paramgerManger);
                }
                return instance;
            }

            private Activity(ParameterManager _parameterManager) {
                this.parameterManager = _parameterManager;
                isValidate();
            }

            /// <summary>
            /// タスクを末尾に追加します。
            /// </summary>
            /// <param name="task">追加するアクティビティタスク</param>
            /// <param name="handler">該当タスクが終了したときのイベントハンドラ</param>
            public void Enqueue(ActivityTask task, ActivityEventHandler handler) {
                Thread t = createTaskThraed(task, handler);
                queue.Enqueue(t);
            }

            /// <summary>
            /// キューイングされたタスクをすべて削除します。
            /// <para>アクティビティが終了させてから実行してください。
            /// さもなくばInActionExceptionが投げられます。</para>
            /// </summary>
            public void Clear() {
                if(IsActive) {
                    throw new InActionException();
                }
                queue.Clear();
            }

            public void Start() {
                queuingThread = new Thread(queueing());
                queuingThread.Start();
            }
            public void Abort() {
                if(IsActive) {
                    queuingThread.Abort();
                    queuingThread.Join();
                }
            }

            /// <summary>
            /// キューに登録するアクティビティスレッドを作成します。
            /// </summary>
            /// <param name="task">アクティビティスレッドで実行するタスク</param>
            /// <param name="handler">アクティビティが終了したときのイベントハンドラ(初期値:null)</param>
            /// <returns>作成したアクティビティスレッド</returns>
            private Thread createTaskThraed(ActivityTask task, ActivityEventHandler handler = null) {
                ThreadStart threadStart = new ThreadStart(delegate {
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
                        mc.StopInching(MechaAxisAddress.XAddress);
                        mc.StopInching(MechaAxisAddress.YAddress);
                        mc.StopInching(MechaAxisAddress.ZAddress);
                        if (handler != null)
                        {
                            handler(this, args);
                        }
                    }
                });
                return new Thread(threadStart);
            }

            private ThreadStart queueing() {
                return new ThreadStart(delegate{
                    while(queue.Count > 0) {
                        Thread activity = queue.Dequeue();
                        activity.Start();
                        activity.Join();
                    }
                });                
            }

            private bool isValidate() {
                if(parameterManager == null) {
                    throw new NullReferenceException("parameterManager is null pointer.");
                }
                return true;
            }
        }

        /// <summary>
        /// アクティビティのキューイングを行うキュークラスです。
        /// </summary>
        internal class ActivityQueue : Queue<Thread> {
        }

#if false

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
                        mc.StopInching(MechaAxisAddress.XAddress);
                        mc.StopInching(MechaAxisAddress.YAddress);
                        mc.StopInching(MechaAxisAddress.ZAddress);
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
#endif
}
