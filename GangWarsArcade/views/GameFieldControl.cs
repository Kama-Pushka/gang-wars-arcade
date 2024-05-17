using GangWarsArcade.domain;
using System.Drawing.Text;
using Point = System.Drawing.Point;

namespace GangWarsArcade.views;

public partial class GameFiledControl : UserControl
{
    public event Action PausingGame;

    private readonly GameState _gameState;
    private readonly InventoryControl _inventoryHumanPlayer;
    private readonly AlertControl _alertControl;

    public GameFiledControl(GameState gameState, Point location, Size size, PrivateFontCollection fonts)
    {
        InitializeComponent();
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        Location = location;
        Size = size;

        //Initialize Game State
        _gameState = gameState;
        _gameState.GameplayPainter.InvalidateVisual += Invalidate;
        Paint += _gameState.GameplayPainter.Render;
        KeyDown += HumanPlayer_KeyPressed;
        MouseClick += HumanPlayer_MouseClick;

        //Initialize Inventory
        _inventoryHumanPlayer = new InventoryControl(new Point(Size.Width / 2 - 64, Size.Height - 120));
        _gameState.GameMap.HumanPlayer.Updated += _inventoryHumanPlayer.Update;
        _gameState.GameMap.HumanPlayer.HumanPlayerShoted += _inventoryHumanPlayer.SetDrawGunCooldown;
        Controls.Add(_inventoryHumanPlayer);

        //Initialize AlertControl
        _alertControl = new AlertControl(new Point(0, 0), Size, fonts);
        _alertControl.AlertShowed += PauseGame;
        _alertControl.AlertHided += RunGame;
        _alertControl.PreparingNewRound += gameState.PrepareNewRound;
        _alertControl.CheckingForFinishGame += gameState.CheckForFinishGame;
        gameState.RoundFinished += ShowFinishRoundAlert;
        gameState.GameMap.HumanPlayer.HumanPlayerWasted += _alertControl.ShowWastedAlert;
        Controls.Add(_alertControl);

        //Show Training window
        var playerLocation = _gameState.GameMap.HumanPlayer.Position;
        var cellSize = _gameState.GameplayPainter.CellSize;
        var highlightWindowPosition = new Point(playerLocation.X * cellSize.Width - 15, playerLocation.Y * cellSize.Height - 15);
        _alertControl.ShowTrainingAlert(highlightWindowPosition);
    }

    public void ShowFinishRoundAlert(Player player)
        => _alertControl.ShowFinishRoundAlert(_gameState.RoundNumber, player);

    public void GameMenu_PauseGame()
    {
        _alertControl.ShowPauseAlert();
        PauseGame();
    }

    public void GameMenu_RunGame()
    {
        _alertControl.Hide();
        RunGame();
    }

    public void PauseGame()
    {
        _gameState.PauseGame();
    }

    public void RunGame()
    {
        Focus();
        _gameState.RunGame();
    }

    public void HumanPlayer_KeyPressed(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.W || e.KeyCode == Keys.S || e.KeyCode == Keys.A || e.KeyCode == Keys.D)
        {
            _gameState.GameMap.HumanPlayer.SetNewDirection((MoveDirection)e.KeyCode);
        }
        else if (e.KeyCode == Keys.E)
        {
            _gameState.GameMap.HumanPlayer.UseInventoryItem(_gameState.GameMap);
        }
        else if (e.KeyCode == Keys.Escape && _alertControl.Visible == false)
        {
            PausingGame();
        }
    }

    public void HumanPlayer_MouseClick(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _gameState.GameMap.HumanPlayer.Shot(_gameState.GameMap);
        }
    }
}