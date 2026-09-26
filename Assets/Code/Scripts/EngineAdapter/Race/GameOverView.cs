using System;
using UnityEngine;
using UnityEngine.UI;
using UsefulToolkit.BlackBoard.BlackBoard;
using ZoukeiJam1.BlackBoard.Race;

namespace ZoukeiJam1.EngineAdapter.Race
{
    /// <summary>
    /// 走行の進行段階がゲームオーバーになったら、ゲームオーバーの表示と「タイトルに戻る」「もう一度走る」のボタンを出す（仮の表示）。
    /// どちらかのボタンを押したら、両方のボタンを押せなくする
    /// </summary>
    public sealed class GameOverView : MonoBehaviour
    {
        [Tooltip("ゲームオーバーのときに表示するオブジェクト。初期化時に非表示にする")]
        [SerializeField] private GameObject _view;

        [SerializeField] private Button _backToTitleButton;
        [SerializeField] private Button _retryButton;

        private IDisposable _phaseChangedRegistration;

        /// <param name="raceState">ゲームオーバーの検知に使う</param>
        /// <param name="onBackToTitle">「タイトルに戻る」を押したときの処理</param>
        /// <param name="onRetry">「もう一度走る」を押したときの処理</param>
        public void Initialize(IRaceState raceState, Action onBackToTitle, Action onRetry)
        {
            _view.SetActive(false);
            _backToTitleButton.onClick.AddListener(() => OnButtonClicked(onBackToTitle));
            _retryButton.onClick.AddListener(() => OnButtonClicked(onRetry));
            _phaseChangedRegistration = raceState.RegisterEventOnPhaseChanged(
                new ActionEntry<StateContext<RacePhase>>(false, OnPhaseChanged));
        }

        private void OnPhaseChanged(StateContext<RacePhase> context)
        {
            if (context.NewValue == RacePhase.GameOver) _view.SetActive(true);
        }

        private void OnButtonClicked(Action onClicked)
        {
            _backToTitleButton.interactable = false;
            _retryButton.interactable = false;
            onClicked();
        }

        private void OnDestroy()
        {
            _phaseChangedRegistration?.Dispose();
        }
    }
}
