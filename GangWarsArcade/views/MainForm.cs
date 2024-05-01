using GangWarsArcade.domain;
using GangWarsArcade.views;

namespace GangWarsArcade
{
    public partial class MainForm : Form
    {
        private GameplayControl gameplayControl;
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
            gameplayControl = new GameplayControl(gang);
            topbarControl = new TopbarControl(gameplayControl.GameMap.Players.Values.ToArray());

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
            alertControl.AlertShowed += gameplayControl.AlertShowed;
            alertControl.AlertShowed += topbarControl.AlertShowed;
            alertControl.AlertNotShowed += gameplayControl.AlertNotShowed;
            alertControl.AlertNotShowed += topbarControl.AlertNotShowed;
            alertControl.StartingNewRound += gameplayControl.SetNewRound;
            alertControl.StartingNewRound += topbarControl.SetNewRound;
            alertControl.CheckForFinishGame += topbarControl.CheckForFinishGame;
            topbarControl.RoundFinished += alertControl.ShowAlert;
            topbarControl.GameFinished += RemoveGameControls;

            gameplayControl.GameMap.HumanPlayer.OnPlayerWasted += alertControl.ShowWastedAlert;
            gameplayControl.Paused += alertControl.ShowAlert;
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
