using UnityEngine;

namespace ZoukeiJam1.EngineAdapter.OmikujiEngine
{
    /// <summary>
    /// クランク・ピストン・コンロッドの Transform を持ち、初期配置から寸法を求めて、クランク角を各部品の姿勢に反映する。
    /// 座標はすべてエンジンのローカル座標
    /// </summary>
    public sealed class OmikujiEngineRig
    {
        private readonly Transform _crank;
        private readonly Transform _piston;
        private readonly Transform _arm;
        private readonly bool _clockwise;
        private readonly Vector2 _initialRodDirection;

        /// <summary>初期配置から求めた寸法</summary>
        public OmikujiEngineGeometry Geometry { get; }

        /// <summary>初期配置のクランク角（rad）</summary>
        public float InitialAngle { get; }

        /// <summary>
        /// 寸法は Crank・CrankPin・Piston がエンジンのローカル座標で初期配置にある前提で求める
        /// </summary>
        /// <param name="crank">クランクの回転中心</param>
        /// <param name="crankPin">クランクの子に置いたクランクピン</param>
        /// <param name="piston">ピストンピンの位置にあるピストン</param>
        /// <param name="arm">ピストンピンを中心に回転するコンロッド</param>
        /// <param name="clockwise">時計回りを正転とするか</param>
        public OmikujiEngineRig(Transform crank, Transform crankPin, Transform piston, Transform arm, bool clockwise)
        {
            _crank = crank;
            _piston = piston;
            _arm = arm;
            _clockwise = clockwise;

            Vector2 crankCenter = crank.localPosition;
            Vector2 crankPinPosition = crankCenter + (Vector2)(crank.localRotation * crankPin.localPosition);
            Vector2 pistonPin = piston.localPosition;

            Geometry = new OmikujiEngineGeometry(crankCenter, crankPinPosition, pistonPin, clockwise);
            InitialAngle = Geometry.AngleOf(crankPinPosition);
            _initialRodDirection = crankPinPosition - pistonPin;
        }

        /// <summary>クランク角をクランク・ピストン・コンロッドの Transform に反映する</summary>
        /// <param name="angle">正転方向に積算したクランク角（rad）</param>
        public void ApplyPose(float angle)
        {
            Vector2 crankPin = Geometry.CrankPinAt(angle);
            Vector2 pistonPin = Geometry.PistonPinAt(angle);

            float crankDegrees = -(_clockwise ? 1f : -1f) * (angle - InitialAngle) * Mathf.Rad2Deg;
            _crank.localRotation = Quaternion.Euler(0f, 0f, crankDegrees);
            _piston.localPosition = new Vector3(pistonPin.x, pistonPin.y, _piston.localPosition.z);
            _arm.localRotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(_initialRodDirection, crankPin - pistonPin));
        }
    }
}
