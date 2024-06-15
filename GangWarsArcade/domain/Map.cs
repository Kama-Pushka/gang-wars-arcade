using GangWarsArcade.Properties;

namespace GangWarsArcade.domain;

public class Map : IMapWithEntity
{
    public event Action<IEntity, EntityAnimation> EntityAdded;
    public event Action<IEntity> EntityRemoved;

    public MapCell[,] Maze { get; private set; }

    public Dictionary<Gang, Player> Players { get; private set; }
    public Player HumanPlayer { get; private set; }
    public Dictionary<Point, OwnedLocation> OwnedLocations { get; private set; }
    public Building[] Buildings { get; private set; }

    public HashSet<IEntity> Entities { get; private set; }
    public void AddEntity(IEntity entity)
    {
        Entities.Add(entity);
        EntityAdded(entity, null);
    }
    public void RemoveEntity(IEntity entity)
    {
        Entities.Remove(entity);
        EntityRemoved(entity);

        if (entity is Bullet)
            EntityAdded(entity, null);
    }

    private Map(MapCell[,] maze, Building[] buildings, HashSet<IEntity> entities, Dictionary<Gang, Player> players)
    {
        Maze = maze;
        IdentifyRoadImages(maze);

        Buildings = buildings;
        Players = players;
        OwnedLocations = new Dictionary<Point, OwnedLocation>();
        Entities = entities;

        foreach (var player in Players.Values)
            player.Respawned += AddEntity;

        // Find empty points
        var emptyPoints = new List<Point>();
        var width = Maze.GetLength(0);
        var height = Maze.GetLength(1);
        for (var i = 0; i < width; i++)
            for (var j = 0; j < height; j++)
                if (Maze[i, j].Cell == MapCellEnum.Empty) emptyPoints.Add(new Point(i, j));
        _emptyPoints = emptyPoints.ToArray();
    }

    public void SetAI(Gang humanPlayer)
    {
        foreach (var player in Players.Values)
            if (player.Gang != humanPlayer) player.CreateAI();
            else HumanPlayer = player;
    }

    public void Update()
    {
        foreach (var building in Buildings)
        {
            building.Update(this);
        }
    }

    private static Point[] _emptyPoints;
    private static readonly Random _rand = new();

    public void AddItemsToRandomCell(ItemType[] items)
    {
        _rand.Shuffle(_emptyPoints);
        for (var i = 0; i < items.Length; i++)
            AddEntity(new Item(items[i], _emptyPoints[i]));
    }

    public void ResetMap()
    {
        Entities.Clear();

        foreach (var building in Buildings)
            building.Image = Building.IdentifyImage();

        OwnedLocations = new Dictionary<Point, OwnedLocation>();
        GenerateStartMap();

        foreach (var player in Players.Values)
            AddEntity(player);
    }

    public bool IsPossibleCellToMove(Point point)
        => InBounds(point) && Maze[point.X, point.Y].Cell == MapCellEnum.Empty;

    public bool InBounds(Point point)
        => point is { X: >= 0, Y: >= 0 }
           && Maze.GetLength(0) > point.X
           && Maze.GetLength(1) > point.Y;

    #region GenerateStartMap

    private void GenerateStartMap()
    {
        _rand.Shuffle(_startHomeLocations);
        for (var i = 0; i < _startHomeLocations.Length; i++)
        {
            for (var j = 0; j < _offsetToAllSurroundingRoads.Length; j++)
            {
                var pos = _startHomeLocations[i].BuildingLocation + _offsetToAllSurroundingRoads[j];
                if (!_notOwnedPoints.Contains(pos))
                {
                    OwnedLocations[pos] = new OwnedLocation((Gang)i + 1, pos);
                }
            }
            Players[(Gang)i + 1].Reset(_startHomeLocations[i].PlayerPosition);
            Players[(Gang)i + 1].SetNewDirection(_startHomeLocations[i].StartDirection);
            Players[(Gang)i + 1].Image = Players[(Gang)i + 1].Sprites[EntityAnimation.DirectionToOrderNumber[Players[(Gang)i + 1].Direction], 0];
        }
    }

    private static readonly (Point BuildingLocation, Point PlayerPosition, MoveDirection StartDirection)[] _startHomeLocations = new[]
    {
        (new Point(2, 2), new Point(1, 4), MoveDirection.Right),
        (new Point(2, 8), new Point(1, 7), MoveDirection.Right),
        (new Point(17, 2), new Point(19, 4), MoveDirection.Left),
        (new Point(17, 8), new Point(19, 7), MoveDirection.Left)
    };

    private static readonly Point[] _notOwnedPoints = new[]
    {
        new Point(2, 4),
        new Point(3, 4),
        new Point(4, 4),
        new Point(2, 7),
        new Point(3, 7),
        new Point(4, 7),
        new Point(16, 4),
        new Point(17, 4),
        new Point(18, 4),
        new Point(16, 7),
        new Point(17, 7),
        new Point(18, 7),
    };

    private static readonly Point[] _offsetToAllSurroundingRoads = new[]
    {
        new Point(-1, -1),
        new Point(0, -1),
        new Point(1, -1),
        new Point(2, -1),
        new Point(2, 0),
        new Point(2, 1),
        new Point(2, 2),
        new Point(1, 2),
        new Point(0, 2),
        new Point(-1, 2),
        new Point(-1, 1),
        new Point(-1, 0)
    };

    #endregion

    #region Initialize Game Map

    private const string _gameMap = @"
#####################
#P                 P#
# E# E# E# E# E# E# #
# ## ## ## ## ## ## #
#                   #
# E# E# E# E# E# E# #
# ## ## ## ## ## ## #
#                   #
# E# E# E# E# E# E# #
# ## ## ## ## ## ## #
#P                 P#
#####################";

    public static Map InitializeMap() => FromText(_gameMap);

    public static Map FromText(string text)
    {
        var lines = text.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        return FromLines(lines);
    }

    public static Map FromLines(string[] lines)
    {
        var map = new MapCell[lines[0].Length, lines.Length];
        var buildings = new List<Building>();
        var entities = new HashSet<IEntity>();
        var players = new Dictionary<Gang, Player>();

        var gang = (Gang)0;
        for (var y = 0; y < lines.Length; y++)
        {
            for (var x = 0; x < lines[0].Length; x++)
            {
                switch (lines[y][x])
                {
                    case '#':
                        map[x, y] = new MapCell() { Image = Resource.Grass, Cell = MapCellEnum.Wall };
                        break;
                    case 'P':
                        map[x, y] = new MapCell() { Image = Resource.Path, Cell = MapCellEnum.Empty };
                        var player = new Player(new Point(x, y), ++gang);
                        entities.Add(player);
                        players[gang] = player;
                        break;
                    case 'C': // для тестов
                        map[x, y] = new MapCell() { Image = Resource.Path, Cell = MapCellEnum.Empty };
                        var chest = new Item((ItemType)_rand.Next(3), new Point(x, y));
                        entities.Add(chest);
                        break;
                    case 'E':
                        map[x, y] = new MapCell() { Image = Resource.Grass, Cell = MapCellEnum.Wall };
                        buildings.Add(new Building(new Point(x, y)));
                        break;
                    default:
                        map[x, y] = new MapCell() { Image = Resource.Path, Cell = MapCellEnum.Empty };
                        break;
                }
            }
        }

        return new Map(map, buildings.ToArray(), entities, players);
    }

    private void IdentifyRoadImages(MapCell[,] map)
    {
        for (int x = 0; x < map.GetLength(0); x++)
            for (int y = 0; y < map.GetLength(1); y++)
                if (map[x, y].Cell == MapCellEnum.Empty)
                    map[x, y].Image = IdentifyRoadImage(new Point(x, y));
    }

    private Bitmap IdentifyRoadImage(Point point)
    {
        var num = 0;
        if (IsPossibleCellToMove(point + DirectionExtensions.ConvertDirectionToOffset(MoveDirection.Up))) num += 1000;
        if (IsPossibleCellToMove(point + DirectionExtensions.ConvertDirectionToOffset(MoveDirection.Down))) num += 100;
        if (IsPossibleCellToMove(point + DirectionExtensions.ConvertDirectionToOffset(MoveDirection.Left))) num += 10;
        if (IsPossibleCellToMove(point + DirectionExtensions.ConvertDirectionToOffset(MoveDirection.Right))) num += 1;

        var image = Resource.Path;
        switch (num)
        {
            case 1100:
                image = Resource.Straight_Road;
                image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                break;
            case 0011:
                image = Resource.Straight_Road;
                break;
            case 1001:
                image = Resource.Сorner_Road;
                break;
            case 1010:
                image = Resource.Сorner_Road;
                image.RotateFlip(RotateFlipType.Rotate180FlipY);
                break;
            case 0101:
                image = Resource.Сorner_Road;
                image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                break;
            case 0110:
                image = Resource.Сorner_Road;
                image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                break;
            case 1101:
                image = Resource.Semi_Cross_Road;
                break;
            case 1110:
                image = Resource.Semi_Cross_Road;
                image.RotateFlip(RotateFlipType.Rotate180FlipY);
                break;
            case 1011:
                image = Resource.Semi_Cross_Road;
                image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                break;
            case 0111:
                image = Resource.Semi_Cross_Road;
                image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                break;
            case 1111:
                image = Resource.Cross_Road;
                break;
        }
        return image;
    }

    #endregion
}