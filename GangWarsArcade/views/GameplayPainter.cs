using GangWarsArcade.domain;
using GangWarsArcade.Properties;
using System.Drawing;
using System.Numerics;
using Point = GangWarsArcade.domain.Point;

namespace GangWarsArcade.views;

public class GameplayPainter
{
    public event Action InvalidateVisual;

    //private Dictionary<Map, Point[]> paths;
    private Map currentMap;
    private int mainIteration;

    private Point lastMouseClick;
    private IEnumerable<List<Point>> pathsToChests;
    private Bitmap grass;
    private Bitmap path;
    private Bitmap peasant;
    private Bitmap castle;
    private Bitmap chest;

    public Size CellSize => grass.Size;

    public static readonly SolidBrush[] colourValues = new[]
    {
        "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#FF00FF", "#00FFFF", "#000000",
        "#800000", "#008000", "#000080", "#808000", "#800080", "#008080", "#808080"
    }.Select(c => new SolidBrush(ColorTranslator.FromHtml(c))).ToArray();

    public GameplayPainter()
    {
        // Load resources
        grass = Resource.Grass;
        path = Resource.Path;
        peasant = Resource.Peasant;
        castle = Resource.Castle;
        chest = Resource.Chest;
    }

    public void Load(Map map)
    {
        currentMap = map;
        mainIteration = 0;
    }

    public void ResetMap()
    {
        currentMap.ResetMap();
        mainIteration = 0;
        lastMouseClick = null;
        pathsToChests = null;
        InvalidateVisual();
    }

    public void Update()
    {
        ++mainIteration;
        currentMap.Update();
        InvalidateVisual();
    }

    public void OnPointerPressed(object sender, MouseEventArgs e)
    {
        var location = e.Location;
        var position = new Point((location.X / CellSize.Width), (location.Y / CellSize.Height));

        lastMouseClick = position;
        pathsToChests = null;
        if (!currentMap.InBounds(position) ||
            currentMap.Maze[lastMouseClick.X, lastMouseClick.Y] != MapCell.Empty) return;

        pathsToChests = BfsTask.FindPaths(currentMap, lastMouseClick, currentMap.Items.Keys.ToArray())
            .Select(x => x.ToList()).ToList();

        foreach (var pathsToChest in pathsToChests)
            pathsToChest.Reverse();
    }

    public void OnPointerReleased(object sender, MouseEventArgs e)
    {
        pathsToChests = null;
    }

    // Paint Scene //

    public void Render(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;

        DrawLevel(g);
        DrawOwnedLocations(g);
        DrawEntity(g, mainIteration);
        
        if (pathsToChests != null && lastMouseClick.HasValue)
            DrawAdditionalPaths(g, lastMouseClick);
    }

    private void DrawLevel(Graphics g)
    {
        RenderMap(g);
        foreach (var itemPoint in currentMap.Items.Keys)
            g.DrawImage(chest,
                new Rectangle(itemPoint.X * CellSize.Width, itemPoint.Y * CellSize.Height, CellSize.Width, CellSize.Height));
        foreach (var building in currentMap.Buildings)
            g.DrawImage(castle,
                new Rectangle(building.X * CellSize.Width, building.Y * CellSize.Height, CellSize.Width, CellSize.Height));
        foreach (var trap in currentMap.Traps.Keys)
            g.DrawImage(chest,
                new Rectangle(trap.X * CellSize.Width, trap.Y * CellSize.Height, CellSize.Width, CellSize.Height));
    }

    private void DrawOwnedLocations(Graphics g)
    {
        foreach (var cell in currentMap.OwnedLocations.Values)
        {
            var cellLocation = new System.Drawing.Point(cell.Location.X * CellSize.Width, cell.Location.Y * CellSize.Height);
            var rect = new Rectangle(cellLocation, grass.Size);
            var color = colourValues[(int)cell.Owner % colourValues.Length];

            g.FillRectangle(new SolidBrush(Color.FromArgb(100, color.Color.R, color.Color.G, color.Color.B)),
                rect);
        }
    }

    private void DrawEntity(Graphics g, int interation)
    {
        foreach (var player in currentMap.Players.Values.Where(p => p.IsAlive))
        {
            g.DrawImage(player.Image,
                new Rectangle(player.Position.X * CellSize.Width, player.Position.Y * CellSize.Height, CellSize.Width, CellSize.Height));

            if (player.DamageColor != null)
            {
                g.FillRectangle(player.DamageColor, player.Position.X * CellSize.Width, player.Position.Y * CellSize.Height, CellSize.Width, CellSize.Height); // TODO прозрачность?
                player.DamageColor = null;
            }
        }
        foreach (var player in currentMap.Bullets)
            g.DrawImage(chest,
                new Rectangle(player.Position.X * CellSize.Width, player.Position.Y * CellSize.Height, CellSize.Width, CellSize.Height));
    }

    private void DrawAdditionalPaths(Graphics g, Point lastClick)
    {
        g.FillRectangle(Brushes.Red,
            new Rectangle(lastClick.X * CellSize.Width, lastClick.Y * CellSize.Height, CellSize.Width, CellSize.Height));
        foreach (var pathToChest in pathsToChests)
            DrawPath(g, Color.Red, pathToChest);
    }

    private void DrawPath(Graphics g, Color color, IEnumerable<Point> path)
    {
        var points = path.Select(x =>
            new PointF(x.X * CellSize.Width + CellSize.Width * 0.5f, x.Y * CellSize.Height + CellSize.Height * 0.5f)).ToArray();
        var pen = new Pen(color, CellSize.Height * 0.125f)
        {
            DashPattern = [CellSize.Width * 0.075f, CellSize.Height * 0.025f] //var newStyle = new DashStyle(new[] { CellSize.Width * 0.125f, CellSize.Height * 0.125f }, 1d);
        };
        for (var i = 0; i < points.Length - 1; i++)
            g.DrawLine(pen, points[i], points[i + 1]);
    }

    private void RenderMap(Graphics g)
    {
        var width = currentMap.Maze.GetLength(0);
        var height = currentMap.Maze.GetLength(1);

        var cellWidth = grass.Size.Width;
        var cellHeight = grass.Size.Height;

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var image = currentMap.Maze[x, y] == MapCell.Wall ? grass : path;
                g.DrawImage(image, new Rectangle(x * cellWidth, y * cellHeight, cellWidth, cellHeight));
            }
        }
    }
}
