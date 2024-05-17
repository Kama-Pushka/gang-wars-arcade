using GangWarsArcade.domain;
using GangWarsArcade.Properties;
using System.Drawing.Text;
using Point = System.Drawing.Point;

namespace GangWarsArcade.views;

public partial class MainMenuControl : UserControl
{
    public event Action<Gang> StartGameButtonClicked;
    
    private Gang _playerGang;

    private readonly PictureBox _mark;
    private readonly PointF[] _markPoint;

    private readonly Button startGameButton;

    public MainMenuControl(Size size, PrivateFontCollection font)
    {
        InitializeComponent();
        Size = size;
        BackColor = Color.Black;
        Font = new Font(font.Families[0], 36);

        _mark = new PictureBox
        {
            Size = new Size(300, 50),
            Image = new Bitmap(300, 50)
        };
        Controls.Add(_mark);
        _markPoint = new PointF[] { new Point(0, 0), new Point(300, 0), new Point(150, 50) };

        var title = new Label()
        {
            Text = "Choose your gang:",
            ForeColor = Color.White,
            Size = new Size(400, 100),
            Location = new Point(Size.Width / 2 - 200, 25)
        };
        Controls.Add(title);
        for (var i = 1; i <= Enum.GetNames(typeof(Gang)).Length; i++)
        {
            CreatePlayerButtons((Gang)i);
        }
        var start = new Button
        {
            Text = $"Start Game",
            Size = new Size(200, 70),
            BackColor = Color.White,
            Location = new Point(Size.Width / 2 - 100, Size.Height - 110),
            Visible = false
        };
        start.Click += (_, __) =>
        {
            Hide();
            StartGameButtonClicked(_playerGang);
        };
        startGameButton = start;
        Controls.Add(start);
    }

    private void CreatePlayerButtons(Gang gang)
    {
        var text = string.Empty;
        var color = Color.White;
        switch (gang)
        {
            case Gang.Green:
                text = " the\nOrcs";
                color = Color.GreenYellow;
                break;
            case Gang.Pink:
                color = Color.HotPink;
                text = " the\nElfs";
                break;
            case Gang.Yellow:
                text = " the\nUndead";
                color = Color.Yellow;
                break;
            case Gang.Blue:
                text = " the\nHumans";
                color = Color.Blue;
                break;
        }
        var shadow = new Label()
        {
            Text = text,
            BackColor = Color.Transparent,
            ForeColor = Color.Black,
            Size = new Size(200, 150),
            Location = new Point(25, 365)
        };
        var label = new Label()
        {
            Text = text,
            BackColor = Color.Transparent,
            ForeColor = color,
            Size = new Size(200, 150),
            Location = new Point(-5, -5)
        };
        var button = new Button
        {
            BackColor = GameplayPainter.ColourValues[(int)gang % GameplayPainter.ColourValues.Length].Color,
            Location = new Point(((int)gang - 1) * 310 + 25, 150),
            Size = new Size(300, 502),
            Image = IdentifyImage(gang),
            ImageAlign = ContentAlignment.BottomCenter,
            FlatStyle = FlatStyle.Flat,
            Tag = gang
        };
        button.FlatAppearance.BorderSize = 0;
        button.Click += (sender, __) =>
        {
            var button = sender as Button;

            _playerGang = (Gang)button.Tag;
            DrawMark(button);
            ShowStartGameButton();
        };
        shadow.Controls.Add(label);
        button.Controls.Add(shadow);
        Controls.Add(button);
    }
    private static Bitmap IdentifyImage(Gang gang)
    {
        return gang switch
        {
            Gang.Green => Resource.Orc,
            Gang.Blue => Resource.Human,
            Gang.Yellow => Resource.Skeleton,
            Gang.Pink => Resource.Elf
        };
    }

    private void DrawMark(Button button)
    {
        var mark = _mark.Image;
        var g = Graphics.FromImage(mark);

        var triangle = new SolidBrush(GameplayPainter.ColourValues[(int)button.Tag % GameplayPainter.ColourValues.Length].Color);
        g.FillPolygon(triangle, _markPoint);

        _mark.Image = mark;
        _mark.Location = new Point(button.Location.X, button.Location.Y - 60);
    }

    private void ShowStartGameButton()
    {
        if (startGameButton.Visible == false) startGameButton.Visible = true;
    }
}
