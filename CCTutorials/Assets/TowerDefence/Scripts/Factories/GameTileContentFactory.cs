using UnityEngine;


[CreateAssetMenu]
public class GameTileContentFactory : GameObjectFactory
{
    [SerializeField]
    private Ground[] groundPrefabs = default;
    [SerializeField]
    private GameTileContent[] waterPrefabs = default;
    [SerializeField]
    private GameTileContent[] mountainPrefabs = default;
    [SerializeField]
    private GameTileContent[] resourcePrefabs = default;
    [SerializeField]
    private GameTileContent[] wallPrefabs = default;
    [SerializeField]
    private GameTileContent[] destinationPrefabs = default;
    [SerializeField]
    private GameTileContent[] spawnPointPrefabs = default;
    [SerializeField]
    private Tower[] towerPrefabs = default;

    public T Get<T>(GameTileContentType type, int variation = 0) where T : GameTileContent
    {
        switch (type)
        {
            case GameTileContentType.Ground: return Get(groundPrefabs, variation) as T;
            case GameTileContentType.Water: return Get(waterPrefabs, variation) as T;
            case GameTileContentType.Mountain: return Get(mountainPrefabs, variation) as T;
            case GameTileContentType.Resource: return Get(resourcePrefabs, variation) as T;
            case GameTileContentType.Wall: return Get(wallPrefabs, variation) as T;
            case GameTileContentType.Destination: return Get(destinationPrefabs, variation) as T;
            case GameTileContentType.SpawnPoint: return Get(spawnPointPrefabs, variation) as T;
            case GameTileContentType.Tower: return Get(towerPrefabs, variation) as T;
        }
        Debug.Assert(false, "Unsupported Type/variation: " + type + "/" + variation);
        return null;
    }

    public T Get<T>(T[] prefabs, int variation) where T : GameTileContent
    {
        Debug.Assert(variation < prefabs.Length, "Unsupported tower type!");
        T prefab = prefabs[variation];
        Debug.Assert(prefab != null, "Prefab at indext " + variation + " is not set!");
        Debug.Assert(variation == prefab.Variation, "Prefab at wrong index!");
        return Get<T>(prefab) as T;
    }

    private T Get<T>(T prefab) where T : GameTileContent
    {
        T instance = CreateGameObjectInstance(prefab);
        instance.OriginFactory = this;
        return instance;
    }

    public void Reclaim(GameTileContent content)
    {
        Debug.Assert(content.OriginFactory == this, "Wrong Factory reclaimed!");
        Destroy(content.gameObject);
    }
}
