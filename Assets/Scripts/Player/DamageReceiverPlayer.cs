using UnityEngine;

public class DamageReceiverPlayer : MonoBehaviour
{
    private Player player;

    void Start()
    {
        player = GetComponentInParent<Player>();
    }

    public void ApplyDamage(int amount, Vector2 hitDirection)
    {
        player?.TakeDamage(amount);
    }
}
