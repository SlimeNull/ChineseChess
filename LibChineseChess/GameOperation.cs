namespace LibChineseChess
{
    public record struct GameOperation(Location From, Location To, Pawn? TargetPawn)
    {
        public static GameOperation None { get; } = new GameOperation(Location.Invalid, Location.Invalid, null);
    }
}
