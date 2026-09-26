namespace ZoukeiJam1.BlackBoard.Race
{
    /// <summary>
    /// ゴールしたときの結果。走行の記録、おみくじ、告白と、それぞれによるスコアの加算を持つ。
    /// 最終スコアは 基礎スコア + おみくじの加算 + 告白の加算
    /// </summary>
    public readonly struct GoalResult
    {
        /// <summary>走行開始からゴールまでの時間（秒）</summary>
        public float GoalTime { get; }

        /// <summary>ゴールまでの平均時速（km/h）</summary>
        public float AverageSpeedKmh { get; }

        /// <summary>ゴールした瞬間の速度（km/h）</summary>
        public float GoalSpeedKmh { get; }

        /// <summary>平均時速に基礎倍率を掛けたスコア</summary>
        public float BaseScore { get; }

        public OmikujiFortune Fortune { get; }

        /// <summary>おみくじの結果による、基礎スコアに掛けて加算する倍率</summary>
        public float OmikujiScoreMultiplier { get; }

        /// <summary>おみくじの結果による加算スコア（基礎スコア × OmikujiScoreMultiplier）</summary>
        public float OmikujiBonus { get; }

        /// <summary>ゴール時の残り時間から求めた告白の基礎成功率（%）</summary>
        public float ConfessionBaseRate { get; }

        /// <summary>おみくじの結果による、告白の基礎成功率に掛ける倍率</summary>
        public float ConfessionRateMultiplier { get; }

        /// <summary>基礎成功率に倍率を掛けた告白の成功率（%）。100 を上限とする</summary>
        public float ConfessionSuccessRate { get; }

        /// <summary>告白の成功率から決まる告白アイテム</summary>
        public ConfessionItem ConfessionItem { get; }

        public bool IsConfessionSucceeded { get; }

        /// <summary>告白の成功による、基礎スコアに掛けて加算する倍率</summary>
        public float ConfessionScoreMultiplier { get; }

        /// <summary>告白の成功による加算スコア。失敗なら 0</summary>
        public float ConfessionBonus { get; }

        /// <summary>基礎スコア + おみくじの加算 + 告白の加算</summary>
        public float FinalScore { get; }

        public GoalResult(
            float goalTime, float averageSpeedKmh, float goalSpeedKmh, float baseScore,
            OmikujiFortune fortune, float omikujiScoreMultiplier, float omikujiBonus,
            float confessionBaseRate, float confessionRateMultiplier, float confessionSuccessRate,
            ConfessionItem confessionItem, bool isConfessionSucceeded, float confessionScoreMultiplier, float confessionBonus)
        {
            GoalTime = goalTime;
            AverageSpeedKmh = averageSpeedKmh;
            GoalSpeedKmh = goalSpeedKmh;
            BaseScore = baseScore;
            Fortune = fortune;
            OmikujiScoreMultiplier = omikujiScoreMultiplier;
            OmikujiBonus = omikujiBonus;
            ConfessionBaseRate = confessionBaseRate;
            ConfessionRateMultiplier = confessionRateMultiplier;
            ConfessionSuccessRate = confessionSuccessRate;
            ConfessionItem = confessionItem;
            IsConfessionSucceeded = isConfessionSucceeded;
            ConfessionScoreMultiplier = confessionScoreMultiplier;
            ConfessionBonus = confessionBonus;
            FinalScore = baseScore + omikujiBonus + confessionBonus;
        }
    }
}
