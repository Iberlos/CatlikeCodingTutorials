using UnityEngine;


[CreateAssetMenu]
public class GameTileContentFactory : GameObjectFactory
{
    [SerializeField]
    private GameTileContent groundPrefab = default;
    [SerializeField]
    private GameTileContent waterPrefab = default;
    [SerializeField]
    private GameTileContent mountainPrefab = default;
    [SerializeField]
    private GameTileContent[] resourcePrefabs = default;
    [SerializeField]
    private GameTileContent wallPrefab = default;
    [SerializeField]
    private GameTileContent destinationPrefab = default;
    [SerializeField]
    private GameTileContent spawnPointPrefab = default;
    [SerializeField]
    private Tower[] towerPrefabs = default;

    public GameTileContent Get(GameTileContentType type)
    {
        switch (type)
        {
            case GameTileContentType.Ground: return Get(groundPrefab);
            case GameTileContentType.Water: return Get(waterPrefab);
            case GameTileContentType.Mountain: return Get(mountainPrefab);
            case GameTileContentType.Wall: return Get(wallPrefab);
            case GameTileContentType.Destination: return Get(destinationPrefab);
            case GameTileContentType.SpawnPoint: return Get(spawnPointPrefab);
        }
        Debug.Assert(false, "Unsupported non-tower Type: " + type);
        return null;
    }

    private T Get<T>(T prefab) where T : GameTileContent
    {
        T instance = CreateGameObjectInstance(prefab);
        instance.OriginFactory = this;
        return instance;
    }

    public GameTileContent Get(ResourceType type)
    {
        Debug.Assert((int)type < resourcePrefabs.Length, "Unsupported tower type!");
        GameTileContent prefab = resourcePrefabs[(int)type];
        Debug.Assert(prefab != null, "Prefab at indext " + (int)type + " is not set!");
        //Debug.Assert(type == prefab.TowerType, "Tower prefab at wrong index!");
        return Get(prefab);
    }

    public Tower Get(TowerType type)
    {
        Debug.Assert((int)type < towerPrefabs.Length, "Unsupported tower type!");
        Tower prefab = towerPrefabs[(int)type];
        Debug.Assert(prefab != null, "Prefab at indext " + (int)type + " is not set!");
        Debug.Assert(type == prefab.TowerType, "Tower prefab at wrong index!");
        return Get(prefab);
    }

    public void Reclaim(GameTileContent content)
    {
        Debug.Assert(content.OriginFactory == this, "Wrong Factory reclaimed!");
        Destroy(content.gameObject);
    }
}
