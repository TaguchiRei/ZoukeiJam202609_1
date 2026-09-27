using UnityEngine;
using UsefulToolkit.Attributes;
using UsefulToolkit.BlackBoard.BlackBoard;
using UsefulToolkit.BlackBoard.Input;
using UsefulToolkit.BlackBoard.Logger;
using UsefulToolkit.Initialization;
using UsefulToolkit.Utility;
using ZoukeiJam1.Application.OmikujiEngine;
using ZoukeiJam1.BlackBoard.OmikujiEngine;
using ZoukeiJam1.EngineAdapter.OmikujiEngine;

namespace ZoukeiJam1.Initialization.OmikujiEngine
{
    /// <summary>
    /// BlackBoard から入力 State と OmikujiEngineBoard を取り出し、OmikujiEngineManager と KujibikeService を生成・初期化して、
    /// Manager の毎フレームのクランクの動きを Service へ渡すようにつなぐ。
    /// LinkedOmikujiEngine・EngineReverseEffect があれば、Service が登録した State を渡して初期化する
    /// </summary>
    [InitializeOrder(InitializeOrderConst.DefaultEarly)]
    public sealed class OmikujiEngineInitializer : InitializerBase
    {
        [SerializeField] private OmikujiEngineManager _engineManager;

        [Tooltip("OmikujiEngineManager と同じ動きをさせるエンジン。空なら何もしない")]
        [SerializeField] private LinkedOmikujiEngine _linkedEngine;

        [Tooltip("ミスしたときに画面振動とポストエフェクトを出す。空なら何もしない")]
        [SerializeField] private EngineReverseEffect _reverseEffect;

        [Tooltip("エンジンの回転速度が、そのフレームの角速度へ近づく速さの時定数（秒）。大きいほどなめらかになり、0 ならそのフレームの角速度をそのまま使う")]
        [SerializeField, Min(0f)] private float _speedSmoothingTime = 0.3f;

        [Tooltip("正転へ動かずに逆方向へ戻しても、ミス（回転速度が 0 になる）としないクランク角の量（rad）。0 なら少しでも逆へ動いたらミス。" +
                 "逆方向へは直前の死点まで（半回転ほど）しか戻れないため、それより大きくするとミスが起きなくなる")]
        [SerializeField, Min(0f)] private float _reverseTolerance = 0.05f;

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

            // Service は Manager が求めた初期の行程とクランク角で State を作るため、Manager の初期化が先
            _engineManager.Initialize(inputState);
            var service = new KujibikeService(
                engineBoard, gameObject.scene.buildIndex,
                _engineManager.Stroke, _engineManager.CrankAngle, _speedSmoothingTime, _reverseTolerance);
            _engineManager.SetCrankUpdatedHandler(service.UpdateEngine);

            if (engineBoard.TryGetSceneState<IOmikujiEngineState>(out var engineState, out _))
            {
                if (_linkedEngine != null) _linkedEngine.Initialize(engineState);
                if (_reverseEffect != null) _reverseEffect.Initialize(engineState);
            }

            base.Initialize(blackBoard);
        }
    }
}
