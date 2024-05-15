using GangWarsArcade.domain;
using System.Drawing.Text;
using Point = System.Drawing.Point;
using Timer = System.Windows.Forms.Timer;

namespace GangWarsArcade.views;

public partial class GameFiledControl : UserControl
{
    public event Action PausingGame;

    private readonly GameState _gameState;
    private readonly InventoryControl _inventoryHumanPlayer;

    private readonly Timer _itemsRespawnTimer; 
    private AlertControl _alertControl;

    public GameFiledControl(GameState gameState, Point location, Size size, PrivateFontCollection fonts)     {
        InitializeComponent();
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        Location = location;
        Size = size;

                
        _gameState = gameState;
        _gameState.GameplayPainter.InvalidateVisual += Invalidate;
        Paint += _gameState.GameplayPainter.Render;
        KeyDown += HumanPlayerDirectionUpdate;
        MouseClick += HumanPlayerShot; 
                _inventoryHumanPlayer = new InventoryControl(new Point(Size.Width / 2 - 64, Size.Height - 120));         _gameState.GameMap.HumanPlayer.PlayerUpdated += _inventoryHumanPlayer.Update;
        _gameState.GameMap.HumanPlayer.OnPlayerShoted += _inventoryHumanPlayer.SetDrawGunCooldown;
        Controls.Add(_inventoryHumanPlayer);

                _itemsRespawnTimer = new Timer { Interval = 20000 };
        _itemsRespawnTimer.Tick += ItemsRespawn;
        _itemsRespawnTimer.Start(); 
                                                
                                        
        
        _alertControl = new AlertControl(new Point(0, 0), Size, fonts);
        _alertControl.AlertShowed += PauseGame;
        _alertControl.AlertHided += RunGame;
        _alertControl.PreparingNewRound += gameState.PrepareNewRound;         _alertControl.CheckingForFinishGame += gameState.CheckForFinishGame; 
        gameState.RoundFinished += ShowFinishRoundAlert;

        gameState.GameMap.HumanPlayer.PlayerWasted += _alertControl.ShowWastedAlert; 
        Controls.Add(_alertControl);

        _alertControl.ShowTrainingAlert();
            }

    public void ShowFinishRoundAlert(Player player)
    {
        _alertControl.ShowFinishRoundAlert(_gameState.RoundNumber, player);
    }

        public void TruePauseGame()
    {
        _alertControl.ShowPauseAlert();
        PauseGame();
    }

    public void TrueRunGame()
    {
        _alertControl.Hide();
        RunGame();
    }

    private void ItemsRespawn(object sender, EventArgs e) 
    {
        _gameState.GameMap.AddRandomItems([ItemType.FireBolt, ItemType.HPRegeneration, ItemType.HPRegeneration, ItemType.Trap]);     }

    public void PauseGame()
    {
                _gameState.PauseGame();
    }

    public void RunGame()
    {
                Focus();
        _gameState.RunGame();
    }

    
        public void HumanPlayerDirectionUpdate(object sender, KeyEventArgs e)     {
        if (e.KeyCode == Keys.W || e.KeyCode == Keys.S || e.KeyCode == Keys.A || e.KeyCode == Keys.D)         {
            _gameState.GameMap.HumanPlayer.SetNewDirection((MoveDirection)e.KeyCode);         }
        else if (e.KeyCode == Keys.E)         {
            _gameState.GameMap.HumanPlayer.UseInventoryItem(_gameState.GameMap);
        }
        else if (e.KeyCode == Keys.Escape)         {
            PausingGame();         }
    }

    public void HumanPlayerShot(object sender, MouseEventArgs e)
    {
        _gameState.GameMap.HumanPlayer.Shot(_gameState.GameMap);     }
}