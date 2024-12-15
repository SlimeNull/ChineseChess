using System.Text;

namespace LibChineseChess.Robots
{
    internal static class UcciUtilities
    {
        public static char GetFenChar(Pawn pawn)
        {
            char c = pawn.Kind switch
            {
                PawnKind.Chariot => 'r',
                PawnKind.Horse => 'n',
                PawnKind.Elephant => 'b',
                PawnKind.Advisor => 'a',
                PawnKind.General => 'k',
                PawnKind.Cannon => 'c',
                PawnKind.Soldier => 'p',
                _ => throw new ArgumentException()
            };

            if (pawn.Camp == Camp.Self)
            {
                c = char.ToUpper(c);
            }

            return c;
        }

        public static string GenerateFenString(Board board, Camp currentCamp)
        {
            StringBuilder sb = new StringBuilder();
            int spaceCount = 0;

            for (int y = Board.Height - 1; y >= 0; y--)
            {
                if (spaceCount != 0)
                {
                    sb.Append(spaceCount);
                    spaceCount = 0;
                }

                if (sb.Length != 0)
                {
                    sb.Append('/');
                }

                for (int x = 0; x < Board.Width; x++)
                {
                    var pawn = board.GetPawn(new Location(x, y));

                    if (pawn != null)
                    {
                        if (spaceCount != 0)
                        {
                            sb.Append(spaceCount);
                            spaceCount = 0;
                        }

                        sb.Append(GetFenChar(pawn.Value));
                    }
                    else
                    {
                        spaceCount++;
                    }
                }
            }

            if (currentCamp == Camp.Self)
            {
                sb.Append(" w - - 0 1");
            }
            else if (currentCamp == Camp.Opponent)
            {
                sb.Append(" b - - 0 1");
            }
            else
            {
                throw new ArgumentException("Invalid Camp", nameof(currentCamp));
            }

            return sb.ToString();
        }
    }
}
