using UnityEngine;

public class MenuTextureScroller : MonoBehaviour
{
    public float xSpeed = 0f;
    public float ySpeed = 0.1f;

    private Renderer rend;
    private Vector2 offset;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        offset = rend.material.mainTextureOffset;   // starts from current Offset in material
    }

    void Update()
    {
        // Add speed over time
        offset.x += xSpeed * Time.deltaTime;
        offset.y += ySpeed * Time.deltaTime;

        // Keep in 0–1 range (same as 0–360° wrap)
        offset.x = Mathf.Repeat(offset.x, 1f);
        offset.y = Mathf.Repeat(offset.y, 1f);

        rend.material.mainTextureOffset = offset;   // this drives the X/Y Offset field
    }
}
