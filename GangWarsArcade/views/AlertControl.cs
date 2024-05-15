using GangWarsArcade.domain;
using Timer = System.Windows.Forms.Timer;
using Point = System.Drawing.Point;
using System.Drawing.Text;
using GangWarsArcade.Properties;

namespace GangWarsArcade.views;

public partial class AlertControl : UserControl
{
    public event Action AlertShowed;     public event Action AlertHided; 
    public event Action CheckingForFinishGame;     public event Action PreparingNewRound;

    private readonly Timer timer;
    private int timeLeft;

    private AlertProperties _option;
    private AlertPropertiesEnums currentOption;

    private readonly PictureBox _background;
    private readonly Label _trainingText;
    private readonly Label _title; 
    private readonly PictureBox _trainingImage;     private readonly PictureBox _trainingImage2;     private Label _trainingSlide;     private int _numTrainingSlide;

    public AlertControl(Point location, Size size, PrivateFontCollection fonts)
    {
        InitializeComponent();
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);

                BackColor = Color.Transparent;
        Location = location;
        Size = size;
        
        _trainingSlide = new Label();
        _trainingSlide.Hide();         Controls.Add(_trainingSlide);

        _background = new PictureBox
        {
            Location = new Point(0, 0),
            Size = Size,
            Image = DrawGrayBackgroud(new Point(0, 0), Size)
        };
        _background.Visible = false; 
        _trainingImage = new PictureBox();
        _trainingImage.Hide();
        _background.Controls.Add(_trainingImage);

        _trainingImage2 = new PictureBox();
        _trainingImage2.Hide();
        _background.Controls.Add(_trainingImage2);

        _trainingText = new Label() { ForeColor = Color.White, Font = new Font(fonts.Families[0], 20) };
        _trainingText.Hide();
        _background.Controls.Add(_trainingText);

        _title = new Label() { Size = new Size(800, 300), ForeColor = Color.Yellow, Font = new Font(fonts.Families[0], 60) };         _title.Location = new Point(Size.Width / 2 - 400, (Size.Height - 100) / 2 - 150);         _background.Controls.Add(_title);

        Controls.Add(_background);

                
                timer = new Timer { Interval = 1000 };
        timer.Tick += TimerTick;

        Hide();
    }

    private Bitmap DrawGrayBackgroud(Point position, Size size)     {         var grayBackground = new Bitmap(size.Width, size.Height);

        var g = Graphics.FromImage(grayBackground);
        g.FillRectangle(new SolidBrush(Color.FromArgb(150, Color.Gray.R, Color.Gray.G, Color.Gray.B)),
                new Rectangle(position, size));
        return grayBackground;
    }

    public void PrepareNewRound(int numRound)     {
        SetAlert(new AlertProperties(this, AlertPropertiesEnums.RoundStarting, numRound));
    }

    public void ShowFinishRoundAlert(int numRound, Player winner)
    {
        SetAlert(new AlertProperties(numRound + 1, AlertPropertiesEnums.RoundFinished, winner));     }

    public void ShowPauseAlert()
    {
        SetAlert(new AlertProperties(this, AlertPropertiesEnums.Pause));
    }

    public void ShowTrainingAlert()
    {
        SetAlert(new AlertProperties(this, AlertPropertiesEnums.Training));
    }

    private void SetAlert(AlertProperties option)     {
        _title.Visible = true; 
        _option = option;
        currentOption = option.Option;
        timeLeft = (int)option.Option;
        switch (currentOption)
        {
            case AlertPropertiesEnums.RoundStarting:
                SetupRoundStartingAlert((int)option.Tag);
                break;
            case AlertPropertiesEnums.Pause:
                SetupPauseAlert();
                break;
            case AlertPropertiesEnums.HumanPlayerWasted:
                SetupHumanPlayerWastedAlert((Player)option.Tag);
                break;
            case AlertPropertiesEnums.RoundFinished:
                SetupRoundFinishedAlert(option);
                break;
            case AlertPropertiesEnums.Training:
                SetupTrainingAlert();
                break;
            default:
                throw new ArgumentException("Было создано уведомление с неправильными параметрами.");         }
        ShowAlert();
    }

    private void SetupRoundStartingAlert(int numRound)
    {
        _background.Visible = true;

        _title.Location = new Point(Size.Width / 2 - 400, (Size.Height - 100) / 2 - 150);         _title.Text = $"Round {numRound}";         _title.TextAlign = ContentAlignment.MiddleCenter;
        timer.Start();
    }

    private void SetupPauseAlert()
    {
        _background.Visible = true;
        _title.Visible = false;
    }

    private void SetupRoundFinishedAlert(AlertProperties option)
    {
        _background.Visible = true;

        var player = option.Tag as Player;

                if (player == null)
        {
            _title.Text = string.Format("Draw!");
        }
        else
        {
            _title.Text = string.Format("Round winner!\n{0}", player.Gang);
        }
        timer.Start();
    }

    private void SetupTrainingAlert()
    {
        _background.Visible = true;
        Focus();

        _background.MouseClick += FlipTrainingSlide;         FlipTrainingSlide(null, null);     }

    private void FlipTrainingSlide(object sender, MouseEventArgs e)
    {
        _numTrainingSlide++;
        switch (_numTrainingSlide)
        {
            case 1:
                _trainingSlide.Size = new Size(100, 100);
                _trainingSlide.Location = new Point(50, 50);
                _trainingSlide.Show();

                _trainingText.Location = new Point(150, 50);                 _trainingText.Size = new Size(500, 100);
                _trainingText.Text = "Your game character.\n\nSet the direction:";
                _trainingText.Show();

                _trainingImage.Image = Resource.wasd;
                _trainingImage.Location = new Point(185, 150);
                _trainingImage.Size = new Size(150, 150);
                _trainingImage.SizeMode = PictureBoxSizeMode.Zoom;
                _trainingImage.Show();
                break;
            case 2:
                _trainingSlide.Size = new Size(200, 100);
                _trainingSlide.Location = new Point(570, 625);
                _trainingSlide.Show();

                _trainingText.Location = new Point(450, 500);                 _trainingText.Size = new Size(500, 100);
                _trainingText.Text = "Your inventory.\nMouse click - Fire\nPress E - Use inventory item";
                _trainingText.Show();

                _trainingImage.Image = Resource.mouse_left_click;
                _trainingImage.Location = new Point(470, 620);
                _trainingImage.Size = new Size(100, 100);
                                _trainingImage.Show();

                _trainingImage2.Image = Resource.e;
                _trainingImage2.Location = new Point(780, 630);
                _trainingImage2.Size = new Size(75, 75);
                _trainingImage2.SizeMode = PictureBoxSizeMode.Zoom;
                _trainingImage2.Show();
                break;
            case 3:
                _trainingSlide.Hide();

                _trainingText.Location = new Point(450, 50);                 _trainingText.Size = new Size(500, 200);
                _trainingText.Text = "The goal of the game - capture buildings or kill all opponents before the round is over.\n\nGood luck!";
                _trainingText.Show(); 
                _trainingImage.Hide();

                _trainingImage2.Hide();
                break;
            case 4:
                MouseDown -= FlipTrainingSlide;
                _trainingSlide.Hide();
                _trainingText.Hide();
                _trainingImage.Hide();
                _trainingImage2.Hide();
                PrepareNewRound(1);
                break;
        }
    }

    private void TimerTick(object sender, EventArgs e)
    {
        if (timeLeft > 0)
        {
            timeLeft--;
            switch (currentOption)
            {
                case AlertPropertiesEnums.RoundStarting:
                    _title.Text = timeLeft.ToString();
                    break;
                case AlertPropertiesEnums.HumanPlayerWasted:                     if (timeLeft == 5)
                    {
                        _title.Text = "Ready?";
                    }
                    else if (timeLeft == 2)
                    {
                        _title.Text = "Fight!";
                                            }
                    break;
                case AlertPropertiesEnums.RoundFinished:                     break;
                default:                     throw new ArgumentException("Было создано уведомление с неправильными параметрами.");             }
        }
        else
        {
            switch (currentOption)
            {
                case AlertPropertiesEnums.RoundFinished:
                                        CheckingForFinishGame();                       PreparingNewRound();
                    PrepareNewRound((int)_option.Sender);                     break;
                case AlertPropertiesEnums.RoundStarting:
                    HideAlert();
                    break;
                case AlertPropertiesEnums.HumanPlayerWasted:
                                        HideAlert();                     break;
                default:
                    throw new ArgumentException("Было создано уведомление с неправильными параметрами.");             }
        }
    }

    private void ShowAlert()
    {
        Show();
        if (currentOption != AlertPropertiesEnums.HumanPlayerWasted)             AlertShowed();
    }

    private void HideAlert()
    {
        
        Hide();
        timer.Stop();         if (currentOption != AlertPropertiesEnums.HumanPlayerWasted)             AlertHided();
    }

        
    public void ShowWastedAlert(Player player)
    { 
        
        SetAlert(new AlertProperties(this, AlertPropertiesEnums.HumanPlayerWasted, player));
    }

    private void SetupHumanPlayerWastedAlert(Player player)
    {
        _title.Text = "Wasted!";
        if (player.IsActive) timer.Start();     }
}


public enum AlertPropertiesEnums {
    RoundStarting = 4,     HumanPlayerWasted = 7,
    RoundFinished = 5,
    Pause = 1,
    Training = 2,
}

public class AlertProperties
{
    public AlertPropertiesEnums Option { get; }
    public object Tag { get; }
    public object Sender { get; } 
    public AlertProperties(object trueSender, AlertPropertiesEnums option, object sender = null)     {
        Option = option;
        Tag = sender;
        Sender = trueSender;
    }
}