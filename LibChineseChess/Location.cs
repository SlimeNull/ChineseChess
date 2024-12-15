namespace LibChineseChess
{
    public record struct Location(int X, int Y)
    {
        public static Location Invalid = new Location(-1, -1);
    }
}
