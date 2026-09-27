namespace ZoukeiJam1.BlackBoard.Race
{
    /// <summary>おみくじの結果の表示名</summary>
    public static class OmikujiFortuneNames
    {
        /// <summary>おみくじの結果を「大吉」などの表示名にする</summary>
        public static string ToDisplayName(this OmikujiFortune fortune)
        {
            return fortune switch
            {
                OmikujiFortune.Daikichi => "大吉",
                OmikujiFortune.Chukichi => "中吉",
                OmikujiFortune.Shokichi => "小吉",
                OmikujiFortune.Kichi => "吉",
                OmikujiFortune.Kyo => "凶",
                _ => fortune.ToString(),
            };
        }
    }
}
