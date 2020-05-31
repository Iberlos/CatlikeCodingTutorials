using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class GameDataReader
{
    public int Version { get; }

    BinaryReader reader;

    public GameDataReader (BinaryReader reader, int version)
    {
        this.reader = reader;
        this.Version = version;
    }

    public float ReadFloat()
    {
        return reader.ReadSingle();
    }

    public int ReadInt()
    {
        return reader.ReadInt32();
    }

    public Quaternion ReadQuaternion()
    {
        Quaternion value;
        value.x = this.ReadFloat();
        value.y = this.ReadFloat();
        value.z = this.ReadFloat();
        value.w = this.ReadFloat();
        return value;
    }

    public Vector3 ReadVector3()
    {
        Vector3 value;
        value.x = this.ReadFloat();
        value.y = this.ReadFloat();
        value.z = this.ReadFloat();
        return value;
    }

    public Color ReadColor()
    {
        Color value;
        value.r = this.ReadFloat();
        value.g = this.ReadFloat();
        value.b = this.ReadFloat();
        value.a = this.ReadFloat();
        return value;
    }
}
