using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bello.Unity;

public class RotateTowardsObject : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 200;
    [SerializeField] private Direction fowardDirection = Direction.Foward;
    [SerializeField] private Direction UpwardDirection = Direction.Upward;
    [Space(10)]
    [SerializeField] private Transform target;
    public enum Direction
    {
        Foward,
        Upward,
        ReverseFoward,
        ReverseUpward,
        Up,
        Down,
        Left,
        Right,
        Target,
        ReverseTarget,
    }

    private void Update()
    {
        transform.RotateToDirection(GetDirectionVector(fowardDirection),
                                    GetDirectionVector(UpwardDirection), rotationSpeed);
    }

    public Vector3 GetDirectionVector(Direction direction)
    {
        return direction switch
        {
            Direction.Foward => transform.forward,
            Direction.ReverseFoward => -transform.forward,
            Direction.Upward => -transform.up,
            Direction.ReverseUpward => -transform.up,
            Direction.Up => Vector3.up,
            Direction.Down => Vector3.down,
            Direction.Left => Vector3.left,
            Direction.Right => Vector3.right,
            Direction.Target => (target.position - transform.position).normalized,
            Direction.ReverseTarget => -(target.position - transform.position).normalized,
            _ => Vector3.zero
        };
    }
}
