namespace GangWarsArcade.views;

public enum AlertOption
{
    RoundStarting = 4,
    HumanPlayerWasted = 7,
    RoundFinished = 5,
    Pause = 1,
    Training = 2,
}

public record AlertProperties(object Sender, AlertOption Option, object Tag = null);
