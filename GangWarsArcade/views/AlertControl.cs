using GangWarsArcade.domain;
using GangWarsArcade.Properties;
using Timer = System.Windows.Forms.Timer;
using Point = System.Drawing.Point;

namespace GangWarsArcade.views;

public partial class AlertControl : UserControl
{
    public event Action AlertShowed;
    public event Action AlertHided;

    public event Action CheckingForFinishGame;
    public event Action PreparingNewRound;

    private AlertProperties _properties;

    private readonly Timer timer;
    private int timeLeft;

    private readonly PictureBox _background;
    private readonly Label _backgroundTitle;

    private readonly PictureBox _trainingImage;
    private readonly PictureBox _trainingImage2;
    private readonly Label _trainingText;
    private readonly Label _trainingHighlight;
    private int _currentTrainingSlide;

    public AlertControl(Point location, Size size, FontFamily fonts)
    {
        InitializeComponent();
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Location = location;
        Size = size;

        _background = new PictureBox
        {
            Location = new Point(0, 0),
            Size = Size,
            Image = CreateGrayBackgroud(new Point(0, 0), Size)
        };

        _backgroundTitle = new Label
        {
            Size = new Size(800, 300),
            ForeColor = Color.Yellow,
            Font = new Font(fonts, 60),
            Location = new Point(Size.Width / 2 - 400, (Size.Height - 100) / 2 - 150),
        };
        _backgroundTitle.Hide();
        _background.Controls.Add(_backgroundTitle);

        // Initialize Training window
        _trainingHighlight = new Label();
        _trainingHighlight.Hide();
        Controls.Add(_trainingHighlight);

        _trainingImage = new PictureBox();
        _trainingImage.Hide();
        _background.Controls.Add(_trainingImage);

        _trainingImage2 = new PictureBox();
        _trainingImage2.Hide();
        _background.Controls.Add(_trainingImage2);

        _trainingText = new Label() { ForeColor = Color.White, Font = new Font(fonts, 20) };
        _trainingText.Hide();
        _background.Controls.Add(_trainingText);
        //

        Controls.Add(_background);

        // Initialize countdown timer
        timer = new Timer { Interval = 1000 };
        timer.Tick += TimerTick;

        Hide();
    }

    private static Bitmap CreateGrayBackgroud(Point position, Size size)
    {
        var grayBackground = new Bitmap(size.Width, size.Height);
        var g = Graphics.FromImage(grayBackground);
        g.FillRectangle(new SolidBrush(Color.FromArgb(150, Color.Gray.R, Color.Gray.G, Color.Gray.B)),
                new Rectangle(position, size));
        return grayBackground;
    }

    public void PrepareNewRound(int numRound) 
        => SetAlert(new AlertProperties(this, AlertOption.RoundStarting, numRound));

    public void ShowWastedAlert(Player player) 
        => SetAlert(new AlertProperties(this, AlertOption.HumanPlayerWasted, player));

    public void ShowFinishRoundAlert(int numRound, Player winner) 
        => SetAlert(new AlertProperties(numRound + 1, AlertOption.RoundFinished, winner));

    public void ShowPauseAlert() 
        => SetAlert(new AlertProperties(this, AlertOption.Pause));

    public void ShowTrainingAlert(Point highlightWindowPosition) 
        => SetAlert(new AlertProperties(this, AlertOption.Training, highlightWindowPosition));

    private void SetAlert(AlertProperties option)
    {
        _background.Show();

        _properties = option;
        var currentOption = option.Option;
        timeLeft = (int)option.Option;
        switch (currentOption)
        {
            case AlertOption.RoundStarting:
                SetupRoundStartingAlert((int)option.Tag);
                break;
            case AlertOption.Pause:
                break;
            case AlertOption.HumanPlayerWasted:
                SetupHumanPlayerWastedAlert((Player)option.Tag);
                break;
            case AlertOption.RoundFinished:
                SetupRoundFinishedAlert(option);
                break;
            case AlertOption.Training:
                SetupTrainingAlert((Point)option.Tag);
                break;
            default:
                throw new ArgumentException("Было создано уведомление с неправильными параметрами.");
        }
        ShowAlert();
    }

    private void SetupRoundStartingAlert(int numRound)
    {
        _backgroundTitle.Location = new Point(Size.Width / 2 - 400, (Size.Height - 100) / 2 - 150);
        _backgroundTitle.Text = $"Round {numRound}";
        _backgroundTitle.TextAlign = ContentAlignment.MiddleCenter;
        _backgroundTitle.Show();

        timer.Start();
    }

    private void SetupRoundFinishedAlert(AlertProperties option)
    {
        if (option.Tag is Player player)
            _backgroundTitle.Text = $"Round winner!\n{player.Gang}";
        else
            _backgroundTitle.Text = "Draw!";
        _backgroundTitle.Show();

        timer.Start();
    }

    private void SetupHumanPlayerWastedAlert(Player player)
    {
        _backgroundTitle.Text = "Wasted!";
        _backgroundTitle.Show();

        if (player.IsActive) timer.Start();
    }

    #region Game Training

    private void SetupTrainingAlert(Point highlightPosition)
    {
        Focus();

        // Initialize 0 slide
        _trainingHighlight.Size = new Size(100, 100);
        _trainingHighlight.Location = highlightPosition;
        _trainingHighlight.Show();

        var offsetX = highlightPosition.X + 500 >= Size.Width;
        var offsetY = highlightPosition.Y + 250 >= Size.Height;

        _trainingText.Location = new Point(
            offsetX ? highlightPosition.X - 100 - 185 : highlightPosition.X + 100,
            offsetY ? highlightPosition.Y - 125 : highlightPosition.Y);
        _trainingText.Size = new Size(500, 100);
        _trainingText.Text = "Your game character.\n\nSet the direction:";
        _trainingText.Show();

        _trainingImage.Image = Resource.wasd;
        _trainingImage.Location = new Point(
            offsetX ? highlightPosition.X - 100 - 150 : highlightPosition.X + 135,
            offsetY ? highlightPosition.Y - 25: highlightPosition.Y + 100);
        _trainingImage.Size = new Size(150, 150);
        _trainingImage.SizeMode = PictureBoxSizeMode.Zoom;
        _trainingImage.Show();
        //

        _background.MouseClick += FlipTrainingSlide;
    }

    private void FlipTrainingSlide(object sender, MouseEventArgs e)
    {
        _currentTrainingSlide++;
        switch (_currentTrainingSlide)
        {
            case 1:
                _trainingHighlight.Size = new Size(200, 100);
                _trainingHighlight.Location = new Point(570, 625);
                _trainingHighlight.Show();

                _trainingText.Location = new Point(450, 500);
                _trainingText.Size = new Size(500, 100);
                _trainingText.Text = "Your inventory.\nMouse click - Fire\nPress E - Use inventory item";

                _trainingImage.Image = Resource.mouse_left_click;
                _trainingImage.Location = new Point(470, 620);
                _trainingImage.Size = new Size(100, 100);

                _trainingImage2.Image = Resource.e;
                _trainingImage2.Location = new Point(780, 630);
                _trainingImage2.Size = new Size(75, 75);
                _trainingImage2.SizeMode = PictureBoxSizeMode.Zoom;
                _trainingImage2.Show();
                break;
            case 2:
                _trainingHighlight.Hide();

                _trainingText.Location = new Point(450, 50);
                _trainingText.Size = new Size(500, 200);
                _trainingText.Text = "The goal of the game - capture buildings or kill all opponents before the round is over.\n\nGood luck!";

                _trainingImage.Hide();

                _trainingImage2.Hide();
                break;
            case 3:
                MouseDown -= FlipTrainingSlide;

                _trainingHighlight.Hide();
                _trainingText.Hide();
                _trainingImage.Hide();
                _trainingImage2.Hide();
                PrepareNewRound(1);
                break;
        }
    }

    #endregion

    private void TimerTick(object? _, EventArgs __)
    {
        if (timeLeft > 0)
        {
            timeLeft--;
            switch (_properties.Option)
            {
                case AlertOption.RoundStarting:
                    _backgroundTitle.Text = timeLeft.ToString();
                    break;
                case AlertOption.HumanPlayerWasted:
                    if (_properties.Tag is Player player && !player.IsActive)
                    {
                        timer.Stop();
                        _backgroundTitle.Text = "Wasted!";
                    }
                    else if (timeLeft == 5)
                    {
                        _backgroundTitle.Text = "Ready?";
                    }
                    else if (timeLeft == 2)
                    {
                        _backgroundTitle.Text = "Fight!";
                    }
                    break;
                case AlertOption.RoundFinished:
                    break;
                default:
                    throw new ArgumentException("Было создано уведомление с неправильными параметрами.");
            }
        }
        else
        {
            switch (_properties.Option)
            {
                case AlertOption.RoundFinished:
                    CheckingForFinishGame();
                    PreparingNewRound();
                    PrepareNewRound((int)_properties.Sender);
                    break;
                case AlertOption.RoundStarting:
                    HideAlert();
                    break;
                case AlertOption.HumanPlayerWasted:
                    HideAlert();
                    break;
                default:
                    throw new ArgumentException("Было создано уведомление с неправильными параметрами.");
            }
        }
    }

    private void ShowAlert()
    {
        Show();
        if (_properties.Option != AlertOption.HumanPlayerWasted)
            AlertShowed();
    }

    private void HideAlert()
    {
        Hide();
        timer.Stop();
        if (_properties.Option != AlertOption.HumanPlayerWasted)
            AlertHided();
    }
}