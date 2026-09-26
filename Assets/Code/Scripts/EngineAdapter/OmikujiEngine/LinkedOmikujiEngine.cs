using UnityEngine;
using UsefulToolkit.Initialization;
using ZoukeiJam1.BlackBoard.OmikujiEngine;

namespace ZoukeiJam1.EngineAdapter.OmikujiEngine
{
    /// <summary>
    /// OmikujiEngine の State のクランク角を毎フレーム読み取り、自身のクランク・ピストン・コンロッドの Transform に反映する。
    /// 入力は受け付けない
    /// </summary>
    public sealed class LinkedOmikujiEngine : InitializableMonoBehaviour
    {
        [SerializeField] private Transform _crank;
        [SerializeField] private Transform _crankPin;
        [SerializeField] private Transform _piston;
        [SerializeField] private Transform _arm;
        [SerializeField] private bool _clockwise = true;

        private IOmikujiEngineState _engineState;
        private OmikujiEngineRig _rig;

        /// <summary>
        /// シーン上の配置から寸法を読み取り、クランク角の反映を開始する。
        /// 寸法は Crank・CrankPin・Piston がエンジンのローカル座標で初期配置にある前提で求める
        /// </summary>
        /// <param name="engineState">クランク角の読み取り元</param>
        public void Initialize(IOmikujiEngineState engineState)
        {
            _engineState = engineState;
            _rig = new OmikujiEngineRig(_crank, _crankPin, _piston, _arm, _clockwise);

            base.Initialize();
        }

        // OmikujiEngineManager の Update で更新されたクランク角を同じフレームで反映するため、LateUpdate で読む
        private void LateUpdate()
        {
            _rig.ApplyPose(_engineState.CrankAngle);
        }
    }
}
