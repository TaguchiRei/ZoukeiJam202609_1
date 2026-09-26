using System;
using UnityEngine;
using UnityEngine.UI;

namespace ZoukeiJam1.EngineAdapter.Title
{
    /// <summary>タイトル画面のスタートボタンを押したら処理を呼ぶ（仮のタイトル）。押した後はボタンを押せなくする</summary>
    public sealed class TitleView : MonoBehaviour
    {
        [SerializeField] private Button _startButton;

        /// <param name="onStart">スタートボタンを押したときの処理</param>
        public void Initialize(Action onStart)
        {
            _startButton.onClick.AddListener(() =>
            {
                _startButton.interactable = false;
                onStart();
            });
        }
    }
}
