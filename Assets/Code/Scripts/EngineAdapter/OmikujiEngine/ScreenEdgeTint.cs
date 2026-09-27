using UnityEngine;
using UnityEngine.UI;

namespace ZoukeiJam1.EngineAdapter.OmikujiEngine
{
    /// <summary>
    /// 画面全体に広げた RawImage に、画面の端で最も濃く、内側へ向かうほど薄くなるグラデーションを表示する。
    /// 各辺から画面の幅・高さの _edgeRatio より内側は完全に透明にする。
    /// グラデーションのテクスチャは Awake で生成するため、_edgeRatio の変更は次の再生から反映される
    /// </summary>
    public sealed class ScreenEdgeTint : MonoBehaviour
    {
        [Tooltip("画面全体に広げた RawImage")]
        [SerializeField] private RawImage _image;

        [Tooltip("縁の色。アルファは濃さが最大のときの不透明度")]
        [SerializeField] private Color _color = Color.red;

        [Tooltip("色がつく帯の太さ。各辺から画面の幅・高さに対するこの割合より内側は影響を受けない")]
        [SerializeField, Range(0.01f, 0.5f)] private float _edgeRatio = 0.1f;

        [Tooltip("グラデーションのテクスチャの解像度（1 辺のピクセル数）")]
        [SerializeField, Min(8)] private int _textureSize = 256;

        private Texture2D _texture;

        private void Awake()
        {
            _texture = CreateGradientTexture(_textureSize, _edgeRatio);
            _image.texture = _texture;
            _image.raycastTarget = false;
            SetIntensity(0f);
        }

        /// <summary>縁の濃さを設定する。0 で非表示、1 で _color の不透明度になる</summary>
        public void SetIntensity(float intensity)
        {
            Color color = _color;
            color.a *= Mathf.Clamp01(intensity);
            _image.color = color;
        }

        /// <summary>
        /// 白で、最も近い辺からの距離が 0 のとき不透明度 1、edgeRatio 以上で 0 になるテクスチャを作る。
        /// 間は smoothstep で変化させる
        /// </summary>
        private static Texture2D CreateGradientTexture(int size, float edgeRatio)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = nameof(ScreenEdgeTint)
            };

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                float v = (y + 0.5f) / size;
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size;
                    float distance = Mathf.Min(Mathf.Min(u, 1f - u), Mathf.Min(v, 1f - v));
                    float alpha = Mathf.SmoothStep(0f, 1f, 1f - Mathf.Clamp01(distance / edgeRatio));
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private void OnDestroy()
        {
            if (_texture != null) Destroy(_texture);
        }
    }
}
