using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZoukeiJam1.Application.Race
{
    /// <summary>
    /// おみくじの項目一覧。ゴール時の速度で各項目の確率を最低速度・最高速度の値の間で Lerp し、その比率で 1 つを引く。
    /// 速度が最低速度〜最高速度の範囲外なら、近い方の端の確率を使う
    /// </summary>
    public sealed class OmikujiTable
    {
        private readonly OmikujiEntry[] _entries;
        private readonly float _minSpeed;
        private readonly float _maxSpeed;

        /// <param name="entries">おみくじの項目。1 つ以上必要</param>
        /// <param name="minSpeed">確率が「最低速度での確率」になる速度（km/h）</param>
        /// <param name="maxSpeed">確率が「最高速度での確率」になる速度（km/h）</param>
        public OmikujiTable(IReadOnlyList<OmikujiEntry> entries, float minSpeed, float maxSpeed)
        {
            if (entries == null || entries.Count == 0)
                throw new ArgumentException("おみくじの項目が 1 つもありません", nameof(entries));

            _entries = new OmikujiEntry[entries.Count];
            for (int i = 0; i < entries.Count; i++) _entries[i] = entries[i];
            _minSpeed = minSpeed;
            _maxSpeed = maxSpeed;
        }

        /// <summary>項目の数</summary>
        public int Count => _entries.Length;

        /// <summary>index 番目の項目</summary>
        public OmikujiEntry this[int index] => _entries[index];

        /// <summary>指定した速度での項目の確率（%）。全項目の合計が 100 になるとは限らない</summary>
        /// <param name="speed">ゴール時の速度（km/h）</param>
        public float GetProbability(in OmikujiEntry entry, float speed)
        {
            float t = Mathf.InverseLerp(_minSpeed, _maxSpeed, speed);
            return Mathf.Max(0f, Mathf.Lerp(entry.ProbabilityAtMinSpeed, entry.ProbabilityAtMaxSpeed, t));
        }

        /// <summary>
        /// 指定した速度で <see cref="Draw"/> が各項目を引く割合（0〜1、合計 1）を、項目の順に rates へ書き込む。
        /// 全項目の確率が 0 のときは、すべて同じ割合にする
        /// </summary>
        /// <param name="speed">速度（km/h）</param>
        /// <param name="rates">書き込み先。長さは <see cref="Count"/> 以上必要</param>
        public void GetDrawRates(float speed, float[] rates)
        {
            float total = 0f;
            for (int i = 0; i < _entries.Length; i++)
            {
                rates[i] = GetProbability(_entries[i], speed);
                total += rates[i];
            }

            for (int i = 0; i < _entries.Length; i++)
                rates[i] = total > 0f ? rates[i] / total : 1f / _entries.Length;
        }

        /// <summary>
        /// 指定した速度での確率の比率で項目を 1 つ引く。
        /// 全項目の確率が 0 のときは、すべて同じ確率で引く
        /// </summary>
        /// <param name="speed">ゴール時の速度（km/h）</param>
        public OmikujiEntry Draw(float speed)
        {
            float total = 0f;
            foreach (var entry in _entries) total += GetProbability(entry, speed);

            if (total <= 0f) return _entries[UnityEngine.Random.Range(0, _entries.Length)];

            float remaining = UnityEngine.Random.value * total;
            int lastDrawableIndex = 0;
            for (int i = 0; i < _entries.Length; i++)
            {
                float probability = GetProbability(_entries[i], speed);
                if (probability <= 0f) continue;

                lastDrawableIndex = i;
                remaining -= probability;
                if (remaining < 0f) return _entries[i];
            }

            // Random.value が 1 のときや浮動小数の誤差で残ったときは、確率が 0 でない最後の項目にする
            return _entries[lastDrawableIndex];
        }
    }
}
