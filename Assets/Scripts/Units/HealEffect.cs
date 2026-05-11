using UnityEngine;

[RequireComponent(typeof(Animator))]
public class HealEffect : MonoBehaviour
{
    void Start()
    {
        var anim = GetComponent<Animator>();
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            float len = anim.GetCurrentAnimatorStateInfo(0).length;
            if (len > 0f) { Destroy(gameObject, len); return; }
        }
        Destroy(gameObject, 1f);
    }
}
