using UnityEngine;
using ZoukeiJam1.BlackBoard.Race;

namespace ZoukeiJam1.Application.Race
{
    /// <summary>
    /// ゴールしたときの結果を求める。
    /// 平均時速から基礎スコアを求め、ゴール時の速度でおみくじを引き、ゴール時の残り時間とおみくじから告白の成功率と告白アイテムを決めて成否を抽選し、
    /// おみくじと告白の成功による加算スコアを求める
    /// </summary>
    public sealed class GoalResultCalculator
    {
        private readonly OmikujiTable _omikujiTable;
        private readonly float _baseScoreMultiplier;
        private readonly float _confessionRatePerRemainingSecond;
        private readonly float _confessionSuccessScoreMultiplier;
        private readonly float _bouquetThreshold;
        private readonly float _ringThreshold;

        /// <param name="omikujiTable">ゴール時に引くおみくじ</param>
        /// <param name="baseScoreMultiplier">平均時速（km/h）に掛けて基礎スコアにする倍率</param>
        /// <param name="confessionRatePerRemainingSecond">ゴール時の残り時間 1 秒あたりの告白の基礎成功率（%）</param>
        /// <param name="confessionSuccessScoreMultiplier">告白が成功したときに、基礎スコアに掛けて加算する倍率</param>
        /// <param name="bouquetThreshold">告白アイテムが花束になる告白の成功率（%）。これ未満は手紙</param>
        /// <param name="ringThreshold">告白アイテムが指輪になる告白の成功率（%）</param>
        public GoalResultCalculator(
            OmikujiTable omikujiTable, float baseScoreMultiplier, float confessionRatePerRemainingSecond,
            float confessionSuccessScoreMultiplier, float bouquetThreshold, float ringThreshold)
        {
            _omikujiTable = omikujiTable;
            _baseScoreMultiplier = baseScoreMultiplier;
            _confessionRatePerRemainingSecond = confessionRatePerRemainingSecond;
            _confessionSuccessScoreMultiplier = confessionSuccessScoreMultiplier;
            _bouquetThreshold = bouquetThreshold;
            _ringThreshold = ringThreshold;
        }

        /// <param name="goalDistance">目標距離（ユニット）</param>
        /// <param name="elapsedTime">走行開始からゴールまでの時間（秒）</param>
        /// <param name="timeLimit">制限時間（秒）</param>
        /// <param name="goalSpeed">ゴールした瞬間の速度（ユニット/秒）</param>
        public GoalResult Calculate(float goalDistance, float elapsedTime, float timeLimit, float goalSpeed)
        {
            float averageSpeedKmh = elapsedTime > 0f ? SpeedUnit.ToKmh(goalDistance / elapsedTime) : 0f;
            float goalSpeedKmh = SpeedUnit.ToKmh(goalSpeed);
            float baseScore = averageSpeedKmh * _baseScoreMultiplier;

            var entry = _omikujiTable.Draw(goalSpeedKmh);
            float omikujiBonus = baseScore * entry.ScoreMultiplier;

            float confessionBaseRate = Mathf.Max(0f, timeLimit - elapsedTime) * _confessionRatePerRemainingSecond;
            float successRate = Mathf.Clamp(confessionBaseRate * entry.ConfessionRateMultiplier, 0f, 100f);
            bool isSucceeded = successRate >= 100f || Random.value * 100f < successRate;
            float confessionBonus = isSucceeded ? baseScore * _confessionSuccessScoreMultiplier : 0f;

            return new GoalResult(
                goalTime: elapsedTime,
                averageSpeedKmh: averageSpeedKmh,
                goalSpeedKmh: goalSpeedKmh,
                baseScore: baseScore,
                fortune: entry.Fortune,
                omikujiScoreMultiplier: entry.ScoreMultiplier,
                omikujiBonus: omikujiBonus,
                confessionBaseRate: confessionBaseRate,
                confessionRateMultiplier: entry.ConfessionRateMultiplier,
                confessionSuccessRate: successRate,
                confessionItem: ToConfessionItem(successRate),
                isConfessionSucceeded: isSucceeded,
                confessionScoreMultiplier: _confessionSuccessScoreMultiplier,
                confessionBonus: confessionBonus);
        }

        private ConfessionItem ToConfessionItem(float successRate)
        {
            if (successRate >= _ringThreshold) return ConfessionItem.Ring;
            if (successRate >= _bouquetThreshold) return ConfessionItem.Bouquet;
            return ConfessionItem.Letter;
        }
    }
}
