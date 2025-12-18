using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 1.5f, -6f);
    public float followSpeed = 10f;
    public float lookSpeed = 10f;

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desiredPos = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);

        Quaternion desiredRot = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, lookSpeed * Time.deltaTime);
    }
}
