using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;

namespace ZoukeiJam1.EngineAdapter.Race
{
    /// <summary>
    /// 画面全体に広げた GoalBurst シェーダーの Image で、ゴール時の画面演出（フラッシュ・集中線・画面の縁の光）を再生する。
    /// 再生を始めてからは、集中線の中心を毎フレーム _focusTarget の画面上の位置に合わせる
    /// </summary>
    public sealed class GoalScreenEffect : MonoBehaviour
    {
        private static readonly int CenterId = Shader.PropertyToID("_Center");
        private static readonly int AspectId = Shader.PropertyToID("_Aspect");
        private static readonly int LineIntensityId = Shader.PropertyToID("_LineIntensity");
        private static readonly int FlashId = Shader.PropertyToID("_Flash");
        private static readonly int GlowIntensityId = Shader.PropertyToID("_GlowIntensity");

        [Tooltip("画面全体に広げた、GoalBurst シェーダーのマテリアルを設定した Image。初期化時に非表示にする")]
        [SerializeField] private Image _image;

        [Tooltip("集中線の中心の画面上の位置を求めるカメラ")]
        [SerializeField] private Camera _camera;

        [Tooltip("集中線の中心にする位置")]
        [SerializeField] private Transform _focusTarget;

        [Tooltip("フラッシュが消えるまでの時間（秒）")]
        [SerializeField, Min(0f)] private float _flashSeconds = 0.35f;

        [Tooltip("集中線が画面の外から伸びきるまでの時間（秒）")]
        [SerializeField, Min(0f)] private float _lineInSeconds = 0.25f;

        [Tooltip("集中線が伸びきった後に落ち着く強さ（0-1。小さいほど線が外側へ下がる）")]
        [SerializeField, Range(0f, 1f)] private float _lineSustainIntensity = 0.8f;

        [Tooltip("集中線が伸びきってから落ち着くまでの時間（秒）")]
        [SerializeField, Min(0f)] private float _lineSettleSeconds = 0.6f;

        [Tooltip("画面の縁の光が最大になるまでの時間（秒）")]
        [SerializeField, Min(0f)] private float _glowInSeconds = 0.5f;

        private Material _material;

        /// <summary>Image のマテリアルを複製して差し替え、非表示にする</summary>
        public void Initialize()
        {
            _material = new Material(_image.material);
            _image.material = _material;
            _image.raycastTarget = false;
            _image.enabled = false;
        }

        /// <summary>Image を表示し、フラッシュ・集中線・画面の縁の光を再生する。集中線と縁の光は再生後も表示したままにする</summary>
        public void Play()
        {
            PlayAsync().Forget();
        }

        private async UniTaskVoid PlayAsync()
        {
            var token = destroyCancellationToken;

            _material.SetFloat(FlashId, 1f);
            _material.SetFloat(LineIntensityId, 0f);
            _material.SetFloat(GlowIntensityId, 0f);
            UpdateCenter();
            _image.enabled = true;

            LMotion.Create(1f, 0f, _flashSeconds)
                .WithEase(Ease.OutCubic)
                .Bind(_material, static (value, material) => material.SetFloat(FlashId, value))
                .ToUniTask(token)
                .Forget();
            LMotion.Create(0f, 1f, _glowInSeconds)
                .WithEase(Ease.OutCubic)
                .Bind(_material, static (value, material) => material.SetFloat(GlowIntensityId, value))
                .ToUniTask(token)
                .Forget();

            await LMotion.Create(0f, 1f, _lineInSeconds)
                .WithEase(Ease.OutExpo)
                .Bind(_material, static (value, material) => material.SetFloat(LineIntensityId, value))
                .ToUniTask(token);
            await LMotion.Create(1f, _lineSustainIntensity, _lineSettleSeconds)
                .WithEase(Ease.OutCubic)
                .Bind(_material, static (value, material) => material.SetFloat(LineIntensityId, value))
                .ToUniTask(token);
        }

        private void LateUpdate()
        {
            if (_image.enabled) UpdateCenter();
        }

        /// <summary>集中線の中心を _focusTarget の画面上の位置に、縦横比を Image の矩形に合わせる</summary>
        private void UpdateCenter()
        {
            Vector3 viewport = _camera.WorldToViewportPoint(_focusTarget.position);
            _material.SetVector(CenterId, new Vector4(viewport.x, viewport.y, 0f, 0f));

            Rect rect = _image.rectTransform.rect;
            _material.SetFloat(AspectId, rect.height > 0f ? rect.width / rect.height : 1f);
        }

        private void OnDestroy()
        {
            if (_material != null) Destroy(_material);
        }
    }
}
