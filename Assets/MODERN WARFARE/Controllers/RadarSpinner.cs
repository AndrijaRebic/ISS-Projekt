using UnityEngine;

namespace EnvironmentProps
{
    public class RadarSpinner : MonoBehaviour
    {
        [Header("Spin Settings")]
        public float spinSpeed = 20f; // Degrees per second

        private void Update()
        {
            // Rotate slowly around Y axis
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);
        }
    }
}
