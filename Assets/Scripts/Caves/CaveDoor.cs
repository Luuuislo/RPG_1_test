using UnityEngine;

public class CaveDoor : MonoBehaviour
{
    public Transform transformToTransport;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (transformToTransport == null)
        {
            Debug.LogWarning("CaveDoor needs a transformToTransport assigned in the Inspector.", this);
            return;
        }

        if (other.gameObject.CompareTag("Player"))
        {
            other.transform.position = transformToTransport.position;
        }
    }
}
