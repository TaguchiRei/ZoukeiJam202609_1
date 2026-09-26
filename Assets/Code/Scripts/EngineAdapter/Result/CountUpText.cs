using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using TMPro;
using UnityEngine;

namespace ZoukeiJam1.EngineAdapter.Result
{
    /// <summary>数値を書式に当てはめて Text に表示し、0 から指定した値までカウントアップさせる</summary>
    public sealed class CountUpText : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        [Tooltip("表示の書式。{0} に数値が入る（例：{0:0}、+{0:0}、{0:0.0} km/h）")]
        [SerializeField] private string _format = "{0:0}";

        /// <summary>値をすぐに表示する</summary>
        public void SetValue(float value)
        {
            _text.text = string.Format(_format, value);
        }

        /// <summary>0 から指定した値まで、指定した時間でカウントアップする</summary>
        /// <param name="value">最後に表示する値</param>
        /// <param name="duration">カウントアップにかける時間（秒）</param>
        public UniTask PlayAsync(float value, float duration, CancellationToken cancellationToken)
        {
            SetValue(0f);
            return LMotion.Create(0f, value, duration)
                .WithEase(Ease.OutCubic)
                .Bind(SetValue)
                .ToUniTask(cancellationToken);
        }
    }
}
