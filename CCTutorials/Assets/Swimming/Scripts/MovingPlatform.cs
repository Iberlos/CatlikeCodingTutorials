using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PlatformMovementStruct
{
    [SerializeField]
    private Vector3 translation;
    [SerializeField]
    private Vector3 eulerRotation;
    [SerializeField]
    public float moveDuration;
    [SerializeField]
    public float idleDurationOverride;
    [HideInInspector]
    public Transform targetTransform;

    public void Initialize(Transform previous, float idleDuration)
    {
        targetTransform = previous;
        targetTransform.localPosition += translation;
        targetTransform.localRotation *= Quaternion.Euler(eulerRotation);

        if(idleDurationOverride == 0)
        {
            idleDurationOverride = idleDuration;
        }
    }
}  

public class MovingPlatform : MonoBehaviour
{
    private enum Mode { DeadEnd = 0, Loop, Reflect }
    private enum State { Idle = 0, Moving, Continuous}

    [SerializeField, Min(0f)]
    private float idleDuration = 1f;
    [SerializeField]
    private List<PlatformMovementStruct> moves = new List<PlatformMovementStruct>();
    [SerializeField]
    private Mode mode = Mode.DeadEnd;
    [SerializeField]
    private State initialState = State.Idle;

    private Rigidbody body;
    private float moveTimer;
    private float idleTimer;
    private int currentMove;
    private int increment;
    private State state;
    delegate void UpdateDelegate();
    private List<UpdateDelegate> Update = new List<UpdateDelegate>();

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        moveTimer = 0;
        idleTimer = 0;
        currentMove = 0;
        increment = 1;
        state = initialState;

        Update.Add(UpdateIdle);
        Update.Add(UpdateMoving);
        Update.Add(UpdateContinuous);

        Transform t = transform;
        foreach(PlatformMovementStruct move in moves)
        {
            move.Initialize(t, idleDuration);
            t = move.targetTransform;
        }
    }

    private void FixedUpdate()
    {
        Update[(int)state]();
    }

    void UpdateContinuous()
    {
        Vector3 rotation = transform.localRotation.eulerAngles;
        rotation.y += 90f * Time.fixedDeltaTime;
        transform.localRotation = Quaternion.Euler(rotation);
    }

    void UpdateIdle()
    {
        if (idleTimer < moves[currentMove].idleDurationOverride)
        {
            idleTimer += Time.fixedDeltaTime;
        }
        else
        {
            idleTimer = 0;

            state = State.Moving;
        }
    }

    void UpdateMoving()
    {
        if(moveTimer < moves[currentMove].moveDuration)
        {
            moveTimer += Time.fixedDeltaTime;
            body.position = Vector3.Lerp(body.position, moves[currentMove].targetTransform.position, moveTimer/moves[currentMove].moveDuration);
            body.rotation = Quaternion.Lerp(body.rotation, moves[currentMove].targetTransform.rotation, moveTimer/moves[currentMove].moveDuration);
        }
        else
        {
            moveTimer = 0;
            currentMove += increment;
            switch (mode)
            {
                case Mode.DeadEnd:
                    {
                        if (currentMove >= moves.Count)
                            currentMove = -1;
                        break;
                    }
                case Mode.Loop:
                    {
                        if (currentMove >= moves.Count)
                            currentMove = 0;
                        break;
                    }
                case Mode.Reflect:
                    {
                        if (currentMove <= -1 || currentMove >= moves.Count)
                        {
                            increment *= -1;
                            currentMove += increment;
                        }
                        break;
                    }
            }

            state = State.Idle;
        }
    }
}
