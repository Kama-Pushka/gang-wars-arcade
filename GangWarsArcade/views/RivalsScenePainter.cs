using GangWarsArcade.domain;
using GangWarsArcade.Properties;

namespace GangWarsArcade.views;

public class RivalsScenePainter
{
    public event Action InvalidateVisual;
    public event Action<int, int> ResizedControl;

    public Size Size => new(currentMap.Maze.GetLength(0) * CellSize.Width,
        currentMap.Maze.GetLength(1) * CellSize.Height);

    private Map currentMap;
    private int currentIteration;
    private Dictionary<Map, List<OwnedLocation>> mapStates;
    private Bitmap grass;
    private Bitmap path;
    private Size CellSize => grass.Size;

    private static readonly SolidBrush[] colourValues = new[]
    {
        "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#FF00FF", "#00FFFF", "#000000",
        "#800000", "#008000", "#000080", "#808000", "#800080", "#008080", "#808080"
    }.Select(c => new SolidBrush(ColorTranslator.FromHtml(c))).ToArray();

    public RivalsScenePainter()
    {
        LoadResources();
    }

    public void LoadMaps(Map[] maps)
    {
        currentMap = maps[0];
        PlayLevels(maps);
        currentIteration = 0;
    }

    private void LoadResources()
    {
        grass = Resource.Grass;
        path = Resource.Path;
    }

    private void PlayLevels(Map[] maps)
    {
        mapStates = new Dictionary<Map, List<OwnedLocation>>();
        foreach (var map in maps)
            mapStates[map] = RivalsTask.AssignOwners(map).ToList();
    }

    public void ChangeLevel(Map newMap)
    {
        currentMap = newMap;
        currentIteration = 0;
        ResizedControl(Size.Width, Size.Height);
        InvalidateVisual();
    }

    public void Update()
    {
        currentIteration = Math.Min(currentIteration + 1, mapStates[currentMap].Count);
        InvalidateVisual();
    }

    public void Render(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;

        RenderMap(g);
        DrawPath(g);
    }

    private void DrawPath(Graphics g)
    {
        var mapState = mapStates[currentMap];
        var textTop = new System.Drawing.Point(0, (int)0.25 * CellSize.Height);

        foreach (var cell in mapState.Take(currentIteration))
        {
            var cellLocation = new System.Drawing.Point(cell.Location.X * CellSize.Width, cell.Location.Y * CellSize.Height);
            var rect = new Rectangle(cellLocation, CellSize);
            var color = colourValues[cell.Owner % colourValues.Length];

            g.FillRectangle(new SolidBrush(Color.FromArgb(100, color.Color.R, color.Color.G, color.Color.B)),
                rect);

            //var formattedText = new FormattedText(
            //    cell.Distance.ToString(),
            //    typeface,
            //    fontSize,
            //    TextAlignment.Center,
            //    TextWrapping.NoWrap,
            //    CellSize);

            var location = new domain.Point(cellLocation.X + textTop.X, cellLocation.Y + textTop.Y);

            g.DrawString(cell.Distance.ToString(), new Font("Segoe UI Light", 26), Brushes.Beige, location.X, location.Y);
        }
    }

    private void RenderMap(Graphics g)
    {
        var dungeonWidth = currentMap.Maze.GetLength(0);
        var dungeonHeight = currentMap.Maze.GetLength(1);

        var cellWidth = CellSize.Width;
        var cellHeight = CellSize.Height;

        for (var x = 0; x < dungeonWidth; x++)
        {
            for (var y = 0; y < dungeonHeight; y++)
            {
                var image = currentMap.Maze[x, y] == MapCell.Empty ? path : grass;
                g.DrawImage(image, new Rectangle(x * cellWidth, y * cellHeight, cellWidth, cellHeight));
            }
        }
    }
}
