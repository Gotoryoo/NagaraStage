using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

using NagaraStage.Parameter;

namespace NagaraStage {
    namespace Ui {
        /// <summary>
        /// メインウィンドウのインターフェイスです．
        /// <para>メインウィンドウはソフトウェアのユーザーインターフェイスの主となるウィンドウです．
        /// 各パラメータに加え，ワークスペースと命名された作業用のユーザーインターフェイス領域を持ちます．
        /// </para>
        /// </summary>
        /// <author>Hirokazu Yokoyama</author>
        public interface IMainWindow {


            /// <summary>
            /// リボンタブのいずれかが選択されたときのイベント
            /// </summary>
            /// <example>
            /// <code>
            /// // 選択されているリボンタブが変更されると実行されるイベントハンドラです．
            /// // イベントハンドラの引数にどのタブが選択されたのかを示す値が含まれています．
            /// // その値に基づいてイベントを作成します．
            /// // ここではRibbonTabSelectedのイベントハンドラを作成する例を示します．
            /// public void function() {
            ///     // IMainWindowインターフェイスを実装したクラスのインスタンスwindowに
            ///     // RibbonTabSelectedイベントのイベントハンドラとして
            ///     window.RibbonTabSelected += window_RibbonTabSelected;
            /// }
            /// 
            /// /// <summary>
            /// /// windowのRibbonTabSelectedイベントのイベントハンドラです．
            /// /// </summary>
            /// private void window_RibbonTabSelected(object sender, RibbonTabEventArgs e) {
            ///     // RibbonTabEventArgsのSelectedTabNameフィールドは
            ///     // 選択されたタブのNameプロパティの値を持っています．
            ///     switch(e.SelectedTabName) {
            ///         case "HomeTab":
            ///             MessageBox.Show("Home tabが選択されました．");
            ///             break;
            ///         case "coordTab":
            ///             MessageBox.Show("coord tabが選択されました．");
            ///             break;
            ///         default:
            ///             throw new ArgumentException("実態のないタブが指定されました．お手数ですが開発元に連絡をお願いします．");
            ///     }
            /// }
            /// </code>
            /// </example>
            event EventHandler<RibbonTabEventArgs> RibbonTabSelected;

            /// <summary>
            /// ソフトウェア全体を通して用いいるParameterManagerインスタンスを取得します．
            /// </summary>
            ParameterManager ParameterManager {
                get;
            }

            /// <summary>
            /// ソフトウェア全体を通して用いるCoordManagerインスタンスを取得します．
            /// </summary>
            CoordManager CoordManager {
                get;
            }

            /// <summary>
            /// 選択されているリボンタブを取得，または設定します．
            /// </summary>
            string SelectedRibbonTabName {
                get;
                set;
            }

            /// <summary>
            /// ワークスペースに設定されているContentを取得します．
            /// </summary>
            object WorkspaceContent {
                get;
            }

            /// <summary>
            /// リボンタブが有効かどうかを取得,または設定します.
            /// </summary>
            bool IsTabsEnabled {
                get;
                set;
            }

            /// <summary>
            /// Stageを作成し，ワークスペースに表示します．
            /// </summary>
            void CreateStage();

            /// <summary>
            /// メインウィンドウのワークスペースにコントロールを設置します．
            /// </summary>
            /// <param name="control">設置するコントロール</param>
            void SetElementOnWorkspace(Workspace control);

        }
    }
}