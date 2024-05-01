using GangWarsArcade.views;

namespace GangWarsArcade.domain;


public class Map
{
    public MapCell[,] Maze { get; private set; }
    public Point[] Buildings { get; private set; }
    public Dictionary<Gang, Player> Players { get; private set; }
    public Dictionary<Point, ItemType> Items { get; private set; }
    public Dictionary<Point, OwnedLocation> OwnedLocations { get; private set; }
    public List<Bullet> Bullets { get; private set; }

    public Dictionary<Point, (Gang, ItemType)> Traps { get; private set; }

    public Player HumanPlayer { get; set; }

    private Map(MapCell[,] maze, Point[] buildings, Dictionary<Point, ItemType> items, Dictionary<Gang, Player> players)
    {
        Maze = maze;
        Buildings = buildings;
        Items = items;
        Players = players;
        OwnedLocations = new Dictionary<Point, OwnedLocation>();
        Bullets = new List<Bullet>();
        Traps = new Dictionary<Point, (Gang, ItemType)>();
    }

    public void Update()
    {
        foreach (var player in Players.Values.Where(p => p.IsAlive))
        {
            player.Update(this);
        }
        foreach (var building in Buildings)
        {
            BuildingStateUpdate(building);
        }
        for (var i = 0; i < Bullets.Count; i++)
        {
            Bullets[i].Update(this);
        }
    }

    private static readonly Point[] startPositions = new[]
    {
        new Point(1, 1),
        new Point(1, 13),
        new Point(19, 1),
        new Point(19, 13)
    };

    public void ResetMap()
    {
        Bullets.Clear();
        Traps.Clear();

        var rand = new Random();
        rand.Shuffle(startPositions);
        for (var i = 0; i < startPositions.Length; i++)
        {
            Players[(Gang)i + 1].ResetPlayer(startPositions[i]);
        }

        OwnedLocations = new Dictionary<Point, OwnedLocation>();
    }

    public bool InBounds(Point point)
        => point is { X: >= 0, Y: >= 0 }
           && Maze.GetLength(0) > point.X
           && Maze.GetLength(1) > point.Y;

    #region Building

    private void BuildingStateUpdate(Point building)
    {
        var result = IsBuildingCaptured(building);
        if (result.Item1 == true)
        {
            if (result.Item2 != 0 && (!OwnedLocations.ContainsKey(building) || OwnedLocations[building].Owner != result.Item2))
            {
                OwnedLocations[building] = new OwnedLocation(result.Item2, building, 0);
                Players[result.Item2].RespawnLocations.Add(OwnedLocations[building]);
            }
        }
        else if (OwnedLocations.TryGetValue(building, out var value) && value != null)
        {
            Players[value.Owner].RespawnLocations.Remove(OwnedLocations[building]);

            OwnedLocations.Remove(building);
        }
    }

    private (bool, Gang) IsBuildingCaptured(Point point)
    {
        var ownerId = (Gang)0;
        foreach (var offset in offsetToSurroundingRoad)
        { 
            var newPoint = point + offset;
            if (!OwnedLocations.TryGetValue(newPoint, out var owner)) return (false, 0);

            if (ownerId == 0) ownerId = owner.Owner;
            else if (ownerId != owner.Owner) return (false, 0);
        }
        return (true, ownerId);
    }

    private static readonly Point[] offsetToSurroundingRoad = new[]
    {
        new Point(0, -1),
        new Point(1, -1),
        new Point(2, 0),
        new Point(2, 1),
        new Point(1, 2),
        new Point(0, 2),
        new Point(-1, 1),
        new Point(-1, 0)
    };

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
        var lines = text.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        return FromLines(lines);
    }

    public static Map FromLines(string[] lines)
    {
        var dungeon = new MapCell[lines[0].Length, lines.Length];
        var players = new Dictionary<Gang, Player>();
        var buildings = new List<Point>();
        var items = new Dictionary<Point, ItemType>();

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
                        gang++;
                        players[gang] = new Player(new Point(x, y), gang);
                        break;
                    case 'H':
                        dungeon[x, y] = MapCell.Empty;
                        items[new Point(x, y)] = ItemType.HPRegeneration;
                        break;
                    case 'T':
                        dungeon[x, y] = MapCell.Empty;
                        items[new Point(x, y)] = ItemType.Trap;
                        break;
                    case 'C':
                        dungeon[x, y] = MapCell.Empty;
                        items[new Point(x, y)] = ItemType.Pistol;
                        break;
                    case 'E':
                        dungeon[x, y] = MapCell.Wall;
                        buildings.Add(new Point(x, y));
                        break;
                    default:
                        dungeon[x, y] = MapCell.Empty;
                        break;
                }
            }
        }

        return new Map(dungeon, buildings.ToArray(), items, players);
    }

    #endregion
}