using System;
using UsefulToolkit.BlackBoard.BlackBoard;

namespace ZoukeiJam1.BlackBoard.OmikujiEngine
{
    /// <summary>
    /// OmikujiEngine の回転数・ピストンの行程・逆回転中かを保持し、値の変化時に登録された Action を実行する。
    /// BlackBoard へは <see cref="IOmikujiEngineState"/> としてのみ登録し、
    /// 値の変更は具象型を保持する生成元だけが行う
    /// </summary>
    [RegisterBoard(typeof(OmikujiEngineBoard))]
    public sealed class OmikujiEngineState : SceneStateBase, IOmikujiEngineState
    {
        private readonly ActionEntryList<StateContext<int>> _revolutionCountChangedActions = new();
        private readonly ActionEntryList<StateContext<PistonStroke>> _strokeChangedActions = new();
        private readonly ActionEntryList<StateContext<bool>> _isReversingChangedActions = new();

        public int RevolutionCount { get; private set; }

        public PistonStroke Stroke { get; private set; }

        public bool IsReversing { get; private set; }

        /// <param name="stroke">初期状態のピストンの行程</param>
        public OmikujiEngineState(PistonStroke stroke)
        {
            Stroke = stroke;
        }

        /// <summary>
        /// 3 つの値をまとめて更新し、変化した値の Action を実行する。
        /// Action の中から他の値を読んでも更新後の値が返るよう、すべての値を更新してから通知する
        /// </summary>
        public void Apply(int revolutionCount, PistonStroke stroke, bool isReversing)
        {
            int oldRevolutionCount = RevolutionCount;
            PistonStroke oldStroke = Stroke;
            bool oldIsReversing = IsReversing;

            RevolutionCount = revolutionCount;
            Stroke = stroke;
            IsReversing = isReversing;

            if (oldRevolutionCount != revolutionCount)
                _revolutionCountChangedActions.Invoke(new StateContext<int>(oldRevolutionCount, revolutionCount));
            if (oldStroke != stroke)
                _strokeChangedActions.Invoke(new StateContext<PistonStroke>(oldStroke, stroke));
            if (oldIsReversing != isReversing)
                _isReversingChangedActions.Invoke(new StateContext<bool>(oldIsReversing, isReversing));
        }

        public IDisposable RegisterEventOnRevolutionCountChanged(ActionEntry<StateContext<int>> changedAction)
        {
            return _revolutionCountChangedActions.Register(changedAction, nameof(changedAction));
        }

        public IDisposable RegisterEventOnStrokeChanged(ActionEntry<StateContext<PistonStroke>> changedAction)
        {
            return _strokeChangedActions.Register(changedAction, nameof(changedAction));
        }

        public IDisposable RegisterEventOnIsReversingChanged(ActionEntry<StateContext<bool>> changedAction)
        {
            return _isReversingChangedActions.Register(changedAction, nameof(changedAction));
        }

        public override string GetLog()
        {
            return $"RevolutionCount : {RevolutionCount} / Stroke : {Stroke} / IsReversing : {IsReversing}";
        }
    }
}
