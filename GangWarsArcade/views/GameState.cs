using GangWarsArcade.domain;

using Timer = System.Windows.Forms.Timer;

namespace GangWarsArcade.views;

public class GameState
{
    private const int roundDurationSeconds = 30;

    public event Action<AlertProperties> RoundFinished;
    public event Action GameFinished;
    public event Action InvalidateTopbarVisual;

    public Map GameMap => GameplayPainter.CurrentMap;
    public readonly GameplayPainter GameplayPainter;

    public int RoundNumber { get => _numRound; private set { _numRound = value; } }
    private int _numRound;
    public int _numСompletedRound;
    public readonly Gang[] RoundsWinners = new Gang[5];

    public int TimeLeft { get => _timeLeft; private set { _timeLeft = value; } }
    private int _timeLeft;
    private readonly Timer _mapUpdateTimer;
    private readonly Timer _gameTimer;

    public GameState(Gang gang)
    {
        // Initialize game map and map painter
        var map = Map.InitializeMap();
        GameplayPainter = new GameplayPainter(map);
        GameMap.HumanPlayer = GameMap.Players[gang]; 
        GameMap.HumanPlayer.IsHumanPlayer = true;

        // Initialize map painter timer
        _mapUpdateTimer = new Timer { Interval = 160 }; // 60
        _mapUpdateTimer.Tick += UpdateGamePainter;

        // Initialize game timer
        _gameTimer = new Timer { Interval = 1000 };
        _gameTimer.Tick += GameTimerTick;

        _timeLeft = roundDurationSeconds;
        RoundNumber = 1;
    }

    private void UpdateGamePainter(object sender, EventArgs e)
    {
        GameplayPainter.Update();
    }

    private void GameTimerTick(object sender, EventArgs e)
    {
        var earlyFinish = СheckForEarlyFinishRound(out var winner);
        if (_timeLeft > 0 && !earlyFinish)
        {
            _timeLeft--;
            InvalidateTopbarVisual();
        }
        else if (!earlyFinish) 
        {
            var activePlayers = GameMap.Players.Values.Where(p => p.IsActive).OrderByDescending(p => p.OwnedBuildings).ToArray();
            if (activePlayers[0].OwnedBuildings == activePlayers[1].OwnedBuildings) FinishRound(null);
            else FinishRound(activePlayers[0]);
        }
        else
        {
            FinishRound(winner);
        }
    }

    private bool СheckForEarlyFinishRound(out Player? winner) // Обновление Player bar и проверка крайних случаев завершения игры (все погибли и не могут возродится или остался только один игрок)
    {
        winner = null;
        var activePlayers = GameMap.Players.Values;
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
        RoundFinished(new AlertProperties(this, AlertPropertiesEnums.RoundFinished, winner));
    }

    // Public methods //

    public void PauseGame()
    {
        _mapUpdateTimer.Stop();
        _gameTimer.Stop();
    }

    public void RunGame()
    {
        _mapUpdateTimer.Start();
        _gameTimer.Start();
    }

    public void SetNewRound()
    {
        GameplayPainter.ResetMap();
        _numRound++; // RoundNumber++;

        _timeLeft = roundDurationSeconds;
        InvalidateTopbarVisual();
    }

    public void CheckForFinishGame(bool kostil)
    {
        if (_numСompletedRound == 5 || kostil) GameFinished();
    }
}
