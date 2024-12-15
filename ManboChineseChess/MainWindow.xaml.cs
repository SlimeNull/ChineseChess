using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibChineseChess;
using LibChineseChess.Robots;
using ManboChineseChess.ValueConverters;

namespace ManboChineseChess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [ObservableObject]
    public partial class MainWindow : Window
    {
        public Game Game { get; private set; }
        public UcciRobot Robot { get; private set; }

        public ObservableCollection<PawnOnWpf> Pawns { get; } = new();
        public ObservableCollection<PawnOnWpf> Locations { get; } = new();


        [ObservableProperty]
        private PawnOnWpf? _selectedPawn;

        [ObservableProperty]
        private bool _computerThinking;

        public MainWindow()
        {
            for (int y = 0; y < Board.Height; y++)
            {
                for (int x = 0; x < Board.Width; x++)
                {
                    Locations.Add(new PawnOnWpf()
                    {
                        LocationX = x,
                        LocationY = y
                    });
                }
            }

            Game = new Game();
            Robot = new UcciRobot(Game, 3, "Executable/ELEEYE.exe");

            InitializeFromBoard(Game.CurrentBoard);
            UpdateLocationsStatus();

            DataContext = this;
            InitializeComponent();
        }

        private void Initailize()
        {
            if (Robot is not null)
            {
                Robot.Dispose();
            }

            Game = new Game();
            Robot = new UcciRobot(Game, 3, "Executable/ELEEYE.exe");

            InitializeFromBoard(Game.CurrentBoard);
            UpdateLocationsStatus();
        }

        private void InitializeFromBoard(Board board)
        {
            Pawns.Clear();

            foreach (var pawnOnBoard in board.EnumerateAllPawns())
            {
                Pawns.Add(new PawnOnWpf()
                {
                    Pawn = pawnOnBoard.Pawn,
                    LocationX = pawnOnBoard.Location.X,
                    LocationY = pawnOnBoard.Location.Y,
                    BoardWidth = 800,
                    BoardHeight = 900
                });
            }
        }

        private void UpdateLocationsStatus()
        {
            foreach (var location in Locations)
            {
                location.IsEnabled = false;
            }

            if (SelectedPawn is not null)
            {
                var location = new Location((int)Math.Round(SelectedPawn.LocationX), (int)Math.Round(SelectedPawn.LocationY));

                var pawnMaybe = Game.CurrentBoard.GetPawn(location);

                if (pawnMaybe is { } pawn)
                {
                    var walkableLocations = Game.CurrentBoard.GetWalkableLocations(new PawnOnBoard(pawn, location));
                    var availableLocations = Locations.Where(loc => walkableLocations.Any(walkableLocation => walkableLocation.X == (int)Math.Round(loc.LocationX) && walkableLocation.Y == (int)Math.Round(loc.LocationY)));

                    foreach (var locationOnWpf in availableLocations)
                    {
                        locationOnWpf.IsEnabled = true;
                    }
                }
            }
        }

        public bool CanMovePawn(PawnOnWpf from, int targetX, int targetY)
        {
            var fromLocation = new Location((int)Math.Round(from.LocationX), (int)Math.Round(from.LocationY));
            var fromPawnMaybe = Game.GetPawn(fromLocation);

            if (fromPawnMaybe is not { } fromPawn)
            {
                return false;
            }

            if (!Game.CurrentBoard.GetWalkableLocations(new PawnOnBoard(fromPawn, fromLocation)).Any(loc => loc.X == targetX && loc.Y == targetY))
            {
                return false;
            }

            return true;
        }

        public Task MovePawn(PawnOnWpf from, PawnOnWpf? to, int targetX, int targetY)
        {
            var fromLocation = new Location((int)Math.Round(from.LocationX), (int)Math.Round(from.LocationY));
            var fromPawnMaybe = Game.GetPawn(fromLocation);

            if (fromPawnMaybe is not { } fromPawn)
            {
                throw new InvalidOperationException();
            }

            if (!Game.CurrentBoard.GetWalkableLocations(new PawnOnBoard(fromPawn, fromLocation)).Any(loc => loc.X == targetX && loc.Y == targetY))
            {
                throw new InvalidOperationException();
            }

            Game.MovePawn(fromLocation, new Location(targetX, targetY));

            Duration duration = new Duration(TimeSpan.FromMilliseconds(300));
            Duration scaleDuration = new Duration(TimeSpan.FromMilliseconds(100));

            var easingFunction = new SineEase() { EasingMode = EasingMode.EaseOut };

            DoubleAnimation xAnimation = new DoubleAnimation()
            {
                To = targetX,
                Duration = duration,
                EasingFunction = easingFunction,
            };

            DoubleAnimation yAnimation = new DoubleAnimation()
            {
                To = targetY,
                Duration = duration,
                EasingFunction = easingFunction,
            };

            Storyboard.SetTarget(xAnimation, from);
            Storyboard.SetTarget(yAnimation, from);
            Storyboard.SetTargetProperty(xAnimation, new PropertyPath(PawnOnWpf.LocationXProperty));
            Storyboard.SetTargetProperty(yAnimation, new PropertyPath(PawnOnWpf.LocationYProperty));

            Storyboard storyboard = new Storyboard()
            {
                Children =
                {
                    xAnimation,
                    yAnimation,
                }
            };

            TaskCompletionSource taskCompletionSource = new TaskCompletionSource();

            storyboard.Completed += (s, e) =>
            {
                if (to is not null)
                {
                    Pawns.Remove(to);
                }

                taskCompletionSource.SetResult();
            };

            storyboard.Begin();

            return taskCompletionSource.Task;
        }

        public async Task BotMove()
        {
            ComputerThinking = true;
            var step = await Robot.GetStep();

            var from = Pawns.First(pawn => (int)Math.Round(pawn.LocationX) == step.From.X && (int)Math.Round(pawn.LocationY) == step.From.Y);
            var to = Pawns.FirstOrDefault(pawn => (int)Math.Round(pawn.LocationX) == step.To.X && (int)Math.Round(pawn.LocationY) == step.To.Y);

            await MovePawn(from, to, step.To.X, step.To.Y);

            ComputerThinking = false;
        }

        [RelayCommand]
        public async Task SelectPawn(PawnOnWpf pawnOnWpf)
        {
            var location = new Location((int)Math.Round(pawnOnWpf.LocationX), (int)Math.Round(pawnOnWpf.LocationY));
            var targetPawnMaybe = Game.CurrentBoard.GetPawn(location);

            if (targetPawnMaybe is not { } targetPawn)
            {
                return;
            }

            if (SelectedPawn is not null)
            {
                if (SelectedPawn == pawnOnWpf)
                {
                    SelectedPawn.IsSelected = false;
                    SelectedPawn = null;
                }
                else if (targetPawn.Camp == Camp.Opponent && CanMovePawn(SelectedPawn, location.X, location.Y))
                {
                    SelectedPawn.IsSelected = false;
                    await MovePawn(SelectedPawn, pawnOnWpf, location.X, location.Y);
                    SelectedPawn = null;

                    await BotMove();
                }
            }
            else
            {
                if (targetPawn.Camp == Camp.Self)
                {
                    SelectedPawn = pawnOnWpf;
                    SelectedPawn.IsSelected = true;
                }
            }

            UpdateLocationsStatus();
        }

        [RelayCommand]
        public async Task SelectLocation(PawnOnWpf pawnOnWpf)
        {
            if (SelectedPawn is null)
            {
                return;
            }

            var targetX = (int)Math.Round(pawnOnWpf.LocationX);
            var targetY = (int)Math.Round(pawnOnWpf.LocationY);
            if (CanMovePawn(SelectedPawn, targetX, targetY))
            {
                SelectedPawn.IsSelected = false;
                await MovePawn(SelectedPawn, pawnOnWpf, targetX, targetY);
                SelectedPawn = null;

                await BotMove();
            }
        }

        [RelayCommand]
        public void Undo()
        {
            if (ComputerThinking)
            {
                return;
            }

            Game.Undo(2);

            InitializeFromBoard(Game.CurrentBoard);
        }

        [RelayCommand]
        public void Reset()
        {
            Initailize();
        }

        protected override void OnClosed(EventArgs e)
        {
            Robot.Dispose();
            base.OnClosed(e);
        }
    }
}