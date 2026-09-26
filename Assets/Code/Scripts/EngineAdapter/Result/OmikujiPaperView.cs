using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;

namespace ZoukeiJam1.EngineAdapter.Result
{
    /// <summary>
    /// リザルトのおみくじの紙。結果と告白成功率の倍率を書き、画面の上から元の位置へ降ろす。
    /// 倍率の文字は、降りた後に拡大しながら出す
    /// </summary>
    public sealed class OmikujiPaperView : MonoBehaviour
    {
        [Tooltip("おみくじの紙全体")]
        [SerializeField] private RectTransform _paper;

        [Tooltip("結果（大吉など）を表示する Text")]
        [SerializeField] private TMP_Text _fortuneText;

        [Tooltip("告白成功率の倍率を表示する Text")]
        [SerializeField] private TMP_Text _multiplierText;

        [Tooltip("倍率の表示の書式。{0} に倍率が入る")]
        [SerializeField] private string _multiplierFormat = "告白成功率\n{0:0.##}倍！";

        private Vector2 _restPosition;

        private void Awake()
        {
            _restPosition = _paper.anchoredPosition;
        }

        /// <summary>結果と倍率を書き、紙を画面の上に隠して、倍率の文字を消しておく</summary>
        /// <param name="fortune">表示する結果の名前</param>
        /// <param name="confessionRateMultiplier">告白成功率の倍率</param>
        /// <param name="dropHeight">紙を降ろし始める高さ（元の位置からの距離）</param>
        public void Prepare(string fortune, float confessionRateMultiplier, float dropHeight)
        {
            _fortuneText.text = fortune;
            _multiplierText.text = string.Format(_multiplierFormat, confessionRateMultiplier);
            _multiplierText.transform.localScale = Vector3.zero;
            _paper.anchoredPosition = _restPosition + Vector2.up * dropHeight;
        }

        /// <summary>紙を元の位置へ、弾ませながら降ろす</summary>
        /// <param name="duration">降ろすのにかける時間（秒）</param>
        public UniTask DropAsync(float duration, CancellationToken cancellationToken)
        {
            return LMotion.Create(_paper.anchoredPosition, _restPosition, duration)
                .WithEase(Ease.OutBounce)
                .BindToAnchoredPosition(_paper)
                .ToUniTask(cancellationToken);
        }

        /// <summary>倍率の文字を拡大しながら出す</summary>
        /// <param name="duration">出すのにかける時間（秒）</param>
        public UniTask ShowMultiplierAsync(float duration, CancellationToken cancellationToken)
        {
            return LMotion.Create(Vector3.zero, Vector3.one, duration)
                .WithEase(Ease.OutBack)
                .BindToLocalScale(_multiplierText.transform)
                .ToUniTask(cancellationToken);
        }
    }
}
