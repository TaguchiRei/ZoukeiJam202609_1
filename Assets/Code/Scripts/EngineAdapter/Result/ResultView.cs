using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZoukeiJam1.BlackBoard.Race;

namespace ZoukeiJam1.EngineAdapter.Result
{
    /// <summary>
    /// ゴールしたときの結果を Text に表示し、「タイトルに戻る」ともう一度走るボタンを出す（仮のリザルト表示）。
    /// もう一度走るボタンの文字は告白の成否で変える。どちらかのボタンを押したら、両方のボタンを押せなくする
    /// </summary>
    public sealed class ResultView : MonoBehaviour
    {
        [Tooltip("おみくじの結果を表示する Text")]
        [SerializeField] private TMP_Text _fortuneText;

        [Tooltip("告白の成否と成功率を表示する Text")]
        [SerializeField] private TMP_Text _confessionText;

        [Tooltip("ゴールタイムと平均時速を表示する Text")]
        [SerializeField] private TMP_Text _runText;

        [Tooltip("スコアを表示する Text")]
        [SerializeField] private TMP_Text _scoreText;

        [SerializeField] private Button _backToTitleButton;
        [SerializeField] private Button _retryButton;

        [Tooltip("もう一度走るボタンの文字を表示する Text")]
        [SerializeField] private TMP_Text _retryButtonLabel;

        [Tooltip("告白に成功したときの、もう一度走るボタンの文字")]
        [SerializeField] private string _retryLabelOnSuccess = "浮気でもするつもりか？";

        [Tooltip("告白に失敗したときの、もう一度走るボタンの文字")]
        [SerializeField] private string _retryLabelOnFailure = "まだあきらめない";

        /// <param name="resultState">表示する結果の読み取り元</param>
        /// <param name="onBackToTitle">「タイトルに戻る」を押したときの処理</param>
        /// <param name="onRetry">もう一度走るボタンを押したときの処理</param>
        public void Initialize(IGameResultState resultState, Action onBackToTitle, Action onRetry)
        {
            var result = resultState.Result;

            _fortuneText.text =
                $"おみくじ：{ToDisplayName(result.Fortune)}（ゴール時 {result.GoalSpeedKmh:0} km/h）告白成功率 {result.ConfessionRateMultiplier:0.##}倍！";
            _confessionText.text =
                $"告白：{(result.IsConfessionSucceeded ? "成功" : "失敗")}（成功率 {result.ConfessionSuccessRate:0.#}% / {result.ConfessionItem}）";
            _runText.text = $"タイム {result.GoalTime:0.00} 秒 / 平均 {result.AverageSpeedKmh:0.0} km/h";
            _scoreText.text =
                $"基礎 {result.BaseScore:0} + おみくじ ×{result.OmikujiScoreMultiplier:0.##} {result.OmikujiBonus:0} + " +
                $"告白 {result.ConfessionBonus:0} = {result.FinalScore:0}";

            _retryButtonLabel.text = result.IsConfessionSucceeded ? _retryLabelOnSuccess : _retryLabelOnFailure;
            _backToTitleButton.onClick.AddListener(() => OnButtonClicked(onBackToTitle));
            _retryButton.onClick.AddListener(() => OnButtonClicked(onRetry));
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
