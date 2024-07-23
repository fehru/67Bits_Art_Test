using UnityEngine;

public class MoveTexture : MonoBehaviour
{
    public float speed = 0.07f; // Velocidade de movimento da textura
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        float offset = Time.time * speed;
        rend.material.mainTextureOffset = new Vector2(rend.material.mainTextureOffset.x, offset);
    }
}
