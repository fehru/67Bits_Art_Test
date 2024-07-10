using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Bello.Unity;
[RequireComponent(typeof(Rigidbody))]
public class CPC_Character : StateManager<CPC_Character>
{
    [Header("Settings")]
    [Space(5)]
    [Tooltip("Don't let the character fall out of a cliff")]
    [SerializeField] bool avoidFalling;
    [field: SerializeField] public float speed { get; private set; } = 15;
    [field: SerializeField] public float rotationSpeed { get; private set; } = 500;
    [field: SerializeField] public LayerMask groundLayer { get; private set; }
    [field: SerializeField] public Animator animator { get; private set; }
    [field: ReadOnly] [field: SerializeField] public Vector3 MovementDirection { get; set; }
    private Vector3 charVelocity;
    [field: ReadOnly] [field: SerializeField] public Vector3 velocity { get; set; }
    [field: ReadOnly] [field: SerializeField] public Vector2 joystickAxis { get; private set; }

    [field: Header("Status")]
    [field: Space(5)]
    [field: ReadOnly] [field: SerializeField] public CPC_States currentState { get; set; }
    [field: ReadOnly] [field: SerializeField] public CPC_SubStates currentSubState { get; set; }
    [field: ReadOnly] [field: SerializeField] public bool Grounded { get; private set; }

    public Rigidbody charRigidbody { get; private set; }

    private void Awake()
    {
        Context = this;
        joystickAxis = new Vector2();
        charRigidbody = GetComponent<Rigidbody>();
        CurrentState = new CPC_Idle();
        SwichState(new CPC_Idle());
        //AddNewSubstate(new CC_Walk(true, 1));
    }
    private void FixedUpdate()
    {
        HandleMovement(MovementDirection * Time.fixedDeltaTime);
    }
    private void HandleMovement(Vector3 movement)
    {
        if (avoidFalling) if (!CheckIfGrounded(transform.position + movement.normalized * .5f)) movement = Vector3.zero;

        charVelocity = movement;
        charVelocity.y = charRigidbody.velocity.y;
        charRigidbody.velocity = charVelocity;

        Grounded = CheckIfGrounded(transform.position);
    }
    public void HandleRotation(Vector3 fowardDirection, Vector3 upwardDirection)
    {
        transform.RotateToDirection(fowardDirection, upwardDirection, rotationSpeed);
    }
    private bool CheckIfGrounded(Vector3 checkPosition)
    {
        Ray groundCheck = new Ray(checkPosition, -Vector3.up);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(groundCheck, out hit, 3, groundLayer)) return true;
        else return false;
    }
    public void SetMovementVelocity(Vector2 direction, float speed)
    {
        Vector3 currentDirection = Vector3.zero;
        currentDirection.x = direction.x;
        currentDirection.y = 0;
        currentDirection.z = direction.y;
        MovementDirection = currentDirection * speed;
    }

    public void JoystickAxis(InputAction.CallbackContext joystickAxis)
    {
        this.joystickAxis = joystickAxis.ReadValue<Vector2>();
    }
}
public enum CPC_States
{
    Idle,
    Walking,
}
public enum CPC_SubStates
{
    Walking,
}
