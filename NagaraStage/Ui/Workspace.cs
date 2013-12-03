/**
 * @author Hirokazu Yokoyama <o1007410@edu.gifu-u.ac.jp>
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;


namespace NagaraStage.Ui {
    /// <summary>
    /// IMainWindowの「ワークスペース」に設置できるコントロールのクラスです．
    /// <para>
    /// IMainWindowインターフェイスを実装したクラスは「ワークスペース」と言うウィンドウの主となる
    /// 作業スペースを持ちます．StageFieldControlはステージに設置できるクラスです．
    /// </para>
    /// <para>
    /// ステージに設置するコントロールクラスを作成する場合は，このクラスを継承するようにしてください．
    /// </para>
    /// <para>このクラスはデフォルトでUnloadedイベント発生時にAbortRunningActivityがイベントハンドラに設定されています．
    /// <seealso cref="AbortRunningActivity"/>
    /// </para>
    /// </summary>
    public class Workspace : UserControl {
        private static Workspace previousControl;
        private static Workspace presentControl;
        private IMainWindow window;
        
        /// <summary>
        /// 現在ワークスペースに設置されているコントロールを取得します．
        /// </summary>
        public static Workspace PresentControl {
            get { return presentControl; }
        }

        /// <summary>
        /// コントロールが閉じられたとき，次に表示するコントロールを指定します．
        /// </summary>
        public Workspace NextControl = null;

        public Workspace(IMainWindow _window) {
            window = _window;
            Loaded += StageFieldControl_Loaded;
            Unloaded += AbortRunningActivity;
        }

        /// <summary>
        /// 実行中のアクティビティを中止します．このメソッドはデフォルトでUnloadedイベント発生時に実行されます．
        /// <para>
        /// もし何らかの理由でUnloadedイベント発生時にアクティビティを中止したくない場合は，このメソッドを
        /// イベントハンドラから削除してください．
        /// </para>
        /// </summary>
        /// <example>
        /// <code>
        /// // Unloadedイベント発生時にアクティビティを中止したくない場合は以下をコーディングします．
        /// Unloaded -= AbortRunningActivity;
        /// </code>
        /// </example>
        protected void AbortRunningActivity(object sender, EventArgs e) {
            Activities.Activity activity = new Activities.Activity(window.ParameterManager);
            activity.Abort();
        }

        /// <summary>
        /// コントロールをワークスペースから退去させ，代わりにNextControlをワークスペースに設置します．
        /// </summary>
        public void Finish() {
            window.SetElementOnWorkspace(NextControl);
        }

        private void StageFieldControl_Loaded(object sender, RoutedEventArgs e) {
            previousControl = presentControl;
            presentControl = this;
            if(NextControl == null) {
                NextControl = previousControl;
            }
        }     
    }
}
