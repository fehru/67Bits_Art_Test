using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed;
    private Animator anim;
    private Rigidbody rigg;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        rigg = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        var joystick = JoystickAxis();
        var direction = new Vector3(joystick.x, 0, joystick.y);
        rigg.velocity = direction  * speed * Time.deltaTime;
        anim.SetFloat("Movement", rigg.velocity.magnitude, .25f, Time.deltaTime);
        if(direction.magnitude != 0)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
    }

    private Vector2 JoystickAxis()
    {
        var x = Input.GetAxis("Horizontal");
        var y = Input.GetAxis("Vertical");
        return new Vector2(x, y);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out GroundButton Event))
            Event.StartCoroutine(Event.Fill(transform));
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out GroundButton Event))
            Event.Cancell();
    }
}
