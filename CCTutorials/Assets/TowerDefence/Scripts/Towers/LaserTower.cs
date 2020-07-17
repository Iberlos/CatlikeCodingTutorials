using UnityEngine;

public class LaserTower : Tower
{
    [SerializeField, Range(1f, 100f)]
    private float damagePerSecond = 10f;
    [SerializeField]
    private Transform turret = default, laserBeam = default;

    private TargetPoint target;
    private Vector3 laserBeamScale;

    public override TowerType TowerType => TowerType.Laser;

    static Collider[] targetsBuffer = new Collider[100];

    void Awake()
    {
        laserBeamScale = laserBeam.localScale;
    }

    public override void GameUpdate()
    {
        if (TrackTarget(ref target) || AcquireTarget(out target))
        {
            if(audioSource.clip == firingSound || !audioSource.isPlaying)
                PlayTowerSound(firingSound);
            Shoot();
        }
        else
        {
            audioSource.Stop();
            laserBeam.localScale = Vector3.zero;
        }
    }

    void Shoot()
    {
        Vector3 point = target.Position;
        turret.LookAt(point);
        laserBeam.localRotation = turret.localRotation;

        float d = Vector3.Distance(turret.position, point);
        laserBeamScale.z = d;
        laserBeam.localScale = laserBeamScale;

        laserBeam.localPosition = turret.localPosition + 0.5f * d * laserBeam.forward;

        target.Enemy.ApplyDamage(damagePerSecond * Time.deltaTime);
    }

    protected override void PlayTowerSound(AudioClip clip)
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }
}
