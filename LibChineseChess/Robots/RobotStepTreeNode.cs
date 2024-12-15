namespace LibChineseChess.Robots
{
    public class RobotStepTreeNode
    {
        public Board InitialBoard { get; }
        public Board CurrentBoard { get; }
        public GameOperation Operation { get; }
        public List<RobotStepTreeNode> NextSteps { get; } = new();

        public IEnumerable<IEnumerable<RobotStepTreeNode>> GetAllPaths()
        {
            var isNonOperation = Operation == GameOperation.None;

            if (NextSteps.Count == 0)
            {
                yield return new RobotStepTreeNode[1] { this };
            }
            else
            {
                foreach (var node in NextSteps)
                {
                    foreach (var nodePath in node.GetAllPaths())
                    {
                        var path = nodePath;

                        if (!isNonOperation)
                        {
                            path = path.Prepend(this);
                        }

                        yield return path;
                    }
                }
            }
        }

        public RobotStepTreeNode(Board initialBoard, GameOperation gameOperation)
        {
            InitialBoard = initialBoard;

            if (gameOperation != GameOperation.None)
            {
                initialBoard.MovePawn(gameOperation.From, gameOperation.To);
            }

            CurrentBoard = initialBoard;
            Operation = gameOperation;
        }

        public void GetFutureMinMaxScoreOffset(Camp camp, int depth, out int minOffset, out int maxOffset)
        {
            int currentScore = CurrentBoard.GetScore(camp);
            minOffset = 0;
            maxOffset = 0;

            List<RobotStepTreeNode> pendingTreeNodes = new List<RobotStepTreeNode>();
            foreach (var child in NextSteps)
            {
                pendingTreeNodes.Add(child);
            }

            for (int i = 0; i < depth; i++)
            {
                List<RobotStepTreeNode> newNodes = new List<RobotStepTreeNode>();

                foreach (var node in pendingTreeNodes)
                {
                    var currentNodeScore = node.CurrentBoard.GetScore(camp);
                    var scoreOffset = currentNodeScore - currentScore;

                    minOffset = Math.Min(minOffset, scoreOffset);
                    maxOffset = Math.Max(maxOffset, scoreOffset);

                    foreach (var childNode in pendingTreeNodes)
                    {
                        newNodes.Add(childNode);
                    }
                }

                pendingTreeNodes = newNodes;
            }
        }
    }
}
