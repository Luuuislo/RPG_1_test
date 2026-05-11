using UnityEngine;

// Flecha/proyectil disparado por AllyArcher.
// Se destruye al impactar con un enemigo o al expirar.
[RequireComponent(typeof(Rigidbody2D))]
public class AllyProjectile : MonoBehaviour
{
    public int       damage      = 10;
    public LayerMask enemyLayers;
    public float     lifetime    = 3f;

    void Start() => Destroy(gameObject, lifetime);

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayers) == 0) return;
        var dr = other.GetComponentInParent<DamageReceiver>();
        if (dr == null) return;
        dr.ApplyDamage(damage, true, false, transform.right);
        Destroy(gameObject);
    }
}
