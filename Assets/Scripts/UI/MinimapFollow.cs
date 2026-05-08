using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    [SerializeField] float zOffset = -10f;
    Transform player;

    void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    void LateUpdate()
    {
        if (player == null) return;
        transform.position = new Vector3(player.position.x, player.position.y, zOffset);
    }
}
