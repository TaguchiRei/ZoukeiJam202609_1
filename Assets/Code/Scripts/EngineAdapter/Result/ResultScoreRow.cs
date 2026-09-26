using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;

namespace ZoukeiJam1.EngineAdapter.Result
{
    /// <summary>リザルトのパネルの 1 行。見出しと数値を持ち、フェードインしてから数値をカウントアップする</summary>
    public sealed class ResultScoreRow : MonoBehaviour
    {
        [Tooltip("行全体の不透明度をまとめて変える CanvasGroup")]
        [SerializeField] private CanvasGroup _canvasGroup;

        [Tooltip("見出しの Text")]
        [SerializeField] private TMP_Text _label;

        [SerializeField] private CountUpText _value;

        /// <summary>行を透明にして隠す</summary>
        public void Hide()
        {
            _canvasGroup.alpha = 0f;
        }

        /// <summary>見出しを設定し、行をフェードインしてから数値を 0 からカウントアップする</summary>
        /// <param name="label">見出し。null なら今の見出しのまま</param>
        /// <param name="value">最後に表示する数値</param>
        /// <param name="fadeSeconds">フェードインにかける時間（秒）</param>
        /// <param name="countUpSeconds">カウントアップにかける時間（秒）</param>
        public async UniTask ShowAsync(
            string label, float value, float fadeSeconds, float countUpSeconds, CancellationToken cancellationToken)
        {
            if (label != null) _label.text = label;
            _value.SetValue(0f);

            await LMotion.Create(0f, 1f, fadeSeconds).BindToAlpha(_canvasGroup).ToUniTask(cancellationToken);
            await _value.PlayAsync(value, countUpSeconds, cancellationToken);
        }
    }
}
