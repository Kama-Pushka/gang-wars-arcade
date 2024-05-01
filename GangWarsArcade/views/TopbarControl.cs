using GangWarsArcade.domain;
using Timer = System.Windows.Forms.Timer;
using Point = System.Drawing.Point;

namespace GangWarsArcade.views;

public partial class TopbarControl : UserControl
{
    private const int roundDuration = 10;

    public event Action<AlertProperties> RoundFinished;
    public event Action GameFinished;
    
    public readonly Gang[] roundsWinners = new Gang[5];
    public int numRound;
    public int numСompletedRound;

    private readonly Timer gameTimer;
    private int timeLeft;
    private readonly Label timeLable;

    private Dictionary<Player, Label> playerBars;
    private Player[] players;

    public TopbarControl(Player[] players)
    {
        InitializeComponent();
        //SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        BackColor = Color.Gray;

        // Initialize Round winners bar
        Paint += DrawRoundWinners;

        // Initialize Player bars
        DrawPlayersBar(players);
        foreach (var player in players)
            player.OnPlayerWasted += PlayerBarUpdate;

        // Initialize game timer
        timeLable = new Label
        {
            Location = new Point(0, 0),
            Size = new Size(100, 30),
            Text = TimeSpan.FromSeconds(timeLeft).ToString(@"mm\:ss")
        };
        Controls.Add(timeLable);
        gameTimer = new Timer { Interval = 1000 };
        gameTimer.Tick += TimerTick;
    }

    private void TimerTick(object sender, EventArgs e)
    {
        var winner = СheckForEarlyVictory();
        if (timeLeft > 0 && !winner.Item1)
        {
            timeLeft--;
            timeLable.Text = TimeSpan.FromSeconds(timeLeft).ToString(@"mm\:ss");
        }
        else
        {
            if (!winner.Item1) // найти победившего по очкам игрока или ничья
            {
                var alivePlayers = players.Where(p => p.IsAlive || p.OwnedBuildings != 0).OrderByDescending(p => p.OwnedBuildings).ToArray();
                if (alivePlayers[0].OwnedBuildings == alivePlayers[1].OwnedBuildings) FinishRound(null);
                else FinishRound(alivePlayers[0]);
            }
            else
            {
                FinishRound(winner.Item2);
            }
        }
    }

    private void FinishRound(Player winner)
    {
        RoundFinished(new AlertProperties(this, AlertPropertiesEnums.RoundFinished, winner));
    }

    public void AlertShowed(AlertPropertiesEnums option)
    {
        if (option == AlertPropertiesEnums.RoundFinished) gameTimer.Stop();
        if (option == AlertPropertiesEnums.Pause) gameTimer.Stop();
    }

    public void AlertNotShowed() 
    {
        gameTimer.Start();
    }

    private (bool, Player?) СheckForEarlyVictory()
    {
        var alivePlayers = players.Where(p => p.IsAlive || p.OwnedBuildings != 0).ToArray();
        foreach (var player in alivePlayers)
        {
            PlayerBarUpdate(player);
        }

        if (alivePlayers.Length == 0) return (true, null);
        if (alivePlayers.Length == 1) return (true, alivePlayers[0]);
        return (false, null);
    }

    private void PlayerBarUpdate(Player player)
    {
        if (player.IsAlive) playerBars[player].Text = string.Format("Player {0} | {1}\nHP {2}, Gun {3}", player.Gang, player.OwnedBuildings, player.HP, player.Weapon);
        else playerBars[player].Text = string.Format("Player {0} | {1}\nHP {2}, Gun {3}\nWASTED", player.Gang, player.OwnedBuildings, player.HP, player.Weapon);
    }

    private void DrawRoundWinners(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;

        for (var i = 0; i < roundsWinners.Length; i++) 
        {
            var rect = new RectangleF(1000f + i * 30f, 50f, 15, 15);
            g.DrawEllipse(new Pen(Color.FromKnownColor(KnownColor.Black)), rect);
            if (roundsWinners[i] != 0)
            {
                var color = GameplayPainter.colourValues[(int)roundsWinners[i] % GameplayPainter.colourValues.Length];
                g.FillEllipse(new SolidBrush(Color.FromArgb(100, color.Color.R, color.Color.G, color.Color.B)), rect);
            }
        }
    }

    private void DrawPlayersBar(Player[] players)
    {
        var labels = new Dictionary<Player, Label>();
        for (var i = 0; i < players.Length; i++)
        {
            var player = players[i];
            var link = new Label
            {
                Size = new Size(150, 70),
                Text = string.Format("Player {0} | {1}", player.Gang, player.OwnedBuildings),
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = GameplayPainter.colourValues[(int)player.Gang % GameplayPainter.colourValues.Length].Color,
                Location = new Point(i * 200 + 100, 15),
                Image = player.Image,
                ImageAlign = ContentAlignment.MiddleLeft,
                Tag = player
            };
            labels[player] = link;
            Controls.Add(link);
        }
        playerBars = labels;
        this.players = players;
    }

    // Public methods //

    public void CheckForFinishGame(bool kostil)
    {
        if (numСompletedRound == 5 || kostil) GameFinished();
    }

    public void SetNewRound()
    {
        timeLeft = roundDuration;
        //gameTimer.Start();
        timeLable.Text = TimeSpan.FromSeconds(timeLeft).ToString(@"mm\:ss");
        numRound++;
        Invalidate();
    }
}