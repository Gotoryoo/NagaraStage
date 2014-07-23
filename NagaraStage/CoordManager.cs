/**
 * @author Hirokazu Yokoyama <o1007410@edu.gifu-u.ac.jp>
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using NagaraStage.Parameter;
using NagaraStage.IO;

using OpenCvSharp;
using OpenCvSharp.CPlusPlus;

namespace NagaraStage {
    /// <summary>
    /// グリッドマークおよび座標を管理するクラスです．
    /// </summary>
    /// <author email="o1007410@edu.gifu-u.ac.jp">Hirokazu Yokoyama</author>
    public class CoordManager : IGridMarkRecognizer {

        /// <summary>
        /// エマルションに存在するグリッドマークの個数
        /// </summary>
        public const int AllGridMarksNum = 9;
        public const int NumStep = 8;

        public const int NFineGridX = GridParameter.NFineGridX;
        public const int NFineGridY = GridParameter.NFineGridY;
        public const int NFineGridXY2 = GridParameter.NFineGridXY2;
        public const int NFineGridNum = GridParameter.NFineGridNum;
        public const int FineGridArea = GridParameter.FineGridArea;
        public const int FineGridArea2 = GridParameter.FineGridArea2;
        public const int FineGridStep = GridParameter.FineGridStep;
        public const int FineGridStep2 = GridParameter.FineGridStep2;
        public Boolean DefdBeamCo;

        /// <summary>
        /// すべてのグリッドマーク情報を保持する配列
        /// </summary>
        private GridMark[] gridMarks;

        /// <summary>
        /// グリッドの角度のズレ
        /// </summary>
        private double angleOfGrid = 0;

        /// <summary>
        /// グリッドの大きさ
        /// </summary>
        private double magnitOfGrid = 0;

        /// <summary>
        /// 3x3グリッドマークによる座標生成が既成かどうか
        /// </summary>
        private Boolean coordDefined = false;

        private double[,] gridOrgX = new double[4, 4];
        private double[,] gridOrgY = new double[4, 4];
        private double[,] gridOffsetX = new double[4, 4];
        private double[,] gridOffsetY = new double[4, 4];

        /// <summary>
        /// 全体のパラメータを管理するインスタンス
        /// </summary>
        private ParameterManager parameterManager;

        /// <summary>
        /// エマルションのパラメータを持つインスタンス
        /// </summary>
        private EmulsionParameter emulsionParameter;

        /// <summary>
        /// グリッドの角度(のズレ)を取得します．        
        /// </summary>
        public double AngleOfGrid {
            get { return angleOfGrid; }
        }

        /// <summary>
        /// グリッドの大きさを取得します．
        /// </summary>
        public double MagnitOfGrid {
            get { return magnitOfGrid; }
        }

        /// <summary>
        /// 定義された(見つかっている)グリッドマーク数を取得します．
        /// </summary>
        public int DefinedGridMarkNum {
            get {
                int num = 0;
                for (int i = 0; i < AllGridMarksNum; ++i) {
                    if (gridMarks[i].Existed) {
                        ++num;
                    }
                }

                return num;
            }
        }

        /// <summary>
        /// 座標系が既成かどうかを取得します．
        /// </summary>
        public Boolean CoordDefined {
            get { return coordDefined; }
        }

        /// <summary>
        /// 全てのグリッドマークが定義されたかどうかを取得します．
        /// <para>true: 定義した，false : 定義されていない</para>
        /// </summary>
        public Boolean AllMarkDefined {
            get {
                return (DefinedGridMarkNum >= 9 ? true : false);
            }
        }

        /// <summary>
        /// 乾板の変形を補正する値を格納します．最寄のグリッドマークの場所が予測からどれだけずれているかによって決定します．
        /// <para>unit of mm</para>
        /// </summary>
        private double hfdx, hfdy;

        public double HFDX {
            set { this.hfdx = value; }
            get { return hfdx; }
        }

        public double HFDY {
            set { this.hfdy = value; }
            get { return hfdy; }
        }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_parameterManager">値が設定されているParameterManagerのインスタンス</param>
        public CoordManager(ParameterManager _parameterManager) {
            this.parameterManager = _parameterManager;
            this.emulsionParameter = parameterManager.EmulsionParameter;
            this.gridMarks = new GridMark[AllGridMarksNum];
            for (int i = 0; i < gridMarks.Length; ++i) {
                this.gridMarks[i] = new GridMark();
            }
            this.gridOrgX = emulsionParameter.GridOrgX;
            this.gridOrgY = emulsionParameter.GridOrgY;
            this.hfdx = 0.0;
            this.hfdy = 0.0;
        }

        /// <summary>
        /// インスタンスのコピーを生成します．
        /// </summary>
        /// <returns>インスタンスのコピー</returns>
        public CoordManager Clone() {
            CoordManager clone = new CoordManager(parameterManager);
            clone.DefdBeamCo = DefdBeamCo;
            clone.angleOfGrid = angleOfGrid;
            clone.magnitOfGrid = magnitOfGrid;
            clone.coordDefined = coordDefined;
            clone.gridOffsetX = gridOffsetX;
            clone.gridOffsetY = gridOffsetY;
            clone.gridOrgX = gridOrgX;
            clone.gridOrgY = gridOrgY;           
            for (int i = 0; i < clone.gridMarks.Length; ++i) {
                clone.gridMarks[i] = new GridMark();
                clone.gridMarks[i].x = gridMarks[i].x;
                clone.gridMarks[i].y = gridMarks[i].y;
                clone.gridMarks[i].Existed = gridMarks[i].Existed;
                clone.gridMarks[i].Image = gridMarks[i].Image;
            }
            return clone;
        }

        /// <summary>
        /// コンピュータ画面上のピクセル値の座標からエマルションの座標値に変換します．
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>エマルションでの座標値</returns>
         public Vector2 TransToEmulsionCoord(int x, int y) {
             MotorControler mc = MotorControler.GetInstance();
             int emX = ParameterManager.ImageResolution.Width;
             int emY = ParameterManager.ImageResolution.Height;
             Vector3 motorPosition = mc.GetPoint();
             Vector2 p = new Vector2();
             p.X = motorPosition.X - (x - emX / 2) * parameterManager.CameraMainResolution;
             p.Y = motorPosition.Y + (y - emY / 2) * parameterManager.CameraSubResolution;
             return p;
        }

         public Vector2 TransToEmulsionCoord(Vector2 point) {
                return TransToEmulsionCoord((int)point.X, (int)point.Y);
         }

        /// <summary>
        /// これまで設定したグリッドマークの座標を初期化(消去)します．
        /// </summary>
        public void InitializeGridMarks() { 
            gridMarks = new GridMark[AllGridMarksNum];
            for (int i = 0; i < gridMarks.Length; ++i) {
                gridMarks[i] = new GridMark();
            }
        }       

        /// <summary>
        /// 指定座標をグリッドマークとして設定します．
        /// <para>座標値は必ずエマルションでの座標を指定してください。</para>
        /// <para>ただし，50倍向けに補正された座標が設定されます．</para>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="gridMarkPoint">設定するグリッドマーク</param>
        /// <param name="exsited">グリッドマークを発見したと設定するかどうか</param>
        public void SetGridMark(double x, double y, GridMarkPoint gridMarkPoint, Boolean exsited = true) {
            Vector2 correctedPoint = getCorrectedGapCoordinate(new Vector2(x, y));
            gridMarks[(int)gridMarkPoint] = new GridMark();
            gridMarks[(int)gridMarkPoint].x = correctedPoint.X;
            gridMarks[(int)gridMarkPoint].y = correctedPoint.Y;
            gridMarks[(int)gridMarkPoint].Existed = exsited;
        }

        public void SetGridMark(double x, double y, GridMarkPoint gridMarkPoint, byte[] image, Boolean exsited = true) {
            SetGridMark(x, y, gridMarkPoint, exsited);
            gridMarks[(int)gridMarkPoint].Image = image;
        }

        public void SetGridMark(Vector2 point, GridMarkPoint gridMarkPoint, Boolean exsited = true) {
            SetGridMark(point.X, point.Y, gridMarkPoint, exsited);
        }

        public void SetGridMark(Vector2 point, GridMarkPoint gridMarkPoint, byte[] image, Boolean exsited = true) {
            SetGridMark(point.X, point.Y, gridMarkPoint, image, exsited);
        }

        /// <summary>
        /// グリッドマークを入力中の映像から探し出し,その座標を返します．
        /// <para>発見に失敗した場合はGridMakrNotFoundExceptionが投げられます．</para>
        /// </summary>
        /// <returns>グリッドマークの座標</returns>
        /// <exception cref="LensTypeException">レンズが10倍に設定されていない場合</exception>
        /// <exception cref="GridMarkNotFoundException">グリッドマークの探索に失敗した場合</exception>
        public Vector2 SearchGridMark() {
            int status = 0;
            // レンズが10倍に設定されていない場合は例外を返す
            if (parameterManager.Magnification != ParameterManager.LensMagnificationOfGridMarkSearch) {
                throw new LensTypeException(ParameterManager.LensMagnificationOfGridMarkSearch);
            }


            Camera c = Camera.GetInstance();
            byte[] b = c.ArrayImage;
            Mat mat = new Mat(440, 512, MatType.CV_8U, b);

            Cv2.GaussianBlur(mat, mat, Cv.Size(5, 5), -1);
            //Cv2.Threshold(mat, mat, 60, 255, ThresholdType.BinaryInv);
            Cv2.Threshold(mat, mat, 60, 1, ThresholdType.BinaryInv);

            //m.ImWrite(@"c:\iiiiii.bmp");
            //Cv2.ImShow("aaaa", mat);

            Moments mom = new Moments(mat);
            if (mom.M00 == 0) status++;
            if (mom.M00 < 500) status++;
            if (status != 0) {
                throw new GridMarkNotFoundException();
            }

            double cx = mom.M10 / mom.M00;
            double cy = mom.M01 / mom.M00;
            Mat innercir = new Mat(440, 512, MatType.CV_8U);
            Cv2.Circle(innercir, new Point(cx, cy), 10, new Scalar(255, 255, 255), 3);
            int innerpath = Cv2.CountNonZero(innercir);
            Cv2.BitwiseAnd(innercir, mat, innercir);
            int innersum = Cv2.CountNonZero(innercir);
            //Cv2.ImShow("inner", innercir);

            Mat outercir = new Mat(440, 512, MatType.CV_8U);
            Cv2.Circle(outercir, new Point(cx, cy), 40, new Scalar(255, 255, 255), 3);
            int outerpath = Cv2.CountNonZero(outercir);
            Cv2.BitwiseAnd(outercir, mat, outercir);
            int outersum = Cv2.CountNonZero(outercir);
            //Cv2.ImShow("outer", outercir);

            double innerratio = innersum * 1.0 / innerpath * 1.0;
            double outerratio = outersum * 1.0 / outerpath * 1.0; 
            if (innerratio < 0.7) status++;
            if (outerratio > 0.2) status++;
            //System.Diagnostics.Debug.WriteLine(String.Format("{0}, {1}, {2}", innerratio, outerratio, mom.M00));

            if (status != 0) {
                throw new GridMarkNotFoundException();
            }
            return new Vector2(cx, cy);

//#if !NoHardware
//            int gridMarkSize = (int)parameterManager.GridMarkSize;
//            int status = new CameraUtil().MarkCenter(ref x, ref y, gridMarkSize);
//            if (status != 0) {
//                throw new GridMarkNotFoundException();
//            }
//#endif
        }

        /// <summary>
        /// この関数を実行すると，カメラから画像を取得し，グリッドマークを検出しその座標を返します．
        /// 実行時のレンズはx50対物であることを前提とします.
        /// </summary>
        /// <returns>グリッドマークを検出したピクセル座標。検出できなかった時は(-1,-1)が返される</returns>        
        public Vector2 SearchGridMarkx50() {
            int status = 0;

            Camera c = Camera.GetInstance();
            byte[] b = c.ArrayImage;
            Mat mat = new Mat(440, 512, MatType.CV_8U, b);

            Cv2.GaussianBlur(mat, mat, Cv.Size(5, 5), -1);
            Cv2.Threshold(mat, mat, 60, 1, ThresholdType.BinaryInv);

            Moments mom = new Moments(mat);
            if (mom.M00 == 0) status++;
            if (mom.M00 < 600) status++;//
            if (status != 0) {
                return new Vector2(-1.0, -1.0);
            }

            double cx = mom.M10 / mom.M00;
            double cy = mom.M01 / mom.M00;
            Mat innercir = new Mat(440, 512, MatType.CV_8U);
            Cv2.Circle(innercir, new Point(cx, cy), 30, new Scalar(255, 255, 255), 3);
            int innerpath = Cv2.CountNonZero(innercir);
            Cv2.BitwiseAnd(innercir, mat, innercir);
            int innersum = Cv2.CountNonZero(innercir);

            Mat outercir = new Mat(440, 512, MatType.CV_8U);
            Cv2.Circle(outercir, new Point(cx, cy), 200, new Scalar(255, 255, 255), 3);
            int outerpath = Cv2.CountNonZero(outercir);
            Cv2.BitwiseAnd(outercir, mat, outercir);
            int outersum = Cv2.CountNonZero(outercir);

            double innerratio = innersum * 1.0 / innerpath * 1.0;
            double outerratio = outersum * 1.0 / outerpath * 1.0;
            if (innerratio < 0.8) status++;
            if (outerratio > 0.2) status++;
            if (status != 0) {
                return new Vector2(-1.0, -1.0);
            }
            return new Vector2(cx, cy);
        }


        /// <summary>
        /// 定義したグリッドマークの値をすべて削除します．
        /// </summary>
        public void CleaerGridMark() {
            for (int i = 0; i < gridMarks.Length; ++i) {
                gridMarks[i] = new GridMark();
            }
        }

        /// <summary>
        /// グリッドマークの情報を取得します．
        /// </summary>
        /// <param name="gridMarkPoint">取得するグリッドマーク</param>
        /// <returns>グリッドマークの情報</returns>
        public GridMark GetGridMark(GridMarkPoint gridMarkPoint) { 
            return gridMarks[(int)gridMarkPoint];
        }

        /// <summary>
        /// グリッドマークを基準に座標系を作成します．
        /// </summary>
        /// <exception cref="Exception">定義されたグリッドマーク数が少ない場合</exception>
        public void CreateCoordSystem() {            
            double x = new double();
            double y = new double();
            double emX, emY;
            Vector2Int iId;
            Vector2 offset;

            try {
                setAngleAndMagnitOfGrid();
                offset = calcOffset();
            } catch (Exception ex) {
                throw ex;
            }

            // 原点を設定
            gridOffsetX[1, 1] = 0;
            gridOffsetY[1, 1] = 0;

            for (int i = 1; i < AllGridMarksNum; ++i) {
                iId = getXYGridId(i);
                if (gridMarks[i].Existed) {
                    emX = gridMarks[i].x - offset.X;
                    emY = gridMarks[i].y - offset.Y;
                    Ipt.MtoG(0, emX, emY, ref x, ref y);
                    iId.X = (int)(((x + 100) / 100) + 0.5);
                    iId.Y = (int)(((y + 100) / 100) + 0.5);
                    gridOffsetX[iId.X, iId.Y] = x - gridOrgX[iId.X, iId.Y];
                    gridOffsetY[iId.X, iId.Y] = y - gridOrgY[iId.X, iId.Y];
                } else {
                    gridOffsetX[iId.X, iId.Y] = 0;
                    gridOffsetX[iId.X, iId.Y] = 0;
                }
            }            

            /* 新しい原点のためにモータコントロールボードを初期化する．*/
            // 新しい原点に移動を開始し，別スレッドで移動完了を感知したら，
            // モータコントロールボードを初期化し原点を再設定する
            MotorControler mc = MotorControler.GetInstance();
            mc.MovePointXY(offset.X, offset.Y);
            
            Thread thread = new Thread(new ParameterizedThreadStart(delegate(object o) {
                MotorControler motor = MotorControler.GetInstance();
                while(motor.IsMoving) {
                    // モータの移動中は待機
                    Thread.Sleep(10);
                }

                // モータコントロールボードを初期化し，原点を設定する
                //motor.InitializeMotorControlBoard(MechaAxisAddress.XAddress);
                //motor.InitializeMotorControlBoard(MechaAxisAddress.YAddress);
                //motor.InitializeMotorControlBoard(MechaAxisAddress.ZAddress);
                coordDefined = true;
            }));

            try {
                thread.Start();
            } catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// 与えられたエンコーダ座標系の地点から最も近いグリッドマーク情報を取得します。
        /// </summary>
        /// <param name="point">エンコーダ座標系の座標</param>
        /// <returns>最も近いグッドマークをエンコーダ座標系で</returns>
        public GridMark GetTheNearestGridMark(Vector3 encoderPoint) {
            GridParameter gridParam = parameterManager.GridParameter;
            if(gridParam.LoadedGridOriginalFine==false){
                throw new Exception("null");
            }
            GridMark retval = new GridMark();
            Vector2 pmover = new Vector2();
            //Vector2 gmover = new Vector2();
            Vector2 gstage = new Vector2();

            try {
                double[,] gridOriginalFineX = gridParam.GridOriginalFineX;
                double[,] gridOriginalFineY = gridParam.GridOriginalFineY;

                double minDistance = 99999999.9;

                for (int ix = 0; ix < gridOriginalFineX.GetLength(0) ; ++ix ) {
                   for (int iy = 0; iy < gridOriginalFineX.GetLength(1) ; ++iy ) {

                       

                       double distanceX = gridOriginalFineX[ix, iy] - encoderPoint.X;
                       double distanceY = gridOriginalFineY[ix, iy] - encoderPoint.Y;
                        double distance = Math.Sqrt(distanceX * distanceX + distanceY * distanceY);
                        if(distance < minDistance) {
                            minDistance = distance;
                            retval.x = gridOriginalFineX[ix,iy];
                            retval.y = gridOriginalFineY[ix, iy];
                        }
                    }
                }
            } catch {
                throw new Exception("null");
            }
            //Ipt.MtoG(0, gmover.X, gmover.Y, ref gstage.X, ref gstage.Y);
            Ipt.GToM("p", retval.x,retval.y, ref gstage.X, ref gstage.Y);
            retval.x = gstage.X;
            retval.y = gstage.Y;
            return retval;
        }


        /// <summary>
        /// 10倍レンズから50倍レンズのグリッドマークの位置のズレを補正した座標を返します．
        /// </summary>
        /// <param name="x10Point">10倍レンズでの座標</param>
        /// <returns>50倍レンズでの座標</returns>
        private Vector2 getCorrectedGapCoordinate(Vector2 x10Point) {
            double x = x10Point.X - parameterManager.LensOffsetX;
            double y = x10Point.Y - parameterManager.LensOffsetY;
            return new Vector2(x, y);
        }

        /// <summary>
        /// グリッドの大きさと角度を定義されたグリッドマークから算出します．
        /// </summary>
        /// <exception cref="System.Exception">定義されたグリッドマーク数が少ない場合</exception>
        private void setAngleAndMagnitOfGrid() {
            double distOrigin = 0, distNow = 0;
            double thetaOrigin = 0, thetaNow = 0, theta = 0;
            double dx, dy;
            Vector2Int iId, jId;
            int combinationNum = 0;

            // 定義されたグリッドマーク数が少ない場合は例外を返す
            int num = DefinedGridMarkNum;
            if (num < 2) {
                throw new Exception("Grid mark data is few(" + num.ToString() + ").");
            }            

            for (int i = 0; i < AllGridMarksNum; ++i) {
                for (int j = i + 1; j < AllGridMarksNum; ++j) {
                    if (gridMarks[i].Existed) {
                        dx = gridMarks[i].x - gridMarks[j].x;
                        dy = gridMarks[i].y - gridMarks[j].y;
                        distNow += Math.Sqrt(dx * dx + dy * dy);
                        thetaNow = (Math.Abs(dx) > 5 ? Math.Atan(dy / dx) : -Math.Atan(dx / dy));
                        iId = getXYGridId(i);
                        jId = getXYGridId(j);
                        dx = gridOrgX[iId.X, iId.Y] - gridOrgX[jId.X, jId.Y];
                        dy = gridOrgY[iId.X, iId.Y] - gridOrgY[jId.X, jId.Y];
                        distOrigin += Math.Sqrt(dx * dx + dy * dy);
                        thetaOrigin = (Math.Abs(dx) > 5 ? Math.Atan(dy / dx) : -Math.Atan(dx / dy));
                        theta += (thetaNow - thetaOrigin);
                        ++combinationNum;
                    } // if END
                } // for j END
            } // for i END             

            magnitOfGrid = distNow / distOrigin;
            angleOfGrid = theta / combinationNum;
            Ipt.SetGridLocal(
                (short)parameterManager.PlateNo, 
                magnitOfGrid, 
                angleOfGrid, 
                parameterManager.EmulsionIndexUp, 
                parameterManager.EmulsionIndexDown);
            System.Diagnostics.Debug.WriteLine(String.Format("mag: {0}, theta: {1}", magnitOfGrid, angleOfGrid));
        }

        /// <summary>
        /// 座標の補正値を算出して，返します．
        /// </summary>
        /// <returns>座標の補正値</returns>
        private Vector2 calcOffset() {
            Vector2 offset = new Vector2();
            Vector2Int iId = new Vector2Int();
            double emX = new double();
            double emY = new double();

            // 定義されたグリッドマーク数が少ない場合は例外を返す
            int num = DefinedGridMarkNum;
            if (num < 2) {
                throw new Exception("Grid mark data is few(" + num.ToString() + ").");
            } 

            if (gridMarks[(int)GridMarkPoint.CenterMiddle].Existed) {
                offset.X = gridMarks[(int)GridMarkPoint.CenterMiddle].x;
                offset.Y = gridMarks[(int)GridMarkPoint.CenterMiddle].y;
            } else {
                for (int i = 1; i < AllGridMarksNum; ++i) {
                    if (gridMarks[i].Existed) {
                        iId = getXYGridId(i);
                        Ipt.GToM("0", gridOrgX[iId.X, iId.Y], gridOrgY[iId.X, iId.Y], ref emX, ref emY);
                        offset.X += gridMarks[i].x - emX;
                        offset.Y += gridMarks[i].y - emY;
                    }
                }
                offset.X /= DefinedGridMarkNum;
                offset.Y /= DefinedGridMarkNum;
            }

            return offset;
        }

        private Vector2Int getXYGridId(int index) {
            Vector2Int v = new Vector2Int();
            switch (index) { 
                case 0:
                    v = new Vector2Int(1, 1);
                    break;
                case 1:
                    v = new Vector2Int(0, 0);
                    break;
                case 2:
                    v = new Vector2Int(1, 0);
                    break;
                case 3:
                    v = new Vector2Int(2, 0);
                    break;
                case 4:
                    v = new Vector2Int(2, 1);
                    break;
                case 5:
                    v = new Vector2Int(2, 2);
                    break;
                case 6:
                    v = new Vector2Int(1, 2);
                    break;
                case 7:
                    v = new Vector2Int(0, 2);
                    break;
                case 8:
                    v = new Vector2Int(0, 1);
                    break;
                default:
                    throw new ArgumentException("index value needs in 0 - 8.");
            }

            return v;
        }
    }
}

