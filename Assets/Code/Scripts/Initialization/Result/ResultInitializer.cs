using UnityEngine;
using UsefulToolkit.BlackBoard.BlackBoard;
using UsefulToolkit.BlackBoard.Logger;
using UsefulToolkit.Initialization;
using ZoukeiJam1.Application.SceneTransition;
using ZoukeiJam1.BlackBoard.Race;
using ZoukeiJam1.EngineAdapter.Result;

namespace ZoukeiJam1.Initialization.Result
{
    /// <summary>
    /// BlackBoard からゴールしたときの結果の State を取り出して ResultView に渡し、
    /// ボタンにタイトル・インゲームへの遷移をつなぐ
    /// </summary>
    public sealed class ResultInitializer : InitializerBase, IInjectable<IGameSceneTransition>
    {
        [SerializeField] private ResultView _resultView;

        private IGameSceneTransition _sceneTransition;

        public void Inject(IGameSceneTransition sceneTransition)
        {
            _sceneTransition = sceneTransition;
        }

        /// <param name="blackBoard">IGameResultState の取得元</param>
        public override void Initialize(IBlackBoard blackBoard)
        {
            if (!blackBoard.TryGetStateBoard<RaceBoard>(out var raceBoard) ||
                !raceBoard.TryGetGameState<IGameResultState>(out var resultState))
            {
                UsefulLogger.LogError("IGameResultState を取得できなかった為、Result の初期化を中止しました", this);
                return;
            }

            _resultView.Initialize(resultState, _sceneTransition.ToTitle, _sceneTransition.ToRace);
            base.Initialize(blackBoard);
        }
    }
}
