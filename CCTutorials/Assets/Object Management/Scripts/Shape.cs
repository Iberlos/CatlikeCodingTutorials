using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : PersistableObject
{
    public int MaterialId { get; private set; }
    public Vector3 AngularVelocity { get; set; }
    public Vector3 Velocity { get; set; }
    public void SetMaterial(Material material, int materialId)
    {
        meshRenderer.material = material;
        MaterialId = materialId;
    }

    public int ShapeId
        {
        get { return shapeId; }
        set {
            if(shapeId == int.MinValue && value != int.MinValue)
                shapeId = value;
            }
        }
    private int shapeId = int.MinValue;

    Color color;
    public void SetColor(Color color)
    {
        this.color = color;
        if(materialPropertyBlock == null)
        {
            materialPropertyBlock = new MaterialPropertyBlock();
        }
        materialPropertyBlock.SetColor(colorPropertyId, color);
        meshRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    static int colorPropertyId = Shader.PropertyToID("_Color");
    static MaterialPropertyBlock materialPropertyBlock;

    MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void GameUpdate()
    {
        transform.Rotate(AngularVelocity * Time.fixedDeltaTime);
        transform.localPosition += Velocity * Time.fixedDeltaTime;
    }

    public override void Save(GameDataWriter writer)
    {
        base.Save(writer);
        writer.Write(color);
        writer.Write(AngularVelocity);
        writer.Write(Velocity);
    }

    public override void Load(GameDataReader reader)
    {
        base.Load(reader);
        SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
        AngularVelocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
        Velocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
    }
}
