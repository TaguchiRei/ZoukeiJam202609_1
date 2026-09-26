using UsefulToolkit.BlackBoard.BlackBoard;

namespace ZoukeiJam1.BlackBoard.Race
{
    /// <summary>直前にゴールしたときの結果の State の読み取り面。シーンをまたいで残る</summary>
    public interface IGameResultState : IStateGetter
    {
        /// <summary>直前にゴールしたときの結果。まだ一度もゴールしていなければ既定値</summary>
        GoalResult Result { get; }
    }
}
