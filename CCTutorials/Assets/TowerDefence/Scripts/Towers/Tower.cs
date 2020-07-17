using UnityEngine;

public abstract class Tower : Buildable
{
    [SerializeField, Range(1.5f, 10.5f)]
    protected float targetingRange = 1.5f;
    [SerializeField]
    protected AudioClip firingSound = default;

    public abstract TowerType TowerType { get; }

    public override int Variation => (int)TowerType;

    protected bool AcquireTarget(out TargetPoint target)
    {
        if(TargetPoint.FillBuffer(transform.localPosition, targetingRange))
        {
            target = TargetPoint.RandomBuffered;
            return true;
        }
        target = null;
        return false;
    }

    protected bool TrackTarget(ref TargetPoint target)
    {
        if (target == null || !target.Enemy.IsValidTarget)
            return false;

        if (target == null)
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 position = transform.localPosition;
        position.y += 0.01f;
        Gizmos.DrawWireSphere(position, targetingRange);
    }

    protected virtual void PlayTowerSound(AudioClip clip)
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = clip;
        audioSource.loop = false;
        audioSource.Play();
    }
}
