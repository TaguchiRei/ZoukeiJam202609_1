using UsefulToolkit.BlackBoard.BlackBoard;
using ZoukeiJam1.BlackBoard.Race;

namespace ZoukeiJam1.Application.Race
{
    /// <summary>
    /// 走行の進行段階がゴールになったら、ゴールタイム・ゴール時の速度から結果を求めて記録する
    /// </summary>
    public sealed class GoalService
    {
        private readonly IRaceState _raceState;
        private readonly GoalResultCalculator _calculator;
        private readonly IGameResultRecorder _recorder;

        /// <param name="raceState">ゴールの検知と、ゴールタイム・ゴール時の速度の読み取り元</param>
        /// <param name="calculator">ゴール時の結果の計算</param>
        /// <param name="recorder">結果の記録先</param>
        public GoalService(IRaceState raceState, GoalResultCalculator calculator, IGameResultRecorder recorder)
        {
            _raceState = raceState;
            _calculator = calculator;
            _recorder = recorder;

            _raceState.RegisterEventOnPhaseChanged(
                new ActionEntry<StateContext<RacePhase>>(false, OnPhaseChanged));
        }

        private void OnPhaseChanged(StateContext<RacePhase> context)
        {
            if (context.NewValue != RacePhase.Goal) return;

            var result = _calculator.Calculate(
                _raceState.GoalDistance, _raceState.ElapsedTime, _raceState.Speed);
            _recorder.Record(result);
        }
    }
}
