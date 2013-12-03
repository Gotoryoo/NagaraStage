using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using NagaraStage.Parameter;
using NagaraStage.IO;

namespace NagaraStage {
    /// <summary>
    /// カメラをらせん状に動かすメソッドを持つクラスです．
    /// </summary>
    /// <author>Hirokazu Yokoyama</author>
    [Obsolete("MotorControlerに統合予定")]
    public class SpiralSearch {
        private static SpiralSearch instance;
        private ParameterManager parameterManager;
        private Vector2Int spiralPosition;
        private Vector3 defaultPosition;

        /// <summary>
        /// らせん移動をしたときのイベント
        /// </summary>
        public event EventHandler<SpiralEventArgs> SpiralMoved;

        /// <summary>
        /// 次の移動先を決定するインデックスを取得，または設定します．
        /// <para>
        /// この値は自動更新されるため，通常はこの値を設定する必要はありません．
        /// ただし，らせん移動において初期値からやり直す場合においては0を設定してください．
        /// </para>
        /// </summary>
        public int MovementIndex = 0;

        /// <summary>
        /// 螺旋移動した現在地を取得します．
        /// </summary>
        public Vector2Int SpiralPosition {
            get { return spiralPosition; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected SpiralSearch(ParameterManager _parameterManager) {
            this.parameterManager = _parameterManager;
        }

        /// <summary>
        /// SpiralSearchのインスタンスを取得します．
        /// </summary>
        /// <returns></returns>        
        public static SpiralSearch GetInstance(ParameterManager _parameterManager = null) {
            if (instance == null) {
                instance = new SpiralSearch(_parameterManager);
            }
            return instance;
        }

        /// <summary>
        /// らせん状移動を行います．
        /// <para>画面１つ分だけ動きます．周回するにはこのメソッドを複数回呼び出してください．</para>
        /// </summary>       
        /// <param name="wait">
        /// 移動処理の完了まで待機する場合はtrue，待機しない場合はfalse
        /// <para>trueにした場合，呼び出し側スレッドが移動完了まで停止することを留意する必要があります．</para>
        /// </param>
        /// <exception cref="MotorActiveException"></exception>
        public void Start(Boolean wait) {
            MotorControler mc = MotorControler.GetInstance();            
            if (mc.IsMoving) {
                throw new MotorActiveException();
            }

            if (MovementIndex == 0) {
                defaultPosition = mc.GetPoint();
            }

            ++MovementIndex;
            Vector2Int i = getSpiralPosition(MovementIndex);
            double mx = defaultPosition.X + i.X * parameterManager.SpiralShiftX;
            double my = defaultPosition.Y + i.Y * parameterManager.SpiralShiftY;
            mc.MovePointXY(mx, my, delegate {
                // 移動完了時のイベントハンドラを実行
                if (SpiralMoved != null) {
                    SpiralMoved(this, new SpiralEventArgs(i.X, i.Y));
                }
            });
            spiralPosition = i;            

            if (wait) {
                waitMoving();
            }
        }

        public void Abort() {
            MotorControler mc = MotorControler.GetInstance();
            mc.AbortMoving();
        }

        /// <summary>
        /// 現在地をらせん移動の中心地(基準)に設定します。
        /// </summary>
        public void SetOriginalPointHere() {
            MovementIndex = 0;
        }

        /// <summary>
        /// 元の位置に戻ります．
        /// </summary>
        /// <param name="wait">
        /// 移動処理の完了まで待機する場合はtrue，待機しない場合はfalse
        /// <para>trueにした場合，呼び出し側スレッドが移動完了まで停止することを留意する必要があります．</para>
        /// </param>
        /// <exception cref="MotorActiveException"></exception>
        public void BackToDefault(Boolean wait) {
            MovementIndex = 0;
            MotorControler mc = MotorControler.GetInstance();
            mc.MovePointXY(defaultPosition.X,  defaultPosition.Y);
            if (wait) {
                waitMoving();
            }
        }

        /// <summary>
        /// モータの移動完了まで待機します。
        /// </summary>
        private void waitMoving() {
            MotorControler mc = MotorControler.GetInstance();
            // モータの移動まで待機するスレッド
            Thread movingThread = new Thread(new ParameterizedThreadStart(delegate(object o) {
                while (mc.IsMoving) {
                    Thread.Sleep(10);
                }
            }));
            movingThread.Start();
            movingThread.Join();            
        }

        /// <summary>
        /// スパイラルファンクション
        /// <para>おそらくらせん状いおいて，次の位置を算出するメソッド</para>
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private Vector2Int getSpiralPosition(int n) {
            Vector2Int _spiral;
            
            int n1 = (int)((3 + Math.Sqrt(4 * n + 1)) / 4);
            int n2 = (int)((2 + Math.Sqrt(4 * n)) / 4); 
            int n3 = (int)((1 + Math.Sqrt(4 * n + 1)) / 4);
            int n4 = (int)(Math.Sqrt(n / 4));

            if (n <= 0) {
                _spiral = new Vector2Int(0, 0);
            } else if (n1 > n2) {
                int nA = (n1 - 1) * (-2) + 4 * n1 * (n1 - 1);
                int ix = -n1 + 1;
                int iy = ix + n - nA;
                _spiral = new Vector2Int(ix, iy);
            } else if (n2 > n3) {
                int nB = 1 + 4 * n2 * (n2 - 1);
                _spiral = new Vector2Int(-n1 + 1 + n - nB, n1);
            } else if (n3 > n4) {
                int nC = 2 + (n3 - 1) * 2 + 4 * n3 * (n3 - 1);
                _spiral = new Vector2Int(n1, n1 - n + nC);
            } else {
                int nD = 4 + (n4 - 1) * 4 + 4 * n4 * (n4 - 1);
                _spiral = new Vector2Int(n1 -n + nD, -n1);
            }
            return _spiral;
        } 
    }
}
