using GangWarsArcade.domain;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using Point = System.Drawing.Point;


namespace GangWarsArcade.views;

public partial class PlayerBarControl : UserControl
{
    private PictureBox _image;
    private Label _player;
    private Label _ownedLocation;

    private Pen _border;

    public PlayerBarControl(Player player, PrivateFontCollection font)     {
        InitializeComponent();

        _border = new Pen(GameplayPainter.colourValues[(int)player.Gang % GameplayPainter.colourValues.Length].Color, 1); 
        Size = new Size(210, 75);
        
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
            ForeColor = GameplayPainter.colourValues[(int)player.Gang % GameplayPainter.colourValues.Length].Color,
            TextAlign = ContentAlignment.MiddleRight,
            Image = player.Image,             ImageAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.Transparent,
            Tag = player
        };
        _ownedLocation = new Label { 
            BackColor = Color.Transparent, 
            Font = new Font(font.Families[0], 24), 
            Size = new Size(45, 70), 
            Text = $"{player.OwnedBuildings}",
            ForeColor = GameplayPainter.colourValues[(int)player.Gang % GameplayPainter.colourValues.Length].Color,
            TextAlign = ContentAlignment.MiddleRight, 
            Location = new Point(155, 2) 
        };
        Controls.Add(_ownedLocation);
        Controls.Add(_player);
    }

         private void DrawPlayerBar(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;

        var path = RoundedRectangle.Create(0, 0, Size.Width - 3, Size.Height - 3, 7);
        var path1 = RoundedRectangle.Create(1, 1, Size.Width - 5, Size.Height - 5, 7);
        var path2 = RoundedRectangle.Create(2, 2, Size.Width - 7, Size.Height - 7, 7);

        var shadow = RoundedRectangle.Create(1, 1, Size.Width - 2, Size.Height - 2, 7);
        var shadow1 = RoundedRectangle.Create(2, 2, Size.Width - 4, Size.Height - 4, 7);
        var shadow2 = RoundedRectangle.Create(3, 3, Size.Width - 6, Size.Height - 6, 7);

        g.DrawPath(Pens.Black, shadow);
        g.DrawPath(Pens.Black, shadow1);
        g.DrawPath(Pens.Black, shadow2);

        g.DrawPath(_border, path);
        g.DrawPath(_border, path1);
        g.DrawPath(_border, path2);

                
                
                                    }

    public void UpdatePlayerBar(Player player)     {
        if (player.IsAlive)
        {
            _player.Text = string.Format("HP: {0}", player.HP, player.Weapon);             _ownedLocation.Text = $"{player.OwnedBuildings}";
        }
        else _player.Text = string.Format("WASTED");
    }
}
