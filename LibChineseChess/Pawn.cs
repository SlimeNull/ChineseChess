namespace LibChineseChess
{
    public record struct Pawn
    {
        public Camp Camp { get; set; }
        public PawnKind Kind { get; set; }

        public Pawn(Camp camp, PawnKind kind)
        {
            Camp = camp;
            Kind = kind;
        }

        public static int GetPawnWeight(PawnKind pawnKind)
        {
            return pawnKind switch
            {
                PawnKind.Chariot => 10,
                PawnKind.Horse => 5,
                PawnKind.Cannon => 8,
                PawnKind.Soldier => 1,
                PawnKind.Advisor => 3,
                PawnKind.Elephant => 3,
                PawnKind.General => 1000,
                _ => throw new ArgumentException()
            };
        }
    }
}
