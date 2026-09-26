using UnityEngine;
using ZoukeiJam1.BlackBoard.Race;

namespace ZoukeiJam1.Application.Race
{
    /// <summary>
    /// ゴールしたときに、平均時速を求め、ゴール時の速度でおみくじを引き、告白の成否を抽選して、最終スコアを求める
    /// </summary>
    public sealed class GoalResultCalculator
    {
        private readonly OmikujiTable _omikujiTable;
        private readonly float _confessionBaseRate;
        private readonly float _confessionSuccessScoreMultiplier;

        /// <param name="omikujiTable">ゴール時に引くおみくじ</param>
        /// <param name="confessionBaseRate">告白の基礎成功率（%）</param>
        /// <param name="confessionSuccessScoreMultiplier">告白が成功したときに平均時速へ掛ける倍率</param>
        public GoalResultCalculator(
            OmikujiTable omikujiTable, float confessionBaseRate, float confessionSuccessScoreMultiplier)
        {
            _omikujiTable = omikujiTable;
            _confessionBaseRate = confessionBaseRate;
            _confessionSuccessScoreMultiplier = confessionSuccessScoreMultiplier;
        }

        /// <param name="goalDistance">目標距離（ユニット）</param>
        /// <param name="elapsedTime">走行開始からゴールまでの時間（秒）</param>
        /// <param name="goalSpeed">ゴールした瞬間の速度（ユニット/秒）</param>
        public GoalResult Calculate(float goalDistance, float elapsedTime, float goalSpeed)
        {
            float averageSpeedKmh = elapsedTime > 0f ? SpeedUnit.ToKmh(goalDistance / elapsedTime) : 0f;
            float goalSpeedKmh = SpeedUnit.ToKmh(goalSpeed);

            var entry = _omikujiTable.Draw(goalSpeedKmh);
            float successRate = Mathf.Clamp(_confessionBaseRate * entry.ConfessionRateMultiplier, 0f, 100f);
            bool isSucceeded = successRate >= 100f || Random.value * 100f < successRate;

            float finalScore = isSucceeded ? averageSpeedKmh * _confessionSuccessScoreMultiplier : averageSpeedKmh;
            return new GoalResult(averageSpeedKmh, goalSpeedKmh, entry.Fortune, successRate, isSucceeded, finalScore);
        }
    }
}
