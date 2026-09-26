using UnityEngine;
using UsefulToolkit.BlackBoard.BlackBoard;
using UsefulToolkit.BlackBoard.Input;
using UsefulToolkit.BlackBoard.Logger;
using UsefulToolkit.Initialization;
using ZoukeiJam1.BlackBoard.OmikujiEngine;
using ZoukeiJam1.EngineAdapter.OmikujiEngine;

namespace ZoukeiJam1.Initialization.OmikujiEngine
{
    /// <summary>
    /// BlackBoard から入力 State と OmikujiEngineBoard を取り出し、OmikujiEngineManager に渡して動作を開始させる
    /// </summary>
    public sealed class OmikujiEngineInitializer : InitializerBase
    {
        [SerializeField] private OmikujiEngineManager _engineManager;

        /// <param name="blackBoard">IInputState と OmikujiEngineBoard の取得元</param>
        public override void Initialize(IBlackBoard blackBoard)
        {
            if (!blackBoard.TryGetStateBoard<InputBoard>(out var inputBoard) ||
                !inputBoard.TryGetGameState<IInputState>(out var inputState))
            {
                UsefulLogger.LogError("IInputState を取得できなかった為、OmikujiEngine の初期化を中止しました", this);
                return;
            }

            if (!blackBoard.TryGetStateBoard<OmikujiEngineBoard>(out var engineBoard))
            {
                UsefulLogger.LogError("OmikujiEngineBoard を取得できなかった為、OmikujiEngine の初期化を中止しました", this);
                return;
            }

            _engineManager.Initialize(inputState, engineBoard, gameObject.scene.buildIndex);
            base.Initialize(blackBoard);
        }
    }
}
