using UnityEngine;
using UsefulToolkit.Attributes;
using UsefulToolkit.BlackBoard.BlackBoard;
using UsefulToolkit.BlackBoard.Logger;
using UsefulToolkit.Initialization;
using UsefulToolkit.Utility;
using ZoukeiJam1.BlackBoard.Race;
using ZoukeiJam1.EngineAdapter.Motorcycle;

namespace ZoukeiJam1.Initialization.Motorcycle
{
    /// <summary>BlackBoard から走行の State を取り出し、MotorcycleManager に渡して動作を開始させる</summary>
    [InitializeOrder(InitializeOrderConst.DefaultLate)]
    public sealed class MotorcycleInitializer : InitializerBase
    {
        [SerializeField] private MotorcycleManager _motorcycleManager;

        /// <param name="blackBoard">IRaceState の取得元</param>
        public override void Initialize(IBlackBoard blackBoard)
        {
            if (!blackBoard.TryGetStateBoard<RaceBoard>(out var raceBoard) ||
                !raceBoard.TryGetSceneState<IRaceState>(out var raceState, out _))
            {
                UsefulLogger.LogError("IRaceState を取得できなかった為、Motorcycle の初期化を中止しました", this);
                return;
            }

            _motorcycleManager.Initialize(raceState);
            base.Initialize(blackBoard);
        }
    }
}
