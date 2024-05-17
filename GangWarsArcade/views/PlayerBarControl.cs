using GangWarsArcade.domain;
using System.Drawing.Text;
using Point = System.Drawing.Point;

namespace GangWarsArcade.views;

public partial class PlayerBarControl : UserControl
{
    private Label _player;
    private Label _ownedLocation;

    private Pen _border;

    public PlayerBarControl(Player player, PrivateFontCollection font)
    {
        InitializeComponent();
        Size = new Size(210, 75);

        _border = new Pen(GameplayPainter.ColourValues[(int)player.Gang % GameplayPainter.ColourValues.Length].Color, 1);

        Paint += DrawPlayerBar;

        CreatePlayerBar(player, font);
    }

    private void CreatePlayerBar(Player player, PrivateFontCollection font)
    {
        Invalidate();
        _player = new Label
        {
            Size = new Size(160, 70),
            Location = new Point(3, 2),
            Font = new Font(font.Families[0], 24),
            Text = string.Format("HP: {0}", player.HP),
            ForeColor = GameplayPainter.ColourValues[(int)player.Gang % GameplayPainter.ColourValues.Length].Color,
            TextAlign = ContentAlignment.MiddleRight,
            Image = player.Image,
            ImageAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.Transparent,
            Tag = player
        };
        _ownedLocation = new Label 
        { 
            BackColor = Color.Transparent, 
            Font = new Font(font.Families[0], 24), 
            Size = new Size(45, 70), 
            Text = $"{player.OwnedBuildingsCount}",
            ForeColor = GameplayPainter.ColourValues[(int)player.Gang % GameplayPainter.ColourValues.Length].Color,
            TextAlign = ContentAlignment.MiddleRight, 
            Location = new Point(155, 2) 
        };
        Controls.Add(_ownedLocation);
        Controls.Add(_player);
    }

    private void DrawPlayerBar(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;

        var line1 = RoundedRectangle.Create(0, 0, Size.Width - 3, Size.Height - 3, 7);
        var line2 = RoundedRectangle.Create(1, 1, Size.Width - 5, Size.Height - 5, 7);
        var path3 = RoundedRectangle.Create(2, 2, Size.Width - 7, Size.Height - 7, 7);

        var shadowLine1 = RoundedRectangle.Create(1, 1, Size.Width - 2, Size.Height - 2, 7);
        var shadowLine2 = RoundedRectangle.Create(2, 2, Size.Width - 4, Size.Height - 4, 7);
        var shadowLine3 = RoundedRectangle.Create(3, 3, Size.Width - 6, Size.Height - 6, 7);

        g.DrawPath(Pens.Black, shadowLine1);
        g.DrawPath(Pens.Black, shadowLine2);
        g.DrawPath(Pens.Black, shadowLine3);

        g.DrawPath(_border, line1);
        g.DrawPath(_border, line2);
        g.DrawPath(_border, path3);
    }

    public void PlayerBarUpdate(Player player)
    {
        if (player.IsAlive)
        {
            BeginInvoke(() => // это необходимо по причине того, что System.Timers.Timer в Player запускает таймер в новом потоке
            { 
                _player.Text = $"HP: {player.HP}"; 
                _ownedLocation.Text = $"{player.OwnedBuildingsCount}"; 
            });
        }
        else
        {
            BeginInvoke(() =>
            { 
                _player.Text = string.Format("WASTED");
                _ownedLocation.Text = "";
            });
        }
    }
}
