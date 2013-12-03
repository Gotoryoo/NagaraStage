using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using NagaraStage.Parameter;

namespace NagaraStage.Ui {
    /// <summary>
    /// Trackの情報からtrackの方角などを表示するクラスです．
    /// </summary>
    public class TrackInfoViewer : PictureBox {

        private ParameterManager parameterManager;
        private PictureBox infoViewer = new PictureBox();
        private Track track;

        /// <summary>
        /// ダブルバッファリング用のバッファ
        /// </summary>
        private Bitmap buffer;
        private Bitmap infoBuffer;

        /// <summary>
        /// 円を描画するためのペン
        /// </summary>
        private Pen circlePen = Pens.Aqua;

        /// <summary>
        /// 線を描画するためのペン
        /// </summary>
        private Pen linePen = Pens.Aqua;

        /// <summary>
        /// 中心座標を取得します．
        /// </summary>
        private Point centralPoint {
            get { return new Point(Width / 2, Height / 2); }
        }

        public Pen CirclePen {
            get { return circlePen; }
            set { circlePen = value; }
        }

        public Pen ForegroundPen {
            get { return linePen; }
            set { linePen = value; }
        }

        public Track Track {
            get { return track; }
            set {
                track = value;
                drawPaint();
                drawTrackId();
            }
        }

        /// <summary>Gets or sets the parameter manager.</summary>
        /// <value>The parameter manager.</value>
        public ParameterManager ParameterManager {
            set { parameterManager = value; }
            get { return parameterManager; }
        }

        /// <summary>コントロールの幅を取得または設定します。</summary>
        /// <returns>コントロールの幅 (ピクセル単位)。</returns>
        ///   <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
        ///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   </PermissionSet>
        public new int Width {
            get { return base.Width; }
            set {
                base.Width = value;
                buffer = new Bitmap(value, Height);
                infoBuffer = new Bitmap(value, Height);
                infoViewer.Width = value;
                Refresh();
                infoViewer.Refresh();
            }
        }

        /// <summary>コントロールの高さを取得または設定します。</summary>
        /// <returns>コントロールの高さ (ピクセル単位)。</returns>
        ///   <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
        ///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   </PermissionSet>
        public new int Height {
            get { return base.Height; }
            set {
                base.Height = value;
                buffer = new Bitmap(Width, value);
                infoBuffer = new Bitmap(Width, value);
                infoViewer.Height = value;
                Refresh();
                infoViewer.Refresh();
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TrackInfoViewer(ParameterManager _parameterManager)
            : this() {
            this.parameterManager = _parameterManager;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackInfoViewer" /> class.
        /// </summary>
        public TrackInfoViewer() {
            Width = 150;
            Height = 150;
            BackColor = Color.White;
            buffer = new Bitmap(Width, Height);
            infoViewer = new PictureBox();
            infoViewer.Size = ClientSize;
            infoViewer.Location = new Point(0, 0);
            infoViewer.BackColor = Color.Transparent;
            infoBuffer = new Bitmap(Width, Height);
            Controls.Add(infoViewer);
            Paint += paint;
            //infoViewer.Paint += infoPaint;
        }

        /// <summary>
        /// トラックを描画します．
        /// </summary>
        private void drawPaint() {
            if (parameterManager == null) {
                throw new Exception("parameterManager is null");
            }
            double x = track.MsDX * 0.07;
            double y = track.MsDY * 0.07;
            Graphics g = Graphics.FromImage(buffer);
            g.Clear(BackColor);
            g.DrawEllipse(CirclePen, centralPoint.X - 5, centralPoint.Y - 5, 10, 10);
            int endX = (int)(centralPoint.X + x / parameterManager.CameraMainResolution);
            int endY = (int)(centralPoint.Y + y / parameterManager.CameraSubResolution);
            g.DrawLine(ForegroundPen, centralPoint.X, centralPoint.Y, endX, endY);
            Refresh();
        }

        private void drawTrackId() {
            Graphics g = Graphics.FromImage(infoBuffer);
            g.Clear(Color.Transparent);
            Font font = new Font("MS UI Gothic", 11);
            g.DrawString(track.IdString, font, Brushes.Black, 10, Height - 10);
            infoViewer.Refresh();
        }

        /// <summary>
        /// ペイントイベントのイベントハンドラです．
        /// </summary>               
        public void paint(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;
            g.DrawImage(buffer, 0, 0);
        }


        private void infoPaint(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;
            g.DrawImage(infoBuffer, 0, 0);
        }
    }
}
