using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace ZoukeiJam1.EngineAdapter.Result
{
    /// <summary>告白ムービー。表示してから Timeline を最初から最後まで再生し、終わったら隠す</summary>
    public sealed class ConfessionMovie : MonoBehaviour
    {
        [Tooltip("ムービーを表示するオブジェクト。初期化時に非表示にする")]
        [SerializeField] private GameObject _view;

        [Tooltip("ムービーの Timeline を再生する PlayableDirector。Wrap Mode は None にする")]
        [SerializeField] private PlayableDirector _director;

        /// <summary>ムービーを隠す</summary>
        public void Hide()
        {
            _view.SetActive(false);
        }

        /// <summary>ムービーを表示して最後まで再生し、隠す</summary>
        public async UniTask PlayAsync(CancellationToken cancellationToken)
        {
            _view.SetActive(true);
            _director.time = 0d;
            _director.Play();

            // Wrap Mode が None の Timeline は、最後まで再生すると再生中でなくなる
            await UniTask.WaitWhile(() => _director.state == PlayState.Playing, cancellationToken: cancellationToken);

            _director.Stop();
            _view.SetActive(false);
        }
    }
}
