namespace ZoukeiJam1.BlackBoard.Race
{
    /// <summary>走行の進行段階</summary>
    public enum RacePhase
    {
        /// <summary>開始演出が終わるまで。エンジンは回せるが、距離と時間は増えない</summary>
        Starting,

        /// <summary>走行中。距離と時間が増える</summary>
        Running,

        /// <summary>目標距離に到達した</summary>
        Goal,

        /// <summary>制限時間内に目標距離へ届かなかった</summary>
        GameOver,
    }
}
