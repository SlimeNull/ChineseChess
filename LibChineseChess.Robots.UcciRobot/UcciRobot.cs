
using System.Diagnostics;

namespace LibChineseChess.Robots
{
    public class UcciRobot : Robot, IDisposable
    {
        private bool _disposedValue;
        private Process _botProcess;
        private StreamWriter _ucciWriter;
        private StreamReader _ucciReader;

        private bool _ucciOK;

        public Game Game { get; }
        public int DefaultDepth { get; }

        public UcciRobot(Game game, int defaultDepth, string executablePath)
        {
            ArgumentNullException.ThrowIfNull(game);
            ArgumentNullException.ThrowIfNull(executablePath);

            _botProcess = Process.Start(
                new ProcessStartInfo()
                {
                    FileName = executablePath,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                }) ?? throw new InvalidOperationException();

            _ucciWriter = _botProcess.StandardInput;
            _ucciReader = _botProcess.StandardOutput;
            Game = game;
            DefaultDepth = defaultDepth;
        }

        private async Task GetReady()
        {
            if (!_ucciOK)
            {
                await _ucciWriter.WriteLineAsync("ucci");
                var received = await _ucciReader.ReadLineAsync();

                if (received is null)
                {
                    throw new InvalidOperationException();
                }

                while (received != "ucciok")
                {
                    received = await _ucciReader.ReadLineAsync();
                }

                _ucciOK = true;
            }

            var fenString = UcciUtilities.GenerateFenString(Game.CurrentBoard, Game.CurrentTurn);
            await _ucciWriter.WriteLineAsync($"position fen {fenString}");
        }

        private GameOperation? GetOperationToWin()
        {
            var otherCamp = Game.GetOtherCamp(Game.CurrentTurn);

            foreach (var pawnOnBoard in Game.CurrentBoard.EnumerateAllPawns(Game.CurrentTurn))
            {
                foreach (var walkableLocation in Game.CurrentBoard.GetWalkableLocations(pawnOnBoard))
                {
                    if (Game.GetPawn(walkableLocation) is { Kind: PawnKind.General } targetPawn &&
                        targetPawn.Camp == otherCamp)
                    {
                        return new GameOperation(pawnOnBoard.Location, walkableLocation, targetPawn);
                    }
                }
            }

            return null;
        }

        private async Task<GameOperation> GoAndReceiveOperation(int depth)
        {
            if (depth < 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            await _ucciWriter.WriteLineAsync($"go depth {depth}");
            var received = await _ucciReader.ReadLineAsync();

            while (true)
            {
                if (received is null)
                {
                    throw new InvalidOperationException();
                }

                if (received.StartsWith("bestmove"))
                {
                    var segments = received.Split(' ');
                    if (segments.Length < 2)
                    {
                        throw new InvalidOperationException();
                    }

                    var todo = segments[1];
                    // var guess = segments[3];

                    var fromX = todo[0] - 'a';
                    var fromY = todo[1] - '0';
                    var toX = todo[2] - 'a';
                    var toY = todo[3] - '0';

                    return new GameOperation(new Location(fromX, fromY), new Location(toX, toY), Game.CurrentBoard.GetPawn(new Location(toX, toY)));
                }
                else if (received.StartsWith("nobestmove"))
                {
                    return GameOperation.None;
                }

                received = await _ucciReader.ReadLineAsync();
            }
        }

        public async Task<GameOperation> GetStep(int depth)
        {
            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(_botProcess));
            }

            if (GetOperationToWin() is { } operationToWin)
            {
                return operationToWin;
            }

            await GetReady();
            return await GoAndReceiveOperation(depth);
        }

        public override Task<GameOperation> GetStep()
            => GetStep(DefaultDepth);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                _botProcess.Kill();
                _botProcess.Dispose();
                _disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~UcciRobot()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
