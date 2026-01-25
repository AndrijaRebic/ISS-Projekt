using UnityEngine;
using UnityEngine.AI;

public class TankRandomSpawn : MonoBehaviour
{
    public Vector2 positivePosition;
    public Vector2 negativePosition;

    public float heightOfCheck = 150f;
    public float rangeOfCheck = 1000f;
    public LayerMask terrainLayer;

    private void Awake()
    {
        SpawnTank();
    }

    void SpawnTank()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        TankController controller = GetComponent<TankController>();

        if (controller != null) controller.enabled = false;

        if (rb != null) {
            rb.isKinematic = true;
        }

        Vector3 randomPosition = new Vector3(
            Random.Range(negativePosition.x, positivePosition.x),
            heightOfCheck,
            Random.Range(negativePosition.y, positivePosition.y)
        );

        RaycastHit hit;
        if (Physics.Raycast(randomPosition, Vector3.down, out hit, rangeOfCheck, terrainLayer))
        {
            rb.position = hit.point;
            rb.rotation = Quaternion.identity;
            Debug.Log("Tank spawned at: " + hit.point);
        }
        Debug.LogWarning("No terrain found below the spawn point. Tank not spawned.");
        if (rb != null) {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.WakeUp();
        }
        if (controller != null) controller.enabled = true;
    }
}
