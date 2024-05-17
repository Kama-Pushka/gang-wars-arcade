using GangWarsArcade.domain;
using GangWarsArcade.Properties;
using System.Drawing.Text;

namespace GangWarsArcade.views;

public class GameplayPainter
{
    public event Action InvalidateVisual;

    private readonly Map _map;

    private readonly Bitmap grass;
    private readonly Bitmap path;

    private PrivateFontCollection _fonts;

    public Size CellSize => grass.Size;

    public static readonly SolidBrush[] ColourValues = new[]
    {
        "#FF0000", "#00FF00", "#FF00FF", "#FFFF00", "#0000FF", "#00FFFF", "#000000",
        "#800000", "#008000", "#000080", "#808000", "#800080", "#008080", "#808080"
    }.Select(c => new SolidBrush(ColorTranslator.FromHtml(c))).ToArray();

    public GameplayPainter(Map map, PrivateFontCollection fonts)
    {
        _map = map;
        _fonts = fonts;

        // Load resources
        grass = Resource.Grass;
        path = Resource.Path;
    }

    public void ResetMap()
    {
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

            if (_map.Maze[cell.Location.X, cell.Location.Y] == MapCell.Wall)
            {
                rect = new Rectangle(cellLocation, grass.Size * 2);

                var format = new StringFormat();
                format.LineAlignment = StringAlignment.Near;
                format.Alignment = StringAlignment.Center;

                g.DrawString(IdentifyText(cell.Owner), new Font(_fonts.Families[0], 80), new SolidBrush(Color.FromArgb(175, color.Color.R, color.Color.G, color.Color.B)), rect, format);
            }
            else 
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(100, color.Color.R, color.Color.G, color.Color.B)),
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
        foreach (var entity in _map.Entities.Where(e => e is not Player))
            g.DrawImage(entity.Image,
                new Rectangle(entity.Position.X * CellSize.Width, entity.Position.Y * CellSize.Height, CellSize.Width, CellSize.Height));

        foreach (var entity in _map.Entities.Where(e => e is Player))
        {
            g.DrawImage(entity.Image,
                new Rectangle(entity.Position.X * CellSize.Width, entity.Position.Y * CellSize.Height, CellSize.Width, CellSize.Height));
            
            var player = (Player)entity;
            if (player.DamageColor != null)
            {
                g.FillRectangle(player.DamageColor, 
                    player.Position.X * CellSize.Width, player.Position.Y * CellSize.Height, CellSize.Width, CellSize.Height);
                player.DamageColor = null;
            }
        }
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
                var image = _map.Maze[x, y] == MapCell.Wall ? grass : path;
                g.DrawImage(image, new Rectangle(x * cellWidth, y * cellHeight, cellWidth, cellHeight));
            }
        }
    }
    #endregion
}
