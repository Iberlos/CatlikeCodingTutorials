using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameDataWriter
{
    BinaryWriter writer;

    public GameDataWriter(BinaryWriter writer)
    {
        this.writer = writer;
    }

    public void Write(float value)
    {
        writer.Write(value);
    }

    public void Write(int value)
    {
        writer.Write(value);
    }

    public void Write(Quaternion value)
    {
        this.Write(value.x);
        this.Write(value.y);
        this.Write(value.z);
        this.Write(value.w);
    }

    public void Write(Vector3 value)
    {
        this.Write(value.x);
        this.Write(value.y);
        this.Write(value.z);
    }
}
