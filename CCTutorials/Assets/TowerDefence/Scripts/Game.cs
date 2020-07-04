using UnityEngine;
using UnityEngine.EventSystems;

public class Game : MonoBehaviour
{
    [SerializeField]
    private CameraBehavior gameCamera = default;
    [SerializeField]
    private Vector2Int boardSize = new Vector2Int(11, 11);
    [SerializeField]
    private GameBoard board = default;
    [SerializeField]
    private GameTileContentFactory tileContentFactory = default;
    [SerializeField]
    private WarFactory warFactory = default;
    [SerializeField]
    private GameScenario scenario = default;
    [SerializeField, Range(0, 100)]
    private int startingPlayerHealth = 10;
    [SerializeField, Range(1f, 10f)]
    private float playSpeed = 1f;

    private Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

    private GameBehaviorCollection enemies = new GameBehaviorCollection();
    private GameBehaviorCollection nonEnemies = new GameBehaviorCollection();
    private TowerType selectedTowerType = TowerType.Laser;
    private GameScenario.State activeScenario;
    private int playerHealth;
    private const float pausedTimeScale = 0f;
    private StructurePlacementManager placementManager = default;

    static Game instance;

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
        playerHealth = startingPlayerHealth;
        board.Initialize(boardSize, tileContentFactory);
        board.ShowGrid = true;
        activeScenario = scenario.Begin();
        placementManager = GetComponent<StructurePlacementManager>();
    }

    private void OnEnable()
    {
        instance = this;
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            HandleHold();
            if(Input.GetMouseButtonUp(0))
                HandleRelease();
        }
        if (Input.GetMouseButtonDown(1))
            HandleAlternativeTouch();

        if (Input.GetKeyDown(KeyCode.V))
        {
            board.ShowPaths = !board.ShowPaths;
        }

        if(Input.GetKeyDown(KeyCode.G))
        {
            board.ShowGrid = !board.ShowGrid;
        }

        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedTowerType = TowerType.Laser;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedTowerType = TowerType.Mortar;
        }

        if(Input.GetKeyDown(KeyCode.B))
        {
            BeginNewGame();
        }

        gameCamera.ApplyInput(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")), Input.GetAxis("Rotate"), Input.mouseScrollDelta.y);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Time.timeScale = Time.timeScale > pausedTimeScale ? pausedTimeScale : playSpeed;
        }
        else if (Time.timeScale > pausedTimeScale)
        {
            Time.timeScale = playSpeed;
        }

        if (playerHealth <= 0 && startingPlayerHealth > 0)
        {
            Debug.Log("Defeat!");
            BeginNewGame();
        }

        if(!activeScenario.Progress() && enemies.IsEmpty)
        {
            Debug.Log("Victory");
            BeginNewGame();
            activeScenario.Progress();
        }

        enemies.GameUpdate();
        Physics.SyncTransforms();
        board.GameUpdate();
        nonEnemies.GameUpdate();
        gameCamera.GameUpdate();
    }

    private void BeginNewGame()
    {
        playerHealth = startingPlayerHealth;
        enemies.Clear();
        nonEnemies.Clear();
        board.Clear();
        activeScenario = scenario.Begin();
    }

    void HandleHold()
    {
        if(!EventSystem.current.IsPointerOverGameObject())
        {
            GameTile tile = board.GetTile(TouchRay);
            placementManager.InitiatePlacement(tile, board);
        }
    }
    void HandleRelease()
    {
        placementManager.ConfirmPlacement();
    }

    void HandleAlternativeTouch()
    {
        placementManager.CancelPlacement();
    }

    public static void SpawnEnemy(EnemyFactory factory, EnemyType type)
    {
        GameTile spawnPoint = instance.board.GetSpawnPoint(Random.Range(0, instance.board.spawnPointCount));
        Enemy enemy = factory.Get(type);
        enemy.SpawnOn(spawnPoint);
        instance.enemies.Add(enemy);
    }

    public static Shell SpawnShell()
    {
        Shell shell = instance.warFactory.Shell;
        instance.nonEnemies.Add(shell);
        return shell;
    }

    public static Explosion SpawnExplosion()
    {
        Explosion explosion = instance.warFactory.Explosion;
        instance.nonEnemies.Add(explosion);
        return explosion;
    }

    public static void EnemyReachedDestination()
    {
        instance.playerHealth -= 1;
    }
}
