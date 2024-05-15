using GangWarsArcade.Properties;

namespace GangWarsArcade.domain;

public class Building
{
    public readonly Point Location; 
    public Bitmap Image;

    public Building(Point point)
    {
        Location = point;

        Image = IdentifyImage();
    }

    public static Bitmap IdentifyImage()     {
        var rand = new Random();
        return (rand.Next(0, 5)) switch
        {
            0 => Resource.Town_Hall,
            1 => Resource.Great_Hall,
            2 => Resource.Human_Barracks,
            3 => Resource.Orc_Barracks,
            4 => Resource.Elven_Lumber_Mill,
            5 => Resource.Troll_Lumber_Mill,
            _ => Resource.Castle         };
    }

    public void Update(Map map)
    {
        var result = IsBuildingCaptured(map);         if (result.Item1 == true)
        {
            if (result.Item2 != 0 && (!map.OwnedLocations.ContainsKey(Location) || map.OwnedLocations[Location].Owner != result.Item2))             {
                                map.OwnedLocations[Location] = new OwnedLocation(result.Item2, Location, 0);                 map.Players[result.Item2].AddRespawnBuilding(this);
            }
        }
        else if (map.OwnedLocations.TryGetValue(Location, out var value) && value != null)
        {
            map.Players[value.Owner].RemoveRespawnBuilding(this);

            map.OwnedLocations.Remove(Location);
        }
    }

    public Point GetRandonPointToRespawn()
    {
        var random = new Random();
        var index = random.Next(offsetToSurroundingRoads.Length);
        return Location + offsetToSurroundingRoads[index];
    }

    private (bool, Gang) IsBuildingCaptured(Map map)     {
        var ownerId = (Gang)0;
        foreach (var offset in offsetToSurroundingRoads)
        {
            var newPoint = Location + offset;
            if (!map.OwnedLocations.TryGetValue(newPoint, out var owner)) return (false, 0);

            if (ownerId == 0) ownerId = owner.Owner;
            else if (ownerId != owner.Owner) return (false, 0);
        }
        return (true, ownerId);
    }

    public Point[] GetSurroundingRoads()     {
        var roads = new Point[offsetToSurroundingRoadsWithСorners.Length];
        for (var i = 0; i < offsetToSurroundingRoadsWithСorners.Length; i++)
        {
            roads[i] = Location + offsetToSurroundingRoadsWithСorners[i];
        }
        return roads;
    }

    private static readonly Point[] offsetToSurroundingRoads = new[]     {
        new Point(0, -1),
        new Point(1, -1),
        new Point(2, 0),
        new Point(2, 1),
        new Point(1, 2),
        new Point(0, 2),
        new Point(-1, 1),
        new Point(-1, 0)
    };

    private static readonly Point[] offsetToSurroundingRoadsWithСorners = new[]     {
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
