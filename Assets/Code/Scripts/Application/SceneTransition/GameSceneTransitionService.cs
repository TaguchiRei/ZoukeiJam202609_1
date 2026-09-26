using System;
using Cysharp.Threading.Tasks;
using UsefulToolkit.BlackBoard.Scene;

namespace ZoukeiJam1.Application.SceneTransition
{
    /// <summary>
    /// タイトル・インゲーム・リザルトの各シーンへ上書きロードで遷移する。
    /// 遷移中に来た要求は無視する
    /// </summary>
    public sealed class GameSceneTransitionService : IGameSceneTransition
    {
        private readonly int _titleSceneId;
        private readonly int _raceSceneId;
        private readonly int _resultSceneId;

        private ISceneState _sceneState;
        private bool _isTransitioning;

        /// <param name="titleSceneId">タイトルシーンの build index</param>
        /// <param name="raceSceneId">インゲームのシーンの build index</param>
        /// <param name="resultSceneId">リザルトシーンの build index</param>
        public GameSceneTransitionService(int titleSceneId, int raceSceneId, int resultSceneId)
        {
            _titleSceneId = titleSceneId;
            _raceSceneId = raceSceneId;
            _resultSceneId = resultSceneId;
        }

        /// <summary>遷移の要求先を設定する。遷移を要求する前に呼ぶ</summary>
        public void SetSceneState(ISceneState sceneState)
        {
            _sceneState = sceneState;
        }

        public void ToTitle() => TransitionAsync(_titleSceneId).Forget();

        public void ToRace() => TransitionAsync(_raceSceneId).Forget();

        public void ToResult() => TransitionAsync(_resultSceneId).Forget();

        private async UniTaskVoid TransitionAsync(int sceneId)
        {
            if (_isTransitioning) return;

            _isTransitioning = true;
            try
            {
                // ロード済みのシーンは読み直されないため、同じシーンへ遷移するときは先にアンロードする
                if (_sceneState.IsLoaded(sceneId))
                    await _sceneState.RequestOverwriteLoadAsync(SceneState.NoSceneId, Array.Empty<int>());

                await _sceneState.RequestOverwriteLoadAsync(sceneId, Array.Empty<int>());
            }
            finally
            {
                _isTransitioning = false;
            }
        }
    }
}
