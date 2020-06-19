using UnityEngine;

public class Tower : GameTileContent
{
    [SerializeField]
    private Transform turret = default, beam = default;
    [SerializeField, Range(1.5f, 10.5f)]
    private float targetingRange = 1.5f;
    [SerializeField, Range(1f, 100f)]
    private float damagePerSecond = 10f;

    private TargetPoint target;
    private const int enemyLayerMask = 1 << 9;
    private Vector3 beamScale;

    static Collider[] targetsBuffer = new Collider[100];

    void Awake()
    {
        beamScale = beam.localScale;
    }

    public override void GameUpdate()
    {
        if (TrackTarget())
        {
            Debug.Log("Tracking Target...");
            Shoot();
        }
        else if (AcquireTarget())
            Debug.Log("Acquired Target!");
        else
        {
            Debug.Log("Searching for target...");
            beam.localScale = Vector3.zero;
        }
    }

    bool AcquireTarget()
    {
        Vector3 a = transform.localPosition;
        Vector3 b = a;
        b.y += 3f;
        int hits = Physics.OverlapCapsuleNonAlloc(a, b, targetingRange, targetsBuffer, enemyLayerMask);
        if(hits > 0)
        {
            target = targetsBuffer[Random.Range(0, hits)].GetComponent<TargetPoint>();
            Debug.Assert(target != null, "Targeted non-enemy!", targetsBuffer[0]);
            return true;
        }
        target = null;
        return false;
    }

    bool TrackTarget()
    {
        if(target == null)
        {
            return false;
        }
        Vector3 a = transform.localPosition;
        Vector3 b = target.Position;
        float x = a.x - b.x;
        float z = a.z - b.z;
        float r = targetingRange + 0.125f * target.Enemy.Scale;
        if (x * x + z * z > r * r)
        {
            target = null;
            return false;
        }
        return true;
    }

    void Shoot()
    {
        Vector3 point = target.Position;
        turret.LookAt(point);
        beam.localRotation = turret.localRotation;

        float d = Vector3.Distance(turret.position, point);
        beamScale.z = d;
        beam.localScale = beamScale;

        beam.localPosition = turret.localPosition + 0.5f * d * beam.forward;

        target.Enemy.ApplyDamage(damagePerSecond * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 position = transform.localPosition;
        position.y += 0.01f;
        Gizmos.DrawWireSphere(position, targetingRange);
        Gizmos.color = Color.red;
        if (target != null)
        {
            Gizmos.DrawLine(position, target.Position);
        }
    }
}
