using UnityEngine;

public class WaveSpawnPoint : MonoBehaviour
{
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.35f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, 0.6f);
        Gizmos.color = new Color(1f, 0.35f, 0f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, 0.6f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        float s = 0.5f;
        Gizmos.DrawLine(transform.position - Vector3.right * s, transform.position + Vector3.right * s);
        Gizmos.DrawLine(transform.position - Vector3.up    * s, transform.position + Vector3.up    * s);
    }
}
