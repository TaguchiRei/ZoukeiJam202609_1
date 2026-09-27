using UnityEngine;
using UsefulToolkit.Initialization;
using ZoukeiJam1.BlackBoard.Race;

namespace ZoukeiJam1.EngineAdapter.Background
{
    /// <summary>
    /// 走行の State の速度（ユニット/秒）を積算した距離と、その速度を ScrollingScenery シェーダーに渡して背景を流す。
    /// 開始演出の間（RacePhase.Starting）は速度を 0 として扱い、背景を止める。
    /// 背景を描く Renderer は、カメラの表示範囲より少し広い大きさにしてカメラの正面に置く
    /// </summary>
    public sealed class ScrollingBackground : InitializableMonoBehaviour
    {
        private static readonly int ScrollDistanceId = Shader.PropertyToID("_ScrollDistance");
        private static readonly int ScrollSpeedId = Shader.PropertyToID("_ScrollSpeed");

        [Tooltip("背景を描く Renderer。ScrollingScenery シェーダーのマテリアルを設定した、親のスケールが 1 の Quad にする")]
        [SerializeField] private Renderer _renderer;

        [Tooltip("表示範囲を合わせる正投影カメラ")]
        [SerializeField] private Camera _camera;

        [Tooltip("表示範囲より外側に広げる幅（ユニット）")]
        [SerializeField, Min(0f)] private float _margin = 1f;

        [Tooltip("Renderer の Order in Layer。スプライトより奥に描くため、他より小さい値にする")]
        [SerializeField] private int _sortingOrder = -1000;

        private IRaceState _raceState;
        private MaterialPropertyBlock _propertyBlock;

        /// <summary>開始演出が終わってから速度を積算した距離（ユニット）</summary>
        private float _distance;

        /// <summary>速度の読み取り元を受け取り、Renderer の描画順を設定して動作を開始する</summary>
        /// <param name="raceState">速度の読み取り元</param>
        public void Initialize(IRaceState raceState)
        {
            _raceState = raceState;
            _propertyBlock = new MaterialPropertyBlock();
            _renderer.sortingOrder = _sortingOrder;

            base.Initialize();
        }

        private void Update()
        {
            float speed = _raceState.Phase == RacePhase.Starting ? 0f : Mathf.Max(0f, _raceState.Speed);
            _distance += speed * Time.deltaTime;

            FitToCamera();

            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(ScrollDistanceId, _distance);
            _propertyBlock.SetFloat(ScrollSpeedId, speed);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        /// <summary>Renderer の X・Y をカメラに合わせ、大きさを表示範囲に余白を足した大きさにする。Z は変えない</summary>
        private void FitToCamera()
        {
            float height = _camera.orthographicSize * 2f;
            float width = height * _camera.aspect;

            Transform target = _renderer.transform;
            Vector3 cameraPosition = _camera.transform.position;
            target.position = new Vector3(cameraPosition.x, cameraPosition.y, target.position.z);
            target.localScale = new Vector3(width + _margin * 2f, height + _margin * 2f, 1f);
        }
    }
}
