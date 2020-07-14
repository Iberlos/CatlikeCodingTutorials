using UnityEngine;

public class ArcherTower : Tower
{
    [SerializeField, Range(0.5f, 2f)]
    float shotsPerSecond = 2f;

    [SerializeField]
    Transform mortar = default;

    [SerializeField, Range(0.5f, 3f)]
    float shellBlastRadius = 0.1f;

    [SerializeField, Range(1f, 100f)]
    float shellDamage = 3f;

    private float launchSpeed;
    private float launchProgress;
    private float precision = 0.2f;

    public override TowerType TowerType => TowerType.Archer;

    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        float x = targetingRange + 0.25001f;
        float y = -mortar.position.y;
        launchSpeed = 3f*Mathf.Sqrt(-Physics.gravity.y * (y + Mathf.Sqrt(x * x + y * y)));
    }

    public override void GameUpdate()
    {
        launchProgress += shotsPerSecond * Time.deltaTime;
        while(launchProgress >= 1f)
        {
            if(AcquireTarget(out TargetPoint target))
            {
                Launch(target);
                launchProgress -= 1f;
            }
            else
            {
                launchProgress = 0.999f;
            }
        }
    }

    public void Launch(TargetPoint target)
    {
        Vector3 launchPoint = mortar.position;
        Vector3 targetPoint = target.Position + target.Enemy.Velocity * 1f/launchSpeed*Random.Range(1f-precision, 1f+precision);

        Vector2 dir;
        dir.x = targetPoint.x - launchPoint.x;
        dir.y = targetPoint.z - launchPoint.z;
        float x = dir.magnitude;
        float y = -launchPoint.y;
        dir /= x;

        float g = -Physics.gravity.y;
        float s = launchSpeed;
        float s2 = s * s;

        float r = s2 * s2 - g * (g * x * x + 2f * y * s2);
        float tanTheta = (s2 - Mathf.Sqrt(r)) / (g * x);
        float cosTheta = Mathf.Cos(Mathf.Atan(tanTheta));
        float sinTheta = cosTheta * tanTheta;

        mortar.localRotation = Quaternion.LookRotation(new Vector3(dir.x, tanTheta, dir.y));

        Game.SpawnArrow().Initialize(launchPoint, targetPoint, new Vector3(s * cosTheta * dir.x, s * sinTheta, s * cosTheta * dir.y), shellBlastRadius, shellDamage);
    }
}