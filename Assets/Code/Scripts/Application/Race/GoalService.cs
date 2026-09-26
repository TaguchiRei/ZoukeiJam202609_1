using System;
using Cysharp.Threading.Tasks;
using UsefulToolkit.BlackBoard.BlackBoard;
using UsefulToolkit.BlackBoard.Scene;
using ZoukeiJam1.BlackBoard.Race;

namespace ZoukeiJam1.Application.Race
{
    /// <summary>
    /// 走行の進行段階がゴールになったら、ゴールタイム・制限時間・ゴール時の速度から結果を求めて記録し、リザルトシーンへ上書きロードする
    /// </summary>
    public sealed class GoalService
    {
        private readonly IRaceState _raceState;
        private readonly GoalResultCalculator _calculator;
        private readonly IGameResultRecorder _recorder;
        private readonly ISceneState _sceneState;
        private readonly int _resultSceneId;

        /// <param name="raceState">ゴールの検知と、ゴールタイム・制限時間・ゴール時の速度の読み取り元</param>
        /// <param name="calculator">ゴール時の結果の計算</param>
        /// <param name="recorder">結果の記録先</param>
        /// <param name="sceneState">リザルトシーンへの遷移の要求先</param>
        /// <param name="resultSceneId">リザルトシーンの build index</param>
        public GoalService(
            IRaceState raceState, GoalResultCalculator calculator, IGameResultRecorder recorder,
            ISceneState sceneState, int resultSceneId)
        {
            _raceState = raceState;
            _calculator = calculator;
            _recorder = recorder;
            _sceneState = sceneState;
            _resultSceneId = resultSceneId;

            _raceState.RegisterEventOnPhaseChanged(
                new ActionEntry<StateContext<RacePhase>>(false, OnPhaseChanged));
        }

        private void OnPhaseChanged(StateContext<RacePhase> context)
        {
            if (context.NewValue != RacePhase.Goal) return;

            var result = _calculator.Calculate(
                _raceState.GoalDistance, _raceState.ElapsedTime, _raceState.TimeLimit, _raceState.Speed);
            _recorder.Record(result);
            _sceneState.RequestOverwriteLoadAsync(_resultSceneId, Array.Empty<int>()).Forget();
        }
    }
}
