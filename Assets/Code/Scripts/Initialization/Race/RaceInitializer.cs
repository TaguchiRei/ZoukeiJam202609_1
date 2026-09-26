using UnityEngine;
using UsefulToolkit.Attributes;
using UsefulToolkit.BlackBoard.BlackBoard;
using UsefulToolkit.BlackBoard.Logger;
using UsefulToolkit.Initialization;
using UsefulToolkit.Utility;
using ZoukeiJam1.Application.Race;
using ZoukeiJam1.BlackBoard.OmikujiEngine;
using ZoukeiJam1.BlackBoard.Race;
using ZoukeiJam1.EngineAdapter.Race;

namespace ZoukeiJam1.Initialization.Race
{
    /// <summary>
    /// BlackBoard から OmikujiEngine の State と RaceBoard を取り出して RaceService を生成し、
    /// RaceTicker から毎フレーム Service を更新させ、開始演出の終了で走行を始めるようにつなぐ。
    /// HUD には Service が登録した State を渡す
    /// </summary>
    [InitializeOrder(InitializeOrderConst.Default)]
    public sealed class RaceInitializer : InitializerBase
    {
        [SerializeField] private GameRuleSettings _settings;
        [SerializeField] private RaceTicker _ticker;
        [SerializeField] private StartCountdown _startCountdown;
        [SerializeField] private RaceHud _hud;

        /// <param name="blackBoard">IOmikujiEngineState と RaceBoard の取得元</param>
        public override void Initialize(IBlackBoard blackBoard)
        {
            if (!blackBoard.TryGetStateBoard<OmikujiEngineBoard>(out var engineBoard) ||
                !engineBoard.TryGetSceneState<IOmikujiEngineState>(out var engineState, out _))
            {
                UsefulLogger.LogError("IOmikujiEngineState を取得できなかった為、Race の初期化を中止しました", this);
                return;
            }

            if (!blackBoard.TryGetStateBoard<RaceBoard>(out var raceBoard))
            {
                UsefulLogger.LogError("RaceBoard を取得できなかった為、Race の初期化を中止しました", this);
                return;
            }

            var service = new RaceService(
                raceBoard, gameObject.scene.buildIndex, engineState,
                _settings.GoalDistance, _settings.TimeLimit, _settings.DistancePerRevolution);

            if (raceBoard.TryGetSceneState<IRaceState>(out var raceState, out _))
            {
                _hud.Initialize(raceState);
            }

            _ticker.Initialize(service.Update);
            _startCountdown.Play(service.StartRunning);

            base.Initialize(blackBoard);
        }
    }
}
