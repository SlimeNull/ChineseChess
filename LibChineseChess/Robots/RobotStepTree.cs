using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibChineseChess.Robots
{
    public class RobotStepTree
    {
        public int Score { get; }
        public RobotStepTreeNode RootNode { get; }

        public RobotStepTree(Board initialBoard)
        {
            RootNode = new RobotStepTreeNode(initialBoard, GameOperation.None);
        }

        private IEnumerable<RobotStepTreeNode> EnumerateLeaves()
        {
            Queue<RobotStepTreeNode> nodeQueue = new();
            nodeQueue.Enqueue(RootNode);

            while (nodeQueue.Count > 0)
            {
                var node = nodeQueue.Dequeue();

                if (node.NextSteps.Count == 0)
                {
                    yield return node;
                }
                else
                {
                    foreach (var child in node.NextSteps)
                    {
                        nodeQueue.Enqueue(child);
                    }
                }
            }
        }

        public IEnumerable<IEnumerable<RobotStepTreeNode>> GetAllPaths()
        {
            return RootNode.GetAllPaths();
        }

        public void AppendSteps(int depth, GameOperationFactory operationFactory)
        {
            if (operationFactory is null)
            {
                throw new ArgumentNullException(nameof(operationFactory));
            }

            Queue<RobotStepTreeNode> addedNodes = new Queue<RobotStepTreeNode>();

            for (int i = 1; i <= depth; i++)
            {
                IEnumerable<RobotStepTreeNode> leaves;
                if (addedNodes.Count != 0)
                {
                    leaves = addedNodes.ToArray();
                }
                else
                {
                    leaves = EnumerateLeaves();
                }

                addedNodes.Clear();

                foreach (var leaf in leaves)
                {
                    var operations = operationFactory.Invoke(leaf.CurrentBoard, i);

                    foreach (var operation in operations)
                    {
                        var newNode = new RobotStepTreeNode(leaf.CurrentBoard, operation);
                        leaf.NextSteps.Add(newNode);
                        addedNodes.Enqueue(newNode);
                    }
                }
            }
        }
    }
}
