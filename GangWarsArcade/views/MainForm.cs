using GangWarsArcade.domain;
using GangWarsArcade.views;
using System.Drawing.Text;
using Point = System.Drawing.Point;

namespace GangWarsArcade
{
    public partial class MainForm : Form
    {
        private MainMenuControl mainMenuControl;
        private GameFiledControl _gameFiledControl;
        private TopbarControl topbarControl;
        private Panel panel;         private GameMenuControl gameMenuControl;

        private Size _topbarSize = new Size(1280, 100);         private Size _gameFieldSize = new Size(1344, 768);
        private Size _formSize = new Size(1280, 100 + 704);

        private readonly PrivateFontCollection _fonts;

        public MainForm()
        {
            InitializeComponent();

            _fonts = new PrivateFontCollection();
            _fonts.AddFontFile("resourses/fonts/LifeCraft_Font.ttf");
            Font = new Font(_fonts.Families[0], 8);

            Size = new Size(_formSize.Width + 18, _formSize.Height + 47);             FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
             MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            mainMenuControl = new MainMenuControl(_formSize, _fonts);
            mainMenuControl.StartGameButtonClicked += InitializeGameControls;
            Controls.Add(mainMenuControl);
        }

        public void InitializeGameControls(Gang gang)
        {
            var gameState = new GameState(gang);

            var gameFiledPosition = new Point(-32, _topbarSize.Height - 32);
            var gameFiledSize = new Size(_gameFieldSize.Width, _gameFieldSize.Height);
            _gameFiledControl = new GameFiledControl(gameState, gameFiledPosition, gameFiledSize, _fonts);

            topbarControl = new TopbarControl(gameState, gameState.GameMap.Players.Values.ToArray(), new Point(0, 0), _topbarSize, _fonts);

            panel = new Panel();
            panel.Controls.Add(topbarControl);
            panel.Controls.Add(_gameFiledControl);
            panel.Dock = DockStyle.Fill;

            _gameFiledControl.Focus(); 
            gameState.GameFinished += RemoveGameControls; 
            gameMenuControl = new GameMenuControl(new Point(Size.Width / 2 - 100, Size.Height / 2 - 200), _fonts);
            _gameFiledControl.PausingGame += gameMenuControl.ShowGameMenuControl;
            gameMenuControl.BackToMenuCliked += gameState.FinishGame;
            gameMenuControl.GameMenuControlShowed += _gameFiledControl.TruePauseGame;
            gameMenuControl.GameMenuControlHided += _gameFiledControl.TrueRunGame;
            Controls.Add(gameMenuControl);

            Controls.Add(panel);
        }

        public void RemoveGameControls()
        {
            _gameFiledControl.Dispose();
            topbarControl.Dispose();
            panel.Dispose();
            gameMenuControl.Dispose();

            mainMenuControl.Show();
        }
    }
}
