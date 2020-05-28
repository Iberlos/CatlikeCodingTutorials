using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fractal : MonoBehaviour
{
    public bool useRandomMesh;
    public int meshToUse;
    public Mesh[] meshes;
    public Material material;
    public int maxDepth;
    [HideInInspector]
    private int depth;
    public float childScale;
    public float spawnProbabilityDecay;
    public float spawnProbability;
    public bool rotate;
    public bool randomSpeed;
    public float rotationSpeed;

    private static Vector3[] chilDirections =
    {
        Vector3.up,
        Vector3.right,
        Vector3.left,
        Vector3.forward,
        Vector3.back
    };

    private static Quaternion[] childOrientations =
    {
        Quaternion.identity,
        Quaternion.Euler(0, 0, -90),
        Quaternion.Euler(0, 0, 90),
        Quaternion.Euler(90, 0, 0),
        Quaternion.Euler(-90, 0, 0)

    };

    private Material[,] materials;



    private void InitilizeMaterials()
    {
        materials = new Material[maxDepth + 1, 2];
        for (int i = 0; i <= maxDepth; i++)
        {
            float t = i / (maxDepth - 1f);
            t *= t;
            materials[i, 0] = new Material(material);
            materials[i, 0].color = Color.Lerp(Color.white, Color.yellow, t);
            materials[i, 1] = new Material(material);
            materials[i, 1].color = Color.Lerp(Color.white, Color.cyan, t);
        }
        materials[maxDepth, 0].color = Color.magenta;
        materials[maxDepth, 1].color = Color.red;
    }

    private void Start()
    {
        if(materials == null)
        {
            InitilizeMaterials();
        }
        gameObject.AddComponent<MeshFilter>().mesh = meshes[useRandomMesh?Random.Range(0,meshes.Length):meshToUse];
        gameObject.AddComponent<MeshRenderer>().material = material;
        GetComponent<MeshRenderer>().material = materials[depth, Random.Range(0,2)];
        if(depth < maxDepth)
        {
            StartCoroutine(CreateChildren());
        }
    }

    private void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }

    public void Initialize(Fractal parent, int index)
    {
        meshes = parent.meshes;
        material = parent.material;
        maxDepth = parent.maxDepth;
        depth = parent.depth + 1;
        childScale = parent.childScale;
        materials = parent.materials;
        spawnProbabilityDecay = parent.spawnProbabilityDecay;
        spawnProbability = parent.spawnProbability * (1f - spawnProbabilityDecay);
        rotate = parent.rotate;
        randomSpeed = parent.randomSpeed;
        rotationSpeed = randomSpeed?Random.Range(-parent.rotationSpeed, parent.rotationSpeed) :parent.rotationSpeed;
        transform.SetParent(parent.transform);
        transform.localScale = Vector3.one * childScale;
        transform.localPosition = chilDirections[index] * (0.5f + 0.5f * childScale);
        transform.localRotation = childOrientations[index];
        //gameObject.name = "FractalChild " + depth.ToString();
    }

    private IEnumerator CreateChildren()
    {
        int childDepth = depth + 1;
        for(int i =0; i < chilDirections.Length; i++)
        {
            if(Random.value < spawnProbability)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
                new GameObject("FractalChild " + childDepth.ToString()).AddComponent<Fractal>().Initialize(this, i);
            }
        }
    }
}
