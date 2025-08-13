
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DamageUnityEvent : UnityEvent<float, Vector3, bool> { } // (amount, hitPos, isShield)

public class DamageRelay : MonoBehaviour
{
    public DamageUnityEvent OnDamaged;

 
    public void Raise(float amount, Vector3 worldHitPos, bool isShield)
    {
        OnDamaged?.Invoke(amount, worldHitPos, isShield);
    }
}
