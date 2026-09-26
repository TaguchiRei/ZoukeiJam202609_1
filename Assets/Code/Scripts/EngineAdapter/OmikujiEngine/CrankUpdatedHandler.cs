using ZoukeiJam1.BlackBoard.OmikujiEngine;

namespace ZoukeiJam1.EngineAdapter.OmikujiEngine
{
    /// <summary>OmikujiEngineManager が毎フレーム、クランクの動きを渡すコールバック</summary>
    /// <param name="angleChange">このフレームでのクランク角の変化量（rad、正転方向が正）</param>
    /// <param name="revolutionCount">初期状態から正転で上死点を通過した回数</param>
    /// <param name="stroke">現在のピストンの行程</param>
    /// <param name="deltaTime">前のフレームからの経過時間（秒）</param>
    public delegate void CrankUpdatedHandler(float angleChange, int revolutionCount, PistonStroke stroke, float deltaTime);
}
