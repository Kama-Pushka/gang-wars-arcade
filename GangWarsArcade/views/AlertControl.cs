using GangWarsArcade.domain;
using Timer = System.Windows.Forms.Timer;
using Point = System.Drawing.Point;

namespace GangWarsArcade.views;

public partial class AlertControl : UserControl
{
    public event Action<AlertPropertiesEnums> AlertShowed;
    public event Action<bool> CheckForFinishGame;
    public event Action StartingNewRound;
    public event Action AlertNotShowed;

    private readonly Timer timer;
    private int timeLeft;

    private AlertPropertiesEnums currentOption;
    private Label DynamicText;

    private Button[] pauseMenu;

    public AlertControl()
    {
        InitializeComponent();
        //SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        //SetStyle(ControlStyles.Opaque, true);
        //BackColor = Color.Transparent;

        // Initialize countdown timer
        timer = new Timer { Interval = 1000 };
        timer.Tick += TimerTick;

        Location = new Point(100, 100);

        DynamicText = new Label();
        Controls.Add(DynamicText);

        KeyDown += ResumeToGameFromPause;

        pauseMenu = CreatePauseMenu();

        Hide();
    }

    private Button[] CreatePauseMenu()
    {
        var buttons = new Button[4];

        var resume = new Button { Text = $"Resume", Location = new Point(10, 10), Visible = false };
        resume.Click += (_, __) =>
        {
            HideAlert();
        };
        buttons[0] = resume;
        Controls.Add(resume);

        var control = new Button { Text = $"Control", Location = new Point(10, 110), Visible = false };
        control.Click += (_, __) =>
        {
            // TODO здесь показано управление и описания предметов
        };
        buttons[1] = control;
        Controls.Add(control);

        var backToMenu = new Button { Text = $"Back To Menu", Location = new Point(10, 210), Visible = false };
        backToMenu.Click += (_, __) =>
        {
            CheckForFinishGame(true);
        };
        buttons[2] = backToMenu;
        Controls.Add(backToMenu);

        var exit = new Button { Text = $"Exit", Location = new Point(10, 310), Visible = false };
        exit.Click += (_, __) =>
        {
            Application.Exit();
        };
        buttons[3] = exit;
        Controls.Add(exit);

        return buttons;
    }

    private void TimerTick(object sender, EventArgs e)
    {
        if (timeLeft > 0)
        {
            timeLeft--;

            if (currentOption == AlertPropertiesEnums.RoundStarting)
            {
                DynamicText.Text = timeLeft.ToString();
            }
            else if (currentOption == AlertPropertiesEnums.HumanPlayerWasted)
            {
                if (timeLeft == 5)
                {
                    DynamicText.Text = "Ready?";
                }
                else if (timeLeft == 2)
                {
                    DynamicText.Text = "Fight!";
                }
            }
        }
        else
        {
            if (currentOption == AlertPropertiesEnums.RoundFinished)
            {
                CheckForFinishGame(false);
                ShowAlert(new AlertProperties(this, AlertPropertiesEnums.RoundStarting));
            }
            else if (currentOption == AlertPropertiesEnums.RoundStarting)
            {
                StartingNewRound();
                HideAlert();
            }
            else if (currentOption == AlertPropertiesEnums.HumanPlayerWasted)
            {
                // пусто
                HideAlert();
            }
        }
    }

    private void HideAlert()
    {
        Hide();
        timer.Stop();
        AlertNotShowed();
    }

    // Public methods //

    public void SetupGame()
    {
        ShowAlert(new AlertProperties(this, AlertPropertiesEnums.RoundStarting));
    }

    public void ShowWastedAlert(Player player)
    { 

        ShowAlert(new AlertProperties(this, AlertPropertiesEnums.HumanPlayerWasted, player));
    }

    public void ShowAlert(AlertProperties option)
    {
        currentOption = option.Option;
        timeLeft = (int)option.Option;

        switch(currentOption) 
        {
            case AlertPropertiesEnums.RoundStarting:
            {
                DynamicText.Text = timeLeft.ToString();
                DynamicText.Location = new Point(0, 20);
                timer.Start();
                break;
            }
            case AlertPropertiesEnums.HumanPlayerWasted:
            {
                DynamicText.Text = "Wasted!";
                DynamicText.Location = new Point(0, 20);
                var player = option.Tag as Player;
                if (player.OwnedBuildings != 0) timer.Start();
                break;
            }
            case AlertPropertiesEnums.RoundFinished:
            {
                var player = option.Tag as Player;
                DynamicText.Location = new Point(0, 20);
                DynamicText.Size = new Size(100, 100);
                var sender = (TopbarControl)option.Sender;
                if (player == null)
                {
                    DynamicText.Text = string.Format("Ничья!");
                }
                else
                {
                    DynamicText.Text = string.Format("Player {0} win", player.Gang);
                    sender.roundsWinners[sender.numСompletedRound++] = player.Gang;
                }
                timer.Start();
                break;
            }
            case AlertPropertiesEnums.Pause:
            {
                DynamicText.Text = "Menu";
                foreach (var button in pauseMenu)
                    button.Visible = true;
                break;
            }
            default:
                throw new ArgumentException("Было создано уведомление с неправильными параметрами.");
        }

        Show();
        AlertShowed(option.Option);
    }

    // Control from keys

    public void ResumeToGameFromPause(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape && currentOption == AlertPropertiesEnums.Pause)
        {
            HideAlert();
        }
    }
}

// Auxiliary data types AlertProperties //

public enum AlertPropertiesEnums
{
    RoundStarting = 4,
    HumanPlayerWasted = 7,
    RoundFinished = 5,
    Pause = 1
}

public class AlertProperties
{
    public AlertPropertiesEnums Option { get; }
    public object Tag { get; }
    public object Sender { get; }

    public AlertProperties(object trueSender, AlertPropertiesEnums option, object sender = null)
    {
        Option = option;
        Tag = sender;
        Sender = trueSender;
    }
}