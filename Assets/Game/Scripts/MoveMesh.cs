using UnityEngine;

public class MoveMesh : MonoBehaviour
{
    public float speed = 1.0f; // Velocidade do movimento
    public float amplitude = 1.0f; // Amplitude do movimento

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float newPositionY = startPosition.y + Mathf.Sin(Time.time * speed) * amplitude;
        transform.position = new Vector3(transform.position.x, newPositionY, transform.position.z);
    }
}
