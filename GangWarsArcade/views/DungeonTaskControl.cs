using GangWarsArcade.domain;
using Timer = System.Windows.Forms.Timer;

namespace GangWarsArcade.views;

public partial class DungeonTaskControl : UserControl
{
    public event Action ShowedLevel;

    private readonly Timer timer;
    private readonly DungeonScenePainter DungeonScenePainter;

    public DungeonTaskControl(LoadLevelControl levelControl)
    {
        InitializeComponent();

        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

        //Title = "Click on any empty cell to run BFS";
        DungeonScenePainter = new DungeonScenePainter();
        Paint += DungeonScenePainter.Render;
        MouseDown += DungeonScenePainter.OnPointerPressed;
        MouseUp += DungeonScenePainter.OnPointerReleased;
        DungeonScenePainter.InvalidateVisual += Invalidate;
        DungeonScenePainter.Load(levelControl.Levels);

        levelControl.LevelChanged += ChangeLevel;

        timer = new Timer { Interval = 50 };
        timer.Tick += TimerTick;
        timer.Start();
    }

    private void ChangeLevel(Map newMap)
    {
        if (newMap.Chests.Length > 0)
        {
            Show();
            DungeonScenePainter.ChangeLevel(newMap);
            timer.Start();
            ShowedLevel();
        }
    }

    private void TimerTick(object sender, EventArgs e)
    {
        DungeonScenePainter.Update();
    }
}