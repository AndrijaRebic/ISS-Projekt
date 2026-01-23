using UnityEngine;

public class MissileExplosion : MonoBehaviour
{
    public GameObject explosionPrefab;
    public float explosionLife = 2f;

    private bool exploded = false;

    /* void OnCollisionEnter(Collision collision)
    {
        if (exploded) return;
        exploded = true;

        Explode();
    } */

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("HIT: " + collision.gameObject.name);
        Explode();
    }


    void Explode()
    {
        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(
                explosionPrefab,
                transform.position,
                Quaternion.identity
            );

            Destroy(fx, explosionLife);
        }

        Destroy(gameObject);
    }
}
