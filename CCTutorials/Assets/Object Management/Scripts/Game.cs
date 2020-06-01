﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : PersistableObject
{
    [SerializeField]
    private PersistentStorage storage;
    [SerializeField]
    private ShapeFactory shapeFactory;

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

    public float CreationSpeed { get; set; }
    public float DestructionSpeed { get; set; }

    private List<Shape> shapes;
    private const int saveVersion = 1;
    private float creationProgress, destructionProgress;

    // Start is called before the first frame update
    void Awake()
    {
        shapes = new List<Shape>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(createKey))
        {
            CreateShape();
        }
        else if (Input.GetKeyDown(destroyKey))
        {
            DestroyShape();
        }
        else if(Input.GetKey(newGameKey))
        {
            BeginNewGame();
        }
        else if (Input.GetKeyDown(saveKey))
        {
            storage.Save(this, saveVersion);
        }
        else if (Input.GetKeyDown(loadKey))
        {
            storage.Load(this);
        }

        creationProgress += Time.deltaTime * CreationSpeed;
        while (creationProgress >= 1f)
        {
            creationProgress -= 1;
            CreateShape();
        }

        destructionProgress += Time.deltaTime * DestructionSpeed;
        while (destructionProgress >= 1f)
        {
            destructionProgress -= 1;
            DestroyShape();
        }
    }

    void CreateShape()
    {
        Shape instance = shapeFactory.GetRandom();
        Transform t = instance.transform;
        t.localPosition = Random.insideUnitSphere * 5f;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * Random.Range(0.1f, 1f);
        instance.SetColor(Random.ColorHSV
            (//you can name parameters being passed in!
            hueMin:0f, hueMax:1f, 
            saturationMin:0.5f, saturationMax:1f,
            valueMin:0.25f, valueMax:1f,
            alphaMin:1f, alphaMax:1f
            )
        );
        shapes.Add(instance);
    }

    void DestroyShape()
    {
        if(shapes.Count > 0)
        {
            int index = Random.Range(0, shapes.Count);
            shapeFactory.Reclaim(shapes[index]);
            int lastIndex = shapes.Count - 1;
            shapes[index] = shapes[lastIndex];
            shapes.RemoveAt(lastIndex);
        }
    }

    void BeginNewGame()
    {
        foreach(Shape shape in shapes)
        {
            shapeFactory.Reclaim(shape);
        }
        shapes.Clear();
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(shapes.Count);
        foreach(Shape shape in shapes)
        {
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
        if(version > saveVersion)
        {
            Debug.LogError("Unsupported future save version " + version);
            return;
        }
        int count = version <= 0 ? -version : reader.ReadInt();
        //load objects
        for(int i = 0; i< count; i++)
        {
            int shapeId = version > 0 ? reader.ReadInt() : 0;
            int materialId = version > 0 ? reader.ReadInt() : 0;
            Shape instance = shapeFactory.Get(shapeId, materialId);
            instance.Load(reader);
            shapes.Add(instance);
        }
    }
}
