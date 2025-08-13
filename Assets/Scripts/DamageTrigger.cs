using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    public float damageAmount = 10f;

    private void OnTriggerEnter(Collider other)
    {
        var health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damageAmount, transform.position);
        }
    }
}
