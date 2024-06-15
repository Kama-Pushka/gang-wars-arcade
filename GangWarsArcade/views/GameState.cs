using GangWarsArcade.domain;
using Point = System.Drawing.Point;
using Timer = System.Windows.Forms.Timer;

namespace GangWarsArcade.views;

public class GameState
{
    private const int _roundDurationSeconds = 100;

    public event Action<Player> RoundFinished;
    public event Action GameFinished;
    public event Action InvalidateTopbarVisual;

    public Map GameMap { get; }
    public GameplayPainter GameplayPainter { get; }

    public int RoundNumber { get => _numRound; private set { _numRound = value; } }
    private int _numRound;
    private int _numСompletedRound;
    public readonly Gang[] RoundsWinners = new Gang[5];

    public int TimeLeft { get => _timeLeft; private set { _timeLeft = value; } }
    private int _timeLeft;

    private readonly Timer _mapUpdateTimer;
    private readonly Timer _gameTimer;
    private readonly Timer _itemsRespawnTimer;

    public GameState(Gang gang, FontFamily fonts)
    {
        // Initialize game map and map painter
        var map = Map.InitializeMap();
        GameMap = map;

        GameplayPainter = new GameplayPainter(map, fonts);

        map.EntityAdded += BeginAct;
        map.EntityRemoved += RemoveAnimation;
        map.ResetMap(); // start setting
        map.SetAI(gang);

        // Initialize map painter timer
        _mapUpdateTimer = new Timer { Interval = 45 };
        _mapUpdateTimer.Tick += UpdateGamePainter;

        // Initialize game timer
        _gameTimer = new Timer { Interval = 1000 };
        _gameTimer.Tick += GameTimerTick;

        _timeLeft = _roundDurationSeconds;
        RoundNumber = 1;

        //Initialize items respawn timer
        _itemsRespawnTimer = new Timer { Interval = 20000 };
        _itemsRespawnTimer.Tick += ItemsRespawn;
        _itemsRespawnTimer.Start();
    }

    private void UpdateGamePainter(object? _, EventArgs __)
    {
        foreach (var e in GameplayPainter.Animations.ToList())
        {
            if (e.Direction != e.Entity.Direction)
            {
                BeginAct(e.Entity, e);
                continue;
            }
            if (e.TargetLogicalLocation != e.Entity.Position)
            {
                e.Location = new Point(e.Location.X + e.Entity.Speed * DirectionExtensions.ConvertDirectionToOffset(e.Direction).X,
                    e.Location.Y + e.Entity.Speed * DirectionExtensions.ConvertDirectionToOffset(e.Direction).Y);
                e.SlowDownFrameRate++;
                if (e.SlowDownFrameRate == e.MaxSlowDownFrameRate)
                {
                    e.CurrentSprite++;
                    if (e.CurrentSprite == e.SpritesCount) e.CurrentSprite = 0;
                    e.SlowDownFrameRate = 0;
                }
            }

            e.CurrentAnimationTime++;
            if (e.Entity.Sprites != null)
                if (e.Entity.IsActive) e.Entity.Image = e.Entity.Sprites[EntityAnimation.DirectionToOrderNumber[e.Direction], e.CurrentSprite];
                else
                {
                    e.CurrentSprite++;
                    if (e.CurrentSprite == e.SpritesCount) e.CurrentSprite = 0;
                    e.Entity.Image = e.Entity.Sprites[4, e.CurrentSprite];
                }

            if (e.CurrentAnimationTime == e.AnimationTime && !e.Entity.IsActive)
                GameplayPainter.Animations.Remove(e);
            else if (e.CurrentAnimationTime == e.AnimationTime || (e.CurrentAnimationTime > e.AnimationTime && e.Entity is Player player && !player.IsHumanPlayer))
            {
                e.Entity.Move(GameMap, e.TargetLogicalLocation);
                e.Entity.Update(GameMap);
                e.Entity.Act(GameMap);
                BeginAct(e.Entity);
            }
            else if (e.Entity.IsActive) e.Entity.Update(GameMap);
        }
        GameplayPainter.Update();
    }

    private void RemoveAnimation(IEntity entity)
    {
        foreach (var a in GameplayPainter.Animations.Where(a => a.Entity == entity).ToList())
            GameplayPainter.Animations.Remove(a);
    }

    public void BeginAct(IEntity entity, EntityAnimation prevAnimation = null)
    {
        RemoveAnimation(entity);
        if (entity.IsActive)
        {
            var location = new Point(entity.Position.X * GameplayPainter.CellSize.Width, entity.Position.Y * GameplayPainter.CellSize.Height);
            if (prevAnimation != null) location = prevAnimation.Location;

            var targetPoint = entity.GetNextPoint(GameMap);

            var distance = Math.Abs(targetPoint.X * GameplayPainter.CellSize.Width - location.X);
            if (entity.Direction == MoveDirection.Up || entity.Direction == MoveDirection.Down)
                distance = Math.Abs(targetPoint.Y * GameplayPainter.CellSize.Width - location.Y);

            if (distance > 0)
            {
                if (entity.Direction == MoveDirection.Up || entity.Direction == MoveDirection.Down)
                    location = new Point(entity.Position.X * GameplayPainter.CellSize.Width, location.Y);
                else
                    location = new Point(location.X, entity.Position.Y * GameplayPainter.CellSize.Height);
            }

            GameplayPainter.Animations.Add(
                new EntityAnimation
                {
                    AnimationTime = entity.Speed != 0 ? distance / entity.Speed : 0,
                    Entity = entity,
                    Direction = entity.Direction,
                    Location = location,
                    TargetLogicalLocation = targetPoint,
                    MaxSlowDownFrameRate = IdentifySlowDown(entity),
                    SpritesCount = entity.Sprites != null ? entity.Sprites.GetLength(1) : default
                });
        }
        else if (entity is Bullet)
        {
            var location = new Point(entity.Position.X * GameplayPainter.CellSize.Width, entity.Position.Y * GameplayPainter.CellSize.Height);
            if (prevAnimation != null) location = prevAnimation.Location;

            GameplayPainter.Animations.Add(
                new EntityAnimation
                {
                    AnimationTime = 4,
                    Entity = entity,
                    Direction = entity.Direction,
                    Location = location,
                    TargetLogicalLocation = entity.Position,
                    MaxSlowDownFrameRate = IdentifySlowDown(entity),
                    SpritesCount = entity.Sprites != null ? entity.Sprites.GetLength(1) : default,
                    IsOneReplayAntimation = entity is Bullet
                });
        }
    }

    private static int IdentifySlowDown(IEntity entity)
    {
        if (entity is Player player)
            return player.Gang switch
            {
                Gang.Green => 2,
                Gang.Blue => 2,
                Gang.Yellow => 3,
                Gang.Pink => 3
            };
        else return 1;
    }

    private void ItemsRespawn(object? _, EventArgs __)
        => GameMap.AddItemsToRandomCell([ItemType.FireBolt, ItemType.HPRegeneration, ItemType.HPRegeneration, ItemType.Trap]);

    private void GameTimerTick(object? _, EventArgs __)
    {
        var earlyFinish = СheckForEarlyFinishRound(out var winner);
        if (_timeLeft > 0 && !earlyFinish)
        {
            _timeLeft--;
            InvalidateTopbarVisual();
        }
        else if (!earlyFinish)
        {
            var activePlayers = GameMap.Players.Values.Where(p => p.IsActive).OrderByDescending(p => p.OwnedBuildingsCount).ToArray();
            if (activePlayers[0].OwnedBuildingsCount == activePlayers[1].OwnedBuildingsCount) FinishRound(null);
            else FinishRound(activePlayers[0]);
        }
        else
        {
            FinishRound(winner);
        }
    }

    private bool СheckForEarlyFinishRound(out Player? winner)
    {
        winner = null;
        var activePlayers = GameMap.Players.Values.Where(p => p.IsActive).ToList();
        if (activePlayers.Count == 0) return true;
        if (activePlayers.Count == 1)
        {
            winner = activePlayers.First();
            return true;
        }
        return false;
    }

    private void FinishRound(Player winner)
    {
        if (winner != null) RoundsWinners[_numСompletedRound++] = winner.Gang;
        RoundFinished(winner);
    }

    public void PauseGame()
    {
        _mapUpdateTimer.Stop();
        _gameTimer.Stop();
        _itemsRespawnTimer.Stop();
    }

    public void RunGame()
    {
        _mapUpdateTimer.Start();
        _gameTimer.Start();
        _itemsRespawnTimer.Start();
    }

    public void PrepareNewRound()
    {
        GameplayPainter.ResetMap();
        _numRound++;

        _timeLeft = _roundDurationSeconds;
        InvalidateTopbarVisual();
    }

    public void CheckForFinishGame()
    {
        if (_numСompletedRound == 5) FinishGame();
    }

    public void FinishGame()
        => GameFinished();
}
