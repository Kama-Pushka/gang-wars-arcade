using GangWarsArcade.domain;
using Point = System.Drawing.Point;

namespace GangWarsArcade.views;

public partial class MenuControl : UserControl
{
    public event Action<Gang> StartGameButtonClicked;
    
    private Gang playerGang;

    private Button startGameButton;
    
    public MenuControl()
    {
        InitializeComponent();

        var buttons = new List<Button>();
        for (var i = 0; i < Enum.GetNames(typeof(Gang)).Length; i++)
        {
            var button = new Button
            {
                Text = $"Player {(Gang)i + 1}",
                BackColor = GameplayPainter.colourValues[i + 1 % GameplayPainter.colourValues.Length].Color,
                Location = new Point(i * 200, 15),
                Tag = i + 1,
                Size = new Size(150, 25)
            };
            button.Click += (sender, __) =>
            {
                var button = sender as Button;
                
                playerGang = (Gang)button.Tag;
                UpdateButtonsColors((Gang)button.Tag, buttons);
                ShowStartGameButton();
            };
            Controls.Add(button);
            buttons.Add(button);
        }

        var start = new Button
        {
            Text = $"Start Game",
            BackColor = Color.White,
            Location = new Point(100, 100),
        };
        start.Click += (_, __) =>
        {
            Hide();
            StartGameButtonClicked(playerGang);
        };
        startGameButton = start;
        startGameButton.Visible = false;
        Controls.Add(start);
    }

    private void ShowStartGameButton()
    {
        if (startGameButton.Visible == false) startGameButton.Visible = true;
    }

    private void UpdateButtonsColors(Gang gang, List<Button> linkLabels)
    {
        foreach (var linkLabel in linkLabels)
        {
            //linkLabel.BackColor = (Gang)linkLabel.Tag == gang ? Color.LimeGreen : Color.White;
        }
    }
}
