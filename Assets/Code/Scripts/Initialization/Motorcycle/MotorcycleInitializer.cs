using UnityEngine;
using UsefulToolkit.BlackBoard.BlackBoard;
using UsefulToolkit.Initialization;
using ZoukeiJam1.EngineAdapter.Motorcycle;

namespace ZoukeiJam1.Initialization.Motorcycle
{
    /// <summary>MotorcycleManager の動作を開始させる</summary>
    public sealed class MotorcycleInitializer : InitializerBase
    {
        [SerializeField] private MotorcycleManager _motorcycleManager;

        public override void Initialize(IBlackBoard blackBoard)
        {
            _motorcycleManager.Initialize();
            base.Initialize(blackBoard);
        }
    }
}
