using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Bello.Unity;
[RequireComponent(typeof(CharacterController))]
public class CC_Character : StateManager<CC_Character>
{
    [Header("Settings")][Space(5)]
    [SerializeField] bool useGravity;
    [Tooltip("Don't let the character fall out of a cliff")]
    [SerializeField] bool avoidFalling;
    [field: SerializeField] public float speed { get; private set; } = 15;
    [field: SerializeField] public float rotationSpeed { get; private set; } = 500;
    [field: SerializeField] public LayerMask groundLayer { get; private set; }
    [field: SerializeField] public Animator animator { get; private set; }
    [field: ReadOnly] [field: SerializeField] public Vector3 MovementDirection { get; set; }
    [field: ReadOnly] [field: SerializeField] public Vector2 joystickAxis { get; private set; }

    [field: Header("Status")][field: Space(5)]
    [field: ReadOnly] [field: SerializeField] public CC_States currentState { get; set; } 
    [field: ReadOnly] [field: SerializeField] public CC_SubStates currentSubState { get; set; } 
    [field: ReadOnly] [field: SerializeField] public bool Grounded { get; private set; }

    private CharacterController characterController;

    private void Awake()
    {
        Context = this;
        joystickAxis = new Vector2();
        characterController = GetComponent<CharacterController>();
        CurrentState = new CC_Idle();
        SwichState(new CC_Idle());
        //AddNewSubstate(new CC_Walk(true, 1));
    }
    private void FixedUpdate()
    {
        HandleMovement(MovementDirection * Time.fixedDeltaTime);
    }
    private void HandleMovement(Vector3 movement)
    {
        if (avoidFalling) if (!CheckIfGrounded(transform.position + movement.normalized * .5f)) movement = Vector3.zero;

        if (useGravity) movement.y -= 9.8f * Time.fixedDeltaTime;
        characterController.Move(movement);

        if (!avoidFalling) Grounded = CheckIfGrounded(transform.position);
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
    public void SetMovementDirection(Vector2 direction, float speed)
    {
        Vector3 currentDirection = MovementDirection;
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
public enum CC_States
{
    Idle,
    Walking,
}
public enum CC_SubStates
{
    Walking,
}
