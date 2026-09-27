using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using Unity.Cinemachine;
using UnityEngine;
using UsefulToolkit.BlackBoard.BlackBoard;
using ZoukeiJam1.BlackBoard.Race;
using ZoukeiJam1.EngineAdapter.Fade;

namespace ZoukeiJam1.EngineAdapter.Race
{
    /// <summary>
    /// 走行の進行段階がゴールになったら、HUD をフェードアウトさせ、画面演出を再生し、画面を振動させながらマフラーを映すカメラへ切り替える。
    /// 少し待ってからおみくじをマフラーから回転させながら飛び出させ、止まってから少し待って暗転し、終了を通知する
    /// </summary>
    public sealed class GoalPresenter : MonoBehaviour
    {
        [Tooltip("ゴール時に切り替えるカメラ。初期化時に非アクティブにし、ゴールでアクティブにする。" +
                 "走行中のカメラより Priority を高くしておく。切り替えにかかる時間は CinemachineBrain の Default Blend で設定する")]
        [SerializeField] private CinemachineCamera _focusCamera;

        [Tooltip("ゴール時に再生する画面演出（フラッシュ・集中線・画面の縁の光）")]
        [SerializeField] private GoalScreenEffect _screenEffect;

        [Tooltip("ゴール時に発生させる画面振動")]
        [SerializeField] private CinemachineImpulseSource _impulse;

        [Tooltip("ゴール時にフェードアウトさせる HUD")]
        [SerializeField] private CanvasGroup _hud;

        [Tooltip("HUD のフェードアウトにかける時間（秒）")]
        [SerializeField, Min(0f)] private float _hudFadeSeconds = 0.3f;

        [Tooltip("ゴールしてからおみくじが飛び出し始めるまでの時間（秒）")]
        [SerializeField, Min(0f)] private float _popDelaySeconds = 0.4f;

        [Tooltip("おみくじが出てくるマフラーの出口")]
        [SerializeField] private Transform _mufflerExit;

        [Tooltip("マフラーから出てくるおみくじ。初期化時に非表示にする")]
        [SerializeField] private Transform _omikuji;

        [Tooltip("おみくじが飛んでいく先の、マフラーの出口からの相対位置（ユニット）")]
        [SerializeField] private Vector3 _popOffset = new(-1.5f, 3f, 0f);

        [Tooltip("おみくじが飛び出してから止まるまでの時間（秒）")]
        [SerializeField, Min(0.01f)] private float _popDuration = 0.8f;

        [Tooltip("おみくじが飛び出す間に回る角度（度、正で反時計回り）")]
        [SerializeField] private float _spinDegrees = 540f;

        [Tooltip("おみくじが出始めるときの大きさ（止まったときの大きさに対する倍率）")]
        [SerializeField, Min(0f)] private float _startScale = 0.2f;

        [Tooltip("おみくじが止まってから暗転を始めるまでの時間（秒）")]
        [SerializeField, Min(0f)] private float _holdSeconds = 0.5f;

        [SerializeField] private ScreenFader _fader;

        [Tooltip("暗転にかける時間（秒）")]
        [SerializeField, Min(0f)] private float _fadeOutSeconds = 0.6f;

        private Vector3 _omikujiScale;
        private Action _onFinished;
        private IDisposable _phaseChangedRegistration;

        /// <param name="raceState">ゴールの検知に使う</param>
        /// <param name="onFinished">暗転が終わったときの処理</param>
        public void Initialize(IRaceState raceState, Action onFinished)
        {
            _onFinished = onFinished;
            _omikujiScale = _omikuji.localScale;
            _omikuji.gameObject.SetActive(false);
            _focusCamera.gameObject.SetActive(false);
            _screenEffect.Initialize();
            _hud.alpha = 1f;
            _fader.SetAlpha(0f);
            _phaseChangedRegistration = raceState.RegisterEventOnPhaseChanged(
                new ActionEntry<StateContext<RacePhase>>(false, OnPhaseChanged));
        }

        private void OnPhaseChanged(StateContext<RacePhase> context)
        {
            if (context.NewValue == RacePhase.Goal) PlayAsync().Forget();
        }

        private async UniTaskVoid PlayAsync()
        {
            var token = destroyCancellationToken;

            _focusCamera.gameObject.SetActive(true);
            _screenEffect.Play();
            _impulse.GenerateImpulse();
            LMotion.Create(_hud.alpha, 0f, _hudFadeSeconds)
                .Bind(_hud, static (alpha, hud) => hud.alpha = alpha)
                .ToUniTask(token)
                .Forget();

            await UniTask.Delay(TimeSpan.FromSeconds(_popDelaySeconds), cancellationToken: token);

            Vector3 start = _mufflerExit.position;

            _omikuji.position = start;
            _omikuji.localScale = _omikujiScale * _startScale;
            _omikuji.gameObject.SetActive(true);

            await UniTask.WhenAll(
                LMotion.Create(start, start + _popOffset, _popDuration)
                    .WithEase(Ease.OutBack)
                    .BindToPosition(_omikuji)
                    .ToUniTask(token),
                LMotion.Create(0f, _spinDegrees, _popDuration)
                    .WithEase(Ease.OutCubic)
                    .BindToLocalEulerAnglesZ(_omikuji)
                    .ToUniTask(token),
                LMotion.Create(_omikujiScale * _startScale, _omikujiScale, _popDuration)
                    .WithEase(Ease.OutCubic)
                    .BindToLocalScale(_omikuji)
                    .ToUniTask(token));

            await UniTask.Delay(TimeSpan.FromSeconds(_holdSeconds), cancellationToken: token);
            await _fader.FadeAsync(1f, _fadeOutSeconds, token);
            _onFinished();
        }

        private void OnDestroy()
        {
            _phaseChangedRegistration?.Dispose();
        }
    }
}
