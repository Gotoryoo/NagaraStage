using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NagaraStage {
    namespace Parameter {
        /// <summary>
        /// 追跡中のTrack，Trackのプロファイルなど追跡に必要なパラメータの管理を行うクラスです．
        /// <para>
        /// このクラスはエマルションのTrackの値など，複数のTrackのパラメータを管理するだけのクラスです．
        /// 従って，このクラスに「次のTrackの座標までモータを駆動させる」といったパラメータ管理以上の
        /// 処理をさせるメソッド，プロパティなどを追加してはいけません．
        /// </para>
        /// </summary>
        public class TracksManager {

            private enum NextTrackStatus {
                Error = -1,
                Finished = 0,
                NoProblem = 1
            }

            private ParameterManager parameterManager;
            private EmulsionParameter emParameter;
            private int presentTrackIndex = 0;
            private int numOfTracks = 0;
            private Boolean initialized = false;
            private Boolean first = true;

            /// <summary>
            /// Track
            /// </summary>
            private List<Track> tracks = new List<Track>();

            /// <summary>
            /// 現在追跡中のTrackを取得します．
            /// </summary>
            /// <exception cref="IndexOutOfRangeException">初期化されていないTrackが参照された場合</exception>
            public Track Track {
                get {
                    if (presentTrackIndex >= tracks.Count) {
                        //throw new IndexOutOfRangeException();
                    }

                    if (tracks[presentTrackIndex] == null) {
                        throw new IndexOutOfRangeException("Track information is not loaded.");
                    }
                    return tracks[presentTrackIndex];
                }
            }

            /// <summary>
            /// 現在追跡中のTrackがファイル内で何番目のTrackであるのかを取得します．
            /// または，何番目のTrackを追跡するのかを設定します．
            /// </summary>
            /// <exception cref="System.ArgumentOutOfRangeException"></exception>
            public int TrackingIndex {
                get { return presentTrackIndex; }
                set {
                    if (value >= tracks.Count) {
                        throw new ArgumentOutOfRangeException("out of range tracks index.");
                    }
                    presentTrackIndex = value;
                }
            }

            /// <summary>
            /// 現在のエマルションに存在するTrack数を取得します．
            /// <para>このプロパティを参照する前にInitializeメソッドを実行する必要があります．</para>
            /// </summary>
            /// <exception>初期化されていない場合</exception>
            public int NumOfTracks {
                get {
                    if (!initialized) {
                        throw new Exception("TracksManager is not initialized.");
                    }
                    return numOfTracks;
                }
            }

            /// <summary>
            /// 初期化済み(データが読み込み済み)であるかを取得します．
            /// </summary>
            public Boolean IsInitialized {
                get { return initialized; }
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="_parameterManager"></param>
            public TracksManager(ParameterManager _parameterManager) {
                this.parameterManager = _parameterManager;
                this.emParameter = parameterManager.EmulsionParameter;
            }

            /// <summary>
            /// Trackの定義ファイルからすべてのTrack情報を読み込んで初期化します．
            /// </summary>
            public void Initialize() {
                Boolean flag = true;
                numOfTracks = 0;
                while (flag) {
                    try {
                        readNextTrack();
                        ++numOfTracks;
                    } catch (TrackNotExistException) {
                        flag = false;
                    }
                }
                initialized = true;
                presentTrackIndex = 0;
            }

            /// <summary>追跡するトラックを更新し，そのトラックを取得します．</summary>
            /// <returns>次に追跡するTrack</returns>
            /// <exception cref="System.ArgumentOutOfRangeException"></exception>
            public Track UpdateTrack() {
                if (first) {
                    TrackingIndex = 0;
                    first = false;
                } else {
                    ++TrackingIndex;
                }
                return Track;
            }

            /// <summary>追跡するtrackを更新し，次のtrackにします．</summary>
            /// <param name="mode">モード(0 or 1)</param>
            /// <exception cref="System.Exception">You need to open a scan file.</exception>
            /// <exception cref="System.IO.IOException">Invalid data.</exception>
            /// <exception cref="TrackNotExistException">全てのtrackを完了し，次のtrackがない場合</exception>
            /// <exception cref="IOException">データが不正であった場合</exception>
            private void readNextTrack(int mode = 1) {
                short[] id = new short[Track.NumOfPlate];
                short direction = new short();
                char[] comment = new char[256];
                double gx0 = new double(), gy0 = new double();
                double gdxdz0 = new double(), gdydz0 = new double();
                int status = 0;
                double deltaZ = -emParameter.GelThick;

                if (parameterManager.OpenedInFileMode == OpenInFileMode.None) {
                    throw new Exception("You need to open a scan file.");
                }

                if (mode == 0) {
                    status = Ipt.GetNextTrack(deltaZ, ref id[0], ref direction, ref gx0, ref gdxdz0, ref gy0, ref gdydz0, comment[0]);
                } else if (mode == 1) {
                    status = Ipt.GetTrackOfId(deltaZ, ref id[0], ref direction, ref gx0, ref gdxdz0, ref gy0, ref gdydz0, comment[0]);
                }

                switch (status) {
                    case -1: // Error
                        throw new IOException("Invalid data.");
                    case 0: // Scan finished.
                        Ipt.SendMessage("Whole scanning End!!");
                        string str = (mode == 0 ? "Scanning finished at all." : "Scan data dose not exists.");
                        throw new TrackNotExistException(str);
                    default:
                        Track t = new Track(numOfTracks, id, direction, gx0, gy0, gdxdz0, gdydz0, new String(comment));
                        tracks.Add(t);
                        presentTrackIndex = tracks.Count - 1;
                        break;
                }
            }

            /// <summary>
            /// Trackを取得します．
            /// </summary>
            /// <param name="index">Track番号</param>
            /// <returns></returns>
            public Track GetTrack(int index) {
                if (index >= tracks.Count) {
                    throw new ArgumentOutOfRangeException();
                }
                return tracks[index];
            }
        }
    }
}