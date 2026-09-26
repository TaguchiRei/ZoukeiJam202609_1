using System;
using UnityEngine;
using UsefulToolkit.BlackBoard.BlackBoard;
using ZoukeiJam1.BlackBoard.Race;

namespace ZoukeiJam1.EngineAdapter.Race
{
    /// <summary>走行の進行段階がゲームオーバーになったら、ゲームオーバーの表示を出す（仮の表示）</summary>
    public sealed class GameOverView : MonoBehaviour
    {
        [Tooltip("ゲームオーバーのときに表示するオブジェクト。初期化時に非表示にする")]
        [SerializeField] private GameObject _view;

        private IDisposable _phaseChangedRegistration;

        /// <param name="raceState">ゲームオーバーの検知に使う</param>
        public void Initialize(IRaceState raceState)
        {
            _view.SetActive(false);
            _phaseChangedRegistration = raceState.RegisterEventOnPhaseChanged(
                new ActionEntry<StateContext<RacePhase>>(false, OnPhaseChanged));
        }

        private void OnPhaseChanged(StateContext<RacePhase> context)
        {
            if (context.NewValue == RacePhase.GameOver) _view.SetActive(true);
        }

        private void OnDestroy()
        {
            _phaseChangedRegistration?.Dispose();
        }
    }
}
