using UnityEngine;
using UsefulToolkit.Attributes;
using ZoukeiJam1.Application.Race;
using ZoukeiJam1.BlackBoard.Race;

namespace ZoukeiJam1.Initialization.Race
{
    /// <summary>走行・おみくじ・告白・スコアのルールの設定値。設定値から GoalResultCalculator を生成する</summary>
    [CreateAssetMenu(fileName = "GameRuleSettings", menuName = "ZoukeiJam1/GameRuleSettings")]
    public sealed class GameRuleSettings : ScriptableObject
    {
        [Header("走行")]
        [Tooltip("目標距離（m）")]
        [SerializeField, Min(0.01f)] private float _goalDistance = 300f;

        [Tooltip("制限時間（秒）。開始演出が終わってから数える")]
        [SerializeField, Min(0.01f)] private float _timeLimit = 30f;

        [Tooltip("エンジンが 1 回転するごとに進む距離（m）。速度はエンジンの回転速度（回転/秒）にこの値を掛けたものになる")]
        [SerializeField, Min(0f)] private float _distancePerRevolution = 5f;

        [Header("おみくじ")]
        [Tooltip("各結果の、最低速度・最高速度での確率（%）と告白の倍率。確率は合計が 100 でなくても、合計に対する割合で引く")]
        [SerializeField] private OmikujiEntry[] _omikujiEntries =
        {
            new(OmikujiFortune.Daikichi, 2f, 40f, 1.5f),
            new(OmikujiFortune.Chukichi, 8f, 30f, 1.2f),
            new(OmikujiFortune.Shokichi, 20f, 15f, 1f),
            new(OmikujiFortune.Kichi, 30f, 10f, 0.9f),
            new(OmikujiFortune.Kyo, 40f, 5f, 0.5f),
        };

        [Tooltip("確率が「最低速度での確率」になるゴール時の速度（km/h）。これより遅くても確率は変わらない")]
        [SerializeField, Min(0f)] private float _minSpeedKmh = 30f;

        [Tooltip("確率が「最高速度での確率」になるゴール時の速度（km/h）。これより速くても確率は変わらない")]
        [SerializeField, Min(0f)] private float _maxSpeedKmh = 120f;

        [Tooltip("最低速度での確率の合計（%）。100 から外れているときは設定を見直す")]
        [SerializeField, ShowOnly] private float _totalProbabilityAtMinSpeed;

        [Tooltip("最高速度での確率の合計（%）。100 から外れているときは設定を見直す")]
        [SerializeField, ShowOnly] private float _totalProbabilityAtMaxSpeed;

        [Header("告白")]
        [Tooltip("告白の基礎成功率（%）。おみくじの結果ごとの倍率を掛け、100% を上限とする")]
        [SerializeField, Range(0f, 100f)] private float _confessionBaseRate = 50f;

        [Tooltip("告白が成功したときに、平均時速へ掛ける倍率")]
        [SerializeField, Min(0f)] private float _confessionSuccessScoreMultiplier = 2f;

        /// <summary>目標距離（m）</summary>
        public float GoalDistance => _goalDistance;

        /// <summary>制限時間（秒）</summary>
        public float TimeLimit => _timeLimit;

        /// <summary>エンジンが 1 回転するごとに進む距離（m）</summary>
        public float DistancePerRevolution => _distancePerRevolution;

        public GoalResultCalculator CreateGoalResultCalculator()
        {
            var omikujiTable = new OmikujiTable(_omikujiEntries, _minSpeedKmh, _maxSpeedKmh);
            return new GoalResultCalculator(omikujiTable, _confessionBaseRate, _confessionSuccessScoreMultiplier);
        }

        private void OnValidate()
        {
            _totalProbabilityAtMinSpeed = 0f;
            _totalProbabilityAtMaxSpeed = 0f;
            if (_omikujiEntries == null) return;

            foreach (var entry in _omikujiEntries)
            {
                _totalProbabilityAtMinSpeed += entry.ProbabilityAtMinSpeed;
                _totalProbabilityAtMaxSpeed += entry.ProbabilityAtMaxSpeed;
            }
        }
    }
}
