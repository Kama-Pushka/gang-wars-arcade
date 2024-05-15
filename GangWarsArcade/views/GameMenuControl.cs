using GangWarsArcade.Properties;
using System.Drawing.Text;

namespace GangWarsArcade.views;

public partial class GameMenuControl : UserControl
{
    public event Action BackToMenuCliked;     public event Action GameMenuControlShowed;     public event Action GameMenuControlHided; 
    private readonly Label _menuTitle;
    private readonly List<Button> _menuButtons;

    private readonly Size _initalSize;
    private readonly Point _initalLocation;

    private Label _text;     private PictureBox _contorsImage;
    private PictureBox _fireImage;
    private PictureBox _useInventoryImage;
    private Button _back;

    public GameMenuControl(Point location, PrivateFontCollection font)
    {
        InitializeComponent();
        Location = location;
        Size = new Size(200, 400);
        BackColor = Color.Gray;

        _initalSize = Size;
        _initalLocation = Location;

        InitializeControlsMenu(font);

        _menuTitle = new Label() { Text = "Menu", Font = new Font(font.Families[0], 18), Location = new Point(Size.Width / 2 - 35, 10) };         Controls.Add(_menuTitle);

        _menuButtons = CreatePauseMenu();
        KeyDown += CloseGameMenuControl;

        Hide();
    }

    private void InitializeControlsMenu(PrivateFontCollection font)
    {
        _text = new Label();
        _text.Text = "Set move direction:\n\nFire:\n\nUse inventory item:";         _text.TextAlign = ContentAlignment.MiddleCenter;
        _text.Font = new Font(font.Families[0], 20);
        _text.Location = new Point(10, 60);
        _text.Size = new Size(150, 300);
        _text.Hide();
        Controls.Add(_text);

        _contorsImage = new PictureBox();
        _contorsImage.Image = Resource.wasd;
        _contorsImage.Location = new Point(170, 50);
        _contorsImage.Size = new Size(100, 100);
        _contorsImage.SizeMode = PictureBoxSizeMode.Zoom;
        _contorsImage.Hide();
        Controls.Add(_contorsImage);

        _fireImage = new PictureBox();
        _fireImage.Image = Resource.mouse_left_click;
        _fireImage.Location = new Point(170, 150);
        _fireImage.Size = new Size(100, 100);
        _fireImage.SizeMode = PictureBoxSizeMode.Zoom;
        _fireImage.Hide();
        Controls.Add(_fireImage);

        _useInventoryImage = new PictureBox();
        _useInventoryImage.Image = Resource.e;
        _useInventoryImage.Location = new Point(195, 275);
        _useInventoryImage.Size = new Size(50, 50);
        _useInventoryImage.SizeMode = PictureBoxSizeMode.Zoom;
        _useInventoryImage.Hide();
        Controls.Add(_useInventoryImage);

        _back = new Button();
        _back.Location = new Point(10, 10);
        _back.Size = new Size(100, 30);
        _back.Text = "Back";
        _back.Click += (_, __) =>
        {
            HideControlsMenu();
        };
        _back.Hide();
        Controls.Add(_back);
    }

    private List<Button> CreatePauseMenu()      {
        var buttons = new List<Button>();
        var resume = new Button { Text = $"Resume", Location = new Point(Size.Width / 2 - 90, 50), Size = new Size(180, 50) };
        resume.Click += (_, __) =>
        {
            HideGameMenuControl();         };
        buttons.Add(resume);
        Controls.Add(resume);

        var control = new Button { Text = $"Control", Location = new Point(Size.Width / 2 - 90, 130), Size = new Size(180, 50) };
        control.Click += (_, __) =>
        {
            ShowControlsMenu();
        };
        buttons.Add(control);
        Controls.Add(control);

        var backToMenu = new Button { Text = $"Back To Menu", Location = new Point(Size.Width / 2 - 90, 210), Size = new Size(180, 50) };
        backToMenu.Click += (_, __) =>
        {
            BackToMenuCliked();
        };
        buttons.Add(backToMenu);
        Controls.Add(backToMenu);

        var exit = new Button { Text = $"Exit", Location = new Point(Size.Width / 2 - 90, 290), Size = new Size(180, 50) };
        exit.Click += (_, __) =>
        {
            Application.Exit();
        };
        buttons.Add(exit);
        Controls.Add(exit);

        return buttons;
    }

    private void ShowControlsMenu()     {
        foreach (var button in _menuButtons) 
            button.Visible = false;
        _menuTitle.Visible = false;

        Size = new Size(300, 400);
        Location = new Point(Location.X - 50, Location.Y);
        _text.Show();
        _contorsImage.Show();
        _fireImage.Show();
        _useInventoryImage.Show();
        _back.Show();
    }

    private void HideControlsMenu()     {
        foreach (var button in _menuButtons)
            button.Visible = true;
        _menuTitle.Visible = true;

        Size = _initalSize;
        Location = _initalLocation;
        _text.Hide();
        _contorsImage.Hide();
        _fireImage.Hide();
        _useInventoryImage.Hide();
        _back.Hide();
    }

    private void CloseGameMenuControl(object sender, KeyEventArgs e)     {
        if (e.KeyCode == Keys.Escape)         {
            HideGameMenuControl();
        }
    }

    public void ShowGameMenuControl()
    {
        GameMenuControlShowed();          Show();
        Focus();     }

    private void HideGameMenuControl()
    {
        Hide();
        GameMenuControlHided();
    }
}