namespace ZoukeiJam1.BlackBoard.Race
{
    /// <summary>ゴールしたときの結果を State に書き込む操作面。BlackBoard には載せず、DI で渡す</summary>
    public interface IGameResultRecorder
    {
        /// <summary>ゴールしたときの結果を記録する。前の結果は上書きする</summary>
        void Record(GoalResult result);
    }
}
