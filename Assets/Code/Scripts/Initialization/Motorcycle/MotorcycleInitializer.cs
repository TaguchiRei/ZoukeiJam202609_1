using UnityEngine;
using UsefulToolkit.BlackBoard.BlackBoard;
using UsefulToolkit.BlackBoard.Logger;
using UsefulToolkit.Initialization;
using ZoukeiJam1.BlackBoard.OmikujiEngine;
using ZoukeiJam1.EngineAdapter.Motorcycle;

namespace ZoukeiJam1.Initialization.Motorcycle
{
    /// <summary>BlackBoard から OmikujiEngine の State を取り出し、MotorcycleManager に渡して動作を開始させる</summary>
    public sealed class MotorcycleInitializer : InitializerBase
    {
        [SerializeField] private MotorcycleManager _motorcycleManager;

        /// <param name="blackBoard">IOmikujiEngineState の取得元</param>
        public override void Initialize(IBlackBoard blackBoard)
        {
            if (!blackBoard.TryGetStateBoard<OmikujiEngineBoard>(out var engineBoard) ||
                !engineBoard.TryGetSceneState<IOmikujiEngineState>(out var engineState, out _))
            {
                UsefulLogger.LogError("IOmikujiEngineState を取得できなかった為、Motorcycle の初期化を中止しました", this);
                return;
            }

            _motorcycleManager.Initialize(engineState);
            base.Initialize(blackBoard);
        }
    }
}
