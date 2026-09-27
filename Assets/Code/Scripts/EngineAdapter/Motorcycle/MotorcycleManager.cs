using UnityEngine;
using UsefulToolkit.Initialization;
using ZoukeiJam1.BlackBoard.Race;

namespace ZoukeiJam1.EngineAdapter.Motorcycle
{
    /// <summary>
    /// 走行の State の速度から、タイヤの回転と車体の上下振動を計算して Transform に反映する。
    /// バイク自体はその場から動かさない。速度の単位はユニット/秒。
    /// 開始演出の間（RacePhase.Starting）はタイヤを回さず、車体の振動だけを行う
    /// </summary>
    public sealed class MotorcycleManager : InitializableMonoBehaviour
    {
        private const float TwoPi = Mathf.PI * 2f;

        [Tooltip("上下に振動させる車体（本体と人）")]
        [SerializeField] private Transform _body;

        [Tooltip("後輪の回転中心")]
        [SerializeField] private Transform _backWheel;

        [Tooltip("前輪の回転中心")]
        [SerializeField] private Transform _frontWheel;

        [Tooltip("タイヤの半径（ユニット）")]
        [SerializeField, Min(0.01f)] private float _tireRadius = 1.69f;

        [Header("小さい振動")]
        [Tooltip("振幅（ユニット）")]
        [SerializeField, Min(0f)] private float _smallVibrationAmplitude = 0.02f;

        [Tooltip("1 ユニット進むごとに振動する回数")]
        [SerializeField, Min(0f)] private float _smallVibrationCyclesPerUnit = 0.8f;

        [Tooltip("この速度（ユニット/秒）以上で振幅が最大になる。これより遅いと速度に比例して小さくなる")]
        [SerializeField, Min(0.01f)] private float _smallVibrationFullAmplitudeSpeed = 5f;

        [Header("大きい振動")]
        [Tooltip("高さ（ユニット）。カーブの値 1 がこの高さになる")]
        [SerializeField, Min(0f)] private float _bumpHeight = 0.12f;

        [Tooltip("1 回の振動にかかる時間（秒）")]
        [SerializeField, Min(0.01f)] private float _bumpDuration = 0.35f;

        [Tooltip("振動の形。横軸は経過時間の割合（0〜1）、縦軸は高さの割合")]
        [SerializeField] private AnimationCurve _bumpCurve = new(
            new Keyframe(0f, 0f),
            new Keyframe(0.2f, 1f),
            new Keyframe(0.55f, -0.25f),
            new Keyframe(1f, 0f));

        [Tooltip("振動の間隔の最小値（ユニット）。この距離を進むごとに振動する")]
        [SerializeField, Min(0f)] private float _bumpMinInterval = 8f;

        [Tooltip("振動の間隔の最大値（ユニット）")]
        [SerializeField, Min(0f)] private float _bumpMaxInterval = 20f;

        private Vector3 _bodyBasePosition;

        /// <summary>小さい振動の位相（rad）</summary>
        private float _smallVibrationPhase;

        /// <summary>次の大きい振動までに進む残りの距離</summary>
        private float _distanceUntilBump;

        /// <summary>大きい振動が始まってからの経過時間。振動していないときは _bumpDuration 以上</summary>
        private float _bumpElapsed = float.PositiveInfinity;

        /// <summary>現在の速度（ユニット/秒）</summary>
        public float Speed { get; private set; }

        private IRaceState _raceState;

        /// <summary>車体の初期位置を振動の基準として記録し、動作を開始する</summary>
        /// <param name="raceState">速度の読み取り元</param>
        public void Initialize(IRaceState raceState)
        {
            _raceState = raceState;
            _bodyBasePosition = _body.localPosition;
            _distanceUntilBump = NextBumpInterval();

            base.Initialize();
        }

        /// <summary>速度（ユニット/秒）を設定する。負の値は 0 として扱う</summary>
        public void SetSpeed(float speed)
        {
            Speed = Mathf.Max(0f, speed);
        }

        private void Update()
        {
            SetSpeed(_raceState.Speed);
            float distance = Speed * Time.deltaTime;

            if (_raceState.Phase != RacePhase.Starting) RotateWheels(distance);
            float offset = SmallVibrationOffset(distance) + BumpOffset(distance);
            _body.localPosition = _bodyBasePosition + Vector3.up * offset;
        }

        /// <summary>進んだ距離の分だけ、空転せずに転がる角度で前後のタイヤを時計回りに回す</summary>
        private void RotateWheels(float distance)
        {
            float degrees = -distance / _tireRadius * Mathf.Rad2Deg;
            _backWheel.Rotate(0f, 0f, degrees);
            _frontWheel.Rotate(0f, 0f, degrees);
        }

        /// <summary>進んだ距離だけ位相を進め、小さい振動の高さを返す</summary>
        private float SmallVibrationOffset(float distance)
        {
            _smallVibrationPhase = Mathf.Repeat(_smallVibrationPhase + distance * _smallVibrationCyclesPerUnit * TwoPi, TwoPi);
            float amplitude = _smallVibrationAmplitude * Mathf.Clamp01(Speed / _smallVibrationFullAmplitudeSpeed);
            return amplitude * Mathf.Sin(_smallVibrationPhase);
        }

        /// <summary>
        /// 振動していない間は進んだ距離を数え、次の間隔に達したら大きい振動を開始する。
        /// 振動中の高さを返す
        /// </summary>
        private float BumpOffset(float distance)
        {
            _bumpElapsed += Time.deltaTime;

            if (_bumpElapsed >= _bumpDuration)
            {
                _distanceUntilBump -= distance;
                if (_distanceUntilBump > 0f) return 0f;

                _distanceUntilBump = NextBumpInterval();
                _bumpElapsed = 0f;
            }

            return _bumpHeight * _bumpCurve.Evaluate(_bumpElapsed / _bumpDuration);
        }

        private float NextBumpInterval()
        {
            return Random.Range(_bumpMinInterval, _bumpMaxInterval);
        }
    }
}
