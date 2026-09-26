using UnityEngine;
using ZoukeiJam1.BlackBoard.OmikujiEngine;
using ZoukeiJam1.BlackBoard.Race;

namespace ZoukeiJam1.Application.Race
{
    /// <summary>
    /// 走行の State を生成して登録し、毎フレーム、エンジンの回転速度から速度を求めて State に書き込む。
    /// 走行中だけ、進んだ距離と経過時間を加算する
    /// </summary>
    public sealed class RaceService
    {
        private readonly RaceState _state;
        private readonly IOmikujiEngineState _engineState;

        /// <summary>エンジンが 1 回転するごとに進む距離（ユニット）</summary>
        private readonly float _distancePerRevolution;

        /// <param name="board">State の登録先</param>
        /// <param name="sceneId">State を破棄するシーンの build index</param>
        /// <param name="engineState">速度の元になるエンジンの回転速度の読み取り元</param>
        /// <param name="goalDistance">目標距離（ユニット）</param>
        /// <param name="timeLimit">制限時間（秒）</param>
        /// <param name="distancePerRevolution">エンジンが 1 回転するごとに進む距離（ユニット）</param>
        public RaceService(
            RaceBoard board, int sceneId, IOmikujiEngineState engineState,
            float goalDistance, float timeLimit, float distancePerRevolution)
        {
            _engineState = engineState;
            _distancePerRevolution = distancePerRevolution;
            _state = new RaceState(goalDistance, timeLimit);
            board.RegisterSceneState<IRaceState>(_state, sceneId);
        }

        /// <summary>開始演出が終わったときに呼び、走行を始める。開始演出中でなければ何もしない</summary>
        public void StartRunning()
        {
            if (_state.Phase != RacePhase.Starting) return;

            _state.Apply(RacePhase.Running, _state.Distance, _state.ElapsedTime, _state.Speed);
        }

        /// <summary>1 フレーム分、速度を求め、走行中なら距離と経過時間を加算する</summary>
        /// <param name="deltaTime">前のフレームからの経過時間（秒）</param>
        public void Update(float deltaTime)
        {
            float speed = Mathf.Max(0f, _engineState.RotationSpeed * _distancePerRevolution);
            float distance = _state.Distance;
            float elapsedTime = _state.ElapsedTime;

            if (_state.Phase == RacePhase.Running)
            {
                distance += speed * deltaTime;
                elapsedTime += deltaTime;
            }

            _state.Apply(_state.Phase, distance, elapsedTime, speed);
        }
    }
}
