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
    /// OmikujiEngine がミスで止まったとき、画面振動を発生させ、
    /// 画面の縁の色の濃さを 1 にしてから 0 へ戻す
    /// </summary>
    public sealed class EngineReverseEffect : InitializableMonoBehaviour
    {
        [Tooltip("ミスしたときに発生させる画面振動")]
        [SerializeField] private CinemachineImpulseSource _impulse;

        [Tooltip("ミスしたときに色をつける画面の縁")]
        [SerializeField] private ScreenEdgeTint _edgeTint;

        [Tooltip("縁の色が消えるまでの時間（秒）")]
        [SerializeField, Min(0f)] private float _fadeOutDuration = 0.6f;

        [Tooltip("縁の色が消えるときのイージング")]
        [SerializeField] private Ease _fadeOutEase = Ease.OutCubic;

        private IDisposable _isStalledChangedRegistration;
        private MotionHandle _fadeHandle;

        /// <param name="engineState">ミスの検知元</param>
        public void Initialize(IOmikujiEngineState engineState)
        {
            if (_edgeTint != null) _edgeTint.SetIntensity(0f);
            _isStalledChangedRegistration = engineState.RegisterEventOnIsStalledChanged(
                new ActionEntry<StateContext<bool>>(false, OnIsStalledChanged));

            base.Initialize();
        }

        private void OnIsStalledChanged(StateContext<bool> context)
        {
            if (!context.NewValue) return;

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
            _isStalledChangedRegistration?.Dispose();
        }
    }
}
