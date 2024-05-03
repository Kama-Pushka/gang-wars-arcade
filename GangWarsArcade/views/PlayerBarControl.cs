using GangWarsArcade.domain;
using Point = System.Drawing.Point;

namespace GangWarsArcade.views;

public partial class PlayerBarControl : UserControl
{
    private PictureBox _image;
    private Label _text;

    public PlayerBarControl(Player player)
    {
        InitializeComponent();

        DrawPlayerBar(player);
    }

    private void DrawPlayerBar(Player player)
    {
        _text = new Label
        {
            Size = new Size(150, 70),
            Text = string.Format("Player {0} | {1}", player.Gang, player.OwnedBuildings),
            TextAlign = ContentAlignment.MiddleRight,
            BackColor = GameplayPainter.colourValues[(int)player.Gang % GameplayPainter.colourValues.Length].Color,
            Image = player.Image,
            ImageAlign = ContentAlignment.MiddleLeft,
            Tag = player
        };
        Controls.Add(_text);
    }

    public void UpdatePlayerBar(Player player)
    {
        if (player.IsAlive) _text.Text = string.Format("Player {0} | {1}\nHP {2}, Gun {3}", player.Gang, player.OwnedBuildings, player.HP, player.Weapon);
        else _text.Text = string.Format("Player {0} | {1}\nHP {2}, Gun {3}\nWASTED", player.Gang, player.OwnedBuildings, player.HP, player.Weapon);
    }
}
