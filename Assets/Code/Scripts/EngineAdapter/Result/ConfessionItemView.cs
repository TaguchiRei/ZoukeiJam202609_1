using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZoukeiJam1.BlackBoard.Race;

namespace ZoukeiJam1.EngineAdapter.Result
{
    /// <summary>
    /// リザルトの告白アイテム。最初は未確定の表示にしておき、決まったアイテムの見た目へ拡大しながら変えてから、成功率をカウントアップする
    /// </summary>
    public sealed class ConfessionItemView : MonoBehaviour
    {
        /// <summary>告白アイテム 1 つ分の見た目</summary>
        [Serializable]
        private struct Appearance
        {
            public ConfessionItem Item;

            [Tooltip("アイコンの画像。空ならアイコンを色だけで塗る")]
            public Sprite Sprite;

            [Tooltip("アイコンの色。画像を使うときは白にする")]
            public Color Color;

            [Tooltip("アイコンの下に出す名前")]
            public string Name;
        }

        [Tooltip("おみくじを吸い込む位置")]
        [SerializeField] private RectTransform _target;

        [Tooltip("アイコンとその名前をまとめたオブジェクト。変化するときに拡大させる")]
        [SerializeField] private RectTransform _iconRoot;

        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameText;

        [Tooltip("成功率の表示。初期化時に非表示にする")]
        [SerializeField] private CountUpText _successRate;

        [Tooltip("アイテムが決まる前に出す名前")]
        [SerializeField] private string _undecidedName = "？";

        [Tooltip("アイテムが決まる前のアイコンの色")]
        [SerializeField] private Color _undecidedColor = new(1f, 1f, 1f, 0.25f);

        [SerializeField] private Appearance[] _appearances =
        {
            new() { Item = ConfessionItem.Letter, Color = new Color(0.95f, 0.95f, 0.9f), Name = "手紙" },
            new() { Item = ConfessionItem.Bouquet, Color = new Color(1f, 0.55f, 0.7f), Name = "花束" },
            new() { Item = ConfessionItem.Ring, Color = new Color(1f, 0.85f, 0.3f), Name = "指輪" },
        };

        /// <summary>おみくじを吸い込む位置</summary>
        public RectTransform Target => _target;

        /// <summary>未確定の表示にし、成功率を隠す</summary>
        public void Prepare()
        {
            _icon.sprite = null;
            _icon.color = _undecidedColor;
            _nameText.text = _undecidedName;
            _successRate.gameObject.SetActive(false);
        }

        /// <summary>決まったアイテムの見た目へ拡大しながら変え、成功率を 0 からカウントアップする</summary>
        /// <param name="item">決まったアイテム</param>
        /// <param name="successRate">告白の成功率（%）</param>
        /// <param name="popSeconds">拡大にかける時間（秒）</param>
        /// <param name="countUpSeconds">成功率のカウントアップにかける時間（秒）</param>
        public async UniTask ChangeAsync(
            ConfessionItem item, float successRate, float popSeconds, float countUpSeconds, CancellationToken cancellationToken)
        {
            var appearance = FindAppearance(item);
            _icon.sprite = appearance.Sprite;
            _icon.color = appearance.Color;
            _nameText.text = appearance.Name;

            await LMotion.Create(Vector3.zero, Vector3.one, popSeconds)
                .WithEase(Ease.OutBack)
                .BindToLocalScale(_iconRoot)
                .ToUniTask(cancellationToken);

            _successRate.gameObject.SetActive(true);
            await _successRate.PlayAsync(successRate, countUpSeconds, cancellationToken);
        }

        private Appearance FindAppearance(ConfessionItem item)
        {
            foreach (var appearance in _appearances)
            {
                if (appearance.Item == item) return appearance;
            }

            return new Appearance { Item = item, Color = Color.white, Name = item.ToString() };
        }
    }
}
