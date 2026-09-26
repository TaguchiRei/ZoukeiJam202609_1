using System;
using UsefulToolkit.BlackBoard.BlackBoard;

namespace ZoukeiJam1.BlackBoard.Race
{
    /// <summary>走行の State の読み取り面</summary>
    public interface IRaceState : IStateGetter
    {
        /// <summary>進行段階</summary>
        RacePhase Phase { get; }

        /// <summary>目標距離（ユニット）</summary>
        float GoalDistance { get; }

        /// <summary>制限時間（秒）</summary>
        float TimeLimit { get; }

        /// <summary>走行開始から進んだ距離（ユニット）。変化の通知は行わない</summary>
        float Distance { get; }

        /// <summary>走行開始からの経過時間（秒）。変化の通知は行わない</summary>
        float ElapsedTime { get; }

        /// <summary>現在の速度（ユニット/秒）。開始演出の間も変わる。変化の通知は行わない</summary>
        float Speed { get; }

        /// <summary>Phase の変化時に実行する Action を登録する</summary>
        /// <returns>Dispose すると登録を解除できる</returns>
        IDisposable RegisterEventOnPhaseChanged(ActionEntry<StateContext<RacePhase>> changedAction);
    }
}
