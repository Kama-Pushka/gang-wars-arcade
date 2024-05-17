using GangWarsArcade.Properties;

namespace GangWarsArcade.domain;

public class Building
{
    public Point Location { get; }
    public Bitmap Image { get; set; }

    public Building(Point point)
    {
        Location = point;

        Image = IdentifyImage();
    }

    public static Bitmap IdentifyImage()
    {
        var rand = new Random();
        return (rand.Next(0, 5)) switch
        {
            0 => Resource.Town_Hall,
            1 => Resource.Great_Hall,
            2 => Resource.Human_Barracks,
            3 => Resource.Orc_Barracks,
            4 => Resource.Elven_Lumber_Mill,
            5 => Resource.Troll_Lumber_Mill
        };
    }

    public void Update(Map map)
    {
        var result = IsBuildingCaptured(map);
        if (result.IsCaptured == true)
        {
            if (result.Gang != 0 && (!map.OwnedLocations.ContainsKey(Location) || map.OwnedLocations[Location].Owner != result.Gang))
            {
                map.OwnedLocations[Location] = new OwnedLocation(result.Gang, Location);
                map.Players[result.Gang].OwnedBuildings.Add(this);
            }
        }
        else if (map.OwnedLocations.TryGetValue(Location, out var value) && value != null)
        {
            map.Players[value.Owner].OwnedBuildings.Remove(this);
            map.OwnedLocations.Remove(Location);
        }
    }

    private (bool IsCaptured, Gang Gang) IsBuildingCaptured(Map map)
    {
        var ownerId = (Gang)0;
        foreach (var offset in _offsetToSurroundingRoads)
        {
            var newPoint = Location + offset;
            if (!map.OwnedLocations.TryGetValue(newPoint, out var owner)) return (false, 0);

            if (ownerId == 0) ownerId = owner.Owner;
            else if (ownerId != owner.Owner) return (false, 0);
        }
        return (true, ownerId);
    }

    public Point GetRandonPointToRespawn()
    {
        var random = new Random();
        var index = random.Next(_offsetToSurroundingRoads.Length);
        return Location + _offsetToSurroundingRoads[index];
    }

    public Point[] GetSurroundingRoads()
    {
        var roads = new Point[_offsetToSurroundingRoadsWithСorners.Length];
        for (var i = 0; i < _offsetToSurroundingRoadsWithСorners.Length; i++)
        {
            roads[i] = Location + _offsetToSurroundingRoadsWithСorners[i];
        }
        return roads;
    }

    private static readonly Point[] _offsetToSurroundingRoads = new[]
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

    private static readonly Point[] _offsetToSurroundingRoadsWithСorners = new[]
    {
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
        new Point(-1, 0),
        new Point(-1, -1)
    };
}
