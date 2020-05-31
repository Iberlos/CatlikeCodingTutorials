using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : PersistableObject
{
    public int MaterialId { get; private set; }
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

    public override void Save(GameDataWriter writer)
    {
        base.Save(writer);
        writer.Write(color);
    }

    public override void Load(GameDataReader reader)
    {
        base.Load(reader);
        SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
    }
}
