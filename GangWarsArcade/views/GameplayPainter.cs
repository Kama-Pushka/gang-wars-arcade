using GangWarsArcade.domain;
using GangWarsArcade.Properties;
using Point = GangWarsArcade.domain.Point;

namespace GangWarsArcade.views;

public class GameplayPainter
{
    public event Action InvalidateVisual;

    public readonly Map CurrentMap;     
    private Bitmap grass;
    private Bitmap path;
    private Bitmap peasant;
    private Bitmap castle;
    private Bitmap chest;

    private Size CellSize => grass.Size;

    public static readonly SolidBrush[] colourValues = new[]     {
        "#FF0000", "#00FF00", "#FF00FF", "#FFFF00", "#0000FF", "#00FFFF", "#000000",
        "#800000", "#008000", "#000080", "#808000", "#800080", "#008080", "#808080"
    }.Select(c => new SolidBrush(ColorTranslator.FromHtml(c))).ToArray();

    public GameplayPainter(Map map)
    {
        CurrentMap = map;
        
                grass = Resource.Grass;
        path = Resource.Path;
        peasant = Resource.Peasant;
        castle = Resource.Castle;
        chest = Resource.Chest;
    }

    public void ResetMap()
    {
        CurrentMap.ResetMap();          InvalidateVisual();
    }

    public void Update()
    {
        CurrentMap.Update();
        InvalidateVisual();
    }

    #region Paint Scene
    public void Render(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;

        DrawLevel(g);
        DrawOwnedLocations(g);
        DrawEntity(g);
    }

    private void DrawLevel(Graphics g)
    {
        RenderMap(g);
        foreach (var building in CurrentMap.Buildings)
            g.DrawImage(building.Image,
                new Rectangle(building.Location.X * CellSize.Width, building.Location.Y * CellSize.Height, CellSize.Width * 2, CellSize.Height * 2));
    }

    private void DrawOwnedLocations(Graphics g)
    {
        foreach (var cell in CurrentMap.OwnedLocations.Values)
        {
            var cellLocation = new System.Drawing.Point(cell.Location.X * CellSize.Width, cell.Location.Y * CellSize.Height);
            var rect = new Rectangle(cellLocation, grass.Size);
            var color = colourValues[(int)cell.Owner % colourValues.Length];

            g.FillRectangle(new SolidBrush(Color.FromArgb(100, color.Color.R, color.Color.G, color.Color.B)),
                rect);
        }
    }

    private void DrawEntity(Graphics g)
    {
        foreach (var entity in CurrentMap.Entities.Where(e => e is not Player))
            g.DrawImage(entity.Image,
                new Rectangle(entity.Position.X * CellSize.Width, entity.Position.Y * CellSize.Height, CellSize.Width, CellSize.Height));

        foreach (var entity in CurrentMap.Entities.Where(e => e is Player))
        {
            g.DrawImage(entity.Image,
                new Rectangle(entity.Position.X * CellSize.Width, entity.Position.Y * CellSize.Height, CellSize.Width, CellSize.Height));
            
            var player = (Player)entity;
            if (player.DamageColor != null)
            {
                g.FillRectangle(player.DamageColor, 
                    player.Position.X * CellSize.Width, player.Position.Y * CellSize.Height, CellSize.Width, CellSize.Height);                 player.DamageColor = null;
            }
        }

                foreach (var player in CurrentMap.Players.Values.Where(p => p.IsAlive))
        {

            
        }
    }

    private void DrawPath(Graphics g, Color color, IEnumerable<Point> path)
    {
        var points = path.Select(x =>
            new PointF(x.X * CellSize.Width + CellSize.Width * 0.5f, x.Y * CellSize.Height + CellSize.Height * 0.5f)).ToArray();
        var pen = new Pen(color, CellSize.Height * 0.125f)
        {
            DashPattern = [CellSize.Width * 0.075f, CellSize.Height * 0.025f]         };
        for (var i = 0; i < points.Length - 1; i++)
            g.DrawLine(pen, points[i], points[i + 1]);
    }

    private void RenderMap(Graphics g)
    {
        var width = CurrentMap.Maze.GetLength(0);
        var height = CurrentMap.Maze.GetLength(1);

        var cellWidth = grass.Size.Width;
        var cellHeight = grass.Size.Height;

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var image = CurrentMap.Maze[x, y] == MapCell.Wall ? grass : path;
                g.DrawImage(image, new Rectangle(x * cellWidth, y * cellHeight, cellWidth, cellHeight));
            }
        }
    }
    #endregion
}
