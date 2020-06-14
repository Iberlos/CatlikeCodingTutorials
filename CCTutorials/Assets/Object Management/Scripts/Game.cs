using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : PersistableObject
{
    [SerializeField]
    private PersistentStorage storage;
    [SerializeField]
    ShapeFactory[] shapeFactories;

    [SerializeField]
    private Slider creationSpeedSlider;
    [SerializeField]
    private Slider destructionSpeedSlider;
    [SerializeField]
    private Text activeShapeCounterLable;

    [SerializeField]
    private float destroyDuration;

    [SerializeField]
    private KeyCode createKey = KeyCode.C;
    [SerializeField]
    private KeyCode destroyKey = KeyCode.X;
    [SerializeField]
    private KeyCode newGameKey = KeyCode.N;
    [SerializeField]
    private KeyCode saveKey = KeyCode.S;
    [SerializeField]
    private KeyCode loadKey = KeyCode.L;

    [SerializeField]
    private int levelCount;
    [SerializeField]
    private bool reseedOnLoad;

    public static Game Instance { get; private set;}

    public float CreationSpeed { get; set; }
    public float DestructionSpeed { get; set; }

    private Random.State mainRandomState;
    private List<Shape> shapes;
    private List<ShapeInstance> killList, markAsDyingList;
    private const int saveVersion = 7;
    private float creationProgress, destructionProgress;
    private int loadedLevelBuildIndex;
    private bool inGameUpdateLoop;
    private int dyingShapeCount;

    // Start is called before the first frame update
    void Start()
    {
        mainRandomState = Random.state;
        shapes = new List<Shape>();
        killList = new List<ShapeInstance>();
        markAsDyingList = new List<ShapeInstance>();

        if (Application.isEditor)
        {
            for(int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(i);
                if(loadedScene.name.Contains("Level "))
                {
                    SceneManager.SetActiveScene(loadedScene);
                    loadedLevelBuildIndex = loadedScene.buildIndex;
                    return;
                }
            }
        }
        BeginNewGame();
        StartCoroutine(LoadLevel(1));
    }

    private void OnEnable()
    {
        Instance = this;

        if(shapeFactories[0].FactoryId != 0)//if the ids were not initialized yet
        {
            for (int i = 0; i < shapeFactories.Length; i++)
            {
                shapeFactories[i].FactoryId = i;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(createKey))
        {
            CreateShape();
        }
        else if (Input.GetKeyDown(destroyKey))
        {
            DestroyShape();
        }
        else if (Input.GetKey(newGameKey))
        {
            BeginNewGame();
            StartCoroutine(LoadLevel(loadedLevelBuildIndex));
        }
        else if (Input.GetKeyDown(saveKey))
        {
            storage.Save(this, saveVersion);
        }
        else if (Input.GetKeyDown(loadKey))
        {
            storage.Load(this);
        }
        else
        {
            for (int i = 1; i <= levelCount; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    BeginNewGame();
                    StartCoroutine(LoadLevel(i));
                    return;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        inGameUpdateLoop = true;
        foreach(Shape shape in shapes)
        {
            shape.GameUpdate();
        }

        creationProgress += Time.fixedDeltaTime * CreationSpeed;
        while (creationProgress >= 1f)
        {
            creationProgress -= 1;
            CreateShape();
        }

        destructionProgress += Time.fixedDeltaTime * DestructionSpeed;
        while (destructionProgress >= 1f)
        {
            destructionProgress -= 1;
            DestroyShape();
        }

        int limit = GameLevel.Current.PopulationLimit;
        if (limit > 0)
        {
            while (shapes.Count- dyingShapeCount > limit)
            {
                DestroyShape();
            }
        }
        GameLevel.Current.GameUpdate();
        inGameUpdateLoop = false;

        //kill shapes that were requested during the game update
        if(killList.Count > 0)
        {
            foreach (ShapeInstance shape in killList)
                if(shape.IsValid)
                    KillImmediatly(shape.Shape);
        }
        killList.Clear();

        //mark as dying the anoed that are dying
        if (markAsDyingList.Count > 0)
        {
            for (int i = 0; i < markAsDyingList.Count; i++)
            {
                if (markAsDyingList[i].IsValid)
                {
                    MarkAsDyingImmediately(markAsDyingList[i].Shape);
                }
            }
            markAsDyingList.Clear();
        }
    }

    void CreateShape()
    {
        GameLevel.Current.SpawnShapes();
        //update counter **ADED**
        activeShapeCounterLable.text = "Active Shapes = " + shapes.Count;
    }

    void DestroyShape()
    {
        if(shapes.Count - dyingShapeCount > 0)
        {
            Shape shape = shapes[Random.Range(dyingShapeCount, shapes.Count)];
            if (destroyDuration <= 0f)
            {
                KillImmediatly(shape);
            }
            else
            {
                shape.AddBehavior<DyingShapeBehavior>().Initialize(
                    shape, destroyDuration
                );
            }
            //update counter **ADED**
            activeShapeCounterLable.text = "Active Shapes = " + shapes.Count;
        }
    }

    void BeginNewGame()
    {
        Random.state = mainRandomState;
        int seed = Random.Range(0, int.MaxValue) ^ (int)Time.unscaledTime;
        mainRandomState = Random.state;
        Random.InitState(seed);
        CreationSpeed = 0;
        creationSpeedSlider.value = 0;
        DestructionSpeed = 0;
        destructionSpeedSlider.value = 0;
        foreach(Shape shape in shapes)
        {
            shape.Recycle();
        }
        shapes.Clear();
        dyingShapeCount = 0;
        //update counter **ADED**
        activeShapeCounterLable.text = "Active Shapes = " + shapes.Count;
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(shapes.Count);
        writer.Write(Random.state);
        writer.Write(CreationSpeed);
        writer.Write(creationProgress);
        writer.Write(DestructionSpeed);
        writer.Write(destructionProgress);
        writer.Write(loadedLevelBuildIndex);
        GameLevel.Current.Save(writer);

        for (int i = 0; i < shapes.Count; i++)
        {
            writer.Write(shapes[i].OriginFactory.FactoryId);
            writer.Write(shapes[i].ShapeId);
            writer.Write(shapes[i].MaterialId);
            shapes[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        BeginNewGame();
        int version = reader.Version;
        if (version > saveVersion)
        {
            Debug.LogError("Unsupported future save version " + version);
            return;
        }
        StartCoroutine(LoadGame(reader));
    }
    IEnumerator LoadGame(GameDataReader reader)
    {
        int version = reader.Version;
        int count = version <= 0 ? -version : reader.ReadInt();

        if (version >= 3)
        {
            Random.State state = reader.ReadRandomState();
            if (!reseedOnLoad)
            {
                Random.state = state;
            }
            creationSpeedSlider.value = CreationSpeed = reader.ReadFloat();
            creationProgress = reader.ReadFloat();
            destructionSpeedSlider.value = DestructionSpeed = reader.ReadFloat();
            destructionProgress = reader.ReadFloat();
        }

        yield return LoadLevel(version < 2 ? 1 : reader.ReadInt());
        if (version >= 3)
        {
            GameLevel.Current.Load(reader);
        }

        for (int i = 0; i < count; i++)
        {
            int factoryId = version >= 5 ? reader.ReadInt() : 0;
            int shapeId = version > 0 ? reader.ReadInt() : 0;
            int materialId = version > 0 ? reader.ReadInt() : 0;
            Shape instance = shapeFactories[factoryId].Get(shapeId, materialId);
            instance.Load(reader);
        }

        for (int i = 0; i < shapes.Count; i++)
        {
            shapes[i].ResolveShapeInstances();
        }
        //update counter **ADED**
        activeShapeCounterLable.text = "Active Shapes = " + shapes.Count;
    }

    IEnumerator LoadLevel(int levelBuildIndex)
    {
        enabled = false;
        if(loadedLevelBuildIndex > 0)
        {
            yield return SceneManager.UnloadSceneAsync(loadedLevelBuildIndex);
        }
        yield return SceneManager.LoadSceneAsync(levelBuildIndex, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(levelBuildIndex));
        loadedLevelBuildIndex = levelBuildIndex;
        enabled = true;
    }

    public void AddShape(Shape shape)
    {
        shape.SaveIndex = shapes.Count;
        shapes.Add(shape);
    }

    public Shape GetShape(int index)
    {
        return shapes[index];
    }

    public void Kill(Shape shape)
    {
        if (inGameUpdateLoop)
            killList.Add(shape);
        else
            KillImmediatly(shape);
    }

    private void KillImmediatly(Shape shape)
    {
        int index = shape.SaveIndex;
        shape.Recycle();

        if (index < dyingShapeCount && index < --dyingShapeCount)
        {
            shapes[dyingShapeCount].SaveIndex = index;
            shapes[index] = shapes[dyingShapeCount];
            index = dyingShapeCount;
        }

        int lastIndex = shapes.Count - 1;
        if (index < lastIndex)
        {
            shapes[lastIndex].SaveIndex = index;
            shapes[index] = shapes[lastIndex];
        }
        shapes.RemoveAt(lastIndex);
    }

    public void MarkAsDying(Shape shape)
    {
        if (inGameUpdateLoop)
        {
            markAsDyingList.Add(shape);
        }
        else
        {
            MarkAsDyingImmediately(shape);
        }
    }

    void MarkAsDyingImmediately(Shape shape)
    {
        int index = shape.SaveIndex;
        if (index < dyingShapeCount)
        {
            return;
        }
        shapes[dyingShapeCount].SaveIndex = index;
        shapes[index] = shapes[dyingShapeCount];
        shape.SaveIndex = dyingShapeCount;
        shapes[dyingShapeCount++] = shape;
    }

    public bool IsMarkedAsDying(Shape shape)
    {
        return shape.SaveIndex < dyingShapeCount;
    }
}
