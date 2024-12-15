namespace LibChineseChess.Robots
{
    public class ErgodicRobot : Robot
    {
        public Game Game { get; }
        public Camp Camp { get; }
        public int DefaultDepth { get; }

        public ErgodicRobot(Game game, Camp camp, int defaultDepth)
        {
            if (defaultDepth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(defaultDepth));
            }

            Game = game;
            Camp = camp;
            DefaultDepth = defaultDepth;
        }

        private IEnumerable<GameOperation> GenerateGameOperations(Board board, int currentDepth)
        {
            var initCamp = Game.CurrentTurn;

            var operateCamp = initCamp;
            for (int i = 1; i < currentDepth; i++)
            {
                operateCamp = Game.GetOtherCamp(operateCamp);
            }

            foreach (var pawnOnBoard in board.EnumerateAllPawns(operateCamp))
            {
                foreach (var walkableLocation in board.GetWalkableLocations(pawnOnBoard))
                {
                    yield return new GameOperation(pawnOnBoard.Location, walkableLocation, board.GetPawn(walkableLocation));
                }
            }
        }

        private int GetPathWeight(IList<RobotStepTreeNode> path)
        {
            if (path.Count == 0)
            {
                return int.MinValue;
            }

            path[0].GetFutureMinMaxScoreOffset(Camp, path.Count - 1, out var minOffset, out var maxOffset);

            return minOffset + maxOffset;
        }

        public RobotStepTree GetStepTree(int depth)
        {
            var tree = new RobotStepTree(Game.CurrentBoard);
            tree.AppendSteps(depth, GenerateGameOperations);

            return tree;
        }

        public Task<GameOperation> GetStep(int depth)
        {
            if (depth <= 0)
            {
                throw new ArgumentException();
            }

            if (Game.CurrentTurn != Camp)
            {
                throw new InvalidOperationException();
            }

            var tree = GetStepTree(depth);
            var allPaths = tree.GetAllPaths().Select(path => path.ToArray()).ToArray();
            var bestPath = allPaths.MaxBy(path => GetPathWeight(path))!;

            return Task.FromResult(bestPath[0].Operation);
        }

        public override Task<GameOperation> GetStep()
            => GetStep(DefaultDepth);
    }
}
