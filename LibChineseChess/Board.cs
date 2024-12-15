using System.Diagnostics.CodeAnalysis;

namespace LibChineseChess
{
    public unsafe record struct Board
    {
        public const int Width = 9;
        public const int Height = 10;

        private int _selfScore;
        private int _opponentScore;
        private BoardPawns _boardPawns;

        public int SelfScore => _selfScore;
        public int OpponentScore => _opponentScore;

        public int GetScore(Camp camp)
        {
            return camp switch
            {
                Camp.Self => SelfScore,
                Camp.Opponent => OpponentScore,
                _ => throw new ArgumentException(),
            };
        }

        public Pawn? GetPawn(Location location)
        {
            return _boardPawns.GetPawn(location);
        }

        public void SetPawn(Location location, Pawn? pawn)
        {
            var originPawn = GetPawn(location);

            if (originPawn.HasValue)
            {
                var weight = Pawn.GetPawnWeight(originPawn.Value.Kind);

                if (originPawn.Value.Camp == Camp.Self)
                {
                    _selfScore -= weight;
                    _opponentScore += weight;
                }
                else if (originPawn.Value.Camp == Camp.Opponent)
                {
                    _selfScore += weight;
                    _opponentScore -= weight;
                }
            }

            _boardPawns.SetPawn(location, pawn);

            if (pawn.HasValue)
            {
                var weight = Pawn.GetPawnWeight(pawn.Value.Kind);

                if (pawn.Value.Camp == Camp.Self)
                {
                    _selfScore += weight;
                    _opponentScore -= weight;
                }
                else if (pawn.Value.Camp == Camp.Opponent)
                {
                    _selfScore -= weight;
                    _opponentScore += weight;
                }
            }
        }

        public void MovePawn(Location from, Location to)
        {
            var pawn = GetPawn(from);
            if (!pawn.HasValue)
            {
                return;
            }

            SetPawn(from, null);
            SetPawn(to, pawn);
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

        public IEnumerable<PawnOnBoard> EnumerateAllPawns()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var pawn = _boardPawns.GetPawn(new(x, y));

                    if (pawn.HasValue)
                    {
                        yield return new PawnOnBoard(pawn.Value, new(x, y));
                    }
                }
            }
        }

        public IEnumerable<PawnOnBoard> EnumerateAllPawns(Camp camp)
            => EnumerateAllPawns().Where(pawnOnBoard => pawnOnBoard.Pawn.Camp == camp);

        public IEnumerable<PawnOnBoard> EnumerateSelfPawns()
            => EnumerateAllPawns().Where(pawnOnBoard => pawnOnBoard.Pawn.Camp == Camp.Self);

        public IEnumerable<PawnOnBoard> EnumerateOpponentPawns()
            => EnumerateAllPawns().Where(pawnOnBoard => pawnOnBoard.Pawn.Camp == Camp.Opponent);

        public IEnumerable<BoardLocation> EnumerateLeft(Location location)
        {
            if (location.X < 0 || location.X >= Width ||
                location.Y < 0 || location.Y >= Height)
                throw new IndexOutOfRangeException();

            for (int x = location.X - 1; x >= 0; x--)
            {
                var currentLocation = new Location(x, location.Y);
                yield return new BoardLocation(currentLocation, _boardPawns.GetPawn(currentLocation));
            }
        }

        public IEnumerable<BoardLocation> EnumerateRight(Location location)
        {
            if (location.X < 0 || location.X >= Width ||
                location.Y < 0 || location.Y >= Height)
                throw new IndexOutOfRangeException();

            for (int x = location.X + 1; x < Width; x++)
            {
                var currentLocation = new Location(x, location.Y);
                yield return new BoardLocation(currentLocation, _boardPawns.GetPawn(currentLocation));
            }
        }

        public IEnumerable<BoardLocation> EnumerateUp(Location location)
        {
            if (location.X < 0 || location.X >= Width ||
                location.Y < 0 || location.Y >= Height)
                throw new IndexOutOfRangeException();

            for (int y = location.Y + 1; y < Board.Height; y++)
            {
                var currentLocation = new Location(location.X, y);
                yield return new BoardLocation(currentLocation, _boardPawns.GetPawn(currentLocation));
            }
        }

        public IEnumerable<BoardLocation> EnumerateDown(Location location)
        {
            if (location.X < 0 || location.X >= Width ||
                location.Y < 0 || location.Y >= Height)
                throw new IndexOutOfRangeException();

            for (int y = location.Y - 1; y >= 0; y--)
            {
                var currentLocation = new Location(location.X, y);
                yield return new BoardLocation(currentLocation, _boardPawns.GetPawn(currentLocation));
            }
        }



        private IEnumerable<Location> GetChariotWalkableLocations(Location location, Camp camp)
        {
            var directions = (IEnumerable<BoardLocation>[])[
                EnumerateLeft(location),
                EnumerateRight(location),
                EnumerateUp(location),
                EnumerateDown(location)
            ];

            foreach (var direction in directions)
            {
                foreach (var loc in direction)
                {
                    if (!loc.Pawn.HasValue ||
                        loc.Pawn.Value.Camp != camp)
                    {
                        yield return loc.Location;
                    }

                    if (loc.Pawn.HasValue)
                    {
                        break;
                    }
                }
            }
        }

        private IEnumerable<Location> GetHorseWalkableLocations(Location location, Camp camp)
        {
            if (IsLocationEmpty(new Location(location.X - 1, location.Y)))
            {
                var leftStep1 = new Location(location.X - 2, location.Y - 1);
                var leftStep2 = new Location(location.X - 2, location.Y + 1);

                if (IsLocationEmpty(leftStep1) ||
                    IsLocationDifferentCamp(leftStep1, camp))
                {
                    yield return leftStep1;
                }

                if (IsLocationEmpty(leftStep2) ||
                    IsLocationDifferentCamp(leftStep2, camp))
                {
                    yield return leftStep2;
                }
            }

            if (IsLocationEmpty(new Location(location.X + 1, location.Y)))
            {
                var rightStep1 = new Location(location.X + 2, location.Y - 1);
                var rightStep2 = new Location(location.X + 2, location.Y + 1);

                if (IsLocationEmpty(rightStep1) ||
                    IsLocationDifferentCamp(rightStep1, camp))
                {
                    yield return rightStep1;
                }


                if (IsLocationEmpty(rightStep2) ||
                    IsLocationDifferentCamp(rightStep2, camp))
                {
                    yield return rightStep2;
                }
            }

            if (IsLocationEmpty(new Location(location.X, location.Y + 1)))
            {
                var upStep1 = new Location(location.X - 1, location.Y + 2);
                var upStep2 = new Location(location.X + 1, location.Y + 2);

                if (IsLocationEmpty(upStep1) ||
                    IsLocationDifferentCamp(upStep1, camp))
                {
                    yield return upStep1;
                }

                if (IsLocationEmpty(upStep2) ||
                    IsLocationDifferentCamp(upStep2, camp))
                {
                    yield return upStep2;
                }
            }

            if (IsLocationEmpty(new Location(location.X, location.Y - 1)))
            {
                var downStep1 = new Location(location.X - 1, location.Y - 2);
                var downStep2 = new Location(location.X + 1, location.Y - 2);

                if (IsLocationAvailable(downStep1))
                {
                    yield return downStep1;
                }

                if (IsLocationAvailable(downStep2))
                {
                    yield return downStep2;
                }
            }
        }

        private IEnumerable<Location> GetCannonWalkableLocations(Location location, Camp camp)
        {
            var directions = (IEnumerable<BoardLocation>[])[
                EnumerateLeft(location),
                EnumerateRight(location),
                EnumerateUp(location),
                EnumerateDown(location)
            ];

            foreach (var direction in directions)
            {
                bool metEnemy = false;
                foreach (var loc in direction)
                {
                    if (!metEnemy)
                    {
                        if (IsLocationEmpty(loc.Location))
                        {
                            yield return loc.Location;
                        }
                        else
                        {
                            metEnemy = true;
                        }
                    }
                    else
                    {
                        var pawn = GetPawn(loc.Location);

                        if (pawn.HasValue)
                        {
                            if (pawn.Value.Camp != camp)
                            {
                                yield return loc.Location;
                            }

                            break;
                        }
                    }
                }
            }
        }

        private IEnumerable<Location> GetSoldierWalkableLocations(Location location, Camp camp)
        {
            if (camp == Camp.Self)
            {
                var left = new Location(location.X - 1, location.Y);
                var right = new Location(location.X + 1, location.Y);
                var up = new Location(location.X, location.Y + 1);

                if (IsLocationEmpty(up) ||
                    IsLocationDifferentCamp(up, camp))
                {
                    yield return up;
                }

                if (location.Y > 4)
                {
                    if (IsLocationAvailable(left) ||
                        IsLocationDifferentCamp(left, camp))
                    {
                        yield return left;
                    }

                    if (IsLocationAvailable(right) ||
                        IsLocationDifferentCamp(right, camp))
                    {
                        yield return right;
                    }
                }
            }
            else if (camp == Camp.Opponent)
            {
                var left = new Location(location.X - 1, location.Y);
                var right = new Location(location.X + 1, location.Y);
                var down = new Location(location.X, location.Y - 1);

                if (IsLocationEmpty(down) ||
                    IsLocationDifferentCamp(down, camp))
                {
                    yield return down;
                }

                if (location.Y < 5)
                {
                    if (IsLocationAvailable(left) ||
                        IsLocationDifferentCamp(left, camp))
                    {
                        yield return left;
                    }

                    if (IsLocationAvailable(right) ||
                        IsLocationDifferentCamp(right, camp))
                    {
                        yield return right;
                    }
                }
            }
            else
            {
                throw new ArgumentException();
            }
        }

        private IEnumerable<Location> GetAdvisorWalkableLocations(Location location, Camp camp)
        {
            var leftUp = new Location(location.X - 1, location.Y + 1);
            var rightUp = new Location(location.X + 1, location.Y + 1);
            var leftDown = new Location(location.X - 1, location.Y - 1);
            var rightDown = new Location(location.X + 1, location.Y - 1);
            var steps = (Location[])[leftUp, rightUp, leftDown, rightDown];
            (int Min, int Max) stepRangeX;
            (int Min, int Max) stepRangeY;

            if (camp == Camp.Self)
            {
                stepRangeX = (3, 5);
                stepRangeY = (0, 2);
            }
            else if (camp == Camp.Opponent)
            {
                stepRangeX = (3, 5);
                stepRangeY = (7, 9);
            }
            else
            {
                throw new ArgumentException();
            }

            foreach (var step in steps)
            {
                if (step.X < stepRangeX.Min || step.X > stepRangeX.Max ||
                    step.Y < stepRangeY.Min || step.Y > stepRangeY.Max)
                {
                    continue;
                }

                if (IsLocationEmpty(step) ||
                    IsLocationDifferentCamp(step, camp))
                {
                    yield return step;
                }
            }
        }

        private IEnumerable<Location> GetElephantWalkableLocations(Location location, Camp camp)
        {
            var obstacleAndTargetSteps = ((Location Obstacle, Location TargetStep)[])[
                (new Location(location.X - 1, location.Y - 1), new Location(location.X - 2, location.Y - 2)),
                (new Location(location.X + 1, location.Y - 1), new Location(location.X + 2, location.Y - 2)),
                (new Location(location.X - 1, location.Y + 1), new Location(location.X - 2, location.Y + 2)),
                (new Location(location.X + 1, location.Y + 1), new Location(location.X + 2, location.Y + 2)),
            ];

            foreach (var obstacleAndTargetStep in obstacleAndTargetSteps)
            {
                if (!IsLocationEmpty(obstacleAndTargetStep.Obstacle))
                {
                    continue;
                }

                if (IsLocationEmpty(obstacleAndTargetStep.TargetStep) ||
                    IsLocationDifferentCamp(obstacleAndTargetStep.TargetStep, camp))
                {
                    yield return obstacleAndTargetStep.TargetStep;
                }
            }
        }

        private IEnumerable<Location> GetGeneralWalkableLocations(Location location, Camp camp)
        {
            var steps = (Location[])[
                new Location(location.X - 1, location.Y),
                new Location(location.X + 1, location.Y),
                new Location(location.X, location.Y - 1),
                new Location(location.X, location.Y + 1),
            ];

            (int Min, int Max) stepRangeX;
            (int Min, int Max) stepRangeY;

            if (camp == Camp.Self)
            {
                stepRangeX = (3, 5);
                stepRangeY = (0, 2);
            }
            else if (camp == Camp.Opponent)
            {
                stepRangeX = (3, 5);
                stepRangeY = (7, 9);
            }
            else
            {
                throw new ArgumentException();
            }

            foreach (var step in steps)
            {
                if (step.X < stepRangeX.Min || step.X > stepRangeX.Max ||
                    step.Y < stepRangeY.Min || step.Y > stepRangeY.Max)
                {
                    continue;
                }

                if (IsLocationEmpty(step) ||
                    IsLocationDifferentCamp(step, camp))
                {
                    yield return step;
                }
            }
        }

        public IEnumerable<Location> GetWalkableLocations(PawnOnBoard pawnOnBoard)
        {
            return pawnOnBoard.Pawn.Kind switch
            {
                PawnKind.Chariot => GetChariotWalkableLocations(pawnOnBoard.Location, pawnOnBoard.Pawn.Camp),
                PawnKind.Horse => GetHorseWalkableLocations(pawnOnBoard.Location, pawnOnBoard.Pawn.Camp),
                PawnKind.Cannon => GetCannonWalkableLocations(pawnOnBoard.Location, pawnOnBoard.Pawn.Camp),
                PawnKind.Soldier => GetSoldierWalkableLocations(pawnOnBoard.Location, pawnOnBoard.Pawn.Camp),
                PawnKind.Advisor => GetAdvisorWalkableLocations(pawnOnBoard.Location, pawnOnBoard.Pawn.Camp),
                PawnKind.Elephant => GetElephantWalkableLocations(pawnOnBoard.Location, pawnOnBoard.Pawn.Camp),
                PawnKind.General => GetGeneralWalkableLocations(pawnOnBoard.Location, pawnOnBoard.Pawn.Camp),
                _ => throw new ArgumentException()
            };
        }

        public static Board CreateNew()
        {
            var board = new Board();

            board.SetPawn(new Location(0, 0), new Pawn(Camp.Self, PawnKind.Chariot));
            board.SetPawn(new Location(1, 0), new Pawn(Camp.Self, PawnKind.Horse));
            board.SetPawn(new Location(2, 0), new Pawn(Camp.Self, PawnKind.Elephant));
            board.SetPawn(new Location(3, 0), new Pawn(Camp.Self, PawnKind.Advisor));
            board.SetPawn(new Location(4, 0), new Pawn(Camp.Self, PawnKind.General));
            board.SetPawn(new Location(5, 0), new Pawn(Camp.Self, PawnKind.Advisor));
            board.SetPawn(new Location(6, 0), new Pawn(Camp.Self, PawnKind.Elephant));
            board.SetPawn(new Location(7, 0), new Pawn(Camp.Self, PawnKind.Horse));
            board.SetPawn(new Location(8, 0), new Pawn(Camp.Self, PawnKind.Chariot));

            board.SetPawn(new Location(1, 2), new Pawn(Camp.Self, PawnKind.Cannon));
            board.SetPawn(new Location(7, 2), new Pawn(Camp.Self, PawnKind.Cannon));

            board.SetPawn(new Location(0, 3), new Pawn(Camp.Self, PawnKind.Soldier));
            board.SetPawn(new Location(2, 3), new Pawn(Camp.Self, PawnKind.Soldier));
            board.SetPawn(new Location(4, 3), new Pawn(Camp.Self, PawnKind.Soldier));
            board.SetPawn(new Location(6, 3), new Pawn(Camp.Self, PawnKind.Soldier));
            board.SetPawn(new Location(8, 3), new Pawn(Camp.Self, PawnKind.Soldier));


            board.SetPawn(new Location(0, 9), new Pawn(Camp.Opponent, PawnKind.Chariot));
            board.SetPawn(new Location(1, 9), new Pawn(Camp.Opponent, PawnKind.Horse));
            board.SetPawn(new Location(2, 9), new Pawn(Camp.Opponent, PawnKind.Elephant));
            board.SetPawn(new Location(3, 9), new Pawn(Camp.Opponent, PawnKind.Advisor));
            board.SetPawn(new Location(4, 9), new Pawn(Camp.Opponent, PawnKind.General));
            board.SetPawn(new Location(5, 9), new Pawn(Camp.Opponent, PawnKind.Advisor));
            board.SetPawn(new Location(6, 9), new Pawn(Camp.Opponent, PawnKind.Elephant));
            board.SetPawn(new Location(7, 9), new Pawn(Camp.Opponent, PawnKind.Horse));
            board.SetPawn(new Location(8, 9), new Pawn(Camp.Opponent, PawnKind.Chariot));

            board.SetPawn(new Location(1, 7), new Pawn(Camp.Opponent, PawnKind.Cannon));
            board.SetPawn(new Location(7, 7), new Pawn(Camp.Opponent, PawnKind.Cannon));

            board.SetPawn(new Location(0, 6), new Pawn(Camp.Opponent, PawnKind.Soldier));
            board.SetPawn(new Location(2, 6), new Pawn(Camp.Opponent, PawnKind.Soldier));
            board.SetPawn(new Location(4, 6), new Pawn(Camp.Opponent, PawnKind.Soldier));
            board.SetPawn(new Location(6, 6), new Pawn(Camp.Opponent, PawnKind.Soldier));
            board.SetPawn(new Location(8, 6), new Pawn(Camp.Opponent, PawnKind.Soldier));

            return board;
        }
    }
}
