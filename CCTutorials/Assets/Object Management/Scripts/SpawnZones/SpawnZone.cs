using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpawnZone : PersistableObject
{
    [System.Serializable]
    public struct SpawnConfiguration
    {
        public enum MovementDirection
        {
            Forward = 0,
            Upward,
            Outward,
            Inward,
            Random,
        }
        public MovementDirection movementDirection;
        public FloatRange spawnSpeed;
        public FloatRange angularSpeed;
        public FloatRange scale;
        public ColorRangeHSV color;
    }

    [SerializeField]
    SpawnConfiguration spawnConfig;

    public abstract Vector3 SpawnPoint { get; }

    public virtual void ConfigureSpawn(Shape shape)
    {
        Transform t = shape.transform;
        t.localPosition = SpawnPoint;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * spawnConfig.scale.RandomValueInRange;
        shape.SetColor(spawnConfig.color.RandomInRange);
        shape.AngularVelocity = Random.onUnitSphere * spawnConfig.angularSpeed.RandomValueInRange;

        Vector3 direction;
        switch (spawnConfig.movementDirection)
        {
            case SpawnConfiguration.MovementDirection.Upward:
                {
                    direction = transform.up;
                    break;
                }
            case SpawnConfiguration.MovementDirection.Forward:
                {
                    direction = transform.forward;
                    break;
                }
            case SpawnConfiguration.MovementDirection.Outward:
                {
                    direction = (t.localPosition - transform.position).normalized;
                    break;
                }
            case SpawnConfiguration.MovementDirection.Inward:
                {
                    direction = (transform.position - t.localPosition).normalized;
                    break;
                }
            case SpawnConfiguration.MovementDirection.Random:
                {
                    direction = Random.insideUnitCircle;
                    break;
                }
            default:
                {
                    direction = Vector3.zero;
                    break;
                }
        }

        shape.Velocity = direction * spawnConfig.spawnSpeed.RandomValueInRange;
    }
}
