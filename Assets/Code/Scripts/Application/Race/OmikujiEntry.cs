using System;
using UnityEngine;
using ZoukeiJam1.BlackBoard.Race;

namespace ZoukeiJam1.Application.Race
{
    /// <summary>
    /// おみくじの 1 項目。最低速度・最高速度での確率と、告白の成功率に掛ける倍率、基礎スコアに掛けて加算する倍率を持つ
    /// </summary>
    [Serializable]
    public struct OmikujiEntry
    {
        [SerializeField] private OmikujiFortune _fortune;

        [Tooltip("ゴール時の速度が最低速度以下のときの確率（%）")]
        [SerializeField, Min(0f)] private float _probabilityAtMinSpeed;

        [Tooltip("ゴール時の速度が最高速度以上のときの確率（%）")]
        [SerializeField, Min(0f)] private float _probabilityAtMaxSpeed;

        [Tooltip("この結果が出たときに、告白の基礎成功率に掛ける倍率（おみくじに「告白成功率 N倍！」と書く値）")]
        [SerializeField, Min(0f)] private float _confessionRateMultiplier;

        [Tooltip("この結果が出たときに、基礎スコアに掛けて加算する倍率（リザルトの「×N」）")]
        [SerializeField, Min(0f)] private float _scoreMultiplier;

        public OmikujiFortune Fortune => _fortune;

        /// <summary>ゴール時の速度が最低速度以下のときの確率（%）</summary>
        public float ProbabilityAtMinSpeed => _probabilityAtMinSpeed;

        /// <summary>ゴール時の速度が最高速度以上のときの確率（%）</summary>
        public float ProbabilityAtMaxSpeed => _probabilityAtMaxSpeed;

        /// <summary>この結果が出たときに、告白の基礎成功率に掛ける倍率</summary>
        public float ConfessionRateMultiplier => _confessionRateMultiplier;

        /// <summary>この結果が出たときに、基礎スコアに掛けて加算する倍率</summary>
        public float ScoreMultiplier => _scoreMultiplier;

        public OmikujiEntry(
            OmikujiFortune fortune, float probabilityAtMinSpeed, float probabilityAtMaxSpeed,
            float confessionRateMultiplier, float scoreMultiplier)
        {
            _fortune = fortune;
            _probabilityAtMinSpeed = probabilityAtMinSpeed;
            _probabilityAtMaxSpeed = probabilityAtMaxSpeed;
            _confessionRateMultiplier = confessionRateMultiplier;
            _scoreMultiplier = scoreMultiplier;
        }
    }
}
