namespace GangWarsArcade.domain;

public class OwnedLocation
{
    public Gang Owner { get; set; }
    public readonly Point Location;

    public OwnedLocation(Gang owner, Point location)
    {
        Owner = owner;
        Location = location;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj is not OwnedLocation) return false;

        var other = (OwnedLocation)obj;
        return Owner.Equals(other.Owner) && Location.Equals(other.Location);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Owner, Location);
    }

    public override string ToString()
    {
        return $"[Location: {Location}, Owner: {Owner}]";
    }
}