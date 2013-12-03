using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using NagaraStage;
using NagaraStage.Parameter;
using NagaraStage.IO;

namespace NagaraStage.Activities {
    /// <summary>高速表面認識を行うクラスです．Surfaceクラスを内部的に継承しています．</summary>
    /// <seealso cref="NagaraStage.Activities.Surface" />
    public class FastSurface : Activity, IActivity, ISurface {
        private static FastSurface instance;
        private Surface parent;
        private ParameterManager parameterManager;
        private double[] surfaces = new double[4];
        private double searchRange = 1;
        private double everyRange = 0.02;
        private DateTime dateTime;

        private FastSurface(ParameterManager _parameterManager) 
            : base(_parameterManager) {
                this.parameterManager = _parameterManager;
                parent = Surface.GetInstance(_parameterManager);
                surfaces[0] = parent.UpTop;
                surfaces[1] = parent.UpBottom;
                surfaces[2] = parent.LowTop;
                surfaces[3] = parent.LowBottom;
        }

        /// <summary>
        /// インスタンスを取得します．
        /// </summary>
        /// <returns>インスタンス</returns>
        public static FastSurface GetInstance(ParameterManager parameterManager) {
            if (instance == null) {
                instance = new FastSurface(parameterManager);
            }
            return instance;
        }

        /// <summary><see cref="ISurface.LowBottomRecognized" /></summary>
        public event EventHandler<ActivityEventArgs> LowBottomRecognized;

        /// <summary><see cref="ISurface.LowTopRecognized" /></summary>
        public event EventHandler<ActivityEventArgs> LowTopRecognized;

        /// <summary><see cref="ISurface.UpBottomRecognized" /></summary>
        public event EventHandler<ActivityEventArgs> UpBottomRecognized;

        /// <summary><see cref="ISurface.UpTopRecognized" /></summary>
        public event EventHandler<ActivityEventArgs> UpTopRecognized;

        /// <summary><see cref="ISurface.Exited" /></summary>
        public event ActivityEventHandler Exited;

        /// <summary>上ゲル上面の座標を取得します．</summary>
        public double UpTop {
            get {
                if (this.dateTime.CompareTo(parent.DateTime) == -1) {
                    surfaces[0] = parent.UpTop;
                }
                return surfaces[0]; 
            }
        }

        /// <summary>上ゲル下面の座標を取得します．</summary>
        public double UpBottom {
            get {
                if (this.dateTime.CompareTo(parent.DateTime) == -1) {
                    surfaces[1] = parent.UpBottom;
                }
                return surfaces[1];
            }
        }

        /// <summary>下ゲル上面の座標を取得します．</summary>
        public double LowTop {
            get {
                if (this.dateTime.CompareTo(parent.DateTime) == -1) {
                    surfaces[2] = parent.LowTop;
                }
                return surfaces[2];
            }
        }

        /// <summary>下ゲル下面の座標を取得します．</summary>
        public double LowBottom {
            get {
                if (this.dateTime.CompareTo(parent.DateTime) == -1) {
                    surfaces[3] = parent.LowBottom;
                }
                return surfaces[3];
            }
        }

        /// <summary>表面認識の実行時間を取得します．</summary>
        public DateTime DateTime {
            get { return dateTime; }
        }

        /// <summary>現在撮影中の位置がゲルの中であるかどうかを取得します．</summary>
        /// <returns><c>true</c> ゲルの中似る場合; そうでなければ, <c>false</c>.</returns>
        public bool IsInGel() {
            return parent.IsInGel(); 
        }

        /// <summary>
        /// 表面認識を開始します．前回のゲルの境界面に基づいて高速な表面認識処理を行います．
        /// ただし，今回が初めての表面認識処理の場合は実行できません．
        /// </summary>
        public void Start() {
            Thread recogThread = Create(recogThread_Task, Exited);
            recogThread.Start();
        }

        private void recogThread_Task() {
            Camera camera = Camera.GetInstance();
            camera.Start();
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            surfaces[0] = UpTop;
            surfaces[1] = UpBottom;
            surfaces[2] = LowTop;
            surfaces[3] = LowBottom;

            // 現在位置が，前回の表面認識結果に基づいて下ゲル以下にいる場合は下ゲル下部から上方向に順に表面認識を行う．
            // 一方，ベース及び上ゲル以上にいる場合は上ゲル上部から下方向に順に表面認識を行う．
            // そのためにforループの条件式を設定する
            GelLayer layerNow = getWhichLayerNow();
            int i, end;
            Action fomula;
            if (layerNow == GelLayer.Under || layerNow == GelLayer.UnderOut) {
                i = surfaces.Length;
                end = 0;
                fomula = (() => --i);
            } else {
                i = 0;
                end = surfaces.Length;
                fomula = (() => ++i);
            }

            for (i = 0; i < end; fomula() ) {
                mc.MovePointZ(surfaces[i]);
                mc.Join();
                bool presentResult = false, previousResult = IsInGel();
                bool[] votes = new bool[parent.NumOfVoting];
                int counter = 1;
                while(presentResult == previousResult) {
                    previousResult = presentResult;
                    mc.MovePointZ(surfaces[i] + everyRange * counter);
                    while (mc.Moving) {
                        for (int j = 0; j < votes.Length; ++j) {
                            votes[i] = IsInGel();
                            Thread.Sleep(parent.DelayTime);
                        }                        
                        presentResult = Surface.Vote(votes);
                        if (presentResult != previousResult) {
                            mc.AbortMoving();                            
                        }
                    }
                    counter *= -1;
                    counter += (counter > 0 ? 1 : 0);
                    if (Math.Abs(everyRange * counter) > searchRange) {
                        throw new SurfaceFailedException("Distance limit out.");
                    }
                }
                surfaces[i] = mc.GetPoint().Z;
                // 境界面認識時のイベントを発生させる．
                ActivityEventArgs eventArgs = new ActivityEventArgs();
                eventArgs.ZValue = surfaces[i];
                switch (i) {
                    case 3:
                        if (LowBottomRecognized != null) {
                            LowBottomRecognized(this, eventArgs);
                        }
                        break;
                    case 2:
                        if (LowTopRecognized != null) {
                            LowTopRecognized(this, eventArgs);
                        }
                        break;
                    case 1:
                        if (UpBottomRecognized != null) {
                            UpBottomRecognized(this, eventArgs);
                        }
                        break;
                    case 0:
                        if (UpTopRecognized != null) {
                            UpTopRecognized(this, eventArgs);
                        }
                        break;
                }
            }
            dateTime = DateTime.Now;
        }

        /// <summary>
        /// 現在，どこのゲルにいるのかを現在の座標値と，前回調べた境界面の座標値から算出します．
        /// </summary>
        /// <returns>現在いるゲル</returns>
        private GelLayer getWhichLayerNow() {
            MotorControler mc = MotorControler.GetInstance(parameterManager);
            double z = mc.GetPoint().Z;
            GelLayer gelLayer;
            if (z > UpTop) {
                gelLayer = GelLayer.OverOut;
            } else if (z <= UpTop && z >= UpBottom) {
                gelLayer = GelLayer.Over;
            } else if (z < UpBottom && z > LowTop) {
                gelLayer = GelLayer.Base;
            } else if (z <= LowTop && z >= LowBottom) {
                gelLayer = GelLayer.Under;
            } else if (z < LowBottom) {
                gelLayer = GelLayer.UnderOut;
            } else {
                gelLayer = GelLayer.Unknown;
            }
            return gelLayer;
        }
    }
}
