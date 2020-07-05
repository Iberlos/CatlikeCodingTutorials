using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralRoundedCube : MonoBehaviour
{
    public int xSize, ySize, zSize;
    public int roundness;
    public float delay = 0.05f;
    private Vector3[] vertices;
    private Vector3[] normals;
    private Mesh mesh;
    private Coroutine coroutine;
    private Color32[] cubeUV;

    private void Awake()
    {
        coroutine = StartCoroutine(Generate());
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.materials[0].color = Color.red;
        renderer.materials[1].color = Color.green;
        renderer.materials[2].color = Color.blue;
        CreateColliders();
    }

    private void CreateColliders()
    {
        AddBoxCollider(xSize, ySize - roundness * 2, zSize - roundness * 2);
        AddBoxCollider(xSize - roundness * 2, ySize, zSize - roundness * 2);
        AddBoxCollider(xSize - roundness * 2, ySize - roundness * 2, zSize);

        Vector3 min = Vector3.one * roundness;
        Vector3 half = new Vector3(xSize, ySize, zSize) * 0.5f;
        Vector3 max = new Vector3(xSize, ySize, zSize) - min;

        AddCapsuleCollider(0, half.x, min.y, min.z);
        AddCapsuleCollider(0, half.x, min.y, max.z);
        AddCapsuleCollider(0, half.x, max.y, min.z);
        AddCapsuleCollider(0, half.x, max.y, max.z);

        AddCapsuleCollider(1, min.x, half.y, min.z);
        AddCapsuleCollider(1, min.x, half.y, max.z);
        AddCapsuleCollider(1, max.x, half.y, min.z);
        AddCapsuleCollider(1, max.x, half.y, max.z);

        AddCapsuleCollider(2, min.x, min.y, half.z);
        AddCapsuleCollider(2, min.x, max.y, half.z);
        AddCapsuleCollider(2, max.x, min.y, half.z);
        AddCapsuleCollider(2, max.x, max.y, half.z);
    }

    private void AddCapsuleCollider(int direction, float x, float y, float z)
    {
        CapsuleCollider c = gameObject.AddComponent<CapsuleCollider>();
        c.center = new Vector3(x, y, z);
        c.direction = direction;
        c.radius = roundness;
        c.height = c.center[direction] * 2f;
    }

    private void AddBoxCollider(float x, float y, float z)
    {
        BoxCollider c = gameObject.AddComponent<BoxCollider>();
        c.size = new Vector3(x, y, z);
    }

    private static int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)
    {
        triangles[i] = v00;
        triangles[i + 1] = triangles[i + 4] = v01;
        triangles[i + 2] = triangles[i + 3] = v10;
        triangles[i + 5] = v11;
        return i + 6;
    }

    private void SetVertex(int i, int x, int y, int z)
    {
        Vector3 inner = vertices[i] = new Vector3(x, y, z);

        if (x < roundness)
        {
            inner.x = roundness;
        }
        else if (x > xSize - roundness)
        {
            inner.x = xSize - roundness;
        }
        if (y < roundness)
        {
            inner.y = roundness;
        }
        else if (y > ySize - roundness)
        {
            inner.y = ySize - roundness;
        }
        if (z < roundness)
        {
            inner.z = roundness;
        }
        else if (z > zSize - roundness)
        {
            inner.z = zSize - roundness;
        }

        normals[i] = (vertices[i] - inner).normalized;
        vertices[i] = inner + normals[i] * roundness;
        cubeUV[i] = new Color32((byte)x, (byte)y, (byte)z, 0);
    }

    private IEnumerator Generate()
    {
        //if(mesh == null)
            GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Cube";
        WaitForSeconds wait = null;
        if (delay > 0f)
            wait = new WaitForSeconds(delay);
        yield return CreateVertices(wait);
        yield return CreateTriangles(wait);
    }

    private IEnumerator CreateVertices(WaitForSeconds wait)
    {
        int cornerVertices = 8;
        int edgeVertices = (xSize + ySize + zSize - 3) * 4;
        int faceVertices = ((xSize - 1) * (ySize - 1) + (xSize - 1) * (zSize - 1) + (ySize - 1) * (zSize - 1)) * 2;

        vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
        normals = new Vector3[vertices.Length];
        cubeUV = new Color32[vertices.Length];

        int v = 0;
        for (int y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                SetVertex(v++, x, y, 0);
                if(wait != null)
                    yield return wait;
            }
            for (int z = 1; z <= zSize; z++)
            {
                SetVertex(v++, xSize, y, z);
                if(wait != null)
                    yield return wait;
            }
            for (int x = xSize - 1; x >= 0; x--)
            {
                SetVertex(v++, x, y, zSize);
                if (wait != null)
                    yield return wait;
            }
            for (int z = zSize - 1; z > 0; z--)
            {
                SetVertex(v++, 0, y, z);
                if (wait != null)
                    yield return wait;
            }
        }
        for (int z = 1; z < zSize; z++)
        {
            for (int x = 1; x < xSize; x++)
            {
                SetVertex(v++, x, ySize, z);
                if (wait != null)
                    yield return wait;
            }
        }
        for (int z = 1; z < zSize; z++)
        {
            for (int x = 1; x < xSize; x++)
            {
                SetVertex(v++, x, 0, z);
                if (wait != null)
                    yield return wait;
            }
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.colors32 = cubeUV;
    }

    private IEnumerator CreateTriangles(WaitForSeconds wait)
    {
        int[] trianglesX = new int[ySize * zSize * 12];
        int[] trianglesY = new int[xSize * zSize * 12];
        int[] trianglesZ = new int[xSize * ySize * 12];

        int ring = (xSize + zSize) * 2;
        int tX = 0, tY = 0, tZ = 0, v = 0;
        mesh.subMeshCount = 3;
        for (int y = 0; y < ySize; y++, v++)
        {
            for (int q = 0; q < xSize; q++, v++)
            {
                tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
                mesh.SetTriangles(trianglesZ, 2);
                if (wait != null)
                    yield return wait;
            }
            for (int q = 0; q < zSize; q++, v++)
            {
                tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
                mesh.SetTriangles(trianglesX, 0);
                if (wait != null)
                    yield return wait;
            }
            for (int q = 0; q < xSize; q++, v++)
            {
                tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
                mesh.SetTriangles(trianglesZ, 2);
                if (wait != null)
                    yield return wait;
            }
            for (int q = 0; q < zSize - 1; q++, v++)
            {
                tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
                mesh.SetTriangles(trianglesX, 0);
                if (wait != null)
                    yield return wait;
            }
            tX = SetQuad(trianglesX, tX, v, v - ring + 1, v + ring, v + 1);
            mesh.SetTriangles(trianglesX, 0);
            if (wait != null)
                yield return wait;
        }
        int[] tArray = { tY };
        yield return CreateTopFace(wait, trianglesY, tArray, ring);
        yield return CreateBottomFace(wait, trianglesY, tArray, ring);
        tY = tArray[0];
    }

    private IEnumerator CreateTopFace(WaitForSeconds wait, int[] trianglesY, int[] t, int ring)
    {
        int v = ring * ySize;
        for(int x = 0; x < xSize-1; x++, v++)
        {
            t[0] = SetQuad(trianglesY, t[0],  v, v + 1, v + ring - 1, v + ring);
            mesh.SetTriangles(trianglesY, 1);
            if (wait != null)
                yield return wait;
        }
        t[0] = SetQuad(trianglesY, t[0], v, v + 1, v + ring - 1, v + 2);
        mesh.SetTriangles(trianglesY, 1);
        if (wait != null)
            yield return wait;

        int vMin = ring * (ySize + 1) - 1;
        int vMid = vMin + 1;
        int vMax = v + 2;
        for(int z =1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
        {
            t[0] = SetQuad(trianglesY, t[0], vMin, vMid, vMin - 1, vMid + xSize - 1);
            mesh.SetTriangles(trianglesY, 1);
            if (wait != null)
                yield return wait;
            for (int x = 1; x < xSize - 1; x++, vMid++)
            {
                t[0] = SetQuad(trianglesY, t[0], vMid, vMid + 1, vMid + xSize - 1, vMid + xSize);
                mesh.SetTriangles(trianglesY, 1);
                if (wait != null)
                    yield return wait;
            }
            t[0] = SetQuad(trianglesY, t[0], vMid, vMax, vMid + xSize - 1, vMax + 1);
            mesh.SetTriangles(trianglesY, 1);
            if (wait != null)
                yield return wait;
        }
        int vTop = vMin - 2;
        t[0] = SetQuad(trianglesY, t[0], vMin, vMid, vTop + 1, vTop);
        mesh.SetTriangles(trianglesY, 1);
        if (wait != null)
            yield return wait;
        for (int x = 1; x < xSize - 1; x++, vTop--, vMid++)
        {
            t[0] = SetQuad(trianglesY, t[0], vMid, vMid + 1, vTop, vTop - 1);
            mesh.SetTriangles(trianglesY, 1);
            if (wait != null)
                yield return wait;
        }
        t[0] = SetQuad(trianglesY, t[0], vMid, vTop - 2, vTop, vTop - 1);
        mesh.SetTriangles(trianglesY, 1);
        if (wait != null)
            yield return wait;
    }

    private IEnumerator CreateBottomFace(WaitForSeconds wait, int[] trianglesY, int[] t, int ring)
    {
        int v = 1;
        int vMid = vertices.Length - (xSize - 1) * (zSize - 1);
        t[0] = SetQuad(trianglesY, t[0], ring - 1, vMid, 0, 1);
        mesh.SetTriangles(trianglesY, 1);
        if (wait != null)
            yield return wait;
        for (int x = 1; x < xSize -1; x++, v++, vMid++)
        {
            t[0] = SetQuad(trianglesY, t[0], vMid, vMid + 1, v, v + 1);
            mesh.SetTriangles(trianglesY, 1);
            if (wait != null)
                yield return wait;
        }
        t[0] = SetQuad(trianglesY, t[0], vMid, v + 2, v, v + 1);
        mesh.SetTriangles(trianglesY, 1);
        if (wait != null)
            yield return wait;

        int vMin = ring - 2;
        vMid -= xSize - 2;
        int vMax = v + 2;

        for(int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
        {
            t[0] = SetQuad(trianglesY, t[0], vMin, vMid + xSize - 1, vMin + 1, vMid);
            mesh.SetTriangles(trianglesY, 1);
            if (wait != null)
                yield return wait;
            for (int x = 1; x < xSize -1; x++, vMid++)
            {
                t[0] = SetQuad(trianglesY, t[0], vMid + xSize -1, vMid + xSize, vMid, vMid + 1);
                mesh.SetTriangles(trianglesY, 1);
                if (wait != null)
                    yield return wait;
            }
            t[0] = SetQuad(trianglesY, t[0], vMid + xSize - 1, vMax + 1, vMid, vMax);
            mesh.SetTriangles(trianglesY, 1);
            if (wait != null)
                yield return wait;
        }

        int vTop = vMin - 1;
        t[0] = SetQuad(trianglesY, t[0], vTop + 1, vTop, vTop + 2, vMid);
        mesh.SetTriangles(trianglesY, 1);
        if (wait != null)
            yield return wait;
        for (int x = 1; x < xSize - 1; x++, vTop--, vMid++)
        {
            t[0] = SetQuad(trianglesY, t[0], vTop, vTop - 1, vMid, vMid + 1);
            mesh.SetTriangles(trianglesY, 1);
            if (wait != null)
                yield return wait;
        }
        t[0] = SetQuad(trianglesY, t[0], vTop, vTop - 1, vMid, vTop - 2);
        mesh.SetTriangles(trianglesY, 1);
        if (wait != null)
            yield return wait;
    }

    //private void OnDrawGizmos()
    //{
    //    if (vertices == null) return;
    //    for (int i = 0; i < vertices.Length; i++)
    //    {
    //        if(vertices[i] != null)
    //        {
    //            Gizmos.color = Color.black;
    //            Gizmos.DrawWireSphere(vertices[i], 0.1f);
    //        }
    //        if(normals[i] != null)
    //        {
    //            Gizmos.color = new Color(Mathf.Abs(normals[i].x), Mathf.Abs(normals[i].y), Mathf.Abs(normals[i].z), 1f);
    //            Gizmos.DrawRay(vertices[i], normals[i]);
    //        }
    //    }
    //}
}
