using UnityEngine;
using UnityEngine.UI;
using ZoukeiJam1.BlackBoard.Race;

namespace ZoukeiJam1.EngineAdapter.Result
{
    /// <summary>ゴールしたときの結果を Text に表示する（仮のリザルト表示）</summary>
    public sealed class ResultView : MonoBehaviour
    {
        [Tooltip("おみくじの結果を表示する Text")]
        [SerializeField] private Text _fortuneText;

        [Tooltip("告白の成否と成功率を表示する Text")]
        [SerializeField] private Text _confessionText;

        [Tooltip("ゴールタイムと平均時速を表示する Text")]
        [SerializeField] private Text _runText;

        [Tooltip("最終スコアを表示する Text")]
        [SerializeField] private Text _scoreText;

        /// <param name="resultState">表示する結果の読み取り元</param>
        public void Initialize(IGameResultState resultState)
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
