using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

using NagaraStage;
using NagaraStage.Parameter;

namespace NagaraStage {
    namespace IO {
        namespace Driver {
            /// <summary>
            /// VP910のために用いるメソッドを集めた静的クラスです．
            /// </summary>
            /// <author>Hirokazu Yokoyama</author>
            static class VP910 {

                public const string ParameterFilePath = "Configure\\videolut.prm";

                /// <summary>
                /// ビデオ取り込みのLUTを設定します．
                /// <para>このメソッドはSony XCHR300の21モードの時のみ使用してください．</para>
                /// </summary>
                /// <param name="videoDeviceId">使用するカメラデバイス</param>
                /// <param name="fileName">パラメータファイルへのパス</param>
                /// <exception cref="IOException">ファイルの読み込みに失敗した場合</exception>
                /// <exception cref="System.Exception"></exception>
                public static void InitializeLUT(int videoDeviceId, string fileName = ParameterFilePath) {
                    string line;
                    const int Length = 256;
                    char[] delimiterChars = { ' ', '\t' };
                    int[] p0lut = new int[Length];
                    int[] p1lut = new int[Length];
                    int sts;
                    int[] optVals = { 3, 0 };
                    string[] args = new string[2];


                    try {
                        StreamReader sr = File.OpenText(fileName);
                        sr.ReadLine();

                        for (int i = 0; i < Length; ++i) {
                            line = sr.ReadLine();
                            args = line.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                            p0lut[i] = short.Parse(args[0]);
                            p1lut[i] = short.Parse(args[1]);
                        }
                    } catch (IOException ex) {
                        throw ex;
                    } catch (Exception ex) {
                        throw ex;
                    }

                    sts = VP910Define.WriteVideoLUT(videoDeviceId, ref p0lut[0], ref p1lut[0], 256, 0);
                    if (sts == 0) {
                        sts = VP910Define.SetVideoOpt(videoDeviceId, 0, 0, ref optVals[0], 8);
                    } else {
                        throw new Exception();
                    }
                }

                public static void InitializeCamera(EmulsionType emulsionType) {
                    int cameraShutterSpeed = 0;
                    int deviceId = Ipt.GetDeviceId();

                    VP910Define.SelectCamera(deviceId, 0, (int)Camera.CameraType);
                    VP910Define.SetTrigerMode(deviceId, 2);

                    switch (emulsionType) {
                        case EmulsionType.ThinType:
                            cameraShutterSpeed = 3;
                            break;
                        case EmulsionType.ThickType:
                            cameraShutterSpeed = 5;
                            break;
                        default:
                            // 該当なし
                            throw new Exception("Emulsion type value is not correct.");
                    }

                    VP910Define.SetShutterSpeed(deviceId, cameraShutterSpeed);

                }

            }
        }
    }
}