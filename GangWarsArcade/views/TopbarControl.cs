using GangWarsArcade.domain;
using Timer = System.Windows.Forms.Timer;
using Point = System.Drawing.Point;

namespace GangWarsArcade.views;

public partial class TopbarControl : UserControl
{
    private readonly GameState _gameState;
    private readonly Label _timeLable;
    private readonly InventoryControl _inventoryHumanPlayer;

    public TopbarControl(GameState gameState, Player[] players)
    {
        InitializeComponent();
        //SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        BackColor = Color.Gray;

        _gameState = gameState;
        _gameState.InvalidateTopbarVisual += Update;

        // Initialize Round winners bar
        Paint += DrawRoundWinners;

        // Initialize Player bars
        CreatePlayerBars(players);

        // Initialize game timer
        _timeLable = new Label
        {
            Location = new Point(0, 0),
            Size = new Size(100, 30),
            Text = TimeSpan.FromSeconds(0).ToString(@"mm\:ss")
        };
        Controls.Add(_timeLable);

        //Initialize Inventory
        _inventoryHumanPlayer = new InventoryControl();
        _gameState.GameMap.HumanPlayer.PlayerUpdated += _inventoryHumanPlayer.Update;
        _gameState.GameMap.HumanPlayer.OnPlayerShoted += _inventoryHumanPlayer.SetDrawGunCooldown;
        Controls.Add(_inventoryHumanPlayer);
    }

    private void CreatePlayerBars(Player[] players)
    {
        for (var i = 0; i < players.Length; i++)
        {
            var player = players[i];
            var bar = new PlayerBarControl(player) { Location = new Point(i * 200 + 100, 15) };
            Controls.Add(bar);

            player.PlayerUpdated += bar.UpdatePlayerBar;
        }
    }

    public void Update()
    {
        _timeLable.Text = TimeSpan.FromSeconds(_gameState.TimeLeft).ToString(@"mm\:ss");
        Invalidate();
    }

    private void DrawRoundWinners(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;

        var rounds = _gameState.RoundsWinners;
        for (var i = 0; i < rounds.Length; i++)
        {
            var rect = new RectangleF(1000f + i * 30f, 50f, 15, 15);
            g.DrawEllipse(new Pen(Color.FromKnownColor(KnownColor.Black)), rect);
            if (rounds[i] != 0)
            {
                var color = GameplayPainter.colourValues[(int)rounds[i] % GameplayPainter.colourValues.Length];
                g.FillEllipse(new SolidBrush(Color.FromArgb(100, color.Color.R, color.Color.G, color.Color.B)), rect);
            }
        }
    }
}