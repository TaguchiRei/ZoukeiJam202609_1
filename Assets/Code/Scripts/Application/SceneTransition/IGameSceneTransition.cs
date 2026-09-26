namespace ZoukeiJam1.Application.SceneTransition
{
    /// <summary>ゲームのシーン遷移を要求する操作面。BlackBoard には載せず、DI で渡す</summary>
    public interface IGameSceneTransition
    {
        /// <summary>タイトルシーンへ遷移する</summary>
        void ToTitle();

        /// <summary>インゲームのシーンへ遷移する。インゲームのシーンにいるときは読み直す</summary>
        void ToRace();

        /// <summary>リザルトシーンへ遷移する</summary>
        void ToResult();
    }
}
