using LibChineseChess;
using LibChineseChess.Robots;

namespace ChineseChess
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Game game = new Game();
            ConsoleGameController controller = new ConsoleGameController(
                game, 
                new UcciRobot(game, 10, @"C:\Users\SlimeNull\Downloads\象棋巫师\ELEEYE.EXE"));

            while (true)
            {
                controller.PrintToConsole();
                var key = Console.ReadKey().Key;

                if (key == ConsoleKey.LeftArrow)
                {
                    controller.MoveLeft();
                }
                else if (key == ConsoleKey.RightArrow)
                {
                    controller.MoveRight();
                }
                else if (key == ConsoleKey.UpArrow)
                {
                    controller.MoveUp();
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    controller.MoveDown();
                }
                else if (key is ConsoleKey.Spacebar or ConsoleKey.Enter)
                {
                    var moved = controller.Select();

                    if (moved)
                    {
                        await controller.BotMoveAsync();
                        controller.ResetCurrent();
                    }
                }

                Console.SetCursorPosition(0, 0);

                if (controller.Winner != null)
                {
                    break;
                }
            }

            Console.WriteLine($"Winner: {controller.Winner}");
        }
    }
}
