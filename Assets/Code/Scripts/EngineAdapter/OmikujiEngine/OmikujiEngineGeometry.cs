using UnityEngine;

namespace ZoukeiJam1.EngineAdapter.OmikujiEngine
{
    /// <summary>
    /// クランク・コンロッド・ピストンの寸法を持ち、クランク角とピストンの高さを相互に変換する。
    /// 座標はすべてエンジンのローカル座標。クランク角は正転方向に増加し、0 でクランクピンがクランク中心の真上に来る
    /// </summary>
    public sealed class OmikujiEngineGeometry
    {
        private readonly Vector2 _crankCenter;
        private readonly float _crankRadius;
        private readonly float _rodLength;
        private readonly float _pistonAxisX;

        /// <summary>時計回りが正転なら 1、反時計回りが正転なら -1</summary>
        private readonly float _directionSign;

        /// <summary>上死点でのピストンの高さ</summary>
        public float TopDeadCenterHeight { get; }

        /// <summary>下死点でのピストンの高さ</summary>
        public float BottomDeadCenterHeight { get; }

        /// <summary>上死点のクランク角（rad）</summary>
        public float TopDeadCenterAngle { get; }

        /// <summary>下死点のクランク角（rad）</summary>
        public float BottomDeadCenterAngle { get; }

        /// <param name="crankCenter">クランクの回転中心</param>
        /// <param name="crankPin">クランクピンの位置</param>
        /// <param name="pistonPin">ピストンピンの位置。x はピストンが動く軸になる</param>
        /// <param name="clockwise">時計回りを正転とするか</param>
        public OmikujiEngineGeometry(Vector2 crankCenter, Vector2 crankPin, Vector2 pistonPin, bool clockwise)
        {
            _crankCenter = crankCenter;
            _crankRadius = Vector2.Distance(crankCenter, crankPin);
            _rodLength = Vector2.Distance(crankPin, pistonPin);
            _pistonAxisX = pistonPin.x;
            _directionSign = clockwise ? 1f : -1f;

            float axisOffset = _pistonAxisX - _crankCenter.x;
            float topReach = _rodLength + _crankRadius;
            float bottomReach = _rodLength - _crankRadius;
            TopDeadCenterHeight = _crankCenter.y + Mathf.Sqrt(topReach * topReach - axisOffset * axisOffset);
            BottomDeadCenterHeight = _crankCenter.y + Mathf.Sqrt(bottomReach * bottomReach - axisOffset * axisOffset);
            TopDeadCenterAngle = SolveAngles(TopDeadCenterHeight).first;
            BottomDeadCenterAngle = SolveAngles(BottomDeadCenterHeight).first;
        }

        /// <summary>クランクピンの位置からクランク角を求める</summary>
        public float AngleOf(Vector2 crankPin)
        {
            Vector2 offset = crankPin - _crankCenter;
            return Mathf.Atan2(_directionSign * offset.x, offset.y);
        }

        /// <summary>クランク角に対応するクランクピンの位置</summary>
        public Vector2 CrankPinAt(float angle)
        {
            return _crankCenter + _crankRadius * new Vector2(_directionSign * Mathf.Sin(angle), Mathf.Cos(angle));
        }

        /// <summary>クランク角に対応するピストンピンの位置</summary>
        public Vector2 PistonPinAt(float angle)
        {
            Vector2 crankPin = CrankPinAt(angle);
            float dx = _pistonAxisX - crankPin.x;
            float height = crankPin.y + Mathf.Sqrt(Mathf.Max(0f, _rodLength * _rodLength - dx * dx));
            return new Vector2(_pistonAxisX, height);
        }

        /// <summary>ピストンの高さを下死点〜上死点の範囲に収める</summary>
        public float ClampHeight(float height)
        {
            return Mathf.Clamp(height, BottomDeadCenterHeight, TopDeadCenterHeight);
        }

        /// <summary>
        /// ピストンがその高さになるクランク角を 2 つ返す。死点では 2 つが同じ角度（2π の差を除く）になる
        /// </summary>
        public (float first, float second) SolveAngles(float height)
        {
            // |ピストンピン - クランクピン| = コンロッド長 を a·sinθ + b·cosθ = k の形に整理して解く
            float a = _directionSign * (_pistonAxisX - _crankCenter.x);
            float b = height - _crankCenter.y;
            float k = (a * a + b * b + _crankRadius * _crankRadius - _rodLength * _rodLength) / (2f * _crankRadius);
            float r = Mathf.Sqrt(a * a + b * b);
            float phi = Mathf.Atan2(a, b);
            float alpha = Mathf.Acos(Mathf.Clamp(k / r, -1f, 1f));
            return (phi - alpha, phi + alpha);
        }
    }
}
