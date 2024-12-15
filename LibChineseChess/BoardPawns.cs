using System.Runtime.CompilerServices;

namespace LibChineseChess
{
    [InlineArray(Width * Height)]
    internal struct BoardPawns : IEquatable<BoardPawns>
    {
        const int Width = 9;
        const int Height = 10;

        public Pawn? First;

        public bool Equals(BoardPawns other)
        {
            for (var i = 0; i < 9 * 10; i++)
            {
                if (this[i] != other[i])
                {
                    return false;
                }
            }

            return true;
        }

        public Pawn? GetPawn(Location location)
        {
            if (location.X < 0 || location.X >= Width ||
                location.Y < 0 || location.Y >= Height)
            {
                throw new IndexOutOfRangeException();
            }

            return this[location.Y * Width + location.X];
        }

        public void SetPawn(Location location, Pawn? pawn)
        {
            if (location.X < 0 || location.X >= Width ||
                location.Y < 0 || location.Y >= Height)
            {
                throw new IndexOutOfRangeException();
            }

            this[location.Y * Width + location.X] = pawn;
        }
    }
}
