namespace ZoukeiJam1.BlackBoard.OmikujiEngine
{
    /// <summary>ピストンの行程。直前に通過した死点で決まり、逆回転しても変わらない</summary>
    public enum PistonStroke
    {
        /// <summary>下死点を通過してから上死点に着くまで</summary>
        Up,

        /// <summary>上死点を通過してから下死点に着くまで</summary>
        Down,
    }
}
