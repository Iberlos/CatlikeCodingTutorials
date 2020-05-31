using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : PersistableObject
{
    [SerializeField]
    private PersistentStorage storage;
    [SerializeField]
    private PersistableObject prefab;

    [SerializeField]
    private KeyCode createKey = KeyCode.C;
    [SerializeField]
    private KeyCode newGameKey = KeyCode.N;
    [SerializeField]
    private KeyCode saveKey = KeyCode.S;
    [SerializeField]
    private KeyCode loadKey = KeyCode.L;

    private List<PersistableObject> objects;

    // Start is called before the first frame update
    void Awake()
    {
        objects = new List<PersistableObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(createKey))
        {
            CreateObject();
        }
        else if(Input.GetKey(newGameKey))
        {
            BeginNewGame();
        }
        else if (Input.GetKeyDown(saveKey))
        {
            storage.Save(this);
        }
        else if (Input.GetKeyDown(loadKey))
        {
            storage.Load(this);
        }
    }

    void CreateObject()
    {
        PersistableObject o = Instantiate(prefab);
        Transform t = o.transform;
        t.localPosition = Random.insideUnitSphere * 5f;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * Random.Range(0.1f, 1f);
        objects.Add(o);
    }

    void BeginNewGame()
    {
        foreach(PersistableObject obj in objects)
        {
            Destroy(obj.gameObject);
        }
        objects.Clear();
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(objects.Count);
        foreach(PersistableObject obj in objects)
        {
            obj.Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        BeginNewGame();
        int count = reader.ReadInt();
        for(int i = 0; i< count; i++)
        {
            PersistableObject o = Instantiate(prefab);
            o.Load(reader);
            objects.Add(o);
        }
    }
}
