using UnityEngine;
using UsefulToolkit.Attributes;
using ZoukeiJam1.Application.Race;
using ZoukeiJam1.BlackBoard.Race;

namespace ZoukeiJam1.Initialization.Result
{
    /// <summary>リザルト（スコア・おみくじ・告白）のルールの設定値。設定値から GoalResultCalculator を生成する</summary>
    [CreateAssetMenu(fileName = "ResultSettings", menuName = "ZoukeiJam1/ResultSettings")]
    public sealed class ResultSettings : ScriptableObject
    {
        [Header("スコア")]
        [Tooltip("平均時速（km/h）に掛けて基礎スコアにする倍率")]
        [SerializeField, Min(0f)] private float _baseScoreMultiplier = 1f;

        [Header("おみくじ")]
        [Tooltip("各結果の、最低速度・最高速度での確率（%）、告白成功率の倍率、スコアの倍率。確率は合計が 100 でなくても、合計に対する割合で引く")]
        [SerializeField] private OmikujiEntry[] _omikujiEntries =
        {
            new(OmikujiFortune.Daikichi, 2f, 40f, 1.5f, 1f),
            new(OmikujiFortune.Chukichi, 8f, 30f, 1.2f, 0.6f),
            new(OmikujiFortune.Shokichi, 20f, 15f, 1f, 0.3f),
            new(OmikujiFortune.Kichi, 30f, 10f, 0.9f, 0.2f),
            new(OmikujiFortune.Kyo, 40f, 5f, 0.5f, 0f),
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
        [Tooltip("ゴール時の残り時間 1 秒あたりの告白の基礎成功率（%）。基礎成功率におみくじの倍率を掛け、100% を上限とする")]
        [SerializeField, Min(0f)] private float _confessionRatePerRemainingSecond = 1f;

        [Tooltip("告白が成功したときに、基礎スコアに掛けて加算する倍率")]
        [SerializeField, Min(0f)] private float _confessionSuccessScoreMultiplier = 1f;

        [Tooltip("告白アイテムが花束になる告白の成功率（%）。これ未満は手紙")]
        [SerializeField, Range(0f, 100f)] private float _bouquetThreshold = 40f;

        [Tooltip("告白アイテムが指輪になる告白の成功率（%）")]
        [SerializeField, Range(0f, 100f)] private float _ringThreshold = 70f;

        /// <summary>確率が「最低速度での確率」になるゴール時の速度（km/h）</summary>
        public float MinSpeedKmh => _minSpeedKmh;

        /// <summary>確率が「最高速度での確率」になるゴール時の速度（km/h）</summary>
        public float MaxSpeedKmh => _maxSpeedKmh;

        public OmikujiTable CreateOmikujiTable()
        {
            return new OmikujiTable(_omikujiEntries, _minSpeedKmh, _maxSpeedKmh);
        }

        public GoalResultCalculator CreateGoalResultCalculator()
        {
            return new GoalResultCalculator(
                CreateOmikujiTable(), _baseScoreMultiplier, _confessionRatePerRemainingSecond,
                _confessionSuccessScoreMultiplier, _bouquetThreshold, _ringThreshold);
        }

        private void OnValidate()
        {
            _ringThreshold = Mathf.Max(_ringThreshold, _bouquetThreshold);

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
