using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLevel : PersistableObject
{
    [SerializeField]
    private SpawnZone spawnZone;
    [SerializeField]
    PersistableObject[] persistableObjects;

    public void ConfigureSpawn(Shape shape)
    {
        spawnZone.ConfigureSpawn(shape);
    }
    public static GameLevel Current { get; private set; }

    private void OnEnable()
    {
        Current = this;
        if(persistableObjects == null)
        {
            persistableObjects = new PersistableObject[0];
        }
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(persistableObjects.Length);
        foreach(PersistableObject obj in persistableObjects)
        {
            obj.Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        int savedCount = reader.ReadInt();
        for (int i = 0; i<savedCount; i++)
        {
            persistableObjects[i].Load(reader);
        }
    }
}
