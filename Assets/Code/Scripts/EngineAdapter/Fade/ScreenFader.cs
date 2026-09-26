using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace ZoukeiJam1.EngineAdapter.Fade
{
    /// <summary>画面全体を覆う Image の不透明度を変えて、暗転・明転させる</summary>
    public sealed class ScreenFader : MonoBehaviour
    {
        [Tooltip("画面全体を覆う Image。色の不透明度だけを変える")]
        [SerializeField] private Image _image;

        /// <summary>不透明度をすぐに変える</summary>
        public void SetAlpha(float alpha)
        {
            var color = _image.color;
            color.a = alpha;
            _image.color = color;
        }

        /// <summary>現在の不透明度から指定した不透明度まで、指定した時間で変える</summary>
        /// <param name="alpha">変化後の不透明度（0 で透明、1 で完全に覆う）</param>
        /// <param name="duration">変化にかける時間（秒）</param>
        public UniTask FadeAsync(float alpha, float duration, CancellationToken cancellationToken)
        {
            return LMotion.Create(_image.color.a, alpha, duration)
                .BindToColorA(_image)
                .ToUniTask(cancellationToken);
        }
    }
}
