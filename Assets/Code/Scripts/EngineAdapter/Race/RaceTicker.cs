using System;
using UnityEngine;
using UsefulToolkit.Initialization;

namespace ZoukeiJam1.EngineAdapter.Race
{
    /// <summary>毎フレーム、前のフレームからの経過時間（秒）を渡して走行の更新処理を呼ぶ</summary>
    public sealed class RaceTicker : InitializableMonoBehaviour
    {
        private Action<float> _onUpdate;

        /// <param name="onUpdate">毎フレーム呼ぶ処理。引数は前のフレームからの経過時間（秒）</param>
        public void Initialize(Action<float> onUpdate)
        {
            _onUpdate = onUpdate;
            base.Initialize();
        }

        private void Update()
        {
            _onUpdate(Time.deltaTime);
        }
    }
}
