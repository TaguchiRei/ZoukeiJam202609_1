using System;
using UsefulToolkit.BlackBoard.BlackBoard;

namespace ZoukeiJam1.BlackBoard.OmikujiEngine
{
    /// <summary>OmikujiEngine の State の読み取り面</summary>
    public interface IOmikujiEngineState : IStateGetter
    {
        /// <summary>初期状態から正転で上死点を通過した回数</summary>
        int RevolutionCount { get; }

        /// <summary>ピストンの行程</summary>
        PistonStroke Stroke { get; }

        /// <summary>本来の回転方向と逆に回っているか</summary>
        bool IsReversing { get; }

        /// <summary>逆方向へ戻しすぎてミスとなり、回転速度が 0 で止まっているか。正転へ動き出すと false に戻る</summary>
        bool IsStalled { get; }

        /// <summary>エンジンの回転速度（回転/秒）。変化の通知は行わない</summary>
        float RotationSpeed { get; }

        /// <summary>正転方向に積算したクランク角（rad）。0 でクランクピンがクランク中心の真上に来る。変化の通知は行わない</summary>
        float CrankAngle { get; }

        /// <summary>RevolutionCount の変化時に実行する Action を登録する</summary>
        /// <returns>Dispose すると登録を解除できる</returns>
        IDisposable RegisterEventOnRevolutionCountChanged(ActionEntry<StateContext<int>> changedAction);

        /// <summary>Stroke の変化時に実行する Action を登録する</summary>
        /// <returns>Dispose すると登録を解除できる</returns>
        IDisposable RegisterEventOnStrokeChanged(ActionEntry<StateContext<PistonStroke>> changedAction);

        /// <summary>IsReversing の変化時に実行する Action を登録する</summary>
        /// <returns>Dispose すると登録を解除できる</returns>
        IDisposable RegisterEventOnIsReversingChanged(ActionEntry<StateContext<bool>> changedAction);

        /// <summary>IsStalled の変化時に実行する Action を登録する</summary>
        /// <returns>Dispose すると登録を解除できる</returns>
        IDisposable RegisterEventOnIsStalledChanged(ActionEntry<StateContext<bool>> changedAction);
    }
}
