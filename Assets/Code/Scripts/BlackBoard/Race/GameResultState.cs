using UsefulToolkit.BlackBoard.BlackBoard;

namespace ZoukeiJam1.BlackBoard.Race
{
    /// <summary>
    /// 直前にゴールしたときの結果を保持する。ゲーム終了まで残る。
    /// BlackBoard へは <see cref="IGameResultState"/> としてのみ登録し、値の変更は具象型を保持する生成元だけが行う
    /// </summary>
    [RegisterBoard(typeof(RaceBoard))]
    public sealed class GameResultState : GameStateBase, IGameResultState
    {
        public GoalResult Result { get; private set; }

        /// <summary>結果を上書きする。変化の通知は行わない</summary>
        public void Apply(GoalResult result)
        {
            Result = result;
        }

        public override string GetLog()
        {
            return $"GoalTime : {Result.GoalTime} / AverageSpeedKmh : {Result.AverageSpeedKmh} / " +
                   $"GoalSpeedKmh : {Result.GoalSpeedKmh} / BaseScore : {Result.BaseScore} / " +
                   $"Fortune : {Result.Fortune} / OmikujiBonus : {Result.OmikujiBonus} / " +
                   $"ConfessionSuccessRate : {Result.ConfessionSuccessRate} / ConfessionItem : {Result.ConfessionItem} / " +
                   $"IsConfessionSucceeded : {Result.IsConfessionSucceeded} / ConfessionBonus : {Result.ConfessionBonus} / " +
                   $"FinalScore : {Result.FinalScore}";
        }
    }
}
