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
    private const int saveVersion = 6;
    private float creationProgress, destructionProgress;
    private int loadedLevelBuildIndex;

    // Start is called before the first frame update
    void Start()
    {
        mainRandomState = Random.state;
        shapes = new List<Shape>();

        if(Application.isEditor)
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
            while (shapes.Count > limit)
            {
                DestroyShape();
            }
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
        if(shapes.Count > 0)
        {
            int index = Random.Range(0, shapes.Count);
            shapes[index].Recycle();
            int lastIndex = shapes.Count - 1;
            shapes[lastIndex].SaveIndex = index;
            shapes[index] = shapes[lastIndex];
            shapes.RemoveAt(lastIndex);
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
        foreach(Shape shape in shapes)
        {
            writer.Write(shape.OriginFactory.FactoryId);
            writer.Write(shape.ShapeId);
            writer.Write(shape.MaterialId);
            shape.Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        BeginNewGame();
        int version = reader.Version;
        //version support
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
        if(version >= 3)
        {
            Random.State state = reader.ReadRandomState();
            if(!reseedOnLoad)
            {
                Random.state = state;
            }
            creationSpeedSlider.value = CreationSpeed = reader.ReadFloat();
            creationProgress = reader.ReadFloat();
            destructionSpeedSlider.value = DestructionSpeed = reader.ReadFloat();
            destructionProgress = reader.ReadFloat();
        }
        yield return LoadLevel(version < 2 ? 1 : reader.ReadInt());
        if(version >= 3)
        {
            GameLevel.Current.Load(reader);
        }
        //load objects
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
}
