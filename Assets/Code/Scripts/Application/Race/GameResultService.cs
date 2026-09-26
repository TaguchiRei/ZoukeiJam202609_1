using ZoukeiJam1.BlackBoard.Race;

namespace ZoukeiJam1.Application.Race
{
    /// <summary>ゴールしたときの結果の State を生成して登録し、記録された結果を書き込む</summary>
    public sealed class GameResultService : IGameResultRecorder
    {
        private readonly GameResultState _state = new();

        /// <summary>State をゲーム終了まで残る State として登録する</summary>
        /// <param name="board">State の登録先</param>
        public void RegisterState(RaceBoard board)
        {
            board.RegisterGameState<IGameResultState>(_state);
        }

        public void Record(GoalResult result)
        {
            _state.Apply(result);
        }
    }
}
