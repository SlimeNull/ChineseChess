namespace LibChineseChess.Robots
{
    public abstract class Robot
    {

        public abstract Task<GameOperation> GetStep();
    }
}