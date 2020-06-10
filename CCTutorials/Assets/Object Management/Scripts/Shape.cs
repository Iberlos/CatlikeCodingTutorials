using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : PersistableObject
{
    [SerializeField]
    MeshRenderer[] meshRenderers;
    public int MaterialId { get; private set; }
    public int ShapeId
    {
        get { return shapeId; }
        set {
            if (shapeId == int.MinValue && value != int.MinValue)
                shapeId = value;
        }
    }
    public int ColorCount
    {
        get
        {
            return colors.Length;
        }
    }
    public ShapeFactory OriginFactory
    {
        get
        {
            return originFactory;
        }
        set
        {
            if (originFactory == null)
                originFactory = value;
            else
                Debug.LogError("Not allowed to change origin factory.");
        }
    }
    public float Age { get; private set;}
    public int InstanceId { get; private set; }
    public int SaveIndex { get; set; }

    ShapeFactory originFactory;
    private int shapeId = int.MinValue;
    Color[] colors;
    static int colorPropertyId = Shader.PropertyToID("_Color");
    static MaterialPropertyBlock sharedPropertyBlock;
    List<ShapeBehavior> behaviorList = new List<ShapeBehavior>();

    public void SetMaterial(Material material, int materialId)
    {
        foreach (MeshRenderer meshRenderer in meshRenderers)
            meshRenderer.material = material;
        MaterialId = materialId;
    }

    public void SetColor(Color color)
    {
        if (sharedPropertyBlock == null)
        {
            sharedPropertyBlock = new MaterialPropertyBlock();
        }
        sharedPropertyBlock.SetColor(colorPropertyId, color);
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            colors[i] = color;
            meshRenderers[i].SetPropertyBlock(sharedPropertyBlock);
        }
    }

    public void SetColor(Color color, int index)
    {
        if (sharedPropertyBlock == null)
        {
            sharedPropertyBlock = new MaterialPropertyBlock();
        }
        sharedPropertyBlock.SetColor(colorPropertyId, color);
        colors[index] = color;
        meshRenderers[index].SetPropertyBlock(sharedPropertyBlock);
    }

    public T AddBehavior<T> () where T:ShapeBehavior, new()
    {
        T behavior = ShapeBehaviorPool<T>.Get();
        behaviorList.Add(behavior);
        return behavior;
    }

    private void Awake()
    {
        colors = new Color[meshRenderers.Length];
    }

    public void GameUpdate()
    {
        Age += Time.fixedDeltaTime;
        for (int i = 0; i < behaviorList.Count; i++)
        {
            if (!behaviorList[i].GameUpdate(this))
            {
                behaviorList[i].Recycle();
                behaviorList.RemoveAt(i--);
            }
        }
    }

    public override void Save(GameDataWriter writer)
    {
        base.Save(writer);
        writer.Write(colors.Length);
        for (int i = 0; i < colors.Length; i++)
        {
            writer.Write(colors[i]);
        }
        writer.Write(Age);
        writer.Write(behaviorList.Count);
        for (int i = 0; i < behaviorList.Count; i++)
        {
            writer.Write((int)behaviorList[i].BehaviorType);
            behaviorList[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        base.Load(reader);
        if (reader.Version >= 5)
        {
            LoadColors(reader);
        }
        else
        {
            SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
        }
        if (reader.Version >= 6)
        {
            Age = reader.ReadFloat();
            int behaviorCount = reader.ReadInt();
            for (int i = 0; i < behaviorCount; i++)
            {
                ShapeBehavior behavior =
                    ((ShapeBehaviorType)reader.ReadInt()).GetInstance();
                behaviorList.Add(behavior);
                behavior.Load(reader);
            }
        }
        else if (reader.Version >= 4)
        {
            AddBehavior<RotationShapeBehavior>().AngularVelocity =
                reader.ReadVector3();
            AddBehavior<MovementShapeBehavior>().Velocity = reader.ReadVector3();
        }
    }

    void LoadColors(GameDataReader reader)
    {
        int count = reader.ReadInt();
        int max = count <= colors.Length ? count : colors.Length;
        int i = 0;
        for (; i < max; i++)
        {
            SetColor(reader.ReadColor(), i);
        }
        if (count > colors.Length)
        {
            for (; i < count; i++)
            {
                reader.ReadColor();//wasting the extra bytes in the save file if there are too many colours saved.
            }
        }
        else if (count < colors.Length)
        {
            for (; i < colors.Length; i++)
            {
                SetColor(Color.white, i);//Seting the extra objects as white if the save file did not have that many colors
            }
        }
    }

    public void Recycle()
    {
        Age = 0;
        InstanceId++;
        foreach(ShapeBehavior behavior in behaviorList)
        {
            behavior.Recycle();
        }
        behaviorList.Clear();
        OriginFactory.Reclaim(this);
    }

    public void ResolveShapeInstances()
    {
        for (int i = 0; i < behaviorList.Count; i++)
        {
            behaviorList[i].ResolveShapeInstances();
        }
    }
}
