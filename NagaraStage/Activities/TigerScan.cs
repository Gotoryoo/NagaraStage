using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using NagaraStage.IO;
using NagaraStage.Parameter;

namespace NagaraStage.Activities {
    /// <summary>
    /// <h1>タイガースキャンの実装</h1>
    /// </summary>
    public class TigerScan: Activity, IActivity {
        private static TigerScan instance = null;

        /// <summary>
        /// タイガースキャンが開始されたときのイベント
        /// <para>タイガースキャンの開始時にタイガースキャンを実行しているスレッドで実行されます。</para>
        /// </summary>
        public EventHandler<EventArgs> Started = null;

        /// <summary>
        /// タイガースキャンが終了したときのイベント
        /// <para>タイガースキャン終了時にタイガースキャンを実行しているスレッドで実行されます。</para>
        /// </summary>
        public ActivityEventHandler Exited = null;

        /// <summary>
        /// インスタンスを取得します。
        /// </summary>
        /// <returns>TigerScanのインスタンス</returns>
        public static TigerScan GetInstance(ParameterManager parameterManager = null) {
            if(instance == null) {
                instance = new TigerScan(parameterManager);
            }
            return instance;
        }

        private TigerScan(ParameterManager _param):base(_param) {
        }

        /// <summary>
        /// タイガースキャンを実行します。
        /// </summary>
        public void Start() {
            Thread tigerThread = Create(tigerScan_Task, Exited);
            tigerThread.Start();
        }

        /// <summary>
        /// 実際にタイガースキャンを行うスレッド用のメソッドです。
        /// このメソッドを直接呼び出さないでください。
        /// </summary>
        private void tigerScan_Task() { 
            if(Started != null) {
                Started(this, new EventArgs());
            }


        }
    }
}
