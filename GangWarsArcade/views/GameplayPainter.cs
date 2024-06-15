using GangWarsArcade.domain;
using GangWarsArcade.Properties;

namespace GangWarsArcade.views;

public class GameplayPainter
{
    public event Action InvalidateVisual;

    private readonly Map _map;

    private readonly Bitmap grass;
    private readonly Bitmap path;

    private FontFamily _fonts;

    public List<EntityAnimation> Animations = new();

    public Size CellSize { get; }

    public static readonly SolidBrush[] ColourValues = new[]
    {
        "#FF0000", "#00FF00", "#FF00FF", "#FFFF00", "#0000FF", "#00FFFF", "#000000",
        "#800000", "#008000", "#000080", "#808000", "#800080", "#008080", "#808080"
    }.Select(c => new SolidBrush(ColorTranslator.FromHtml(c))).ToArray();

    public GameplayPainter(Map map, FontFamily fonts)
    {
        _map = map;
        _fonts = fonts;

        // Load resources
        grass = Resource.Grass;
        path = Resource.Path;
        CellSize = grass.Size;
    }

    public void ResetMap()
    {
        Animations.Clear();
        _map.ResetMap();
        InvalidateVisual();
    }

    public void Update()
    {
        _map.Update();
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
        foreach (var building in _map.Buildings)
            g.DrawImage(building.Image,
                new Rectangle(building.Location.X * CellSize.Width, building.Location.Y * CellSize.Height, CellSize.Width * 2, CellSize.Height * 2));
    }

    private void DrawOwnedLocations(Graphics g)
    {
        foreach (var cell in _map.OwnedLocations.Values)
        {
            var cellLocation = new System.Drawing.Point(cell.Location.X * CellSize.Width, cell.Location.Y * CellSize.Height);
            var rect = new Rectangle(cellLocation, grass.Size);
            var color = ColourValues[(int)cell.Owner % ColourValues.Length];

            if (_map.Maze[cell.Location.X, cell.Location.Y].Cell == MapCellEnum.Wall)
            {
                rect = new Rectangle(cellLocation, grass.Size * 2);

                var format = new StringFormat();
                format.LineAlignment = StringAlignment.Near;
                format.Alignment = StringAlignment.Center;

                g.DrawString(IdentifyText(cell.Owner), new Font(_fonts, 80), new SolidBrush(Color.FromArgb(175, color.Color.R, color.Color.G, color.Color.B)), rect, format);
            }
            else
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(60, color.Color.R, color.Color.G, color.Color.B)),
                    rect);
            }
        }
    }

    private static string IdentifyText(Gang gang)
    {
        return gang switch
        {
            Gang.Green => "[",
            Gang.Pink => "¤",
            Gang.Yellow => "^",
            Gang.Blue => "]",
            _ => "",
        };
    }

    private void DrawEntity(Graphics g)
    {
        foreach (var animation in Animations)
            g.DrawImage(animation.Entity.Image,
                new Rectangle(animation.Location.X, animation.Location.Y, CellSize.Width, CellSize.Height));
    }

    private void RenderMap(Graphics g)
    {
        var width = _map.Maze.GetLength(0);
        var height = _map.Maze.GetLength(1);

        var cellWidth = grass.Size.Width;
        var cellHeight = grass.Size.Height;

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var image = _map.Maze[x, y].Image;
                g.DrawImage(image, new Rectangle(x * cellWidth, y * cellHeight, cellWidth, cellHeight));
            }
        }
    }
    #endregion
}
