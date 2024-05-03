using GangWarsArcade.domain;
using GangWarsArcade.views;

namespace GangWarsArcade
{
    public partial class MainForm : Form
    {
        private GameFiledControl gameplayControl;
        private TopbarControl topbarControl;
        private TableLayoutPanel tableLayoutPanel;
        private AlertControl alertControl;
        private MenuControl menuControl;
        
        public MainForm()
        {
            InitializeComponent();

            menuControl = new MenuControl();
            menuControl.StartGameButtonClicked += InitializeGameControls;
            Controls.Add(menuControl);
        }

        public void InitializeGameControls(Gang gang)
        {
            var gameState = new GameState(gang);
            
            gameplayControl = new GameFiledControl(gameState);
            topbarControl = new TopbarControl(gameState, gameState.GameMap.Players.Values.ToArray());

            tableLayoutPanel = new TableLayoutPanel();
            tableLayoutPanel.RowStyles.Clear();
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize, 100));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            tableLayoutPanel.Controls.Add(topbarControl);
            tableLayoutPanel.Controls.Add(gameplayControl);

            tableLayoutPanel.Dock = DockStyle.Fill;
            topbarControl.Dock = DockStyle.Fill;
            gameplayControl.Dock = DockStyle.Fill;

            alertControl = new AlertControl();
            alertControl.AlertShowed += gameState.PauseGame;
            alertControl.AlertHided += gameState.RunGame;
            alertControl.StartingNewRound += gameState.SetNewRound;
            alertControl.CheckForFinishGame += gameState.CheckForFinishGame;
            
            gameState.RoundFinished += alertControl.SetAlert;
            gameState.GameFinished += RemoveGameControls;

            gameState.GameMap.HumanPlayer.OnPlayerWasted += alertControl.ShowWastedAlert;
            gameplayControl.PausingGame += alertControl.SetAlert;
            gameplayControl.Focus();

            Controls.Add(alertControl);

            alertControl.SetupGame();

            Controls.Add(tableLayoutPanel);
        }

        public void RemoveGameControls()
        {
            gameplayControl.Dispose();
            topbarControl.Dispose();
            tableLayoutPanel.Dispose();
            alertControl.Dispose();

            menuControl.Show();
        }
    }
}
