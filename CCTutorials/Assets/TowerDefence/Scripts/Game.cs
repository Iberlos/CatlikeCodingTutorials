using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField]
    private Vector2Int boardSize = new Vector2Int(11, 11);
    [SerializeField]
    private GameBoard board = default;
    [SerializeField]
    private GameTileContentFactory tileContentFactory = default;
    [SerializeField]
    private EnemyFactory enemyFactory = default;
    [SerializeField]
    float spawnSpeed = 1f;

    private Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

    private float spawnProgress;
    private EnemyCollection enemies = new EnemyCollection();

    private void OnValidate()
    {
        if (boardSize.x < 2)
        {
            boardSize.x = 2;
        }
        if (boardSize.y < 2)
        {
            boardSize.y = 2;
        }
    }

    private void Awake()
    {
        board.Initialize(boardSize, tileContentFactory);
        board.ShowGrid = true;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HandleTouch();
        else if (Input.GetMouseButtonDown(1))
            HandleAlternativeTouch();

        if(Input.GetKeyDown(KeyCode.V))
        {
            board.ShowPaths = !board.ShowPaths;
        }

        if(Input.GetKeyDown(KeyCode.G))
        {
            board.ShowGrid = !board.ShowGrid;
        }

        spawnProgress += spawnSpeed * Time.deltaTime;
        while(spawnProgress >= 1f)
        {
            spawnProgress -= 1f;
            SpawnEnemy();
        }

        enemies.GameUpdate();
        Physics.SyncTransforms();
        board.GameUpdate();
    }

    void HandleAlternativeTouch()
    {
        GameTile tile = board.GetTile(TouchRay);
        if (tile != null)
            if (Input.GetKey(KeyCode.LeftShift))
                board.ToggleDestination(tile);
            else
                board.ToggleSpawnPoint(tile);
    }

    void HandleTouch()
    {
        GameTile tile = board.GetTile(TouchRay);
        if(tile != null)
        {
            if(Input.GetKey(KeyCode.LeftShift))
            {
                board.ToggleTower(tile);
            }
            else
            {
                board.ToggleWall(tile);
            }
        }
    }

    private void SpawnEnemy()
    {
        GameTile spawnPoint = board.GetSpawnPoint(Random.Range(0, board.spawnPointCount));
        Enemy enemy = enemyFactory.Get();
        enemy.SpawnOn(spawnPoint);
        enemies.Add(enemy);
    }
}
