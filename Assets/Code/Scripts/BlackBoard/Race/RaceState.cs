using System;
using UsefulToolkit.BlackBoard.BlackBoard;

namespace ZoukeiJam1.BlackBoard.Race
{
    /// <summary>
    /// 走行の進行段階・目標距離・制限時間・進んだ距離・経過時間・速度を保持し、進行段階の変化時に登録された Action を実行する。
    /// BlackBoard へは <see cref="IRaceState"/> としてのみ登録し、値の変更は具象型を保持する生成元だけが行う
    /// </summary>
    [RegisterBoard(typeof(RaceBoard))]
    public sealed class RaceState : SceneStateBase, IRaceState
    {
        private readonly ActionEntryList<StateContext<RacePhase>> _phaseChangedActions = new();

        public RacePhase Phase { get; private set; } = RacePhase.Starting;

        public float GoalDistance { get; }

        public float TimeLimit { get; }

        public float Distance { get; private set; }

        public float ElapsedTime { get; private set; }

        public float Speed { get; private set; }

        /// <param name="goalDistance">目標距離（ユニット）</param>
        /// <param name="timeLimit">制限時間（秒）</param>
        public RaceState(float goalDistance, float timeLimit)
        {
            GoalDistance = goalDistance;
            TimeLimit = timeLimit;
        }

        /// <summary>
        /// すべての値をまとめて更新し、進行段階が変化したときだけ Action を実行する。
        /// Action の中から他の値を読んでも更新後の値が返るよう、すべての値を更新してから通知する
        /// </summary>
        public void Apply(RacePhase phase, float distance, float elapsedTime, float speed)
        {
            RacePhase oldPhase = Phase;

            Phase = phase;
            Distance = distance;
            ElapsedTime = elapsedTime;
            Speed = speed;

            if (oldPhase != phase)
                _phaseChangedActions.Invoke(new StateContext<RacePhase>(oldPhase, phase));
        }

        public IDisposable RegisterEventOnPhaseChanged(ActionEntry<StateContext<RacePhase>> changedAction)
        {
            return _phaseChangedActions.Register(changedAction, nameof(changedAction));
        }

        public override string GetLog()
        {
            return $"Phase : {Phase} / Distance : {Distance} / {GoalDistance} / " +
                   $"ElapsedTime : {ElapsedTime} / {TimeLimit} / Speed : {Speed}";
        }
    }
}
