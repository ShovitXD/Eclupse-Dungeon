using UnityEngine;

public class LightMoverLoop : MonoBehaviour
{
    public Transform point1;   // Obj 1
    public Transform point2;   // Obj 2
    public float speed = 2f;

    private Transform target;

    void Start()
    {
        if (point1 == null || point2 == null) return;

        // Start at point1 and move towards point2
        transform.position = point1.position;
        target = point2;
    }

    void Update()
    {
        if (point1 == null || point2 == null) return;

        // Move towards the current target (always point2 in this case)
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        // When we reach point2 -> instantly reset to point1
        if (Vector3.Distance(transform.position, point2.position) < 0.001f)
        {
            transform.position = point1.position;   // instant teleport back
        }
    }
}
