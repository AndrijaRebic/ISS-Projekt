using UnityEngine;

public class BradleySmokeLauncher : MonoBehaviour
{
    [Header("Smoke")]
    public GameObject smokePrefab;
    public Transform[] launchPoints;

    [Header("Gameplay")]
    public KeyCode smokeKey = KeyCode.X;
    public int maxCharges = 2;
    public float cooldown = 15f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip smokeFireClip;

    int chargesUsed = 0;
    float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (Input.GetKeyDown(smokeKey))
            TryDeploySmoke();
    }

    void TryDeploySmoke()
    {
        if (smokePrefab == null || launchPoints.Length == 0)
            return;

        if (chargesUsed >= maxCharges)
            return;

        if (timer < cooldown)
            return;

        timer = 0f;
        chargesUsed++;

        foreach (var point in launchPoints)
        {
            Instantiate(
                smokePrefab,
                point.position,
                point.rotation
            );

            if (audioSource != null && smokeFireClip != null)
                audioSource.PlayOneShot(smokeFireClip, 1f);
        }
    }
}
