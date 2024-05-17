using GangWarsArcade.domain;
using System.Drawing.Text;
using Timer = System.Windows.Forms.Timer;

namespace GangWarsArcade.views;

public class GameState
{
    private const int _roundDurationSeconds = 100;

    public event Action<Player> RoundFinished;
    public event Action GameFinished;
    public event Action InvalidateTopbarVisual;

    public Map GameMap { get; }
    public GameplayPainter GameplayPainter { get; }

    public int RoundNumber { get => _numRound; private set { _numRound = value; } }
    private int _numRound;
    private int _numСompletedRound;
    public readonly Gang[] RoundsWinners = new Gang[5];

    public int TimeLeft { get => _timeLeft; private set { _timeLeft = value; } }
    private int _timeLeft;

    private readonly Timer _mapUpdateTimer;
    private readonly Timer _gameTimer;
    private readonly Timer _itemsRespawnTimer;

    public GameState(Gang gang, PrivateFontCollection fonts)
    {
        // Initialize game map and map painter
        var map = Map.InitializeMap();
        map.ResetMap();
        map.SetAI(gang);

        GameMap = map;
        GameplayPainter = new GameplayPainter(map, fonts);

        // Initialize map painter timer
        _mapUpdateTimer = new Timer { Interval = 180 };
        _mapUpdateTimer.Tick += UpdateGamePainter;

        // Initialize game timer
        _gameTimer = new Timer { Interval = 1000 };
        _gameTimer.Tick += GameTimerTick;

        _timeLeft = _roundDurationSeconds;
        RoundNumber = 1;

        //Initialize items respawn timer
        _itemsRespawnTimer = new Timer { Interval = 20000 };
        _itemsRespawnTimer.Tick += ItemsRespawn;
        _itemsRespawnTimer.Start();
    }

    private void UpdateGamePainter(object? _, EventArgs __)
        => GameplayPainter.Update();

    private void ItemsRespawn(object? _, EventArgs __)
        => GameMap.AddItemsToRandomCell([ItemType.FireBolt, ItemType.HPRegeneration, ItemType.HPRegeneration, ItemType.Trap]);

    private void GameTimerTick(object? _, EventArgs __)
    {
        var earlyFinish = СheckForEarlyFinishRound(out var winner); 
        if (_timeLeft > 0 && !earlyFinish)
        {
            _timeLeft--;
            InvalidateTopbarVisual();
        }
        else if (!earlyFinish) 
        {
            var activePlayers = GameMap.Players.Values.Where(p => p.IsActive).OrderByDescending(p => p.OwnedBuildingsCount).ToArray();
            if (activePlayers[0].OwnedBuildingsCount == activePlayers[1].OwnedBuildingsCount) FinishRound(null);
            else FinishRound(activePlayers[0]);
        }
        else
        {
            FinishRound(winner);
        }
    }

    private bool СheckForEarlyFinishRound(out Player? winner)
    {
        winner = null;
        var activePlayers = GameMap.Players.Values.Where(p => p.IsActive).ToList();
        if (activePlayers.Count == 0) return true;
        if (activePlayers.Count == 1)
        {
            winner = activePlayers.First();
            return true;
        }
        return false;
    }

    private void FinishRound(Player winner)
    {
        if (winner != null) RoundsWinners[_numСompletedRound++] = winner.Gang;
        RoundFinished(winner);
    }

    public void PauseGame()
    {
        _mapUpdateTimer.Stop();
        _gameTimer.Stop();
        _itemsRespawnTimer.Stop();
    }

    public void RunGame()
    {
        _mapUpdateTimer.Start();
        _gameTimer.Start();
        _itemsRespawnTimer.Start();
    }

    public void PrepareNewRound()
    {
        GameplayPainter.ResetMap();
        _numRound++;

        _timeLeft = _roundDurationSeconds;
        InvalidateTopbarVisual();
    }

    public void CheckForFinishGame()
    {
        if (_numСompletedRound == 5) FinishGame();
    }

    public void FinishGame()
        => GameFinished();
}
