using UnityEngine;
using UsefulToolkit.BlackBoard.BlackBoard;
using UsefulToolkit.BlackBoard.Logger;
using UsefulToolkit.Initialization;
using ZoukeiJam1.BlackBoard.Race;
using ZoukeiJam1.EngineAdapter.Result;

namespace ZoukeiJam1.Initialization.Result
{
    /// <summary>BlackBoard からゴールしたときの結果の State を取り出し、ResultView に渡して表示させる</summary>
    public sealed class ResultInitializer : InitializerBase
    {
        [SerializeField] private ResultView _resultView;

        /// <param name="blackBoard">IGameResultState の取得元</param>
        public override void Initialize(IBlackBoard blackBoard)
        {
            if (!blackBoard.TryGetStateBoard<RaceBoard>(out var raceBoard) ||
                !raceBoard.TryGetGameState<IGameResultState>(out var resultState))
            {
                UsefulLogger.LogError("IGameResultState を取得できなかった為、Result の初期化を中止しました", this);
                return;
            }

            _resultView.Initialize(resultState);
            base.Initialize(blackBoard);
        }
    }
}
