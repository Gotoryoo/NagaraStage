﻿/**
 * @author Hirokazu Yokoyama <o1007410@edu.gifu-u.ac.jp>
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using NagaraStage;
using NagaraStage.Parameter;
using NagaraStage.IO;

namespace NagaraStage.Activities {
    /// <summary>
    /// グリッドマークを自動探索する動作を行うクラスです．
    /// </summary>
    /// <author>Hirokazu Yokoyama</author>
    public class GridMarkSearch : Activity, IActivity {
        private static GridMarkSearch instance;

        /// <summary>
        /// グリッドマークが見つからなかったときのらせん移動探索を行う回数の初期値
        /// </summary>
        public const int NumOfSpiralSearchDefault = 30;

        /// <summary>
        /// グリッドマーク探索をするときのレンズの倍率
        /// </summary>
        public const int LensMagnification = 10;

        /// <summary>
        /// 自動探索が開始したときのイベント
        /// <para>グリッドマーク探索を行うスレッドで実行されます．</para>
        /// </summary>
        public event EventHandler<EventArgs> Started;

        /// <summary>
        /// グリッドマークが見つかった時のイベント
        /// <para>グリッドマーク探索を行うスレッドで実行されます．</para>
        /// </summary>
        public event EventHandler<GridMarkEventArgs> Found;

        /// <summary>
        /// グリッドマークが見つからなかったときのイベント
        /// <para>グリッドマーク探索を行うスレッドで実行されます．</para>
        /// </summary>
        public event EventHandler<GridMarkEventArgs> NotFound;

        /// <summary>
        /// 自動探索処理が終了したときのイベント
        /// <para>グリッドマーク探索を行うスレッドで実行されます．</para> 
        /// </summary>
        public event ActivityEventHandler Exited;

        /// <summary>
        /// グリッドマークを入力画像から認識するために用いるメソッドを設定，または取得します．
        /// <para>設定するメソッドは検出したグリッドマークの</para>
        /// </summary>
        public IGridMarkRecogner GridMarkRecoogner;

        private CoordManager coordManager;
        private ParameterManager parameterManager;
        private GridMarkDefinitionUtil utility;
        private int numOfSpiralSearch = Properties.Settings.Default.GridSpiral;

        /// <summary>
        /// グリッドマークの自動探索において，グリッドマークが見つからなかったときの
        /// らせん移動探索を行う回数を取得，または設定します．
        /// <para>0にした場合，らせん移動を行いません．負の値にした場合，らせん移動をグリッドマークが見つかるまでし続けます．</para>
        /// <para>ただし，グリッドマーク自動探索中に設定できません．</para>
        /// </summary>
        /// <exception cref="NagaraStage.Activities.InActionException"></exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public int NumOfSpiralSearch {
            get { return numOfSpiralSearch; }
            set {
                if (IsActive) {
                    throw new InActionException();
                }                

                numOfSpiralSearch = value;
                Properties.Settings.Default.GridSpiral = value;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_coordManager"></param>
        /// <param name="_parameterManager"></param>
        private GridMarkSearch(CoordManager _coordManager, ParameterManager _parameterManager)
            : base(_parameterManager) {
            this.coordManager = _coordManager;
            this.parameterManager = _parameterManager;
            this.GridMarkRecoogner = coordManager;
            this.utility = new GridMarkDefinitionUtil(coordManager);
        }

        /// <summary>
        /// 探索を実行するのに十分なグリッドマークが定義されているかどうかを取得します．
        /// </summary>
        public bool IsGridMarkedToStart {
            get {
                return (utility.IsMinimumDefined
                    && coordManager.GetGridMark(GridMarkPoint.CenterMiddle).Existed);
            }
        }

        /// <summary>
        /// インスタンスを取得します．
        /// </summary>
        /// <returns></returns>
        public static GridMarkSearch GetInstance(
            CoordManager coordManager = null,
            ParameterManager parameterManger = null) {
            if (instance == null) {
                instance = new GridMarkSearch(coordManager, parameterManger);
            }
            return instance;
        }

        /// <summary>
        /// グリッドマークの自動探索を開始します．
        /// <para>中央グリッドマークともう1カ所のグリッドマークが定義済みである必要があります．</para>
        /// <para>レンズタイプをX10にしておく必要があります．</para>
        /// </summary>
        /// <exception cref="NagaraStage.GridMarkFewException"></exception>
        /// <exception cref="NagaraStage.Activities.InActionException"></exception>
        /// <exception cref="NagaraStage.Parameter.LensTypeException"></exception>
        public void Start() {
            // 定義済みグリッドマークが少なすぎないかを確認
            if (!IsGridMarkedToStart) {
                throw new GridMarkFewException();
            }

            // レンズタイプがX10であるかを確認
            if (parameterManager.Magnification != LensMagnification) {
                throw new LensTypeException(LensMagnification);
            }

            Thread searchThread = Create(searchThread_Task, Exited);
            searchThread.Start();
        }

        /// <summary>
        /// searchThreadが行う処理です．このメソッドを直接呼び出さないでください．
        /// </summary>
        private void searchThread_Task() {
            if (Started != null) {
                Started(this, new EventArgs());
            }

            Camera camera = Camera.GetInstance();
            camera.Start();

            for (int i = coordManager.DefinedGridMarkNum; i < CoordManager.AllGridMarksNum; ++i) {
                GridMarkPoint presentMark = utility.NextPoint;
                Vector2 nextGridCoord = utility.GetGridMarkCoord(presentMark);                
                MotorControler mc = MotorControler.GetInstance(parameterManager);
                mc.MovePointXY(nextGridCoord, new Action(delegate {
                    mc.SetSpiralCenterPoint();
                }));
                mc.Join();

                /* グリッドマークを検出する */
                // 入力画像にて，グリッドマーク検出を行う．
                // 見つからなかった場合はNumOfSpiralSearchの回数分だけらせん移動を行い探す．
                bool continueFlag = true;
                int spiralCount = 0;
                while (continueFlag) {
                    GridMarkEventArgs eventArgs = new GridMarkEventArgs();
                    eventArgs.GridMarkPoint = presentMark;
                    try {
                        Led led = Led.GetInstance();
                        led.AdjustLight(parameterManager);
                        Vector2 viewerPoint = GridMarkRecoogner.SearchGridMark();
                        Vector2 encoderPoint = coordManager.TransToEmulsionCoord(viewerPoint);
                        mc.MovePointXY(encoderPoint);
                        mc.Join();
                        coordManager.SetGridMark(encoderPoint, presentMark, camera.ArrayImage);
                        continueFlag = false;
                        if (Found != null) {                            
                            Vector3 position = mc.GetPoint();
                            eventArgs.ViewerPoint = new Vector2(position.X, position.Y);
                            eventArgs.EncoderPoint = encoderPoint;
                            eventArgs.GridMarkPoint = presentMark;
                            Found(this, eventArgs);
                        }
                    } catch (GridMarkNotFoundException) {
                        mc.MoveInSpiral(true);
                        ++spiralCount;
                        continueFlag = (spiralCount < NumOfSpiralSearch);
                        if (!continueFlag && NotFound != null) {
                            NotFound(this, eventArgs);
                        }
                    } 
                } // while
            } // for

            // 座標系の生成
            coordManager.CreateCoordSystem();
        }
    }

}