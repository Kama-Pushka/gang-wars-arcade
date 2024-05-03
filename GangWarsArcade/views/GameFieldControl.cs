using GangWarsArcade.domain;
using Point = System.Drawing.Point;
using Timer = System.Windows.Forms.Timer;

namespace GangWarsArcade.views;

public partial class GameFiledControl : UserControl
{
    public event Action<AlertProperties> PausingGame;

    private readonly GameState _gameState;

    public GameFiledControl(GameState gameState)
    {
        InitializeComponent();
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

        _gameState = gameState;

        _gameState.GameplayPainter.InvalidateVisual += Invalidate;
        Paint += _gameState.GameplayPainter.Render;
        KeyDown += HumanPlayerDirectionUpdate;
        MouseClick += HumanPlayerShot;
    }

    // Human Player Control //

    public void HumanPlayerDirectionUpdate(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.W || e.KeyCode == Keys.S || e.KeyCode == Keys.A || e.KeyCode == Keys.D)
        {
            _gameState.GameMap.HumanPlayer.SetNewDirection((MoveDirection)e.KeyCode);
        }
        else if (e.KeyCode == Keys.E)
        {
            _gameState.GameMap.HumanPlayer.UseInventoryItem(_gameState.GameMap);
        }
        else if (e.KeyCode == Keys.Escape)
        {
            PausingGame(new AlertProperties(this, AlertPropertiesEnums.Pause));
        }
    }

    public void HumanPlayerShot(object sender, MouseEventArgs e)
    {
        _gameState.GameMap.HumanPlayer.Shot(_gameState.GameMap);
    }
}