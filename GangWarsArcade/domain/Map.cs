namespace GangWarsArcade.domain;

public class Map : IMapWithEntity
{
    public MapCell[,] Maze { get; private set; }

    public Dictionary<Gang, Player> Players { get; private set; }
    public Dictionary<Point, OwnedLocation> OwnedLocations { get; private set; }
    public Building[] Buildings { get; private set; }

    public Player HumanPlayer { get; set; }

    private Map(MapCell[,] maze, Building[] buildings, HashSet<IEntity> entities, Dictionary<Gang, Player> players)
    {
        Maze = maze;
        Buildings = buildings;
        Players = players;
        OwnedLocations = new Dictionary<Point, OwnedLocation>();

        foreach (var player in Players.Values)
            player.PlayerRespawned += AddEntity;

        Entities = entities;
    }

    public HashSet<IEntity> Entities { get; private set; }

    public void AddEntity(IEntity entity)
    { 
        Entities.Add(entity);
    }

    public void Update()
    {
        foreach (var entity in Entities.ToList())
        {
            entity.Act(this);
        }
        foreach (var player in Players.Values.Where(p => p.IsActive))
        { // коллизия (она вся крутится вокруг игроков, я не знаю как адекватно сделать по другому)
            player.Update(this);
        }
        foreach (var entity in Entities.Where(e => e.Type.Name != "Player").ToList()) 
        { // обновление состояний объектов после коллизий (у игрока состояние уже обновлено)
            entity.Update(this);
        }
        foreach (var building in Buildings)
        {
            building.Update(this);
        }
    }

    public void ResetMap()
    {
        Entities.Clear();
        foreach (var player in Players.Values) 
            Entities.Add(player);

        OwnedLocations = new Dictionary<Point, OwnedLocation>();
        GenerateStartMap();
    }

    public bool InBounds(Point point)
        => point is { X: >= 0, Y: >= 0 }
           && Maze.GetLength(0) > point.X
           && Maze.GetLength(1) > point.Y;

    #region GenerateStartMap
    private static readonly (Point, Point, MoveDirection)[] startHomeLocation = new[]
    {
        (new Point(2, 2), new Point(1, 4), MoveDirection.Right),
        (new Point(2, 11), new Point(1, 10), MoveDirection.Right),
        (new Point(17, 2), new Point(19, 4), MoveDirection.Left),
        (new Point(17, 11), new Point(19, 10), MoveDirection.Left)
    };

    private static readonly Point[] notOwnedPoints = new[]
    {
        new Point(2, 4),
        new Point(3, 4),
        new Point(4, 4),
        new Point(2, 10),
        new Point(3, 10),
        new Point(4, 10),
        new Point(16, 4),
        new Point(17, 4),
        new Point(18, 4),
        new Point(16, 10),
        new Point(17, 10),
        new Point(18, 10),
    };

    private static readonly Point[] offsetToSurroundingRoads = new[]
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
    private void GenerateStartMap()
    {
        var rand = new Random();
        rand.Shuffle(startHomeLocation);
        for (var i = 0; i < startHomeLocation.Length; i++)
        {
            for (var j = 0; j < offsetToSurroundingRoads.Length; j++)
            {
                var pos = startHomeLocation[i].Item1 + offsetToSurroundingRoads[j];
                if (!notOwnedPoints.Contains(pos))
                {
                    OwnedLocations[pos] = new OwnedLocation((Gang)i + 1, pos, 0);
                }
            }
            Players[(Gang)i + 1].ResetPlayer(startHomeLocation[i].Item2);
            Players[(Gang)i + 1].SetNewDirection(startHomeLocation[i].Item3);
        }
    }
    #endregion

    #region Initialize game map

    private const string gameMap = @"
#####################
#P     C           P#
# E# E# E# E# E# E# #
# ## ## ## ## ## ## #
#H  T               #
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

    public static Map InitializeMap()
    {
        return FromText(gameMap);
    }

    public static Map FromText(string text)
    {
        var lines = text.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries); // new[] { Environment.NewLine }
        return FromLines(lines);
    }

    public static Map FromLines(string[] lines)
    {
        var dungeon = new MapCell[lines[0].Length, lines.Length];
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
                        dungeon[x, y] = MapCell.Wall;
                        break;
                    case 'P':
                        dungeon[x, y] = MapCell.Empty;
                        var player = new Player(new Point(x, y), ++gang);
                        entities.Add(player);
                        players[gang] = player;
                        break;
                    case 'H':
                        dungeon[x, y] = MapCell.Empty;
                        entities.Add(new Item(ItemType.HPRegeneration, new Point(x, y)));
                        break;
                    case 'T':
                        dungeon[x, y] = MapCell.Empty;
                        entities.Add(new Item(ItemType.Trap, new Point(x, y)));
                        break;
                    case 'C':
                        dungeon[x, y] = MapCell.Empty;
                        entities.Add(new Item(ItemType.FireBolt, new Point(x, y)));
                        break;
                    case 'E':
                        dungeon[x, y] = MapCell.Wall;
                        buildings.Add(new Building(new Point(x, y)));
                        break;
                    default:
                        dungeon[x, y] = MapCell.Empty;
                        break;
                }
            }
        }

        return new Map(dungeon, buildings.ToArray(), entities, players);
    }

    #endregion
}