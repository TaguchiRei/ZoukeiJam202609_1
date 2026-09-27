using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UsefulToolkit.Initialization;
using ZoukeiJam1.BlackBoard.Race;

namespace ZoukeiJam1.EngineAdapter.Race
{
    /// <summary>走行の State から、残り時間を Text に、目標距離までの進み具合をメーターに表示する（仮の HUD）</summary>
    public sealed class RaceHud : InitializableMonoBehaviour
    {
        [Tooltip("残り時間を表示する Text")]
        [SerializeField] private TMP_Text _timeText;

        [Tooltip("進み具合に合わせて伸ばすメーターのバー（Image Type は Filled）")]
        [SerializeField] private Image _progressFill;

        [Tooltip("メーター上の現在地を示すマーカー。アンカーの X を進み具合に合わせて動かす")]
        [SerializeField] private RectTransform _progressMarker;

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
            float progress = _raceState.GoalDistance > 0f
                ? Mathf.Clamp01(_raceState.Distance / _raceState.GoalDistance)
                : 0f;

            _timeText.text = $"TIME {remainingTime:0.0}";

            _progressFill.fillAmount = progress;
            _progressMarker.anchorMin = new Vector2(progress, _progressMarker.anchorMin.y);
            _progressMarker.anchorMax = new Vector2(progress, _progressMarker.anchorMax.y);
        }
    }
}
