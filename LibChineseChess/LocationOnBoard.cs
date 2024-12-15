namespace LibChineseChess
{
    public record struct PawnOnBoard(Pawn Pawn, Location Location);

    public record struct BoardLocation(Location Location, Pawn? Pawn);
}
