
using System;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace LibChineseChess
{
    public class Game
    {
        private readonly List<GameOperation> _history = new();

        private int _stateOffset = 0;
        private Board _currentBoard;

        public Board CurrentBoard => _currentBoard; 
        public Camp CurrentTurn { get; private set; }
        public int MaxHistory { get; set; } = 10000;

        public Game(Board board)
        {
            _currentBoard = board;
        }

        public Game() : this(Board.CreateNew())
        {

        }

        private void SwitchTurn()
        {
            CurrentTurn = CurrentTurn switch
            {
                Camp.Self => Camp.Opponent,
                Camp.Opponent => Camp.Self,
                _ => throw new InvalidOperationException()
            };
        }

        public Board GetHistory(int distance)
        {
            if (distance < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            var board = CurrentBoard;

            while (distance > 0)
            {
                var operation = _history[_history.Count - distance];
                var operatedPawn = board.GetPawn(operation.To);
                var targetPawn = operation.TargetPawn;

                board.SetPawn(operation.From, operatedPawn);
                board.SetPawn(operation.To, targetPawn);
            }

            return board;
        }

        public void Undo(int count)
        {
            if (count <= 0 ||
                _stateOffset + count > _history.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            for (int i = 0; i < count; i++)
            {
                var operation = _history[_history.Count - _stateOffset - 1 - i];
                var operatedPawn = _currentBoard.GetPawn(operation.To);
                var targetPawn = operation.TargetPawn;

                _currentBoard.SetPawn(operation.From, operatedPawn);
                _currentBoard.SetPawn(operation.To, targetPawn);
                _stateOffset++;

                SwitchTurn();
            }
        }

        public void Redo(int count)
        {
            if (count <= 0 ||
                count - _stateOffset > 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            for (int i = 0; i < count; i++)
            {
                var operation = _history[_history.Count - _stateOffset + i];

                _currentBoard.MovePawn(operation.From, operation.To);
                _stateOffset--;

                SwitchTurn();
            }
        }

        public void Undo() => Undo(1);
        public void Redo() => Redo(1);

        public void MovePawn(Location from, Location to)
        {
            var targetPawn = GetPawn(to);

            while (_stateOffset > 0)
            {
                _history.RemoveAt(_history.Count - 1);
            }

            _history.Add(new GameOperation(from, to, targetPawn));
            _currentBoard.MovePawn(from, to);

            SwitchTurn();
        }

        public static Camp GetOtherCamp(Camp camp)
        {
            return camp switch
            {
                Camp.Self => Camp.Opponent,
                Camp.Opponent => Camp.Self,
                _ => throw new ArgumentException()
            };
        }

        public bool IsLocationAvailable(Location location)
        {
            return
                location.X >= 0 && location.X < Board.Width &&
                location.Y >= 0 && location.Y < Board.Height;
        }

        public bool IsLocationEmpty(Location location)
        {
            return IsLocationAvailable(location) && GetPawn(location) == null;
        }

        public bool IsLocationSpecifiedCamp(Location location, Camp camp)
        {
            if (!IsLocationAvailable(location))
            {
                return false;
            }

            var pawn = GetPawn(location);
            return pawn.HasValue && pawn.Value.Camp == camp;
        }

        public bool IsLocationDifferentCamp(Location location, Camp camp)
        {
            if (!IsLocationAvailable(location))
            {
                return false;
            }

            var pawn = GetPawn(location);
            return pawn.HasValue && pawn.Value.Camp != camp;
        }

        public bool IsLocationAlly(Location location)
            => IsLocationSpecifiedCamp(location, Camp.Self);

        public bool IsLocationEnemy(Location location)
            => IsLocationSpecifiedCamp(location, Camp.Opponent);

        public Pawn? GetPawn(Location location)
        {
            return CurrentBoard.GetPawn(location);
        }

        public PawnOnBoard? FindPawn(Camp camp, PawnKind pawnKind)
        {
            var enumerator = CurrentBoard
                .EnumerateAllPawns()
                .Where(pawn => pawn.Pawn.Camp == camp && pawn.Pawn.Kind == pawnKind)
                .GetEnumerator();

            if (!enumerator.MoveNext())
            {
                return null;
            }

            return enumerator.Current;
        }
    }

}
