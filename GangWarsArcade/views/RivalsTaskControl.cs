using GangWarsArcade.domain;
using Timer = System.Windows.Forms.Timer;

namespace GangWarsArcade.views;

public partial class RivalsTaskControl : UserControl
{
    public event Action ShowedLevel;
        
    private readonly Timer timer;
    private readonly RivalsScenePainter rivalsScenePainter;

    public RivalsTaskControl(LoadLevelControl levelControl)
    {
        InitializeComponent();

        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

        rivalsScenePainter = new RivalsScenePainter();
        Paint += rivalsScenePainter.Render;
        rivalsScenePainter.InvalidateVisual += Invalidate;
        rivalsScenePainter.ResizedControl += ResizeControl;
        rivalsScenePainter.LoadMaps(levelControl.Levels);

        levelControl.LevelChanged += ChangeLevel;

        timer = new Timer { Interval = 20 };
        timer.Tick += TimerTick;
        timer.Start();
    }

    private void ChangeLevel(Map newMap)
    {
        if (newMap.Chests.Length == 0)
        {
            Show();
            rivalsScenePainter.ChangeLevel(newMap);
            timer.Start();
            ShowedLevel();
        }
    }

    private void ResizeControl(int width, int height)
    {
        Width = width;
        Height = height;
    }

    private void TimerTick(object sender, EventArgs e)
    {
        rivalsScenePainter.Update();
    }
}