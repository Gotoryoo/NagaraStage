﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using NagaraStage.IO;
using NagaraStage.Parameter;


namespace NagaraStage.Activities {
    class ActivityManager:IActivity {
        private static ActivityManager instance = null;

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

        private Thread activityThread = null;

        /// <summary>
        /// アクティビティが実行中かどうかを取得します。
        /// <para>true: 実行中, false: 停止中</para>
        /// </summary>
        public bool IsActive {
            get {
                bool retval = false;
                if (queuingThread != null) {
                    retval = (queuingThread.IsAlive);
                }
                return retval;
            }
        }

        public static ActivityManager GetInstance(ParameterManager paramgerManger = null) {
            if (instance == null) {
                instance = new ActivityManager(paramgerManger);
            }
            return instance;
        }

        private ActivityManager(ParameterManager _parameterManager) {
            this.parameterManager = _parameterManager;
            isValidate();
        }

        /// <summary>
        /// 実行するアクティビティを末尾に追加します。
        /// </summary>
        /// <param name="task">追加するアクティビティタスク</param>
        /// <param name="handler">該当タスクが終了したときのイベントハンドラ</param>
        public void Enqueue(IActivity activity) {
            List<Thread> threads = activity.CreateTask();
            threads.ForEach(delegate(Thread t) {
                t.IsBackground = true;
                queue.Enqueue(t);
            });
        }

        /// <summary>
        /// キューイングされたタスクをすべて削除します。
        /// <para>アクティビティが終了させてから実行してください。
        /// さもなくばInActionExceptionが投げられます。</para>
        /// </summary>
        public void Clear() {
            if (IsActive) {
                throw new InActionException();
            }
            queue.Clear();
        }

        public void Start() {
            queuingThread = new Thread(queueing());
            queuingThread.IsBackground = true;
            queuingThread.Start();
        }
        public void Abort() {
            if (IsActive) {
                activityThread.Abort();
                queuingThread.Abort();
                activityThread.Join();
                queuingThread.Join();
            }
        }

        private ThreadStart queueing() {
            return new ThreadStart(delegate {
                while (queue.Count > 0) {
                    activityThread = queue.Dequeue();
                    activityThread.Start();
                    activityThread.Join();
                }
            });
        }

        private bool isValidate() {
            if (parameterManager == null) {
                throw new NullReferenceException("parameterManager is null pointer.");
            }
            return true;
        }


        public event EventHandler<EventArgs> Started;

        public event ActivityEventHandler Exited;

        public List<Thread> CreateTask() {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// アクティビティのキューイングを行うキュークラスです。
    /// </summary>
    public class ActivityQueue : Queue<Thread> {
    }

}
