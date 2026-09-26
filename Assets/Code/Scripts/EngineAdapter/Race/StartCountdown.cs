using System;
using System.Collections;
using UnityEngine;
using TMPro;

namespace ZoukeiJam1.EngineAdapter.Race
{
    /// <summary>
    /// 開始演出の仮実装。数字をカウントダウン表示し、開始の文字を出した時点で終了を通知する。
    /// 開始の文字は一定時間表示してから消す
    /// </summary>
    public sealed class StartCountdown : MonoBehaviour
    {
        [Tooltip("カウントダウンを表示する Text")]
        [SerializeField] private TMP_Text _text;

        [Tooltip("カウントダウンを始める数")]
        [SerializeField, Min(1)] private int _count = 3;

        [Tooltip("1 つの数字を表示する時間（秒）")]
        [SerializeField, Min(0.01f)] private float _secondsPerCount = 1f;

        [Tooltip("カウントダウンの後に表示する文字")]
        [SerializeField] private string _startMessage = "GO!";

        [Tooltip("開始の文字を表示しておく時間（秒）")]
        [SerializeField, Min(0f)] private float _startMessageSeconds = 0.8f;

        /// <summary>カウントダウンを始める</summary>
        /// <param name="onFinished">開始の文字を出した時点で呼ぶ処理</param>
        public void Play(Action onFinished)
        {
            StartCoroutine(PlayRoutine(onFinished));
        }

        private IEnumerator PlayRoutine(Action onFinished)
        {
            _text.gameObject.SetActive(true);
            for (int i = _count; i > 0; i--)
            {
                _text.text = i.ToString();
                yield return new WaitForSeconds(_secondsPerCount);
            }

            _text.text = _startMessage;
            onFinished?.Invoke();

            yield return new WaitForSeconds(_startMessageSeconds);
            _text.gameObject.SetActive(false);
        }
    }
}
