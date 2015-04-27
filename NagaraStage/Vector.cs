using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
namespace NagaraStage {

    /// <summary>方向を表す列挙体です．</summary>
    public enum VectorId { 
        /// <summary>
        /// X軸，X方向を示します．
        /// </summary>
        X = 0,

        /// <summary>
        /// Y軸，Y方向を示します．
        /// </summary>
        Y = 1,

        /// <summary>
        /// Z軸，Z方向を示します．
        /// </summary>
        Z = 2
    }


    /// <summary>
    /// 2次元ベクトル(x, y)のdouble型構造体です．
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2 {
        /// <summary>
        /// X方向の値
        /// </summary>
        public double X;

        /// <summary>
        /// Y方向の値
        /// </summary>
        public double Y;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2" /> struct.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public Vector2(double x, double y) {
            this.X = x;
            this.Y = y;
        }

        public static Vector2 operator+(Vector2 a, Vector2 b) {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b) {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }

        public static bool operator >(Vector2 a, Vector2 b) {
            return (a.X > b.X && a.Y > b.Y);
        }

        public static bool operator <(Vector2 a, Vector2 b) {
            return (a.X < b.X && a.Y < b.Y);
        }

        public static bool operator >=(Vector2 a, Vector2 b) {
            return (a.X >= b.X && a.Y >= b.Y);
        }

        public static bool operator <=(Vector2 a, Vector2 b) {
            return (a.X <= b.X && a.Y <= b.Y);
        }

        public static bool operator ==(Vector2 a, Vector2 b) {
            return (a.X == b.X && a.Y == b.Y);
        }

        public static bool operator !=(Vector2 a, Vector2 b) {
            return (a.X != b.X || a.Y != b.Y);
        }

        /// <summary>
        /// 取り出します．
        /// </summary>
        /// <param name="vectorId">方向</param>
        /// <returns>引数で与えられた方向の値</returns>
        public double Index(VectorId vectorId) {
            double val = 0;
            
            switch ((int)vectorId) {
                case (int)VectorId.X:
                    val = X;
                    break;
                case (int)VectorId.Y:
                    val = Y;
                    break;                    
            }

            return val;
        }
    }

    /// <summary>2次元ベクトル(x, y)のint型構造体です．</summary>
    public struct Vector2Int {
        /// <summary>X方向の値</summary>
        public int X;

        /// <summary>Y方向の値</summary>
        public int Y;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2Int" /> struct.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public Vector2Int(int x, int y) {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// 特定方向の成分を取り出します．
        /// </summary>
        /// <param name="vectorId">方向</param>
        /// <returns>引数で与えられた方向の値</returns>
        public int Index(VectorId vectorId) {
            int val = 0;
            
            switch ((int)vectorId) {
                case (int)VectorId.X:
                    val = X;
                    break;
                case (int)VectorId.Y:
                    val = Y;
                    break;
            }

            return val;
        }
    }

    /// <summary>
    /// 2次元ベクトル(x, y)と角度(angle)を保持するクラスです．
    /// </summary>
    public struct Vector2Angle {
        public double X;
        public double Y;
        public double Angle;

        public Vector2Angle(double x, double y, double angle) {
            this.X = x;
            this.Y = y;
            this.Angle = angle;
        }
    }

    /// <summary>
    /// 3次元ベクトル(x, y, z)のdouble型構造体です．
    /// </summary>
    public struct Vector3 {
        /// <summary>
        /// X方向の値
        /// </summary>
        public double X;

        /// <summary>
        /// Y方向の値
        /// </summary>
        public double Y;

        /// <summary>
        /// Z方向の値
        /// </summary>
        public double Z;        

        public static Vector3 operator +(Vector3 a, Vector3 b) {
            return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b) {
            return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static bool operator >(Vector3 a, Vector3 b) {
            return (a.X > b.X && a.Y > b.Y && a.Z > b.Z);
        }

        public static bool operator <(Vector3 a, Vector3 b) {
            return (a.X < b.X && a.Y < b.Y && a.Z < b.Z);
        }

        public static bool operator >=(Vector3 a, Vector3 b) {
            return (a.X >= b.X && a.Y >= b.Y);
        }

        public static bool operator <=(Vector3 a, Vector3 b) {
            return (a.X <= b.X && a.Y <= b.Y && a.Z <= b.Z);
        }

        public static bool operator ==(Vector3 a, Vector3 b) {
            return (a.X == b.X && a.Y == b.Y && a.Z == b.Z);
        }

        public static bool operator !=(Vector3 a, Vector3 b) {
            return (a.X != b.X || a.Y != b.Y || a.Z != b.Z);
        }

        public Vector3 ToAbs() {
            return new Vector3(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3" /> struct.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        public Vector3(double x, double y, double z) {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// 特定方向の成分を取り出します．
        /// </summary>
        /// <param name="vectorId">方向</param>
        /// <returns>引数で与えられた方向の値</returns>
        public double Index(VectorId vectorId) {
            double val = 0;
            switch ((int)vectorId) {
                case (int)VectorId.X:
                    val = X;
                    break;
                case (int)VectorId.Y:
                    val = Y;
                    break;
                case (int)VectorId.Z:
                    val = Z;
                    break;
            }
            
            return val;
        }
    }

    /// <summary>
    /// ３次元ベクトル(x, y, z)のint型構造体です．
    /// </summary>
    public struct Vector3Int {
        /// <summary>
        /// X方向の値
        /// </summary>
        public int X;

        /// <summary>
        /// Y方向の値
        /// </summary>
        public int Y;

        /// <summary>
        /// Z方向の値
        /// </summary>
        public int Z;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3Int" /> struct.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        public Vector3Int(int x, int y, int z) {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// 特定方向の成分を取り出します．
        /// </summary>
        /// <param name="vectorId">方向</param>
        /// <returns>引数で与えられた方向の値</returns>
        public int Index(VectorId vectorId) {
            int val = 0;
            switch ((int)vectorId) {
                case (int)VectorId.X:
                    val = X;
                    break;
                case (int)VectorId.Y:
                    val = Y;
                    break;
                case (int)VectorId.Z:
                    val = Z;
                    break;
            }
            
            return val;
        }
    }
}
