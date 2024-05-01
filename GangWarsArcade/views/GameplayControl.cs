using GangWarsArcade.domain;
using Point = System.Drawing.Point;
using Timer = System.Windows.Forms.Timer;

namespace GangWarsArcade.views;

public partial class GameplayControl : UserControl
{
    private readonly Timer _mapUpdateTimer;
    private readonly GameplayPainter _gameplayPainter;

    public Map GameMap { get; }
    public int RoundNumber { get; private set; }

    public event Action<AlertProperties> Paused;

    public GameplayControl(Gang gang)
    {
        InitializeComponent();
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

        // Initialize map painter
        _gameplayPainter = new GameplayPainter();
        Paint += _gameplayPainter.Render;
        //MouseDown += _gameplayPainter.OnPointerPressed;
        //MouseUp += _gameplayPainter.OnPointerReleased;
        _gameplayPainter.InvalidateVisual += Invalidate;
        KeyDown += HumanPlayerDirectionUpdate;

        // Initialize game map
        GameMap = Map.InitializeMap();
        GameMap.HumanPlayer = GameMap.Players[gang];
        GameMap.HumanPlayer.IsHumanPlayer = true;
        MouseClick += GameMap.HumanPlayer.Shot;

        _gameplayPainter.Load(GameMap);

        // Initialize map painter timer
        _mapUpdateTimer = new Timer { Interval = 160 }; // 60
        _mapUpdateTimer.Tick += TimerTick;
    }

    private void TimerTick(object sender, EventArgs e)
    {
        _gameplayPainter.Update();
    }

    // Public methods //

    public void SetNewRound()
    {
        _gameplayPainter.ResetMap();
        //_mapUpdateTimer.Start();
        RoundNumber++;
    }

    public void AlertShowed(AlertPropertiesEnums option)
    {
        if (option == AlertPropertiesEnums.RoundFinished) _mapUpdateTimer.Stop();
        if (option == AlertPropertiesEnums.Pause) _mapUpdateTimer.Stop();
    }

    public void AlertNotShowed()
    {
        _mapUpdateTimer.Start();
    }

    // Human Player Control //

    public void HumanPlayerDirectionUpdate(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.W || e.KeyCode == Keys.S || e.KeyCode == Keys.A || e.KeyCode == Keys.D)
        {
            GameMap.HumanPlayer.SetNewDirection((MoveDirection)e.KeyCode);
        }
        else if (e.KeyCode == Keys.E)
        {
            GameMap.HumanPlayer.UseInventoryItem(GameMap);
        }
        else if (e.KeyCode == Keys.Escape)
        {
            Paused(new AlertProperties(this, AlertPropertiesEnums.Pause));
        }
    }
}