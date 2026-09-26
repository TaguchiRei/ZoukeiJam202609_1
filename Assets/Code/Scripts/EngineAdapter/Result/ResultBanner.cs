using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;

namespace ZoukeiJam1.EngineAdapter.Result
{
    /// <summary>画面の中央に文字を拡大しながら出し、一定時間表示してからフェードアウトさせる</summary>
    public sealed class ResultBanner : MonoBehaviour
    {
        [Tooltip("帯全体の不透明度をまとめて変える CanvasGroup")]
        [SerializeField] private CanvasGroup _canvasGroup;

        [SerializeField] private TMP_Text _text;

        /// <summary>帯を透明にして隠す</summary>
        public void Hide()
        {
            _canvasGroup.alpha = 0f;
        }

        /// <summary>文字を拡大しながら出し、一定時間表示してからフェードアウトする</summary>
        /// <param name="text">表示する文字</param>
        /// <param name="color">文字の色</param>
        /// <param name="popSeconds">拡大にかける時間（秒）</param>
        /// <param name="holdSeconds">表示しておく時間（秒）</param>
        /// <param name="fadeSeconds">フェードアウトにかける時間（秒）</param>
        public async UniTask ShowAsync(
            string text, Color color, float popSeconds, float holdSeconds, float fadeSeconds,
            CancellationToken cancellationToken)
        {
            _text.text = text;
            _text.color = color;
            _canvasGroup.alpha = 1f;

            await LMotion.Create(Vector3.zero, Vector3.one, popSeconds)
                .WithEase(Ease.OutBack)
                .BindToLocalScale(_text.transform)
                .ToUniTask(cancellationToken);
            await UniTask.Delay(TimeSpan.FromSeconds(holdSeconds), cancellationToken: cancellationToken);
            await LMotion.Create(1f, 0f, fadeSeconds).BindToAlpha(_canvasGroup).ToUniTask(cancellationToken);
        }
    }
}
