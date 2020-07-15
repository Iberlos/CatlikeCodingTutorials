﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    [SerializeField]
    private CameraBehavior gameCamera = default;
    [SerializeField]
    private Vector2Int boardSize = new Vector2Int(11, 11);
    [SerializeField]
    public GameBoard board = default;
    [SerializeField]
    private GameTileContentFactory tileContentFactory = default;
    [SerializeField]
    private WarFactory warFactory = default;
    [SerializeField]
    private GameScenario scenario = default;
    [SerializeField]
    private Image scenarioTextWindow;
    [SerializeField]
    private Text scenarioText;
    [SerializeField]
    private ScenarioTimeBar scenarioTimeBar;
    [SerializeField, Range(0, 100)]
    private int startingPlayerHealth = 10;
    [SerializeField, Range(1f, 10f)]
    private float fastTimeScale = 1.5f;
    [Header("Map Generation")]
    [SerializeField]
    private GeneratorParams generatorParameters;

    private Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

    private GameBehaviorCollection enemies = new GameBehaviorCollection();
    private GameBehaviorCollection nonEnemies = new GameBehaviorCollection();
    private GameScenario.State activeScenario;
    public int playerHealth;
    private const float playTimeScale = 1f;
    private const float pausedTimeScale = 0f;
    private StructurePlacementManager placementManager = default;
    [HideInInspector]
    public ResourceWallet wallet = default;

    public GameSpeedState GameSpeedState { get; private set; }

    public static Game instance;

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
        instance = this;
        playerHealth = startingPlayerHealth;
        wallet = GetComponent<ResourceWallet>();
        wallet.Recycle();
        board.Initialize(boardSize, tileContentFactory, ref generatorParameters);
        board.ShowGrid = true;
        activeScenario = scenario.Begin();
        placementManager = GetComponent<StructurePlacementManager>();
        SetPlaySpeed(GameSpeedState.Playing);
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
        }
        if (Input.GetMouseButtonUp(0))
            HandleRelease();
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

        if(Input.GetKeyDown(KeyCode.B))
        {
            BeginNewGame();
            return;
        }

        gameCamera.ApplyInput(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")), Input.GetAxis("Rotate"), Input.mouseScrollDelta.y);

        if (playerHealth <= 0 && startingPlayerHealth > 0)
        {
            Debug.Log("Defeat!");
            BeginNewGame();
            return;
        }

        if(!activeScenario.Progress() && enemies.IsEmpty)
        {
            Debug.Log("Victory");
            BeginNewGame();
            activeScenario.Progress();
            return;
        }

        enemies.GameUpdate();
        Physics.SyncTransforms();
        board.GameUpdate();
        wallet.GameUpdate();
        nonEnemies.GameUpdate();
        gameCamera.GameUpdate();
    }

    private void BeginNewGame()
    {
        playerHealth = startingPlayerHealth;
        enemies.Clear();
        nonEnemies.Clear();
        board.GenerateMap(ref generatorParameters);
        activeScenario = scenario.Begin();
        wallet.Recycle();
        gameCamera.SetPositionAndRotation(Vector3.zero);
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

    public static Arrow SpawnArrow()
    {
        Arrow arrow = instance.warFactory.Arrow;
        instance.nonEnemies.Add(arrow);
        return arrow;
    }

    public static Explosion SpawnExplosion()
    {
        Explosion explosion = instance.warFactory.Explosion;
        instance.nonEnemies.Add(explosion);
        return explosion;
    }

    public static void EnemyReachedDestinationAtTile(GameTile tile)
    {
        Buildable b = tile.Content as Buildable;
        if(b!=null)
            b.TakeDamage(instance.board, tile);
    }

    public static bool IsTileOccupiedByBuilding(GameTile tile)
    {
        if(tile != null && (tile.Content.Type == GameTileContentType.Wall || tile.Content.Type == GameTileContentType.Tower))
        {
            Buildable b = tile.Content as Buildable;
            if (b != null)
                b.TakeDamage(instance.board, tile);
            return true;
        }
        return false;
    }

    public static void DisplayScenarioText(string text)
    {
        instance.scenarioTextWindow.gameObject.SetActive(true);
        instance.scenarioText.text = text;
    }

    public static void UpdateTimerBar(GameScenario scenario, float timer)
    {
        int secs = ((int)timer % 60);
        string text = ((int)(timer / 60f)).ToString() + ":" + (secs<10?"0":"") + secs.ToString();
        instance.scenarioTimeBar.UpdateTimeBar(timer/(scenario.initialDelay*60f), text);
    }

    public void ExitGame()
    {
        if(Application.isEditor)
            EditorApplication.ExecuteMenuItem("Edit/Play");
        else
            Application.Quit();
    }

    public void SetPlaySpeed(GameSpeedState state)
    {
        GameSpeedState = state;
        switch (state)
        {
            case GameSpeedState.Paused:
                {
                    Time.timeScale = pausedTimeScale;
                    break;
                }
            case GameSpeedState.Playing:
                {
                    Time.timeScale = playTimeScale;
                    break;
                }
            case GameSpeedState.Fast:
                {
                    Time.timeScale = fastTimeScale;
                    break;
                }
        }
    }
}
