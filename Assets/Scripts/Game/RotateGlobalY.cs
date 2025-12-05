using UnityEngine;

public class RotateGlobalY : MonoBehaviour
{
    public float rotationSpeed = 90f; // degrees per second

    void Update()
    {
        // Rotate around global Y axis
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }
}
