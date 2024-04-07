using GangWarsArcade.domain;
using GangWarsArcade.Dungeons;

namespace GangWarsArcade.views;

public partial class LoadLevelControl : UserControl
{
    public event Action<Map> LevelChanged;

    public Map[] Levels;
        
    public LoadLevelControl()
    {
        InitializeComponent();

        //SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

        BackColor = Color.Gray;

        Levels = LoadLevels().ToArray();
        DrawLevelSwitch(Levels);
    }

    private static IEnumerable<Map> LoadLevels()
    {
        yield return Map.FromText(DungeonsLoader.Load(DungeonsName.D_Dungeon1));
        yield return Map.FromText(DungeonsLoader.Load(DungeonsName.D_Dungeon2));
        yield return Map.FromText(DungeonsLoader.Load(DungeonsName.D_Dungeon3));
        yield return Map.FromText(DungeonsLoader.Load(DungeonsName.D_Dungeon4));

        yield return Map.FromText(DungeonsLoader.Load(DungeonsName.R_Dungeon1));
        yield return Map.FromText(DungeonsLoader.Load(DungeonsName.R_Dungeon2));
        yield return Map.FromText(DungeonsLoader.Load(DungeonsName.R_Dungeon3));
        yield return Map.FromText(DungeonsLoader.Load(DungeonsName.R_Dungeon4));
        yield return Map.FromText(DungeonsLoader.Load(DungeonsName.R_Dungeon5));
    }

    private void DrawLevelSwitch(Map[] levels)
    {
        var buttons = new List<Button>();
        for (var i = 0; i < levels.Length; i++)
        {
            var level = levels[i];
            var link = new Button
            {
                Text = $"Level {i + 1}",
                BackColor = Color.LimeGreen,
                Location = new System.Drawing.Point(i * 100, 15),
                Tag = level
            };
            link.Click += (_, __) =>
            {
                LevelChanged(level);
                UpdateButtonsColors(level, buttons);
            };
            buttons.Add(link);
            Controls.Add(link);
        }

        UpdateButtonsColors(levels[0], buttons);
    }

    private void UpdateButtonsColors(Map level, List<Button> buttons)
    {
        foreach (var button in buttons)
            button.BackColor = button.Tag == level ? Color.LimeGreen : Color.White;
    }
}