using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UsefulToolkit.BlackBoard.Logger;
using UsefulToolkit.Initialization;
using ZoukeiJam1.BlackBoard.Race;

namespace ZoukeiJam1.EngineAdapter.Race
{
    /// <summary>
    /// 速度を OmikujiMeter シェーダーのアーチと針で表示し、その速度でおみくじの各結果が引かれる割合を、アーチの帯の幅と Text で表示する。
    /// 帯はアーチの左端から、項目の並びの逆順に並べる。表示する速度は、実際の速度に遅れて追従させた値を使う
    /// </summary>
    public sealed class OmikujiProbabilityMeter : InitializableMonoBehaviour
    {
        /// <summary>シェーダーが描ける帯の数</summary>
        private const int MaxSegmentCount = 5;

        private static readonly int SpeedTId = Shader.PropertyToID("_SpeedT");
        private static readonly int SegmentBoundsId = Shader.PropertyToID("_SegmentBounds");
        private static readonly int ProbabilityRangeId = Shader.PropertyToID("_ProbabilityRange");

        private static readonly int[] SegmentColorIds =
        {
            Shader.PropertyToID("_SegmentColor0"),
            Shader.PropertyToID("_SegmentColor1"),
            Shader.PropertyToID("_SegmentColor2"),
            Shader.PropertyToID("_SegmentColor3"),
            Shader.PropertyToID("_SegmentColor4"),
        };

        /// <summary>おみくじの結果と、その帯と文字の色</summary>
        [Serializable]
        private struct FortuneColor
        {
            public OmikujiFortune Fortune;
            public Color Color;
        }

        [Tooltip("メーターを描く Image。OmikujiMeter シェーダーのマテリアルを設定し、RectTransform は正方形にする")]
        [SerializeField] private Image _meterImage;

        [Tooltip("速度を表示する Text")]
        [SerializeField] private TMP_Text _speedText;

        [Tooltip("各結果が引かれる割合を表示する Text")]
        [SerializeField] private TMP_Text _probabilityText;

        [Tooltip("アーチの右端に当たる速度（km/h）")]
        [SerializeField, Min(1f)] private float _maxScaleSpeedKmh = 200f;

        [Tooltip("表示する速度が実際の速度に追いつくまでのおおよその時間（秒）。0 なら遅らせない")]
        [SerializeField, Min(0f)] private float _smoothTime = 0.1f;

        [Tooltip("速度の表示の書式。{0} に速度（km/h、整数）が入る")]
        [SerializeField] private string _speedFormat = "{0}<size=45%> km/h</size>";

        [Tooltip("1 つの結果の割合の表示の書式。{0} に結果の名前、{1} に割合（%、整数）が入る。名前と割合の間は改行されないよう、ノーブレークスペースにする")]
        [SerializeField] private string _probabilityFormat = "{0} <b>{1}</b>%";

        [Tooltip("割合の表示で、結果と結果の間に入れる文字")]
        [SerializeField] private string _probabilitySeparator = "   ";

        [Tooltip("各結果の、帯と文字の色")]
        [SerializeField] private FortuneColor[] _fortuneColors =
        {
            new() { Fortune = OmikujiFortune.Daikichi, Color = new Color(1f, 0.78f, 0.2f) },
            new() { Fortune = OmikujiFortune.Chukichi, Color = new Color(1f, 0.45f, 0.25f) },
            new() { Fortune = OmikujiFortune.Shokichi, Color = new Color(0.95f, 0.4f, 0.65f) },
            new() { Fortune = OmikujiFortune.Kichi, Color = new Color(0.35f, 0.7f, 1f) },
            new() { Fortune = OmikujiFortune.Kyo, Color = new Color(0.55f, 0.45f, 0.75f) },
        };

        private readonly StringBuilder _probabilityBuilder = new();

        private IRaceState _raceState;
        private Action<float, float[]> _writeDrawRates;
        private Material _material;
        private float[] _rates;
        private int[] _shownPercents;
        private string[] _probabilityLabels;
        private int _segmentCount;
        private float _displaySpeedKmh;
        private float _speedVelocity;
        private int _shownSpeedKmh = -1;

        /// <param name="raceState">速度の読み取り元</param>
        /// <param name="fortunes">おみくじの各項目の結果。項目の順</param>
        /// <param name="writeDrawRates">速度（km/h）から、各項目が引かれる割合（0〜1、合計 1）を項目の順に配列へ書き込む処理</param>
        /// <param name="probabilityMinSpeedKmh">確率が「最低速度での確率」になる速度（km/h）</param>
        /// <param name="probabilityMaxSpeedKmh">確率が「最高速度での確率」になる速度（km/h）</param>
        public void Initialize(
            IRaceState raceState, IReadOnlyList<OmikujiFortune> fortunes, Action<float, float[]> writeDrawRates,
            float probabilityMinSpeedKmh, float probabilityMaxSpeedKmh)
        {
            _raceState = raceState;
            _writeDrawRates = writeDrawRates;

            if (fortunes.Count > MaxSegmentCount)
                UsefulLogger.LogError($"おみくじの項目が {MaxSegmentCount} を超えている為、先頭の {MaxSegmentCount} 項目だけを帯に描きます", this);

            _segmentCount = Mathf.Min(fortunes.Count, MaxSegmentCount);
            _rates = new float[fortunes.Count];
            _shownPercents = new int[fortunes.Count];
            _probabilityLabels = new string[fortunes.Count];
            Array.Fill(_shownPercents, -1);

            _material = new Material(_meterImage.material);
            _meterImage.material = _material;

            var material = _meterImage.materialForRendering;
            for (int d = 0; d < MaxSegmentCount; d++)
            {
                int index = ToEntryIndex(Mathf.Min(d, _segmentCount - 1));
                material.SetColor(SegmentColorIds[d], GetColor(fortunes[index]));
            }

            material.SetVector(ProbabilityRangeId, new Vector4(
                probabilityMinSpeedKmh / _maxScaleSpeedKmh, probabilityMaxSpeedKmh / _maxScaleSpeedKmh, 0f, 0f));

            for (int i = 0; i < fortunes.Count; i++)
            {
                string colorCode = ColorUtility.ToHtmlStringRGB(GetColor(fortunes[i]));
                _probabilityLabels[i] = $"<color=#{colorCode}>{fortunes[i].ToDisplayName()}</color>";
            }

            _displaySpeedKmh = SpeedUnit.ToKmh(_raceState.Speed);
            base.Initialize();
        }

        private void LateUpdate()
        {
            float speedKmh = SpeedUnit.ToKmh(_raceState.Speed);
            _displaySpeedKmh = _smoothTime > 0f
                ? Mathf.SmoothDamp(_displaySpeedKmh, speedKmh, ref _speedVelocity, _smoothTime)
                : speedKmh;

            _writeDrawRates(_displaySpeedKmh, _rates);

            UpdateMaterial();
            UpdateSpeedText();
            UpdateProbabilityText();
        }

        private void OnDestroy()
        {
            if (_material != null) Destroy(_material);
        }

        private void UpdateMaterial()
        {
            var material = _meterImage.materialForRendering;
            material.SetFloat(SpeedTId, Mathf.Clamp01(_displaySpeedKmh / _maxScaleSpeedKmh));

            var bounds = Vector4.one;
            float cumulative = 0f;
            for (int d = 0; d < _segmentCount - 1; d++)
            {
                cumulative += _rates[ToEntryIndex(d)];
                bounds[d] = cumulative;
            }

            material.SetVector(SegmentBoundsId, bounds);
        }

        private void UpdateSpeedText()
        {
            int speedKmh = Mathf.RoundToInt(_displaySpeedKmh);
            if (speedKmh == _shownSpeedKmh) return;

            _shownSpeedKmh = speedKmh;
            _speedText.text = string.Format(_speedFormat, speedKmh);
        }

        /// <summary>各項目の割合（%）を整数に丸め、どれかが前回の表示から変わったときだけ Text を書き直す</summary>
        private void UpdateProbabilityText()
        {
            bool changed = false;
            for (int i = 0; i < _rates.Length; i++)
            {
                int percent = Mathf.RoundToInt(_rates[i] * 100f);
                if (percent == _shownPercents[i]) continue;

                _shownPercents[i] = percent;
                changed = true;
            }

            if (!changed) return;

            _probabilityBuilder.Clear();
            for (int i = 0; i < _rates.Length; i++)
            {
                if (i > 0) _probabilityBuilder.Append(_probabilitySeparator);
                _probabilityBuilder.AppendFormat(_probabilityFormat, _probabilityLabels[i], _shownPercents[i]);
            }

            _probabilityText.SetText(_probabilityBuilder);
        }

        /// <summary>帯の並び（アーチの左端から数えた番号）を、項目の番号に直す</summary>
        private int ToEntryIndex(int segmentIndex)
        {
            return _segmentCount - 1 - segmentIndex;
        }

        private Color GetColor(OmikujiFortune fortune)
        {
            foreach (var fortuneColor in _fortuneColors)
            {
                if (fortuneColor.Fortune == fortune) return fortuneColor.Color;
            }

            return Color.white;
        }
    }
}
