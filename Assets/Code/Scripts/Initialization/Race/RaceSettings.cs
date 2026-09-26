using UnityEngine;

namespace ZoukeiJam1.Initialization.Race
{
    /// <summary>走行のルールの設定値（目標距離・制限時間・1 回転で進む距離）</summary>
    [CreateAssetMenu(fileName = "RaceSettings", menuName = "ZoukeiJam1/RaceSettings")]
    public sealed class RaceSettings : ScriptableObject
    {
        [Tooltip("目標距離（m）")]
        [SerializeField, Min(0.01f)] private float _goalDistance = 300f;

        [Tooltip("制限時間（秒）。開始演出が終わってから数える")]
        [SerializeField, Min(0.01f)] private float _timeLimit = 30f;

        [Tooltip("エンジンが 1 回転するごとに進む距離（m）。速度はエンジンの回転速度（回転/秒）にこの値を掛けたものになる")]
        [SerializeField, Min(0f)] private float _distancePerRevolution = 5f;

        /// <summary>目標距離（m）</summary>
        public float GoalDistance => _goalDistance;

        /// <summary>制限時間（秒）</summary>
        public float TimeLimit => _timeLimit;

        /// <summary>エンジンが 1 回転するごとに進む距離（m）</summary>
        public float DistancePerRevolution => _distancePerRevolution;
    }
}
