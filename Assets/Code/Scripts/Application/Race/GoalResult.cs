using ZoukeiJam1.BlackBoard.Race;

namespace ZoukeiJam1.Application.Race
{
    /// <summary>ゴールしたときの結果（平均時速・おみくじ・告白・最終スコア）</summary>
    public readonly struct GoalResult
    {
        /// <summary>ゴールまでの平均時速（km/h）</summary>
        public float AverageSpeedKmh { get; }

        /// <summary>ゴールした瞬間の速度（km/h）</summary>
        public float GoalSpeedKmh { get; }

        public OmikujiFortune Fortune { get; }

        /// <summary>おみくじの倍率を掛けた後の告白の成功率（%）</summary>
        public float ConfessionSuccessRate { get; }

        public bool IsConfessionSucceeded { get; }

        /// <summary>平均時速に、告白が成功したときの倍率を掛けた値</summary>
        public float FinalScore { get; }

        public GoalResult(
            float averageSpeedKmh, float goalSpeedKmh, OmikujiFortune fortune,
            float confessionSuccessRate, bool isConfessionSucceeded, float finalScore)
        {
            AverageSpeedKmh = averageSpeedKmh;
            GoalSpeedKmh = goalSpeedKmh;
            Fortune = fortune;
            ConfessionSuccessRate = confessionSuccessRate;
            IsConfessionSucceeded = isConfessionSucceeded;
            FinalScore = finalScore;
        }
    }
}
