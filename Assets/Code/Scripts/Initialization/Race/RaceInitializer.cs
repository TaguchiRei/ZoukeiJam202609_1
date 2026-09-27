using UnityEngine;
using UnityEngine.Serialization;
using UsefulToolkit.Attributes;
using UsefulToolkit.BlackBoard.BlackBoard;
using UsefulToolkit.BlackBoard.Logger;
using UsefulToolkit.Initialization;
using UsefulToolkit.Utility;
using ZoukeiJam1.Application.Race;
using ZoukeiJam1.Application.SceneTransition;
using ZoukeiJam1.BlackBoard.OmikujiEngine;
using ZoukeiJam1.BlackBoard.Race;
using ZoukeiJam1.EngineAdapter.Race;
using ZoukeiJam1.Initialization.Result;

namespace ZoukeiJam1.Initialization.Race
{
    /// <summary>
    /// BlackBoard から OmikujiEngine の State と RaceBoard を取り出して RaceService と GoalService を生成し、
    /// RaceTicker から毎フレーム RaceService を更新させ、開始演出の終了で走行を始めるようにつなぐ。
    /// HUD・おみくじの確率メーター・ゲームオーバーの表示・ゴールの演出には RaceService が登録した State を渡し、
    /// ゲームオーバーのボタンにタイトル・インゲームへの遷移を、ゴールの演出の終了にリザルトへの遷移をつなぐ
    /// </summary>
    [InitializeOrder(InitializeOrderConst.Default)]
    public sealed class RaceInitializer : InitializerBase, IInjectable<IGameResultRecorder, IGameSceneTransition>
    {
        [FormerlySerializedAs("_settings")]
        [SerializeField] private RaceSettings _raceSettings;
        [SerializeField] private ResultSettings _resultSettings;
        [SerializeField] private RaceTicker _ticker;
        [SerializeField] private StartCountdown _startCountdown;
        [SerializeField] private RaceHud _hud;
        [SerializeField] private OmikujiProbabilityMeter _probabilityMeter;
        [SerializeField] private GameOverView _gameOverView;
        [SerializeField] private GoalPresenter _goalPresenter;

        private IGameResultRecorder _resultRecorder;
        private IGameSceneTransition _sceneTransition;
        private GoalService _goalService;

        public void Inject(IGameResultRecorder resultRecorder, IGameSceneTransition sceneTransition)
        {
            _resultRecorder = resultRecorder;
            _sceneTransition = sceneTransition;
        }

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
                _raceSettings.GoalDistance, _raceSettings.TimeLimit, _raceSettings.DistancePerRevolution);

            if (raceBoard.TryGetSceneState<IRaceState>(out var raceState, out _))
            {
                _goalService = new GoalService(
                    raceState, _resultSettings.CreateGoalResultCalculator(), _resultRecorder);
                _hud.Initialize(raceState);
                InitializeProbabilityMeter(raceState);
                _gameOverView.Initialize(raceState, _sceneTransition.ToTitle, _sceneTransition.ToRace);
                _goalPresenter.Initialize(raceState, _sceneTransition.ToResult);
            }

            _ticker.Initialize(service.Update);
            _startCountdown.Play(service.StartRunning);

            base.Initialize(blackBoard);
        }

        /// <summary>ResultSettings のおみくじの項目から、確率メーターに項目の結果の並びと、速度から各項目が引かれる割合を求める処理を渡す</summary>
        private void InitializeProbabilityMeter(IRaceState raceState)
        {
            var omikujiTable = _resultSettings.CreateOmikujiTable();
            var fortunes = new OmikujiFortune[omikujiTable.Count];
            for (int i = 0; i < fortunes.Length; i++) fortunes[i] = omikujiTable[i].Fortune;

            _probabilityMeter.Initialize(
                raceState, fortunes, omikujiTable.GetDrawRates,
                _resultSettings.MinSpeedKmh, _resultSettings.MaxSpeedKmh);
        }
    }
}
