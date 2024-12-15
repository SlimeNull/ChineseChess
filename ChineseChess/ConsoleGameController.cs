using System.Text;
using EleCho.ConsoleAnsi;
using LibChineseChess;
using LibChineseChess.Robots;

namespace ChineseChess
{
    public class ConsoleGameController
    {
        public int CurrentX { get; set; }
        public int CurrentY { get; set; }
        public int SelectedX { get; set; } = -1;
        public int SelectedY { get; set; } = -1;

        public Game Game { get; }
        public Robot? Robot { get; }
        public Camp? Winner { get; private set; }

        public ConsoleGameController(Game game, Robot? robot)
        {
            Game = game;
            Robot = robot;

            if (Game.FindPawn(Game.CurrentTurn, PawnKind.General) is PawnOnBoard pawnOnBoard)
            {
                CurrentX = pawnOnBoard.Location.X;
                CurrentY = pawnOnBoard.Location.Y;
            }
            else
            {
                Winner = Game.GetOtherCamp(Game.CurrentTurn);
            }
        }

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

        string GetEmptyChar(int x, int y)
        {
            if (x == 0)
            {
                if (y == 0)
                {
                    return "└";
                }
                else if (y == Board.Height - 1)
                {
                    return "┌";
                }
                else
                {
                    return "├";
                }
            }
            else if (x == Board.Width - 1)
            {
                if (y == 0)
                {
                    return "┘";
                }
                else if (y == Board.Height - 1)
                {
                    return "┐";
                }
                else
                {
                    return "┤";
                }
            }
            else
            {
                if (y == 0)
                {
                    return "┴";
                }
                else if (y == Board.Height - 1)
                {
                    return "┬";
                }
                else
                {
                    return "┼";
                }
            }
        }

        bool IsWindows8OrGreator()
        {
            return
                Environment.OSVersion.Version >= new Version(6, 2);
        }

        void PrintBoard()
        {
            var isWindows8OrGreator = IsWindows8OrGreator();
            var ansiSeq = new ConsoleAnsiSeq();

            ansiSeq.SetBackColor(ConsoleColor.White);
            ansiSeq.SetForeColor(ConsoleColor.Black);

            var currentForeground = ConsoleColor.Black;
            var currentBackground = ConsoleColor.White;

            var selectedLocation = new Location(SelectedX, SelectedY);
            var selectedPawn = default(Pawn?);
            var nextLocations = default(IEnumerable<Location>);

            if (Game.IsLocationAvailable(selectedLocation))
            {
                selectedPawn = Game.GetPawn(selectedLocation)!;
                nextLocations = Game.CurrentBoard.GetWalkableLocations(new PawnOnBoard(selectedPawn.Value, selectedLocation));
            }

            ansiSeq.AppendText($"中国象棋");
            ansiSeq.BaseBuilder.AppendLine();

            for (int y = Board.Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < Board.Width; x++)
                {
                    var pawn = Game.CurrentBoard.GetPawn(new Location(x, y));
                    var isCurrent = CurrentX == x && CurrentY == y;
                    var isSelected = SelectedX == x && SelectedY == y;

                    ConsoleColor newForeground = currentForeground;
                    ConsoleColor newBackground = currentBackground;

                    if (pawn.HasValue)
                    {
                        if (pawn.Value.Camp == Camp.Self)
                        {
                            newForeground = ConsoleColor.Red;
                        }
                        else
                        {
                            newForeground = ConsoleColor.Black;
                        }
                    }
                    else
                    {
                        newForeground = ConsoleColor.DarkGray;
                    }

                    if (isCurrent)
                    {
                        if (isSelected)
                        {
                            newBackground = ConsoleColor.DarkGreen;
                        }
                        else
                        {
                            if (pawn.HasValue)
                            {
                                if (pawn.Value.Camp == Camp.Self)
                                {
                                    newBackground = ConsoleColor.Black;
                                }
                                else
                                {
                                    newBackground = ConsoleColor.Red;
                                }
                            }
                            else
                            {
                                newBackground = ConsoleColor.Cyan;
                            }
                        }
                    }
                    else
                    {
                        if (isSelected)
                        {
                            newBackground = ConsoleColor.Green;
                        }
                        else
                        {
                            newBackground = ConsoleColor.White;
                        }
                    }

                    if (nextLocations is not null)
                    {
                        foreach (var step in nextLocations)
                        {
                            if (step.X == x && step.Y == y)
                            {
                                newForeground = ConsoleColor.DarkYellow;
                                break;
                            }
                        }
                    }

                    if (newForeground != currentForeground)
                    {
                        ansiSeq.SetForeColor(newForeground);
                        currentForeground = newForeground;
                    }

                    if (newBackground != currentBackground)
                    {
                        ansiSeq.SetBackColor(newBackground);
                        currentBackground = newBackground;
                    }

                    if (pawn.HasValue)
                    {
                        ansiSeq.AppendText(GetPawnChar(pawn.Value.Camp, pawn.Value.Kind));
                    }
                    else
                    {
                        ansiSeq.AppendText(GetEmptyChar(x, y));

                        if (isWindows8OrGreator)
                        {
                            ansiSeq.AppendText(" ");
                        }
                    }
                }

                ansiSeq.BaseBuilder.AppendLine();
            }

            Console.WriteLine(ansiSeq.Build());
        }

        public void ResetCurrent()
        {
            if (Game.FindPawn(Game.CurrentTurn, PawnKind.General) is PawnOnBoard pawnOnBoard)
            {
                CurrentX = pawnOnBoard.Location.X;
                CurrentY = pawnOnBoard.Location.Y;
            }
        }

        public async Task BotMoveAsync()
        {
            if (Robot is null ||
                Game.CurrentTurn != Camp.Opponent)
            {
                return;
            }

            var operation = await Robot.GetStep();
            Game.MovePawn(operation.From, operation.To);

            if (Game.FindPawn(Game.CurrentTurn, PawnKind.General) is PawnOnBoard pawnOnBoard)
            {
                CurrentX = pawnOnBoard.Location.X;
                CurrentY = pawnOnBoard.Location.Y;
            }
            else
            {
                Winner = Game.GetOtherCamp(Game.CurrentTurn);
            }
        }

        public void MoveLeft()
        {
            CurrentX = Math.Max(CurrentX - 1, 0);
        }

        public void MoveRight()
        {
            CurrentX = Math.Min(CurrentX + 1, Board.Width - 1);
        }

        public void MoveDown()
        {
            CurrentY = Math.Max(CurrentY - 1, 0);
        }

        public void MoveUp()
        {
            CurrentY = Math.Min(CurrentY + 1, Board.Height - 1);
        }

        public bool Select()
        {
            var selectedLocation = new Location(SelectedX, SelectedY);
            var currentPawn = default(Pawn?);

            if (Game.IsLocationAvailable(selectedLocation))
            {
                currentPawn = Game.GetPawn(selectedLocation);
            }

            var newSelectedLocation = new Location(CurrentX, CurrentY);
            var newPawn = default(Pawn?);

            if (Game.IsLocationAvailable(newSelectedLocation))
            {
                newPawn = Game.GetPawn(newSelectedLocation);
            }

            if (selectedLocation == newSelectedLocation)
            {
                SelectedX = -1;
                SelectedY = -1;
                return false;
            }
            else if (currentPawn.HasValue)
            {
                var walkableLocations = Game.CurrentBoard.GetWalkableLocations(new PawnOnBoard(currentPawn.Value, selectedLocation));

                if (walkableLocations.Any(loc => loc == newSelectedLocation))
                {
                    Game.MovePawn(selectedLocation, newSelectedLocation);

                    if (Game.FindPawn(Game.CurrentTurn, PawnKind.General) is PawnOnBoard pawn)
                    {
                        CurrentX = pawn.Location.X;
                        CurrentY = pawn.Location.Y;
                        SelectedX = -1;
                        SelectedY = -1;
                        return true;
                    }
                    else
                    {
                        SelectedX = -1;
                        SelectedY = -1;
                        Winner = Game.GetOtherCamp(Game.CurrentTurn);
                        return true;
                    }
                }

                return false;
            }
            else if (newPawn.HasValue && Game.CurrentTurn == newPawn.Value.Camp)
            {
                SelectedX = CurrentX;
                SelectedY = CurrentY;
                return false;
            }
            else
            {
                return false;
            }
        }

        public void PrintToConsole()
        {
            PrintBoard();
        }
    }
}
