using GangWarsArcade.domain;
using GangWarsArcade.Properties;
using Point = GangWarsArcade.domain.Point;

namespace GangWarsArcade.views
{
    public class DungeonScenePainter
    {
        public event Action InvalidateVisual;

        private Dictionary<Map, Point[]> paths;
        private Map currentMap;

        private int mainIteration;

        private Point lastMouseClick;
        private IEnumerable<List<Point>> pathsToChests;
        private Bitmap grass;
        private Bitmap path;
        private Bitmap peasant;
        private Bitmap castle;
        private Bitmap chest;

        private int cellWidth => grass.Size.Width;
        private int cellHeight => grass.Size.Height;

        public DungeonScenePainter()
        {
            LoadResources();
        }

        public void Load(Map[] maps)
        {
            paths = maps
                .ToDictionary(x => x, x => TransformPath(x, DungeonTask.FindShortestPath(x))
                    .ToArray());

            currentMap = maps[0];
            mainIteration = 0;
        }

        private void LoadResources()
        {
            grass = Resource.Grass;
            path = Resource.Path;
            peasant = Resource.Peasant;
            castle = Resource.Castle;
            chest = Resource.Chest;
        }

        public void ChangeLevel(Map newMap)
        {
            currentMap = newMap;
            mainIteration = 0;
            lastMouseClick = null;
            pathsToChests = null;
            InvalidateVisual();
        }

        public void Update()
        {
            mainIteration = Math.Min(mainIteration + 1, paths[currentMap].Length - 1);
            InvalidateVisual();
        }

        public void OnPointerPressed(object sender, MouseEventArgs e)
        {
            var location = e.Location;
            var position = new Point((location.X / cellWidth), (location.Y / cellHeight));

            lastMouseClick = position;
            pathsToChests = null;
            if (!currentMap.InBounds(position) ||
                currentMap.Maze[lastMouseClick.X, lastMouseClick.Y] != MapCell.Empty) return;

            pathsToChests = BfsTask.FindPaths(currentMap, lastMouseClick, currentMap.Chests)
                .Select(x => x.ToList()).ToList();

            foreach (var pathsToChest in pathsToChests)
                pathsToChest.Reverse();
        }

        public void OnPointerReleased(object sender, MouseEventArgs e)
        {
            pathsToChests = null;
        }

        public void Render(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;

            DrawLevel(g);
            DrawMainPath(g, mainIteration);
            if (pathsToChests != null && lastMouseClick.HasValue)
                DrawAdditionalPaths(g, lastMouseClick);
        }

        private void DrawLevel(Graphics g)
        {
            RenderMap(g);
            foreach (var chestPoint in currentMap.Chests)
                g.DrawImage(chest,
                    new Rectangle(chestPoint.X * cellWidth, chestPoint.Y * cellHeight, cellWidth, cellHeight));
            g.DrawImage(castle,
                new Rectangle(currentMap.Exit.X * cellWidth, currentMap.Exit.Y * cellHeight, cellWidth, cellHeight));
        }

        private void DrawMainPath(Graphics g, int interation)
        {
            var path = paths[currentMap].Take(interation + 1).ToArray();
            DrawPath(g, Color.Green, path);
            var position = path[^1];
            g.DrawImage(peasant, new Rectangle(position.X * cellWidth, position.Y * cellHeight, cellWidth, cellHeight));
        }

        private void DrawAdditionalPaths(Graphics g, Point lastClick)
        {
            g.FillRectangle(Brushes.Red,
                new Rectangle(lastClick.X * cellWidth, lastClick.Y * cellHeight, cellWidth, cellHeight));
            foreach (var pathToChest in pathsToChests)
                DrawPath(g, Color.Red, pathToChest);
        }

        private void DrawPath(Graphics g, Color color, IEnumerable<Point> path)
        {
            var points = path.Select(x =>
                new PointF(x.X * cellWidth + cellWidth * 0.5f, x.Y * cellHeight + cellHeight * 0.5f)).ToArray();
            var pen = new Pen(color, cellHeight * 0.125f)
            {
                DashPattern = [cellWidth * 0.075f, cellHeight * 0.025f] //var newStyle = new DashStyle(new[] { cellWidth * 0.125f, cellHeight * 0.125f }, 1d);
            };
            for (var i = 0; i < points.Length - 1; i++)
                g.DrawLine(pen, points[i], points[i + 1]);
        }

        private IEnumerable<Point> TransformPath(Map map, MoveDirection[] path)
        {
            var walker = new Walker(map.InitialPosition);
            yield return map.InitialPosition;
            foreach (var direction in path)
            {
                walker = walker.WalkInDirection(map, direction);
                yield return walker.Position;
                if (walker.PointOfCollision.HasValue)
                    break;
            }
        }

        private void RenderMap(Graphics g)
        {
            var cellWidth = grass.Size.Width;
            var cellHeight = grass.Size.Height;
            var width = currentMap.Maze.GetLength(0);
            var height = currentMap.Maze.GetLength(1);
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
}
