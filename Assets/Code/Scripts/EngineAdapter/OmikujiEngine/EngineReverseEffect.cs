using System;
using LitMotion;
using Unity.Cinemachine;
using UnityEngine;
using UsefulToolkit.BlackBoard.BlackBoard;
using UsefulToolkit.Initialization;
using ZoukeiJam1.BlackBoard.OmikujiEngine;

namespace ZoukeiJam1.EngineAdapter.OmikujiEngine
{
    /// <summary>
    /// OmikujiEngine が逆回転して回転速度が 0 に戻ったとき、画面振動を発生させ、
    /// 画面の縁の色の濃さを 1 にしてから 0 へ戻す。
    /// 逆回転する直前の回転速度が _minSpeedToTrigger 未満のときは何もしない
    /// </summary>
    public sealed class EngineReverseEffect : InitializableMonoBehaviour
    {
        [Tooltip("逆回転したときに発生させる画面振動")]
        [SerializeField] private CinemachineImpulseSource _impulse;

        [Tooltip("逆回転したときに色をつける画面の縁")]
        [SerializeField] private ScreenEdgeTint _edgeTint;

        [Tooltip("縁の色が消えるまでの時間（秒）")]
        [SerializeField, Min(0f)] private float _fadeOutDuration = 0.6f;

        [Tooltip("縁の色が消えるときのイージング")]
        [SerializeField] private Ease _fadeOutEase = Ease.OutCubic;

        [Tooltip("逆回転する直前の回転速度（回転/秒）がこの値以上のときだけ演出を出す。0 なら常に出す")]
        [SerializeField, Min(0f)] private float _minSpeedToTrigger = 0.3f;

        private IOmikujiEngineState _engineState;
        private IDisposable _isReversingChangedRegistration;
        private MotionHandle _fadeHandle;

        /// <summary>前のフレームの終わりに読み取った回転速度（回転/秒）</summary>
        private float _previousSpeed;

        /// <param name="engineState">逆回転の検知と回転速度の読み取り元</param>
        public void Initialize(IOmikujiEngineState engineState)
        {
            _engineState = engineState;
            _previousSpeed = engineState.RotationSpeed;
            if (_edgeTint != null) _edgeTint.SetIntensity(0f);
            _isReversingChangedRegistration = engineState.RegisterEventOnIsReversingChanged(
                new ActionEntry<StateContext<bool>>(false, OnIsReversingChanged));

            base.Initialize();
        }

        // 通知の時点で State の回転速度はすでに 0 になっているため、逆回転する直前の速度をフレームの終わりに記録しておく
        private void LateUpdate()
        {
            if (_engineState != null) _previousSpeed = _engineState.RotationSpeed;
        }

        private void OnIsReversingChanged(StateContext<bool> context)
        {
            if (!context.NewValue || _previousSpeed < _minSpeedToTrigger) return;

            if (_impulse != null) _impulse.GenerateImpulse();
            PlayEdgeTint();
        }

        /// <summary>縁の色の濃さを 1 にし、_fadeOutDuration かけて 0 へ戻す。再生中なら最初からやり直す</summary>
        private void PlayEdgeTint()
        {
            if (_edgeTint == null) return;

            _fadeHandle.TryCancel();
            _edgeTint.SetIntensity(1f);
            _fadeHandle = LMotion.Create(1f, 0f, _fadeOutDuration)
                .WithEase(_fadeOutEase)
                .Bind(_edgeTint, static (intensity, edgeTint) => edgeTint.SetIntensity(intensity));
        }

        private void OnDestroy()
        {
            _fadeHandle.TryCancel();
            _isReversingChangedRegistration?.Dispose();
        }
    }
}
