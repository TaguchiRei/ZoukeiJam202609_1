using UnityEngine;
using TMPro;
using UsefulToolkit.Initialization;
using ZoukeiJam1.BlackBoard.Race;

namespace ZoukeiJam1.EngineAdapter.Race
{
    /// <summary>走行の State から、残り時間・進んだ距離を Text に表示する（仮の HUD）</summary>
    public sealed class RaceHud : InitializableMonoBehaviour
    {
        [Tooltip("残り時間を表示する Text")]
        [SerializeField] private TMP_Text _timeText;

        [Tooltip("進んだ距離と目標距離を表示する Text")]
        [SerializeField] private TMP_Text _distanceText;

        private IRaceState _raceState;

        /// <param name="raceState">表示する値の読み取り元</param>
        public void Initialize(IRaceState raceState)
        {
            _raceState = raceState;
            base.Initialize();
        }

        private void LateUpdate()
        {
            float remainingTime = Mathf.Max(0f, _raceState.TimeLimit - _raceState.ElapsedTime);
            float distance = Mathf.Min(_raceState.Distance, _raceState.GoalDistance);

            _timeText.text = $"TIME {remainingTime:0.0}";
            _distanceText.text = $"{distance:0} / {_raceState.GoalDistance:0} m";
        }
    }
}
