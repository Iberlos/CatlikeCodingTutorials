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

public enum ResourceType
{
    Forest = 0,
    Metal,
    Crystal,
    Last
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
    Last
}

public enum EnemyType
{
    Small = 0,
    Medium,
    Large,
    Last
}
