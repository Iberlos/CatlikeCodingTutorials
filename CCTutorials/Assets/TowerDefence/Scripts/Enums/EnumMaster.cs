public enum GameSpeedState
{
    Paused = 0,
    Playing,
    Fast
}

public enum GameTileContentType
{
    Ground = 0,
    Water,
    Mountain,
    Resource,
    Destination,
    Wall,
    SpawnPoint,
    Tower,
    Last
}

public enum GroundType
{
    Normal = 0,
    Bridge
}

public enum ResourceType
{
    Wood = 0,
    Metal,
    Crystal,
    Food,
    Gold,
    Last
}

public static class ResourceExtensions
{
    public static float GoldValue(this ResourceType type)
    {
        switch(type)
        {
            case ResourceType.Food:
                {
                    return 0.1f;
                }
            case ResourceType.Wood:
                {
                    return 0.3f;
                }
            case ResourceType.Metal:
                {
                    return 0.5f;
                }
            case ResourceType.Crystal:
                {
                    return 1f;
                }
            default:
                {
                    return 0f;
                }
        }
    }
}

public enum DestinationType
{
    Capital = 0,
    Farm,
    Outpost
}

public enum TowerType
{
    Laser = 0,
    Mortar,
    Archer,
    Last
}

public enum EnemyType
{
    Small = 0,
    Medium,
    Large,
    Last
}
