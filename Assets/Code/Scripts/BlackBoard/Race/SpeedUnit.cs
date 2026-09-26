namespace ZoukeiJam1.BlackBoard.Race
{
    /// <summary>速度の単位の変換。1 ユニット = 1 m として扱う</summary>
    public static class SpeedUnit
    {
        /// <summary>ユニット/秒を km/h に直す係数</summary>
        public const float UnitsPerSecondToKmh = 3.6f;

        /// <summary>ユニット/秒を km/h に直す</summary>
        public static float ToKmh(float unitsPerSecond)
        {
            return unitsPerSecond * UnitsPerSecondToKmh;
        }
    }
}
