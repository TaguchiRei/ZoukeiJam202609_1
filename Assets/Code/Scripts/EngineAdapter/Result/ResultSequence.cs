using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZoukeiJam1.BlackBoard.Race;
using ZoukeiJam1.EngineAdapter.Fade;

namespace ZoukeiJam1.EngineAdapter.Result
{
    /// <summary>
    /// ゴールしたときの結果を、リザルト画面の演出として順番に見せる。
    /// 明転 → 平均時速・基礎スコア → おみくじが降りてきて倍率を出す → おみくじの加算 → 最終スコア → ボタン、の順に進める。
    /// どちらかのボタンを押したら、両方のボタンを押せなくする
    /// </summary>
    public sealed class ResultSequence : MonoBehaviour
    {
        [Header("明転")]
        [SerializeField] private ScreenFader _fader;

        [Tooltip("明転にかける時間（秒）")]
        [SerializeField, Min(0f)] private float _fadeInSeconds = 0.6f;

        [Header("スコアのパネル")]
        [SerializeField] private ResultScoreRow _averageSpeedRow;
        [SerializeField] private ResultScoreRow _baseScoreRow;
        [SerializeField] private ResultScoreRow _omikujiBonusRow;
        [SerializeField] private ResultScoreRow _finalScoreRow;

        [Tooltip("おみくじの加算の行の見出しの書式。{0} にスコアの倍率が入る")]
        [SerializeField] private string _omikujiBonusLabelFormat = "おみくじ ×{0:0.##}";

        [Tooltip("行のフェードインにかける時間（秒）")]
        [SerializeField, Min(0f)] private float _rowFadeSeconds = 0.25f;

        [Tooltip("数値のカウントアップにかける時間（秒）")]
        [SerializeField, Min(0f)] private float _countUpSeconds = 1f;

        [Header("おみくじ")]
        [SerializeField] private OmikujiPaperView _omikujiPaper;

        [Tooltip("おみくじを降ろし始める高さ（元の位置からの距離）")]
        [SerializeField] private float _omikujiDropHeight = 800f;

        [Tooltip("おみくじを降ろすのにかける時間（秒）")]
        [SerializeField, Min(0f)] private float _omikujiDropSeconds = 0.8f;

        [Tooltip("告白成功率の倍率の文字を出すのにかける時間（秒）")]
        [SerializeField, Min(0f)] private float _multiplierPopSeconds = 0.35f;

        [Header("間")]
        [Tooltip("各段階の間に置く待ち時間（秒）")]
        [SerializeField, Min(0f)] private float _stepIntervalSeconds = 0.4f;

        [Header("ボタン")]
        [Tooltip("最後に表示するボタンをまとめたオブジェクト。初期化時に非表示にする")]
        [SerializeField] private GameObject _buttons;

        [SerializeField] private Button _backToTitleButton;
        [SerializeField] private Button _retryButton;

        [Tooltip("もう一度走るボタンの文字を表示する Text")]
        [SerializeField] private TMP_Text _retryButtonLabel;

        [Tooltip("告白に成功したときの、もう一度走るボタンの文字")]
        [SerializeField] private string _retryLabelOnSuccess = "浮気でもするつもりか？";

        [Tooltip("告白に失敗したときの、もう一度走るボタンの文字")]
        [SerializeField] private string _retryLabelOnFailure = "まだあきらめない";

        /// <summary>結果の演出を始める</summary>
        /// <param name="result">見せる結果</param>
        /// <param name="onBackToTitle">「タイトルに戻る」を押したときの処理</param>
        /// <param name="onRetry">もう一度走るボタンを押したときの処理</param>
        public void Play(GoalResult result, Action onBackToTitle, Action onRetry)
        {
            _fader.SetAlpha(1f);
            _averageSpeedRow.Hide();
            _baseScoreRow.Hide();
            _omikujiBonusRow.Hide();
            _finalScoreRow.Hide();
            _omikujiPaper.Prepare(ToDisplayName(result.Fortune), result.ConfessionRateMultiplier, _omikujiDropHeight);
            _buttons.SetActive(false);

            _retryButtonLabel.text = result.IsConfessionSucceeded ? _retryLabelOnSuccess : _retryLabelOnFailure;
            _backToTitleButton.onClick.AddListener(() => OnButtonClicked(onBackToTitle));
            _retryButton.onClick.AddListener(() => OnButtonClicked(onRetry));

            PlayAsync(result, destroyCancellationToken).Forget();
        }

        private async UniTaskVoid PlayAsync(GoalResult result, CancellationToken token)
        {
            await _fader.FadeAsync(0f, _fadeInSeconds, token);

            await _averageSpeedRow.ShowAsync(null, result.AverageSpeedKmh, _rowFadeSeconds, _countUpSeconds, token);
            await _baseScoreRow.ShowAsync(null, result.BaseScore, _rowFadeSeconds, _countUpSeconds, token);
            await WaitIntervalAsync(token);

            await _omikujiPaper.DropAsync(_omikujiDropSeconds, token);
            await _omikujiPaper.ShowMultiplierAsync(_multiplierPopSeconds, token);
            await WaitIntervalAsync(token);

            string omikujiLabel = string.Format(_omikujiBonusLabelFormat, result.OmikujiScoreMultiplier);
            await _omikujiBonusRow.ShowAsync(omikujiLabel, result.OmikujiBonus, _rowFadeSeconds, _countUpSeconds, token);
            await WaitIntervalAsync(token);

            await _finalScoreRow.ShowAsync(null, result.FinalScore, _rowFadeSeconds, _countUpSeconds, token);
            await WaitIntervalAsync(token);

            _buttons.SetActive(true);
        }

        private UniTask WaitIntervalAsync(CancellationToken token)
        {
            return UniTask.Delay(TimeSpan.FromSeconds(_stepIntervalSeconds), cancellationToken: token);
        }

        private void OnButtonClicked(Action onClicked)
        {
            _backToTitleButton.interactable = false;
            _retryButton.interactable = false;
            onClicked();
        }

        private static string ToDisplayName(OmikujiFortune fortune)
        {
            return fortune switch
            {
                OmikujiFortune.Daikichi => "大吉",
                OmikujiFortune.Chukichi => "中吉",
                OmikujiFortune.Shokichi => "小吉",
                OmikujiFortune.Kichi => "吉",
                OmikujiFortune.Kyo => "凶",
                _ => fortune.ToString(),
            };
        }
    }
}
