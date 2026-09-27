using UnityEngine;
using ZoukeiJam1.BlackBoard.OmikujiEngine;

namespace ZoukeiJam1.Application.OmikujiEngine
{
    /// <summary>
    /// OmikujiEngine の State を生成して登録し、毎フレームのクランクの動きから
    /// 逆回転中か・ミスで止まっているか・エンジンの回転速度（回転/秒）を求めて State に書き込む。
    /// 正転へ動かずに逆方向へ戻した量がミスの許容量を超えたらミスとし、回転速度を 0 にする。
    /// ミス中は正転へ動き出すまで回転速度を 0 のままにし、動き出したら 0 から測り直す
    /// </summary>
    public sealed class KujibikeService
    {
        private const float TwoPi = Mathf.PI * 2f;

        private readonly OmikujiEngineState _state;

        /// <summary>回転速度がそのフレームの角速度へ近づく速さの時定数（秒）</summary>
        private readonly float _speedSmoothingTime;

        /// <summary>ミスとせずに逆方向へ戻せるクランク角の量（rad）</summary>
        private readonly float _reverseTolerance;

        /// <summary>最後に正転へ動いてから逆方向へ戻したクランク角の合計（rad）</summary>
        private float _reversedAmount;

        /// <param name="board">State の登録先</param>
        /// <param name="sceneId">State を破棄するシーンの build index</param>
        /// <param name="initialStroke">初期状態のピストンの行程</param>
        /// <param name="initialCrankAngle">初期状態のクランク角（rad）</param>
        /// <param name="speedSmoothingTime">回転速度がそのフレームの角速度へ近づく速さの時定数（秒）。0 ならそのフレームの角速度をそのまま使う</param>
        /// <param name="reverseTolerance">ミスとせずに逆方向へ戻せるクランク角の量（rad）。0 なら少しでも逆へ動いたらミスにする</param>
        public KujibikeService(
            OmikujiEngineBoard board, int sceneId, PistonStroke initialStroke, float initialCrankAngle,
            float speedSmoothingTime, float reverseTolerance)
        {
            _speedSmoothingTime = speedSmoothingTime;
            _reverseTolerance = reverseTolerance;
            _state = new OmikujiEngineState(initialStroke, initialCrankAngle);
            board.RegisterSceneState<IOmikujiEngineState>(_state, sceneId);
        }

        /// <summary>
        /// 1 フレーム分のクランクの動きを State に反映する。
        /// 逆回転中かは、正転へ動いたら false、逆へ動いたら true にし、動かなかったときは前の値のまま。
        /// 許容量以内の逆回転は、止まっていたのと同じ扱いで回転速度を減らす
        /// </summary>
        /// <param name="crankAngle">正転方向に積算したクランク角（rad）</param>
        /// <param name="angleChange">このフレームでのクランク角の変化量（rad、正転方向が正）</param>
        /// <param name="revolutionCount">初期状態から正転で上死点を通過した回数</param>
        /// <param name="stroke">現在のピストンの行程</param>
        /// <param name="deltaTime">前のフレームからの経過時間（秒）</param>
        public void UpdateEngine(float crankAngle, float angleChange, int revolutionCount, PistonStroke stroke, float deltaTime)
        {
            bool isReversing = angleChange < 0f || (angleChange == 0f && _state.IsReversing);

            if (angleChange > 0f) _reversedAmount = 0f;
            else _reversedAmount -= angleChange;

            bool isStalled = angleChange <= 0f && (_state.IsStalled || _reversedAmount > _reverseTolerance);
            float rotationSpeed = isStalled
                ? 0f
                : SmoothSpeed(_state.RotationSpeed, Mathf.Max(angleChange, 0f), deltaTime);

            _state.Apply(revolutionCount, stroke, isReversing, isStalled, rotationSpeed, crankAngle);
        }

        /// <summary>現在の回転速度を、このフレームの角速度（回転/秒）へ時定数に従って近づける</summary>
        private float SmoothSpeed(float currentSpeed, float angleChange, float deltaTime)
        {
            if (deltaTime <= 0f) return currentSpeed;

            float frameSpeed = angleChange / TwoPi / deltaTime;
            if (_speedSmoothingTime <= 0f) return frameSpeed;

            float rate = 1f - Mathf.Exp(-deltaTime / _speedSmoothingTime);
            return Mathf.Lerp(currentSpeed, frameSpeed, rate);
        }
    }
}
