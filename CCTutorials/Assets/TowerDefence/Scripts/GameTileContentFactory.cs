using UnityEngine;


[CreateAssetMenu]
public class GameTileContentFactory : GameObjectFactory
{
    [SerializeField]
    private GameTileContent emptyPrefab = default;
    [SerializeField]
    private GameTileContent wallPrefab = default;
    [SerializeField]
    private GameTileContent destinationPrefab = default;
    [SerializeField]
    private GameTileContent spawnPointPrefab = default;

    public GameTileContent Get(GameTileContentType type)
    {
        switch (type)
        {
            case GameTileContentType.Empty: return Get(emptyPrefab);
            case GameTileContentType.Wall: return Get(wallPrefab);
            case GameTileContentType.Destination: return Get(destinationPrefab);
            case GameTileContentType.SpawnPoint: return Get(spawnPointPrefab);
        }
        Debug.Assert(false, "Unsupported Type: " + type);
        return null;
    }

    private GameTileContent Get(GameTileContent prefab)
    {
        GameTileContent instance = CreateGameObjectInstance(prefab);
        instance.OriginFactory = this;
        return instance;
    }

    public void Reclaim(GameTileContent content)
    {
        Debug.Assert(content.OriginFactory == this, "Wrong Factory reclaimed!");
        Destroy(content.gameObject);
    }
}
