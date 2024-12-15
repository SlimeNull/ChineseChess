using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EleCho.WpfSuite.ValueConverters;
using LibChineseChess;

namespace ManboChineseChess.ValueConverters
{
    public class PawnOnWpfToDisplayTextConverter : SingletonValueConverterBase<PawnOnWpfToDisplayTextConverter, PawnOnWpf, string>
    {
        string GetPawnChar(Camp camp, PawnKind pawnKind)
        {
            if (camp == Camp.Self)
            {
                return pawnKind switch
                {
                    PawnKind.Chariot => "俥",
                    PawnKind.Horse => "傌",
                    PawnKind.Cannon => "炮",
                    PawnKind.Soldier => "兵",
                    PawnKind.Advisor => "仕",
                    PawnKind.Elephant => "相",
                    PawnKind.General => "帥",
                    _ => throw new ArgumentException("Invlaid PawnKind", nameof(pawnKind))
                };
            }
            else if (camp == Camp.Opponent)
            {
                return pawnKind switch
                {
                    PawnKind.Chariot => "車",
                    PawnKind.Horse => "馬",
                    PawnKind.Cannon => "砲",
                    PawnKind.Soldier => "卒",
                    PawnKind.Advisor => "士",
                    PawnKind.Elephant => "象",
                    PawnKind.General => "將",
                    _ => throw new ArgumentException("Invlaid PawnKind", nameof(pawnKind))
                };
            }
            else
            {
                throw new ArgumentException("Invalid Camp", nameof(camp));
            }
        }

        public override string? Convert(PawnOnWpf value, Type targetType, object? parameter, CultureInfo culture)
        {
            var pawn = value.Pawn;

            return GetPawnChar(pawn.Camp, pawn.Kind);
        }
    }
}
