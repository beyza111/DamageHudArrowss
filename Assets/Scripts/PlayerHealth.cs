using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float maxShield = 50f;

    private float currentHealth;
    private float currentShield;

    public DamageRelay damageRelay;

    void Start()
    {
        currentHealth = maxHealth;
        currentShield = maxShield;
    }

    public void TakeDamage(float amount, Vector3 sourcePosition)
    {
        float remainingDamage = amount;

       
        if (currentShield > 0)
        {
            float shieldDamage = Mathf.Min(currentShield, remainingDamage);
            currentShield -= shieldDamage;
            remainingDamage -= shieldDamage;

            Debug.Log($"Shield took {shieldDamage} damage. Shield left: {currentShield}");

          
            if (damageRelay != null)
                damageRelay.Raise(shieldDamage, sourcePosition, true);
        }

       
        if (remainingDamage > 0)
        {
            currentHealth = Mathf.Max(0, currentHealth - remainingDamage);
            Debug.Log($"Health took {remainingDamage} damage. Health left: {currentHealth}");

            
            if (damageRelay != null)
                damageRelay.Raise(remainingDamage, sourcePosition, false);

            if (currentHealth <= 0)
                Debug.Log("Player is Dead");
        }
    }
}
