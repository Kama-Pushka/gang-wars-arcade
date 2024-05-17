using GangWarsArcade.domain;
using GangWarsArcade.views;
using System.Drawing.Text;
using Point = System.Drawing.Point;

namespace GangWarsArcade;

public partial class MainForm : Form
{
    private MainMenuControl _mainMenuControl;
    private GameFiledControl _gameFiledControl;
    private TopbarControl _topbarControl;
    private Panel _panel;
    private GameMenuControl _gameMenuControl;

    private Size _topbarSize = new Size(1280, 100);
    private Size _gameFieldSize = new Size(1344, 768);
    private Size _formSize = new Size(1280, 100 + 704);

    private readonly PrivateFontCollection _fonts;

    public MainForm()
    {
        InitializeComponent();

        _fonts = new PrivateFontCollection();
        _fonts.AddFontFile("resourses/fonts/LifeCraft_Font.ttf");
        Font = new Font(_fonts.Families[0], 8);

        Size = new Size(_formSize.Width, _formSize.Height);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        _mainMenuControl = new MainMenuControl(_formSize, _fonts.Families[0]);
        _mainMenuControl.StartGameButtonClicked += InitializeGameControls;
        Controls.Add(_mainMenuControl);
    }

    public void InitializeGameControls(Gang gang)
    {
        var gameState = new GameState(gang, _fonts.Families[0]);

        var gameFiledPosition = new Point(-32, _topbarSize.Height - 32);
        var gameFiledSize = new Size(_gameFieldSize.Width, _gameFieldSize.Height);
        _gameFiledControl = new GameFiledControl(gameState, gameFiledPosition, gameFiledSize, _fonts.Families[0]);

        _topbarControl = new TopbarControl(gameState, gameState.GameMap.Players.Values.ToArray(), new Point(0, 0), _topbarSize, _fonts.Families[0]);

        _panel = new Panel();
        _panel.Controls.Add(_topbarControl);
        _panel.Controls.Add(_gameFiledControl);
        _panel.Dock = DockStyle.Fill;

        _gameFiledControl.Focus();

        gameState.GameFinished += RemoveGameControls;

        _gameMenuControl = new GameMenuControl(new Point(Size.Width / 2 - 100, Size.Height / 2 - 200), _fonts.Families[0]);
        _gameFiledControl.PausingGame += _gameMenuControl.ShowGameMenuControl;
        _gameMenuControl.BackToMenuCliked += gameState.FinishGame;
        _gameMenuControl.GameMenuControlShowing += _gameFiledControl.GameMenu_PauseGame;
        _gameMenuControl.GameMenuControlHided += _gameFiledControl.GameMenu_RunGame;
        Controls.Add(_gameMenuControl);

        Controls.Add(_panel);
    }

    public void RemoveGameControls()
    {
        _gameFiledControl.Dispose();
        _topbarControl.Dispose();
        _panel.Dispose();
        _gameMenuControl.Dispose();

        _mainMenuControl.Show();
    }
}