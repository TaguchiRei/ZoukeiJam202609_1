using UnityEngine;
using UsefulToolkit.BlackBoard.BlackBoard;
using UsefulToolkit.Initialization;
using ZoukeiJam1.Application.SceneTransition;
using ZoukeiJam1.EngineAdapter.Title;

namespace ZoukeiJam1.Initialization.Title
{
    /// <summary>タイトル画面のスタートボタンに、インゲームへの遷移をつなぐ</summary>
    public sealed class TitleInitializer : InitializerBase, IInjectable<IGameSceneTransition>
    {
        [SerializeField] private TitleView _titleView;

        private IGameSceneTransition _sceneTransition;

        public void Inject(IGameSceneTransition sceneTransition)
        {
            _sceneTransition = sceneTransition;
        }

        public override void Initialize(IBlackBoard blackBoard)
        {
            _titleView.Initialize(_sceneTransition.ToRace);
            base.Initialize(blackBoard);
        }
    }
}
