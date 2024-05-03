using GangWarsArcade.domain;
using Timer = System.Windows.Forms.Timer;
using Point = System.Drawing.Point;

namespace GangWarsArcade.views;

public partial class AlertControl : UserControl
{
    public event Action AlertShowed;
    public event Action AlertHided;

    public event Action<bool> CheckForFinishGame;
    public event Action StartingNewRound;

    private readonly Timer timer;
    private int timeLeft;

    private AlertPropertiesEnums currentOption;
    private Label DynamicText;

    private Button[] pauseMenu;

    public AlertControl()
    {
        InitializeComponent();

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
            switch (currentOption)
            {
                case AlertPropertiesEnums.RoundStarting:
                    DynamicText.Text = timeLeft.ToString();
                    break;
                case AlertPropertiesEnums.HumanPlayerWasted:
                    if (timeLeft == 5)
                    {
                        DynamicText.Text = "Ready?";
                    }
                    else if (timeLeft == 2)
                    {
                        DynamicText.Text = "Fight!";
                        // TODO сделать серый фон, но здесь он убирается
                    }
                    break;
                case AlertPropertiesEnums.RoundFinished:
                    break;
                default:
                    throw new ArgumentException("Было создано уведомление с неправильными параметрами.");
            }
        }
        else
        {
            switch (currentOption)
            {
                case AlertPropertiesEnums.RoundFinished:
                    CheckForFinishGame(false); 
                    StartingNewRound();
                    SetAlert(new AlertProperties(this, AlertPropertiesEnums.RoundStarting));
                    break;
                case AlertPropertiesEnums.RoundStarting:
                    HideAlert();
                    break;
                case AlertPropertiesEnums.HumanPlayerWasted:
                    // пусто
                    HideAlert();
                    break;
                default:
                    throw new ArgumentException("Было создано уведомление с неправильными параметрами.");
            }
        }
    }

    private void HideAlert()
    {
        Hide();
        timer.Stop();
        AlertHided();
    }

    // Public methods //

    public void SetupGame()
    {
        SetAlert(new AlertProperties(this, AlertPropertiesEnums.RoundStarting));
    }

    public void ShowWastedAlert(Player player)
    { 
        SetAlert(new AlertProperties(this, AlertPropertiesEnums.HumanPlayerWasted, player));
    }

    public void SetAlert(AlertProperties option)
    {
        currentOption = option.Option;
        timeLeft = (int)option.Option;
        switch(currentOption) 
        {
            case AlertPropertiesEnums.RoundStarting:
                SetupRoundStartingAlert();
                break;
            case AlertPropertiesEnums.HumanPlayerWasted:
                SetupHumanPlayerWastedAlert((Player)option.Tag);
                break;
            case AlertPropertiesEnums.RoundFinished:
                SetupRoundFinishedAlert(option);
                break;
            case AlertPropertiesEnums.Pause:
                SetupPauseAlert();
                break;
            default:
                throw new ArgumentException("Было создано уведомление с неправильными параметрами.");
        }
        ShowAlert();
    }

    private void SetupRoundStartingAlert()
    {
        DynamicText.Text = timeLeft.ToString();
        DynamicText.Location = new Point(0, 20);
        timer.Start();
    }

    private void SetupHumanPlayerWastedAlert(Player player)
    {
        DynamicText.Text = "Wasted!";
        DynamicText.Location = new Point(0, 20);
        if (player.IsActive) timer.Start(); // else он не может возродится и до конца раунда должен висеть текст Wasted!
    }

    private void SetupRoundFinishedAlert(AlertProperties option)
    {
        var player = option.Tag as Player;
        DynamicText.Location = new Point(0, 20);
        DynamicText.Size = new Size(100, 100);
        
        var sender = (GameState)option.Sender;
        if (player == null)
        {
            DynamicText.Text = string.Format("Ничья!");
        }
        else
        {
            DynamicText.Text = string.Format("Player {0} win", player.Gang);
            sender.RoundsWinners[sender._numСompletedRound++] = player.Gang;
        }
        timer.Start();
    }

    private void SetupPauseAlert()
    {
        DynamicText.Text = "Menu";
        foreach (var button in pauseMenu)
            button.Visible = true;
    }

    private void ShowAlert()
    {
        Show();
        if (currentOption != AlertPropertiesEnums.HumanPlayerWasted) AlertShowed();
    }

    // Control from keys (типа методы которые запускаются если человек тыкает) //

    public void ResumeToGameFromPause(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape && currentOption == AlertPropertiesEnums.Pause)
        {
            HideAlert();
        }
    }
}

// Auxiliary data types AlertProperties //
// TODO вынести в отдельные файлы

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